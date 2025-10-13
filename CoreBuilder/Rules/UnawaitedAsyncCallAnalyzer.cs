/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        Rules/UnawaitedAsyncCallAnalyzer.cs
 * PURPOSE:     Detects unawaited async calls in C# code.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreBuilder.Rules
{
    /// <summary>
    /// Detects method calls returning Task / ValueTask (or generic variants) that are not awaited
    /// and not assigned to a variable (simple heuristic for likely unawaited async calls).
    /// </summary>
    public sealed class UnawaitedAsyncCallAnalyzer : ICodeAnalyzer
    {
        /// <inheritdoc />
        public string Name => nameof(UnawaitedAsyncCallAnalyzer);

        /// <inheritdoc />
        public string Description => "Detects calls to async methods that are not awaited or assigned.";

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string filePath, string source)
        {
            var diagnostics = new List<Diagnostic>();

            // 🔹 Ignore generated code and compiler artifacts
            if (CoreHelper.ShouldIgnoreFile(filePath))
                return diagnostics;

            if (string.IsNullOrWhiteSpace(source))
                return diagnostics;

            SyntaxTree tree;
            try
            {
                tree = CSharpSyntaxTree.ParseText(source);
            }
            catch
            {
                return diagnostics;
            }

            Compilation? compilation = null;
            try
            {
                compilation = CSharpCompilation.Create("Analysis")
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                    .AddSyntaxTrees(tree);
            }
            catch
            {
                // fallback: syntax-only heuristic
                foreach (var exprOnly in tree.GetRoot().DescendantNodes().OfType<ExpressionStatementSyntax>())
                {
                    if (exprOnly.Expression is InvocationExpressionSyntax invocation &&
                        !(invocation.Parent is AwaitExpressionSyntax) &&
                        !(invocation.Parent is AssignmentExpressionSyntax))
                    {
                        var line = exprOnly.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        diagnostics.Add(new Diagnostic(
                            Name,
                            DiagnosticSeverity.Warning,
                            filePath,
                            line,
                            $"Possible unawaited async call (syntax heuristic): '{invocation}'"
                        ));
                    }
                }

                return diagnostics;
            }

            SemanticModel? model;
            try
            {
                model = compilation.GetSemanticModel(tree);
            }
            catch
            {
                return diagnostics;
            }

            var root = tree.GetRoot();

            foreach (var expr in root.DescendantNodes().OfType<ExpressionStatementSyntax>())
            {
                // If the statement is already awaited, skip
                if (expr.Expression is AwaitExpressionSyntax)
                    continue;

                // If assigned to variable, skip
                if (expr.Expression is AssignmentExpressionSyntax)
                    continue;

                if (expr.Expression is InvocationExpressionSyntax invocation)
                {
                    try
                    {
                        var symbolInfo = model.GetSymbolInfo(invocation);
                        var methodSymbol = symbolInfo.Symbol as IMethodSymbol
                                           ?? symbolInfo.CandidateSymbols.FirstOrDefault() as IMethodSymbol;

                        var returnType = methodSymbol?.ReturnType;
                        if (returnType == null)
                            continue;

                        var rtName = returnType.Name;
                        var isTaskLike = string.Equals(rtName, "Task", StringComparison.Ordinal)
                                         || string.Equals(rtName, "ValueTask", StringComparison.Ordinal);

                        if (!isTaskLike && returnType is INamedTypeSymbol { IsGenericType: true } namedType)
                        {
                            var genericName = namedType.ConstructedFrom?.Name;
                            isTaskLike = string.Equals(genericName, "Task", StringComparison.Ordinal)
                                         || string.Equals(genericName, "ValueTask", StringComparison.Ordinal);
                        }

                        if (isTaskLike)
                        {
                            var lineNumber = expr.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                            diagnostics.Add(new Diagnostic(
                                Name,
                                DiagnosticSeverity.Warning,
                                filePath,
                                lineNumber,
                                $"Async call '{invocation}' is not awaited or assigned; this may be unintended.",
                                DiagnosticImpact.Concurrency
                            ));
                        }
                    }
                    catch
                    {
                        var line = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        diagnostics.Add(new Diagnostic(
                            Name,
                            DiagnosticSeverity.Info,
                            filePath,
                            line,
                            $"Possible unawaited async call: '{invocation}' (could not resolve symbol)"
                        ));
                    }
                }
            }

            return diagnostics;
        }
    }
}
