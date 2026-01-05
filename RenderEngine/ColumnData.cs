/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        ColumnData.cs
 * PURPOSE:     Handle Columns of data for rendering. Mostly used for voxel and raycasting rendering.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Numerics;

namespace RenderEngine;

/// <summary>
/// Basic idea was to use it for voxel and raycasting rendering.
/// </summary>
public struct ColumnData
{
    /// <summary>
    /// The height
    /// </summary>
    public float Height; // Height of the slice

    /// <summary>
    /// The color
    /// </summary>
    public Vector3 Color; // RGB color of the slice
}