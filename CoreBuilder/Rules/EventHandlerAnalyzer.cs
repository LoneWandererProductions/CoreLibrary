/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Rules
 * FILE:        EventHandlerAnalyzer.cs
 * PURPOSE:     Analyzer that detects potential event handler leaks.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using CoreBuilder.Enums;
using CoreBuilder.Helper;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

namespace CoreBuilder.Rules;

/// <inheritdoc cref="ICodeAnalyzer" />
/// <summary>
/// Check if Event is unsubscribed.
/// </summary>
/// <seealso cref="ICodeAnalyzer" />
public sealed class EventHandlerAnalyzer : ICodeAnalyzer, ICommand
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    public string Name => "EventHandler";

    /// <inheritdoc cref="ICodeAnalyzer" />
    public string Description => "Analyzer that detects potential event handler leaks.";

    /// <inheritdoc />
    public string Namespace => "Analyzer";

    /// <inheritdoc />
    public int ParameterCount => 1;

    /// <inheritdoc />
    public CommandSignature Signature => new(Namespace, Name, ParameterCount);

    /// <summary>
    /// The event stats
    /// </summary>
    private readonly Dictionary<string, (int Count, HashSet<string> Files)> _eventStats = new();

    /// <inheritdoc />
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        // 🔹 Ignore generated code and compiler artifacts
        if (CoreHelper.ShouldIgnoreFile(filePath))
            yield break;

        var tree = CSharpSyntaxTree.ParseText(fileContent);
        var root = tree.GetRoot();

        foreach (var sub in root.DescendantNodes()
                     .OfType<AssignmentExpressionSyntax>()
                     .Where(a => a.IsKind(SyntaxKind.AddAssignmentExpression)))
        {
            if (sub.Left is not IdentifierNameSyntax eventName)
                continue;

            var key = eventName.Identifier.Text;
            if (!_eventStats.TryGetValue(key, out var stats))
                stats = (0, new HashSet<string>());

            stats.Count++;
            stats.Files.Add(filePath);
            _eventStats[key] = stats;

            var line = sub.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

            yield return new Diagnostic(
                Name,
                Enums.DiagnosticSeverity.Warning,
                filePath,
                line,
                $"Event '{key}' subscribed {stats.Count} times so far. Check for corresponding unsubscribes.",
                DiagnosticImpact.IoBound
            );
        }
    }

    /// <inheritdoc />
    public CommandResult Execute(params string[] args)
    {
        List<Diagnostic> results;
        try
        {
            results = AnalyzerExecutor.ExecutePath(this, args, "Usage: EventHandler <fileOrDirectoryPath>");
        }
        catch (Exception ex)
        {
            return CommandResult.Fail(ex.Message);
        }

        var output = string.Join(Environment.NewLine, results.Select(d => d.ToString()));
        return CommandResult.Ok(output, results);
    }

    /// <inheritdoc />
    public CommandResult InvokeExtension(string extensionName, params string[] args)
        => CommandResult.Fail($"'{Name}' has no extensions.");
}
