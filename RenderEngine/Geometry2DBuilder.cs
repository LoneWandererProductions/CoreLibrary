/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        Geometry2DBuilder.cs
 * PURPOSE:     Your file purpose here
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
        internal static float[] BuildColoredLines(
            (float x, float y, int r, int g, int b, int a)[] points)
        {
            var data = new float[points.Length * 6];

            for (int i = 0; i < points.Length; i++)
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

        internal static float[] BuildSolidQuad(
            (int x, int y) p0,
            (int x, int y) p1,
            (int x, int y) p2,
            (int x, int y) p3,
            (int r, int g, int b, int a) fill)
        {
            float r = fill.r / 255f;
            float g = fill.g / 255f;
            float b = fill.b / 255f;
            float a = fill.a / 255f;

            return new[]
            {
                p0.x, p0.y, r, g, b, a,
                p1.x, p1.y, r, g, b, a,
                p2.x, p2.y, r, g, b, a,

                p2.x, p2.y, r, g, b, a,
                p3.x, p3.y, r, g, b, a,
                p0.x, p0.y, r, g, b, a
            };
        }

        public static float[] BuildColoredTriangle(
            (float x, float y, int r, int g, int b, int a) p0,
            (float x, float y, int r, int g, int b, int a) p1,
            (float x, float y, int r, int g, int b, int a) p2)
        {
            return new[]
            {
                p0.x, p0.y, p0.r / 255f, p0.g / 255f, p0.b / 255f, p0.a / 255f,
                p1.x, p1.y, p1.r / 255f, p1.g / 255f, p1.b / 255f, p1.a / 255f,
                p2.x, p2.y, p2.r / 255f, p2.g / 255f, p2.b / 255f, p2.a / 255f,
            };
        }

        public static float[] BuildTexturedQuad(
            (int x, int y) p0,
            (int x, int y) p1,
            (int x, int y) p2,
            (int x, int y) p3)
        {
            return new[]
            {
                p0.x, p0.y, 0f, 0f,
                p1.x, p1.y, 1f, 0f,
                p2.x, p2.y, 1f, 1f,

                p2.x, p2.y, 1f, 1f,
                p3.x, p3.y, 0f, 1f,
                p0.x, p0.y, 0f, 0f
            };
        }

        internal static (float x, float y, int r, int g, int b, int a)[] BuildPolyline(
            ReadOnlySpan<(float x, float y)> points,
            (int r, int g, int b, int a) color)
        {
            if (points.Length < 2)
                return Array.Empty<(float, float, int, int, int, int)>();

            var data = new (float x, float y, int r, int g, int b, int a)[points.Length * 2 - 2];

            int idx = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                data[idx++] = (points[i].x, points[i].y, color.r, color.g, color.b, color.a);
                data[idx++] = (points[i + 1].x, points[i + 1].y, color.r, color.g, color.b, color.a);
            }

            return data;
        }

        public static (float x, float y, int r, int g, int b, int a)[]
            BuildRectOutline(
                float x, float y,
                float w, float h,
                (int r, int g, int b, int a) color)
        {
            Span<(float x, float y)> pts = stackalloc[]
            {
                (x, y),
                (x + w, y),
                (x + w, y + h),
                (x, y + h),
                (x, y),
            };

            return BuildPolyline(pts, color);
        }


        internal static (float x, float y, int r, int g, int b, int a)[] BuildCircleOutline(
            float cx, float cy,
            float radius,
            int segments,
            (int r, int g, int b, int a) color)
        {
            if (segments < 3)
                return Array.Empty<(float, float, int, int, int, int)>();

            var pts = new (float x, float y, int r, int g, int b, int a)[segments * 2];

            float step = MathF.PI * 2f / segments;
            int idx = 0;

            float prevX = cx + MathF.Cos(0) * radius;
            float prevY = cy + MathF.Sin(0) * radius;

            for (int i = 1; i <= segments; i++)
            {
                float a = i * step;

                var x = cx + MathF.Cos(a) * radius;
                float y = cy + MathF.Sin(a) * radius;

                pts[idx++] = (prevX, prevY, color.r, color.g, color.b, color.a);
                pts[idx++] = (x, y, color.r, color.g, color.b, color.a);

                prevX = x;
                prevY = y;
            }

            return pts;
        }
    }
}