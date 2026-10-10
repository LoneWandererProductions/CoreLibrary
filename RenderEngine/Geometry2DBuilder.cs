/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        Geometry2DBuilder.cs
 * PURPOSE:     Builder for 2D geometry vertex buffers.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;

namespace RenderEngine
{
    /// <summary>
    /// Static helper for building 2D geometry vertex buffers.
    /// Contains no OpenGL code.
    /// </summary>
    internal static class Geometry2DBuilder
    {
        /// <summary>
        /// Builds the colored lines.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>Array of floats representing the colored lines.</returns>
        internal static float[] BuildColoredLines(
            (float x, float y, int r, int g, int b, int a)[] points)
        {
            var data = new float[points.Length * 6];

            for (var i = 0; i < points.Length; i++)
            {
                data[i * 6 + 0] = points[i].x;
                data[i * 6 + 1] = points[i].y;
                data[i * 6 + 2] = points[i].r / 255f;
                data[i * 6 + 3] = points[i].g / 255f;
                data[i * 6 + 4] = points[i].b / 255f;
                data[i * 6 + 5] = points[i].a / 255f;
            }

            return data;
        }

        /// <summary>
        /// Builds the solid quad.
        /// </summary>
        /// <param name="p0">The p0.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        /// <param name="fill">The fill.</param>
        /// <returns>Array of floats representing a solid quad.</returns>
        internal static float[] BuildSolidQuad(
            (int x, int y) p0,
            (int x, int y) p1,
            (int x, int y) p2,
            (int x, int y) p3,
            (int r, int g, int b, int a) fill)
        {
            var r = fill.r / 255f;
            var g = fill.g / 255f;
            var b = fill.b / 255f;
            var a = fill.a / 255f;

            return new[]
            {
                p0.x, p0.y, r, g, b, a, p1.x, p1.y, r, g, b, a, p2.x, p2.y, r, g, b, a, p2.x, p2.y, r, g, b, a,
                p3.x, p3.y, r, g, b, a, p0.x, p0.y, r, g, b, a
            };
        }

        /// <summary>
        /// Builds the colored triangle.
        /// </summary>
        /// <param name="p0">The p0.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <returns>Array of floats representing a colored triangle.</returns>
        public static float[] BuildColoredTriangle(
            (float x, float y, int r, int g, int b, int a) p0,
            (float x, float y, int r, int g, int b, int a) p1,
            (float x, float y, int r, int g, int b, int a) p2)
        {
            return new[]
            {
                p0.x, p0.y, p0.r / 255f, p0.g / 255f, p0.b / 255f, p0.a / 255f, p1.x, p1.y, p1.r / 255f,
                p1.g / 255f, p1.b / 255f, p1.a / 255f, p2.x, p2.y, p2.r / 255f, p2.g / 255f, p2.b / 255f,
                p2.a / 255f,
            };
        }

        /// <summary>
        /// Builds the textured quad.
        /// </summary>
        /// <param name="p0">The p0.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        /// <returns>Array of floats representing a textured quad.</returns>
        public static float[] BuildTexturedQuad(
            (int x, int y) p0,
            (int x, int y) p1,
            (int x, int y) p2,
            (int x, int y) p3)
        {
            return new[]
            {
                p0.x, p0.y, 0f, 0f, p1.x, p1.y, 1f, 0f, p2.x, p2.y, 1f, 1f, p2.x, p2.y, 1f, 1f, p3.x, p3.y, 0f, 1f,
                p0.x, p0.y, 0f, 0f
            };
        }

        /// <summary>
        /// Builds the polyline.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="color">The color.</param>
        /// <returns>Array of floats representing a polyline.</returns>
        internal static (float x, float y, int r, int g, int b, int a)[] BuildPolyline(
            ReadOnlySpan<(float x, float y)> points,
            (int r, int g, int b, int a) color)
        {
            if (points.Length < 2)
                return Array.Empty<(float, float, int, int, int, int)>();

            var data = new (float x, float y, int r, int g, int b, int a)[points.Length * 2 - 2];

            var idx = 0;
            for (var i = 0; i < points.Length - 1; i++)
            {
                data[idx++] = (points[i].x, points[i].y, color.r, color.g, color.b, color.a);
                data[idx++] = (points[i + 1].x, points[i + 1].y, color.r, color.g, color.b, color.a);
            }

            return data;
        }

        /// <summary>
        /// Builds the rect outline.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="w">The w.</param>
        /// <param name="h">The h.</param>
        /// <param name="color">The color.</param>
        /// <returns>Array of floats representing a rectangle outline.</returns>
        public static (float x, float y, int r, int g, int b, int a)[]
            BuildRectOutline(
                float x, float y,
                float w, float h,
                (int r, int g, int b, int a) color)
        {
            Span<(float x, float y)> pts = stackalloc[] { (x, y), (x + w, y), (x + w, y + h), (x, y + h), (x, y), };

            return BuildPolyline(pts, color);
        }


        /// <summary>
        /// Builds the circle outline.
        /// </summary>
        /// <param name="cx">The cx.</param>
        /// <param name="cy">The cy.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="segments">The segments.</param>
        /// <param name="color">The color.</param>
        /// <returns>Array of floats representing a circle outline.</returns>
        internal static (float x, float y, int r, int g, int b, int a)[] BuildCircleOutline(
            float cx, float cy,
            float radius,
            int segments,
            (int r, int g, int b, int a) color)
        {
            if (segments < 3)
                return Array.Empty<(float, float, int, int, int, int)>();

            var pts = new (float x, float y, int r, int g, int b, int a)[segments * 2];

            var step = MathF.PI * 2f / segments;
            var idx = 0;

            var prevX = cx + MathF.Cos(0) * radius;
            var prevY = cy + MathF.Sin(0) * radius;

            for (var i = 1; i <= segments; i++)
            {
                var a = i * step;

                var x = cx + MathF.Cos(a) * radius;
                var y = cy + MathF.Sin(a) * radius;

                pts[idx++] = (prevX, prevY, color.r, color.g, color.b, color.a);
                pts[idx++] = (x, y, color.r, color.g, color.b, color.a);

                prevX = x;
                prevY = y;
            }

            return pts;
        }
    }
}
