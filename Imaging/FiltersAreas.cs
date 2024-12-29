/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/FilterType.cs
 * PURPOSE:     Provide filters for certain areas
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Drawing;

namespace Imaging
{
    /// <summary>
    ///     Apply textures to certain areas
    /// </summary>
    internal static class FiltersAreas
    {
        /// <summary>
        /// Generates the filter.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="imageSettings">The image settings.</param>
        /// <param name="shapeParams">The shape parameters.</param>
        /// <param name="startPoint">The optional starting point (top-left corner) of the rectangle. Defaults to (0, 0).</param>
        /// <returns>
        /// Generates a filter for a certain area
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">filter - null
        /// or
        /// shape - null</exception>
        internal static Bitmap GenerateFilter(Bitmap image,
            FiltersType filter,
            MaskShape shape,
            ImageRegister imageSettings,
            object shapeParams = null,
            Point? startPoint = null)
        {
            // If no start point is provided, default to (0, 0)
            var actualStartPoint = startPoint ?? new Point(0, 0);

            // Create a bitmap to apply the filter
            Bitmap filterBitmap = new(image.Width, image.Height);

            // Generate texture based on the selected filter
            switch (filter)
            {
                case FiltersType.None:
                    break;

                case FiltersType.GrayScale:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.GrayScale, imageSettings);
                    break;

                case FiltersType.Invert:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.Invert, imageSettings);
                    break;

                case FiltersType.Sepia:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.Sepia, imageSettings);
                    break;

                case FiltersType.BlackAndWhite:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.BlackAndWhite, imageSettings);
                    break;

                case FiltersType.Polaroid:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.Polaroid, imageSettings);
                    break;

                case FiltersType.Contour:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.Contour, imageSettings);
                    break;

                case FiltersType.Brightness:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.Brightness, imageSettings);
                    break;

                case FiltersType.Contrast:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.Contrast, imageSettings);
                    break;

                case FiltersType.HueShift:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.HueShift, imageSettings);
                    break;

                case FiltersType.ColorBalance:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.ColorBalance, imageSettings);
                    break;

                case FiltersType.Vintage:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.Vintage, imageSettings);
                    break;

                case FiltersType.Sharpen:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.Sharpen, imageSettings);
                    break;

                case FiltersType.GaussianBlur:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.GaussianBlur, imageSettings);
                    break;

                case FiltersType.Emboss:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.Emboss, imageSettings);
                    break;

                case FiltersType.BoxBlur:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.BoxBlur, imageSettings);
                    break;

                case FiltersType.Laplacian:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.Laplacian, imageSettings);
                    break;

                case FiltersType.EdgeEnhance:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.EdgeEnhance, imageSettings);
                    break;

                case FiltersType.MotionBlur:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.MotionBlur, imageSettings);
                    break;

                case FiltersType.UnsharpMask:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.UnsharpMask, imageSettings);
                    break;

                case FiltersType.DifferenceOfGaussians:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.DifferenceOfGaussians, imageSettings);
                    break;

                case FiltersType.Crosshatch:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.Crosshatch, imageSettings);
                    break;

                case FiltersType.FloydSteinbergDithering:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.FloydSteinbergDithering, imageSettings);
                    break;

                case FiltersType.AnisotropicKuwahara:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.AnisotropicKuwahara, imageSettings);
                    break;

                case FiltersType.SupersamplingAntialiasing:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.SupersamplingAntialiasing, imageSettings);
                    break;

                case FiltersType.PostProcessingAntialiasing:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.PostProcessingAntialiasing, imageSettings);
                    break;

                case FiltersType.PencilSketchEffect:
                    filterBitmap = FiltersStream.FilterImage(filterBitmap, FiltersType.PencilSketchEffect, imageSettings);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(filter), filter, null);
            }

            // Apply the texture to the specified area shape
            switch (shape)
            {
                case MaskShape.Rectangle:
                    filterBitmap = ImageMask.ApplyRectangleMask(filterBitmap, image.Width, image.Height, actualStartPoint);
                    break;

                case MaskShape.Circle:
                    filterBitmap = ImageMask.ApplyCircleMask(filterBitmap, image.Width, image.Height, actualStartPoint);
                    break;

                case MaskShape.Polygon:
                    filterBitmap = ImageMask.ApplyPolygonMask(filterBitmap, (Point[])shapeParams);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(shape), shape, null);
            }

            //overlay both images and return
            return ImageStream.CombineBitmap(image, filterBitmap, 0, 0);
        }
    }
}