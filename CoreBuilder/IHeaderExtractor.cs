/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        CoreBuilder/IHeaderExtractor.cs
 * PURPOSE:     Interface for HeaderExtractor
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace CoreBuilder
{
    /// <summary>
    ///     Interface for HeaderExtractor
    /// </summary>
    public interface IHeaderExtractor
    {
        /// <summary>
        /// Processes the files.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="includeSubdirectories">if set to <c>true</c> [include subdirectories].</param>
        /// <returns>Messages about the converted files.</returns>
        string ProcessFiles(string directoryPath, bool includeSubdirectories);
    }
}
