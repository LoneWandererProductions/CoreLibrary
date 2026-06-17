/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        Simple3DRenderer.cs
 * PURPOSE:     A simple 3D renderer for basic shapes and sprites.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using OpenTK.Graphics.OpenGL4;
using System;
using System.Numerics;
using TK = OpenTK.Mathematics;

namespace RenderEngine
{
    ///<inheritdoc/>
    /// <summary>
    /// 3D Renderer for basic shapes and sprites, using OpenGL for rendering.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class Simple3DRenderer : IDisposable
    {
        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int Height { get; private set; }

        /// <summary>
        /// The resources
        /// </summary>
        private readonly GlResourceManager _resources;

        /// <summary>
        /// The vao solid
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

        /// <summary>
        /// Gets the view matrix.
        /// </summary>
        /// <value>
        /// The view matrix.
        /// </value>
        public TK.Matrix4 ViewMatrix => _view;

        /// <summary>
        /// Initializes a new instance of the <see cref="Simple3DRenderer"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="resources">The resources.</param>
        public Simple3DRenderer(int width, int height, GlResourceManager resources)
        {
            _resources = resources;
            UpdateProjection(width, height);
            SetCamera(new Vector3(8, 15, 25), new Vector3(8, 0, 8), Vector3.UnitY);
        }

        /// <summary>
        /// Sets the projection.
        /// </summary>
        /// <param name="fovDegrees">The fov degrees.</param>
        /// <param name="aspect">The aspect.</param>
        /// <param name="near">The near plane distance.</param>
        /// <param name="far">The far plane distance.</param>
        public void SetProjection(float fovDegrees, float aspect, float near, float far)
        {
            _projection = TK.Matrix4.CreatePerspectiveFieldOfView(
                TK.MathHelper.DegreesToRadians(fovDegrees),
                aspect,
                near,
                far);
        }

        /// <summary>
        /// Updates the projection.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public void UpdateProjection(int width, int height)
        {
            if (width <= 0) width = 1;
            if (height <= 0) height = 1;
            Width = width;
            Height = height;
            var aspect = width / (float)height;

            // Using a large far plane for terrain visibility
            SetProjection(45f, aspect, 0.1f, 10000f);
        }

        /// <summary>
        /// Sets the camera.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="target">The target.</param>
        /// <param name="up">Up.</param>
        public void SetCamera(Vector3 position, Vector3 target, Vector3 up)
        {
            if (Vector3.DistanceSquared(position, target) < 0.001f)
                target += Vector3.UnitZ;

            _view = TK.Matrix4.LookAt(
                new TK.Vector3(position.X, position.Y, position.Z),
                new TK.Vector3(target.X, target.Y, target.Z),
                new TK.Vector3(up.X, up.Y, up.Z));
        }

        /// <summary>
        /// Ensures the initialized.
        /// </summary>
        private void EnsureInitialized()
        {
            if (_initialized) return;

            // Solid Shader Setup
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

            // Textured Shader Setup
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
            _initialized = true;
        }

        /// <summary>
        /// Draws the triangle.
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
            UploadMatrices(_shaderSolid, ref model);

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
        /// Draws the textured triangle.
        /// </summary>
        /// <param name="v0">The v0.</param>
        /// <param name="uv0">The uv0.</param>
        /// <param name="v1">The v1.</param>
        /// <param name="uv1">The uv1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="uv2">The uv2.</param>
        /// <param name="textureId">The texture identifier.</param>
        public void DrawTexturedTriangle(Vector3 v0, Vector2 uv0, Vector3 v1, Vector2 uv1, Vector3 v2, Vector2 uv2,
            int textureId)
        {
            EnsureInitialized();
            GL.UseProgram(_shaderTex);
            GL.BindVertexArray(_vaoTex);
            var model = TK.Matrix4.Identity;
            UploadMatrices(_shaderTex, ref model);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            float[] data =
            {
                v0.X, v0.Y, v0.Z, uv0.X, uv0.Y, 1, 1, 1, 1, v1.X, v1.Y, v1.Z, uv1.X, uv1.Y, 1, 1, 1, 1, v2.X, v2.Y,
                v2.Z, uv2.X, uv2.Y, 1, 1, 1, 1
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }

        /// <summary>
        /// Draws the sprite.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="textureId">The texture identifier.</param>
        public void DrawSprite(Vector3 position, float radius, int textureId)
        {
            EnsureInitialized();
            GL.UseProgram(_shaderTex);
            GL.BindVertexArray(_vaoTex);

            // Billboarding math
            TK.Vector3 right = new(_view[0, 0], _view[1, 0], _view[2, 0]);
            TK.Vector3 up = new(_view[0, 1], _view[1, 1], _view[2, 1]);
            TK.Vector3 pos = new(position.X, position.Y, position.Z);

            var v0 = pos - (right * radius) - (up * radius);
            var v1 = pos + (right * radius) - (up * radius);
            var v2 = pos + (right * radius) + (up * radius);
            var v3 = pos - (right * radius) + (up * radius);

            var model = TK.Matrix4.Identity;
            UploadMatrices(_shaderTex, ref model);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            float[] data =
            {
                v0.X, v0.Y, v0.Z, 0, 0, 1, 1, 1, 1, v1.X, v1.Y, v1.Z, 1, 0, 1, 1, 1, 1, v2.X, v2.Y, v2.Z, 1, 1, 1,
                1, 1, 1, v0.X, v0.Y, v0.Z, 0, 0, 1, 1, 1, 1, v2.X, v2.Y, v2.Z, 1, 1, 1, 1, 1, 1, v3.X, v3.Y, v3.Z,
                0, 1, 1, 1, 1, 1
            };

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        }

        /// <summary>
        /// Uploads the matrices.
        /// </summary>
        /// <param name="shader">The shader.</param>
        /// <param name="model">The model.</param>
        private void UploadMatrices(int shader, ref TK.Matrix4 model)
        {
            var locModel = GL.GetUniformLocation(shader, "model");
            var locView = GL.GetUniformLocation(shader, "view");
            var locProj = GL.GetUniformLocation(shader, "projection");

            if (locModel != -1) GL.UniformMatrix4(locModel, false, ref model);
            if (locView != -1) GL.UniformMatrix4(locView, false, ref _view);
            if (locProj != -1) GL.UniformMatrix4(locProj, false, ref _projection);
        }

        /// <summary>
        /// Manually sets a custom projection matrix (e.g., from System.Numerics for Ortho).
        /// </summary>
        /// <param name="projection">The custom projection matrix from System.Numerics.</param>
        public void SetCustomProjection(Matrix4x4 projection)
        {
            // AND convert System.Numerics.Matrix4x4 to OpenTK.Mathematics.Matrix4
            _projection = new TK.Matrix4(
                projection.M11, projection.M12, projection.M13, projection.M14,
                projection.M21, projection.M22, projection.M23, projection.M24,
                projection.M31, projection.M32, projection.M33, projection.M34,
                projection.M41, projection.M42, projection.M43, projection.M44
            );
        }

        /// <summary>
        /// Flushes the specified batch.
        /// </summary>
        /// <param name="batch">The batch.</param>
        public unsafe void Flush(RenderBatch batch)
        {
            EnsureInitialized();

            if (batch.Solid3DVertices.Length > 0)
            {
                GL.UseProgram(_shaderSolid);
                GL.BindVertexArray(_vaoSolid);
                var id = TK.Matrix4.Identity;
                UploadMatrices(_shaderSolid, ref id);
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
                var id = TK.Matrix4.Identity;
                UploadMatrices(_shaderTex, ref id);
                foreach (var kvp in batch.Textured3DBatches)
                {
                    if (kvp.Value.Count == 0) continue;

                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, kvp.Key);
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
        /// Ensures the buffer capacity.
        /// </summary>
        /// <param name="vbo">The vertex buffer object.</param>
        /// <param name="cap">The current capacity.</param>
        /// <param name="req">The required capacity.</param>
        private void EnsureBufferCapacity(int vbo, ref int cap, int req)
        {
            if (req <= cap) return;

            while (cap < req) cap *= 2;
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, cap * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }

        ///<inheritdoc/>
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
