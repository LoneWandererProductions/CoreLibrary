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
 * TODO:        - Add dynamic resizing support
 *              - Consider batching multiple quads/lines for performance
 *              - Add optional Y-flip mode for top-left origin
 *              - Add text rendering
 */

using OpenTK.Graphics.OpenGL4;
using System;

namespace RenderEngine
{
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
        public int Width { get; }

        /// <summary>
        /// The height of the viewport.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int Height { get; }

        private readonly GlResourceManager _resources;

        // Solid VAO/VBO
        private int _vaoSolid;
        private int _vboSolid;

        // Textured VAO/VBO
        private int _vaoTex;
        private int _vboTex;

        private int _ui2DColorShader;
        private int _ui2DTextureShader;

        private bool _initialized;

        private int _vboSolidCapacity = 4096; // Start with a more reasonable size
        private int _vboTexCapacity = 4096;
        private int _fallbackTextureId = -1; // Caches the checkerboard texture

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
            GL.BufferData(BufferTarget.ArrayBuffer, _vboSolidCapacity * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

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
            GL.BufferData(BufferTarget.ArrayBuffer, _vboTexCapacity * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0); // Position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float),
                2 * sizeof(float)); // Texcoord
            GL.EnableVertexAttribArray(1);
            GL.BindVertexArray(0);

            // --- Shaders ---
            _ui2DColorShader = CompileShader(
                Ui2DShaders.ColorVertex(Width, Height, true),
                Ui2DShaders.ColorFragment());

            _ui2DTextureShader = CompileShader(
                Ui2DShaders.TextureVertex(Width, Height, true),
                Ui2DShaders.TextureFragment());

