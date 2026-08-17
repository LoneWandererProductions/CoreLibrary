/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        ImageExtensions.cs
 * PURPOSE:     Extension methods for UnmanagedImageBuffer to convert to BitmapSource.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RenderEngine;

namespace Solaris
{
    /// <summary>
    /// Extension methods for RawTextureBuffer to convert to Bitmap.
    /// </summary>
    internal static class ImageExtensions
    {
        /// <summary>
        /// Converts a System.Drawing.Bitmap into a WPF BitmapImage.
        /// </summary>
        /// <param name="bitmap">The source bitmap.</param>
        /// <returns>A frozen WPF BitmapImage ready for UI rendering, or null if source is null.</returns>
        public static BitmapImage? ToBitmapImage(this Bitmap? bitmap)
        {
            if (bitmap == null) return null;

            using var memory = new MemoryStream();
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Allows cross-thread access and releases stream lock

            return bitmapImage;
        }

        /// <summary>
        /// Updates an existing WriteableBitmap directly from the UnmanagedImageBuffer.
        /// Reuses the existing WPF BackBuffer memory if dimensions match.
        /// </summary>
        /// <param name="buffer">The source unmanaged image buffer.</param>
        /// <param name="target">The target WPF WriteableBitmap to update.</param>
        /// <returns>The updated or newly instantiated WriteableBitmap.</returns>
        public static WriteableBitmap UpdateWriteableBitmap(this UnmanagedImageBuffer? buffer, WriteableBitmap? target)
        {
            if (buffer == null) return null!;

            if (target == null || target.PixelWidth != buffer.Width || target.PixelHeight != buffer.Height)
            {
                target = new WriteableBitmap(
                    buffer.Width,
                    buffer.Height,
                    96, 96,
                    PixelFormats.Bgra32,
                    null);
            }

            target.Lock();

            unsafe
            {
                Buffer.MemoryCopy(
                    buffer.Buffer.ToPointer(),
                    target.BackBuffer.ToPointer(),
                    buffer.Count,
                    buffer.Count);
            }

            target.AddDirtyRect(new Int32Rect(0, 0, buffer.Width, buffer.Height));
            target.Unlock();

            return target;
        }
    }
}
