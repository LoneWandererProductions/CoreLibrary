/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        LinqInLoopAnalyzer.cs
 * PURPOSE:     Detects LINQ method calls inside loops that cause hidden heap allocations.
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
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;
using DiagnosticSeverity = Core.Apps.Enums.DiagnosticSeverity;

namespace Core.Apps.Rules
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    /// <summary>
    /// Analyzer that detects LINQ queries inside tight loops.
    /// </summary>
    public sealed class LinqInLoopAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "LinqInLoop";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description =>
            "Detects LINQ calls inside loops which allocate delegates and enumerators on every iteration.";

        /// <inheritdoc />
        public string Namespace => "Analyzer";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <summary>
        /// Common LINQ extension methods to flag when executed inside loops.
        /// </summary>
        private static readonly HashSet<string> LinqMethods = new(StringComparer.Ordinal)
        {
            "Where",
            "Select",
            "SelectMany",
            "Any",
            "All",
            "First",
            "FirstOrDefault",
            "Single",
            "SingleOrDefault",
            "Last",
            "LastOrDefault",
            "Count",
            "ToList",
            "ToArray",
            "ToDictionary",
            "GroupBy",
            "OrderBy",
            "OrderByDescending"
        };

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string? filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath))
                yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = tree.GetRoot();

            foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (!IsInsideLoop(invocation))
                    continue;

                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                    LinqMethods.Contains(memberAccess.Name.Identifier.Text))
                {
                    var line = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                    yield return new Diagnostic(
                        Name,
                        DiagnosticSeverity.Info,
                        filePath,
                        line,
                        $"LINQ call '.{memberAccess.Name.Identifier.Text}()' inside loop. Allocates enumerators/closures per iteration; consider hoisted or imperative loops.",
                        DiagnosticImpact.CpuBound
                    );
                }
            }
        }

        /// <summary>
        /// Determines whether [is inside loop] [the specified node].
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>
        ///   <c>true</c> if [is inside loop] [the specified node]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsInsideLoop(SyntaxNode node)
        {
            return node.Ancestors().Any(a =>
                a is ForStatementSyntax ||
                a is ForEachStatementSyntax ||
                a is WhileStatementSyntax ||
                a is DoStatementSyntax);
        }

        /// <inheritdoc />
        public CommandResult Execute(params string?[] args)
        {
            try
            {
                var results = AnalyzerExecutor.ExecutePath(this, args, "Usage: LinqInLoop <fileOrDirectoryPath>");
                return CommandResult.Ok($"LINQ calls in loops found: {results.Count}\n" +
                                        string.Join("\n",
                                            results.Select(d => $"{d.FilePath}({d.LineNumber}): {d.Message}")));
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }
        }
    }
}
