/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        LicenseHeaderAnalyzer.cs
 * PURPOSE:     Just a simple License Header Analyzer. Checks if the file starts with a license header.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
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
        // Skip ignored files
        if (CoreHelper.ShouldIgnoreFile(filePath))
        {
            yield break;
        }

        // Check if the file starts with the license header
        if (!fileContent.StartsWith("// Licensed under", StringComparison.OrdinalIgnoreCase))
        {
            yield return new Diagnostic(Name, DiagnosticSeverity.Info, filePath, 1, "Missing license header.");
        }
    }
}
