/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/ImageExtension.cs
 * PURPOSE:     Image Extensions, I think that are helpful and should be there from the beginning
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Drawing;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;

namespace Imaging
{
    /// <summary>
    ///     Image Extension Methods
    /// </summary>
    public static class ImageExtension
    {
        /// <summary>
        ///     Extension Method
        ///     Converts Bitmap to BitmapImage.
        /// </summary>
        /// <param name="bmp">The BMP.</param>
        /// <returns>
        ///     A BitmapImage
        /// </returns>
        public static BitmapImage ToBitmapImage(this Bitmap bmp)
        {
            return ImageStreamMedia.BitmapToBitmapImage(bmp);
        }

        /// <summary>
        ///     Extension Method
        ///     Converts Bitmap to BitmapImage.
        /// </summary>
        /// <param name="bmp">The BMP.</param>
        /// <returns>
        ///     A BitmapImage
        /// </returns>
        public static Bitmap ToBitmap(this BitmapImage bmp)
        {
            return ImageStreamMedia.BitmapImageToBitmap(bmp);
        }

        /// <summary>
        ///     Extension Method
        ///     Converts Image to BitmapImage.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>
        ///     A BitmapImage
        /// </returns>
        public static BitmapImage ToBitmapImage(this Image image)
        {
            var bitmap = new Bitmap(image);
            return bitmap.ToBitmapImage();
        }

        /// <summary>
        ///     Converts to argb.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>Id of the color</returns>
        public static int ToArgb(this Color color)
        {
            // ARGB format: (Alpha << 24) | (Red << 16) | (Green << 8) | Blue
            return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
        }

        /// <summary>
        ///     Converts the color to open gl format.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>Format accpeted by OpenTK</returns>
        public static float[] ConvertColorToOpenGLFormat(this Color color)
        {
            // Convert ARGB (0-255) to RGBA (0.0-1.0)
            var normalizedColor = new float[4];

            normalizedColor[0] = color.R / 255.0f; // Red
            normalizedColor[1] = color.G / 255.0f; // Green
            normalizedColor[2] = color.B / 255.0f; // Blue
            normalizedColor[3] = color.A / 255.0f; // Alpha

            return normalizedColor;
        }
    }
}
