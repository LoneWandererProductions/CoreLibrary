/*
 * COPYRIGHT:   See COPYING in the top-level directory
 * PROJECT:     Imaging.Helpers
 * FILE:        TextureAreas.cs
 * PURPOSE:     Provide textures for certain areas
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Drawing;
using Imaging.Enums;

namespace Imaging.Helpers
{
    /// <summary>
    ///     Apply textures to certain areas
    /// </summary>
    internal static class TextureAreas
    {
        /// <summary>
        ///     Generates the texture for a specified area.
        /// </summary>
        /// <param name="image">The image, optional.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="texture">The texture type.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="imageSettings">The image settings.</param>
        /// <param name="shapeParams">The shape parameters (optional).</param>
        /// <param name="startPoint">The optional starting point. Defaults to (0, 0).</param>
        /// <returns>
        ///     A Bitmap with the applied texture.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown for unsupported texture or shape types.</exception>
        internal static Bitmap? GenerateTexture(
            Bitmap? image,
            int width,
            int height,
            TextureType texture,
            MaskShape shape,
            ImageRegister? imageSettings,
            object? shapeParams = null,
            Point? startPoint = null)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException(ImagingResources.InvalidDimensions);
            }

            if (imageSettings == null)
            {
                throw new ArgumentNullException(nameof(imageSettings), ImagingResources.ImageSettingsNull);
            }

            // Use 0 for x and y if startPoint is null
            var x = startPoint?.X ?? 0;
            var y = startPoint?.Y ?? 0;

            // Retrieve texture settings
            var settings = imageSettings.GetSettings(texture) ??
                           throw new ArgumentException(ImagingResources.InvalidTextureSettings, nameof(imageSettings));

            // Generate texture
            var textureBitmap = texture switch
            {
                TextureType.Noise => TextureStream.GenerateNoiseBitmap(
                    width, height, settings.MinValue, settings.MaxValue, settings.Alpha, settings.IsMonochrome,
                    settings.IsTiled, settings.TurbulenceSize),

                TextureType.Clouds => TextureStream.GenerateCloudsBitmap(
                    width, height, settings.MinValue, settings.MaxValue, settings.Alpha, settings.TurbulenceSize),

                TextureType.Marble => TextureStream.GenerateMarbleBitmap(
                    width, height, settings.Alpha, baseColor: settings.BaseColor),

                TextureType.Wood => TextureStream.GenerateWoodBitmap(
                    width, height, settings.Alpha, baseColor: settings.BaseColor),

                TextureType.Wave => TextureStream.GenerateWaveBitmap(
                    width, height, settings.Alpha),

                TextureType.Crosshatch => TextureStream.GenerateCrosshatchBitmap(
                    width, height, settings.LineSpacing, settings.LineColor, settings.LineThickness,
                    settings.AnglePrimary,
                    settings.AngleSecondary, settings.Alpha),

                TextureType.Concrete => TextureStream.GenerateConcreteBitmap(
                    width, height, settings.MinValue, settings.MaxValue, settings.Alpha, settings.XPeriod,
                    settings.YPeriod, settings.TurbulencePower, settings.TurbulenceSize),

                TextureType.Canvas => TextureStream.GenerateCanvasBitmap(
                    width, height, settings.LineSpacing, settings.LineColor, settings.LineThickness, settings.Alpha,
                    settings.WaveFrequency, settings.WaveAmplitude, settings.RandomizationFactor,
                    settings.EdgeJaggednessLimit, settings.JaggednessThreshold),

                TextureType.Cellular => TextureStream.GenerateCellularBitmap(
                    width, height, settings.CellSize, settings.Alpha, settings.CenterColor, settings.EdgeColor),

                TextureType.ColorMapped => TextureStream.GenerateColorMappedBitmap(
                    width, height, settings.ColorRamp, settings.TurbulenceSize, settings.Alpha),

                //Preset textures, no settings can be provided.
                TextureType.MagicalEther => TexturePresets.GenerateMagicalEther(width, height),
                TextureType.Cobblestone => TexturePresets.GenerateCobblestone(width, height),
                TextureType.DragonScales => TexturePresets.GenerateDragonScales(width, height),
                TextureType.LavaPool => TexturePresets.GenerateLavaPool(width, height),
                _ => throw new ArgumentOutOfRangeException(nameof(texture), texture,
                    ImagingResources.UnsupportedTexture)
            };

            // Validate shape parameters and apply mask
            textureBitmap = shape switch
            {
                MaskShape.Rectangle => ImageMask.ApplyRectangleMask(textureBitmap, width, height),

                MaskShape.Circle => ImageMask.ApplyCircleMask(textureBitmap, width, height),

                MaskShape.Polygon when shapeParams is Point[] points => ImageMask.ApplyPolygonMask(textureBitmap,
                    TranslatePoints(points, x, y)),

                MaskShape.Polygon => throw new ArgumentException(ImagingResources.InvalidPolygonParams,
                    nameof(shapeParams)),

                _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, ImagingResources.UnsupportedShape)
            };

            //if we have an input Image, Combine original and filtered images
            return image == null ? textureBitmap : ImageStream.CombineBitmap(image, textureBitmap, x, y);
        }

        /// <summary>
        ///     Shifts absolute image-space points into the local (0,0-based) coordinate
        ///     space of a bitmap that was generated at just the selection's bounding-box
        ///     size (<paramref name="x" />/<paramref name="y" /> being that box's top-left
        ///     corner in the original image). Without this, a polygon/freeform texture
        ///     selection is masked against coordinates that fall outside the small
        ///     texture bitmap entirely, so the visible selection and the actually
        ///     textured area end up in different places (or the mask paints nothing).
        /// </summary>
        /// <param name="points">The absolute image-space polygon points.</param>
        /// <param name="x">The selection bounding box's left offset in the source image.</param>
        /// <param name="y">The selection bounding box's top offset in the source image.</param>
        /// <returns>The same points, translated to be relative to (x, y).</returns>
        private static Point[] TranslatePoints(Point[] points, int x, int y)
        {
            var translated = new Point[points.Length];
            for (var i = 0; i < points.Length; i++)
            {
                translated[i] = new Point(points[i].X - x, points[i].Y - y);
            }

            return translated;
        }
    }
}