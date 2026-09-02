/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        Simple3DRenderer.cs
 * PURPOSE:     High-performance zero-allocation 3D renderer for basic shapes and sprites supporting vertex color lighting and Post-Processing FBO effects.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using OpenTK.Graphics.OpenGL4;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using RenderEngine.Enums;
using TK = OpenTK.Mathematics;

namespace RenderEngine
{
    /// <inheritdoc/>
    /// <summary>
    /// 3D Renderer for basic shapes and sprites, using OpenGL for rendering.
    /// Handles unmanaged batch conversions and maps vertex fragments to the active graphics pipeline with FBO post-processing support.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class Simple3DRenderer : IDisposable
    {
        // --- PROPERTIES ---

        /// <summary>
        /// Gets the current physical pixel width of the viewport projection target.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the current physical pixel height of the viewport projection target.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the active 3D camera look-at View matrix profile.
        /// </summary>
        public TK.Matrix4 ViewMatrix => _view;

        // --- PRIVATE STORAGE FIELDS ---

        /// <summary>
        /// Reference tracker managing engine-level compiled shader assets and texture definitions.
        /// </summary>
        private readonly GlResourceManager _resources;

        /// <summary>
        /// Hardware layout buffer markers tracking active states
        /// </summary>
        private int _vaoSolid, _vboSolid, _vaoTex, _vboTex;

        /// <summary>
        /// The shader solid
        /// </summary>
        private int _shaderSolid, _shaderTex;

        /// <summary>
        /// The initialized
        /// </summary>
        private bool _initialized;

        /// <summary>
        /// Default internal capacities tracking bounds for auto-growth re-allocations
        /// </summary>
        private int _vboSolidCapacity = 16384, _vboTexCapacity = 16384;

        /// <summary>
        /// The projection
        /// </summary>
        private TK.Matrix4 _projection;

        /// <summary>
        /// The view
        /// </summary>
        private TK.Matrix4 _view;

        // --- PERFORMANCE CACHE FIELDS ---

        /// <summary>
        /// Cached memory uniform indices for the flat vertex-shading pipeline program.
        /// </summary>
        private int _locModelSolid, _locViewSolid, _locProjSolid;

        /// <summary>
        /// Cached memory uniform indices for the textured/billboard sprite pipeline program.
        /// </summary>
        private int _locModelTex, _locViewTex, _locProjTex;

        /// <summary>
        /// Cached memory uniform index for the 2D texture sampler uniform ("uTexture").
        /// </summary>
        private int _locTexSampler;

        // --- POST-PROCESSING FBO & QUAD STORAGE ---

        /// <summary>
        /// The fbo
        /// </summary>
        private int _fbo;

        /// <summary>
        /// The previous fbo
        /// </summary>
        private int _previousFbo;

        /// <summary>
        /// The fbo texture
        /// </summary>
        private int _fboTexture;

        /// <summary>
        /// The previous viewport
        /// </summary>
        private readonly int[] _previousViewport = new int[4];

        /// <summary>
        /// The rbo depth
        /// </summary>
        private int _rboDepth;

        /// <summary>
        /// The quad vao
        /// </summary>
        private int _quadVao;

        /// <summary>
        /// The quad vbo
        /// </summary>
        private int _quadVbo;

        /// <summary>
        /// The post process shader
        /// </summary>
        private int _postProcessShader;

        /// <summary>
        /// The fbo initialized
        /// </summary>
        private bool _fboInitialized;

        /// <summary>
        /// Gets or sets a value indicating whether post-processing is enabled.
        /// </summary>
        public bool PostProcessingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the active post-processing filter mode.
        /// </summary>
        public PostProcessFilter CurrentFilter { get; set; } = PostProcessFilter.Painterly;

        // --- CONSTRUCTOR ---

        /// <summary>
        /// Initializes a new instance of the <see cref="Simple3DRenderer"/> class.
        /// Sets default projection models and primes camera viewing ranges.
        /// </summary>
        /// <param name="width">The target canvas width.</param>
        /// <param name="height">The target canvas height.</param>
        /// <param name="resources">The shared unmanaged resource coordinator.</param>
        public Simple3DRenderer(int width, int height, GlResourceManager resources)
        {
            _resources = resources;
            UpdateProjection(width, height);
            SetCamera(new Vector3(8, 15, 25), new Vector3(8, 0, 8), Vector3.UnitY);
        }

        // --- POST-PROCESSING FILTER CONTROLS ---

        /// <summary>
        /// Toggles post-processing on or off.
        /// </summary>
        public void TogglePostProcessing()
        {
            PostProcessingEnabled = !PostProcessingEnabled;
        }

        /// <summary>
        /// Switches the active post-processing filter mode.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public void SetFilter(PostProcessFilter filter)
        {
            CurrentFilter = filter;
        }

        // --- MATRIX CONFIGURATION INTERFACES ---

        /// <summary>
        /// Explicitly overwrites the active 3D projection viewing calculations matrix template.
        /// </summary>
        /// <param name="fovDegrees">Field-of-View angle specification.</param>
        /// <param name="aspect">Aspect ratio dimensions multiplier.</param>
        /// <param name="near">The closest clip distance boundary plane.</param>
        /// <param name="far">The maximum visible distance horizon depth plane.</param>
        public void SetProjection(float fovDegrees, float aspect, float near, float far)
        {
            _projection = TK.Matrix4.CreatePerspectiveFieldOfView(
                TK.MathHelper.DegreesToRadians(fovDegrees),
                aspect,
                near,
                far);
        }

        /// <summary>
        /// Evaluates rendering aspect variables and reconstructs perspective matrices to fit screen translations.
        /// Dynamic resizing support included for off-screen FBO backing textures.
        /// </summary>
        /// <param name="width">The new canvas width footprint.</param>
        /// <param name="height">The new canvas height footprint.</param>
        public void UpdateProjection(int width, int height)
        {
            if (width <= 0) width = 1;
            if (height <= 0) height = 1;
            Width = width;
            Height = height;
            var aspect = width / (float)height;

            SetProjection(45f, aspect, 1.0f, 1000f);

            // Resize FBO textures if already allocated
            if (_fboInitialized)
            {
                GL.BindTexture(TextureTarget.Texture2D, _fboTexture);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb,
                    PixelType.UnsignedByte, IntPtr.Zero);

                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rboDepth);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width,
                    height);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            }
        }

        /// <summary>
        /// Positions the viewport eye vector and builds lookup translation matrices.
        /// </summary>
        /// <param name="position">The eye coordinate of the spectator view position.</param>
        /// <param name="target">The focal node coordinate the spectator view is facing.</param>
        /// <param name="up">The upward directional world orientation vector.</param>
        public void SetCamera(Vector3 position, Vector3 target, Vector3 up)
        {
            if (Vector3.DistanceSquared(position, target) < 0.001f)
                target += Vector3.UnitZ;

            _view = TK.Matrix4.LookAt(
                new TK.Vector3(position.X, position.Y, position.Z),
                new TK.Vector3(target.X, target.Y, target.Z),
                new TK.Vector3(up.X, up.Y, up.Z));
        }

        // --- PIPELINE INITIALIZATION ENGINE ---

        /// <summary>
        /// Allocates unmanaged layout markers and fetches cached variable indexes.
        /// </summary>
        private void EnsureInitialized()
        {
            if (_initialized) return;

            GL.Disable(EnableCap.Dither);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _vaoSolid = GL.GenVertexArray();
            _vboSolid = GL.GenBuffer();
            GL.BindVertexArray(_vaoSolid);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);
            GL.BufferData(BufferTarget.ArrayBuffer, _vboSolidCapacity * sizeof(float), IntPtr.Zero,
                BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            _vaoTex = GL.GenVertexArray();
            _vboTex = GL.GenBuffer();
            GL.BindVertexArray(_vaoTex);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
            GL.BufferData(BufferTarget.ArrayBuffer, _vboTexCapacity * sizeof(float), IntPtr.Zero,
                BufferUsageHint.DynamicDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 9 * sizeof(float), 5 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(0);

            _shaderSolid = _resources.GetShaderProgram(ShaderTypeApp.VertexColor);
            _shaderTex = _resources.GetShaderProgram(ShaderTypeApp.TexturedQuad);

            _locModelSolid = GL.GetUniformLocation(_shaderSolid, "model");
            _locViewSolid = GL.GetUniformLocation(_shaderSolid, "view");
            _locProjSolid = GL.GetUniformLocation(_shaderSolid, "projection");

            _locModelTex = GL.GetUniformLocation(_shaderTex, "model");
            _locViewTex = GL.GetUniformLocation(_shaderTex, "view");
            _locProjTex = GL.GetUniformLocation(_shaderTex, "projection");

            _locTexSampler = GL.GetUniformLocation(_shaderTex, "uTexture");

            _initialized = true;
        }

        /// <summary>
        /// Allocates off-screen Framebuffer Object (FBO) and full-screen pass quad.
        /// </summary>
        private void EnsureFboInitialized()
        {
            if (_fboInitialized) return;

            var w = Width <= 0 ? 800 : Width;
            var h = Height <= 0 ? 600 : Height;

            // 1. Framebuffer Object
            _fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);

            // 2. Color Attachment Texture (RGBA format)
            _fboTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fboTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, w, h, 0, PixelFormat.Rgba,
                PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D, _fboTexture, 0);

            // 3. Depth-Stencil Renderbuffer
            _rboDepth = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rboDepth);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, w, h);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment,
                RenderbufferTarget.Renderbuffer, _rboDepth);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // 4. Full-Screen Quad (Pos: X,Y, UV: U,V)
            var quadVertices = new float[]
            {
                -1.0f, 1.0f, 0.0f, 1.0f, -1.0f, -1.0f, 0.0f, 0.0f, 1.0f, -1.0f, 1.0f, 0.0f, -1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, -1.0f, 1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f
            };

            _quadVao = GL.GenVertexArray();
            _quadVbo = GL.GenBuffer();
            GL.BindVertexArray(_quadVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _quadVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices,
                BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);

            _postProcessShader = _resources.GetShaderProgram(ShaderTypeApp.PostProcessing);
            _fboInitialized = true;
        }

        /// <summary>
        /// Binds the offscreen Framebuffer when post-processing is enabled.
        /// Remembers GLWpfControl's active target framebuffer ID.
        /// </summary>
        public void BeginFrame()
        {
            EnsureInitialized();

            if (PostProcessingEnabled)
            {
                EnsureFboInitialized();

                // 1. Capture GLWpfControl's active FBO and Viewport bounds
                GL.GetInteger(GetPName.FramebufferBinding, out _previousFbo);
                GL.GetInteger(GetPName.Viewport, _previousViewport);

                var vpWidth = _previousViewport[2] > 0 ? _previousViewport[2] : (Width > 0 ? Width : 800);
                var vpHeight = _previousViewport[3] > 0 ? _previousViewport[3] : (Height > 0 ? Height : 600);

                // 2. Auto-resize FBO textures if control dimensions changed
                if (vpWidth != Width || vpHeight != Height)
                {
                    Width = vpWidth;
                    Height = vpHeight;

                    GL.BindTexture(TextureTarget.Texture2D, _fboTexture);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0,
                        PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rboDepth);
                    GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, Width,
                        Height);
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
                }

                // 3. Target offscreen FBO
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);
                GL.Viewport(0, 0, Width, Height);

                GL.DepthMask(true);
                GL.Enable(EnableCap.DepthTest);
                GL.DepthFunc(DepthFunction.Lequal);

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit |
                         ClearBufferMask.StencilBufferBit);
            }
        }

        /// <summary>
        /// Renders the post-processing filter pass onto the screen.
        /// Call at the end of frame drawing.
        /// </summary>
        public void EndFrame()
        {
            if (!PostProcessingEnabled || !_fboInitialized) return;

            // 1. Restore GLWpfControl's target FBO and Viewport
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _previousFbo);
            GL.Viewport(_previousViewport[0], _previousViewport[1], _previousViewport[2], _previousViewport[3]);

            // 2. Disable Depth, Culling, AND Blending for 2D screen quad
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend); // Fixes transparent quad blending!
            GL.DepthMask(false);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            // 3. Render Post-Processing Quad
            GL.UseProgram(_postProcessShader);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _fboTexture);

            GL.Uniform1(GL.GetUniformLocation(_postProcessShader, "uScene"), 0);
            GL.Uniform2(GL.GetUniformLocation(_postProcessShader, "uScreenSize"), Width, (float)Height);
            GL.Uniform1(GL.GetUniformLocation(_postProcessShader, "uFilterMode"), (int)CurrentFilter);

            GL.BindVertexArray(_quadVao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            // 4. Restore state for 2D UI / future passes
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.DepthMask(true);
        }

        // --- CORE GEOMETRY GENERATION METHODS ---

        /// <summary>
        /// Directly emits a solid flat-shaded 3D primitive triangle to the active context canvas.
        /// </summary>
        public unsafe void DrawTriangle(Vector3 v0, Vector3 v1, Vector3 v2, (int r, int g, int b, int a) color)
        {
            EnsureInitialized();
            GL.UseProgram(_shaderSolid);
            GL.BindVertexArray(_vaoSolid);

            var model = TK.Matrix4.Identity;
            GL.UniformMatrix4(_locModelSolid, false, ref model);
            GL.UniformMatrix4(_locViewSolid, false, ref _view);
            GL.UniformMatrix4(_locProjSolid, false, ref _projection);

            var r = color.r / 255f;
            var g = color.g / 255f;
            var b = color.b / 255f;
            var a = color.a / 255f;

            Span<float> data = stackalloc float[21]
            {
                v0.X, v0.Y, v0.Z, r, g, b, a, v1.X, v1.Y, v1.Z, r, g, b, a, v2.X, v2.Y, v2.Z, r, g, b, a
            };

            fixed (float* ptr = data)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), (IntPtr)ptr);
            }

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }

        /// <summary>
        /// Draws a textured triangle face structure.
        /// </summary>
        /// <param name="v0">The v0.</param>
        /// <param name="uv0">The uv0.</param>
        /// <param name="v1">The v1.</param>
        /// <param name="uv1">The uv1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="uv2">The uv2.</param>
        /// <param name="textureId">The texture identifier.</param>
        /// <param name="color">The color.</param>
        public unsafe void DrawTexturedTriangle(Vector3 v0, Vector2 uv0, Vector3 v1, Vector2 uv1, Vector3 v2,
            Vector2 uv2,
            int textureId, (int r, int g, int b, int a)? color = null)
        {
            textureId = _resources.ResolveTextureId(textureId);

            EnsureInitialized();
            GL.UseProgram(_shaderTex);
            GL.BindVertexArray(_vaoTex);

            if (_locTexSampler >= 0)
            {
                GL.Uniform1(_locTexSampler, 0);
            }

            var model = TK.Matrix4.Identity;
            GL.UniformMatrix4(_locModelTex, false, ref model);
            GL.UniformMatrix4(_locViewTex, false, ref _view);
            GL.UniformMatrix4(_locProjTex, false, ref _projection);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            var c = color ?? (255, 255, 255, 255);
            var r = c.r / 255f;
            var g = c.g / 255f;
            var b = c.b / 255f;
            var a = c.a / 255f;

            Span<float> data = stackalloc float[27]
            {
                v0.X, v0.Y, v0.Z, uv0.X, uv0.Y, r, g, b, a, v1.X, v1.Y, v1.Z, uv1.X, uv1.Y, r, g, b, a, v2.X, v2.Y,
                v2.Z, uv2.X, uv2.Y, r, g, b, a
            };

            fixed (float* ptr = data)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), (IntPtr)ptr);
            }

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }

        /// <summary>
        /// Evaluates camera viewing matrices and constructs a camera-facing billboarded graphic sprite quad.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="textureId">The texture identifier.</param>
        /// <param name="color">The color.</param>
        public unsafe void DrawSprite(Vector3 position, float radius, int textureId,
            (int r, int g, int b, int a)? color = null)
        {
            textureId = _resources.ResolveTextureId(textureId);

            EnsureInitialized();
            GL.UseProgram(_shaderTex);
            GL.BindVertexArray(_vaoTex);

            if (_locTexSampler >= 0)
            {
                GL.Uniform1(_locTexSampler, 0);
            }

            TK.Vector3 right = new(_view[0, 0], _view[1, 0], _view[2, 0]);
            TK.Vector3 up = new(_view[0, 1], _view[1, 1], _view[2, 1]);
            TK.Vector3 pos = new(position.X, position.Y, position.Z);

            var v0 = pos - (right * radius) - (up * radius);
            var v1 = pos + (right * radius) - (up * radius);
            var v2 = pos + (right * radius) + (up * radius);
            var v3 = pos - (right * radius) + (up * radius);

            var model = TK.Matrix4.Identity;
            GL.UniformMatrix4(_locModelTex, false, ref model);
            GL.UniformMatrix4(_locViewTex, false, ref _view);
            GL.UniformMatrix4(_locProjTex, false, ref _projection);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            var c = color ?? (255, 255, 255, 255);
            var r = c.r / 255f;
            var g = c.g / 255f;
            var b = c.b / 255f;
            var a = c.a / 255f;

            Span<float> data = stackalloc float[54]
            {
                v0.X, v0.Y, v0.Z, 0, 0, r, g, b, a, v1.X, v1.Y, v1.Z, 1, 0, r, g, b, a, v2.X, v2.Y, v2.Z, 1, 1, r,
                g, b, a, v0.X, v0.Y, v0.Z, 0, 0, r, g, b, a, v2.X, v2.Y, v2.Z, 1, 1, r, g, b, a, v3.X, v3.Y, v3.Z,
                0, 1, r, g, b, a
            };

            fixed (float* ptr = data)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), (IntPtr)ptr);
            }

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        /// <summary>
        /// Manually registers an unmanaged configuration layout schema matrix coordinate.
        /// </summary>
        public void SetCustomProjection(Matrix4x4 projection)
        {
            _projection = new TK.Matrix4(
                projection.M11, projection.M12, projection.M13, projection.M14,
                projection.M21, projection.M22, projection.M23, projection.M24,
                projection.M31, projection.M32, projection.M33, projection.M34,
                projection.M41, projection.M42, projection.M43, projection.M44
            );
        }

        // --- HARDWARE BATCH FLUSH RUNTIMES ---

        /// <summary>
        /// Flushes accumulated data arrays out of host storage pools into hardware stream paths.
        /// </summary>
        public unsafe void Flush(RenderBatch batch)
        {
            EnsureInitialized();

            if (batch.Solid3DVertices.Length > 0)
            {
                GL.UseProgram(_shaderSolid);
                GL.BindVertexArray(_vaoSolid);

                var id = TK.Matrix4.Identity;
                GL.UniformMatrix4(_locModelSolid, false, ref id);
                GL.UniformMatrix4(_locViewSolid, false, ref _view);
                GL.UniformMatrix4(_locProjSolid, false, ref _projection);

                EnsureBufferCapacity(_vboSolid, ref _vboSolidCapacity, batch.Solid3DVertices.Length);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, batch.Solid3DVertices.Length * sizeof(float),
                    (IntPtr)batch.Solid3DVertices.Pointer);
                GL.DrawArrays(PrimitiveType.Triangles, 0, batch.Solid3DVertices.Length / 7);
            }

            if (batch.Textured3DBatches.Count > 0)
            {
                GL.UseProgram(_shaderTex);
                GL.BindVertexArray(_vaoTex);

                if (_locTexSampler >= 0)
                {
                    GL.Uniform1(_locTexSampler, 0);
                }

                var id = TK.Matrix4.Identity;
                GL.UniformMatrix4(_locModelTex, false, ref id);
                GL.UniformMatrix4(_locViewTex, false, ref _view);
                GL.UniformMatrix4(_locProjTex, false, ref _projection);

                foreach (var kvp in batch.Textured3DBatches)
                {
                    if (kvp.Value.Count == 0) continue;

                    var texToBind = _resources.ResolveTextureId(kvp.Key);

                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, texToBind);

                    var span = CollectionsMarshal.AsSpan(kvp.Value);
                    EnsureBufferCapacity(_vboTex, ref _vboTexCapacity, span.Length);

                    fixed (float* ptr = span)
                    {
                        GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
                        GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, span.Length * sizeof(float),
                            (IntPtr)ptr);
                    }

                    GL.DrawArrays(PrimitiveType.Triangles, 0, span.Length / 9);
                }
            }

            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Renders a persistent mesh directly from GPU VRAM with zero PCIe memory transfers.
        /// </summary>
        /// <param name="mesh">The mesh.</param>
        public void DrawStaticMesh(GpuMesh mesh)
        {
            EnsureInitialized();

            if (mesh.SolidVertexCount > 0)
            {
                GL.UseProgram(_shaderSolid);
                GL.BindVertexArray(mesh.SolidVao);

                var id = TK.Matrix4.Identity;
                GL.UniformMatrix4(_locModelSolid, false, ref id);
                GL.UniformMatrix4(_locViewSolid, false, ref _view);
                GL.UniformMatrix4(_locProjSolid, false, ref _projection);

                GL.DrawArrays(PrimitiveType.Triangles, 0, mesh.SolidVertexCount);
            }

            if (mesh.Ranges.Count == 0) return;

            GL.UseProgram(_shaderTex);
            GL.BindVertexArray(mesh.Vao);
            if (_locTexSampler >= 0) GL.Uniform1(_locTexSampler, 0);

            var idTex = TK.Matrix4.Identity;
            GL.UniformMatrix4(_locModelTex, false, ref idTex);
            GL.UniformMatrix4(_locViewTex, false, ref _view);
            GL.UniformMatrix4(_locProjTex, false, ref _projection);

            foreach (var range in mesh.Ranges)
            {
                if (range.VertexCount == 0) continue;

                var texToBind = _resources.ResolveTextureId(range.TextureId);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, texToBind);
                GL.DrawArrays(PrimitiveType.Triangles, range.StartVertex, range.VertexCount);
            }

            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Evaluates active buffer layouts and automatically reallocates memory capacities upwards on demand.
        /// </summary>
        private void EnsureBufferCapacity(int vbo, ref int cap, int req)
        {
            if (req <= cap) return;

            while (cap < req) cap *= 2;
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, cap * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }

        // --- CLEANUP UNMANAGED ASSETS ---

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_initialized) return;

            GL.DeleteBuffer(_vboSolid);
            GL.DeleteVertexArray(_vaoSolid);
            GL.DeleteBuffer(_vboTex);
            GL.DeleteVertexArray(_vaoTex);

            if (_fboInitialized)
            {
                GL.DeleteFramebuffer(_fbo);
                GL.DeleteTexture(_fboTexture);
                GL.DeleteRenderbuffer(_rboDepth);
                GL.DeleteBuffer(_quadVbo);
                GL.DeleteVertexArray(_quadVao);
                _fboInitialized = false;
            }
        }
    }
}
