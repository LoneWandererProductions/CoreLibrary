/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        DirectBitmap.cs
 * PURPOSE:     Custom BitmapImage Class, speeds up Set Pixel
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Imaging;

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

        Bits = new uint[width * height];
        _bitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
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
    public uint[] Bits { get; }

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
                    (uint)(pixel.A << 24 | pixel.R << 16 | pixel.G << 8 | pixel.B);
            }
        }

        _bitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
        _bitmap.Unlock();
    }

    /// <summary>
    ///     Fills the bitmap with a uniform color using SIMD.
    /// </summary>
    /// <param name="color">The color to fill with.</param>
    public unsafe void FillSimd(Color color)
    {
        var packed = (uint)(color.A << 24 | color.R << 16 | color.G << 8 | color.B);
        _bitmap.Lock();

        var ptr = (uint*)_bitmap.BackBuffer.ToPointer();
        var len = Bits.Length;
        var vecSize = Vector<uint>.Count;
        var i = 0;

        // SIMD bulk fill
        var v = new Vector<uint>(packed);
        for (; i <= len - vecSize; i += vecSize)
            v.CopyTo(Bits, i);

        // Write tail (scalar)
        for (; i < len; i++)
            Bits[i] = packed;

        // Copy to WPF Bitmap in one block, fast
        Buffer.MemoryCopy(
            source: (void*)_bitsHandle.AddrOfPinnedObject(),
            destination: (void*)_bitmap.BackBuffer,
            destinationSizeInBytes: len * sizeof(uint),
            sourceBytesToCopy: len * sizeof(uint)
        );

        _bitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
        _bitmap.Unlock();
    }

    /// <summary>
    ///     Applies a 4x4 color matrix to all pixels.
    /// </summary>
    /// <param name="matrix">Color transformation matrix.</param>
    public void ApplyColorMatrix(float[][] matrix)
    {
        // Same validation as before (optional but safe)
        if (matrix == null || matrix.Length < 4 ||
            matrix[0].Length < 4 || matrix[1].Length < 4 ||
            matrix[2].Length < 4 || matrix[3].Length < 4)
            throw new ArgumentException("Matrix must be 4x4");

        // Cached locals for faster access (matrix[][] is slow)
        float m00 = matrix[0][0], m01 = matrix[0][1], m02 = matrix[0][2], m03 = matrix[0][3];
        float m10 = matrix[1][0], m11 = matrix[1][1], m12 = matrix[1][2], m13 = matrix[1][3];
        float m20 = matrix[2][0], m21 = matrix[2][1], m22 = matrix[2][2], m23 = matrix[2][3];
        float m30 = matrix[3][0], m31 = matrix[3][1], m32 = matrix[3][2], m33 = matrix[3][3];

        // We reuse output array to avoid allocations when repeating effects
        var pixels = new (int x, int y, Color color)[Bits.Length];

        for (var i = 0; i < Bits.Length; i++)
        {
            var argb = Bits[i];

            // Manual unpack (fastest + identical)
            float r = (byte)(argb >> 16);
            float g = (byte)(argb >> 8);
            float b = (byte)argb;
            float a = (byte)(argb >> 24);

            // Ordered multiply like WinForms (NOT SIMD parallel)
            var rr = r * m00 + g * m01 + b * m02 + a * m03;
            var gg = r * m10 + g * m11 + b * m12 + a * m13;
            var bb = r * m20 + g * m21 + b * m22 + a * m23;
            var aa = r * m30 + g * m31 + b * m32 + a * m33;

            // Same exact truncation as WinForms (NOT Math.Round)
            var R = (byte)(rr < 0f ? 0f : (rr > 255f ? 255f : rr));
            var G = (byte)(gg < 0f ? 0f : (gg > 255f ? 255f : gg));
            var B = (byte)(bb < 0f ? 0f : (bb > 255f ? 255f : bb));
            var A = (byte)(aa < 0f ? 0f : (aa > 255f ? 255f : aa));

            // Save (no packing yet)
            pixels[i] = (i % Width, i / Width, Color.FromArgb(A, R, G, B));
        }

        SetPixelsSimd(pixels); // Your fast final batch copy
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
                continue;
            }

            var index = pixel.Y * Width + pixel.X;
            Bits[index] = (uint)(pixel.A << 24 | pixel.R << 16 | pixel.G << 8 | pixel.B);
        }

        UpdateBitmapFromBits();
    }

    /// <summary>
    /// Alpha blends another pixel buffer onto this image using SIMD.
    /// Format: BGRA (32-bit uint). Alpha is premultiplied at runtime.
    /// </summary>
    /// <param name="src">Source pixels to blend (same size as current bitmap)</param>
    /// <exception cref="System.ArgumentException">Source must match image size</exception>
    public void BlendSimd(uint[] src)
    {
        if (src == null || src.Length != Bits.Length)
            throw new ArgumentException("Source must match image size");

        var len = Bits.Length;

        for (var i = 0; i < len; i++)
        {
            var s = src[i];
            if ((s >> 24) == 0)
                continue; // fully transparent, skip completely

            var d = Bits[i];

            // Load BGRA from packed uint into float components
            var sf = new Vector4(
                (s >> 16) & 255, // R
                (s >> 8) & 255, // G
                (s) & 255, // B
                (s >> 24) & 255 // A
            );

            var df = new Vector4(
                (d >> 16) & 255,
                (d >> 8) & 255,
                (d) & 255,
                (d >> 24) & 255
            );

            var a = sf.W * (1f / 255f);
            if (a <= 0.0001f)
                continue; // ignore near-transparent pixels

            // SIMD blend: Dst = Src * a + Dst * (1−a)
            var r = sf * a + df * (1f - a);

            // Clamp and repack BGRA
            Bits[i] =
                ((uint)Math.Clamp(r.W, 0, 255) << 24) |
                ((uint)Math.Clamp(r.X, 0, 255) << 16) |
                ((uint)Math.Clamp(r.Y, 0, 255) << 8) |
                (uint)Math.Clamp(r.Z, 0, 255);
        }

        UpdateBitmapFromBits();
    }

    /// <summary>
    ///     SIMD-based batch pixel update from (x,y,color) triplets.
    /// </summary>
    /// <param name="pixels">The pixels.</param>
    public unsafe void SetPixelsSimd(IEnumerable<(int x, int y, Color color)> pixels)
    {
        _bitmap?.Lock();
        var ptr = (uint*)_bitmap.BackBuffer.ToPointer();

        foreach (var (x, y, c) in pixels)
        {
            if ((uint)x >= Width || (uint)y >= Height) continue;
            var packed = (uint)(c.A << 24 | c.R << 16 | c.G << 8 | c.B);
            var index = x + y * Width;
            ptr[index] = packed;
            Bits[index] = packed;
        }

        _bitmap.AddDirtyRect(new Int32Rect(0, 0, Width, Height));
        _bitmap.Unlock();
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
            var src = (void*)_bitsHandle.AddrOfPinnedObject();
            var dst = (void*)_bitmap.BackBuffer;
            Buffer.MemoryCopy(src, dst, Bits.Length * sizeof(uint), Bits.Length * sizeof(uint));
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