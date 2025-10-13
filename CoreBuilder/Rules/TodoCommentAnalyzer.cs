/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        Rules/TodoCommentAnalyzer.cs
 * PURPOSE:     Detects comments containing TODO, FIXME, or similar markers.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CoreBuilder.Interface;

namespace CoreBuilder.Rules;

/// <summary>
/// Detects comments containing TODO, FIXME, or similar markers.
/// </summary>
public sealed class TodoCommentAnalyzer : ICodeAnalyzer
{
    /// <summary>
    /// Pattern to detect TODO, FIXME, or similar markers.
    /// </summary>
    private static readonly Regex TodoPattern = new(
        @"\b(TODO|FIXME|HACK|UNDONE|BUG)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <inheritdoc />
    public string Name => nameof(TodoCommentAnalyzer);

    /// <inheritdoc />
    public string Description => "Detects comments containing TODO, FIXME, or similar markers.";

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

        using var reader = new StringReader(fileContent);
        string? line;
        var lineNumber = 0;

        while ((line = reader.ReadLine()) != null)
        {
            lineNumber++;

            // Only check comment lines
            if (!line.Contains("//"))
                continue;

            var commentIndex = line.IndexOf("//", StringComparison.Ordinal);
            var comment = line[commentIndex..];

            if (TodoPattern.IsMatch(comment))
            {
                yield return new Diagnostic(
                    Name,
                    DiagnosticSeverity.Info,
                    filePath,
                    lineNumber,
                    $"Found TODO/FIXME comment: {comment.Trim()}",
                    DiagnosticImpact.Maintainability
                );
            }
        }
    }
}
