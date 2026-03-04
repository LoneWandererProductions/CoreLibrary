/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        Simple3DRenderer.cs
 * PURPOSE:     Lightweight 3D renderer using OpenGL
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 * NOTES:       - Uses basic MVP (Model-View-Projection) matrices
 *              - Designed to work seamlessly alongside Simple2DRenderer
 */

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace RenderEngine
{
    public sealed class Simple3DRenderer : IDisposable
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        private readonly GlResourceManager _resources;

        private int _vao;
        private int _vbo;
        private int _shaderProgram;
        private bool _initialized;

        // Matrices
        private Matrix4 _projection;
        private Matrix4 _view;

        public Simple3DRenderer(int width, int height, GlResourceManager resources)
        {
            Width = width;
            Height = height;
            _resources = resources;

            // Default camera looking at the origin from slightly back and up
            UpdateProjection(width, height);
            SetCamera(new Vector3(0, 2, 5), Vector3.Zero, Vector3.UnitY);
        }

        public void UpdateProjection(int width, int height)
        {
            // Prevent zero dimensions during WPF initialization or when minimized
            if (width <= 0) width = 1;
            if (height <= 0) height = 1;

            Width = width;
            Height = height;

            var aspect = width / (float)height;

            // 45 degree Field of View, Near plane 0.1f, Far plane 1000f
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), aspect, 0.1f, 1000f);
        }

        public void SetCamera(Vector3 position, Vector3 target, Vector3 up)
        {
            _view = Matrix4.LookAt(position, target, up);
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;

            // 1. Generate Buffers
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            // Give it a decent starting capacity
            GL.BufferData(BufferTarget.ArrayBuffer, 8192 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);

            // 2. Vertex Attributes (Layout: X, Y, Z, R, G, B, A) -> 7 floats total
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0); // Position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float),
                3 * sizeof(float)); // Color
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);

            // 3. Fetch Shader from ResourceManager instead of compiling inline
            _shaderProgram = _resources.GetShaderProgram(ShaderTypeApp.VertexColor);

            _initialized = true;
        }

        /// <summary>
        /// Draws a colored cube at a specific 3D position with a given scale.
        /// </summary>
        public void DrawCube(Vector3 position, Vector3 scale, (int r, int g, int b, int a) color)
        {
            EnsureInitialized();

            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vao);

            // Set MVP matrices
            var model = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(position);

            // Note: Updated uniform names to match ShaderResource.cs ("model", "view", "projection")
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "model"), false, ref model);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "view"), false, ref _view);
            GL.UniformMatrix4(GL.GetUniformLocation(_shaderProgram, "projection"), false, ref _projection);

            // Generate Cube Vertices
            var data = GenerateCubeData(color);

            // Upload and Draw
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, data.Length * sizeof(float), data);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36); // 36 vertices in a cube

            GL.BindVertexArray(0);
        }

        // Helper to generate a 1x1x1 cube centered at the local origin
        private float[] GenerateCubeData((int r, int g, int b, int a) c)
        {
            var r = c.r / 255f;
            var g = c.g / 255f;
            var b = c.b / 255f;
            var a = c.a / 255f;

            // X, Y, Z, R, G, B, A (36 vertices for 12 triangles)
            return new float[]
            {
                // Front face
                -0.5f, -0.5f, 0.5f, r, g, b, a, 0.5f, -0.5f, 0.5f, r, g, b, a, 0.5f, 0.5f, 0.5f, r, g, b, a, 0.5f, 0.5f,
                0.5f, r, g, b, a, -0.5f, 0.5f, 0.5f, r, g, b, a, -0.5f, -0.5f, 0.5f, r, g, b, a,
                // Back face
                -0.5f, -0.5f, -0.5f, r, g, b, a, -0.5f, 0.5f, -0.5f, r, g, b, a, 0.5f, 0.5f, -0.5f, r, g, b, a, 0.5f,
                0.5f, -0.5f, r, g, b, a, 0.5f, -0.5f, -0.5f, r, g, b, a, -0.5f, -0.5f, -0.5f, r, g, b, a,
                // Left face
                -0.5f, 0.5f, 0.5f, r, g, b, a, -0.5f, 0.5f, -0.5f, r, g, b, a, -0.5f, -0.5f, -0.5f, r, g, b, a, -0.5f,
                -0.5f, -0.5f, r, g, b, a, -0.5f, -0.5f, 0.5f, r, g, b, a, -0.5f, 0.5f, 0.5f, r, g, b, a,
                // Right face
                0.5f, 0.5f, 0.5f, r, g, b, a, 0.5f, -0.5f, -0.5f, r, g, b, a, 0.5f, 0.5f, -0.5f, r, g, b, a, 0.5f,
                -0.5f, -0.5f, r, g, b, a, 0.5f, 0.5f, 0.5f, r, g, b, a, 0.5f, -0.5f, 0.5f, r, g, b, a,
                // Top face
                -0.5f, 0.5f, -0.5f, r, g, b, a, -0.5f, 0.5f, 0.5f, r, g, b, a, 0.5f, 0.5f, 0.5f, r, g, b, a, 0.5f, 0.5f,
                0.5f, r, g, b, a, 0.5f, 0.5f, -0.5f, r, g, b, a, -0.5f, 0.5f, -0.5f, r, g, b, a,
                // Bottom face
                -0.5f, -0.5f, -0.5f, r, g, b, a, 0.5f, -0.5f, -0.5f, r, g, b, a, 0.5f, -0.5f, 0.5f, r, g, b, a, 0.5f,
                -0.5f, 0.5f, r, g, b, a, -0.5f, -0.5f, 0.5f, r, g, b, a, -0.5f, -0.5f, -0.5f, r, g, b, a
            };
        }

        public void Dispose()
        {
            if (!_initialized) return;
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            // Note: We do NOT delete the _shaderProgram here anymore, 
            // because the GlResourceManager owns it and will clean it up!
        }
    }
}
