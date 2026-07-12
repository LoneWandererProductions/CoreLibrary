/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Texture
 * FILE:        RawTextureBuffer.cs
 * PURPOSE:     Texture buffer for raw pixel data, used for texture generation and manipulation.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Texture
{
    /// <summary>
    /// Texture buffer for raw pixel data, used for texture generation and manipulation.
    /// </summary>
    public sealed class RawTextureBuffer
    {
        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width { get; }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int Height { get; }

        /// <summary>
        /// Gets the pixel data.
        /// Raw row-major packed bytes
        /// </summary>
        /// <value>
        /// The pixel data.
        /// </value>
        public byte[] PixelData { get; }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length => PixelData.Length;

        /// <summary>
        /// Convert image data into a span.
        /// </summary>
        /// <returns>Return a span of the pixel data.</returns>
        public Span<byte> AsSpan() => PixelData;

        /// <summary>
        /// Initializes a new instance of the <see cref="RawTextureBuffer"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <exception cref="System.ArgumentException">Dimensions must be greater than zero.</exception>
        public RawTextureBuffer(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Dimensions must be greater than zero.");

            Width = width;
            Height = height;
            PixelData = new byte[width * height * 4]; // 4 bytes per pixel (BGRA)
        }
    }
}