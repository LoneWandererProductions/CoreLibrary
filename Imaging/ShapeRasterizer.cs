/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        ShapeRasterizer.cs
 * PURPOSE:     Turns Imaging.Objects.Shapes shapes into pixels for DocumentRenderer.Flatten.
 *              Solid fills and strokes are drawn directly with GDI+. Texture and filter fills reuse the
 *              same TextureGenerator/ImageRender area-fill code the single-image Fill/Texture/Filter
 *              tools already use, so a shape with a texture fill looks exactly like the equivalent
 *              SelectionFrame-based fill did, just kept as editable data instead of being baked in.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Imaging.Enums;
using Imaging.Interfaces;
using Imaging.Objects;
using Imaging.Objects.Documents;
using Imaging.Objects.Interfaces;
using Imaging.Objects.Shapes;

namespace Imaging
{
    /// <inheritdoc />
    public sealed class ShapeRasterizer : IShapeRasterizer
    {
        /// <summary>
        /// The render
        /// </summary>
        private readonly IImageRender _render;

        /// <summary>
        /// The texture generator
        /// </summary>
        private readonly ITextureGenerator _textureGenerator;

        /// <summary>
        /// Creates a rasterizer with its own <see cref="ImageRender" /> and <see cref="TextureGenerator" />.
        /// </summary>
        public ShapeRasterizer()
            : this(new ImageRender(), new TextureGenerator())
        {
        }

        /// <summary>
        /// Creates a rasterizer that reuses the given engines (handy for tests or to share caches).
        /// </summary>
        /// <param name="render">The render.</param>
        /// <param name="textureGenerator">The texture generator.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public ShapeRasterizer(IImageRender render, ITextureGenerator textureGenerator)
        {
            ArgumentNullException.ThrowIfNull(render);
            ArgumentNullException.ThrowIfNull(textureGenerator);

            _render = render;
            _textureGenerator = textureGenerator;
        }

        /// <inheritdoc />
        public void Rasterize(IReadOnlyList<Shape> shapes, UnmanagedImageBuffer target)
        {
            ArgumentNullException.ThrowIfNull(shapes);
            ArgumentNullException.ThrowIfNull(target);

            if (shapes.Count == 0) return;

            // GDI+ needs a System.Drawing.Bitmap. This one is a *view* over the target's own memory - see
            // UnmanagedImageBuffer.Buffer - so every draw call below lands directly in the buffer, no copy back.
            using var bitmap = new Bitmap(target.Width, target.Height,
                target.Width * UnmanagedImageBuffer.BytesPerPixel,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb, target.Buffer);

            foreach (var shape in shapes)
            {
                RasterizeOne(bitmap, shape);
            }
        }

        /// <summary>
        /// Rasterizes the one.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="shape">The shape.</param>
        private void RasterizeOne(Bitmap bitmap, Shape shape)
        {
            switch (shape)
            {
                case LineShape line:
                    DrawStroke(bitmap, shape,
                        g => g.DrawLine(StrokePen(shape.Stroke), ToPoint(line.From), ToPoint(line.To)));
                    break;

                case PolylineShape polyline when polyline.Points.Length > 1:
                    var openPoints = polyline.Points.Select(ToPoint).ToArray();
                    DrawStroke(bitmap, shape, g => g.DrawLines(StrokePen(shape.Stroke), openPoints));
                    break;

                case RectShape rect:
                    FillArea(bitmap, shape.Fill, MaskShape.Rectangle, TopLeft(rect.X, rect.Y, rect.Width, rect.Height),
                        Size(rect.Width, rect.Height), null);
                    DrawStroke(bitmap, shape, g => g.DrawRectangle(StrokePen(shape.Stroke),
                        (float)Math.Min(rect.X, rect.X + rect.Width), (float)Math.Min(rect.Y, rect.Y + rect.Height),
                        (float)Math.Abs(rect.Width), (float)Math.Abs(rect.Height)));
                    break;

                case EllipseShape ellipse:
                    FillArea(bitmap, shape.Fill, MaskShape.Circle,
                        TopLeft(ellipse.X, ellipse.Y, ellipse.Width, ellipse.Height),
                        Size(ellipse.Width, ellipse.Height), null);
                    DrawStroke(bitmap, shape, g => g.DrawEllipse(StrokePen(shape.Stroke),
                        (float)Math.Min(ellipse.X, ellipse.X + ellipse.Width),
                        (float)Math.Min(ellipse.Y, ellipse.Y + ellipse.Height),
                        (float)Math.Abs(ellipse.Width), (float)Math.Abs(ellipse.Height)));
                    break;

                case PolygonShape polygon when polygon.Points.Length > 2:
                    var closedPoints = polygon.Points.Select(ToPoint).ToArray();

                    // Texture/filter fills go through the same area-fill code the rectangle/ellipse cases use
                    // (MaskShape.Polygon there takes absolute points and ignores start/width/height), so the
                    // width/height/start arguments below are placeholders only used by the Rectangle/Circle cases.
                    FillArea(bitmap, shape.Fill, MaskShape.Polygon, default, default, closedPoints);
                    DrawStroke(bitmap, shape, g => g.DrawPolygon(StrokePen(shape.Stroke), closedPoints));
                    break;
            }
        }

