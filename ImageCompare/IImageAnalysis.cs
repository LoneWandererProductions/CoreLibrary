/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/IImageAnalysis.cs
 * PURPOSE:     Interface for the Image Analysis Tools
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMemberInSuper.Global

using System;
using System.Collections.Generic;
using System.Drawing;

namespace ImageCompare
{
    /// <summary>
    ///     Interface Definitions
    /// </summary>
    internal interface IImageAnalysis
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
        /// <exception cref="ArgumentException">Argument Exception</exception>
        /// <exception cref="InvalidOperationException">Invalid Operation</exception>
        List<string> FindImagesInColorRange(int r, int g, int b, int range, string folderPath,
            bool checkSubfolders,
            IEnumerable<string> extensions);

        /// <summary>
        ///     Gets the image details.
        /// </summary>
        /// <param name="imagePath">The image path.</param>
        /// <returns>Image Details as Image Data Object</returns>
        /// <exception cref="ArgumentException">Argument Exception</exception>
        /// <exception cref="InvalidOperationException">Invalid Operation</exception>
        ImageData GetImageDetails(string imagePath);

        /// <summary>
        ///     Gets the image details.
        /// </summary>
        /// <param name="imagePaths">The image paths.</param>
        /// <returns>List of Image Details as Image Data Object</returns>
        /// <exception cref="ArgumentException">Argument Exception</exception>
        /// <exception cref="InvalidOperationException">Invalid Operation</exception>
        List<ImageData> GetImageDetails(List<string> imagePaths);

        /// <summary>
        /// Compares the two images.
        /// </summary>
        /// <param name="one">The Bitmap one.</param>
        /// <param name="two">The Bitmap two.</param>
        /// <returns>Data about two images</returns>
        ImageCompareData CompareImages(Bitmap one, Bitmap two);

        /// <summary>
        /// Compares the two images.
        /// </summary>
        /// <param name="pathOne">The path to image one.</param>
        /// <param name="pathTwo">The path to image two.</param>
        /// <returns>Data about two images</returns>
        ImageCompareData CompareImages(string pathOne, string pathTwo);
    }
}
