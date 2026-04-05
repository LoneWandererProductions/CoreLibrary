/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Gifs
 * FILE:        ImageGifHelper.cs
 * PURPOSE:     Some Helpers for ImageGif.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Windows;
using System.Windows.Media.Imaging;

namespace Imaging.Gifs
{
    internal static class ImageGifHelper
    {

        /// <summary>
        /// Gets the short.
        /// </summary>
        /// <param name="meta">The meta.</param>
        /// <param name="query">The query.</param>
        /// <returns>Meta Data as int.</returns>
        internal static int GetShort(BitmapMetadata? meta, string query)
        {
            try
            {
                return (ushort)(meta?.GetQuery(query) ?? 0);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Clears the bitmap.
        /// </summary>
        /// <param name="bmp">The BMP.</param>
        internal static void ClearBitmap(WriteableBitmap bmp)
        {
            int stride = bmp.PixelWidth * 4;
            byte[] clear = new byte[stride * bmp.PixelHeight];

            bmp.WritePixels(
                new Int32Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight),
                clear,
                stride,
                0);
        }

        /// <summary>
        /// Clears the region.
        /// </summary>
        /// <param name="bmp">The BMP.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="w">The w.</param>
        /// <param name="h">The h.</param>
        internal static void ClearRegion(WriteableBitmap bmp, int x, int y, int w, int h)
        {
            int stride = w * 4;
            byte[] clear = new byte[stride * h];

            bmp.WritePixels(
                new Int32Rect(x, y, w, h),
                clear,
                stride,
                0);
        }

        /// <summary>
        /// Copies the bitmap.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="dst">The DST.</param>
        internal static void CopyBitmap(WriteableBitmap src, WriteableBitmap dst)
        {
            int stride = src.PixelWidth * 4;
            byte[] buffer = new byte[stride * src.PixelHeight];

            src.CopyPixels(buffer, stride, 0);

            dst.WritePixels(
                new Int32Rect(0, 0, src.PixelWidth, src.PixelHeight),
                buffer,
                stride,
                0);
        }

        /// <summary>
        /// Blends the frame.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="frame">The frame.</param>
        /// <param name="offsetX">The offset x.</param>
        /// <param name="offsetY">The offset y.</param>
        internal static void BlendFrame(WriteableBitmap target, BitmapSource frame, int offsetX, int offsetY)
        {
            int w = frame.PixelWidth;
            int h = frame.PixelHeight;

            int stride = w * 4;
            byte[] srcPixels = new byte[stride * h];
            frame.CopyPixels(srcPixels, stride, 0);

            byte[] dstPixels = new byte[stride * h];

            target.CopyPixels(
                new Int32Rect(offsetX, offsetY, w, h),
                dstPixels,
                stride,
                0);

            for (int i = 0; i < srcPixels.Length; i += 4)
            {
                byte a = srcPixels[i + 3];

                if (a == 255)
                {
                    // fully opaque → overwrite
                    dstPixels[i + 0] = srcPixels[i + 0];
                    dstPixels[i + 1] = srcPixels[i + 1];
                    dstPixels[i + 2] = srcPixels[i + 2];
                    dstPixels[i + 3] = 255;
                }
                else if (a > 0)
                {
                    // alpha blend
                    float alpha = a / 255f;

                    dstPixels[i + 0] = (byte)(srcPixels[i + 0] * alpha + dstPixels[i + 0] * (1 - alpha));
                    dstPixels[i + 1] = (byte)(srcPixels[i + 1] * alpha + dstPixels[i + 1] * (1 - alpha));
                    dstPixels[i + 2] = (byte)(srcPixels[i + 2] * alpha + dstPixels[i + 2] * (1 - alpha));
                    dstPixels[i + 3] = 255;
                }
                // else fully transparent → keep dst
            }

            target.WritePixels(
                new Int32Rect(offsetX, offsetY, w, h),
                dstPixels,
                stride,
                0);
        }
    }
}
