/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        Simple3DRenderer.cs
 * PURPOSE:     A simple 3D renderer for basic shapes and sprites supporting vertex color lighting.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using OpenTK.Graphics.OpenGL4;
using System;
using System.Numerics;
using TK = OpenTK.Mathematics;

namespace RenderEngine
{
    /// <inheritdoc/>
    /// <summary>
    /// 3D Renderer for basic shapes and sprites, using OpenGL for rendering.
    /// Handles unmanaged batch conversions and maps vertex fragments to the active graphics pipeline.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class Simple3DRenderer : IDisposable
    {
        // =================================================================================
        // PROPERTIES
        // =================================================================================

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

        // =================================================================================
        // PRIVATE STORAGE FIELDS
        // =================================================================================

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
        /// The vbo solid capacity
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

        // =================================================================================
        // PERFORMANCE CACHE FIELDS
        // =================================================================================

        /// <summary>
        /// Cached memory uniform indices for the flat vertex-shading pipeline program.
        /// Prevents heavy driver string lookups and stalls mid-frame loop.
        /// </summary>
        private int _locModelSolid, _locViewSolid, _locProjSolid;

        /// <summary>
        /// Cached memory uniform indices for the textured/billboard sprite pipeline program.
        /// Prevents heavy driver string lookups and stalls mid-frame loop.
        /// </summary>
        private int _locModelTex, _locViewTex, _locProjTex;

        // =================================================================================
        // CONSTRUCTOR
        // =================================================================================

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

        // =================================================================================
        // MATRIX CONFIGURATION INTERFACES
        // =================================================================================

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

            // Maintain an extended far plane distance cutoff limit so large mountains stay visible
            SetProjection(45f, aspect, 1.0f, 1000f);
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

        // =================================================================================
        // PIPELINE INITIALIZATION ENGINE
        // =================================================================================

        /// <summary>
        /// Allocates unmanaged layout markers and fetches cached variable indexes.
        /// Runs exactly once on demand to preserve active pipeline safety.
        /// </summary>
        private void EnsureInitialized()
        {
            if (_initialized) return;

            // 1. Build and configure the Solid flat-color attribute parsing pipeline
            _vaoSolid = GL.GenVertexArray();
            _vboSolid = GL.GenBuffer();
            GL.BindVertexArray(_vaoSolid);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);
            GL.BufferData(BufferTarget.ArrayBuffer, _vboSolidCapacity * sizeof(float), IntPtr.Zero,
                BufferUsageHint.DynamicDraw);

            // Layout attribute 0: Local position coordinates (X, Y, Z)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            // Layout attribute 1: Color interpolation values (R, G, B, A)
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // 2. Build and configure the Textured / Alpha mapping attribute parsing pipeline
            _vaoTex = GL.GenVertexArray();
            _vboTex = GL.GenBuffer();
            GL.BindVertexArray(_vaoTex);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
            GL.BufferData(BufferTarget.ArrayBuffer, _vboTexCapacity * sizeof(float), IntPtr.Zero,
                BufferUsageHint.DynamicDraw);

            // Layout attribute 0: Local position coordinates (X, Y, Z)
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            // Layout attribute 1: UV Texture map pointers (U, V)
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            // Layout attribute 2: Color tint multipliers channel trackers (R, G, B, A)
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 9 * sizeof(float), 5 * sizeof(float));
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(0);

            // 3. Extract compiled binary shader pipeline references from the manager registry
            _shaderSolid = _resources.GetShaderProgram(ShaderTypeApp.VertexColor);
            _shaderTex = _resources.GetShaderProgram(ShaderTypeApp.TexturedQuad);

            // ====================================================================
            // PERFORMANCE UPGRADE: UNIFORM HANDLE CACHING PASS
            // Queries memory indices exactly once here so the rendering loop runs
            // entirely on direct numeric pointers instead of heavy string evaluations.
            // ====================================================================
            _locModelSolid = GL.GetUniformLocation(_shaderSolid, "model");
            _locViewSolid = GL.GetUniformLocation(_shaderSolid, "view");
            _locProjSolid = GL.GetUniformLocation(_shaderSolid, "projection");

            _locModelTex = GL.GetUniformLocation(_shaderTex, "model");
            _locViewTex = GL.GetUniformLocation(_shaderTex, "view");
            _locProjTex = GL.GetUniformLocation(_shaderTex, "projection");

