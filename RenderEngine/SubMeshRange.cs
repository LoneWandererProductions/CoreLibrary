/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SubMeshRange
 * FILE:        SubMeshRange.cs
 * PURPOSE:     SubMeshRange struct represents a range of vertices for a sub-mesh, including the texture ID, starting vertex index, and vertex count.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

namespace RenderEngine
{
    /// <summary>
    /// Struct representing a range of vertices for a sub-mesh, including the texture ID, starting vertex index, and vertex count.
    /// </summary>
    public struct SubMeshRange
    {
        /// <summary>
        /// The texture identifier
        /// </summary>
        public int TextureId;

        /// <summary>
        /// The start vertex
        /// </summary>
        public int StartVertex;

        /// <summary>
        /// The vertex count
        /// </summary>
        public int VertexCount;
    }
}
