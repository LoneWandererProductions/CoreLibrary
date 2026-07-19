/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        Simple2DRenderer.cs
 * PURPOSE:     Lightweight 2D renderer using OpenGL
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 * NOTES:       - Pixel-based 2D rendering
 *              - Origin Top-left
 *              - flipY = true → OpenGL default (bottom-left origin)
 *              - flipY = false → software-like top-left origin
 *              - Consider batching multiple quads/lines for performance
 *              - Add optional Y-flip mode for top-left origin
 *              - Add text rendering
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Windows.Media.TextFormatting;
using static System.Net.Mime.MediaTypeNames;

namespace RenderEngine
{
    ///<inheritdoc/>
    /// <summary>
    /// Simple GPU-accelerated 2D renderer for colored lines, solid quads, and textured quads.
    /// </summary>
    public sealed class Simple2DRenderer : IDisposable
    {
        /// <summary>
        /// Gets the width of the viewport.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width { get; private set; }

        /// <summary>
        /// The height of the viewport.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int Height { get; private set; }

        /// <summary>
        /// The resources
        /// </summary>
        private readonly GlResourceManager _resources;

        // Solid VAO/VBO
        private int _vaoSolid;
        private int _vboSolid;

        // --- Textured VAO/VBO ---

        /// <summary>
        /// The vao tex
        /// </summary>
        private int _vaoTex;

        /// <summary>
        /// The vbo tex
        /// </summary>
        private int _vboTex;

        private int _ui2DColorShader;
        private int _ui2DTextureShader;

        /// <summary>
        /// The initialized
        /// </summary>
        private bool _initialized;

        private int _vboSolidCapacity = 4096; // Start with a more reasonable size
        private int _vboTexCapacity = 4096;
        private int _fallbackTextureId = -1; // Caches the checkerboard texture

        // --- 2D TEXT ATLAS REGISTRIES ---

        /// <summary>
        /// The font texture identifier
        /// </summary>
        private int _fontTextureId = -1;

        /// <summary>
        /// The font cell width
        /// High-legibility cell metrics
        /// </summary>
        private const int FontCellWidth = 12;

        /// <summary>
        /// The font cell height
        /// High-legibility cell metrics
        /// </summary>
        private const int FontCellHeight = 24;

        /// <summary>
        /// Dynamically updates the 2D resolution parameters when the host screen or canvas transforms.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Simple2DRenderer"/> class.
        /// </summary>
        /// <param name="width">Width of the render viewport in pixels.</param>
        /// <param name="height">Height of the render viewport in pixels.</param>
        /// <param name="resources">The GL resource manager instance.</param>
        public Simple2DRenderer(int width, int height, GlResourceManager resources)
        {
            Width = width;
            Height = height;
            _resources = resources;
        }

        /// <summary>
        /// Ensures that VAOs, VBOs, and shaders are initialized.
        /// Lazy-initialized at first draw call.
        /// </summary>
        private void EnsureInitialized()
        {
            if (_initialized) return;

            // --- Solid VAO/VBO (position + color) ---
            _vaoSolid = GL.GenVertexArray();
            _vboSolid = GL.GenBuffer();
            GL.BindVertexArray(_vaoSolid);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);

            //Buffer
            // For Solid VBO
            GL.BufferData(BufferTarget.ArrayBuffer, _vboSolidCapacity * sizeof(float), IntPtr.Zero,
                BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0); // Position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 6 * sizeof(float),
                2 * sizeof(float)); // Color
            GL.EnableVertexAttribArray(1);
            GL.BindVertexArray(0);

