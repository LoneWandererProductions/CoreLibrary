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
    public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent)
    {
        // Skip ignored files (obj/, .g.cs, generated, etc.)
        if (CoreHelper.ShouldIgnoreFile(filePath))
            yield break;

        // Trim leading whitespace/newlines
        var trimmed = fileContent.TrimStart();

        // Acceptable license header patterns
        var validHeaders = new[]
        {
        "// Licensed under",
        "/* COPYRIGHT",
        "/* LICENSE",
    };

        // Check if file starts with one of the known headers
        bool hasHeader = validHeaders.Any(h =>
            trimmed.StartsWith(h, StringComparison.OrdinalIgnoreCase));

        if (!hasHeader)
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
