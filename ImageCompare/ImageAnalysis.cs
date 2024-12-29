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
using System.IO;
using System.Linq;
using Mathematics;

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
            if (lst.Count != imagePaths.Count) return null;

            var similarity = AnalysisProcessing.GetSimilarity(imagePaths);
            for (var i = 0; i < lst.Count; i++) lst[i].Similarity = similarity[i];

            return lst;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Compares the two images.
        /// </summary>
        /// <param name="first">The Bitmap one.</param>
        /// <param name="second">The Bitmap two.</param>
        /// <returns>Data about two images</returns>
        /// <exception cref="ArgumentException">Argument Exception</exception>
        public ImageCompareData CompareImages(Bitmap first, Bitmap second)
        {
            if (first == null)
                throw new ArgumentException(string.Concat(ImageResources.ErrorImageEmpty, nameof(first)));

            if (second == null)
                throw new ArgumentException(string.Concat(ImageResources.ErrorImageEmpty, nameof(second)));

            return ImageHelper.CompareImages(first, second);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Compares the two images.
        /// </summary>
        /// <param name="first">The path to image one.</param>
        /// <param name="second">The path to image two.</param>
        /// <returns>Data about two images</returns>
        /// <exception cref="ArgumentException">Argument Exception</exception>
        public ImageCompareData CompareImages(string first, string second)
        {
            return ImageHelper.CompareImages(first, second);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the colors of an Image and collects them with the amount in a Dictionary.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Color Dictionary</returns>
        /// <exception cref="T:System.ArgumentException">Argument Exception</exception>
        public Dictionary<Color, int> GetColors(string path)
        {
            if (!File.Exists(path)) throw new ArgumentException(string.Concat(ImageResources.ErrorFileNotFound, path));

            var image = new Bitmap(path);

            return AnalysisProcessing.GetColors(image);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the colors of an Image and collects them with the amount in a Dictionary.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>Color Dictionary</returns>
        /// <exception cref="T:System.ArgumentException">Argument Exception</exception>
        public Dictionary<Color, int> GetColors(Bitmap image)
        {
            if (image == null)
                throw new ArgumentException(string.Concat(ImageResources.ErrorImageEmpty, nameof(image)));

            return AnalysisProcessing.GetColors(image);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Generates a differences bitmap.
        /// </summary>
        /// <param name="first">The first bitmap.</param>
        /// <param name="second">The second bitmap.</param>
        /// <param name="color">The color.</param>
        /// <returns>
        ///     The difference Bitmap
        /// </returns>
        /// <exception cref="T:System.ArgumentException"></exception>
        public Bitmap DifferenceImage(Bitmap first, Bitmap second, Color color)
        {
            if (first == null)
                throw new ArgumentException(string.Concat(ImageResources.ErrorImageEmpty, nameof(first)));

            if (second == null)
                throw new ArgumentException(string.Concat(ImageResources.ErrorImageEmpty, nameof(second)));

            return AnalysisProcessing.DifferenceImage(first, second, color);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Determines whether [is part of] [the specified big image].
        /// </summary>
        /// <param name="bigImage">The big image.</param>
        /// <param name="smallImage">The small image.</param>
        /// <param name="startCoordinates">The start coordinates.</param>
        /// <returns>
        ///     <c>true</c> if [is part of] [the specified big image]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPartOf(Bitmap bigImage, Bitmap smallImage, out Coordinate2D startCoordinates)
        {
            return ImageSlider.IsPartOf(bigImage, smallImage, out startCoordinates);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Determines whether [is part of] [the specified big image].
        /// </summary>
        /// <param name="bigImagePath">The Path to big image.</param>
        /// <param name="smallImagePath">The Path to small image.</param>
        /// <param name="startCoordinates">The start coordinates.</param>
        /// <param name="threshold">The threshold. Optional Parameter</param>
        /// <returns>
        ///     <c>true</c> if [is part of] [the specified big image]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPartOf(string bigImagePath, string smallImagePath, out Coordinate2D startCoordinates,
            int threshold = 0)
        {
            return ImageSlider.IsPartOf(bigImagePath, smallImagePath, out startCoordinates, threshold);
        }
    }
}