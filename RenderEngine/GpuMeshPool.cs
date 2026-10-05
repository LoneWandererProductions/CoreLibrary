/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        GpuMeshPool.cs
 * PURPOSE:     Pools persistent GpuMesh instances to eliminate GPU buffer allocation stalls.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;

namespace RenderEngine
{
    /// <summary>
    /// Manages reusable <see cref="GpuMesh"/> instances to avoid driver VAO/VBO handle churn.
    /// </summary>
    public sealed class GpuMeshPool : IDisposable
    {
        private readonly Stack<GpuMesh> _pool = new();

        /// <summary>
        /// Rents a recycled mesh from the pool, or creates a new one if empty.
        /// </summary>
        public GpuMesh Rent()
        {
            if (_pool.Count > 0)
            {
                var mesh = _pool.Pop();
                mesh.Reset();
                return mesh;
            }

            return new GpuMesh();
        }

        /// <summary>
        /// Returns a mesh to the pool for reuse without disposing its GPU handles.
        /// </summary>
        public void Return(GpuMesh? mesh)
        {
            if (mesh == null) return;
            mesh.Reset();
            _pool.Push(mesh);
        }

        /// <summary>
        /// Disposes all pooled GPU resources.
        /// </summary>
        public void Dispose()
        {
            while (_pool.Count > 0)
            {
                _pool.Pop().Dispose();
            }
        }
    }
}