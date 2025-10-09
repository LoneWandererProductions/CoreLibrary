/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        LicenseHeaderAnalyzer.cs
 * PURPOSE:     Just a simple License Header Analyzer. Checks if the file starts with a license header.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using CoreBuilder.Interface;

namespace CoreBuilder;

/// <inheritdoc />
/// <summary>
///     Find missing License Header.
/// </summary>
/// <seealso cref="T:CoreBuilder.ICodeAnalyzer" />
public sealed class LicenseHeaderAnalyzer : ICodeAnalyzer
{
    /// <inheritdoc />
    public string Name => "LicenseHeader";

    /// <inheritdoc />
    /// <inheritdoc />
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        if (CoreHelper.ShouldIgnoreFile(filePath))
            yield break;

        // Trim leading whitespace
        var trimmed = fileContent.TrimStart();

        // Quick exit if no comment at all
        if (!(trimmed.StartsWith("/*") || trimmed.StartsWith("//")))
        {
            yield return new Diagnostic(
                Name,
                DiagnosticSeverity.Info,
                filePath,
                1,
                "Missing license header.");

            yield break;
        }

        // Extract the first comment block or line(s)
        string firstChunk;
        if (trimmed.StartsWith("/*"))
        {
            // Find the end of block comment
            var endIdx = trimmed.IndexOf("*/", StringComparison.Ordinal);
            firstChunk = endIdx > 0 ? trimmed[..(endIdx + 2)] : trimmed;
        }
        else
        {
            // Line comments at start
            var lines = trimmed.Split('\n').TakeWhile(l => l.TrimStart().StartsWith("//"));
            firstChunk = string.Join("\n", lines);
        }

        // Normalize
        var header = firstChunk.ToUpperInvariant();

        // Acceptable keywords
        if (!(header.Contains("COPYRIGHT") || header.Contains("LICENSE")))
        {
            yield return new Diagnostic(
                Name,
                DiagnosticSeverity.Info,
                filePath,
                1,
                "Missing license header.");
        }
    }
}
