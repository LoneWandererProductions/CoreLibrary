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
    /// Analyzer that detects method calls inside loops that don't depend on loop variables, suggesting hoisting or caching.
    /// </summary>
    public sealed class LoopCacheAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICommand" />
        public string Name => "LoopCache";

        /// <inheritdoc cref="ICommand" />
        public string Description => "Detects invariant method calls inside loops that could be cached or hoisted.";

        /// <inheritdoc />
        public string Namespace => "Analyzer";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
        {
            if (CoreHelper.ShouldIgnoreFile(filePath))
                yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = tree.GetRoot();

            foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                // Ignore basic static utilities that are trivial
                var symbolName = invocation.Expression.ToString();
                if (symbolName.StartsWith("System.", StringComparison.Ordinal) || symbolName.StartsWith("Math.", StringComparison.Ordinal))
                    continue;

                // Find enclosing loop statement
                var enclosingLoop = invocation.Ancestors().FirstOrDefault(IsLoopSyntax);
                if (enclosingLoop == null)
                    continue;

                // Get loop control variable identifiers (e.g., 'i' in 'for', 'item' in 'foreach')
                var loopVariables = GetLoopVariables(enclosingLoop);
                if (loopVariables.Count == 0)
                    continue;

                // Check if invocation syntax references any of the loop's iteration variables
                var invocationIdentifiers = invocation.DescendantNodes()
                    .OfType<IdentifierNameSyntax>()
                    .Select(id => id.Identifier.ValueText)
                    .ToHashSet();

                // If none of the loop control variables are used anywhere in this method call
                bool isInvariant = !loopVariables.Any(v => invocationIdentifiers.Contains(v));

                if (isInvariant)
                {
                    var line = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                    yield return new Diagnostic(
                        Name,
                        Enums.DiagnosticSeverity.Info,
                        filePath,
                        line,
                        $"Method '{symbolName}' inside loop does not depend on loop variables ({string.Join(", ", loopVariables)}). Consider hoisting it outside the loop or caching the result.",
                        DiagnosticImpact.CpuBound
                    );
                }
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
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
            sb.AppendLine("⚡ Loop Cache / Hoisting Diagnostics:");
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
            node is ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax;

        /// <summary>
        /// Gets the loop variables.
        /// </summary>
        /// <param name="loop">The loop.</param>
        /// <returns>Variables in the loop.</returns>
        private static HashSet<string> GetLoopVariables(SyntaxNode loop)
        {
            var vars = new HashSet<string>();

            switch (loop)
            {
                case ForEachStatementSyntax foreachLoop:
                    vars.Add(foreachLoop.Identifier.ValueText);
                    break;

                case ForStatementSyntax forLoop:
                    if (forLoop.Declaration != null)
                    {
                        foreach (var v in forLoop.Declaration.Variables)
                            vars.Add(v.Identifier.ValueText);
                    }

                    break;
            }

            return vars;
        }
    }
}
