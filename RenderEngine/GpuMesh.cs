/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        GpuMesh.cs
 * PURPOSE:     Gpu Mesh represents a mesh stored in GPU memory, encapsulating its vertex array object (VAO), vertex buffer object (VBO), and associated data for rendering.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace RenderEngine
{
    /// <inheritdoc/>
    /// <summary>
    /// Encapsulates GPU VRAM handles (VAO/VBO). Designed for high-frequency reuse to eliminate GPU driver thrashing and GC allocations.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public sealed class GpuMesh : IDisposable
    {
        /// <summary>
        /// The is textured layout configured
        /// </summary>
        private bool _isTexturedLayoutConfigured;

        /// <summary>
        /// The is solid layout configured
        /// </summary>
        private bool _isSolidLayoutConfigured;

        /// <summary>
        /// Gets the textured VAO handle.
        /// </summary>
        public int Vao { get; private set; }

        /// <summary>
        /// Gets the textured VBO handle.
        /// </summary>
        public int Vbo { get; private set; }

        /// <summary>
        /// Gets the solid VAO handle.
        /// </summary>
        public int SolidVao { get; private set; }

        /// <summary>
        /// Gets the solid VBO handle.
        /// </summary>
        public int SolidVbo { get; private set; }

        /// <summary>
        /// Gets the solid vertex count.
        /// </summary>
        public int SolidVertexCount { get; private set; }

        /// <summary>
        /// Gets the sub-mesh ranges for multi-textured rendering.
        /// </summary>
        public List<SubMeshRange> Ranges { get; } = new();

        /// <summary>
        /// Uploads textured vertex data (9 floats/vertex: Pos:3, UV:2, Color:4).
        /// Reuses existing GPU handles if this instance has already been initialized.
        /// </summary>
        /// <param name="vertexData">The vertex data span.</param>
        /// <param name="ranges">Sub-mesh ranges span (zero-GC memory passing).</param>
        /// <param name="isDynamic">Set to true if this mesh updates frequently (e.g. dynamic terrain chunks).</param>
        public unsafe void Upload(ReadOnlySpan<float> vertexData, ReadOnlySpan<SubMeshRange> ranges,
            bool isDynamic = false)
        {
            // 1. Copy ranges without allocating enumerators or lists
            Ranges.Clear();
            for (var i = 0; i < ranges.Length; i++)
            {
                Ranges.Add(ranges[i]);
            }

            if (vertexData.IsEmpty) return;

            // 2. Generate handles only if they don't exist yet
            if (Vao == 0) Vao = GL.GenVertexArray();
            if (Vbo == 0) Vbo = GL.GenBuffer();

            GL.BindVertexArray(Vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);

            var usage = isDynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;

            // 3. Upload data to GPU VRAM (reuses existing buffer storage)
            fixed (float* ptr = vertexData)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), (nint)ptr, usage);
            }

            // 4. Configure VAO Attribute Pointers ONLY ONCE! (VAO saves this layout state internally)
            if (!_isTexturedLayoutConfigured)
            {
                // Layout Attribute 0: Position (X, Y, Z) - 3 floats
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);

                // Layout Attribute 1: UV (U, V) - 2 floats
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float),
                    3 * sizeof(float));
                GL.EnableVertexAttribArray(1);

                // Layout Attribute 2: Color (R, G, B, A) - 4 floats
                GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 9 * sizeof(float),
                    5 * sizeof(float));
                GL.EnableVertexAttribArray(2);

                _isTexturedLayoutConfigured = true;
            }

            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Overload for backwards compatibility with List&lt;SubMeshRange&gt;.
        /// </summary>
        public void Upload(ReadOnlySpan<float> vertexData, List<SubMeshRange> ranges, bool isDynamic = false)
        {
            Upload(vertexData, System.Runtime.InteropServices.CollectionsMarshal.AsSpan(ranges), isDynamic);
        }

        /// <summary>
        /// Uploads untextured solid geometry (7 floats/vertex: X,Y,Z,R,G,B,A).
        /// Reuses existing GPU handles if available.
        /// </summary>
        public void UploadSolid(IntPtr dataPointer, int floatCount, bool isDynamic = false)
        {
            SolidVertexCount = floatCount / 7;
            if (floatCount == 0) return;

            if (SolidVao == 0) SolidVao = GL.GenVertexArray();
            if (SolidVbo == 0) SolidVbo = GL.GenBuffer();

            GL.BindVertexArray(SolidVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, SolidVbo);

            var usage = isDynamic ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;
            GL.BufferData(BufferTarget.ArrayBuffer, floatCount * sizeof(float), dataPointer, usage);

            // Configure VAO attributes only once
            if (!_isSolidLayoutConfigured)
            {
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float),
                    3 * sizeof(float));
                GL.EnableVertexAttribArray(1);

                _isSolidLayoutConfigured = true;
            }

            GL.BindVertexArray(0);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Vbo != 0)
            {
                GL.DeleteBuffer(Vbo);
                Vbo = 0;
            }

            if (Vao != 0)
            {
                GL.DeleteVertexArray(Vao);
                Vao = 0;
            }

            if (SolidVbo != 0)
            {
                GL.DeleteBuffer(SolidVbo);
                SolidVbo = 0;
            }

            if (SolidVao != 0)
            {
                GL.DeleteVertexArray(SolidVao);
                SolidVao = 0;
            }

            _isTexturedLayoutConfigured = false;
            _isSolidLayoutConfigured = false;
            Ranges.Clear();
        }
    }
}
