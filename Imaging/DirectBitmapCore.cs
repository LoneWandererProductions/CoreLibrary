/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        DireDirectBitmapCorectBitmap.cs
 * PURPOSE:     Shared logic for DirectBitmap and DirectBitmapImage to set and get pixels from the underlying Pixel32 array. 
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;

namespace Imaging
{
    /// <summary>
    /// Hopefully this will be the last time we need to duplicate code between DirectBitmap and DirectBitmapImage. 
    /// This class contains shared logic for both to set and get pixels from the underlying Pixel32 array, as well as an optimized method to set multiple pixels efficiently using contiguous runs per row.
    /// It works on a Pixel32 array only and does not touch any bitmap object, allowing both DirectBitmap and DirectBitmapImage to use it without duplication.
    /// </summary>
    public static class DirectBitmapCore
    {
        /// <summary>
        /// Sets the pixel.
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="color">The color.</param>
        public static void SetPixel(Pixel32[] bits, int width, int height, int x, int y, Pixel32 color)
        {
            if ((uint)x >= width || (uint)y >= height) return;

            int index = x + y * width;
            bits[index] = color;
        }

        /// <summary>
        /// Gets the pixel.
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>Pixel32 Struct for the Coordinate.</returns>
        public static Pixel32 GetPixel(Pixel32[] bits, int width, int height, int x, int y)
        {
            int index = x + y * width;
            return bits[index];
        }

        /// <summary>
        /// Sets the pixels.
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="pixels">The pixels.</param>
        public static void SetPixels(Pixel32[] bits, int width, int height, IEnumerable<PixelData> pixels)
        {
            foreach (var pixel in pixels)
            {
                if ((uint)pixel.X >= width || (uint)pixel.Y >= height) continue;

                bits[pixel.Y * width + pixel.X] =
                    new Pixel32(pixel.R, pixel.G, pixel.B, pixel.A);
            }
        }

        /// <summary>
        /// Sets multiple pixels efficiently using contiguous runs per row.
        /// Works on a Pixel32 array only; does not touch any bitmap object.
        /// </summary>
        /// <param name="bits">The pixel buffer.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="pixels">Enumerable of (x, y, Pixel32) tuples to set.</param>
        public static void SetPixelsSimd(Pixel32[] bits, int width, int height, IEnumerable<(int x, int y, Pixel32 color)> pixels)
        {
            if (bits == null || pixels == null) return;

            // Group pixels by row and color to make contiguous writes cache-friendly
            var grouped = pixels
                .Where(p => (uint)p.x < width && (uint)p.y < height) // bounds check
                .GroupBy(p => (p.y, p.color));

            foreach (var group in grouped)
            {
                int y = group.Key.y;
                Pixel32 color = group.Key.color;

                // Sort X positions to detect contiguous runs
                var xs = group.Select(p => p.x).Order().ToArray();

                int i = 0;
                while (i < xs.Length)
                {
                    int runStart = xs[i];
                    int runLength = 1;

                    // Detect contiguous sequence
                    while (i + runLength < xs.Length && xs[i + runLength] == runStart + runLength)
                        runLength++;

                    int startIndex = runStart + (y * width);

                    // Scalar write for the run
                    for (int offset = 0; offset < runLength; offset++)
                        bits[startIndex + offset] = color;

                    i += runLength;
                }
            }
        }
    }
}

