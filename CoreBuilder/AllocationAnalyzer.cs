/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        AllocationAnalyzer.cs
 * PURPOSE:     Analyzer that detects allocations in hot paths.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreBuilder;

/// <summary>
/// Analyzer that detects allocations in hot paths.
/// </summary>
/// <seealso cref="CoreBuilder.Interface.ICodeAnalyzer" />
public sealed partial class AllocationAnalyzer : ICodeAnalyzer
{
    public string Name => "Allocation";

    /// <inheritdoc />
    private readonly Dictionary<string, (int Count, int TotalRisk, HashSet<string> Files)> _aggregateStats = new();

    private const int ConstantLoopWeight = 10;
    private const int VariableLoopWeight = 20;
    private const int NestedLoopWeight = 50;

    /// <inheritdoc />
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        var tree = CSharpSyntaxTree.ParseText(fileContent);
        var root = tree.GetRoot();

        foreach (var alloc in root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
        {
            // Ignore trivial .NET classes like string
            var typeName = alloc.Type.ToString();
            if (typeName == "string") continue;

            var loopContext = CoreHelper.GetLoopContext(alloc);
            if (loopContext == LoopContext.None) continue;

            int risk = loopContext switch
            {
                LoopContext.ConstantBounded => ConstantLoopWeight,
                LoopContext.VariableBounded => VariableLoopWeight,
                LoopContext.Nested => NestedLoopWeight,
                _ => 1
            };

            if (!_aggregateStats.TryGetValue(typeName, out var stats))
                stats = (0, 0, new HashSet<string>());

            stats.Count++;
            stats.TotalRisk += risk;
            stats.Files.Add(filePath);
            _aggregateStats[typeName] = stats;

            var line = alloc.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            yield return new Diagnostic(
                Name,
                DiagnosticSeverity.Info,
                filePath,
                line,
                $"Allocation of '{typeName}' inside loop. Called {stats.Count} times, current risk {risk}, total risk {stats.TotalRisk}.",
                DiagnosticImpact.CpuBound
            );
        }
    }

    /// <summary>
    /// Gets the summary.
    /// </summary>
    /// <returns>The analyzer Summary</returns>
    public IEnumerable<string> GetSummary()
    {
        foreach (var kv in _aggregateStats.OrderByDescending(k => k.Value.TotalRisk))
            yield return $"{kv.Key} -> {kv.Value.Count} allocations, total risk {kv.Value.TotalRisk} (in {kv.Value.Files.Count} files)";
    }
}
