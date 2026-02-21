/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Rules
 * FILE:        HotPathAnalyzer.cs
 * PURPOSE:     Analyzer that detects frequently called methods and flags hot paths.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using CoreBuilder.Enums;
using CoreBuilder.Helper;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.Rules
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    /// <summary>
    /// Analyzer that detects method calls in hot paths (loops) and aggregates statistics.
    /// </summary>
    public sealed class HotPathAnalyzer : ICodeAnalyzer, ICommand
    {
        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Name => "HotPath";

        /// <inheritdoc cref="ICodeAnalyzer" />
        public string Description => "Analyzer that detects frequently called methods and flags hot paths.";

        /// <inheritdoc />
        public string Namespace => "Analyzer";

        /// <inheritdoc />
        public int ParameterCount => 1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <summary>
        /// Project-wide aggregation: method FQN -> (call count, total risk, files seen)
        /// </summary>
        private readonly Dictionary<string, (int Count, int TotalRisk, HashSet<string> Files)> _aggregateStats = new();

        // Thresholds / weights

        /// <summary>
        /// The constant loop weight
        /// </summary>
        private const int ConstantLoopWeight = 10;

        /// <summary>
        /// The variable loop weight
        /// </summary>
        private const int VariableLoopWeight = 20;

        /// <summary>
        /// The nested loop weight
        /// </summary>
        private const int NestedLoopWeight = 50;

        /// <inheritdoc />
        public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
        {
            // 🔹 Ignore generated code and compiler artifacts
            if (CoreHelper.ShouldIgnoreFile(filePath))
                yield break;

            var tree = CSharpSyntaxTree.ParseText(fileContent);
            var root = tree.GetRoot();

            foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var symbolName = invocation.Expression.ToString();

                // Ignore framework / system calls
                if (symbolName.StartsWith("System.") ||
                    symbolName.StartsWith("Microsoft.") ||
                    symbolName.StartsWith("Path.") ||
                    symbolName.StartsWith("File.") ||
                    symbolName.StartsWith("Directory."))
                    continue;

                var loopContext = CoreHelper.GetLoopContext(invocation);
                if (loopContext == LoopContext.None)
                    continue;

                var risk = loopContext switch
                {
                    LoopContext.ConstantBounded => ConstantLoopWeight,
                    LoopContext.VariableBounded => VariableLoopWeight,
                    LoopContext.Nested => NestedLoopWeight,
                    _ => 1
                };

                if (!_aggregateStats.TryGetValue(symbolName, out var stats))
                    stats = (0, 0, new HashSet<string>());

                stats.Count++;
                stats.TotalRisk += risk;
                stats.Files.Add(filePath);
                _aggregateStats[symbolName] = stats;

                var line = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                var impact = loopContext switch
                {
                    LoopContext.ConstantBounded or LoopContext.VariableBounded or LoopContext.Nested => DiagnosticImpact
                        .CpuBound,
                    _ => DiagnosticImpact.Other
                };

                yield return new Diagnostic(
                    Name,
                    DiagnosticSeverity.Info,
                    filePath,
                    line,
                    $"{BuildMessage(symbolName, loopContext)} Called {stats.Count} times so far, current risk {risk}, total risk {stats.TotalRisk}.",
                    impact
                );
            }
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            List<Diagnostic> results;
            try
            {
                results = AnalyzerExecutor.ExecutePath(this, args, "Usage: HotPath <fileOrDirectoryPath>");
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(ex.Message);
            }

            var sb = new StringBuilder();
            sb.AppendLine("🔥 Hot Path Summary:");
            sb.AppendLine(new string('-', 50));

            // Add diagnostics first
            foreach (var d in results)
                sb.AppendLine(d.ToString());

            sb.AppendLine("\nAggregated Methods:");
            sb.AppendLine("-------------------");

            foreach (var kvp in _aggregateStats.OrderByDescending(k => k.Value.TotalRisk))
            {
                var (method, data) = (kvp.Key, kvp.Value);
                sb.AppendLine(
                    $"{method}: {data.Count} calls, total risk {data.TotalRisk}, files [{string.Join(", ", data.Files)}]");
            }

            return CommandResult.Ok(
                message: sb.ToString(),
                type: EnumTypes.Wstring
            );
        }

        /// <summary>
        /// Builds the message.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="ctx">The CTX.</param>
        /// <returns>Concated message.</returns>
        private static string BuildMessage(string method, LoopContext ctx) =>
            ctx switch
            {
                LoopContext.ConstantBounded => $"Method '{method}' inside constant-bounded loop.",
                LoopContext.VariableBounded => $"Method '{method}' inside variable-bounded loop.",
                LoopContext.Nested => $"Method '{method}' inside nested loops.",
                _ => $"Method '{method}' inside loop."
            };
    }
}
