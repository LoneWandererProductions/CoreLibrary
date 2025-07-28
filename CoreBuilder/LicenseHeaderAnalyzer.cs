/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        LicenseHeaderAnalyzer.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;

namespace CoreBuilder;

/// <inheritdoc />
/// <summary>
///     Find missing License Header.
/// </summary>
/// <seealso cref="T:CoreBuilder.ICodeAnalyzer" />
public sealed class LicenseHeaderAnalyzer : ICodeAnalyzer
{
    /// <summary>
    ///     Gets the name.
    /// </summary>
    /// <value>
    ///     The name.
    /// </value>
    public string Name => "LicenseHeader";

    /// <summary>
    ///     Analyzes the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="fileContent">Content of the file.</param>
    /// <returns>
    ///     Code Analyzer results.
    /// </returns>
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
            yield return new Diagnostic(filePath, 1, "Missing license header.");
        }
    }
}
