/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        UnmanagedImageBuffer.cs
 * PURPOSE:     A way to store images in a fast way.
 *              It aims to bridge System.Drawing and OpenGL in a clean and fast way.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace RenderEngine
{
    /// <inheritdoc />
    /// <summary>
    /// Represents an unmanaged memory buffer for storing image pixel data with direct memory access,
    /// optimized for fast pixel manipulation and bulk operations using SIMD acceleration where available.
    /// Useful for fast pixel manipulation without GC overhead.
    /// This buffer uses a fixed pixel format (RGBA8888 → 4 bytes per pixel).
    /// </summary>
    /// <remarks>
    /// This class allocates unmanaged memory of size Width * Height * BytesPerPixel to store image data in BGRA format by default.
    /// It supports setting individual pixels, clearing the buffer to a uniform color,
    /// applying multiple pixel changes at once, and replacing the entire buffer efficiently.
    /// </remarks>
    public unsafe class UnmanagedImageBuffer : IDisposable
    {
        /// <summary>
        /// Image width in pixels.
        /// </summary>
        public readonly int Width;

        /// <summary>
        /// Image height in pixels.
        /// </summary>
        public readonly int Height;

        /// <summary>
        /// Number of bytes per pixel.
        /// Fixed RGBA8888 format: 4 channels × 1 byte each.
        /// </summary>
        public const int BytesPerPixel = 4;

        /// <summary>
        /// Pointer to the unmanaged memory buffer.
        /// Holds raw pixel data in row-major order (top-left → bottom-right).
        /// Each pixel is stored as BGRA (little endian: 0xAABBGGRR).
        /// </summary>
        private IntPtr _buffer;

        /// <summary>
        /// Returns a span over the raw buffer.
        /// Useful for fast iteration over all pixels.
        /// </summary>
        public Span<byte> BufferSpan => new((void*)_buffer, Width * Height * BytesPerPixel);

        /// <summary>
        /// Constructs a new unmanaged image buffer.
        /// Allocates enough memory for Width × Height × BytesPerPixel.
        /// </summary>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        public UnmanagedImageBuffer(int width, int height)
        {
            Width = width;
            Height = height;

            // Allocate unmanaged memory (Width × Height × 4 bytes)
            _buffer = Marshal.AllocHGlobal(width * height * BytesPerPixel);
        }

        /// <summary>
        /// Writes a pixel at (x,y) directly into the buffer.
        /// No bounds checks for performance (caller must validate coordinates).
        /// Stored in BGRA order.
        /// </summary>
        public void SetPixel(int x, int y, byte r, byte g, byte b, byte a = 255)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;

            var offset = (y * Width + x) * BytesPerPixel;
            var ptr = (byte*)_buffer.ToPointer() + offset;

            ptr[0] = b; // Blue
            ptr[1] = g; // Green
            ptr[2] = r; // Red
            ptr[3] = a; // Alpha
        }

        /// <summary>
        ///     Sets the pixel color at the specified coordinates using a System.Drawing.Color.
        /// </summary>
        /// <param name="x">X coordinate (0-based)</param>
        /// <param name="y">Y coordinate (0-based)</param>
        /// <param name="color">The color to set</param>
        public void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.A, color.R, color.G, color.B);
        }

        /// <summary>
        /// Reads a pixel at (x,y) directly from the buffer.
        /// No bounds checks for performance.
        /// Returns (r, g, b, a) tuple.
        /// </summary>
        public (byte r, byte g, byte b, byte a) GetPixel(int x, int y)
        {
            var offset = (y * Width + x) * BytesPerPixel;
            var ptr = (byte*)_buffer.ToPointer() + offset;
            return (ptr[2], ptr[1], ptr[0], ptr[3]); // BGRA → return RGBA
        }

        /// <summary>
        /// Applies multiple pixel changes to the buffer in-place, given a span of coordinate-color tuples.
        /// Each tuple contains the x and y pixel coordinates and a packed 32-bit BGRA color.
        /// Pixels outside the valid image bounds are ignored.
        /// </summary>
        /// <param name="changes">A read-only span of pixel changes, each specified as (x, y, BGRA color).</param>
        public void ApplyChanges(ReadOnlySpan<(int x, int y, uint bgra)> changes)
        {
            var buffer = BufferSpan;

            foreach (var (x, y, bgra) in changes)
            {
                // Skip out-of-bounds pixels efficiently
                if ((uint)x >= (uint)Width || (uint)y >= (uint)Height) continue;

                var offset = (y * Width + x) * BytesPerPixel;

                // Unpack BGRA uint into bytes
                buffer[offset + 0] = (byte)(bgra & 0xFF);         // B
                buffer[offset + 1] = (byte)((bgra >> 8) & 0xFF);  // G
                buffer[offset + 2] = (byte)((bgra >> 16) & 0xFF); // R
                buffer[offset + 3] = (byte)((bgra >> 24) & 0xFF); // A
            }
        }

        /// <summary>
        /// Replaces the entire unmanaged buffer with a new byte span.
        /// The input buffer must match the internal buffer size exactly.
        /// Uses hardware-accelerated AVX2 instructions for bulk copy if supported.
        /// </summary>
        /// <param name="fullBuffer">The source byte span representing the full image buffer to copy.</param>
        /// <exception cref="ArgumentException">Thrown if the input buffer length does not match the internal buffer size.</exception>
        public void ReplaceBuffer(ReadOnlySpan<byte> fullBuffer)
        {
            var bufferSize = Width * Height * BytesPerPixel;
            if (fullBuffer.Length != bufferSize) throw new ArgumentException(RenderResource.ErrorInputBuffer);

            var buffer = BufferSpan;

            if (Avx2.IsSupported)
            {
                const int vectorSize = 32; // 256-bit vector
                var vectorCount = bufferSize / vectorSize;
                var remainder = bufferSize % vectorSize;

                fixed (byte* srcPtr = fullBuffer)
                {
                    var dstPtr = (byte*)_buffer.ToPointer();

                    for (var i = 0; i < vectorCount; i++)
                    {
                        var vec = Avx.LoadVector256(srcPtr + i * vectorSize);
                        Avx.Store(dstPtr + i * vectorSize, vec);
                    }

                    // Copy remaining bytes
                    for (var i = bufferSize - remainder; i < bufferSize; i++)
                        buffer[i] = fullBuffer[i];
                }
            }
            else
            {
                // Fallback to managed copy
                fullBuffer.CopyTo(buffer);
            }
        }

        /// <summary>
        /// Retrieves a span representing a horizontal sequence of pixels starting at (x, y).
        /// The span length is equal to count pixels, each containing bytes per pixel.
        /// </summary>
        /// <param name="x">The starting horizontal pixel coordinate (0-based).</param>
        /// <param name="y">The vertical pixel coordinate (0-based).</param>
        /// <param name="count">The number of consecutive pixels to retrieve.</param>
        /// <returns>A span of bytes representing the requested pixels.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the requested pixel range is out of bounds of the image dimensions.
        /// </exception>
        public Span<byte> GetPixelSpan(int x, int y, int count)
        {
            if ((uint)x >= (uint)Width || (uint)y >= (uint)Height || x + count > Width)
                throw new ArgumentOutOfRangeException();

            var offset = (y * Width + x) * BytesPerPixel;
            return BufferSpan.Slice(offset, count * BytesPerPixel);
        }

        /// <summary>
        /// Alpha-blends a pixel at (x, y) with the given color and alpha.
        /// Combines source color with destination pixel in the buffer.
        /// </summary>
        public void SetPixelAlphaBlend(int x, int y, byte a, byte r, byte g, byte b)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;

            var offset = (y * Width + x) * BytesPerPixel;

            // Read existing pixel
            var destB = BufferSpan[offset + 0];
            var destG = BufferSpan[offset + 1];
            var destR = BufferSpan[offset + 2];
            var destA = BufferSpan[offset + 3];

            var alpha = a / 255f;
            var invAlpha = 1f - alpha;

            // Blend each channel
            BufferSpan[offset + 0] = (byte)(b * alpha + destB * invAlpha);
            BufferSpan[offset + 1] = (byte)(g * alpha + destG * invAlpha);
            BufferSpan[offset + 2] = (byte)(r * alpha + destR * invAlpha);
            BufferSpan[offset + 3] = (byte)(a * alpha + destA * invAlpha); // optional: blend alpha
        }

        /// <summary>
        ///     Blits a rectangular region from the source buffer to this buffer.
        /// </summary>
        /// <param name="src">The source.</param>
        /// <param name="srcX">X-coordinate of the top-left corner in the source.</param>
        /// <param name="srcY">Y-coordinate of the top-left corner in the source.</param>
        /// <param name="width">Width of the region to copy.</param>
        /// <param name="height">Height of the region to copy.</param>
        /// <param name="destX">The dest x.</param>
        /// <param name="destY">The dest y.</param>
        public void BlitRegion(UnmanagedImageBuffer src, int srcX, int srcY, int width, int height, int destX, int destY)
        {
            for (var y = 0; y < height; y++)
            {
                var srcRow = src.GetPixelSpan(srcX, srcY + y, width);
                var dstRow = GetPixelSpan(destX, destY + y, width);
                srcRow.CopyTo(dstRow);
            }
        }

        /// <summary>
        /// Returns the raw pointer to the unmanaged buffer.
        /// Use with caution — pointer arithmetic required.
        /// </summary>
        public IntPtr Buffer => _buffer;

        /// <summary>
        /// Releases unmanaged memory allocated for the buffer.
        /// Safe to call multiple times.
        /// </summary>
        public void Dispose()
        {
            if (_buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_buffer);
                _buffer = IntPtr.Zero;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer (safety net).
        /// Ensures unmanaged memory is released if Dispose() wasn’t called.
        /// </summary>
        ~UnmanagedImageBuffer() => Dispose();

        /// <summary>
        /// Clears the entire buffer to the given color.
        /// Fast implementation using Span iteration.
        /// </summary>
        public void Clear(System.Drawing.Color color)
        {
            var span = BufferSpan;
            for (var i = 0; i < span.Length; i += BytesPerPixel)
            {
                span[i + 0] = color.B;
                span[i + 1] = color.G;
                span[i + 2] = color.R;
                span[i + 3] = color.A;
            }
        }

        /// <summary>
        /// Packs bytes into a single uint in BGRA order (little endian).
        /// </summary>
        public static uint PackBgra(byte a, byte r, byte g, byte b)
        {
            return ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | b;
        }

        /// <summary>
        /// Converts the unmanaged buffer into a managed <see cref="Bitmap"/>.
        /// </summary>
        public Bitmap ToBitmap()
        {
            var bmp = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Lock bitmap data for fast access
            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, Width, Height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                bmp.PixelFormat);

            unsafe
            {
                var dstPtr = (byte*)bmpData.Scan0;
                var dstSpan = new Span<byte>(dstPtr, Width * Height * BytesPerPixel);
                BufferSpan.CopyTo(dstSpan);
            }

            bmp.UnlockBits(bmpData);
            return bmp;
        }
    }
}
