/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/ImageColors.cs
 * PURPOSE:     Find Similar Color Range
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://www.codeproject.com/Articles/374386/Simple-image-comparison-in-NET
 */

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using ExtendedSystemObjects;
using FileHandler;

namespace ImageCompare
{
    /// <summary>
    ///     Get all Images with a similar Color Range
    /// </summary>
    internal static class ImageColoring
    {
        /// <summary>
        ///     Search all Images that find the images in this color Range
        /// </summary>
        /// <param name="r">Red Value</param>
        /// <param name="g">Green Value</param>
        /// <param name="b">Blue Value</param>
        /// <param name="range">Range of colors we allow</param>
        /// <param name="folderPath">The folder to look for duplicates in</param>
        /// <param name="checkSubfolders">Whether to look in subfolders too</param>
        /// <param name="extensions">The extensions.</param>
        /// <returns>List of Images with similar Color range</returns>
        [return: MaybeNull]
        internal static List<string> GetSimilarColors(int r, int g, int b, int range, string folderPath,
            bool checkSubfolders, IEnumerable<string> extensions)
        {
            var localDate = DateTime.Now;
            Trace.WriteLine(localDate.ToString(CultureInfo.InvariantCulture));

            //create Directories
            var imagePaths = FileHandleSearch.GetFilesByExtensionFullPath(folderPath, extensions, checkSubfolders);

            if (imagePaths.IsNullOrEmpty()) return null;

            var images = GetSortedColorValues(imagePaths);

            if (images.IsNullOrEmpty()) return null;

            //Generate Image we compare to
            var dup = new ImageColor { R = r, G = g, B = b, Threshold = range };

            //Just get all Images that are in the same Color Space

            return (from image in images where dup.Equals(image) select image.Path).ToList();
        }

        /// <summary>
        ///     Gets the sorted gray scale values.
        /// </summary>
        /// <param name="imagePaths"></param>
        /// <returns>
        ///     List of possible similar images
        /// </returns>
        /// <exception cref="OutOfMemoryException">Out of Memory</exception>
        /// <exception cref="ArgumentException">Wrong Argument</exception>
        /// <exception cref="InvalidOperationException">Invalid Operation</exception>
        private static List<ImageColor> GetSortedColorValues(IReadOnlyCollection<string> imagePaths)
        {
            var imagePathsAndGrayValues = new List<ImageColor>(imagePaths.Count);

            //with sanity check in Case one file went missing, we won't have to stop everything
            foreach (var path in imagePaths.Where(File.Exists))
                try
                {
                    using var btm = new Bitmap(path);
                    var dup = AnalysisProcessing.GenerateData(btm, path);
                    imagePathsAndGrayValues.Add(dup);
                }
                catch (ArgumentException ex)
                {
                    Trace.WriteLine(ex);
                    throw new ArgumentException(ex.ToString(), path);
                }
                catch (InvalidOperationException ex)
                {
                    Trace.WriteLine(ex);
                    throw new InvalidOperationException(ex.ToString());
                }

            return imagePathsAndGrayValues;
        }
    }
}