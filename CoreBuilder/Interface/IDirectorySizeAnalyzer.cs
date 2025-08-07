/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        CoreBuilder/IDirectorySizeAnalyzer.cs
 * PURPOSE:     Interface for Directory Size Analyzer Tool
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace CoreBuilder.Interface;

/// <summary>
///     Interface for a tool that analyzes and summarizes file sizes in a directory.
/// </summary>
public interface IDirectorySizeAnalyzer
{
    /// <summary>
    ///     Lists file sizes and their percentage of total size in a directory.
    /// </summary>
    /// <param name="directoryPath">The directory to analyze.</param>
    /// <param name="includeSubdirectories">Whether to include subdirectories.</param>
    /// <returns>A formatted string showing size and percentage information.</returns>
    string DisplayDirectorySizeOverview(string? directoryPath, bool includeSubdirectories);
}
