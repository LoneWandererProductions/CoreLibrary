/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Texture
 * FILE:        TextureBufferExtensions.cs
 * PURPOSE:     Simple extension methods to bridge raw texture buffer data to various managed and unmanaged memory structures.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Imaging.Texture
{
    /// <summary>
    /// Image Conversion Extensions for bridging raw texture buffer data to various managed and unmanaged memory structures.
    /// </summary>
    public static class TextureBufferExtensions
    {
        /// <summary>
        /// Bridges raw generated data directly to the OpenGL-bound layout memory buffer.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="unmanagedImageBuffer">The unmanaged image buffer.</param>
        public static void BlitToUnmanagedBuffer(this RawTextureBuffer source, dynamic unmanagedImageBuffer)
        {
            // Leverages duck-typing or direct structural casting via Span data references
            Span<byte> targetSpan = unmanagedImageBuffer.BufferSpan;
            source.AsSpan().CopyTo(targetSpan);
        }

        /// <summary>
        /// Bridges data to your pinned high-speed intermediate DirectBitmap configuration structures.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="directBitmap">The direct bitmap.</param>
        /// <exception cref="System.ArgumentException">Dimensions between structural buffers must match perfectly.</exception>
        public static unsafe void BlitToDirectBitmap(this RawTextureBuffer source, dynamic directBitmap)
        {
            if (source.Width != directBitmap.Width || source.Height != directBitmap.Height)
                throw new ArgumentException("Dimensions between structural buffers must match perfectly.");

            // Directly stage memory elements into the pinned structural field handle
            fixed (byte* pSource = source.PixelData)
            {
                // Access pinned array destination through Address handle pointer
                IntPtr pDestHandle = directBitmap.BitsHandle.AddrOfPinnedObject();
                Buffer.MemoryCopy(pSource, (void*)pDestHandle, source.Length, source.Length);
            }
        }
    }
}
