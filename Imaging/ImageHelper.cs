/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/ImageHelper.cs
 * PURPOSE:     Here I try to minimise the footprint of my class and pool all shared methods
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace Imaging
{
    /// <summary>
    ///     Helper methods for image processing.
    /// </summary>
    internal static class ImageHelper
    {
        /// <summary>
        ///     Combines two images by adding their pixel values.
        /// </summary>
        /// <param name="imgOne">The first image.</param>
        /// <param name="imgTwo">The second image.</param>
        /// <returns>A bitmap resulting from the combination of the two images, or null if an error occurs.</returns>
        internal static Bitmap CombineImages(Image imgOne, Image imgTwo)
        {
            var result = new DirectBitmap(imgOne.Width, imgOne.Height);
            var pixelsToSet = new List<(int x, int y, Color color)>();

            using (var dbmOne = new DirectBitmap(imgOne))
            using (var dbmTwo = new DirectBitmap(imgTwo))
            {
                for (var y = 0; y < dbmOne.Height; y++)
                {
                    for (var x = 0; x < dbmOne.Width; x++)
                    {
                        var color1 = dbmOne.GetPixel(x, y);
                        var color2 = dbmTwo.GetPixel(x, y);

                        var r = Math.Min(255, color1.R + color2.R);
                        var g = Math.Min(255, color1.G + color2.G);
                        var b = Math.Min(255, color1.B + color2.B);

                        pixelsToSet.Add((x, y, Color.FromArgb(r, g, b)));
                    }
                }
            }

            try
            {
                result.SetPixelsSimd(pixelsToSet);
                return result.Bitmap;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error setting pixels: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        ///     Checks if the Color is  transparent.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>True if conditions are met</returns>
        internal static bool CheckTransparent(Color color)
        {
            //0,0,0 is Black or Transparent
            return color.R == 0 && color.G == 0 && color.B == 0;
        }

        /// <summary>
        ///     Gets all points in a Circle.
        ///     Uses the  Bresenham's circle drawing algorithm.
        ///     https://en.wikipedia.org/wiki/Midpoint_circle_algorithm
        /// </summary>
        /// <param name="center">The center point.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="length">The length.</param>
        /// <param name="width">The height.</param>
        /// <returns>List of Points</returns>
        internal static List<Point> GetCirclePoints(Point center, int radius, int length, int width)
        {
            var points = new List<Point>();

            for (var x = Math.Max(0, center.X - radius); x <= Math.Min(width - 1, center.X + radius); x++)
            {
                var dx = x - center.X;
                var height = (int)Math.Sqrt((radius * radius) - (dx * dx));

                for (var y = Math.Max(0, center.Y - height); y <= Math.Min(length - 1, center.Y + height); y++)
                {
                    points.Add(new Point(x, y));
                }
            }

            return points;
        }

        /// <summary>
        ///     Generates a Gaussian kernel.
        /// </summary>
        /// <param name="sigma">The sigma value for the Gaussian distribution.</param>
        /// <param name="size">The size of the kernel (must be odd).</param>
        /// <returns>A 2D array representing the Gaussian kernel.</returns>
        internal static double[,] GenerateGaussianKernel(double sigma, int size)
        {
            var kernel = new double[size, size];
            var mean = size / 2.0;
            var sum = 0.0;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    kernel[y, x] =
                        Math.Exp(-0.5 * (Math.Pow((x - mean) / sigma, 2.0) + Math.Pow((y - mean) / sigma, 2.0)))
                        / (2 * Math.PI * sigma * sigma);
                    sum += kernel[y, x];
                }
            }

            // Normalize the kernel
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    kernel[y, x] /= sum;
                }
            }

            return kernel;
        }

        /// <summary>
        ///     Gets the color of the region pixels and the mean color.
        /// </summary>
        /// <param name="dbm">The DirectBitmap base.</param>
        /// <param name="region">The region to process.</param>
        /// <returns>A tuple containing a list of colors and the mean color.</returns>
        internal static (List<Color> Pixels, Color Mean) GetRegionPixelsAndMeanColor(DirectBitmap dbm, Rectangle region)
        {
            var (pixels, meanColor) = ProcessPixels(dbm, region);
            return (pixels, meanColor ?? Color.Black); // Ensure a non-null value for meanColor
        }

        /// <summary>
        ///     Gets the mean color of a specified region.
        /// </summary>
        /// <param name="dbm">The DirectBitmap base.</param>
        /// <param name="region">The region to process.</param>
        /// <returns>The mean color of the specified region.</returns>
        internal static Color GetMeanColor(DirectBitmap dbm, Rectangle region)
        {
            var processPixels = ProcessPixels(dbm, region);
            return processPixels.Mean ?? Color.Black; // Handle case where meanColor is null
        }

        /// <summary>
        ///     Gets the non transparent bounds.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>Area as rectangle that is not transparent anymore</returns>
        internal static Rectangle GetNonTransparentBounds(Bitmap image)
        {
            var minX = image.Width;
            var maxX = 0;
            var minY = image.Height;
            var maxY = 0;

            var hasNonTransparentPixel = false;

            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    var pixel = image.GetPixel(x, y);
                    if (pixel.A != 0) // Not fully transparent
                    {
                        hasNonTransparentPixel = true;
                        if (x < minX)
                        {
                            minX = x;
                        }

                        if (x > maxX)
                        {
                            maxX = x;
                        }

                        if (y < minY)
                        {
                            minY = y;
                        }

                        if (y > maxY)
                        {
                            maxY = y;
                        }
                    }
                }
            }

            if (!hasNonTransparentPixel)
            {
                // If all pixels are transparent, return a zero-sized rectangle
                return new Rectangle(0, 0, 0, 0);
            }

            return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        /// <summary>
        ///     Handles the exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <exception cref="System.ApplicationException"></exception>
        internal static void HandleException(Exception ex)
        {
            // Log the exception details (implementation may vary)
            Trace.WriteLine($"Exception Type: {ex.GetType().Name}");
            Trace.WriteLine($"Message: {ex.Message}");
            Trace.WriteLine($"Stack Trace: {ex.StackTrace}");

            // Optionally, rethrow or handle further
            if (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException ||
                ex is UriFormatException || ex is IOException)
            {
                throw new ApplicationException("An error occurred while processing the image.", ex);
            }
        }

        /// <summary>
        ///     Validates the image.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="image">The image.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        internal static void ValidateImage(string method, Bitmap image)
        {
            if (image == null)
            {
                var innerException =
                    new ArgumentNullException(string.Concat(method, ImagingResources.Spacing, nameof(image)));
                throw new ArgumentNullException(ImagingResources.ErrorWrongParameters, innerException);
            }
        }

        /// <summary>
        ///     Retrieves pixel colors from a region and optionally calculates the mean color.
        /// </summary>
        /// <param name="dbmBase">The DirectBitmap base.</param>
        /// <param name="region">The region to process.</param>
        /// <param name="calculateMeanColor">If true, calculates the mean color. Defaults to true.</param>
        /// <returns>A tuple containing a list of colors and optionally the mean color.</returns>
        private static (List<Color> Pixels, Color? Mean) ProcessPixels(DirectBitmap dbmBase, Rectangle region,
            bool calculateMeanColor = true)
        {
            var pixels = new List<Color>();
            int rSum = 0, gSum = 0, bSum = 0;
            var count = 0;

            for (var y = region.Top; y < region.Bottom; y++)
            {
                for (var x = region.Left; x < region.Right; x++)
                {
                    var pixel = dbmBase.GetPixel(x, y);
                    pixels.Add(pixel);
                    rSum += pixel.R;
                    gSum += pixel.G;
                    bSum += pixel.B;
                    count++;
                }
            }

            Color? meanColor = null;
            if (calculateMeanColor && count > 0)
            {
                var averageRed = rSum / count;
                var averageGreen = gSum / count;
                var averageBlue = bSum / count;
                meanColor = Color.FromArgb(averageRed, averageGreen, averageBlue);
            }

            return (pixels, meanColor);
        }
    }
}
