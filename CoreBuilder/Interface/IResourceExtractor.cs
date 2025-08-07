/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        CoreBuilder/IResourceExtractor.cs
 * PURPOSE:     String Resource Interface.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable MemberCanBeInternal

using System.Collections.Generic;

namespace CoreBuilder.Interface;

/// <summary>
///     Interface for my Resource Extractor
/// </summary>
public interface IResourceExtractor
{
    /// <summary>
    ///     Processes the given project directory, extracting string literals
    ///     and replacing them with resource references.
    /// </summary>
    /// <param name="projectPath">The root directory of the C# project.</param>
    /// <param name="outputResourceFile">Path to generate the resource file.</param>
    /// <param name="appendToExisting">Indicates whether to append to an existing file or create a new one.</param>
    /// <param name="replace">if set to <c>true</c> [replace].</param>
    /// <returns>
    ///     List of changed Files with directory.
    /// </returns>
    List<string> ProcessProject(string? projectPath, string? outputResourceFile = null,
        bool appendToExisting = false, bool replace = false);

    /// <summary>
    ///     Detects the affected files.
    /// </summary>
    /// <param name="projectPath">The project path.</param>
    /// <returns>Affected Files</returns>
    string? DetectAffectedFiles(string? projectPath);
}
