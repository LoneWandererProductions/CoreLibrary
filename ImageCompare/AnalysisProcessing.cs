/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/AnalysisProcessing.cs
 * PURPOSE:     Basic Processing of Images, in this case mostly for specific Image Analysis
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExtendedSystemObjects;
using Imaging;

namespace ImageCompare
{
    /// <summary>
    ///     Image Analysis
    /// </summary>
    internal static class AnalysisProcessing
    {
        /// <summary>
        ///     The render
        /// </summary>
        private static readonly ImageRender Render = new();

        /// <summary>
        ///     Compares a list of Images and returns the Difference in Percentage
        /// </summary>
        /// <param name="imagePaths">Paths to the Images</param>
        /// <returns>Similarity to the first Image in the List</returns>
        /// <exception cref="InvalidOperationException">Invalid Operation</exception>
        [return: MaybeNull]
        internal static List<float> GetSimilarity(List<string> imagePaths)
        {
            if (imagePaths.IsNullOrEmpty()) return null;

            if (imagePaths.Count == 1) return null;

            var paths = new List<string>(imagePaths);

            var path = imagePaths[0];
            if (!File.Exists(path)) return null;

            var lst = new List<float>(paths.Count - 1);

            try
            {
                using var btmOne = new Bitmap(path);
                var first = ImageProcessing.GenerateData(btmOne, 0);
                paths.RemoveAt(0);

                //with sanity check in Case one file went missing, we won't have to stop everything
                foreach (var element in paths.Where(File.Exists))
                    try
                    {
                        using var btm = new Bitmap(element);
                        //we have 2 Bitmaps, generate SimImage
                        var second = ImageProcessing.GenerateData(btm, 1);
                        var cache = ImageProcessing.GetPercentageDifference(first, second);
                        lst.Add(cache);
                    }
                    catch (InvalidOperationException ex)
                    {
                        //Could not load an Image
                        Trace.WriteLine(ex);
                        throw new InvalidOperationException(ex.ToString());
                    }
            }
            catch (InvalidOperationException ex)
            {
                //Could not load an Image
                Trace.WriteLine(ex);
                throw new InvalidOperationException(ex.ToString());
            }

            //File was skipped? Return null
            if (lst.Count != imagePaths.Count - 1) return null;

            lst.AddFirst(100);
            return lst;
        }

        /// <summary>
        ///     Generates the data.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="path">The Path to the image.</param>
        /// <returns>ImageColor Object</returns>
        internal static ImageColor GenerateData(Bitmap bitmap, string path)
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
            bitmap = Render.FilterImage(bitmap, FiltersType.GrayScale);

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

            return new ImageColor { R = r, G = g, B = b, Path = path };
        }

        /// <summary>
        ///     Converts the Image into a Dictionary of Colors.
        /// </summary>
        /// <param name="image">The color Dictionary.</param>
        internal static Dictionary<Color, int> GetColors(Bitmap image)
        {
            var cif = new Cif(image);
            return cif.ColorCount;
        }

        /// <summary>
        ///     Gets the image details.
        /// </summary>
        /// <param name="imagePath">The image path.</param>
        /// <returns>Image Data</returns>
        [return: MaybeNull]
        internal static ImageData GetImageDetails(string imagePath)
        {
            if (!File.Exists(imagePath)) return null;

            using var btm = new Bitmap(imagePath);
            var color = GenerateData(btm, string.Empty);

            return new ImageData
            {
                ImagePath = imagePath,
                ImageName = Path.GetFileName(imagePath),
                Height = btm.Height,
                Width = btm.Width,
                Size = new FileInfo(imagePath).Length,
                R = color.R,
                G = color.G,
                B = color.B,
                Similarity = 100,
                Extension = Path.GetExtension(imagePath)
            };
        }

        /// <summary>
        ///     Gets the image details.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>Image Data</returns>
        [return: MaybeNull]
        internal static ImageData GetImageDetails(Bitmap image)
        {
            var color = GenerateData(image, string.Empty);

            return new ImageData
            {
                Height = image.Height,
                Width = image.Width,
                R = color.R,
                G = color.G,
                B = color.B,
                Similarity = 100
            };
        }

        /// <summary>
        ///     Generates a differences bitmap.
        /// </summary>
        /// <param name="first">The first bitmap.</param>
        /// <param name="second">The second bitmap.</param>
        /// <param name="color">The color.</param>
        /// <returns>The difference Bitmap</returns>
        internal static Bitmap DifferenceImage(Bitmap first, Bitmap second, Color color)
        {
            var width = Math.Min(first.Width, second.Width);
            var height = Math.Min(first.Height, second.Height);

            var canvas = Render.CutBitmap(first, 0, 0, height, width);

            using var dbmCanvas = new DirectBitmap(canvas);
            using var dbmCompare = new DirectBitmap(second);

            // Access the pixel arrays directly for comparison
            var canvasPixels = dbmCanvas.Bits;
            var comparePixels = dbmCompare.Bits;
            var colorArgb = color.ToArgb();

            // Process the pixels in parallel
            _ = Parallel.For(0, height, y =>
            {
                var offset = y * width;
                for (var x = 0; x < width; x++)
                {
                    var index = offset + x;
                    if (canvasPixels[index] != comparePixels[index]) canvasPixels[index] = colorArgb;
                }
            });

            return dbmCanvas.Bitmap;
        }
    }
}