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
using System.Numerics;
using System.Runtime.InteropServices;
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

            int height = bits.Length / width;
            Bits = bits;
            _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
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
        ///     Sets pixels from a collection of <see cref="PixelData" />.
        ///     Uses unsafe pointer arithmetic for speed.
        /// </summary>
        /// <param name="pixels">The pixels to set.</param>
        public void SetPixelsUnsafe(IEnumerable<PixelData> pixels)
        {
            _bitmap.Lock();
            unsafe
            {
                var buffer = (byte*)_bitmap.BackBuffer.ToPointer();
                var stride = _bitmap.BackBufferStride;

                foreach (var pixel in pixels)
                {
                    if (pixel.X < 0 || pixel.X >= Width || pixel.Y < 0 || pixel.Y >= Height)
                        continue;

                    var offset = pixel.Y * stride + pixel.X * 4;
                    buffer[offset + 0] = pixel.B;
                    buffer[offset + 1] = pixel.G;
                    buffer[offset + 2] = pixel.R;
                    buffer[offset + 3] = pixel.A;

                    Bits[pixel.Y * Width + pixel.X] =
                        new Pixel32(pixel.R, pixel.G, pixel.B, pixel.A);
                }
            }

            _bitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
            _bitmap.Unlock();
        }

        /// <summary>
        ///     Fills the bitmap with a uniform color using SIMD.
        /// </summary>
        /// <param name="color">The color to fill with.</param>
        public unsafe void FillSimd(System.Drawing.Color color)
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
        ///     Sets individual pixels in the image using a collection of <see cref="PixelData" />.
        ///     Each entry defines the X/Y position and RGBA components.
        /// </summary>
        /// <param name="pixels">A collection of <see cref="PixelData" /> describing the pixels to set.</param>
        public void SetPixels(IEnumerable<PixelData> pixels)
        {
            foreach (var pixel in pixels)
            {
                if (pixel.X < 0 || pixel.X >= Width || pixel.Y < 0 || pixel.Y >= Height)
                {
                    // Throwing a detailed exception makes debugging much easier
                    throw new ArgumentOutOfRangeException(
                        nameof(pixels),
                        $"Pixel coordinate ({pixel.X}, {pixel.Y}) is outside the image bounds of {Width}x{Height}.");
                }

                var index = pixel.Y * Width + pixel.X;
                Bits[index] = new Pixel32(pixel.R, pixel.G, pixel.B, pixel.A);
            }

            UpdateBitmapFromBits();
        }

        /// <summary>
        /// Alpha blends another pixel buffer onto this image using SIMD.
        /// Format: BGRA (32-bit uint). Alpha is premultiplied at runtime.
        /// </summary>
        /// <param name="src">Source pixels to blend (same size as current bitmap)</param>
        /// <exception cref="System.ArgumentException">Source must match image size</exception>
        public unsafe void BlendInt(uint[] src)
        {
            if (src == null || src.Length != Bits.Length)
                throw new ArgumentException("Source must match image size");

            // Get spans for safe bounds check elimination (optional but good practice)
            // Casting Pixel32[] to uint[] for easier bitwise ops
            var dstSpan = System.Runtime.InteropServices.MemoryMarshal.Cast<Pixel32, uint>(Bits.AsSpan());
            var srcSpan = src.AsSpan();

            int len = dstSpan.Length;

            // Use unsafe pointers for maximum speed in the loop
            fixed (uint* pDst = dstSpan)
            fixed (uint* pSrc = srcSpan)
            {
                uint* dPtr = pDst;
                uint* sPtr = pSrc;

                for (int i = 0; i < len; i++)
                {
                    uint s = *sPtr;

                    // Fast check: if source is fully transparent, skip
                    uint sa = s >> 24;
                    if (sa == 0)
                    {
                        dPtr++;
                        sPtr++;
                        continue;
                    }

                    // Fast check: if source is fully opaque, just overwrite
                    if (sa == 255)
                    {
                        *dPtr = s;
                        dPtr++;
                        sPtr++;
                        continue;
                    }

                    uint d = *dPtr;

                    // Extract components (0xAARRGGBB format)
                    // Destination
                    uint da = d >> 24;
                    uint dr = (d >> 16) & 0xFF;
                    uint dg = (d >> 8) & 0xFF;
                    uint db = d & 0xFF;

                    // Source
                    uint sr = (s >> 16) & 0xFF;
                    uint sg = (s >> 8) & 0xFF;
                    uint sb = s & 0xFF;

                    // Standard Alpha Blending Formula: Out = (Src * Alpha + Dst * (255 - Alpha)) / 255
                    // We approximate division by 255 using: (x + 128) / 255  ~=  (x * 257 + 128) >> 16
                    // Or simpler fast approximation: (v + (v >> 8)) >> 8

                    uint invA = 255 - sa;

                    // Blend Color Channels
                    uint r = (sr * sa + dr * invA) / 255;
                    uint g = (sg * sa + dg * invA) / 255;
                    uint b = (sb * sa + db * invA) / 255;

                    // Blend Alpha Channel (Standard "Over" operator)
                    // ResultAlpha = SrcAlpha + DstAlpha * (1 - SrcAlpha)
                    uint a = sa + ((da * invA) / 255);

                    // Re-pack and write
                    *dPtr = (a << 24) | (r << 16) | (g << 8) | b;

                    dPtr++;
                    sPtr++;
                }
            }

            UpdateBitmapFromBits();
        }

        /// <summary>
        ///     SIMD-based batch pixel update from (x,y,color) triplets.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        public void SetPixelsBulk(IEnumerable<(int x, int y, Color color)> pixels)
        {
            foreach (var (x, y, c) in pixels)
            {
                if ((uint)x >= Width || (uint)y >= Height)
                    continue;

                var index = x + y * Width;
                Bits[index] = new Pixel32(c.R, c.G, c.B, c.A);
            }

            UpdateBitmapFromBits();
        }

        /// <summary>
        ///     Converts the internal bitmap to a <see cref="BitmapImage" />.
        /// </summary>
        private BitmapImage ConvertImage()
        {
            var tempBitmap = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgra32, null);
            var byteArray = new byte[Bits.Length * sizeof(uint)];

            Buffer.BlockCopy(Bits, 0, byteArray, 0, byteArray.Length);

            tempBitmap.Lock();
            Marshal.Copy(byteArray, 0, tempBitmap.BackBuffer, byteArray.Length);
            tempBitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
            tempBitmap.Unlock();

            var bitmapImage = new BitmapImage();
            using var stream = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(tempBitmap));
            encoder.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);

            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
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
