/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects
 * FILE:        AlphaBlender.cs
 * PURPOSE:     Straight-alpha "source over" blending on BGRA buffers.
 *              Shared by LayeredImageContainer and the document renderer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Documents;

namespace Imaging.Objects
{
    internal static unsafe class AlphaBlender
    {
        /// <summary>
        ///     Blends <paramref name="source" /> over <paramref name="destination" /> in place.
        ///     Both spans are tightly packed straight-alpha BGRA and must have the same length.
        ///     This is the loop that used to live inside <see cref="LayeredImageContainer" />, extended by an
        ///     opacity (0..255) that scales the source alpha.
        /// </summary>
        /// <param name="destination">The pixels to blend onto.</param>
        /// <param name="source">The pixels to blend.</param>
        /// <param name="opacity">Layer opacity, 255 = unchanged source alpha.</param>
        internal static void Over(Span<byte> destination, ReadOnlySpan<byte> source, int opacity = 255)
        {
            if (destination.Length != source.Length) throw new ArgumentException(ImageResource.ErrorInputBuffer);

            if (opacity <= 0) return;
            if (opacity > 255) opacity = 255;

            var length = destination.Length;

            fixed (byte* pBase = destination)
            fixed (byte* pOverlay = source)
            {
                for (var i = 0; i + 3 < length; i += 4)
                {
                    int srcA = pOverlay[i + 3];

                    // 1. Fast path: fully transparent overlay pixel.
                    if (srcA == 0) continue;

                    if (opacity != 255)
                    {
                        srcA = srcA * opacity / 255;
                        if (srcA == 0) continue;
                    }

                    // 2. Fast path: fully opaque result, just overwrite (all 4 bytes in one go).
                    if (srcA == 255)
                    {
                        *(int*)(pBase + i) = *(int*)(pOverlay + i);
                        continue;
                    }

                    // 3. Integer Porter-Duff "over" for straight alpha.
                    int dstA = pBase[i + 3];
                    var invSrcA = 255 - srcA;

                    var outA = (srcA * 255) + (dstA * invSrcA);
                    if (outA == 0) continue;

                    pBase[i] = (byte)(((pOverlay[i] * srcA * 255) + (pBase[i] * dstA * invSrcA)) / outA);
                    pBase[i + 1] = (byte)(((pOverlay[i + 1] * srcA * 255) + (pBase[i + 1] * dstA * invSrcA)) / outA);
                    pBase[i + 2] = (byte)(((pOverlay[i + 2] * srcA * 255) + (pBase[i + 2] * dstA * invSrcA)) / outA);
                    pBase[i + 3] = (byte)(outA / 255);
                }
            }
        }

        /// <summary>
        ///     Blends the pixels of <paramref name="source" /> inside <paramref name="region" /> over the same
        ///     region of <paramref name="destination" />. The region is clipped to both buffers.
        /// </summary>
        internal static void Over(UnmanagedImageBuffer destination, UnmanagedImageBuffer source, PixelRect region,
            int opacity = 255)
        {
            var bounds = new PixelRect(0, 0, Math.Min(destination.Width, source.Width),
                Math.Min(destination.Height, source.Height));

            region = region.Intersect(bounds);
            if (region.IsEmpty) return;

            var dst = destination.BufferSpan;
            var src = source.BufferSpan;
            var rowBytes = region.Width * UnmanagedImageBuffer.BytesPerPixel;

            for (var y = region.Y; y < region.Bottom; y++)
            {
                var d = dst.Slice(((y * destination.Width) + region.X) * UnmanagedImageBuffer.BytesPerPixel, rowBytes);
                var s = src.Slice(((y * source.Width) + region.X) * UnmanagedImageBuffer.BytesPerPixel, rowBytes);
                Over(d, s, opacity);
            }
        }
    }
}
