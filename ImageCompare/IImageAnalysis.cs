/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/IImageAnalysis.cs
 * PURPOSE:     Interface for the Image Analysis Tools
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Drawing;
using Mathematics;

namespace ImageCompare;

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
    ///     Compares the two images.
    /// </summary>
    /// <param name="first">The Bitmap one.</param>
    /// <param name="second">The Bitmap two.</param>
    /// <returns>Data about two images</returns>
    /// <exception cref="ArgumentException">Argument Exception</exception>
    ImageCompareData CompareImages(Bitmap first, Bitmap second);

    /// <summary>
    ///     Compares the two images.
    /// </summary>
    /// <param name="first">The path to image one.</param>
    /// <param name="second">The path to image two.</param>
    /// <returns>Data about two images</returns>
    /// <exception cref="ArgumentException">Argument Exception</exception>
    ImageCompareData CompareImages(string first, string second);

    /// <summary>
    ///     Gets the colors of an Image and collects them with the amount in a Dictionary.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <returns>Color Dictionary</returns>
    /// <exception cref="ArgumentException">Argument Exception</exception>
    Dictionary<Color, int> GetColors(string path);

    /// <summary>
    ///     Gets the colors of an Image and collects them with the amount in a Dictionary.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <returns>Color Dictionary</returns>
    /// <exception cref="ArgumentException">Argument Exception</exception>
    Dictionary<Color, int> GetColors(Bitmap image);

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
    Bitmap DifferenceImage(Bitmap first, Bitmap second, Color color);

    /// <summary>
    ///     Determines whether [is part of] [the specified big image].
    /// </summary>
    /// <param name="bigImage">The big image.</param>
    /// <param name="smallImage">The small image.</param>
    /// <param name="startCoordinates">The start coordinates.</param>
    /// <returns>
    ///     <c>true</c> if [is part of] [the specified big image]; otherwise, <c>false</c>.
    /// </returns>
    bool IsPartOf(Bitmap bigImage, Bitmap smallImage, out Coordinate2D startCoordinates);


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
    bool IsPartOf(string bigImagePath, string smallImagePath, out Coordinate2D startCoordinates, int threshold = 0);
}
