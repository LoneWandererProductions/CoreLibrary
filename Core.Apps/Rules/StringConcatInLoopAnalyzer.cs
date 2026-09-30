/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps.Rules
 * FILE:        StringConcatInLoopAnalyzer.cs
 * PURPOSE:     Detects string concatenation (+, +=, interpolation) inside loops.
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
    /// Analyzer that detects string concatenation inside loops.
    /// </summary>
    public sealed class StringConcatInLoopAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "StringConcatInLoop";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Detects string concatenation inside loops which creates heavy GC allocations.";

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
            var root = tree.GetRoot();

            // 1. Check for += or + assignments inside loops
            foreach (var assignment in root.DescendantNodes().OfType<AssignmentExpressionSyntax>())
            {
                if (!IsInsideLoop(assignment))
                    continue;

                if (assignment.IsKind(SyntaxKind.AddAssignmentExpression) ||
                    assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
                {
                    // Check if the assignment involves a string or string addition
                    if (assignment.Right is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.AddExpression))
                    {
                        var line = assignment.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        yield return new Diagnostic(
                            Name,
                            DiagnosticSeverity.Warning,
                            filePath,
                            line,
                            "String concatenation inside loop detected. Consider using StringBuilder or ValueStringBuilder.",
                            DiagnosticImpact.MemoryBound
                        );
                    }
                    else if (assignment.IsKind(SyntaxKind.AddAssignmentExpression))
                    {
                        var line = assignment.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                        yield return new Diagnostic(
                            Name,
                            DiagnosticSeverity.Warning,
                            filePath,
                            line,
                            "Operator '+=' used inside loop for concatenation. Consider using StringBuilder.",
                            DiagnosticImpact.MemoryBound
                        );
                    }
                }
            }

            // 2. Check for interpolated strings inside loops
            foreach (var interpolated in root.DescendantNodes().OfType<InterpolatedStringExpressionSyntax>())
            {
                if (!IsInsideLoop(interpolated))
                    continue;

                // Ignore if it's immediately passed to a logging statement
                var line = interpolated.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                yield return new Diagnostic(
                    Name,
                    DiagnosticSeverity.Info,
                    filePath,
                    line,
                    "Interpolated string allocated inside loop. Ensure this does not create unnecessary allocations.",
                    DiagnosticImpact.MemoryBound
                );
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
                var results =
                    AnalyzerExecutor.ExecutePath(this, args, "Usage: StringConcatInLoop <fileOrDirectoryPath>");
                return CommandResult.Ok($"String concatenations in loops found: {results.Count}\n" +
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
