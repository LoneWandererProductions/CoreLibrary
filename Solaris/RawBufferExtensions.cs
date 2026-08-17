/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        RawTextureBufferExtensions.cs
 * PURPOSE:     Extension methods for RawTextureBuffer to convert to Bitmap.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using Imaging.Texture;
using RenderEngine;

namespace Solaris
{
    /// <summary>
    /// Extension methods for RawTextureBuffer to convert to Bitmap.
    /// </summary>
    internal static class RawBufferExtensions
    {
        /// <summary>
        /// Converts to bitmap.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>The bitmap representation of the raw texture buffer.</returns>
        public static Bitmap ToBitmap(this RawTextureBuffer buffer)
        {
            var bitmap = new Bitmap(buffer.Width, buffer.Height, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, buffer.Width, buffer.Height);
            var bmpData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);

            Marshal.Copy(buffer.PixelData, 0, bmpData.Scan0, buffer.PixelData.Length);
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }

        /// <summary>
        /// Copies unmanaged buffer memory directly into WPF's WriteableBitmap BackBuffer.
        /// Zero GDI+ involvement, zero intermediate bitmap allocations.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        public static unsafe void CopyToWriteableBitmap(this UnmanagedImageBuffer source, WriteableBitmap target)
        {
            if (target.PixelWidth != source.Width || target.PixelHeight != source.Height)
                return;

            target.Lock();

            // Copy unmanaged memory straight into WPF back-buffer handle
            Buffer.MemoryCopy(
                source.Buffer.ToPointer(),
                target.BackBuffer.ToPointer(),
                source.Count,
                source.Count);

            target.AddDirtyRect(new Int32Rect(0, 0, source.Width, source.Height));
            target.Unlock();
        }
    }
}
