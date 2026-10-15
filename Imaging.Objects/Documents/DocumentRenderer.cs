/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        DocumentRenderer.cs
 * PURPOSE:     Flattens a document into one pixel buffer (save as PNG, run filters, clipboard).
 *              Shape layers are rasterized by a pluggable IShapeRasterizer, so this project needs no WPF.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Interfaces;

namespace Imaging.Objects.Documents
{
    /// <summary>
    ///     Renders a <see cref="Document" /> to pixels.
    /// </summary>
    public static class DocumentRenderer
    {
        /// <summary>
        ///     Composites all visible layers, bottom to top, onto a transparent buffer the size of the document.
        ///     The caller owns (and must dispose) the result.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="rasterizer">Needed only if a visible, non-empty shape layer exists.</param>
        /// <exception cref="InvalidOperationException">A shape layer needs rasterizing but no rasterizer was given.</exception>
        public static UnmanagedImageBuffer Flatten(Document document, IShapeRasterizer? rasterizer = null)
        {
            ArgumentNullException.ThrowIfNull(document);

            var result = new UnmanagedImageBuffer(document.Width, document.Height);

            try
            {
                result.Clear(0, 0, 0, 0);

                foreach (var layer in document.Layers)
                {
                    if (!layer.Visible) continue;

                    var opacity = (int)Math.Round(layer.Opacity * 255d);
                    if (opacity <= 0) continue;

                    switch (layer)
                    {
                        case RasterLayer raster:
                            AlphaBlender.Over(result, raster.Pixels, document.Bounds, opacity);
                            break;

                        case ShapeLayer shapes when shapes.Shapes.Count > 0:
                            if (rasterizer is null)
                                throw new InvalidOperationException(ImageResource.ErrorNoShapeRasterizer);

                            using (var scratch = new UnmanagedImageBuffer(document.Width, document.Height))
                            {
                                scratch.Clear(0, 0, 0, 0);
                                rasterizer.Rasterize(shapes.Shapes, scratch);
                                AlphaBlender.Over(result, scratch, document.Bounds, opacity);
                            }

                            break;
                    }
                }

                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }
    }
}