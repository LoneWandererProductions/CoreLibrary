/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        CoreBuilder/IHeaderExtractor.cs
 * PURPOSE:     Interface for HeaderExtractor
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace CoreBuilder;

/// <summary>
///     Interface for HeaderExtractor
/// </summary>
public interface IHeaderExtractor
{
    /// <summary>
    ///     Processes the files.s
    /// </summary>
    /// <param name="directoryPath">The directory path.</param>
    /// <param name="includeSubdirectories">if set to <c>true</c> [include subdirectories].</param>
    /// <returns>Messages about the converted files.</returns>
    string ProcessFiles(string? directoryPath, bool includeSubdirectories);

    /// <summary>
    ///     Detects the files needing headers.
    /// </summary>
    /// <param name="directoryPath">The directory path.</param>
    /// <param name="includeSubdirectories">if set to <c>true</c> [include subdirectories].</param>
    /// <returns>List of files needing a header.</returns>
    string DetectFilesNeedingHeaders(string? directoryPath, bool includeSubdirectories);
}
