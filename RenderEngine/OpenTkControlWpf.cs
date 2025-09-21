/*
 * COPYRIGHT:   See COPYING in the top-level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/OpenTkControlWpf.cs
 * PURPOSE:     OpenGL Viewer for WPF applications using OpenTKDrawHelper.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

#nullable enable
using OpenTK.Graphics.OpenGL4;
using OpenTK.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;

namespace RenderEngine
{
    /// <summary>
    /// OpenGL viewer control for WPF, using OpenTKDrawHelper for drawing primitives and textured quads.
    /// Supports background textures, optional skybox, column overlays, pixel overlays,
    /// and also accepts raw OpenGL-style vertex data (via Span&lt;float&gt; or Span&lt;byte&gt;).
    /// </summary>
    public sealed class OpenTkControlWpf : UserControl, IDisposable
    {
        private readonly GLWpfControl _glControl;

        // Background texture ID
        private int _backgroundTexture = -1;

        // Skybox related
        private bool _enableSkybox;
        private int _skyboxShader;
        private int _skyboxTexture;
        private int _skyboxVao, _skyboxVbo;

        // VBO reuse for dynamic rendering
        private int _vbo;
        private int _vao;

        // Shader/resource manager
        private readonly GlResourceManager _resourceManager = new();

        public OpenTkControlWpf()
        {
            if (!OpenTkHelper.IsOpenGlCompatible())
                throw new NotSupportedException("OpenGL not supported on this system.");

            var settings = new GLWpfControlSettings { MajorVersion = 4, MinorVersion = 6, RenderContinuously = false };

            _glControl = new GLWpfControl { Settings = settings };
            Content = _glControl;

            _glControl.Start(settings);
            _glControl.Render += OnRender;
            _glControl.SizeChanged += OnResize;

            Loaded += (_, _) => InitializeGl();
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
                _glControl.InvalidateVisual();
            }
        }

        /// <summary>
        /// Initialize OpenGL resources.
        /// </summary>
        private void InitializeGl()
        {
            GL.ClearColor(0.1f, 0.2f, 0.3f, 1f);
            GL.Enable(EnableCap.DepthTest);

            OpenTkDrawHelper.Initialize();

            _backgroundTexture = OpenTkHelper.LoadTextureFromFile("background.jpg");

            InitializeSkybox();

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
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
        /// WPF GL render loop.
        /// Clears the screen, draws background, optionally skybox.
        /// </summary>
        private void OnRender(TimeSpan delta)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (_backgroundTexture != -1)
            {
                OpenTkDrawHelper.DrawTexturedQuad(
                    _backgroundTexture,
                    new System.Drawing.Point(0, 0),
                    new System.Drawing.Point((int)ActualWidth, 0),
                    new System.Drawing.Point((int)ActualWidth, (int)ActualHeight),
                    new System.Drawing.Point(0, (int)ActualHeight));
            }

            if (_enableSkybox && _skyboxTexture != -1)
                RenderSkybox();
        }

        /// <summary>
        /// WPF resize event handler. Updates the OpenGL viewport.
        /// </summary>
        private void OnResize(object? sender, SizeChangedEventArgs e)
        {
            GL.Viewport(0, 0, (int)ActualWidth, (int)ActualHeight);
        }

        // --- Your RenderColumns, RenderPixels, RenderVertices, RenderRaw, RenderBackground,
        //     CaptureScreenshot methods go here unchanged, just adapt width/height to (int)ActualWidth/ActualHeight ---

        /// <summary>
        /// Dispose all OpenGL resources and the GLControl.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);

            GL.DeleteVertexArray(_skyboxVao);
            GL.DeleteBuffer(_skyboxVbo);
            GL.DeleteTexture(_skyboxTexture);
            GL.DeleteTexture(_backgroundTexture);
            GL.DeleteProgram(_skyboxShader);

            _resourceManager.Dispose();
        }
    }
}
