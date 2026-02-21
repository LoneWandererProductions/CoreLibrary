/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     Imaging.Helpers
* FILE:        ColorHsv.cs
* PURPOSE:     General Conversions for images via my Hsv class
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

using System;
using System.Drawing;

namespace Imaging.Helpers
{
    /// <summary>
    ///     Another round of minor Image manipulations
    /// </summary>
    internal static class ImageStreamHsv
    {
        /// <summary>
        ///     Adjusts the hue.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="hueShift">The hue shift.</param>
        /// <returns>Processed Image</returns>
        internal static Bitmap AdjustHue(Bitmap image, double hueShift)
        {
            return ProcessImage(image, colorHsv =>
            {
                // Adjust hue
                colorHsv.H = (colorHsv.H + hueShift) % 360;
                colorHsv.H = colorHsv.H < 0 ? colorHsv.H + 360 : colorHsv.H;
            });
        }

        /// <summary>
        ///     Adjusts the saturation.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="saturationFactor">The saturation factor.</param>
        /// <returns>Processed Image</returns>
        internal static Bitmap AdjustSaturation(Bitmap image, double saturationFactor)
        {
            return ProcessImage(image, colorHsv => colorHsv.S = ImageHelper.Clamp(colorHsv.S * saturationFactor, 0, 1));
        }

        /// <summary>
        ///     Adjusts the brightness.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="brightnessFactor">The brightness factor.</param>
        /// <returns>Processed Image</returns>
        internal static Bitmap AdjustBrightness(Bitmap image, double brightnessFactor)
        {
            return ProcessImage(image, colorHsv => colorHsv.V = ImageHelper.Clamp(colorHsv.V * brightnessFactor, 0, 1));
        }

        /// <summary>
        ///     Applies the gamma correction.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="gamma">The gamma.</param>
        /// <returns>Processed Image</returns>
        /// <exception cref="ArgumentOutOfRangeException">gamma - Gamma must be greater than 0.</exception>
        internal static Bitmap ApplyGammaCorrection(Bitmap image, double gamma)
        {
            if (gamma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gamma), "Gamma must be greater than 0.");
            }

            var gammaCorrection = 1.0 / gamma;
            return ProcessImage(image, colorHsv =>
            {
                colorHsv.R = ImageHelper.Clamp(Math.Pow(colorHsv.R / 255.0, gammaCorrection) * 255);
                colorHsv.G = ImageHelper.Clamp(Math.Pow(colorHsv.G / 255.0, gammaCorrection) * 255);
                colorHsv.B = ImageHelper.Clamp(Math.Pow(colorHsv.B / 255.0, gammaCorrection) * 255);
            });
        }

        /// <summary>
        ///     Processes the image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="pixelOp">The pixel operation.</param>
        /// <returns>Processed Image</returns>
        private static Bitmap ProcessImage(Bitmap image, Action<ColorHsv> pixelOp)
        {
            ImageHelper.ValidateImage(nameof(ProcessImage), image);

            var source = new DirectBitmap(image);
            var result = new DirectBitmap(source.Width, source.Height);

            for (var y = 0; y < source.Height; y++)
            {
                for (var x = 0; x < source.Width; x++)
                {
                    var c = source.GetPixel(x, y);

                    // Convert RGB -> HSV using your shared class
                    var hsv = new ColorHsv(c.R, c.G, c.B, c.A);

                    // User provided operation on HSV
                    pixelOp(hsv);

                    // Convert HSV -> RGB (your class handles it)
                    result.SetPixel(x, y, Color.FromArgb(hsv.A, hsv.R, hsv.G, hsv.B));
                }
            }

            return new Bitmap(result.UnsafeBitmap);
        }
    }
}
