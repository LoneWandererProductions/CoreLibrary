/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/ImageProcessing.cs
 * PURPOSE:     Basic Processing of Images, in this case mostly for Similar Images
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading.Tasks;
using ExtendedSystemObjects;
using Imaging;

namespace ImageCompare
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
        ///     Find all duplicate images from in list
        /// </summary>
        /// <param name="imageToCompareTo">The path of image to compare to.</param>
        /// <param name="images">The paths to the images to check for duplicates</param>
        /// <param name="maximumDifferenceInPercentage">The maximum difference in percentage.</param>
        /// <returns>
        ///     A list of paths to all the duplicates found.
        /// </returns>
        [return: MaybeNull]
        internal static List<ImageSimilar> FindSimilarImages(ImageSimilar imageToCompareTo,
            IEnumerable<ImageSimilar> images,
            float maximumDifferenceInPercentage)
        {
            var similarImagesFound = new List<ImageSimilar>();

            _ = Parallel.ForEach(images, image =>
            {
                var percentageDiff =
                    GetPercentageDifference(image, imageToCompareTo);
                if (percentageDiff >= maximumDifferenceInPercentage)
                {
                    similarImagesFound.Add(image);
                }
            });

            if (similarImagesFound.IsNullOrEmpty())
            {
                return null;
            }

            return similarImagesFound.Count == 1 ? null : similarImagesFound;
        }

        /// <summary>
        ///     Generates the data. No need to resize! Only Change to  Greyscale.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>Image Object to compare</returns>
        internal static ImageSimilar GenerateData(Bitmap bitmap, int id)
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
            bitmap = Render.FilterImage(bitmap, ImageFilter.GrayScale);

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

            return new ImageSimilar
            {
                R = r,
                G = g,
                B = b,
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
                (((ImageResources.MaxColor - Math.Abs(imageToCompareTo.R - targetBitmap.R)) / ImageResources.MaxColor) +
                 ((ImageResources.MaxColor - Math.Abs(imageToCompareTo.G - targetBitmap.G)) / ImageResources.MaxColor) +
                 ((ImageResources.MaxColor - Math.Abs(imageToCompareTo.B - targetBitmap.B)) /
                  ImageResources.MaxColor)) / 3 * 100;

            return (pixel + color) / 2;
        }
    }
}
