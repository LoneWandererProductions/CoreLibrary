/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/ImageComparer.cs
 * PURPOSE:     ImageCompare Implementation
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://www.codeproject.com/Articles/374386/Simple-image-comparison-in-NET
 */

using System.Collections.Generic;

namespace ImageCompare;

/// <inheritdoc />
/// <summary>
///     Compare Images
/// </summary>
/// <seealso cref="IImageComparer" />
public sealed class ImageComparer : IImageComparer
{
    /// <inheritdoc />
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
    public List<List<string>>? GetSimilarImages(string folderPath, bool checkSubfolders,
        IEnumerable<string> extensions, float threshold)
    {
        return ImageSimilarity.GetSimilarImages(folderPath, checkSubfolders, extensions, threshold);
    }

    /// <inheritdoc />
    /// <summary>
    ///     Find all duplicate images in a folder, and possibly subfolders
    /// </summary>
    /// <param name="folderPath">The folder to look for duplicates in</param>
    /// <param name="checkSubfolders">Whether to look in subfolders too</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>
    ///     A list of all the duplicates found, collected in separate Lists (one for each distinct image found)
    /// </returns>
    public List<List<string>>? GetDuplicateImages(string folderPath, bool checkSubfolders,
        IEnumerable<string> extensions)
    {
        return ImageDuplication.GetDuplicateImages(folderPath, checkSubfolders, extensions);
    }
}
