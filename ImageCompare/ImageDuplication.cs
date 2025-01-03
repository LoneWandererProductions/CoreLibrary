/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/ImageDuplication.cs
 * PURPOSE:     Find Duplicate Images
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://www.codeproject.com/Articles/374386/Simple-image-comparison-in-NET
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

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
using Imaging;

namespace ImageCompare
{
    /// <summary>
    ///     Duplicate Images
    /// </summary>
    internal static class ImageDuplication
    {
        /// <summary>
        ///     The render
        /// </summary>
        private static readonly ImageRender Render = new();

        /// <summary>
        ///     The Temp path dictionary
        /// </summary>
        private static Dictionary<int, string> Translator { get; set; }

        /// <summary>
        ///     Find all duplicate images in a folder, and possibly subfolders
        /// </summary>
        /// <param name="folderPath">The folder to look for duplicates in</param>
        /// <param name="checkSubfolders">Whether to look in subfolders too</param>
        /// <param name="extensions">The extensions.</param>
        /// <returns>
        ///     A list of all the duplicates found, collected in separate Lists (one for each distinct image found)
        /// </returns>
        [return: MaybeNull]
        internal static List<List<string>> GetDuplicateImages(string folderPath, bool checkSubfolders,
            IEnumerable<string> extensions)
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

            images.Sort();

            var duplicateGroups = GetDuplicateGroups(images);

            var result = Translate(duplicateGroups);
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
        private static List<ImageDuplicate> GetSortedGrayScaleValues()
        {
            var imagePathsAndGrayValues = new List<ImageDuplicate>(Translator.Count);

            //with sanity check in Case one file went missing, we won't have to stop everything
            foreach (var (key, value) in Translator.Where(pathImage => File.Exists(pathImage.Value)))
            {
                try
                {
                    using var btm = new Bitmap(value);
                    var dup = GenerateData(btm, key);
                    imagePathsAndGrayValues.Add(dup);
                }
                catch (ArgumentException ex)
                {
                    Trace.WriteLine(ex);
                    throw new ArgumentException(ex.Message, value);
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

            return imagePathsAndGrayValues;
        }

        /// <summary>
        ///     Gets the duplicate groups.
        ///     Only finds pairs
        /// </summary>
        /// <param name="imagePathsAndGrayValues">The image paths and gray values.</param>
        /// <returns>Group of Duplicates</returns>
        private static IEnumerable<List<ImageDuplicate>> GetDuplicateGroups(
            IEnumerable<ImageDuplicate> imagePathsAndGrayValues)
        {
            var duplicateGroups = new List<List<ImageDuplicate>>();
            var currentDuplicates = new List<ImageDuplicate>();

            foreach (var image in imagePathsAndGrayValues)
            {
                if (currentDuplicates.Count > 0 && !currentDuplicates[0].Equals(image))
                {
                    if (currentDuplicates.Count > 1)
                    {
                        duplicateGroups.Add(currentDuplicates);
                        currentDuplicates = new List<ImageDuplicate>();
                    }
                    else
                    {
                        currentDuplicates.Clear();
                    }
                }

                currentDuplicates.Add(image);
            }

            if (currentDuplicates.Count > 1)
            {
                duplicateGroups.Add(currentDuplicates);
            }

            return duplicateGroups;
        }

        /// <summary>
        ///     Generates the data. No need to resize! Only Change to  Greyscale.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>Image Object to compare</returns>
        private static ImageDuplicate GenerateData(Bitmap bitmap, int id)
        {
            //resize
            bitmap = Render.BitmapScaling(bitmap, ImageResources.DuplicateSize, ImageResources.DuplicateSize);

            //use our new Format
            var dbm = DirectBitmap.GetInstance(bitmap);

            //get the average Color Value
            var r = 0;
            var b = 0;
            var g = 0;

            for (var y = 0; y < ImageResources.DuplicateSize; y++)
            for (var x = 0; x < ImageResources.DuplicateSize; x++)
            {
                r += dbm.GetPixel(x, y).R;
                b += dbm.GetPixel(x, y).B;
                g += dbm.GetPixel(x, y).G;
            }

            r /= ImageResources.DuplicateSize * ImageResources.DuplicateSize;
            b /= ImageResources.DuplicateSize * ImageResources.DuplicateSize;
            g /= ImageResources.DuplicateSize * ImageResources.DuplicateSize;

            var image = new byte[ImageResources.DuplicateSize, ImageResources.DuplicateSize];
            var hash = new byte[ImageResources.DuplicateSize * ImageResources.DuplicateSize];

            //get greyscale
            bitmap = Render.FilterImage(bitmap, FiltersType.GrayScale);

            //Get array Map for comparison
            dbm = DirectBitmap.GetInstance(bitmap);

            try
            {
                var i = -1;
                for (var y = 0; y < ImageResources.DuplicateSize; y++)
                for (var x = 0; x < ImageResources.DuplicateSize; x++)
                {
                    i++;
                    var cache = dbm.GetPixel(x, y).R;
                    image[x, y] = cache;
                    hash[i] = cache;
                }
            }
            catch (InvalidOperationException ex)
            {
                Trace.WriteLine(ex);
            }

            return new ImageDuplicate
            {
                R = r,
                G = g,
                B = b,
                Id = id,
                Image = image
            };
        }

        /// <summary>
        ///     Translates the specified duplicate groups.
        /// </summary>
        /// <param name="duplicateGroups">The duplicate groups.</param>
        /// <returns>List of Similar Images</returns>
        private static List<List<string>> Translate(IEnumerable<List<ImageDuplicate>> duplicateGroups)
        {
            return duplicateGroups.Select(group =>
                    (from element in @group where Translator[element.Id] != null select Translator[element.Id])
                    .ToList())
                .ToList();
        }
    }
}
