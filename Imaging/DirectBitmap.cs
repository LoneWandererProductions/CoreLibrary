﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/DirectBitmap.cs
 * PURPOSE:     Custom Image Class, speeds up Get and Set Pixel
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Imaging
{
    /// <inheritdoc />
    /// <summary>
    ///     Simple elegant Solution to get Color of an pixel, for more information look into Source.
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    public sealed class DirectBitmap : IDisposable
    {
        /// <summary>
        ///     Gets the bits.
        /// </summary>
        /// <value>
        ///     The bits.
        /// </value>
        private readonly int[] _bits;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectBitmap" /> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            _bits = new int[width * height];
            BitsHandle = GCHandle.Alloc(_bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb,
                BitsHandle.AddrOfPinnedObject());
        }

        /// <summary>
        ///     Gets the bitmap.
        /// </summary>
        /// <value>
        ///     The bitmap.
        /// </value>
        public Bitmap Bitmap { get; }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="DirectBitmap" /> is disposed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if disposed; otherwise, <c>false</c>.
        /// </value>
        private bool Disposed { get; set; }

        /// <summary>
        ///     Gets the height.
        /// </summary>
        /// <value>
        ///     The height.
        /// </value>
        public int Height { get; }

        /// <summary>
        ///     Gets the width.
        /// </summary>
        /// <value>
        ///     The width.
        /// </value>
        public int Width { get; }

        /// <summary>
        ///     Gets the bits handle.
        /// </summary>
        /// <value>
        ///     The bits handle.
        /// </value>
        private GCHandle BitsHandle { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Free up all the Memory.
        ///     See:
        ///     https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1063?view=vs-2019
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <param name="btm">The custom Bitmap.</param>
        public static DirectBitmap GetInstance(Bitmap btm)
        {
            var dbm = new DirectBitmap(btm.Width, btm.Height);

            using var graph = Graphics.FromImage(dbm.Bitmap);
            graph.DrawImage(btm, new Rectangle(0, 0, btm.Width, btm.Height), 0, 0, btm.Width, btm.Height,
                GraphicsUnit.Pixel);

            return dbm;
        }

        /// <summary>
        ///     Sets the pixel.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="color">The color.</param>
        public void SetPixel(int x, int y, Color color)
        {
            var index = x + (y * Width);
            _bits[index] = color.ToArgb();
        }

        /// <summary>
        ///     Gets the pixel.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>Color of the Pixel</returns>
        public Color GetPixel(int x, int y)
        {
            var index = x + (y * Width);
            var col = _bits[index];
            return Color.FromArgb(col);
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (Disposed)
            {
                return;
            }

            if (disposing)
            {
                // free managed resources
                Bitmap.Dispose();
                BitsHandle.Free();
            }

            Disposed = true;
        }

        /// <summary>
        ///     NOTE: Leave out the finalizer altogether if this class doesn't
        ///     own unmanaged resources, but leave the other methods
        ///     exactly as they are.
        ///     Finalizes an instance of the <see cref="DirectBitmap" /> class.
        /// </summary>
        ~DirectBitmap()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
    }
}
