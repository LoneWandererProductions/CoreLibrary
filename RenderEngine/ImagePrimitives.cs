/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        ImagePrimitives.cs
 * PURPOSE:     Primitive drawing and image manipulation helpers.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Drawing;
using System.Numerics;

namespace RenderEngine
{
    /// <summary>
    /// Provides primitive drawing operations for <see cref="UnmanagedImageBuffer"/>.
    /// Includes basic shapes, fills, lines, and textured rendering.
    /// </summary>
    public static class ImagePrimitives
    {
        /// <summary>
        /// Clears the entire buffer to the specified color.
        /// </summary>
        /// <param name="buf">Target image buffer.</param>
        /// <param name="c">Fill color.</param>
        public static void Clear(UnmanagedImageBuffer buf, Color c)
        {
            var span = buf.BufferSpan;
            const int bpp = UnmanagedImageBuffer.BytesPerPixel;
            var len = span.Length;
            for (var i = 0; i < len; i += bpp)
            {
                span[i + 0] = c.B;
                span[i + 1] = c.G;
                span[i + 2] = c.R;
                span[i + 3] = c.A;
            }
        }

        /// <summary>
        /// Clears the entire buffer to the specified color.
        /// </summary>
        /// <param name="buf">Target image buffer.</param>
        /// <param name="a">a.</param>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        public static void Clear(UnmanagedImageBuffer buf, int a, int r, int g, int b)
        {
            var span = buf.BufferSpan;
            const int bpp = UnmanagedImageBuffer.BytesPerPixel;
            var len = span.Length;
            for (var i = 0; i < len; i += bpp)
            {
                span[i + 0] = (byte)b;
                span[i + 1] = (byte)g;
                span[i + 2] = (byte)r;
                span[i + 3] = (byte)a;
            }
        }

