/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/IImageComparer.cs
 * PURPOSE:     Interface for Similar Image and duplicate Images search
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://www.codeproject.com/Articles/374386/Simple-image-comparison-in-NET
 */

// ReSharper disable UnusedMemberInSuper.Global

using System.Collections.Generic;

namespace ImageCompare
{
    /// <summary>
    ///     Image Compare Interface
    /// </summary>
    internal interface IImageComparer
    {
        /// <summary>
        ///     Find all similar images in a folder, and possibly subfolders
        /// </summary>
        /// <param name="folderPath">The folder to look for duplicates in</param>
        /// <param name="checkSubfolders">Whether to look in subfolders too</param>
        /// <param name="extensions">The extensions.</param>
        /// <param name="threshold">The Value of differences allowed.</param>
        /// <returns>
        ///     A list of all the duplicates found, collected in separate Lists (one for each distinct image found)
        /// </returns>
        List<List<string>> GetSimilarImages(string folderPath, bool checkSubfolders,
            IEnumerable<string> extensions, float threshold);

        /// <summary>
        ///     Find all duplicate images in a folder, and possibly subfolders
        /// </summary>
        /// <param name="folderPath">The folder to look for duplicates in</param>
        /// <param name="checkSubfolders">Whether to look in subfolders too</param>
        /// <param name="extensions">The extensions.</param>
        /// <returns>
        ///     A list of all the duplicates found, collected in separate Lists (one for each distinct image found)
        /// </returns>
        List<List<string>> GetDuplicateImages(string folderPath, bool checkSubfolders,
            IEnumerable<string> extensions);
    }
}