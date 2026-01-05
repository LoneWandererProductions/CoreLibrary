/*
 * COPYRIGHT:   See COPYING in the top-level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/OpenTkControl.cs
 * PURPOSE:     OpenGL Viewer for WPF applications using OpenTKDrawHelper.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace RenderEngine
{
    /// <inheritdoc />
    /// <summary>
    /// OpenGL viewer control for WPF, using OpenTKDrawHelper for drawing primitives and textured quads.
    /// Supports background textures, optional skybox, column overlays, pixel overlays,
    /// and also accepts raw OpenGL-style vertex data (via Span&lt;float&gt; or Span&lt;byte&gt;).
    /// </summary>
    public sealed class OpenTkControl : WindowsFormsHost
    {
        private GLControl? _glControl;

        // Background texture ID
        private int _backgroundTexture = -1;

        // Skybox related
        private bool _enableSkybox;
        private int _skyboxShader;
        private int _skyboxTexture;
        private int _skyboxVao, _skyboxVbo;

        private Batched2DRenderer? _renderer;

        // VBO reuse for dynamic rendering
        private int _vbo;
        private int _vao;

        // Shader/resource manager
        private readonly GlResourceManager _resourceManager = new();

        public OpenTkControl()
        {
            if (!OpenTkHelper.IsOpenGlCompatible())
                throw new NotSupportedException("OpenGL not supported on this system.");

            InitializeGlControl();
            Child = _glControl;
        }

        /// <summary>
        /// Enable or disable the skybox rendering.
        /// </summary>
        public bool EnableSkybox
        {
            get => _enableSkybox;
            set
            {
                _enableSkybox = value;
                _glControl?.Invalidate();
            }
        }

        /// <summary>
        /// Initialize the GLControl and set up OpenGL context.
        /// </summary>
        private void InitializeGlControl()
        {
            _glControl = new GLControl();
            _glControl.HandleCreated += (s, e) =>
            {
                _glControl.MakeCurrent();

                // Set default clear color and enable depth testing
                GL.ClearColor(0.1f, 0.2f, 0.3f, 1f);
                GL.Enable(EnableCap.DepthTest);

                // Initialize helper
                _renderer = new Batched2DRenderer(_glControl.Width, _glControl.Height); //, _resourceManager);

                // Load a default background texture
                _backgroundTexture = OpenTkHelper.LoadTextureFromFile("background.jpg");

                // Initialize skybox (can be toggled later)
                InitializeSkybox();

                // Create reusable VAO/VBO for dynamic primitives
                _vao = GL.GenVertexArray();
                _vbo = GL.GenBuffer();
            };

            _glControl.Paint += GlControl_Paint;
            _glControl.Resize += GlControl_Resize;
        }

        /// <summary>
        /// Initializes the skybox shaders and cubemap.
        /// </summary>
        private void InitializeSkybox()
        {
            _skyboxShader = OpenTkHelper.LoadShader(RenderResource.ShaderSkyboxVertex,
                RenderResource.ShaderSkyboxFragment);

            _skyboxTexture = OpenTkHelper.LoadCubeMap(new[]
            {
                "right.jpg", "left.jpg", "top.jpg", "bottom.jpg", "front.jpg", "back.jpg"
            });

            _skyboxVao = GL.GenVertexArray();
            _skyboxVbo = GL.GenBuffer();

            GL.BindVertexArray(_skyboxVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _skyboxVbo);
            GL.BufferData(BufferTarget.ArrayBuffer,
                ShaderResource.SkyboxVertices.Length * sizeof(float),
                ShaderResource.SkyboxVertices,
                BufferUsageHint.StaticDraw);
        }

        /// <summary>
        /// Renders the skybox cubemap.
        /// </summary>
        private void RenderSkybox()
        {
            GL.DepthFunc(DepthFunction.Lequal);
            GL.UseProgram(_skyboxShader);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, _skyboxTexture);

            GL.BindVertexArray(_skyboxVao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            GL.DepthFunc(DepthFunction.Less);
        }

        /// <summary>
        /// GLControl Paint event handler.
        /// Clears the screen, draws background, optionally skybox, then swaps buffers.
        /// </summary>
        private void GlControl_Paint(object? sender, PaintEventArgs e)
        {
            if (_glControl == null) return;

            if (!_glControl.Context.IsCurrent) _glControl.MakeCurrent();

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Draw background
            if (_backgroundTexture != -1)
            {
                _renderer?.DrawTexturedQuad(
                    new Point(0, 0),
                    new Point(_glControl.Width, 0),
                    new Point(_glControl.Width, _glControl.Height),
                    new Point(0, _glControl.Height),
                    _backgroundTexture);
            }

            // Draw skybox if enabled
            if (_enableSkybox && _skyboxTexture != -1)
                RenderSkybox();

            _glControl.SwapBuffers();
        }

        /// <summary>
        /// GLControl Resize event handler. Updates the OpenGL viewport.
        /// </summary>
        private void GlControl_Resize(object? sender, EventArgs e)
        {
            if (_glControl == null) return;

            if (!_glControl.Context.IsCurrent) _glControl.MakeCurrent();
            GL.Viewport(0, 0, _glControl.Width, _glControl.Height);
        }

        /// <summary>
        /// Draws column overlays (e.g., debug rectangles) using the reusable VBO.
        /// </summary>
        public void RenderColumns(ColumnData[] columns, int screenWidth)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Render background first
            if (_backgroundTexture != -1)
                RenderBackground(_backgroundTexture);

            var vertexData = new float[columns.Length * 6 * 5]; // 6 vertices per quad, 5 floats per vertex
            var idx = 0;
            var columnWidth = (float)screenWidth / columns.Length;

            for (var i = 0; i < columns.Length; i++)
            {
                var col = columns[i];
                var x0 = i * columnWidth;
                var x1 = x0 + columnWidth;
                const float y0 = 0;
                var y1 = col.Height;

                // Two triangles per column
                var verts = new[,] { { x0, y0 }, { x1, y0 }, { x1, y1 }, { x1, y1 }, { x0, y1 }, { x0, y0 } };

                for (var v = 0; v < 6; v++)
                {
                    vertexData[idx++] = verts[v, 0];
                    vertexData[idx++] = verts[v, 1];
                    vertexData[idx++] = col.Color.X;
                    vertexData[idx++] = col.Color.Y;
                    vertexData[idx++] = col.Color.Z;
                }
            }

            RenderVertices(vertexData, 5, columns.Length * 6, PrimitiveType.Triangles);
        }

        /// <summary>
        /// Draws pixel overlays efficiently using OpenTKDrawHelper and converts Vector3 color to Color.
        /// </summary>
        public void RenderPixels(CoordinateData[] pixels, int screenWidth, int screenHeight)
        {
            if (_glControl == null) return;

            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Draw background first
            if (_backgroundTexture != -1)
            {
                _renderer?.DrawTexturedQuad(
                    new Point(0, 0),
                    new Point(screenWidth, 0),
                    new Point(screenWidth, screenHeight),
                    new Point(0, screenHeight),
                    _backgroundTexture);
            }

            foreach (var pixel in pixels)
            {
                var px = pixel.X;
                var py = screenHeight - pixel.Y;

                _renderer?.DrawSolidQuad(
                    new Point(px, py),
                    new Point(px + 1, py),
                    new Point(px + 1, py + 1),
                    new Point(px, py + 1),
                    pixel.Color);
            }

            _glControl.SwapBuffers();
        }

        /// <summary>
        /// Generic renderer for raw vertex data provided as float array/span.
        /// </summary>
        public void RenderVertices(ReadOnlySpan<float> vertexData, int stride, int vertexCount,
            PrimitiveType primitiveType)
        {
            if (_glControl == null) return;

            if (!_glControl.Context.IsCurrent) _glControl.MakeCurrent();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            unsafe
            {
                fixed (float* ptr = vertexData)
                {
                    GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), (IntPtr)ptr,
                        BufferUsageHint.DynamicDraw);
                }
            }

            // Example: first 2 floats = position, next 3 floats = color
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride * sizeof(float), 0);

            if (stride >= 5)
            {
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride * sizeof(float),
                    2 * sizeof(float));
            }

            GL.DrawArrays(primitiveType, 0, vertexCount);

            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        /// <summary>
        /// Generic renderer for raw vertex data provided as bytes.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="vertexCount">The vertex count.</param>
        /// <param name="primitiveType">Type of the primitive.</param>
        public void RenderRaw(ReadOnlySpan<byte> data, int vertexCount, PrimitiveType primitiveType)
        {
            if (_glControl == null) return;

            if (!_glControl.Context.IsCurrent) _glControl.MakeCurrent();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            unsafe
            {
                fixed (byte* ptr = data)
                {
                    GL.BufferData(BufferTarget.ArrayBuffer, data.Length, (IntPtr)ptr, BufferUsageHint.DynamicDraw);
                }
            }

            // Caller must set up their own attrib pointers before drawing
            GL.DrawArrays(primitiveType, 0, vertexCount);
        }

        /// <summary>
        /// Draws a full-screen background using a texture.
        /// </summary>
        private void RenderBackground(int textureId)
        {
            _resourceManager.UseShader(ShaderTypeApp.TexturedQuad);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            _renderer?.DrawTexturedQuad(
                new Point(0, 0),
                new Point((int)Width, 0),
                new Point((int)Width, (int)Height),
                new Point(0, (int)Height), textureId);
        }

        /// <summary>
        /// Capture the current framebuffer and save as PNG.
        /// </summary>
        public void CaptureScreenshot(string filePath)
        {
            if (_glControl == null) return;

            var width = _glControl.Width;
            var height = _glControl.Height;
            var pixels = new byte[width * height * 4];

            GL.ReadPixels(0, 0, width, height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

            using Bitmap bitmap = new(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Rectangle rect = new(0, 0, width, height);
            var bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);

            Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);
            bitmap.UnlockBits(bitmapData);
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            bitmap.Save(filePath, ImageFormat.Png);
        }

        /// <summary>
        /// Dispose all OpenGL resources and the GLControl.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _glControl?.Dispose();

                GL.DeleteVertexArray(_vao);
                GL.DeleteBuffer(_vbo);

                GL.DeleteVertexArray(_skyboxVao);
                GL.DeleteBuffer(_skyboxVbo);
                GL.DeleteTexture(_skyboxTexture);
                GL.DeleteTexture(_backgroundTexture);
                GL.DeleteProgram(_skyboxShader);

                _resourceManager.Dispose(); // Dispose shader/textures
            }

            base.Dispose(disposing);
        }
    }
}