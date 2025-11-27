/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder.Interface
 * FILE:        ICodeAnalyzer.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMemberInSuper.Global

using System.Collections.Generic;
using System.Linq;

namespace CoreBuilder.Interface;

/// <summary>
///     Analyzer Interface, that will be shared around.
/// </summary>
public interface ICodeAnalyzer
{
    /// <summary>
    ///     Gets the name.
    /// </summary>
    /// <value>
    ///     The name.
    /// </value>
    string Name { get; }

    /// <summary>
    /// Gets the description.
    /// </summary>
    /// <value>
    /// The description.
    /// </value>
    string Description { get; }

    /// <summary>
    ///     Analyzes the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="fileContent">Content of the file.</param>
    /// <returns>Code Analyzer results.</returns>
    IEnumerable<Diagnostic> Analyze(string filePath, string fileContent) => Enumerable.Empty<Diagnostic>();

    /// <summary>
    /// Analyze all files together.
    /// Override this only if your analyzer requires project-wide context.
    /// </summary>
    IEnumerable<Diagnostic> AnalyzeProject(Dictionary<string, string> allFiles) => Enumerable.Empty<Diagnostic>();
}