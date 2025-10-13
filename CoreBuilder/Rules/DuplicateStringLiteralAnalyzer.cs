/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        Rules/DuplicateStringLiteralAnalyzer.cs
 * PURPOSE:     Analyzer that finds duplicate string literals across a project.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreBuilder.Interface;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CoreBuilder.Rules;

/// <inheritdoc />
/// <summary>
///     Analyzer that identifies duplicate string literals across a project.
///     Useful for detecting hardcoded strings that should be centralized
///     in constants, resources, or configuration.
/// </summary>
/// <seealso cref="ICodeAnalyzer" />
public sealed class DuplicateStringLiteralAnalyzer : ICodeAnalyzer
{
    /// <inheritdoc />
    public string Name => "DuplicateStringLiteral";

    /// <inheritdoc />
    public string Description => "Analyzer that finds duplicate string literals across a project.";

    /// <inheritdoc />
    /// <remarks>
    ///     This method intentionally yields no results, since duplicate
    ///     string detection requires project-wide context. Use
    ///     <see cref="AnalyzeDirectory"/> instead.
    /// </remarks>
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        // 🔹 Ignore generated code and compiler artifacts
        if (CoreHelper.ShouldIgnoreFile(filePath))
        {
            yield break;
        }

        // Per-file analysis not supported here
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
        var literalOccurrences = new Dictionary<string, List<(string file, int line)>>();

        var files = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(content);
            var root = tree.GetRoot();
            var literals = root.DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .Where(lit => lit.IsKind(SyntaxKind.StringLiteralExpression));

            foreach (var lit in literals)
            {
                var text = lit.Token.ValueText.Trim();
                if (string.IsNullOrEmpty(text)) continue;

                var line = lit.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                if (!literalOccurrences.ContainsKey(text))
                    literalOccurrences[text] = new List<(string, int)>();

                literalOccurrences[text].Add((file, line));
            }
        }

        foreach (var kvp in literalOccurrences.Where(k => k.Value.Count > 1))
        {
            foreach (var (file, line) in kvp.Value)
            {
                yield return new Diagnostic(Name, DiagnosticSeverity.Info, file, line,
                    $"String literal \"{kvp.Key}\" occurs {kvp.Value.Count} times across the project. Consider centralizing it.");
            }
        }
    }
}
