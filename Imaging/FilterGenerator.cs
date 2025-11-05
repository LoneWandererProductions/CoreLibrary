/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        FilterGenerator.cs
 * PURPOSE:     Central filter generator
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Drawing;
using Imaging.Enums;
using Imaging.Helpers;
using Imaging.Interfaces;

namespace Imaging
{
    /// <inheritdoc />
    /// <summary>
    /// The Filter Generator
    /// </summary>
    /// <seealso cref="Imaging.Interfaces.IFilterGenerator" />
    public class FilterGenerator : IFilterGenerator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TextureGenerator" /> class.
        /// </summary>
        public FilterGenerator()
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
        /// <param name="filter">The filter type filter.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="startPoint">The Start point.</param>
        /// <param name="shapeParams">The shape parameters.</param>
        /// <returns>Texture Bitmap</returns>
        public Bitmap? GenerateFilter(int width, int height, FiltersType filter, MaskShape shape,
            Point? startPoint = null,
            object? shapeParams = null)
        {
            return FiltersAreas.GenerateFilter(
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
        public Bitmap? GenerateFilterOverlay(Bitmap image, int width, int height, FiltersType filter, MaskShape shape,
            Point? startPoint = null,
            object? shapeParams = null)
        {
            return FiltersAreas.GenerateFilter(
                image,
                width,
                height,
                filter,
                shape,
                ImageSettings, shapeParams, startPoint);
        }
    }
}
