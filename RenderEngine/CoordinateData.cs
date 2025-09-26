/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        CoordinateData.cs
 * PURPOSE:     Pixel data structure for rendering and Color information.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Drawing;

namespace RenderEngine;

/// <summary>
/// Mostly used to pass coordinate and color data to shaders.
/// </summary>
public struct CoordinateData
{
    /// <summary>
    /// The x point.
    /// </summary>
    public int X;

    /// <summary>
    /// The y point.
    /// </summary>
    public int Y;

    /// <summary>
    /// The color
    /// </summary>
    public Color Color;
}
