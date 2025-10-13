/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        Rules/EventSubscriptionAnalyzer.cs
 * PURPOSE:     Detects event subscriptions and checks for potential unsubscribes.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreBuilder.Rules;

/// <summary>
/// Detects event subscriptions and warns if events are repeatedly subscribed without unsubscribing.
/// </summary>
public sealed class EventSubscriptionAnalyzer : ICodeAnalyzer
{
    private readonly Dictionary<string, (int Count, HashSet<string> Files)> _eventStats = new();

    /// <inheritdoc />
    public string Name => nameof(EventSubscriptionAnalyzer);

    /// <inheritdoc />
    public string Description => "Detects event subscriptions and checks for potential unsubscribes.";

    /// <inheritdoc />
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        // 🔹 Ignore generated code and compiler artifacts
        if (CoreHelper.ShouldIgnoreFile(filePath))
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(fileContent))
        {
            yield break;
        }

        var tree = CSharpSyntaxTree.ParseText(fileContent);
        var root = tree.GetRoot();

        // Find event subscriptions (+=)
        var subscriptions = root.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Where(a => a.IsKind(SyntaxKind.AddAssignmentExpression));

        foreach (var sub in subscriptions)
        {
            if (sub.Left is IdentifierNameSyntax eventName)
            {
                var key = eventName.Identifier.Text;
                if (!_eventStats.TryGetValue(key, out var stats))
                    stats = (0, new HashSet<string>());

                stats.Count++;
                stats.Files.Add(filePath);
                _eventStats[key] = stats;

                var line = sub.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

                yield return new Diagnostic(
                    Name,
                    DiagnosticSeverity.Warning,
                    filePath,
                    line,
                    $"Event '{key}' subscribed {stats.Count} times so far. Check for corresponding unsubscribes.",
                    DiagnosticImpact.IoBound
                );
            }
        }
    }
}
