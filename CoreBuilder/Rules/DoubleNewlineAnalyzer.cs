/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        Rules/DoubleNewlineAnalyzer.cs
 * PURPOSE:     Simple Double Newline Analyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using CoreBuilder.Interface;

namespace CoreBuilder.Rules;

/// <inheritdoc />
/// <summary>
///     Finds double line breaks.
/// </summary>
/// <seealso cref="T:CoreBuilder.ICodeAnalyzer" />
public sealed class DoubleNewlineAnalyzer : ICodeAnalyzer
{
    /// <inheritdoc />
    public string Name => nameof(DoubleNewlineAnalyzer);

    /// <inheritdoc />
    public string Description => "Simple Double Newline Analyzer.";

    /// <inheritdoc />
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        // Skip ignored files
        if (CoreHelper.ShouldIgnoreFile(filePath))
        {
            yield break;
        }

        var lines = fileContent.Split('\n');
        for (var i = 1; i < lines.Length - 1; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]) && string.IsNullOrWhiteSpace(lines[i - 1]))
            {
                yield return new Diagnostic(Name, DiagnosticSeverity.Info, filePath, i + 1,
                    "Multiple blank lines in a row.");
            }
        }
    }
}
