/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        LoopCacheAnalyzer.cs
 * PURPOSE:     Check if we can perhaps cache some stuff in a loop,
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using Core.Apps.Enums;
using Core.Apps.Helper;
using Core.Apps.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Core.Apps.Rules
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    /// <summary>
    /// Analyzer that uses Semantic DataFlowAnalysis to safely detect loop-invariant
    /// method calls that can be hoisted or cached, avoiding side-effect false positives.
    /// </summary>
    public sealed class LoopCacheAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICommand" />
        public string Name => "LoopCache";

        /// <inheritdoc cref="ICommand" />
        public string Description => "Detects pure, invariant method calls inside loops via DataFlow analysis.";

        /// <inheritdoc />
        public string Namespace => "Analyzer";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string? filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath))
                yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);

            // Build compilation to enable Data Flow Analysis
            var compilation = CSharpCompilation.Create("AnalysisAssembly")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(tree);

            var semanticModel = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var loopNode = invocation.Ancestors().FirstOrDefault(IsLoopSyntax);
                if (loopNode == null)
                    continue;

                // 1. Resolve the method symbol
                if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
                    continue;

                // 2. Filter out guaranteed side-effect methods
                if (methodSymbol.ReturnsVoid)
                    continue; // Methods returning void exist solely for side effects (e.g. List.Add)

                if (methodSymbol.Parameters.Any(p => p.RefKind != RefKind.None))
                    continue; // Method mutates via ref/out parameters

                // Ignore basic framework utilities where caching overhead > computation cost
                var fqn = methodSymbol.ContainingType.ToDisplayString();
                if (fqn.StartsWith("System.") || fqn.StartsWith("Math"))
                    continue;

                // 3. Perform Data Flow Analysis
                var loopFlow = semanticModel.AnalyzeDataFlow(loopNode);
                var invocationFlow = semanticModel.AnalyzeDataFlow(invocation);

                if (invocationFlow == null || !loopFlow.Succeeded || !invocationFlow.Succeeded)
                    continue;

                // Variables mutated anywhere inside the loop (includes 'i' in for-loops, 'item' in foreach, etc.)
                var mutatedInLoop = loopFlow.WrittenInside;

                // Variables read specifically by this method call (target object + arguments)
                var readByInvocation = invocationFlow.ReadInside;

                // 4. The Intersection Test
                // If the method reads ANY variable that is written to inside the loop, it is NOT invariant.
                bool dependsOnLoopState =
                    readByInvocation.Any(v => mutatedInLoop.Contains(v, SymbolEqualityComparer.Default));

                if (!dependsOnLoopState)
                {
                    var line = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                    yield return new Diagnostic(
                        Name,
                        Enums.DiagnosticSeverity.Info,
                        filePath,
                        line,
                        $"Method '{methodSymbol.Name}' inside loop does not depend on any state mutated during the loop. Consider hoisting it or caching the result.",
                        DiagnosticImpact.CpuBound
                    );
                }
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string?[] args)
        {
            List<Diagnostic> results;
            try
            {
                results = AnalyzerExecutor.ExecutePath(this, args, "Usage: LoopCache <fileOrDirectoryPath>");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }

            var sb = new StringBuilder();
            sb.AppendLine("⚡ Semantic Loop Cache / Hoisting Diagnostics:");
            sb.AppendLine(new string('-', 50));

            foreach (var d in results)
                sb.AppendLine(d.ToString());

            return CommandResult.Ok(sb.ToString(), EnumTypes.Wstring);
        }

        /// <summary>
        /// Determines whether [is loop syntax] [the specified node].
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        ///   <c>true</c> if [is loop syntax] [the specified node]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsLoopSyntax(SyntaxNode node) =>
            node is ForStatementSyntax ||
            node is ForEachStatementSyntax ||
            node is WhileStatementSyntax ||
            node is DoStatementSyntax;
    }
}