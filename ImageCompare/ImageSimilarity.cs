/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/ImageSimilarity.cs
 * PURPOSE:     Find Similar Images
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://www.codeproject.com/Articles/374386/Simple-image-comparison-in-NET
 */

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using ExtendedSystemObjects;
using FileHandler;

namespace ImageCompare;

internal static class ImageSimilarity
{
    /// <summary>
    ///     The Temp path dictionary
    /// </summary>
    private static Dictionary<int, string> Translator { get; set; }

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
    internal static List<List<string>>? GetSimilarImages(string folderPath, bool checkSubfolders,
        IEnumerable<string> extensions, float threshold)
    {
        var localDate = DateTime.Now;
        Trace.WriteLine(localDate.ToString(CultureInfo.InvariantCulture));

        //create Directories
        var imagePaths = FileHandleSearch.GetFilesByExtensionFullPath(folderPath, extensions, checkSubfolders);

        if (imagePaths.IsNullOrEmpty())
        {
            return null;
        }

        Translator = imagePaths.ToDictionary();

        var images = GetSortedGrayScaleValues();

        if (images.IsNullOrEmpty())
        {
            return null;
        }

        //Just get all Images that are in the same Color Space
        var duplicateGroups = GetDuplicateGroups(images);
        localDate = DateTime.Now;
        Trace.WriteLine(localDate.ToString(CultureInfo.InvariantCulture));
        Trace.WriteLine(nameof(duplicateGroups));
        Trace.WriteLine(nameof(duplicateGroups.Count));
        //Let's compare all result sets, oif empty well tough luck

        if (duplicateGroups.IsNullOrEmpty())
        {
            return null;
        }

        Trace.WriteLine(duplicateGroups.Count);

        var groups = new List<List<ImageSimilar>>();

        foreach (var duplicates in duplicateGroups)
        {
            var dup = new List<ImageSimilar>(duplicates);

            foreach (var cache in duplicates.Select(item => ImageProcessing.FindSimilarImages(item, dup, threshold))
                         .Where(cache => cache != null))
            {
                dup = dup.Except(cache).ToList();
                groups.Add(cache);
            }
        }

        localDate = DateTime.Now;
        Trace.WriteLine(localDate.ToString(CultureInfo.InvariantCulture));

        if (groups.IsNullOrEmpty())
        {
            return null;
        }

        var result = Translate(groups);

        localDate = DateTime.Now;
        Trace.WriteLine(localDate.ToString(CultureInfo.InvariantCulture));

        return result;
    }

    /// <summary>
    ///     Gets the sorted gray scale values.
    /// </summary>
    /// <returns>
    ///     List of possible similar images
    /// </returns>
    /// <exception cref="OutOfMemoryException">Out of Memory</exception>
    /// <exception cref="ArgumentException">Wrong Argument</exception>
    /// <exception cref="InvalidOperationException">Invalid Operation</exception>
    private static List<ImageSimilar> GetSortedGrayScaleValues()
    {
        var imagePathsAndGrayValues = new List<ImageSimilar>(Translator.Count);

        //with sanity check in Case one file went missing, we won't have to stop everything
        foreach (var (key, value) in Translator.Where(pathImage => File.Exists(pathImage.Value)))
        {
            try
            {
                using var btm = new Bitmap(value);
                var dup = ImageProcessing.GenerateData(btm, key);
                imagePathsAndGrayValues.Add(dup);
            }
            catch (ArgumentException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (OutOfMemoryException ex)
            {
                var memory = Process.GetCurrentProcess().VirtualMemorySize64.ToString();
                Trace.WriteLine(ex, memory);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                Trace.WriteLine(ex);
                throw new InvalidOperationException(ex.Message);
            }
        }

        Trace.WriteLine(nameof(GetSortedGrayScaleValues));
        Trace.WriteLine(imagePathsAndGrayValues.Count);
        return imagePathsAndGrayValues;
    }

    /// <summary>
    ///     Gets the duplicate groups.
    /// </summary>
    /// <param name="imagePathsAndGrayValues">The image paths and gray values.</param>
    /// <returns>Group of Duplicates</returns>
    private static List<List<ImageSimilar>> GetDuplicateGroups(
        IReadOnlyCollection<ImageSimilar> imagePathsAndGrayValues)
    {
        var duplicateGroups = new List<List<ImageSimilar>>();
        var currentDuplicates = new List<ImageSimilar>();
        var dup = new List<ImageSimilar>(imagePathsAndGrayValues);

        foreach (var duplicate in imagePathsAndGrayValues)
        {
            currentDuplicates.AddRange(dup.Where(image => duplicate.Equals(image)));

            dup = dup.Except(currentDuplicates).ToList();

            if (currentDuplicates.Count is 1 or 0)
            {
                currentDuplicates.Clear();
            }
            else
            {
                duplicateGroups.Add(currentDuplicates);
                currentDuplicates = new List<ImageSimilar>();
            }
        }

        Trace.WriteLine(nameof(GetDuplicateGroups));
        Trace.WriteLine(duplicateGroups.Count);
        return duplicateGroups;
    }

    /// <summary>
    ///     Translates the specified duplicate groups.
    /// </summary>
    /// <param name="duplicateGroups">The duplicate groups.</param>
    /// <returns>List of Similar Images</returns>
    private static List<List<string>> Translate(IEnumerable<List<ImageSimilar>> duplicateGroups)
    {
        return duplicateGroups.Select(group =>
                (from element in @group where Translator[element.Id] != null select Translator[element.Id])
                .ToList())
            .ToList();
    }
}
