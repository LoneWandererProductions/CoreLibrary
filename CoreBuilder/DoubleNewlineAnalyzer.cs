/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        DoubleNewlineAnalyzer.cs
 * PURPOSE:     Simple Double Newline Analyzer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

namespace CoreBuilder;

/// <inheritdoc />
/// <summary>
///     Finds double line breaks.
/// </summary>
/// <seealso cref="T:CoreBuilder.ICodeAnalyzer" />
public sealed class DoubleNewlineAnalyzer : ICodeAnalyzer
{
    /// <inheritdoc />
    /// <summary>
    ///     Gets the name.
    /// </summary>
    /// <value>
    ///     The name.
    /// </value>
    public string Name => "DoubleNewline";

    /// <inheritdoc />
    /// <summary>
    ///     Analyzes the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="fileContent">Content of the file.</param>
    /// <returns>Diagnostic results.</returns>
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
                yield return new Diagnostic(filePath, i + 1, "Multiple blank lines in a row.");
            }
        }
    }
}
