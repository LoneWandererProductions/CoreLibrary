/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/ImageHelper.cs
 * PURPOSE:     Some basic helper methods to wire in some other stuff
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Drawing;
using System.IO;

namespace ImageCompare;

/// <summary>
///     Image Helper methods
/// </summary>
internal static class ImageHelper
{
    /// <summary>
    ///     Compares the images.
    /// </summary>
    /// <param name="first">The first.</param>
    /// <param name="second">The second.</param>
    /// <returns>Image compare results</returns>
    /// <exception cref="ArgumentException">Argument Exception</exception>
    internal static ImageCompareData CompareImages(Bitmap first, Bitmap second)
    {
        var oneSimilar = ImageProcessing.GenerateData(first, 0);
        var twoSimilar = ImageProcessing.GenerateData(second, 1);

        return new ImageCompareData
        {
            Similarity = ImageProcessing.GetPercentageDifference(oneSimilar, twoSimilar),
            ImageOne = AnalysisProcessing.GetImageDetails(first).GetDetailsSimple(),
            ImageTwo = AnalysisProcessing.GetImageDetails(second).GetDetailsSimple()
        };
    }

    /// <summary>
    ///     Compares the images.
    /// </summary>
    /// <param name="first">First Image Path</param>
    /// <param name="second">Second Image Path</param>
    /// <returns>Image compare results</returns>
    /// <exception cref="ArgumentException">Argument Exception</exception>
    internal static ImageCompareData CompareImages(string first, string second)
    {
        if (!File.Exists(first))
        {
            throw new ArgumentException(string.Concat(ImageResources.ErrorFileNotFound, first));
        }

        if (!File.Exists(second))
        {
            throw new ArgumentException(string.Concat(ImageResources.ErrorFileNotFound, second));
        }

        var one = new Bitmap(first);
        var two = new Bitmap(second);

        var oneSimilar = ImageProcessing.GenerateData(one, 0);
        var twoSimilar = ImageProcessing.GenerateData(two, 1);

        return new ImageCompareData
        {
            Similarity = ImageProcessing.GetPercentageDifference(oneSimilar, twoSimilar),
            ImageOne = AnalysisProcessing.GetImageDetails(first).GetDetails(),
            ImageTwo = AnalysisProcessing.GetImageDetails(second).GetDetails()
        };
    }
}