            _initialized = true;
        }

        /// <summary>
        /// Compiles vertex and fragment shader sources into a GL program.
        /// </summary>
        private int CompileShader(string vertexSrc, string fragmentSrc)
        {
            var v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vertexSrc);
            GL.CompileShader(v);
            GL.GetShader(v, ShaderParameter.CompileStatus, out var success);
            if (success == 0) throw new Exception(GL.GetShaderInfoLog(v));

            var f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fragmentSrc);
            GL.CompileShader(f);
            GL.GetShader(f, ShaderParameter.CompileStatus, out success);
            if (success == 0) throw new Exception(GL.GetShaderInfoLog(f));

            var program = GL.CreateProgram();
            GL.AttachShader(program, v);
            GL.AttachShader(program, f);
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out success);
            if (success == 0) throw new Exception(GL.GetProgramInfoLog(program));

            GL.DeleteShader(v);
            GL.DeleteShader(f);
            return program;
        }

        public void Flush(RenderBatch batch)
        {
            EnsureInitialized();
            FlushBatch(batch);
        }

        public void DrawLine(
            float x0, float y0,
            float x1, float y1,
            (int r, int g, int b, int a) color)
        {
            DrawColoredLines(new[]
            {
                (x0, y0, color.r, color.g, color.b, color.a),
                (x1, y1, color.r, color.g, color.b, color.a),
            });
        }

        public void DrawRectOutline(
            float x, float y,
            float w, float h,
            (int r, int g, int b, int a) color)
        {
            var data = Geometry2DBuilder.BuildRectOutline(x, y, w, h, color);
            if (data.Length == 0) return;

            DrawColoredLines(data);
        }

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
            GL.UseProgram(_ui2DColorShader);
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
            GL.UseProgram(_ui2DColorShader);
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
            GL.UseProgram(_ui2DColorShader);
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
                textureId = GetOrCreateCheckerboardTexture();

            EnsureInitialized();
            GL.UseProgram(_ui2DTextureShader);
            GL.BindVertexArray(_vaoTex);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.Uniform1(GL.GetUniformLocation(_ui2DTextureShader, "uTexture"), 0);

            var data = new[]
            {
                p0.x, p0.y, 0f, 0f,
                p1.x, p1.y, 1f, 0f,
                p2.x, p2.y, 0.5f, 1f
            };

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
                textureId = GetOrCreateCheckerboardTexture();

            EnsureInitialized();
            GL.UseProgram(_ui2DTextureShader);
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

        public void DrawSolidCircle(
            float cx, float cy,
            float radius,
            int segments,
            (int r, int g, int b, int a) fill)
        {
            DrawSolidEllipse(cx, cy, radius, radius, segments, fill);
        }

        public void DrawTexturedCircle(
            float cx, float cy,
            float radius,
            int segments,
            int textureId)
        {
            DrawTexturedEllipse(cx, cy, radius, radius, segments, textureId);
        }

        public void DrawSolidEllipse(
            float cx, float cy,
            float radiusX, float radiusY,
            int segments,
            (int r, int g, int b, int a) fill)
        {
            EnsureInitialized();
            GL.UseProgram(_ui2DColorShader);
            GL.BindVertexArray(_vaoSolid);

            float r = fill.r / 255f;
            float g = fill.g / 255f;
            float b = fill.b / 255f;
            float a = fill.a / 255f;

            var data = new float[segments * 3 * 6];
            int idx = 0;

            float step = MathF.PI * 2f / segments;

            for (int i = 0; i < segments; i++)
            {
                float a0 = i * step;
                float a1 = (i + 1) * step;

                float x0 = cx + MathF.Cos(a0) * radiusX;
                float y0 = cy + MathF.Sin(a0) * radiusY;
                float x1 = cx + MathF.Cos(a1) * radiusX;
                float y1 = cy + MathF.Sin(a1) * radiusY;

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

        public void DrawTexturedEllipse(
            float cx, float cy,
            float radiusX, float radiusY,
            int segments,
            int textureId)
        {
            if (textureId < 0)
                textureId = GetOrCreateCheckerboardTexture();

            EnsureInitialized();
            GL.UseProgram(_ui2DTextureShader);
            GL.BindVertexArray(_vaoTex);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.Uniform1(GL.GetUniformLocation(_ui2DTextureShader, "uTexture"), 0);

            var data = new float[segments * 3 * 4];
            int idx = 0;

            float step = MathF.PI * 2f / segments;

            for (int i = 0; i < segments; i++)
            {
                // (Vertex calculation logic stays exactly the same)
                float a0 = i * step;
                float a1 = (i + 1) * step;

                float x0 = cx + MathF.Cos(a0) * radiusX;
                float y0 = cy + MathF.Sin(a0) * radiusY;
                float x1 = cx + MathF.Cos(a1) * radiusX;
                float y1 = cy + MathF.Sin(a1) * radiusY;

                float u0 = MathF.Cos(a0) * 0.5f + 0.5f;
                float v0 = MathF.Sin(a0) * 0.5f + 0.5f;
                float u1 = MathF.Cos(a1) * 0.5f + 0.5f;
                float v1 = MathF.Sin(a1) * 0.5f + 0.5f;

                data[idx++] = cx; data[idx++] = cy; data[idx++] = 0.5f; data[idx++] = 0.5f;
                data[idx++] = x0; data[idx++] = y0; data[idx++] = u0; data[idx++] = v0;
                data[idx++] = x1; data[idx++] = y1; data[idx++] = u1; data[idx++] = v1;
            }

            // FIXED: Using correct texture VBO and capacity
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

            GL.UseProgram(_ui2DTextureShader);
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

        private void FlushBatch(RenderBatch batch)
        {
            EnsureInitialized();

            // --- Solid Lines ---
            if (batch.SolidLineVertices.Count > 0)
            {
                GL.UseProgram(_ui2DColorShader);
                GL.BindVertexArray(_vaoSolid);

                var data = batch.SolidLineVertices.ToArray();
                EnsureBufferCapacity(_vboSolid, ref _vboSolidCapacity, data.Length);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
                GL.DrawArrays(PrimitiveType.Lines, 0, data.Length / 6);

                GL.BindVertexArray(0);
            }

            // --- Solid Triangles/Quads ---
            if (batch.SolidTriangleVertices.Count > 0)
            {
                GL.UseProgram(_ui2DColorShader);
                GL.BindVertexArray(_vaoSolid);

                var data = batch.SolidTriangleVertices.ToArray();
                EnsureBufferCapacity(_vboSolid, ref _vboSolidCapacity, data.Length);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
                GL.DrawArrays(PrimitiveType.Triangles, 0, data.Length / 6);

                GL.BindVertexArray(0);
            }

            // --- Textured Vertices ---
            if (batch.TexturedBatches.Count > 0)
            {
                GL.UseProgram(_ui2DTextureShader);
                GL.BindVertexArray(_vaoTex);

                // Loop through each texture group
                foreach (var kvp in batch.TexturedBatches)
                {
                    int texId = kvp.Key;
                    var dataList = kvp.Value;

                    if (dataList.Count == 0) continue;

                    var data = dataList.ToArray();
                    EnsureBufferCapacity(_vboTex, ref _vboTexCapacity, data.Length);

                    // Bind the specific texture for this batch
                    GL.ActiveTexture(TextureUnit.Texture0);

                    // Use checkerboard if the texture ID is invalid (< 0)
                    GL.BindTexture(TextureTarget.Texture2D, texId < 0 ? GetOrCreateCheckerboardTexture() : texId);
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
        /// This performs a GPU → CPU readback of the active framebuffer.
        /// Result is top-left origin (software style).
        /// </summary>
        /// <returns></returns>
        public UnmanagedImageBuffer CaptureFrame()
        {
            EnsureInitialized();

            int w = Width;
            int h = Height;

            var buffer = new UnmanagedImageBuffer(w, h);

            // OpenGL gives bottom-left origin, so we read and flip.
            int byteCount = w * h * UnmanagedImageBuffer.BytesPerPixel;
            byte[] temp = new byte[byteCount];

            GL.ReadPixels(
                0,
                0,
                w,
                h,
                PixelFormat.Bgra,
                PixelType.UnsignedByte,
                temp);

            var dst = buffer.BufferSpan;

            int rowSize = w * UnmanagedImageBuffer.BytesPerPixel;

            // Flip Y while copying
            for (int y = 0; y < h; y++)
            {
                int srcRow = (h - 1 - y) * rowSize;
                int dstRow = y * rowSize;

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

            int texId = GL.GenTexture();
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

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            return texId;
        }

        private void EnsureBufferCapacity(int vbo, ref int currentCapacity, int requiredFloats)
        {
            if (requiredFloats <= currentCapacity) return;

            // Double capacity until it fits
            while (currentCapacity < requiredFloats)
            {
                currentCapacity *= 2;
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, currentCapacity * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }

        // FIX 5: Create once and cache
        public int GetOrCreateCheckerboardTexture()
        {
            EnsureInitialized();
            if (_fallbackTextureId >= 0) return _fallbackTextureId;

            byte[] pixels = new byte[]
            {
        0, 0, 0, 255, 255, 255, 255, 255,
        255, 255, 255, 255, 0, 0, 0, 255
            };

            _fallbackTextureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fallbackTextureId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 2, 2, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return _fallbackTextureId;
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

            GL.DeleteProgram(_ui2DColorShader);
            GL.DeleteProgram(_ui2DTextureShader);

            if (_fallbackTextureId >= 0)
            {
                GL.DeleteTexture(_fallbackTextureId);
            }
        }
    }
}
