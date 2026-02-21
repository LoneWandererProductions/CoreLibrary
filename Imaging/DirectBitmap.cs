/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        DirectBitmap.cs
 * PURPOSE:     Custom Image Class, speeds up Get and Set Pixel
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Sources:     https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.gchandle.addrofpinnedobject?view=net-7.0
 *              https://learn.microsoft.com/en-us/dotnet/api/system.drawing.imaging.pixelformat?view=dotnet-plat-ext-8.0
 *              https://learn.microsoft.com/en-us/dotnet/api/system.drawing.bitmap.-ctor?view=dotnet-plat-ext-8.0#system-drawing-bitmap-ctor(system-int32-system-int32-system-int32-system-drawing-imaging-pixelformat-system-intptr)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Imaging.Interfaces;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Imaging
{
    /// <inheritdoc cref="IDisposable" />
    /// <summary>
    ///     Simple elegant Solution to get Color of an pixel, for more information look into Source.
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    public sealed class DirectBitmap : IDisposable, IEquatable<DirectBitmap>, IPixelSurface
    {
        /// <summary>
        ///     The synchronize lock
        /// </summary>
        private readonly Lock _syncLock = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectBitmap" /> class.
        ///     Bitmap which references pixel data directly
        ///     PixelFormat, Specifies the format of the color data for each pixel in the image.
        ///     AddrOfPinnedObject, reference to address of pinned object
        ///     GCHandleType, Retrieves the address of object data in a Pinned handle.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="color">The color. Totally optional.</param>
        public DirectBitmap(int width, int height, Color color = default)
        {
            Width = width;
            Height = height;
            Initiate(color);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectBitmap" /> class.
        ///     Bitmap which references pixel data directly
        ///     PixelFormat, Specifies the format of the color data for each pixel in the image.
        ///     AddrOfPinnedObject, reference to address of pinned object
        ///     GCHandleType, Retrieves the address of object data in a Pinned handle.
        /// </summary>
        /// <param name="image">The image in question.</param>
        public DirectBitmap(Image image)
        {
            Width = image.Width;
            Height = image.Height;
            Bits = new Pixel32[Width * Height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);

            using var bmp = new Bitmap(image);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    Bits[y * Width + x] = new Pixel32(c.R, c.G, c.B, c.A);
                }
            }

            UnsafeBitmap = new Bitmap(
                Width,
                Height,
                Width * Marshal.SizeOf<Pixel32>(),
                PixelFormat.Format32bppArgb, // REMOVE THE 'P' HERE
                BitsHandle.AddrOfPinnedObject()
            );
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectBitmap" /> class from a file path.
        ///     Loads an image from the specified file path and initializes the DirectBitmap instance.
        /// </summary>
        /// <param name="filePath">The file path to the image.</param>
        /// <exception cref="ArgumentException">Thrown if the file path is null or empty.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
        /// <exception cref="Exception">Thrown if the file could not be loaded as an image.</exception>
        public DirectBitmap(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException(ImagingResources.ErrorPath, nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(ImagingResources.ErrorFileNotFound, filePath);
            }

            try
            {
                using var image = Image.FromFile(filePath);
                Width = image.Width;
                Height = image.Height;
                Initiate();

                using var graphics = Graphics.FromImage(UnsafeBitmap);
                graphics.DrawImage(image, new Rectangle(0, 0, Width, Height), 0, 0, Width, Height, GraphicsUnit.Pixel);
            }
            catch (Exception ex)
            {
                throw new Exception(ImagingResources.ErrorWrongParameters, ex);
            }
        }

        /// <inheritdoc />
        public Pixel32[] Bits { get; private set; }

        /// <summary>
        ///     Gets the bitmap.
        ///     Be careful, we pass a reference that never gets copied, so any changes to this Bitmap will affect the DirectBitmap and vice versa.
        ///     GcHandle is pinned, so the memory address of the pixel data will not change, allowing for direct manipulation of the bitmap's pixels.
        ///     This memory is not managed by the .NET runtime, so it is crucial to ensure that it is properly released to avoid memory leaks. Always call Dispose() when done with the DirectBitmap to free the pinned handle and associated resources.
        /// </summary>
        /// <value>
        ///     The bitmap.
        /// </value>
        public Bitmap UnsafeBitmap { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="DirectBitmap" /> is disposed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if disposed; otherwise, <c>false</c>.
        /// </value>
        private bool Disposed { get; set; }

        /// <inheritdoc />
        public int Height { get; }

        /// <inheritdoc />
        public int Width { get; }

        /// <summary>
        /// Gets or sets the bits handle.
        /// </summary>
        /// <value>
        /// The bits handle.
        /// </value>
        private GCHandle BitsHandle { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Byte data for this instance.
        /// </summary>
        /// <returns>Image data as Bytes</returns>
        public byte[]? Bytes()
        {
            lock (_syncLock)
            {
                if (Bits == null)
                {
                    return null;
                }

                var byteArray = new byte[Bits.Length * 4];

                for (var i = 0; i < Bits.Length; i++)
                {
                    var color = Bits[i];

                    // Pack as RGBA
                    byteArray[(i * 4) + 0] = color.R;
                    byteArray[(i * 4) + 1] = color.G;
                    byteArray[(i * 4) + 2] = color.B;
                    byteArray[(i * 4) + 3] = color.A;
                }

                return byteArray;
            }
        }

        /// <summary>
        ///     Initiates this instance and sets all Helper Variables.
        /// </summary>
        private void Initiate(Color color = default)
        {
            // Allocate Pixel32 array
            Bits = new Pixel32[Width * Height];

            // Pin the array
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);

            // Create Bitmap from pinned Pixel32 array
            UnsafeBitmap = new Bitmap(
                Width,
                Height,
                Width * Marshal.SizeOf<Pixel32>(), // stride in bytes
                PixelFormat.Format32bppArgb,
                BitsHandle.AddrOfPinnedObject()
            );

            // Fill background
            var bg = new Pixel32(color.R, color.G, color.B, color.A);
            for (var i = 0; i < Bits.Length; i++)
                Bits[i] = bg;
        }

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <param name="btm">The custom Bitmap.</param>
        public static DirectBitmap GetInstance(Bitmap btm)
        {
            var dbm = new DirectBitmap(btm.Width, btm.Height);

            // Lock source bits
            var rect = new Rectangle(0, 0, btm.Width, btm.Height);
            var srcData = btm.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            // We can copy directly into our Bits array since it's pinned
            unsafe
            {
                fixed (Pixel32* pDest = dbm.Bits)
                {
                    Buffer.MemoryCopy(
                        (void*)srcData.Scan0,
                        (void*)pDest,
                        dbm.Bits.Length * 4,
                        dbm.Bits.Length * 4
                    );
                }
            }

            btm.UnlockBits(srcData);
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
            lock (_syncLock)
            {
                var colorPixel = new Pixel32(color.R, color.G, color.B, color.A); // Convert once

                for (var i = y; i < y + height && i < Height; i++)
                {
                    Bits[x + (i * Width)] = colorPixel;
                }
            }
        }

        /// <summary>
        ///     Draws a horizontal line with a specified color.
        ///     For now Microsoft's Rectangle Method is faster in certain circumstances
        /// </summary>
        /// <param name="x">The x Coordinate.</param>
        /// <param name="y">The y Coordinate.</param>
        /// <param name="length">The length.</param>
        /// <param name="color">The color.</param>
        public void DrawHorizontalLine(int x, int y, int length, Color color)
        {
            lock (_syncLock)
            {
                if (y < 0 || y >= Height || length <= 0)
                    return;

                var colorPixel = new Pixel32(color.R, color.G, color.B, color.A); // Convert once
                var endX = Math.Min(x + length, Width);

                var rowStart = y * Width;
                for (var i = x; i < endX; i++)
                {
                    Bits[rowStart + i] = colorPixel;
                }
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
            lock (_syncLock)
            {
                var colorPixel = new Pixel32(color.R, color.G, color.B, color.A);

                for (var y = y2; y < y2 + height && y < Height; y++)
                {
                    var rowStart = y * Width;

                    for (var x = x1; x < x1 + width && x < Width; x++)
                    {
                        Bits[rowStart + x] = colorPixel;
                    }
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
            lock (_syncLock)
            {
                var colorPixel = new Pixel32(color.R, color.G, color.B, color.A);
                var indices = idList as int[] ?? idList.ToArray();

                // Process indices in chunks to improve CPU caching, scalar only
                const int chunkSize = 1024;

                for (var start = 0; start < indices.Length; start += chunkSize)
                {
                    var length = Math.Min(chunkSize, indices.Length - start);

                    // Write pixels scalar but in tight loop for speed
                    for (var i = start; i < start + length; i++)
                    {
                        var idx = indices[i];
                        if (idx >= 0 && idx < Bits.Length)
                        {
                            Bits[idx] = colorPixel;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixel(int x, int y, Color color)
        {
            var px = new Pixel32(color.R, color.G, color.B, color.A);
            lock (_syncLock)
            {
                DirectBitmapCore.SetPixel(Bits, Width, Height, x, y, px);
            }
        }

        /// <inheritdoc />
        public void SetPixels(IEnumerable<(int x, int y, Color color)> pixels)
        {
            if (pixels == null) return;

            SetPixels(Convert(pixels));
        }

        /// <summary>
        /// Sets the pixels.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <param name="threshold">The threshold.</param>
        public void SetPixels(IEnumerable<PixelData> pixels, int threshold = 64)
        {
            lock (_syncLock)
            {
                DirectBitmapCore.SetPixelsAdaptive(Bits, Width, Height, pixels, threshold);
            }
        }

        /// <inheritdoc />
        public void BlendInt(uint[] src)
        {
            DirectBitmapCore.BlendInt(Bits, src);
        }

        /// <summary>
        ///     Draws the vertical lines.
        /// </summary>
        /// <param name="verticalLines">The vertical lines.</param>
        public void DrawVerticalLines(IEnumerable<(int x, int y, int finalY, Color color)> verticalLines)
        {
            _ = Parallel.ForEach(verticalLines, line =>
            {
                var (x, y, finalY, color) = line;
                var pixel = new Pixel32(color.R, color.G, color.B, color.A);

                // Calculate the number of rows in the vertical line
                var rowCount = finalY - y + 1;

                for (var i = 0; i < rowCount; i++)
                {
                    var index = x + (y + i) * Width;

                    // Write the Pixel32 directly
                    Bits[index] = pixel;
                }
            });
        }

        /// <summary>
        ///     Gets the pixel.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>Color of the Pixel</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Color GetPixel(int x, int y)
        {
            var p = DirectBitmapCore.GetPixel(Bits, Width, Height, x, y);
            return Color.FromArgb(p.A, p.R, p.G, p.B);
        }

        /// <inheritdoc />
        public Pixel32 GetPixel32(int x, int y)
        {
            var p = DirectBitmapCore.GetPixel(Bits, Width, Height, x, y);
            return new Pixel32(p.R, p.G, p.B, p.A);
        }

        /// <summary>
        ///     Gets the column of pixels at a given x-coordinate.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <returns>Array of Colors in the column.</returns>
        public Color[] GetColumn(int x)
        {
            var column = new Color[Height];

            for (var y = 0; y < Height; y++)
            {
                var index = x + (y * Width);
                var p = Bits[index];
                column[y] = Color.FromArgb(p.A, p.R, p.G, p.B);
            }

            return column;
        }

        /// <summary>
        ///     Gets the row of pixels at a given y-coordinate.
        /// </summary>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>Array of Colors in the row.</returns>
        public Color[] GetRow(int y)
        {
            var row = new Color[Width];

            for (var i = 0; i < Width; i++)
            {
                var p = Bits[y * Width + i];
                row[i] = Color.FromArgb(p.A, p.R, p.G, p.B);
            }

            return row;
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
                var px = Bits[i];
                span[i] = Color.FromArgb(px.A, px.R, px.G, px.B);
            }

            return span;
        }

        /// <summary>
        ///     Converts the Bits into bitmapImage.
        /// </summary>
        /// <returns>BitmapImage image data</returns>
        public BitmapImage ToBitmapImage()
        {
            return UnsafeBitmap.ToBitmapImage();
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
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(DirectBitmap? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Width != other.Width || Height != other.Height) return false;

            // Compare pixel buffer
            return Bits.AsSpan().SequenceEqual(other.Bits);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.
        /// </returns>
        public override bool Equals(object? obj) => Equals(obj as DirectBitmap);

        /// <summary>
        /// Converts the specified pixels.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <returns>Conveteed Coordinates into Pixel32</returns>
        private static IEnumerable<PixelData> Convert(IEnumerable<(int x, int y, Color color)> pixels)
        {
            foreach (var p in pixels)
                yield return new PixelData(p.x, p.y, p.color.R, p.color.G, p.color.B, p.color.A);
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
            if (Disposed) return;

            if (disposing)
            {
                // Managed resources (objects that implement IDisposable)
                UnsafeBitmap?.Dispose();
            }

            // Unmanaged resources/Handles (Free these always)
            if (BitsHandle.IsAllocated)
            {
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
