/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        InstancedVaoCache.cs
 * PURPOSE:     Implements a cache that resolves arbitrary CPU geometry/matrices into hardware VAOs.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace RenderEngine
{
    /// <summary>
    /// Generic cache that resolves arbitrary CPU geometry/matrices into hardware VAOs.
    /// </summary>
    public class InstancedVaoCache
    {
        /// <summary>
        /// The resources
        /// </summary>
        private readonly GlResourceManager _resources;

        // Cache: (X, Y, Z, Type) -> (VaoId, InstanceCount, VertexCount)
        private readonly Dictionary<(float, float, float, int), (int Vao, int InstanceCount, int VertexCount)> _cache =
            new();

        /// <summary>
        /// Initializes a new instance of the <see cref="InstancedVaoCache"/> class.
        /// </summary>
        /// <param name="resources">The resources.</param>
        public InstancedVaoCache(GlResourceManager resources)
        {
            _resources = resources;
        }

        /// <summary>
        /// Gets the or build vao.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="seed">The seed.</param>
        /// <param name="type">The type.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <returns>A tuple containing the VAO ID, instance count, and vertex count.</returns>
        public (int Vao, int InstanceCount, int VertexCount) GetOrBuildVao(
            System.Numerics.Vector3 position,
            int seed,
            int type,
            Func<(float[] Vertices, System.Numerics.Matrix4x4[] Matrices)> dataProvider)
        {
            var key = (position.X, position.Y, position.Z, type);
            if (_cache.TryGetValue(key, out var cached)) return cached;

            // Execute the lazy delegate ONLY on the very first frame this chunk is seen
            var data = dataProvider();

            var instanceCount = data.Matrices.Length;
            var vertexCount = data.Vertices.Length / 5; // Assuming 5 floats per vertex (X,Y,Z, U,V)

            // Convert System.Numerics to OpenTK internally
            var tkMatrices = new Matrix4[instanceCount];
            for (var i = 0; i < instanceCount; i++)
            {
                var m = data.Matrices[i];
                tkMatrices[i] = new Matrix4(
                    m.M11, m.M12, m.M13, m.M14,
                    m.M21, m.M22, m.M23, m.M24,
                    m.M31, m.M32, m.M33, m.M34,
                    m.M41, m.M42, m.M43, m.M44
                );
            }

            var vao = _resources.CreateInstancedVao(data.Vertices, tkMatrices, out _);

            var result = (vao, instanceCount, vertexCount);
            _cache[key] = result;
            return result;
        }
    }
}