        /// <summary>
        /// Fills the area.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="fill">The fill.</param>
        /// <param name="mask">The mask.</param>
        /// <param name="start">The start.</param>
        /// <param name="size">The size.</param>
        /// <param name="polygon">The polygon.</param>
        /// <returns></returns>
        private void FillArea(Bitmap bitmap, FillSpec fill, MaskShape mask, Point start, Size size, Point[]? polygon)
        {
            switch (fill)
            {
                case NoFill:
                    return;

                case SolidFill solid:
                    using (var g = Graphics.FromImage(bitmap))
                    using (var brush = new SolidBrush(ToColor(solid.Argb)))
                    {
                        switch (mask)
                        {
                            case MaskShape.Rectangle:
                                g.FillRectangle(brush, new Rectangle(start, size));
                                break;
                            case MaskShape.Circle:
                                g.FillEllipse(brush, new Rectangle(start, size));
                                break;
                            case MaskShape.Polygon:
                                if (polygon is { Length: > 0 }) g.FillPolygon(brush, polygon);
                                break;
                        }
                    }

                    return;

                case TextureFill texture:
                    if (Enum.TryParse<TextureType>(texture.Name, ignoreCase: true, out var textureType))
                    {
                        _textureGenerator.GenerateTextureOverlay(bitmap, size.Width, size.Height, textureType, mask,
                            start, (object?)polygon);
                    }

                    return;

                case FilterFill filterFill:
                    if (Enum.TryParse<FiltersType>(filterFill.Name, ignoreCase: true, out var filterType))
                    {
                        _render.FilterImageArea(bitmap, size.Width, size.Height, filterType, mask, (object?)polygon,
                            start);
                    }

                    return;
            }
        }

        /// <summary>
        /// Draws the stroke.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="draw">The draw.</param>
        private static void DrawStroke(Bitmap bitmap, Shape shape, Action<Graphics> draw)
        {
            if (shape.Stroke.Width <= 0) return;

            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            draw(g);
        }

        /// <summary>
        /// Strokes the pen.
        /// </summary>
        /// <param name="stroke">The stroke.</param>
        private static Pen StrokePen(StrokeSpec stroke)
        {
            // Not disposed here: GDI+ Pen has no unmanaged state worth freeing eagerly for the short lifetime
            // of one flatten call, and callers pass it straight into a using(Graphics) draw call above.
            return new Pen(ToColor(stroke.Argb), (float)stroke.Width) { LineJoin = LineJoin.Round };
        }

        /// <summary>
        /// Converts to color.
        /// </summary>
        /// <param name="argb">The ARGB.</param>
        /// <returns>Convert Color to System.Drawing.Point.</returns>
        private static Color ToColor(uint argb)
        {
            return Color.FromArgb((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);
        }

        /// <summary>
        /// Converts to point.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns>Rounds point.</returns>
        private static Point ToPoint(PointD p)
        {
            return new Point((int)Math.Round(p.X), (int)Math.Round(p.Y));
        }

        /// <summary>
        /// Tops the left.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Point To left.</returns>
        private static Point TopLeft(double x, double y, double width, double height)
        {
            return new Point((int)Math.Round(Math.Min(x, x + width)), (int)Math.Round(Math.Min(y, y + height)));
        }

        /// <summary>
        /// Sizes the Image.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Size of the Image</returns>
        private static Size Size(double width, double height)
        {
            return new Size((int)Math.Round(Math.Abs(width)), (int)Math.Round(Math.Abs(height)));
        }
    }
}