            _initialized = true;
        }

        // =================================================================================
        // CORE GEOMETRY GENERATION METHODS
        // =================================================================================

        /// <summary>
        /// Directly emits a solid flat-shaded 3D primitive triangle to the active context canvas.
        /// </summary>
        /// <param name="v0">The v0.</param>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="color">The color.</param>
        public void DrawTriangle(Vector3 v0, Vector3 v1, Vector3 v2, (int r, int g, int b, int a) color)
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

            float[] data = { v0.X, v0.Y, v0.Z, r, g, b, a, v1.X, v1.Y, v1.Z, r, g, b, a, v2.X, v2.Y, v2.Z, r, g, b, a };

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
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
        /// <param name="color">Optional color parameter mapping voxel light scales directly into shader engines.</param>
        public void DrawTexturedTriangle(Vector3 v0, Vector2 uv0, Vector3 v1, Vector2 uv1, Vector3 v2, Vector2 uv2,
            int textureId, (int r, int g, int b, int a)? color = null)
        {
            if (textureId <= 0) textureId = _resources.GetFallbackTexture();

            EnsureInitialized();
            GL.UseProgram(_shaderTex);
            GL.BindVertexArray(_vaoTex);

            var model = TK.Matrix4.Identity;
            GL.UniformMatrix4(_locModelTex, false, ref model);
            GL.UniformMatrix4(_locViewTex, false, ref _view);
            GL.UniformMatrix4(_locProjTex, false, ref _projection);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            // Fallback to absolute clear white if no color block data parameter arrives
            var c = color ?? (255, 255, 255, 255);
            var r = c.r / 255f;
            var g = c.g / 255f;
            var b = c.b / 255f;
            var a = c.a / 255f;

            float[] data =
            {
                v0.X, v0.Y, v0.Z, uv0.X, uv0.Y, r, g, b, a, v1.X, v1.Y, v1.Z, uv1.X, uv1.Y, r, g, b, a, v2.X, v2.Y,
                v2.Z, uv2.X, uv2.Y, r, g, b, a
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }

        /// <summary>
        /// Evaluates camera viewing matrices and constructs a camera-facing billboarded graphic sprite quad.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="textureId">The texture identifier.</param>
        /// <param name="color">Optional color parameter mapping environment lumens straight to entity surfaces.</param>
        public void DrawSprite(Vector3 position, float radius, int textureId,
            (int r, int g, int b, int a)? color = null)
        {
            if (textureId <= 0) textureId = _resources.GetFallbackTexture();

            EnsureInitialized();
            GL.UseProgram(_shaderTex);
            GL.BindVertexArray(_vaoTex);

            // Reverse-engineer active look-at orientations straight out of the view matrix channels
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

            // Apply local illumination byte values to map shadows across sprites
            var c = color ?? (255, 255, 255, 255);
            var r = c.r / 255f;
            var g = c.g / 255f;
            var b = c.b / 255f;
            var a = c.a / 255f;

            float[] data =
            {
                v0.X, v0.Y, v0.Z, 0, 0, r, g, b, a, v1.X, v1.Y, v1.Z, 1, 0, r, g, b, a, v2.X, v2.Y, v2.Z, 1, 1, r,
                g, b, a, v0.X, v0.Y, v0.Z, 0, 0, r, g, b, a, v2.X, v2.Y, v2.Z, 1, 1, r, g, b, a, v3.X, v3.Y, v3.Z,
                0, 1, r, g, b, a
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        /// <summary>
        /// Manually registers an unmanaged configuration layout schema matrix matrix coordinate.
        /// </summary>
        /// <param name="projection">The projection.</param>
        public void SetCustomProjection(Matrix4x4 projection)
        {
            _projection = new TK.Matrix4(
                projection.M11, projection.M12, projection.M13, projection.M14,
                projection.M21, projection.M22, projection.M23, projection.M24,
                projection.M31, projection.M32, projection.M33, projection.M34,
                projection.M41, projection.M42, projection.M43, projection.M44
            );
        }

        // =================================================================================
        // HARDWARE BATCH FLUSH RUNTIMES
        // =================================================================================

        /// <summary>
        /// Flushes accumulated data arrays out of host storage pools into hardware stream paths.
        /// </summary>
        /// <param name="batch">The batch.</param>
        public unsafe void Flush(RenderBatch batch)
        {
            EnsureInitialized();

            // 1. Process flat-shaded geometries
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

            // 2. Process textured collection arrays
            if (batch.Textured3DBatches.Count > 0)
            {
                GL.UseProgram(_shaderTex);
                GL.BindVertexArray(_vaoTex);

                var id = TK.Matrix4.Identity;
                GL.UniformMatrix4(_locModelTex, false, ref id);
                GL.UniformMatrix4(_locViewTex, false, ref _view);
                GL.UniformMatrix4(_locProjTex, false, ref _projection);

                foreach (var kvp in batch.Textured3DBatches)
                {
                    if (kvp.Value.Count == 0) continue;

                    // ✨ THE ONLY CHANGE: Use the smart coordinator to route file IDs, fallbacks, and high-range math textures natively
                    var texToBind = _resources.ResolveTextureId(kvp.Key);

                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, texToBind);
                    var data = kvp.Value.ToArray();
                    EnsureBufferCapacity(_vboTex, ref _vboTexCapacity, data.Length);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
                    GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
                    GL.DrawArrays(PrimitiveType.Triangles, 0, data.Length / 9);
                }
            }

            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Evaluates active buffer layouts and automatically reallocates memory capacities upwards on demand.
        /// </summary>
        /// <param name="vbo">The vbo.</param>
        /// <param name="cap">The cap.</param>
        /// <param name="req">The req.</param>
        private void EnsureBufferCapacity(int vbo, ref int cap, int req)
        {
            if (req <= cap) return;

            while (cap < req) cap *= 2;
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, cap * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }

        // =================================================================================
        // CLEANUP UNMANAGED ASSETS
        // =================================================================================

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_initialized) return;

            GL.DeleteBuffer(_vboSolid);
            GL.DeleteVertexArray(_vaoSolid);
            GL.DeleteBuffer(_vboTex);
            GL.DeleteVertexArray(_vaoTex);
        }
    }
}
