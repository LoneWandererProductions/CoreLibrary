/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     FolkTales
 * FILE:        OpenTKDrawHelper.cs
 * PURPOSE:     OpenGL4 renderer mostly using OpenTK functions to draw colored lines and textured quads.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;

namespace RenderEngine;

public static class OpenTkDrawHelper
{
    // --- Persistent buffers ---
    private static int _lineVbo;
    private static int _lineVao;
    private static int _lineBufferSize;

    private static int _quadVbo;
    private static int _quadVao;
    private static int _quadBufferSize;

    /// <summary>
    /// Initialize persistent VAOs/VBOs. Call once at startup.
    /// </summary>
    public static void Initialize()
    {
        // Lines
        _lineVbo = GL.GenBuffer();
        _lineVao = GL.GenVertexArray();

        GL.BindVertexArray(_lineVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _lineVbo);

        GL.EnableVertexAttribArray(0); // position
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

        GL.EnableVertexAttribArray(1); // color
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 6 * sizeof(float), 2 * sizeof(float));

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);

        // Quads (textured)
        _quadVbo = GL.GenBuffer();
        _quadVao = GL.GenVertexArray();

        GL.BindVertexArray(_quadVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _quadVbo);

        GL.EnableVertexAttribArray(0); // position
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

        GL.EnableVertexAttribArray(1); // uv
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }

    /// <summary>
    /// Draw colored lines using persistent VBO/VAO.
    /// </summary>
    /// <param name="vertices">The vertices.</param>
    public static void DrawColoredLines((float x, float y, Color c)[] vertices)
    {
        var requiredSize = vertices.Length * 6 * sizeof(float);
        if (requiredSize > _lineBufferSize)
        {
            _lineBufferSize = requiredSize;
            GL.BindBuffer(BufferTarget.ArrayBuffer, _lineVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _lineBufferSize, IntPtr.Zero, BufferUsageHint.StreamDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        // Upload vertex data
        var data = new float[vertices.Length * 6];
        for (var i = 0; i < vertices.Length; i++)
        {
            data[i * 6 + 0] = vertices[i].x;
            data[i * 6 + 1] = vertices[i].y;
            data[i * 6 + 2] = vertices[i].c.R / 255f;
            data[i * 6 + 3] = vertices[i].c.G / 255f;
            data[i * 6 + 4] = vertices[i].c.B / 255f;
            data[i * 6 + 5] = vertices[i].c.A / 255f;
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, _lineVbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);

        GL.BindVertexArray(_lineVao);
        GL.DrawArrays(PrimitiveType.Lines, 0, vertices.Length);
        GL.BindVertexArray(0);
    }

    /// <summary>
    /// Draw a solid-colored quad using 2 triangles.
    /// </summary>
    /// <param name="p0">The p0.</param>
    /// <param name="p1">The p1.</param>
    /// <param name="p2">The p2.</param>
    /// <param name="p3">The p3.</param>
    /// <param name="c">The c.</param>
    public static void DrawSolidQuad(Point p0, Point p1, Point p2, Point p3, Color c)
    {
        var verts = new (float x, float y, Color col)[]
        {
            (p0.X, p0.Y, c), (p1.X, p1.Y, c), (p2.X, p2.Y, c), (p2.X, p2.Y, c), (p3.X, p3.Y, c), (p0.X, p0.Y, c)
        };

        DrawColoredLines(verts); // uses persistent buffer
    }

    /// <summary>
    /// Draw a textured quad using persistent VBO/VAO.
    /// </summary>
    /// <param name="texId">The tex identifier.</param>
    /// <param name="p0">The p0.</param>
    /// <param name="p1">The p1.</param>
    /// <param name="p2">The p2.</param>
    /// <param name="p3">The p3.</param>
    public static void DrawTexturedQuad(int texId, Point p0, Point p1, Point p2, Point p3)
    {
        float[] vertices =
        {
            p0.X, p0.Y, 0f, 0f, p1.X, p1.Y, 1f, 0f, p2.X, p2.Y, 1f, 1f, p2.X, p2.Y, 1f, 1f, p3.X, p3.Y, 0f, 1f,
            p0.X, p0.Y, 0f, 0f
        };

        var requiredSize = vertices.Length * sizeof(float);
        if (requiredSize > _quadBufferSize)
        {
            _quadBufferSize = requiredSize;
            GL.BindBuffer(BufferTarget.ArrayBuffer, _quadVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _quadBufferSize, IntPtr.Zero, BufferUsageHint.StreamDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, _quadVbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * sizeof(float), vertices);

        GL.BindVertexArray(_quadVao);
        GL.BindTexture(TextureTarget.Texture2D, texId);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        GL.BindVertexArray(0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public static void DrawTexturedQuad(int texId, Point p0, Point p1, Point p2, Point p3,
        float u0 = 0f, float v0 = 0f, float u1 = 1f, float v1 = 1f)
    {
        float[] vertices =
        {
            p0.X, p0.Y, u0, v0, p1.X, p1.Y, u1, v0, p2.X, p2.Y, u1, v1, p2.X, p2.Y, u1, v1, p3.X, p3.Y, u0, v1,
            p0.X, p0.Y, u0, v0
        };

        var requiredSize = vertices.Length * sizeof(float);
        if (requiredSize > _quadBufferSize)
        {
            _quadBufferSize = requiredSize;
            GL.BindBuffer(BufferTarget.ArrayBuffer, _quadVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _quadBufferSize, IntPtr.Zero, BufferUsageHint.StreamDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, _quadVbo);
        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * sizeof(float), vertices);

        GL.BindVertexArray(_quadVao);
        GL.BindTexture(TextureTarget.Texture2D, texId);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        GL.BindVertexArray(0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }


    /// <summary>
    /// Dispose persistent buffers.
    /// </summary>
    public static void Dispose()
    {
        if (_lineVbo != 0) GL.DeleteBuffer(_lineVbo);
        if (_lineVao != 0) GL.DeleteVertexArray(_lineVao);
        if (_quadVbo != 0) GL.DeleteBuffer(_quadVbo);
        if (_quadVao != 0) GL.DeleteVertexArray(_quadVao);

        _lineVbo = _lineVao = _quadVbo = _quadVao = 0;
    }
}
