/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/TextureGenerator.cs
 * PURPOSE:     Generate some textures
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Sources:     https://lodev.org/cgtutor/randomnoise.html
 */

// ReSharper disable UnusedType.Global

using System.Drawing;

namespace Imaging
{
    /// <inheritdoc />
    /// <summary>
    ///     Main Entry Class that will handle all things related to textures
    /// </summary>
    /// <seealso cref="T:Imaging.ITextureGenerator" />
    public class TextureGenerator : ITextureGenerator
    {
        /// <inheritdoc />
        /// <summary>
        ///     Generates the noise bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Texture Bitmap</returns>
        public Bitmap GenerateNoiseBitmap(int width, int height)
        {
            var config = ImageRegister.GetSettings(TextureType.Noise);
            return Texture.GenerateNoiseBitmap(
                width,
                height,
                config.MinValue,
                config.MaxValue,
                config.Alpha,
                config.UseSmoothNoise,
                config.UseTurbulence,
                config.TurbulenceSize
            );
        }

        /// <inheritdoc />
        /// <summary>
        ///     Generates the clouds bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Texture Bitmap</returns>
        public Bitmap GenerateCloudsBitmap(int width, int height)
        {
            var config = ImageRegister.GetSettings(TextureType.Clouds);
            return Texture.GenerateCloudsBitmap(
                width,
                height,
                config.MinValue,
                config.MaxValue,
                config.Alpha,
                config.TurbulenceSize
            );
        }

        /// <inheritdoc />
        /// <summary>
        ///     Generates the marble bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Texture Bitmap</returns>
        public Bitmap GenerateMarbleBitmap(int width, int height)
        {
            var config = ImageRegister.GetSettings(TextureType.Marble);
            return Texture.GenerateMarbleBitmap(
                width,
                height,
                config.XPeriod,
                config.YPeriod,
                config.Alpha,
                config.TurbulencePower,
                config.TurbulenceSize,
                config.BaseColor
            );
        }

        /// <inheritdoc />
        /// <summary>
        ///     Generates the wave bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Texture Bitmap</returns>
        public Bitmap GenerateWaveBitmap(int width, int height)
        {
            var config = ImageRegister.GetSettings(TextureType.Wave);
            return Texture.GenerateWaveBitmap(
                width,
                height,
                config.Alpha,
                config.XyPeriod,
                config.TurbulencePower,
                config.TurbulenceSize
            );
        }

        /// <inheritdoc />
        /// <summary>
        ///     Generates the wood bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Texture Bitmap</returns>
        public Bitmap GenerateWoodBitmap(int width, int height)
        {
            var config = ImageRegister.GetSettings(TextureType.Wood);
            return Texture.GenerateWoodBitmap(
                width,
                height,
                config.Alpha,
                config.XyPeriod,
                config.TurbulencePower,
                config.TurbulenceSize,
                config.BaseColor
            );
        }

        /// <inheritdoc />
        /// <summary>
        ///     Generates the texture.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="filter">The texture type filter.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="shapeParams">The shape parameters.</param>
        /// <returns>Texture Bitmap</returns>
        public Bitmap GenerateTexture(int width, int height, TextureType filter, TextureShape shape,
            object shapeParams = null)
        {
            return TextureAreas.GenerateTexture(
                width,
                height,
                filter,
                shape,
                shapeParams
            );
        }

        /// <inheritdoc />
        /// <summary>
        ///     Generates the crosshatch bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Texture Bitmap</returns>
        public Bitmap GenerateCrosshatchBitmap(int width, int height)
        {
            var config = ImageRegister.GetSettings(TextureType.Crosshatch);

            return Texture.GenerateCrosshatchBitmap(
                width,
                height,
                config.LineSpacing,
                config.LineColor,
                config.LineThickness,
                config.Angle1,
                config.Angle2
            );
        }
    }
}
