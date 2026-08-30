/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare.Compare
 * FILE:        ImageHelper.cs
 * PURPOSE:     Some basic helper methods to wire in some other stuff
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Drawing;

namespace Imaging.Compare
{
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
        internal static ImageCompareData CompareImages(Bitmap? first, Bitmap? second)
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

        /// <summary>
        /// Gets the color count.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>
        /// Color and Counts sorted by most first
        /// </returns>
        internal static Dictionary<Color, int> GetColorCount(Bitmap? image)
        {
            var colorCount = new Dictionary<Color, int>();

            // 1. Guard against null image input
            if (image == null)
            {
                return colorCount;
            }

            // 2. Iterate through every pixel in the bitmap
            for (var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    var color = image.GetPixel(x, y);

                    // Increment the count for this color
                    colorCount[color] = colorCount.GetValueOrDefault(color) + 1;
                }
            }

            // 3. Sort the dictionary by pixel frequency in descending order
            return colorCount
                .OrderByDescending(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }
}