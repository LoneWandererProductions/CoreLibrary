﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/DirectBitmap.cs
 * PURPOSE:     Custom Image Class, speeds up Get and Set Pixel
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Sources:     https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.gchandle.addrofpinnedobject?view=net-7.0
 *              https://learn.microsoft.com/en-us/dotnet/api/system.drawing.imaging.pixelformat?view=dotnet-plat-ext-8.0
 *              https://learn.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.-ctor?view=dotnet-plat-ext-8.0#system-drawing-bitmap-ctor(system-int32-system-int32-system-int32-system-drawing-imaging-pixelformat-system-intptr)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal

using System;
using System.Collections.Generic;
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
        ///     Initializes a new instance of the <see cref="DirectBitmap" /> class.
        ///     Bitmap which references pixel data directly
        ///     PixelFormat, Specifies the format of the color data for each pixel in the image.
        ///     AddrOfPinnedObject, reference to address of pinned object
        ///     GCHandleType, Retrieves the address of object data in a Pinned handle.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;

            Initiate();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectBitmap" /> class.
        ///     Bitmap which references pixel data directly
        ///     PixelFormat, Specifies the format of the color data for each pixel in the image.
        ///     AddrOfPinnedObject, reference to address of pinned object
        ///     GCHandleType, Retrieves the address of object data in a Pinned handle.
        /// </summary>
        /// <param name="btm">The in question.</param>
        public DirectBitmap(Image btm)
        {
            Width = btm.Width;
            Height = btm.Height;

            Initiate();

            using var graph = Graphics.FromImage(Bitmap);
            graph.DrawImage(btm, new Rectangle(0, 0, btm.Width, btm.Height), 0, 0, btm.Width, btm.Height,
                GraphicsUnit.Pixel);
        }

        /// <summary>
        ///     Gets the bits.
        /// </summary>
        /// <value>
        ///     The bits.
        /// </value>
        public int[] Bits { get; set; }

        /// <summary>
        ///     Gets the bitmap.
        /// </summary>
        /// <value>
        ///     The bitmap.
        /// </value>
        public Bitmap Bitmap { get; set; }

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
        private GCHandle BitsHandle { get; set; }

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
        ///     Initiates this instance and sets all Helper Variables.
        /// </summary>
        private void Initiate()
        {
            Bits = new int[Width * Height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb,
                BitsHandle.AddrOfPinnedObject());
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
        ///     Draws a vertical line with a specified color.
        ///     For now Microsoft's Rectangle Method is faster in certain circumstances
        /// </summary>
        /// <param name="x">The x Coordinate.</param>
        /// <param name="y">The y Coordinate.</param>
        /// <param name="height">The height.</param>
        /// <param name="color">The color.</param>
        public void DrawVerticalLine(int x, int y, int height, Color color)
        {
            for (var i = y; i < height; i++)
            {
                SetPixel(x, i, color);
            }
        }

        /// <summary>
        ///     Draws a horizontal line with a specified color.
        ///     For now Microsoft's Rectangle Method is faster in certain circumstances
        ///     ///
        /// </summary>
        /// <param name="x">The x Coordinate.</param>
        /// <param name="y">The y Coordinate.</param>
        /// <param name="length">The length.</param>
        /// <param name="color">The color.</param>
        public void DrawHorizontalLine(int x, int y, int length, Color color)
        {
            for (var i = x; i < length; i++)
            {
                SetPixel(i, y, color);
            }
        }

        /// <summary>
        ///     Draws the rectangle.
        ///     For now Microsoft's Rectangle Method is faster
        /// </summary>
        /// <param name="x1">The x Coordinate.</param>
        /// <param name="y2">The y Coordinate.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="color">The color.</param>
        public void DrawRectangle(int x1, int y2, int width, int height, Color color)
        {
            // Iterate over the pixels within the rectangle
            for (var y = y2; y < y2 + height && y < Height; y++)
            {
                for (var x = x1; x < x1 + width && x < Width; x++)
                {
                    // Set the color of each pixel
                    SetPixel(x, y, color);
                }
            }
        }

        /// <summary>
        ///     Sets the area.
        /// </summary>
        /// <param name="idList">The identifier list.</param>
        /// <param name="color">The color.</param>
        public void SetArea(IEnumerable<int> idList, Color color)
        {
            foreach (var index in idList)
            {
                Bits[index] = color.ToArgb();
            }
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
            Bits[index] = color.ToArgb();
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
            var col = Bits[index];
            return Color.FromArgb(col);
        }

        /// <summary>
        ///     Gets the color list.
        /// </summary>
        /// <returns>The Image as a list of Colors</returns>
        public Span<Color> GetColors()
        {
            if (Bits == null)
            {
                return null;
            }

            var length = Height * Width;

            var array = new Color[length];

            var span = new Span<Color>(array, 0, length);

            for (var i = 0; i < length; i++)
            {
                var col = Bits[i];
                span[i] = Color.FromArgb(col);
            }

            return span;
        }

        /// <summary>
        ///     Converts to string.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var info = string.Empty;

            for (var i = 0; i < Bits.Length - 1; i++)
            {
                info = string.Concat(info, Bits[i], ImagingResources.Indexer);
            }

            return string.Concat(info, ImagingResources.Spacing, Bits[Bits.Length]);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Height, Width);
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
