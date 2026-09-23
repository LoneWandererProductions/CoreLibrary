/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     ImageCompare
* FILE:        ImageProcessing.cs
* PURPOSE:     Basic Processing of Images, in this case mostly for Similar Images
* PROGRAMMER:  Peter Geinitz (Wayfarer)
*/

using System.Collections.Concurrent;
using System.Drawing;

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

            var size = ImageResources.DuplicateSize;
            var totalPixels = size * size;
            var imageBytes = new byte[totalPixels];

            var rect = new Rectangle(0, 0, size, size);
            var bmpData = scaled.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            double rSum = 0, gSum = 0, bSum = 0;

            unsafe
            {
                var ptr = (byte*)bmpData.Scan0;
                var stride = bmpData.Stride;

                for (var y = 0; y < size; y++)
                {
                    var row = ptr + (y * stride);
                    for (var x = 0; x < size; x++)
                    {
                        var pixelIdx = x * 4;
                        var b = row[pixelIdx];
                        var g = row[pixelIdx + 1];
                        var r = row[pixelIdx + 2];

                        var gray = (byte)(r * 0.299 + g * 0.587 + b * 0.114);
                        imageBytes[y * size + x] = gray;

                        rSum += r * 0.299;
                        gSum += g * 0.587;
                        bSum += b * 0.114;
                    }
                }
            }

            scaled.UnlockBits(bmpData);

            return new ImageSimilar
            {
                R = (byte)(rSum / totalPixels),
                G = (byte)(gSum / totalPixels),
                B = (byte)(bSum / totalPixels),
                Id = id,
                Image = imageBytes
            };
        }

        /// <summary>
        ///     Gets the percentage difference.
        /// </summary>
        /// <param name="imageToCompareTo">The image to compare to.</param>
        /// <param name="targetBitmap">The target bitmap.</param>
        /// <returns>Difference in Percentage</returns>
        internal static float GetPercentageDifference(in ImageSimilar imageToCompareTo, in ImageSimilar targetBitmap)
        {
            if (imageToCompareTo.Image == null || targetBitmap.Image == null)
            {
                return 0f;
            }

            ReadOnlySpan<byte> img1 = imageToCompareTo.Image;
            ReadOnlySpan<byte> img2 = targetBitmap.Image;
            var threshold = ImageResources.ColorThreshold;
            var diff = 0;

            var length = Math.Min(img1.Length, img2.Length);

            for (var i = 0; i < length; i++)
            {
                if (Math.Abs(img1[i] - img2[i]) <= threshold)
                {
                    diff++;
                }
            }

            var pixel = (float)diff / ImageResources.MaxPixel * 100f;

            var color = ((ImageResources.MaxColor - Math.Abs(imageToCompareTo.R - targetBitmap.R)) /
                         (float)ImageResources.MaxColor +
                         (ImageResources.MaxColor - Math.Abs(imageToCompareTo.G - targetBitmap.G)) /
                         (float)ImageResources.MaxColor +
                         (ImageResources.MaxColor - Math.Abs(imageToCompareTo.B - targetBitmap.B)) /
                         (float)ImageResources.MaxColor) / 3f * 100f;

            return (pixel + color) / 2f;
        }

        /// <summary>
        ///     Find all duplicate images from in list
        /// </summary>
        /// <param name="imageToCompareTo">The path of image to compare to.</param>
        /// <param name="images">The paths to the images to check for duplicates</param>
        /// <param name="maximumDifferenceInPercentage">The maximum difference in percentage.</param>
        /// <returns>
        ///     A list of paths to all the duplicates found.
        /// </returns>
        internal static List<ImageSimilar>? FindSimilarImages(
            ImageSimilar imageToCompareTo,
            IReadOnlyList<ImageSimilar> images,
            float maximumDifferenceInPercentage)
        {
            var similarImagesFound = new List<ImageSimilar>();

            // Process sequentially when candidate list is small to prevent thread contention
            if (images.Count < 64)
            {
                for (var i = 0; i < images.Count; i++)
                {
                    if (GetPercentageDifference(images[i], imageToCompareTo) >= maximumDifferenceInPercentage)
                    {
                        similarImagesFound.Add(images[i]);
                    }
                }
            }
            else
            {
                var bag = new ConcurrentBag<ImageSimilar>();
                Parallel.For(0, images.Count, i =>
                {
                    if (GetPercentageDifference(images[i], imageToCompareTo) >= maximumDifferenceInPercentage)
                    {
                        bag.Add(images[i]);
                    }
                });
                similarImagesFound = bag.ToList();
            }

            return similarImagesFound.Count <= 1 ? null : similarImagesFound;
        }
    }
}
