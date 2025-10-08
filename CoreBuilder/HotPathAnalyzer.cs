/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        HotPathAnalyzer.cs
 * PURPOSE:     Analyzer that detects frequently called methods and flags hot paths.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreBuilder;

/// <summary>
/// Analyzer that detects method calls in hot paths (loops) and aggregates statistics.
/// </summary>
public sealed class HotPathAnalyzer : ICodeAnalyzer
{
    /// <inheritdoc />
    public string Name => "HotPath";

    /// <summary>
    /// Project-wide aggregation: method FQN -> (call count, total risk, files seen)
    /// </summary>
    private readonly Dictionary<string, (int Count, int TotalRisk, HashSet<string> Files)> _aggregateStats =
        new();

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
        {
            yield break;
        }

        var tree = CSharpSyntaxTree.ParseText(fileContent);
        var root = tree.GetRoot();

        var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();

        foreach (var invocation in invocations)
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

            int risk = loopContext switch
            {
                LoopContext.ConstantBounded => ConstantLoopWeight,
                LoopContext.VariableBounded => VariableLoopWeight,
                LoopContext.Nested => NestedLoopWeight,
                _ => 1
            };

            // Update aggregation
            if (!_aggregateStats.TryGetValue(symbolName, out var stats))
            {
                stats = (0, 0, new HashSet<string>());
            }

            stats.Count++;
            stats.TotalRisk += risk;
            stats.Files.Add(filePath);
            _aggregateStats[symbolName] = stats;

            var line = invocation.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            DiagnosticImpact impact = loopContext switch
            {
                LoopContext.ConstantBounded => DiagnosticImpact.CpuBound,
                LoopContext.VariableBounded => DiagnosticImpact.CpuBound,
                LoopContext.Nested => DiagnosticImpact.CpuBound,
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

    /// <summary>
    /// Returns a project-wide summary of hot paths.
    /// </summary>
    /// <returns>Collected messages.</returns>
    public IEnumerable<string> GetSummary()
    {
        foreach (var kv in _aggregateStats.OrderByDescending(k => k.Value.TotalRisk))
        {
            yield return $"{kv.Key} -> {kv.Value.Count} calls, total risk {kv.Value.TotalRisk} (in {kv.Value.Files.Count} files)";
        }
    }

    /// <summary>
    /// Builds the message.
    /// </summary>
    /// <param name="method">The method.</param>
    /// <param name="ctx">The CTX.</param>
    /// <returns>Message string.</returns>
    private static string BuildMessage(string method, LoopContext ctx)
    {
        return ctx switch
        {
            LoopContext.ConstantBounded => $"Method '{method}' inside constant-bounded loop.",
            LoopContext.VariableBounded => $"Method '{method}' inside variable-bounded loop.",
            LoopContext.Nested => $"Method '{method}' inside nested loops.",
            _ => $"Method '{method}' inside loop."
        };
    }
}
