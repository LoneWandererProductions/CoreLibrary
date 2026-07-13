/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Interfaces
 * FILE:        ITextureGenerator.cs
 * PURPOSE:     Interface of TextureGenerator
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal

using System.Drawing;
using Imaging.Enums;

namespace Imaging.Interfaces
{
    /// <summary>
    ///     Skeleton for an Texture Interface
    /// </summary>
    public interface ITextureGenerator
    {
        /// <summary>
        ///     Generates the texture.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="filter">The texture.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="startPoint">The Start point.</param>
        /// <param name="shapeParams">The shape parameters.</param>
        /// <returns>
        ///     Texture Bitmap
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     texture - null
        ///     or
        ///     shape - null
        /// </exception>
        Bitmap? GenerateTexture(
            int width,
            int height,
            TextureType filter,
            MaskShape shape,
            Point? startPoint = null,
            object? shapeParams = null);

        /// <summary>
        ///     Generates the texture overlay.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="filter">The texture.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="startPoint">The start point.</param>
        /// <param name="shapeParams">The shape parameters.</param>
        /// <returns>
        ///     Texture Bitmap
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     texture - null
        ///     or
        ///     shape - null
        /// </exception>
        Bitmap? GenerateTextureOverlay(Bitmap image, int width, int height, TextureType filter, MaskShape shape,
            Point? startPoint = null,
            object? shapeParams = null);
    }
}
