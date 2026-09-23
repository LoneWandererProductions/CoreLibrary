/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     ImageCompare
* FILE:        ImageProcessing.cs
* PURPOSE:     Basic Processing of Images, in this case mostly for Similar Images
* PROGRAMMER:  Peter Geinitz (Wayfarer)
*/

using System.Drawing;
using Extended.Extensions;

namespace Imaging.Compare
{
    /// <summary>
    ///     Helper Class that handles the specific way to get the similarity of two Images
    /// </summary>
    internal static class ImageProcessing
    {
        /// <summary>
        ///     The Image render
        /// </summary>
        private static readonly ImageRender Render = new();

        /// <summary>
        ///     Generates the data. No need to resize! Only Change to  Greyscale.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>Image Object to compare</returns>
        internal static ImageSimilar GenerateData(Bitmap? bitmap, int id)
        {
            using var scaled = Render.BitmapScaling(bitmap, ImageResources.DuplicateSize, ImageResources.DuplicateSize);
            using var dbm = DirectBitmap.GetInstance(scaled);

            // Initialize variables for average color value
            var r = 0.0; // Use double for precision
            var g = 0.0;
            var b = 0.0;

            // Create arrays for image and hash
            var image = new byte[ImageResources.DuplicateSize, ImageResources.DuplicateSize];

            // Get total pixels
            const int totalPixels = ImageResources.DuplicateSize * ImageResources.DuplicateSize;

            for (var y = 0; y < ImageResources.DuplicateSize; y++)
            for (var x = 0; x < ImageResources.DuplicateSize; x++)
            {
                var pixel = dbm.GetPixel(x, y);

                // Calculate grayscale value
                var grayValue = (byte)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
                image[x, y] = grayValue; // Store grayscale value

                // Accumulate RGB values based on grayscale contribution
                r += pixel.R * 0.299;
                g += pixel.G * 0.587;
                b += pixel.B * 0.114;
            }

            // Calculate average color values
            r /= totalPixels;
            g /= totalPixels;
            b /= totalPixels;

            // Return the ImageSimilar object
            return new ImageSimilar
            {
                R = (byte)r, // Cast back to byte
                G = (byte)g,
                B = (byte)b,
                Id = id,
                Image = image
            };
        }


        /// <summary>
        ///     Gets the percentage difference.
        /// </summary>
        /// <param name="imageToCompareTo">The image to compare to.</param>
        /// <param name="targetBitmap">The target bitmap.</param>
        /// <returns>Difference in Percentage</returns>
        internal static float GetPercentageDifference(ImageSimilar imageToCompareTo, ImageSimilar targetBitmap)
        {
            var diff = 0;

            for (var y = 0; y < ImageResources.SimilarSize; y++)
            for (var x = 0; x < ImageResources.SimilarSize; x++)
            {
                int one = imageToCompareTo.Image[x, y];
                int two = targetBitmap.Image[x, y];

                if (one.Interval(two, ImageResources.ColorThreshold))
                {
                    diff++;
                }
            }

            var pixel = (float)diff / ImageResources.MaxPixel * 100;

            var color = (float)
                ((ImageResources.MaxColor - Math.Abs(imageToCompareTo.R - targetBitmap.R)) / ImageResources.MaxColor +
                 (ImageResources.MaxColor - Math.Abs(imageToCompareTo.G - targetBitmap.G)) / ImageResources.MaxColor +
                 (ImageResources.MaxColor - Math.Abs(imageToCompareTo.B - targetBitmap.B)) /
                 ImageResources.MaxColor) / 3 * 100;

            return (pixel + color) / 2;
        }
    }
}