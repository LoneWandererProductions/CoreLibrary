/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        Shapes.cs
 * PURPOSE:     Geometry as pure, immutable data. A shape layer stores these records; renderers turn them
 *              into pixels (or WPF visuals) only when needed.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents
{
    /// <summary>
    ///     Outline of a shape. <see cref="Argb" /> is straight (non-premultiplied) 0xAARRGGBB.
    /// </summary>
    public sealed record StrokeSpec(uint Argb, double Width)
    {
        /// <summary>Gets the default stroke: opaque black, one pixel.</summary>
        public static StrokeSpec Default { get; } = new(0xFF000000, 1.0);
    }
}
