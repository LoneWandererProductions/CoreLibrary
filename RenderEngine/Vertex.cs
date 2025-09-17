/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        Vertex.cs
 * PURPOSE:     Store vertex attributes for rasterization.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace RenderEngine;

/// <summary>
/// Represents a single vertex in screen space with texture coordinates and depth.
/// </summary>
public struct Vertex
{
    /// <summary>
    /// Screen-space position (pixel coordinates, floating point).
    /// </summary>
    public float X, Y;

    /// <summary>
    /// Texture coordinates (normalized 0..1).
    /// </summary>
    public float U, V;

    /// <summary>
    /// Depth value in camera space (Z &gt; 0).
    /// Used for Z-buffer comparison.
    /// </summary>
    public float Z;

    /// <summary>
    /// Initializes a new instance of the <see cref="Vertex"/> struct.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <param name="u">The u.</param>
    /// <param name="v">The v.</param>
    /// <param name="z">The z.</param>
    public Vertex(float x, float y, float u, float v, float z)
    {
        X = x;
        Y = y;
        U = u;
        V = v;
        Z = z;
    }
}
