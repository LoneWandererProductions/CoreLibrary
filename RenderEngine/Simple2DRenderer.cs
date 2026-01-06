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
        private readonly int _width;

        /// <summary>
        /// The height of the viewport.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="Simple2DRenderer"/> class.
        /// </summary>
        /// <param name="width">Width of the render viewport in pixels.</param>
        /// <param name="height">Height of the render viewport in pixels.</param>
        /// <param name="resources">The GL resource manager instance.</param>
        public Simple2DRenderer(int width, int height, GlResourceManager resources)
        {
            _width = width;
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
            GL.BufferData(BufferTarget.ArrayBuffer, 1024 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

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
            GL.BufferData(BufferTarget.ArrayBuffer, 1024 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0); // Position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float),
                2 * sizeof(float)); // Texcoord
            GL.EnableVertexAttribArray(1);
            GL.BindVertexArray(0);

            // --- Shaders ---
            _ui2DColorShader = CompileShader(Ui2DColorVertexShader(), SoliUi2DColorFragmentShaderdFragment());
            _ui2DTextureShader = CompileShader(Ui2DTextureVertexShader(), Ui2DTextureFragmentShader());

            _initialized = true;
        }

        #region Shaders

        /// <summary>
        /// Vertex shader for 2D solid color quads/lines.
        /// </summary>
        /// <param name="flipY">If true, flips Y to match OpenGL bottom-left origin.
        /// If false, keeps Y increasing downward like software renderer.</param>
        private string Ui2DColorVertexShader(bool flipY = true) => $@"
            #version 410 core
            layout(location = 0) in vec2 aPos;
            layout(location = 1) in vec4 aColor;
            out vec4 vColor;
            void main() {{
                vec2 pos = aPos / vec2({_width},{Height}) * 2.0 - 1.0;
                {(flipY ? "pos.y = -pos.y;" : "")}   // flip only if needed
                gl_Position = vec4(pos, 0.0, 1.0);
                vColor = aColor;
            }}";

        /// <summary>
        /// Fragment shader for 2D solid color quads/lines.
        /// </summary>
        private string SoliUi2DColorFragmentShaderdFragment() => @"
            #version 410 core
            in vec4 vColor;
            out vec4 FragColor;
            void main() { FragColor = vColor; }";

        /// <summary>
        /// Vertex shader for textured quads.
        /// </summary>
        /// <summary>
        /// Vertex shader for 2D textured quads.
        /// </summary>
        /// <param name="flipY">If true, flips Y and optionally V coordinate to match OpenGL default.</param>
        private string Ui2DTextureVertexShader(bool flipY = true) => $@"
            #version 410 core
            layout(location = 0) in vec2 aPos;
            layout(location = 1) in vec2 aTex;
            out vec2 vTex;
            void main() {{
                vec2 pos = aPos / vec2({_width},{Height}) * 2.0 - 1.0;
                {(flipY ? "pos.y = -pos.y;" : "")} // flip Y if needed
                gl_Position = vec4(pos, 0.0, 1.0);
                vTex = {(flipY ? "vec2(aTex.x, 1.0 - aTex.y)" : "aTex")};
            }}";

        /// <summary>
        /// Fragment shader for textured quads.
        /// </summary>
        private string Ui2DTextureFragmentShader() => @"
            #version 410 core
            in vec2 vTex;
            uniform sampler2D uTexture;
            out vec4 FragColor;
            void main() { FragColor = texture(uTexture, vTex); }";

        #endregion

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

        #region DrawMethods

        /// <summary>
        /// Draws lines with color. Points as (x,y,r,g,b,a).
        /// </summary>
        public void DrawColoredLines((float x, float y, int r, int g, int b, int a)[] points)
        {
            EnsureInitialized();
            GL.UseProgram(_ui2DColorShader);
            GL.BindVertexArray(_vaoSolid);

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

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
            GL.DrawArrays(PrimitiveType.Lines, 0, points.Length);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Draws a solid colored quad using 4 points.
        /// </summary>
        public void DrawSolidQuad((int x, int y) p0, (int x, int y) p1, (int x, int y) p2, (int x, int y) p3,
            (int r, int g, int b, int a) fill)
        {
            EnsureInitialized();
            GL.UseProgram(_ui2DColorShader);
            GL.BindVertexArray(_vaoSolid);

            var r = fill.r / 255f;
            var g = fill.g / 255f;
            var b = fill.b / 255f;
            var a = fill.a / 255f;

            var data = new float[]
            {
                p0.x, p0.y, r, g, b, a, p1.x, p1.y, r, g, b, a, p2.x, p2.y, r, g, b, a, p2.x, p2.y, r, g, b, a,
                p3.x, p3.y, r, g, b, a, p0.x, p0.y, r, g, b, a
            };

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

            var data = new float[3 * 6]; // 3 vertices, 6 floats each
            var points = new[] { p0, p1, p2 };

            for (var i = 0; i < 3; i++)
            {
                data[i * 6 + 0] = points[i].x;
                data[i * 6 + 1] = points[i].y;
                data[i * 6 + 2] = points[i].r / 255f;
                data[i * 6 + 3] = points[i].g / 255f;
                data[i * 6 + 4] = points[i].b / 255f;
                data[i * 6 + 5] = points[i].a / 255f;
            }

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
            // if textureId is -1, create a checkerboard texture internally
            if (textureId < 0)
                textureId = CreateCheckerboardTexture();

            EnsureInitialized();
            GL.UseProgram(_ui2DTextureShader);
            GL.BindVertexArray(_vaoTex);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.Uniform1(GL.GetUniformLocation(_ui2DTextureShader, "uTexture"), 0);

            var data = new float[]
            {
                // x, y, u, v
                p0.x, p0.y, 0f, 0f, p1.x, p1.y, 1f, 0f, p2.x, p2.y, 0.5f, 1f // UV mapping for triangle
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
        }


        /// <summary>
        /// Draws a textured quad using 4 points.
        /// </summary>
        public void DrawTexturedQuad((int x, int y) p0, (int x, int y) p1, (int x, int y) p2, (int x, int y) p3,
            int textureId)
        {
            // if textureId is -1, create a checkerboard texture internally
            if (textureId < 0)
                textureId = CreateCheckerboardTexture();

            EnsureInitialized();
            GL.UseProgram(_ui2DTextureShader);
            GL.BindVertexArray(_vaoTex);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.Uniform1(GL.GetUniformLocation(_ui2DTextureShader, "uTexture"), 0);

            var data = new float[]
            {
                p0.x, p0.y, 0f, 0f, p1.x, p1.y, 1f, 0f, p2.x, p2.y, 1f, 1f, p2.x, p2.y, 1f, 1f, p3.x, p3.y, 0f, 1f,
                p0.x, p0.y, 0f, 0f
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Blits a texture directly to the full viewport (hardware scanout).
        /// Pixel-perfect. No scaling artifacts.
        /// </summary>
        /// <param name="textureId">The texture identifier.</param>
        public void DrawFullscreenQuad(int textureId)
        {
            EnsureInitialized();

            GL.UseProgram(_ui2DTextureShader);
            GL.BindVertexArray(_vaoTex);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.Uniform1(GL.GetUniformLocation(_ui2DTextureShader, "uTexture"), 0);

            // Fullscreen quad in pixel space
            float w = _width;
            float h = Height;

            var data = new float[] { 0, 0, 0, 0, w, 0, 1, 0, w, h, 1, 1, w, h, 1, 1, 0, h, 0, 1, 0, 0, 0, 0 };

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
        }


        /// <summary>
        /// Creates a small 2x2 checkerboard texture.
        /// </summary>
        public int CreateCheckerboardTexture()
        {
            EnsureInitialized();
            byte[] pixels = new byte[] { 0, 0, 0, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 0, 255 };

            int texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 2, 2, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Nearest);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            return texId;
        }

        #endregion

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
        }
    }
}