            // --- Textured VAO/VBO (position + texcoord) ---
            _vaoTex = GL.GenVertexArray();
            _vboTex = GL.GenBuffer();
            GL.BindVertexArray(_vaoTex);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);

            // for Textured VBO ...
            GL.BufferData(BufferTarget.ArrayBuffer, _vboTexCapacity * sizeof(float), IntPtr.Zero,
                BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0); // Position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float),
                2 * sizeof(float)); // Texcoord
            GL.EnableVertexAttribArray(1);
            GL.BindVertexArray(0);

            // --- Shaders ---
            _ui2DColorShader = _resources.GetShaderProgram(ShaderTypeApp.VertexColor2D);
            _ui2DTextureShader = _resources.GetShaderProgram(ShaderTypeApp.TexturedQuad2D);

            _initialized = true;
        }

        /// <summary>
        /// Flushes the specified batch.
        /// </summary>
        /// <param name="batch">The batch.</param>
        public void Flush(RenderBatch batch)
        {
            EnsureInitialized();
            FlushBatch(batch);
        }

        /// <summary>
        /// Draws the line.
        /// </summary>
        /// <param name="x0">The x0.</param>
        /// <param name="y0">The y0.</param>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="color">The color.</param>
        public void DrawLine(
            float x0, float y0,
            float x1, float y1,
            (int r, int g, int b, int a) color)
        {
            DrawColoredLines(new[]
            {
                (x0, y0, color.r, color.g, color.b, color.a), (x1, y1, color.r, color.g, color.b, color.a),
            });
        }

        /// <summary>
        /// Draws the rect outline.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="w">The w.</param>
        /// <param name="h">The h.</param>
        /// <param name="color">The color.</param>
        public void DrawRectOutline(
            float x, float y,
            float w, float h,
            (int r, int g, int b, int a) color)
        {
            var data = Geometry2DBuilder.BuildRectOutline(x, y, w, h, color);
            if (data.Length == 0) return;

            DrawColoredLines(data);
        }

        /// <summary>
        /// Draws the polyline.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="color">The color.</param>
        public void DrawPolyline(
            ReadOnlySpan<(float x, float y)> points,
            (int r, int g, int b, int a) color)
        {
            var data = Geometry2DBuilder.BuildPolyline(points, color);
            if (data.Length == 0) return;

            DrawColoredLines(data);
        }


        /// <summary>
        /// Draws lines with color. Points as (x,y,r,g,b,a).
        /// </summary>
        /// <param name="points">The points.</param>
        public void DrawColoredLines((float x, float y, int r, int g, int b, int a)[] points)
        {
            EnsureInitialized();
            BindShaderAndViewport(_ui2DColorShader);
            GL.BindVertexArray(_vaoSolid);

            var data = Geometry2DBuilder.BuildColoredLines(points);

            EnsureBufferCapacity(_vboSolid, ref _vboSolidCapacity, data.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
            GL.DrawArrays(PrimitiveType.Lines, 0, points.Length);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Draws a solid colored quad using 4 points.
        /// </summary>
        /// <param name="p0">The p0.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        /// <param name="fill">The fill.</param>
        public void DrawSolidQuad((int x, int y) p0, (int x, int y) p1, (int x, int y) p2, (int x, int y) p3,
            (int r, int g, int b, int a) fill)
        {
            EnsureInitialized();
            BindShaderAndViewport(_ui2DColorShader);
            GL.BindVertexArray(_vaoSolid);

            var data = Geometry2DBuilder.BuildSolidQuad(p0, p1, p2, p3, fill);

            EnsureBufferCapacity(_vboSolid, ref _vboSolidCapacity, data.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Draws a solid colored triangle using 3 points.
        /// </summary>
        /// <param name="p0">First vertex (x, y, r, g, b, a)</param>
        /// <param name="p1">Second vertex (x, y, r, g, b, a)</param>
        /// <param name="p2">Third vertex (x, y, r, g, b, a)</param>
        public void DrawColoredTriangle(
            (float x, float y, int r, int g, int b, int a) p0,
            (float x, float y, int r, int g, int b, int a) p1,
            (float x, float y, int r, int g, int b, int a) p2)
        {
            EnsureInitialized();
            BindShaderAndViewport(_ui2DColorShader);
            GL.BindVertexArray(_vaoSolid);

            var data = Geometry2DBuilder.BuildColoredTriangle(p0, p1, p2);

            EnsureBufferCapacity(_vboSolid, ref _vboSolidCapacity, data.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Draws a textured triangle using 3 points.
        /// </summary>
        /// <param name="p0">First vertex (position)</param>
        /// <param name="p1">Second vertex (position)</param>
        /// <param name="p2">Third vertex (position)</param>
        /// <param name="textureId">OpenGL texture ID</param>
        public void DrawTexturedTriangle(
            (int x, int y) p0,
            (int x, int y) p1,
            (int x, int y) p2,
            int textureId)
        {
            if (textureId < 0)
                textureId = _resources.GetFallbackTexture();

            EnsureInitialized();
            BindShaderAndViewport(_ui2DTextureShader);
            GL.BindVertexArray(_vaoTex);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.Uniform1(GL.GetUniformLocation(_ui2DTextureShader, "uTexture"), 0);

            var data = new[] { p0.x, p0.y, 0f, 0f, p1.x, p1.y, 1f, 0f, p2.x, p2.y, 0.5f, 1f };

            EnsureBufferCapacity(_vboTex, ref _vboTexCapacity, data.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Draws a textured quad using 4 points.
        /// </summary>
        /// <param name="p0">The p0.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="p2">The p2.</param>
        /// <param name="p3">The p3.</param>
        /// <param name="textureId">The texture identifier.</param>
        public void DrawTexturedQuad((int x, int y) p0, (int x, int y) p1, (int x, int y) p2, (int x, int y) p3,
            int textureId)
        {
            if (textureId < 0)
                textureId = _resources.GetFallbackTexture();

            EnsureInitialized();
            BindShaderAndViewport(_ui2DTextureShader);
            GL.BindVertexArray(_vaoTex);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.Uniform1(GL.GetUniformLocation(_ui2DTextureShader, "uTexture"), 0);

            var data = Geometry2DBuilder.BuildTexturedQuad(p0, p1, p2, p3);

            EnsureBufferCapacity(_vboTex, ref _vboTexCapacity, data.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Draws the circle outline.
        /// </summary>
        /// <param name="cx">The cx.</param>
        /// <param name="cy">The cy.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="segments">The segments.</param>
        /// <param name="color">The color.</param>
        public void DrawCircleOutline(
            float cx, float cy,
            float radius,
            int segments,
            (int r, int g, int b, int a) color)
        {
            var data = Geometry2DBuilder.BuildCircleOutline(cx, cy, radius, segments, color);
            if (data.Length == 0) return;

            DrawColoredLines(data);
        }

        /// <summary>
        /// Draws the solid circle.
        /// </summary>
        /// <param name="cx">The cx.</param>
        /// <param name="cy">The cy.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="segments">The segments.</param>
        /// <param name="fill">The fill.</param>
        public void DrawSolidCircle(
            float cx, float cy,
            float radius,
            int segments,
            (int r, int g, int b, int a) fill)
        {
            DrawSolidEllipse(cx, cy, radius, radius, segments, fill);
        }

        /// <summary>
        /// Draws the textured circle.
        /// </summary>
        /// <param name="cx">The cx.</param>
        /// <param name="cy">The cy.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="segments">The segments.</param>
        /// <param name="textureId">The texture identifier.</param>
        public void DrawTexturedCircle(
            float cx, float cy,
            float radius,
            int segments,
            int textureId)
        {
            DrawTexturedEllipse(cx, cy, radius, radius, segments, textureId);
        }

        /// <summary>
        /// Draws the solid ellipse.
        /// </summary>
        /// <param name="cx">The cx.</param>
        /// <param name="cy">The cy.</param>
        /// <param name="radiusX">The radius x.</param>
        /// <param name="radiusY">The radius y.</param>
        /// <param name="segments">The segments.</param>
        /// <param name="fill">The fill.</param>
        public void DrawSolidEllipse(
            float cx, float cy,
            float radiusX, float radiusY,
            int segments,
            (int r, int g, int b, int a) fill)
        {
            EnsureInitialized();
            BindShaderAndViewport(_ui2DColorShader);
            GL.BindVertexArray(_vaoSolid);

            var r = fill.r / 255f;
            var g = fill.g / 255f;
            var b = fill.b / 255f;
            var a = fill.a / 255f;

            var data = new float[segments * 3 * 6];
            var idx = 0;

            var step = MathF.PI * 2f / segments;

            for (var i = 0; i < segments; i++)
            {
                var a0 = i * step;
                var a1 = (i + 1) * step;

                var x0 = cx + MathF.Cos(a0) * radiusX;
                var y0 = cy + MathF.Sin(a0) * radiusY;
                var x1 = cx + MathF.Cos(a1) * radiusX;
                var y1 = cy + MathF.Sin(a1) * radiusY;

                // center
                data[idx++] = cx;
                data[idx++] = cy;
                data[idx++] = r;
                data[idx++] = g;
                data[idx++] = b;
                data[idx++] = a;

                // p0
                data[idx++] = x0;
                data[idx++] = y0;
                data[idx++] = r;
                data[idx++] = g;
                data[idx++] = b;
                data[idx++] = a;

                // p1
                data[idx++] = x1;
                data[idx++] = y1;
                data[idx++] = r;
                data[idx++] = g;
                data[idx++] = b;
                data[idx++] = a;
            }

            // Ensure buffer capacity before uploading
            EnsureBufferCapacity(_vboSolid, ref _vboSolidCapacity, idx);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, idx * sizeof(float), data);
            GL.DrawArrays(PrimitiveType.Triangles, 0, segments * 3);

            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Draws a text string overlay on screen-space coordinates.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="color">The color.</param>
        /// <param name="fontSize">Size of the font.</param>
        public void DrawText(string text, float x, float y, (int r, int g, int b, int a) color, int fontSize = 13)
        {
            if (string.IsNullOrEmpty(text)) return;

            EnsureInitialized();
            EnsureFontInitialized();

            BindShaderAndViewport(_ui2DTextureShader);
            GL.BindVertexArray(_vaoTex);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _fontTextureId);
            GL.Uniform1(GL.GetUniformLocation(_ui2DTextureShader, "uTexture"), 0);

            // --- NEW: Apply the color parameter via uniform ---
            // Make sure "uColor" matches the uniform name in your fragment shader
            int colorLoc = GL.GetUniformLocation(_ui2DTextureShader, "uColor");
            if (colorLoc >= 0)
            {
                GL.Uniform4(colorLoc, color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f);
            }

            // --- Retrieve the actual texture bounds ---
            // Note: For better performance, store actualAtlasW and actualAtlasH as class 
            // fields when you initially generate/load _fontTextureId.
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureWidth, out int actualAtlasW);
            GL.GetTexLevelParameter(TextureTarget.Texture2D, 0, GetTextureParameter.TextureHeight, out int actualAtlasH);

            //config variables
            const float charSpacing = 1.0f; // Increase this to spread letters apart
            const float verticalOffset = 0.0f; // Adjust this if the font is floating too high/low
            const float inset = 0.25f; // Prevents texture bleeding by shrinking the UV map by half a texel


            // Fallback just in case the query fails, but use the real resolution first
            var atlasW = actualAtlasW > 0 ? (float)actualAtlasW : (FontCellWidth * 16f);
            var atlasH = actualAtlasH > 0 ? (float)actualAtlasH : (FontCellHeight * 6f);

            var vertexData = new float[text.Length * 24];
            var idx = 0;

            var scale = (float)fontSize / FontCellHeight;
            var currentX = x;
            var currentY = y;

            foreach (var ch in text)
            {
                var ascii = (int)ch;
                if (ascii < 32 || ascii > 126) ascii = 63; // Fallback to '?'

                // Handle newlines
                if (ch == '\n')
                {
                    currentX = x; // Reset to start of line
                    currentY += (FontCellHeight * scale); // Move down one full line height
                    continue;
                }


                var charIdx = ascii - 32;
                var col = charIdx % 16;
                var row = charIdx / 16;

                // Calculate pixel coordinates for the sub-rectangle
                var pixelLeft = (col * FontCellWidth) + inset;
                var pixelRight = ((col + 1) * FontCellWidth) - inset;
                var pixelTop = (row * FontCellHeight) + inset;
                var pixelBottom = ((row + 1) * FontCellHeight) - inset;

                // Map to UV space
                // Compute exact horizontal texture boundaries using TRUE atlas bounds
                var u0 = pixelLeft / atlasW;
                var u1 = pixelRight / atlasW;

                // Invert V coordinates (1.0f - ...) to flip the texture right-side up!
                var v0 = 1.0f - (pixelTop / atlasH);
                var v1 = 1.0f - (pixelBottom / atlasH);

                // Physical geometry - Ensure y1 covers the full height
                var x0 = currentX;
                var y0 = currentY + verticalOffset;
                var x1 = currentX + (FontCellWidth * scale);
                var y1 = currentY + (FontCellHeight * scale) + verticalOffset;

                // --- Triangle 1 (CCW) ---
                vertexData[idx++] = x0; vertexData[idx++] = y0; vertexData[idx++] = u0; vertexData[idx++] = v0; // Top-Left
                vertexData[idx++] = x0; vertexData[idx++] = y1; vertexData[idx++] = u0; vertexData[idx++] = v1; // Bottom-Left
                vertexData[idx++] = x1; vertexData[idx++] = y1; vertexData[idx++] = u1; vertexData[idx++] = v1; // Bottom-Right

                // --- Triangle 2 (CCW) ---
                vertexData[idx++] = x0; vertexData[idx++] = y0; vertexData[idx++] = u0; vertexData[idx++] = v0; // Top-Left
                vertexData[idx++] = x1; vertexData[idx++] = y1; vertexData[idx++] = u1; vertexData[idx++] = v1; // Bottom-Right
                vertexData[idx++] = x1; vertexData[idx++] = y0; vertexData[idx++] = u1; vertexData[idx++] = v0; // Top-Right

                //adjust spacing
                currentX += (FontCellWidth * scale) + charSpacing;
            }

            // --- 1. SAVE & ISOLATE STATES FOR 2D OVERLAY ---
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Ensure your buffer capacity and bind your structures as normal
            EnsureBufferCapacity(_vboTex, ref _vboTexCapacity, vertexData.Length);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertexData.Length * sizeof(float), vertexData);

            // --- 2. EXECUTE DRAW ---
            GL.DrawArrays(PrimitiveType.Triangles, 0, text.Length * 6);

            // --- 3. CLEAN UP & RESTORE STATES FOR 3D ENGINE ---
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        /// <summary>
        /// Draws the textured ellipse.
        /// </summary>
        /// <param name="cx">The cx.</param>
        /// <param name="cy">The cy.</param>
        /// <param name="radiusX">The radius x.</param>
        /// <param name="radiusY">The radius y.</param>
        /// <param name="segments">The segments.</param>
        /// <param name="textureId">The texture identifier.</param>
        public void DrawTexturedEllipse(
            float cx, float cy,
            float radiusX, float radiusY,
            int segments,
            int textureId)
        {
            if (textureId < 0)
                textureId = _resources.GetFallbackTexture();

            EnsureInitialized();
            BindShaderAndViewport(_ui2DTextureShader);
            GL.BindVertexArray(_vaoTex);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.Uniform1(GL.GetUniformLocation(_ui2DTextureShader, "uTexture"), 0);

            var data = new float[segments * 3 * 4];
            var idx = 0;

            var step = MathF.PI * 2f / segments;

            for (var i = 0; i < segments; i++)
            {
                // (Vertex calculation logic stays exactly the same)
                var a0 = i * step;
                var a1 = (i + 1) * step;

                var x0 = cx + MathF.Cos(a0) * radiusX;
                var y0 = cy + MathF.Sin(a0) * radiusY;
                var x1 = cx + MathF.Cos(a1) * radiusX;
                var y1 = cy + MathF.Sin(a1) * radiusY;

                var u0 = MathF.Cos(a0) * 0.5f + 0.5f;
                var v0 = MathF.Sin(a0) * 0.5f + 0.5f;
                var u1 = MathF.Cos(a1) * 0.5f + 0.5f;
                var v1 = MathF.Sin(a1) * 0.5f + 0.5f;

                data[idx++] = cx;
                data[idx++] = cy;
                data[idx++] = 0.5f;
                data[idx++] = 0.5f;
                data[idx++] = x0;
                data[idx++] = y0;
                data[idx++] = u0;
                data[idx++] = v0;
                data[idx++] = x1;
                data[idx++] = y1;
                data[idx++] = u1;
                data[idx++] = v1;
            }

            EnsureBufferCapacity(_vboTex, ref _vboTexCapacity, idx);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, idx * sizeof(float), data);
            GL.DrawArrays(PrimitiveType.Triangles, 0, segments * 3);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Blits a texture directly to the full viewport (hardware scanout).
        /// Pixel-perfect. No scaling artifacts.
        /// </summary>
        /// <param name="idx">The texture identifier.</param>
        public void DrawFullscreenQuad(int idx)
        {
            EnsureInitialized();

            BindShaderAndViewport(_ui2DTextureShader);
            GL.BindVertexArray(_vaoTex);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, idx);
            GL.Uniform1(GL.GetUniformLocation(_ui2DTextureShader, "uTexture"), 0);

            float w = Width;
            float h = Height;

            var data = Geometry2DBuilder.BuildTexturedQuad(
                (0, 0),
                ((int)w, 0),
                ((int)w, (int)h),
                (0, (int)h));

            EnsureBufferCapacity(_vboTex, ref _vboTexCapacity, data.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Flushes the batch.
        /// </summary>
        /// <param name="batch">The batch.</param>
        private unsafe void FlushBatch(RenderBatch batch)
        {
            EnsureInitialized();

            // --- Solid Lines ---
            if (batch.SolidLineVertices.Length > 0) // Use .Length instead of .Count
            {
                BindShaderAndViewport(_ui2DColorShader);
                GL.BindVertexArray(_vaoSolid);

                EnsureBufferCapacity(_vboSolid, ref _vboSolidCapacity, batch.SolidLineVertices.Length);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);

                // ✨ THE MAGIC HAPPENS HERE ✨
                // We pass the raw pointer directly to OpenGL. Zero C# array allocations!
                GL.BufferSubData(
                    BufferTarget.ArrayBuffer,
                    IntPtr.Zero,
                    batch.SolidLineVertices.Length * sizeof(float),
                    (IntPtr)batch.SolidLineVertices.Pointer);

                GL.DrawArrays(PrimitiveType.Lines, 0, batch.SolidLineVertices.Length / 6);

                GL.BindVertexArray(0);
            }

            // --- Solid Triangles/Quads ---
            if (batch.SolidTriangleVertices.Length > 0)
            {
                BindShaderAndViewport(_ui2DColorShader);

                GL.BindVertexArray(_vaoSolid);

                EnsureBufferCapacity(_vboSolid, ref _vboSolidCapacity, batch.SolidTriangleVertices.Length);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);

                // Pass the raw pointer directly!
                GL.BufferSubData(
                    BufferTarget.ArrayBuffer,
                    IntPtr.Zero,
                    batch.SolidTriangleVertices.Length * sizeof(float),
                    (IntPtr)batch.SolidTriangleVertices.Pointer);

                GL.DrawArrays(PrimitiveType.Triangles, 0, batch.SolidTriangleVertices.Length / 6);

                GL.BindVertexArray(0);
            }

            // --- Textured Vertices ---
            if (batch.TexturedBatches.Count > 0)
            {
                BindShaderAndViewport(_ui2DTextureShader);
                GL.BindVertexArray(_vaoTex);

                // Loop through each texture group
                foreach (var kvp in batch.TexturedBatches)
                {
                    var texId = kvp.Key;
                    var dataList = kvp.Value;

                    if (dataList.Count == 0) continue;

                    var data = dataList.ToArray();
                    EnsureBufferCapacity(_vboTex, ref _vboTexCapacity, data.Length);

                    // Bind the specific texture for this batch
                    GL.ActiveTexture(TextureUnit.Texture0);

                    // Use checkerboard if the texture ID is invalid (< 0)
                    GL.BindTexture(TextureTarget.Texture2D, texId < 0 ? _resources.GetFallbackTexture() : texId);
                    GL.Uniform1(GL.GetUniformLocation(_ui2DTextureShader, "uTexture"), 0);

                    // Upload and draw
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, data.Length / 4);
                }

                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindVertexArray(0);
            }
        }

        /// <summary>
        /// Reads the current framebuffer into an <see cref="UnmanagedImageBuffer" />.
        /// This performs a GPU → CPU read back of the active framebuffer.
        /// Result is top-left origin (software style).
        /// </summary>
        /// <returns>My UnmanagedBuffer Image implementation</returns>
        public UnmanagedImageBuffer CaptureFrame()
        {
            EnsureInitialized();

            var w = Width;
            var h = Height;

            var buffer = new UnmanagedImageBuffer(w, h);

            // OpenGL gives bottom-left origin, so we read and flip.
            var byteCount = w * h * UnmanagedImageBuffer.BytesPerPixel;
            var temp = new byte[byteCount];

            GL.ReadPixels(
                0,
                0,
                w,
                h,
                PixelFormat.Bgra,
                PixelType.UnsignedByte,
                temp);

            var dst = buffer.BufferSpan;

            var rowSize = w * UnmanagedImageBuffer.BytesPerPixel;

            // Flip Y while copying
            for (var y = 0; y < h; y++)
            {
                var srcRow = (h - 1 - y) * rowSize;
                var dstRow = y * rowSize;

                temp.AsSpan(srcRow, rowSize).CopyTo(dst.Slice(dstRow, rowSize));
            }

            return buffer;
        }

        /// <summary>
        /// Uploads an <see cref="UnmanagedImageBuffer" /> as a 2D texture and returns the OpenGL texture ID.
        /// </summary>
        /// <param name="image">Source image buffer (must match RGBA8888 format).</param>
        /// <param name="linearFilter">Whether to use linear filtering (true) or nearest (false).</param>
        /// <returns>
        /// OpenGL texture ID.
        /// </returns>
        public int UploadImage(UnmanagedImageBuffer image, bool linearFilter = false)
        {
            EnsureInitialized();

            var texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texId);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                image.Width,
                image.Height,
                0,
                PixelFormat.Bgra,
                PixelType.UnsignedByte,
                image.Buffer);

            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (int)(linearFilter ? TextureMinFilter.Linear : TextureMinFilter.Nearest));

            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (int)(linearFilter ? TextureMagFilter.Linear : TextureMagFilter.Nearest));

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            return texId;
        }

        /// <summary>
        /// Ensures the buffer capacity.
        /// </summary>
        /// <param name="vbo">The vbo.</param>
        /// <param name="currentCapacity">The current capacity.</param>
        /// <param name="requiredFloats">The required floats.</param>
        private void EnsureBufferCapacity(int vbo, ref int currentCapacity, int requiredFloats)
        {
            if (requiredFloats <= currentCapacity) return;

            // Double capacity until it fits
            while (currentCapacity < requiredFloats)
            {
                currentCapacity *= 2;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, currentCapacity * sizeof(float), IntPtr.Zero,
                BufferUsageHint.DynamicDraw);
        }

        /// <summary>
        /// Binds the shader and viewport.
        /// </summary>
        /// <param name="shaderId">The shader identifier.</param>
        private void BindShaderAndViewport(int shaderId)
        {
            GL.UseProgram(shaderId);
            GL.Uniform2(GL.GetUniformLocation(shaderId, "uViewport"), Width, (float)Height);
        }

        /// <summary>
        /// Generates a static ASCII character texture sheet mapping glyph indices 32 through 126.
        /// Formatted natively to match your BGRA/RGBA hardware layout.
        /// </summary>
        private void EnsureFontInitialized()
        {
            if (_fontTextureId >= 0) return;

            const int atlasW = FontCellWidth * 16;
            const int atlasH = FontCellHeight * 6;

            using var bitmap =
                new System.Drawing.Bitmap(atlasW, atlasH, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.Clear(System.Drawing.Color.Transparent);
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                // Scaled down to 11f to ensure characters sit perfectly inside the 12x24 pixel limits
                using var font = new System.Drawing.Font("Consolas", 11f, System.Drawing.FontStyle.Regular);
                using var brush = new System.Drawing.SolidBrush(System.Drawing.Color.White);

                // CRITICAL FIX: Strips out layout margins so characters align strictly to grid squares
                var strictFormat = System.Drawing.StringFormat.GenericTypographic;

                var charIdx = 32;
                for (var row = 0; row < 6; row++)
                {
                    for (var col = 0; col < 16; col++)
                    {
                        if (charIdx > 126) break;

                        var symbol = ((char)charIdx).ToString();
                        g.DrawString(symbol, font, brush, col * FontCellWidth, row * FontCellHeight, strictFormat);
                        charIdx++;
                    }
                }
            }

            var imgBuffer = new UnmanagedImageBuffer(atlasW, atlasH);
            var rect = new System.Drawing.Rectangle(0, 0, atlasW, atlasH);
            var bmpData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            unsafe
            {
                var srcSpan = new ReadOnlySpan<byte>((void*)bmpData.Scan0, atlasW * atlasH * 4);
                srcSpan.CopyTo(imgBuffer.BufferSpan);
            }

            //Debug: Save the font atlas to a PNG file for verification
            //bitmap.Save(@"font_atlas_diagnostic.png", System.Drawing.Imaging.ImageFormat.Png);
            bitmap.UnlockBits(bmpData);

            _fontTextureId = UploadImage(imgBuffer, linearFilter: false);
        }

        /// <summary>
        /// Disposes all OpenGL resources.
        /// </summary>
        public void Dispose()
        {
            if (!_initialized) return;

            GL.DeleteBuffer(_vboSolid);
            GL.DeleteVertexArray(_vaoSolid);
            GL.DeleteBuffer(_vboTex);
            GL.DeleteVertexArray(_vaoTex);

            if (_fallbackTextureId >= 0)
            {
                GL.DeleteTexture(_fallbackTextureId);
            }
        }
    }
}