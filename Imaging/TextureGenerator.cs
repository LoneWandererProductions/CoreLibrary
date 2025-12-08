/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        TextureGenerator.cs
 * PURPOSE:     Generate some textures
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System.Drawing;
using Imaging.Enums;
using Imaging.Helpers;
using Imaging.Interfaces;

namespace Imaging;

/// <inheritdoc />
/// <summary>
///     Main Entry Class that will handle all things related to textures
/// </summary>
/// <seealso cref="T:Imaging.ITextureGenerator" />
public sealed class TextureGenerator : ITextureGenerator
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TextureGenerator" /> class.
    /// </summary>
    public TextureGenerator()
    {
        ImageSettings = ImageRegister.Instance; // Ensure singleton instance is available
    }

    /// <summary>
    ///     The image Settings
    /// </summary>
    /// <value>
    ///     The image settings.
    /// </value>
    private ImageRegister ImageSettings { get; }

    /// <inheritdoc />
    /// <summary>
    ///     Generates the texture.
    /// </summary>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="filter">The texture type filter.</param>
    /// <param name="shape">The shape.</param>
    /// <param name="startPoint">The Start point.</param>
    /// <param name="shapeParams">The shape parameters.</param>
    /// <returns>Texture Bitmap</returns>
    public Bitmap? GenerateTexture(int width, int height, TextureType filter, MaskShape shape,
        Point? startPoint = null,
        object? shapeParams = null)
    {
        return TextureAreas.GenerateTexture(
            null,
            width,
            height,
            filter,
            shape,
            ImageSettings, shapeParams, startPoint);
    }

    /// <inheritdoc />
    /// <summary>
    ///     Generates the texture overlay.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="height">The height.</param>
    /// <param name="filter">The filter.</param>
    /// <param name="shape">The shape.</param>
    /// <param name="startPoint">The start point.</param>
    /// <param name="shapeParams">The shape parameters.</param>
    /// <param name="width">The width.</param>
    /// <returns>
    ///     Texture Bitmap
    /// </returns>
    /// <exception cref="System.ArgumentOutOfRangeException">
    ///     filter - null
    ///     or
    ///     shape - null
    /// </exception>
    public Bitmap? GenerateTextureOverlay(Bitmap image, int width, int height, TextureType filter, MaskShape shape,
        Point? startPoint = null,
        object? shapeParams = null)
    {
        return TextureAreas.GenerateTexture(
            image,
            width,
            height,
            filter,
            shape,
            ImageSettings, shapeParams, startPoint);
    }
}