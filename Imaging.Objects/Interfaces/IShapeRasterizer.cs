/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Interfaces;
 * FILE:        IShapeRasterizer.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Shapes;

namespace Imaging.Objects.Interfaces;

/// <summary>
///     Turns shapes into pixels. Implemented next to the WPF/GDI code (for example with a DrawingVisual and
///     RenderTargetBitmap); the document model only knows this interface.
/// </summary>
public interface IShapeRasterizer
{
    /// <summary>
    ///     Draws <paramref name="shapes" /> (bottom to top) into <paramref name="target" />.
    ///     The target is fully transparent, straight-alpha BGRA and as large as the document.
    /// </summary>
    void Rasterize(IReadOnlyList<Shape> shapes, UnmanagedImageBuffer target);
}