        /// <summary>
        /// Draws a line using Bresenham’s algorithm.
        /// </summary>
        /// <param name="buf">Target image buffer.</param>
        /// <param name="p0">Start point of the line.</param>
        /// <param name="p1">End point of the line.</param>
        /// <param name="c">Line color.</param>
        public static void DrawLine(UnmanagedImageBuffer buf, Point p0, Point p1, Color c)
        {
            int x0 = p0.X, y0 = p0.Y;
            int x1 = p1.X, y1 = p1.Y;

            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var sx = x0 < x1 ? 1 : -1;
            var sy = y0 < y1 ? 1 : -1;
            var err = dx - dy;

            while (true)
            {
                if ((uint)x0 < (uint)buf.Width && (uint)y0 < (uint)buf.Height)
                    buf.SetPixel(x0, y0, c.R, c.G, c.B, c.A);

                if (x0 == x1 && y0 == y1) break;

                var e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        /// <summary>
        /// Fills a rectangle with the given color.
        /// Rectangle bounds are clamped to the buffer dimensions.
        /// </summary>
        /// <param name="buf">Target image buffer.</param>
        /// <param name="rect">Rectangle to fill.</param>
        /// <param name="c">Fill color.</param>
        public static void FillRect(UnmanagedImageBuffer buf, Rectangle rect, Color c)
        {
            var left = Math.Max(0, rect.Left);
            var top = Math.Max(0, rect.Top);
            var right = Math.Min(buf.Width, rect.Right);
            var bottom = Math.Min(buf.Height, rect.Bottom);

            for (var y = top; y < bottom; y++)
            {
                for (var x = left; x < right; x++)
                    buf.SetPixel(x, y, c.R, c.G, c.B, c.A);
            }
        }


        /// <summary>
        /// Creates a new <see cref="UnmanagedImageBuffer"/> from a <see cref="Bitmap"/>.
        /// Converts the bitmap into 32bpp ARGB format.
        /// </summary>
        /// <param name="bmp">Source bitmap.</param>
        /// <returns>A new unmanaged buffer containing the bitmap data.</returns>
        public static UnmanagedImageBuffer FromBitmap(Bitmap bmp)
        {
            var width = bmp.Width;
            var height = bmp.Height;
            var buffer = new UnmanagedImageBuffer(width, height);

            var rect = new Rectangle(0, 0, width, height);
            var bmpData = bmp.LockBits(rect,
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                unsafe
                {
                    var srcPtr = (byte*)bmpData.Scan0;
                    var dstSpan = buffer.BufferSpan;

                    for (var y = 0; y < height; y++)
                    {
                        var rowOffset = y * width * UnmanagedImageBuffer.BytesPerPixel;
                        var rowSrc = srcPtr + y * bmpData.Stride;

                        for (var x = 0; x < width; x++)
                        {
                            var colOffset = x * UnmanagedImageBuffer.BytesPerPixel;

                            var b = rowSrc[colOffset + 0];
                            var g = rowSrc[colOffset + 1];
                            var r = rowSrc[colOffset + 2];
                            var a = rowSrc[colOffset + 3];

                            var dstIndex = rowOffset + colOffset;
                            dstSpan[dstIndex + 0] = b;
                            dstSpan[dstIndex + 1] = g;
                            dstSpan[dstIndex + 2] = r;
                            dstSpan[dstIndex + 3] = a;
                        }
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bmpData);
            }

            return buffer;
        }


        /// <summary>
        /// Fills a triangle with a solid color using affine barycentric rasterization.
        /// </summary>
        /// <param name="buf">Target image buffer.</param>
        /// <param name="p0">First vertex.</param>
        /// <param name="p1">Second vertex.</param>
        /// <param name="p2">Third vertex.</param>
        /// <param name="color">Fill color.</param>
        public static void FillTriangle(UnmanagedImageBuffer buf, Point p0, Point p1, Point p2, Color color)
        {
            var minX = Math.Max(0, Math.Min(p0.X, Math.Min(p1.X, p2.X)));
            var maxX = Math.Min(buf.Width - 1, Math.Max(p0.X, Math.Max(p1.X, p2.X)));
            var minY = Math.Max(0, Math.Min(p0.Y, Math.Min(p1.Y, p2.Y)));
            var maxY = Math.Min(buf.Height - 1, Math.Max(p0.Y, Math.Max(p1.Y, p2.Y)));

            var v0 = new Vector2(p1.X - p0.X, p1.Y - p0.Y);
            var v1 = new Vector2(p2.X - p0.X, p2.Y - p0.Y);
            var denom = v0.X * v1.Y - v1.X * v0.Y;
            if (Math.Abs(denom) < 1e-9f) return;

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var v2 = new Vector2(x - p0.X, y - p0.Y);
                    var u = (v2.X * v1.Y - v1.X * v2.Y) / denom;
                    var v = (v0.X * v2.Y - v2.X * v0.Y) / denom;
                    var w = 1 - u - v;

                    if (u >= 0f && v >= 0f && w >= 0f)
                    {
                        buf.SetPixel(x, y, color.R, color.G, color.B, color.A);
                    }
                }
            }
        }

        // ---------------------------
        // Textured triangle (perspective-correct if Zs provided)
        // ---------------------------

        /// <summary>
        /// Small helper for prepped vertices
        /// </summary>
        private struct PrepVertex
        {
            public float X, Y; // screen space
            public readonly float UOverZ; // u/z
            public readonly float VOverZ; // v/z
            public readonly float InvZ; // 1/z

            public PrepVertex(float x, float y, float uOverZ, float vOverZ, float invZ)
            {
                X = x;
                Y = y;
                UOverZ = uOverZ;
                VOverZ = vOverZ;
                InvZ = invZ;
            }
        }

        /// <summary>
        /// Draws a textured triangle with optional perspective correction.
        /// If all z-values are greater than 0, perspective mapping is applied; otherwise affine interpolation is used.
        /// </summary>
        /// <param name="buf">Target image buffer.</param>
        /// <param name="p0">First screen-space vertex.</param>
        /// <param name="uv0">First texture coordinate (0..1).</param>
        /// <param name="z0">First depth value.</param>
        /// <param name="p1">Second screen-space vertex.</param>
        /// <param name="uv1">Second texture coordinate (0..1).</param>
        /// <param name="z1">Second depth value.</param>
        /// <param name="p2">Third screen-space vertex.</param>
        /// <param name="uv2">Third texture coordinate (0..1).</param>
        /// <param name="z2">Third depth value.</param>
        /// <param name="texture">Texture buffer.</param>
        /// <param name="wrap">Whether texture coordinates should wrap (true) or clamp (false).</param>
        public static void DrawTexturedTriangle(
            UnmanagedImageBuffer buf,
            Point p0, Vector2 uv0, float z0,
            Point p1, Vector2 uv1, float z1,
            Point p2, Vector2 uv2, float z2,
            UnmanagedImageBuffer? texture,
            bool wrap = false)
        {
            if (texture == null) return;

            var usePerspective = (z0 > 0f && z1 > 0f && z2 > 0f);

            // bounding box
            var minX = Math.Max(0, (int)MathF.Floor(MathF.Min(p0.X, MathF.Min(p1.X, p2.X))));
            var maxX = Math.Min(buf.Width - 1, (int)MathF.Ceiling(MathF.Max(p0.X, MathF.Max(p1.X, p2.X))));
            var minY = Math.Max(0, (int)MathF.Floor(MathF.Min(p0.Y, MathF.Min(p1.Y, p2.Y))));
            var maxY = Math.Min(buf.Height - 1, (int)MathF.Ceiling(MathF.Max(p0.Y, MathF.Max(p1.Y, p2.Y))));

            float Edge(float x0, float y0, float x1, float y1, float x, float y)
                => (x - x0) * (y1 - y0) - (y - y0) * (x1 - x0);

            bool IsTopLeft(Point a, Point b) => (a.Y < b.Y) || (a.Y == b.Y && a.X > b.X);

            bool PixelInside(float w0, float w1, float w2)
                => (w0 > 0 || (w0 == 0 && IsTopLeft(p1, p2))) &&
                   (w1 > 0 || (w1 == 0 && IsTopLeft(p2, p0))) &&
                   (w2 > 0 || (w2 == 0 && IsTopLeft(p0, p1)));

            var area = Edge(p0.X, p0.Y, p1.X, p1.Y, p2.X, p2.Y);
            if (Math.Abs(area) < 1e-6f) return;

            var texBuf = texture.BufferSpan;
            var texW = texture.Width;
            var texH = texture.Height;
            const int bpp = UnmanagedImageBuffer.BytesPerPixel;

            PrepVertex pv0 = default, pv1 = default, pv2 = default;
            if (usePerspective)
            {
                float iz0 = 1f / z0, iz1 = 1f / z1, iz2 = 1f / z2;
                pv0 = new PrepVertex(p0.X, p0.Y, uv0.X * iz0, uv0.Y * iz0, iz0);
                pv1 = new PrepVertex(p1.X, p1.Y, uv1.X * iz1, uv1.Y * iz1, iz1);
                pv2 = new PrepVertex(p2.X, p2.Y, uv2.X * iz2, uv2.Y * iz2, iz2);
            }

            for (var y = minY; y <= maxY; y++)
            {
                for (var x = minX; x <= maxX; x++)
                {
                    var px = x + 0.5f;
                    var py = y + 0.5f;

                    var w0 = Edge(p1.X, p1.Y, p2.X, p2.Y, px, py);
                    var w1 = Edge(p2.X, p2.Y, p0.X, p0.Y, px, py);
                    var w2 = Edge(p0.X, p0.Y, p1.X, p1.Y, px, py);

                    if (!PixelInside(w0, w1, w2)) continue;

                    w0 /= area;
                    w1 /= area;
                    w2 /= area;

                    float u, v;
                    if (usePerspective)
                    {
                        var oneOverZ = w0 * pv0.InvZ + w1 * pv1.InvZ + w2 * pv2.InvZ;
                        var uOverZ = w0 * pv0.UOverZ + w1 * pv1.UOverZ + w2 * pv2.UOverZ;
                        var vOverZ = w0 * pv0.VOverZ + w1 * pv1.VOverZ + w2 * pv2.VOverZ;

                        if (oneOverZ == 0f) continue;

                        u = uOverZ / oneOverZ;
                        v = vOverZ / oneOverZ;
                    }
                    else
                    {
                        u = w0 * uv0.X + w1 * uv1.X + w2 * uv2.X;
                        v = w0 * uv0.Y + w1 * uv1.Y + w2 * uv2.Y;
                    }

                    var tx = (int)MathF.Round(u * (texW - 1));
                    var ty = (int)MathF.Round(v * (texH - 1));

                    if (wrap)
                    {
                        tx = ((tx % texW) + texW) % texW;
                        ty = ((ty % texH) + texH) % texH;
                    }
                    else
                    {
                        tx = Math.Clamp(tx, 0, texW - 1);
                        ty = Math.Clamp(ty, 0, texH - 1);
                    }

                    var toff = (ty * texW + tx) * bpp;
                    var tb = texBuf[toff + 0];
                    var tg = texBuf[toff + 1];
                    var tr = texBuf[toff + 2];
                    var ta = texBuf[toff + 3];

                    if (ta != 0)
                        buf.SetPixelAlphaBlend(x, y, tr, tg, tb, ta);
                }
            }
        }

        /// <summary>
        /// Draws a textured triangle using affine mapping only (no perspective correction).
        /// </summary>
        /// <param name="buf">Target image buffer.</param>
        /// <param name="p0">First screen-space vertex.</param>
        /// <param name="uv0">First texture coordinate (0..1).</param>
        /// <param name="p1">Second screen-space vertex.</param>
        /// <param name="uv1">Second texture coordinate (0..1).</param>
        /// <param name="p2">Third screen-space vertex.</param>
        /// <param name="uv2">Third texture coordinate (0..1).</param>
        /// <param name="texture">Texture buffer.</param>
        /// <param name="wrap">Whether texture coordinates should wrap (true) or clamp (false).</param>
        public static void DrawTexturedTriangle(
            UnmanagedImageBuffer buf,
            Point p0, Vector2 uv0,
            Point p1, Vector2 uv1,
            Point p2, Vector2 uv2,
            UnmanagedImageBuffer? texture,
            bool wrap = false)
        {
            DrawTexturedTriangle(buf, p0, uv0, 1f, p1, uv1, 1f, p2, uv2, 1f, texture, wrap);
        }

        /// <summary>
        /// Fills a triangle with a solid packed BGRA value.
        /// Optional: Draw filled triangle (uint packed BGRA) using the same rasterization core.
        /// </summary>
        /// <param name="buf">Target image buffer.</param>
        /// <param name="p0">First vertex.</param>
        /// <param name="p1">Second vertex.</param>
        /// <param name="p2">Third vertex.</param>
        /// <param name="packedBgra">Packed BGRA color (0xAARRGGBB).</param>
        public static void FillTriangle(UnmanagedImageBuffer buf, Point p0, Point p1, Point p2, uint packedBgra)
        {
            var b = (byte)(packedBgra & 0xFF);
            var g = (byte)((packedBgra >> 8) & 0xFF);
            var r = (byte)((packedBgra >> 16) & 0xFF);
            var a = (byte)((packedBgra >> 24) & 0xFF);
            FillTriangle(buf, p0, p1, p2, Color.FromArgb(a, r, g, b));
        }

        /// <summary>
        /// Draws a textured quadrilateral as two triangles.
        /// Texture coordinates are assumed in standard order:
        /// p0=(0,0), p1=(1,0), p2=(1,1), p3=(0,1).
        /// Convenience: Draw textured quad as two triangles (UVs top-left/top-right/bottom-right/bottom-left)
        /// </summary>
        /// <param name="buf">Target image buffer.</param>
        /// <param name="p0">Top-left corner.</param>
        /// <param name="p1">Top-right corner.</param>
        /// <param name="p2">Bottom-right corner.</param>
        /// <param name="p3">Bottom-left corner.</param>
        /// <param name="texture">Texture buffer.</param>
        /// <param name="wrap">Whether texture coordinates should wrap (true) or clamp (false).</param>
        public static void DrawTexturedQuad(UnmanagedImageBuffer buf,
            Point p0, Point p1, Point p2, Point p3,
            UnmanagedImageBuffer? texture, bool wrap = false)
        {
            // UVs: p0=(0,0), p1=(1,0), p2=(1,1), p3=(0,1)
            DrawTexturedTriangle(buf, p0, new Vector2(0f, 0f), p1, new Vector2(1f, 0f), p2, new Vector2(1f, 1f),
                texture, wrap);
            DrawTexturedTriangle(buf, p0, new Vector2(0f, 0f), p2, new Vector2(1f, 1f), p3, new Vector2(0f, 1f),
                texture, wrap);
        }
    }
}
