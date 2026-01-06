/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        HeadlessRenderer.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

namespace RenderEngine.Tests
{
    class HeadlessRenderer
    {
        private readonly GameWindow _window;
        private int _fbo, _tex, _vao, _vbo;
        private readonly int _width = 256;
        private readonly int _height = 256;

        public HeadlessRenderer()
        {
            var nativeSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(1, 1), // hidden tiny window
                Title = "Hidden GL Context",
                StartVisible = false
            };

            _window = new GameWindow(GameWindowSettings.Default, nativeSettings);
            _window.MakeCurrent();

            SetupFbo();
            SetupGeometry();
        }

        private void SetupFbo()
        {
            // Create framebuffer
            _fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);

            // Create texture
            _tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                _width, _height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, nint.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Nearest);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, _tex, 0);

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("FBO incomplete");

            GL.Viewport(0, 0, _width, _height);
        }

        private void SetupGeometry()
        {
            // Simple quad (can be reused for tiles)
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            float[] vertices = { -0.5f, -0.5f, 0.5f, -0.5f, 0.5f, 0.5f, -0.5f, 0.5f };

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices,
                BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        }

        public Bitmap Render()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
            GL.ClearColor(Color4.CornflowerBlue);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Draw simple tile/line example (just one quad)
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);

            // Read pixels
            var bmp = new Bitmap(_width, _height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var data = bmp.LockBits(new Rectangle(0, 0, _width, _height),
                ImageLockMode.WriteOnly, bmp.PixelFormat);
            GL.ReadPixels(0, 0, _width, _height, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte,
                data.Scan0);
            bmp.UnlockBits(data);

            return bmp;
        }

        public void Dispose()
        {
            GL.DeleteFramebuffer(_fbo);
            GL.DeleteTexture(_tex);
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            _window.Close();
        }
    }

    // Usage
    class Program
    {
        static void Main()
        {
            var renderer = new HeadlessRenderer();
            var bmp = renderer.Render();
            bmp.Save("test.png", ImageFormat.Png);
            Console.WriteLine("Rendered image saved!");
        }
    }
}
