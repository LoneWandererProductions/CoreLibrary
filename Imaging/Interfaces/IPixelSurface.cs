/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Interfaces
 * FILE:        IPixelSurface.cs
 * PURPOSE:     Simple interface for a pixel surface, which can be used for rendering and manipulation of pixel data.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;

namespace Imaging.Interfaces
{
    /// <summary>
    /// Pixel manipulation surface interface, which can be used for rendering and manipulation of pixel data.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IPixelSurface : IDisposable
    {
        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        int Width { get; }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        int Height { get; }

        /// <summary>
        /// Gets the bits.
        /// </summary>
        /// <value>
        /// The bits.
        /// </value>
        Pixel32[] Bits { get; }

        /// <summary>
        /// Sets the pixel.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="color">The color.</param>
        void SetPixel(int x, int y, System.Drawing.Color color);

        /// <summary>
        /// Sets the pixels.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        void SetPixels(IEnumerable<(int x, int y, System.Drawing.Color color)> pixels);

        /// <summary>
        /// Blends the int.
        /// </summary>
        /// <param name="src">The source.</param>
        void BlendInt(uint[] src);

        /// <summary>
        /// Gets the pixel32.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        Pixel32 GetPixel32(int x, int y);
    }
}
