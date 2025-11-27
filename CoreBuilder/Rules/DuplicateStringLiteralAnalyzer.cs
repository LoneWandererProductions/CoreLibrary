/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Rules
 * FILE:        DuplicateStringLiteralAnalyzer.cs
 * PURPOSE:     Analyzer that finds duplicate string literals across a project.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Helper;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Weaver;
using Weaver.Interfaces;
using Weaver.Messages;

// ReSharper disable UnusedType.Global

namespace CoreBuilder.Rules;

/// <inheritdoc cref="ICodeAnalyzer" />
/// <summary>
///     Analyzer that identifies duplicate string literals across a project.
///     Useful for detecting hardcoded strings that should be centralized
///     in constants, resources, or configuration.
/// </summary>
/// <seealso cref="ICodeAnalyzer" />
public sealed class DuplicateStringLiteralAnalyzer : ICodeAnalyzer, ICommand
{
    /// <inheritdoc cref="ICodeAnalyzer" />
    public string Name => "DuplicateStringLiteral";

    /// <inheritdoc cref="ICodeAnalyzer" />
    public string Description => "Analyzer that finds duplicate string literals across a project.";

    /// <inheritdoc />
    public string Namespace => "Analyzer";

    /// <inheritdoc />
    public int ParameterCount => 1;

    /// <inheritdoc />
    public CommandSignature Signature => new(Namespace, Name, ParameterCount);

    /// <summary>
    /// The cached literals, keyed by string literal and storing file+line lists.
    /// </summary>
    private static Dictionary<string, List<(string file, int line)>>? _cachedLiterals;

    /// <inheritdoc />
    /// <remarks>
    ///     This method intentionally yields no results per-file until project-wide analysis is performed.
    ///     Uses lazy-loading of project-wide string literals to avoid repeated scans.
    /// </remarks>
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        if (CoreHelper.ShouldIgnoreFile(filePath))
            yield break;

        // Lazy-load project-wide string literals once
        if (_cachedLiterals == null)
        {
            var rootDir = CoreHelper.FindProjectRoot(filePath);
            _cachedLiterals = BuildProjectLiterals(rootDir);
        }

        // Only return diagnostics relevant to this file
        foreach (var kvp in _cachedLiterals)
        {
            foreach (var (file, line) in kvp.Value)
            {
                if (file == filePath)
                    yield return new Diagnostic(Name, Enums.DiagnosticSeverity.Info, file, line,
                        kvp.Key); // kvp.Key = the message with literal
            }
        }
    }

    /// <summary>
    ///     Performs project-wide analysis for duplicate string literals.
    /// </summary>
    /// <param name="directory">The root directory to scan for C# files.</param>
    /// <returns>
    ///     A collection of <see cref="Diagnostic"/> instances for string literals
    ///     that occur multiple times across the project.
    /// </returns>
    public IEnumerable<Diagnostic> AnalyzeDirectory(string directory)
    {
        _cachedLiterals ??= BuildProjectLiterals(directory);

        foreach (var kvp in _cachedLiterals)
        {
            foreach (var (file, line) in kvp.Value)
                yield return new Diagnostic(Name, Enums.DiagnosticSeverity.Info, file, line,
                    $"String literal \"{kvp.Key}\" occurs {kvp.Value.Count} times across the project. Consider centralizing it.");
        }
    }

    /// <inheritdoc />
    public CommandResult Execute(params string[] args)
    {
        List<Diagnostic> diagnostics;

        try
        {
            diagnostics = AnalyzerExecutor.ExecutePath(
                this,
                args,
                "Usage: DuplicateStringLiteral <fileOrDirectoryPath>"
            );
        }
        catch (ArgumentException ae)
        {
            return CommandResult.Fail(ae.Message);
        }
        catch (FileNotFoundException fnfe)
        {
            return CommandResult.Fail(fnfe.Message);
        }
        catch (Exception ex)
        {
            return CommandResult.Fail($"DuplicateStringLiteral execution failed: {ex.Message}", ex);
        }

        if (diagnostics.Count == 0)
            return CommandResult.Ok($"No duplicate string literals found in '{args[0]}'.");

        var sb = new StringBuilder();
        sb.AppendLine($"Duplicate string literal analysis for: {args[0]}");
        sb.AppendLine(new string('-', 80));

        foreach (var d in diagnostics)
            sb.AppendLine(d.ToString());

        sb.AppendLine(new string('-', 80));
        sb.AppendLine($"{diagnostics.Count} duplicate string occurrences detected.");

        return CommandResult.Ok(sb.ToString(), diagnostics);
    }


    /// <inheritdoc />
    public CommandResult InvokeExtension(string extensionName, params string[] args)
    {
        return CommandResult.Fail($"'{Name}' has no extensions.");
    }

    /// <summary>
    /// Builds a dictionary of all duplicate string literals in the project.
    /// </summary>
    /// <param name="directory">The root directory to scan.</param>
    /// <returns>
    /// A dictionary keyed by literal string containing file+line lists.
    /// </returns>
    private static Dictionary<string, List<(string file, int line)>> BuildProjectLiterals(string directory)
    {
        var occurrences = new Dictionary<string, List<(string file, int line)>>();

        foreach (var file in Directory.GetFiles(directory, CoreResources.ResourceCsExtension,
                     SearchOption.AllDirectories))
        {
            if (CoreHelper.ShouldIgnoreFile(file))
                continue;

            foreach (var (literal, line) in ExtractLiteralsFromFile(file))
            {
                if (!occurrences.ContainsKey(literal))
                    occurrences[literal] = new List<(string, int)>();

                occurrences[literal].Add((file, line));
            }
        }

        // Keep only duplicates
        return occurrences
            .Where(k => k.Value.Count > 1)
            .ToDictionary(k => k.Key, k => k.Value);
    }

    /// <summary>
    /// Extracts all string literals from a single file.
    /// </summary>
    /// <param name="filePath">The C# file path.</param>
    /// <returns>Pairs of string literal and line number.</returns>
    private static IEnumerable<(string literal, int line)> ExtractLiteralsFromFile(string filePath)
    {
        var content = File.ReadAllText(filePath);
        var root = CSharpSyntaxTree.ParseText(content).GetRoot();

        var literals = root.DescendantNodes()
            .OfType<LiteralExpressionSyntax>()
            .Where(l => l.IsKind(SyntaxKind.StringLiteralExpression));

        foreach (var lit in literals)
        {
            var text = lit.Token.ValueText.Trim();
            if (string.IsNullOrEmpty(text)) continue;

            var line = lit.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            yield return (text, line);
        }
    }
}