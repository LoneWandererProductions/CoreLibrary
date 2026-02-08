/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        DirectBitmap.cs
 * PURPOSE:     Custom BitmapImage Class, speeds up Set Pixel
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Imaging
{
    /// <inheritdoc />
    /// <summary>
    ///     Similar to DirectBitmap, generate a pixel image, should be slightly faster.
    ///     Supports fast SIMD-based pixel operations and unsafe pointer access.
    ///     This class must be used only from the WPF UI thread when updating the bitmap.
    /// </summary>
    public sealed class DirectBitmapImage : IDisposable
    {
        /// <summary>
        /// The bitmap
        /// </summary>
        private readonly WriteableBitmap _bitmap;

        /// <summary>
        /// The bits handle
        /// </summary>
        private GCHandle _bitsHandle;

        /// <summary>
        /// The cached image
        /// </summary>
        private BitmapImage? _cachedImage;

        /// <summary>
        /// The disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public ImageSource Source => _bitmap;

        /// <summary>
        ///     The synchronize lock
        /// </summary>
        private readonly Lock _syncLock = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectBitmapImage" /> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public DirectBitmapImage(int width, int height)
        {
            Width = width;
            Height = height;

            Bits = new Pixel32[width * height];
            _bitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        }

        /// <summary>
        /// Constructs a DirectBitmapImage from a pixel array and width.
        /// Assumes BGRA32 format and calculates height automatically.
        /// </summary>
        /// <param name="bits">The pixel array (BGRA32).</param>
        /// <param name="width">The width of the image.</param>
        public DirectBitmapImage(Pixel32[] bits, int width)
        {
            if (bits == null) throw new ArgumentNullException(nameof(bits));
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (bits.Length % width != 0) throw new ArgumentException("Pixel array length must be divisible by width.");

            _cachedImage = null;
            Width = width;
            Height = bits.Length / width;
            Bits = bits;
            _bitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            _bitmap = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgra32, null);
            UpdateBitmapFromBits();
        }

        /// <summary>
        ///     The height of the image.
        /// </summary>
        public int Height { get; }

        /// <summary>
        ///     The width of the image.
        /// </summary>
        public int Width { get; }

        /// <summary>
        ///     Gets the raw pixel buffer.
        /// </summary>
        public Pixel32[] Bits { get; }

        /// <summary>
        ///     Gets the cached converted BitmapImage.
        /// </summary>
        public BitmapImage BitmapImage
        {
            get
            {
                if (_cachedImage != null)
                {
                    return _cachedImage;
                }

                _cachedImage = ConvertImage();

                return _cachedImage;
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Frees memory and unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sets the pixel.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="color">The color.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixel(int x, int y, Color color)
        {
            var px = new Pixel32(color.R, color.G, color.B, color.A);
            lock (_syncLock)
            {
                DirectBitmapCore.SetPixel(Bits, Width, Height, x, y, px);
            }
        }

        /// <summary>
        ///     Fills the bitmap with a uniform color using SIMD.
        /// </summary>
        /// <param name="color">The color to fill with.</param>
        public unsafe void FillSimd(Color color)
        {
            var packed = (uint)(color.A << 24 | color.R << 16 | color.G << 8 | color.B);

            var span = MemoryMarshal.Cast<Pixel32, uint>(Bits.AsSpan());

            var len = span.Length;
            var vecSize = Vector<uint>.Count;
            var i = 0;

            var v = new Vector<uint>(packed);

            for (; i <= len - vecSize; i += vecSize)
                v.CopyTo(span.Slice(i, vecSize));

            for (; i < len; i++)
                span[i] = packed;

            UpdateBitmapFromBits();
        }

        /// <summary>
        /// Applies a 4x4 color matrix to all pixels.
        /// </summary>
        /// <param name="matrix">Color transformation matrix (4x4).</param>
        public void ApplyColorMatrix(float[][] matrix)
        {
            if (matrix == null || matrix.Length < 4)
                throw new ArgumentException("Matrix must be 4x4");

            var span = Bits.AsSpan();

            for (int i = 0; i < span.Length; i++)
            {
                var p = span[i];

                // preserve original alpha
                var a = p.A;

                // compute R,G,B using 4x4 matrix
                var r = (byte)Math.Clamp(p.R * matrix[0][0] + p.G * matrix[0][1] + p.B * matrix[0][2], 0, 255);
                var g = (byte)Math.Clamp(p.R * matrix[1][0] + p.G * matrix[1][1] + p.B * matrix[1][2], 0, 255);
                var b = (byte)Math.Clamp(p.R * matrix[2][0] + p.G * matrix[2][1] + p.B * matrix[2][2], 0, 255);

                span[i] = new Pixel32(r, g, b, a);
            }

            UpdateBitmapFromBits();
        }

        /// <summary>
        /// Sets pixels from PixelData collection efficiently.
        /// </summary>
        /// <param name="pixels">Collection of PixelData (x, y, RGBA).</param>
        public void SetPixels(IEnumerable<PixelData> pixels, int threshold = 64)
        {
            lock (_syncLock)
            {
                DirectBitmapCore.SetPixelsAdaptive(Bits, Width, Height, pixels, threshold);
            }

            UpdateBitmapFromBits();
        }

        /// <summary>
        /// Sets multiple pixels efficiently using SIMD or run-length optimization.
        /// </summary>
        /// <param name="pixels">The pixels to set, as (x, y, Color) tuples.</param>
        public void SetPixelsBulk(IEnumerable<(int x, int y, Color color)> pixels)
        {
            lock (_syncLock)
            {
                // Convert Colors to Pixel32
                var pixelData = pixels.Select(p => (p.x, p.y, new Pixel32(p.color.R, p.color.G, p.color.B, p.color.A)));

                // Use the shared SIMD-optimized core method
                DirectBitmapCore.SetPixelsSimd(Bits, Width, Height, pixelData);
            }

            UpdateBitmapFromBits();
        }

        /// <summary>
        /// Alpha blends another pixel buffer onto this image using SIMD.
        /// Format: BGRA (32-bit uint). Alpha is premultiplied at runtime.
        /// </summary>
        /// <param name="src">Source pixels to blend (same size as current bitmap)</param>
        /// <exception cref="System.ArgumentException">Source must match image size</exception>
        public void BlendInt(uint[] src)
        {
            DirectBitmapCore.BlendInt(Bits, src);
            UpdateBitmapFromBits();
        }

        /// <summary>
        ///     Updates the WPF bitmap from the Bits buffer.
        /// </summary>
        public void UpdateBitmapFromBits()
        {
            _bitmap.Lock();
            unsafe
            {
                var dst = (Pixel32*)_bitmap.BackBuffer; // treat back buffer as Pixel32[]
                var span = Bits.AsSpan();               // Pixel32[]

                for (int i = 0; i < span.Length; i++)
                {
                    dst[i] = span[i];                   // direct struct copy
                }
            }

            _bitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
            _bitmap.Unlock();
        }

        /// <summary>
        /// Converts the image.
        /// </summary>
        /// <returns>A BitmapImage.</returns>
        private BitmapImage? ConvertImage()
        {
            if (_cachedImage != null) return _cachedImage;

            // 1. Ensure the WriteableBitmap is ready
            // (Assuming _bitmap is your WriteableBitmap instance)

            using (var stream = new MemoryStream())
            {
                // 2. Use PngBitmapEncoder to preserve the alpha channel accurately
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(_bitmap));
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                var bi = new BitmapImage();
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;

                // 3. THIS IS THE KEY: Tell WPF not to "optimize" your colors
                bi.CreateOptions = BitmapCreateOptions.PreservePixelFormat;

                bi.StreamSource = stream;
                bi.EndInit();

                // 4. Freeze it so it's thread-safe and performs better in UI
                bi.Freeze();

                _cachedImage = bi;
            }

            return _cachedImage;
        }

        /// <summary>
        ///     Releases unmanaged and managed resources.
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing && _bitsHandle.IsAllocated)
            {
                _cachedImage = null;
                if (_bitsHandle.IsAllocated)
                    _bitsHandle.Free();
            }

            _disposed = true;
        }

        /// <summary>
        ///     Finalizer.
        /// </summary>
        ~DirectBitmapImage()
        {
            Dispose(false);
        }
    }
}
