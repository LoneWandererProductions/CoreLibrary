/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/ImageAnalysis.cs
 * PURPOSE:     Some basic Tools to compare Images
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;

namespace ImageCompare
{
    /// <inheritdoc />
    /// <summary>
    ///     Some Basic Analysis Tools for Images
    /// </summary>
    /// <seealso cref="IImageAnalysis" />
    public sealed class ImageAnalysis : IImageAnalysis
    {
        /// <inheritdoc />
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
        /// <exception cref="ArgumentException">Argument Exception</exception>
        /// <exception cref="InvalidOperationException">Invalid Operation</exception>
        [return: MaybeNull]
        public List<string> FindImagesInColorRange(int r, int g, int b, int range, string folderPath,
            bool checkSubfolders, IEnumerable<string> extensions)
        {
            return ImageColoring.GetSimilarColors(r, g, b, range, folderPath, checkSubfolders, extensions);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the image details.
        /// </summary>
        /// <param name="imagePath">The image path.</param>
        /// <returns>Image Details as Image Data Object</returns>
        /// <exception cref="ArgumentException">Argument Exception</exception>
        /// <exception cref="InvalidOperationException">Invalid Operation</exception>
        [return: MaybeNull]
        public ImageData GetImageDetails(string imagePath)
        {
            return AnalysisProcessing.GetImageDetails(imagePath);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the image details.
        /// </summary>
        /// <param name="imagePaths">The image paths.</param>
        /// <returns>List of Image Details as Image Data Object</returns>
        /// <exception cref="ArgumentException">Argument Exception</exception>
        /// <exception cref="InvalidOperationException">Invalid Operation</exception>
        [return: MaybeNull]
        public List<ImageData> GetImageDetails(List<string> imagePaths)
        {
            var lst = new List<ImageData>(imagePaths.Count);

            lst.AddRange(imagePaths.Select(AnalysisProcessing.GetImageDetails).Where(cache => cache != null));

            //File was skipped? Return null
            if (lst.Count != imagePaths.Count)
            {
                return null;
            }

            var similarity = AnalysisProcessing.GetSimilarity(imagePaths);
            for (var i = 0; i < lst.Count; i++)
            {
                lst[i].Similarity = similarity[i];
            }

            return lst;
        }

        /// <summary>
        /// Compares the two images.
        /// </summary>
        /// <param name="one">The Bitmap one.</param>
        /// <param name="two">The Bitmap two.</param>
        /// <returns>Data about two images</returns>
        public ImageCompareData CompareImages(Bitmap one, Bitmap two)
        {
            var oneSimilar = ImageProcessing.GenerateData(one, 0);
            var twoSimilar = ImageProcessing.GenerateData(two, 1);

            return new ImageCompareData
            {
                Similarity = ImageProcessing.GetPercentageDifference(oneSimilar, twoSimilar),
                ImageOne = AnalysisProcessing.GetImageDetails(one).GetDetailsSimple(),
                ImageTwo = AnalysisProcessing.GetImageDetails(two).GetDetailsSimple()
            };
        }

        /// <summary>
        /// Compares the two images.
        /// </summary>
        /// <param name="pathOne">The path to image one.</param>
        /// <param name="pathTwo">The path to image two.</param>
        /// <returns>Data about two images</returns>
        public ImageCompareData CompareImages(string pathOne, string pathTwo)
        {
            var one = new Bitmap(pathOne);
            var two = new Bitmap(pathTwo);

            var oneSimilar = ImageProcessing.GenerateData(one, 0);
            var twoSimilar = ImageProcessing.GenerateData(two, 1);

            return new ImageCompareData
            {
                Similarity = ImageProcessing.GetPercentageDifference(oneSimilar, twoSimilar),
                ImageOne = AnalysisProcessing.GetImageDetails(pathOne).GetDetails(),
                ImageTwo = AnalysisProcessing.GetImageDetails(pathTwo).GetDetails()
            };
        }
    }
}
