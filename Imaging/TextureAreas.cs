/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/TextureAreas.cs
 * PURPOSE:     Provide textures for certain areas
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Sources:     https://lodev.org/cgtutor/randomnoise.html
 */

using System;
using System.Drawing;

namespace Imaging
{
    /// <summary>
    ///     Apply textures to certain areas
    /// </summary>
    internal static class TextureAreas
    {
        /// <summary>
        ///     Generates the texture.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="shapeParams">The shape parameters.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <param name="baseColor">Color of the base.</param>
        /// <returns>
        ///     Generates a filter for a certain area
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     filter - null
        ///     or
        ///     shape - null
        /// </exception>
        internal static Bitmap GenerateTexture(
            int width,
            int height,
            TextureType filter,
            TextureShape shape,
            object shapeParams = null,
            int minValue = 0,
            int maxValue = 255,
            int alpha = 255,
            double turbulenceSize = 64,
            Color? baseColor = null)
        {
            // Retrieve the settings for the specified filter
            var settings = ImageRegister.GetSettings(filter);

            // Create a bitmap to apply the texture
            Bitmap textureBitmap;

            // Generate texture based on the selected filter
            switch (filter)
            {
                case TextureType.Noise:
                    textureBitmap = Texture.GenerateNoiseBitmap(
                        width,
                        height,
                        settings.MinValue,
                        settings.MaxValue,
                        settings.Alpha,
                        settings.IsMonochrome,
                        settings.IsTiled,
                        settings.TurbulenceSize);
                    break;

                case TextureType.Clouds:
                    textureBitmap = Texture.GenerateCloudsBitmap(
                        width,
                        height,
                        settings.MinValue,
                        settings.MaxValue,
                        settings.Alpha,
                        settings.TurbulenceSize);
                    break;

                case TextureType.Marble:
                    textureBitmap = Texture.GenerateMarbleBitmap(
                        width,
                        height,
                        settings.Alpha,
                        baseColor: settings.BaseColor);
                    break;

                case TextureType.Wood:
                    textureBitmap = Texture.GenerateWoodBitmap(
                        width,
                        height,
                        settings.Alpha,
                        baseColor: settings.BaseColor);
                    break;

                case TextureType.Wave:
                    textureBitmap = Texture.GenerateWaveBitmap(width, height, settings.Alpha);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
            }

            // Apply the texture to the specified area shape
            switch (shape)
            {
                case TextureShape.Rectangle:
                    return ApplyRectangleMask(textureBitmap, width, height);

                case TextureShape.Circle:
                    return ApplyCircleMask(textureBitmap, width, height);

                case TextureShape.Polygon:
                    return ApplyPolygonMask(textureBitmap, (Point[])shapeParams);

                default:
                    throw new ArgumentOutOfRangeException(nameof(shape), shape, null);
            }
        }

        /// <summary>
        ///     Applies the rectangle mask.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Rectangle Bitmap</returns>
        private static Bitmap ApplyRectangleMask(Bitmap bitmap, int width, int height)
        {
            // In this case, the rectangle shape means applying the texture as-is

            //Todo add area
            return bitmap;
        }

        /// <summary>
        ///     Applies the circle mask.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Circle Bitmap</returns>
        private static Bitmap ApplyCircleMask(Image bitmap, int width, int height)
        {
            var circleBitmap = new Bitmap(width, height);
            using var g = Graphics.FromImage(circleBitmap);
            g.Clear(Color.Transparent);
            using var brush = new TextureBrush(bitmap);
            g.FillEllipse(brush, 0, 0, width, height);

            return circleBitmap;
        }

        /// <summary>
        ///     Applies the polygon mask.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="points">The points.</param>
        /// <returns>Polygon Bitmap</returns>
        private static Bitmap ApplyPolygonMask(Image bitmap, Point[] points)
        {
            var polyBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            using var g = Graphics.FromImage(polyBitmap);
            g.Clear(Color.Transparent);
            using var brush = new TextureBrush(bitmap);
            g.FillPolygon(brush, points);

            return polyBitmap;
        }
    }
}
