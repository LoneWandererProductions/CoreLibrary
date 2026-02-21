/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        DireDirectBitmapCorectBitmap.cs
 * PURPOSE:     Shared logic for DirectBitmap and DirectBitmapImage to set and get pixels from the underlying Pixel32 array. 
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Imaging
{
    /// <summary>
    /// Hopefully this will be the last time we need to duplicate code between DirectBitmap and DirectBitmapImage.
    /// This class contains shared logic for both to set and get pixels from the underlying Pixel32 array, as well as an optimized method to set multiple pixels efficiently using contiguous runs per row.
    /// It works on a Pixel32 array only and does not touch any bitmap object, allowing both DirectBitmap and DirectBitmapImage to use it without duplication.
    /// </summary>
    internal static class DirectBitmapCore
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
        internal static void SetPixel(Pixel32[] bits, int width, int height, int x, int y, Pixel32 color)
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
        internal static Pixel32 GetPixel(Pixel32[] bits, int width, int height, int x, int y)
        {
            int index = x + y * width;
            return bits[index];
        }

        /// <summary>
        /// Sets the pixels adaptive.
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="pixels">The pixels.</param>
        /// <param name="threshold">The threshold.</param>
        internal static unsafe void SetPixelsAdaptive(
            Pixel32[] bits,
            int width,
            int height,
            IEnumerable<PixelData> pixels,
            int threshold)
        {
            if (pixels == null) return;

            IEnumerable<PixelData> pixelCollection = pixels;

            int count;
            if (pixels is ICollection<PixelData> col)
            {
                count = col.Count;
            }
            else
            {
                var list = pixels.ToList();
                count = list.Count;
                pixelCollection = list;
            }

            // Small batch → stackalloc
            if (count <= threshold)
            {
                Span<PixelData> span = stackalloc PixelData[count];

                int i = 0;
                foreach (var p in pixelCollection)
                    span[i++] = p;

                SetPixelsUnsafeSpan(bits, width, height, span);
                return;
            }

            // Large batch → linear write (NOT GroupBy)
            foreach (var p in pixelCollection)
            {
                if ((uint)p.X >= width || (uint)p.Y >= height)
                    continue;

                bits[p.Y * width + p.X] =
                    new Pixel32(p.R, p.G, p.B, p.A);
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
        internal static void SetPixelsSimd(Pixel32[] bits, int width, int height,
            IEnumerable<(int x, int y, Pixel32 color)> pixels)
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

        /// <summary>
        /// Sets pixels directly using unsafe pointer arithmetic for speed.
        /// Updates both the WPF back buffer and the Pixel32 array.
        /// </summary>
        /// <param name="bits">The Pixel32 array.</param>
        /// <param name="width">Width of the image.</param>
        /// <param name="height">Height of the image.</param>
        /// <param name="pixels">Pixels to set.</param>
        /// <param name="backBuffer">Pointer to the back buffer (optional, for WPF WriteableBitmap).</param>
        /// <param name="stride">Stride of the back buffer (required if backBuffer is not null).</param>
        internal static unsafe void SetPixelsUnsafe(
            Pixel32[] bits,
            int width,
            int height,
            IEnumerable<PixelData> pixels,
            byte* backBuffer = null,
            int stride = 0)
        {
            if (bits == null || pixels == null) return;

            if (backBuffer != null && stride <= 0)
                throw new ArgumentException("Stride must be positive when backBuffer is provided.");

            foreach (var pixel in pixels)
            {
                if ((uint)pixel.X >= width || (uint)pixel.Y >= height)
                    continue;

                // Update Pixel32 array
                bits[pixel.Y * width + pixel.X] = new Pixel32(pixel.R, pixel.G, pixel.B, pixel.A);

                // Update back buffer if provided
                if (backBuffer != null)
                {
                    var offset = pixel.Y * stride + pixel.X * 4;
                    backBuffer[offset + 0] = pixel.B;
                    backBuffer[offset + 1] = pixel.G;
                    backBuffer[offset + 2] = pixel.R;
                    backBuffer[offset + 3] = pixel.A;
                }
            }
        }

        /// <summary>
        /// Sets the pixels unsafe span.
        /// </summary>
        /// <param name="bits">The bits.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="pixels">The pixels.</param>
        /// <exception cref="System.ArgumentException">Stride must be positive when backBuffer is provided.</exception>
        internal static unsafe void SetPixelsUnsafeSpan(
            Pixel32[] bits,
            int width,
            int height,
            ReadOnlySpan<PixelData> pixels)
        {
            if (bits == null) return;

            fixed (Pixel32* pBits = bits)
            {
                for (int i = 0; i < pixels.Length; i++)
                {
                    var pixel = pixels[i];

                    if ((uint)pixel.X >= width || (uint)pixel.Y >= height)
                        continue;

                    pBits[pixel.Y * width + pixel.X] =
                        new Pixel32(pixel.R, pixel.G, pixel.B, pixel.A);
                }
            }
        }

        /// <summary>
        /// Alpha blends another pixel buffer onto this image using SIMD.
        /// Format: BGRA (32-bit uint). Alpha is premultiplied at runtime.
        /// </summary>
        /// <param name="dstBits">The DST bits.</param>
        /// <param name="src">Source pixels to blend (same size as current bitmap)</param>
        /// <exception cref="System.ArgumentNullException">
        /// dstBits
        /// or
        /// src
        /// </exception>
        /// <exception cref="System.ArgumentException">Source must match image size</exception>
        internal static unsafe void BlendInt(Pixel32[] dstBits, uint[] src)
        {
            if (dstBits == null) throw new ArgumentNullException(nameof(dstBits));
            if (src == null) throw new ArgumentNullException(nameof(src));
            if (dstBits.Length != src.Length)
                throw new ArgumentException("Source must match destination size");

            var dstSpan = MemoryMarshal.Cast<Pixel32, uint>(dstBits.AsSpan());
            var srcSpan = src.AsSpan();

            int len = dstSpan.Length;

            fixed (uint* pDst = dstSpan)
            fixed (uint* pSrc = srcSpan)
            {
                uint* dPtr = pDst;
                uint* sPtr = pSrc;

                for (int i = 0; i < len; i++)
                {
                    uint s = *sPtr;

                    uint sa = s >> 24;
                    if (sa == 0)
                    {
                        dPtr++;
                        sPtr++;
                        continue;
                    }

                    if (sa == 255)
                    {
                        *dPtr = s;
                        dPtr++;
                        sPtr++;
                        continue;
                    }

                    uint d = *dPtr;

                    uint da = d >> 24;
                    uint dr = (d >> 16) & 0xFF;
                    uint dg = (d >> 8) & 0xFF;
                    uint db = d & 0xFF;

                    uint sr = (s >> 16) & 0xFF;
                    uint sg = (s >> 8) & 0xFF;
                    uint sb = s & 0xFF;

                    uint invA = 255 - sa;

                    uint r = (sr * sa + dr * invA) / 255;
                    uint g = (sg * sa + dg * invA) / 255;
                    uint b = (sb * sa + db * invA) / 255;

                    uint a = sa + ((da * invA) / 255);

                    *dPtr = (a << 24) | (r << 16) | (g << 8) | b;

                    dPtr++;
                    sPtr++;
                }
            }
        }
    }
}
