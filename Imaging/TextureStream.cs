/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/TextureStream.cs
 * PURPOSE:     Basic stuff for generating textures
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Sources:     https://lodev.org/cgtutor/randomnoise.html
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

// ReSharper disable UnusedMember.Local

namespace Imaging
{
    /// <summary>
    ///     Class that generates Textures
    /// </summary>
    internal static class TextureStream
    {
        /// <summary>
        ///     Generates the noise bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="useSmoothNoise">if set to <c>true</c> [use smooth noise].</param>
        /// <param name="useTurbulence">if set to <c>true</c> [use turbulence].</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <returns>Texture Bitmap</returns>
        internal static Bitmap GenerateNoiseBitmap(
            int width,
            int height,
            int minValue = 0,
            int maxValue = 255,
            int alpha = 255,
            bool useSmoothNoise = false,
            bool useTurbulence = false,
            double turbulenceSize = 64)
        {
            // Validate parameters
            ImageHelper.ValidateParameters(minValue, maxValue, alpha);

            // Generate noise
            var noiseGen = new NoiseGenerator(width, height);

            // Create DirectBitmap
            var noiseBitmap = new DirectBitmap(width, height);
            var pixelData = new List<(int x, int y, Color color)>();

            // Set pixels directly
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var value = useTurbulence
                        ? noiseGen.Turbulence(x, y, turbulenceSize)
                        : useSmoothNoise
                            ? noiseGen.SmoothNoise(x, y)
                            : noiseGen.GetNoise(x, y); // Use GetNoise for basic noise

                    // Normalize value to 0.0 - 1.0 range
                    var normalizedValue = Math.Clamp(value / 255.0, 0.0, 1.0);

                    // Then, apply the color scaling
                    var colorValue = Math.Clamp(minValue + (int)((maxValue - minValue) * normalizedValue), minValue, maxValue);


                    pixelData.Add((x, y, Color.FromArgb(alpha, colorValue, colorValue, colorValue)));
                }
            }

            // Use SIMD-based bulk pixel setting
            noiseBitmap.SetPixelsSimd(pixelData.ToArray());
            pixelData.Clear();

            return noiseBitmap.Bitmap;
        }


        /// <summary>
        ///     Generates the clouds bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <returns>Texture Bitmap</returns>
        internal static Bitmap GenerateCloudsBitmap(
            int width,
            int height,
            int minValue = 0,
            int maxValue = 255,
            int alpha = 255,
            double turbulenceSize = 64)
        {
            ImageHelper.ValidateParameters(minValue, maxValue, alpha);
            var noiseGen = new NoiseGenerator(width, height);

            var cloudsBitmap = new DirectBitmap(width, height);
            var pixelData = new List<(int x, int y, Color color)>();

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                // Generate turbulence value
                var turbulenceValue = noiseGen.Turbulence(x, y, turbulenceSize);

                // Adjust turbulence value like in the C code (divide by 4)
                var L = (byte)Math.Clamp(192 + (int)(turbulenceValue / 4), 192, 230);  // Lightness adjustment

                // Set Hue and Saturation (H = 190 for light blue, S = 200 for muted saturation)
                var H = 190;  // Adjusted Hue value closer to light blue
                var S = 200;  // Reduced Saturation for a more muted, light blue

                // Convert HSL to RGB
                var color = HsLtoRgb(H, S, L);

                // Add the pixel data for SIMD processing
                pixelData.Add((x, y, color));
            }

            // Convert list to array for SIMD processing
            var pixelArray = pixelData.ToArray();
            cloudsBitmap.SetPixelsSimd(pixelArray);
            pixelData.Clear();

            return cloudsBitmap.Bitmap;
        }

        /// <summary>
        ///     Generates the marble bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="xPeriod">The x period.</param>
        /// <param name="yPeriod">The y period.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="turbulencePower">The turbulence power.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <param name="baseColor">Color of the base.</param>
        /// <returns>
        ///     Texture Bitmap
        /// </returns>
        internal static Bitmap GenerateMarbleBitmap(
            int width,
            int height,
            double xPeriod = 5.0,
            double yPeriod = 10.0,
            int alpha = 255,
            double turbulencePower = 5.0,
            double turbulenceSize = 32.0,
            Color baseColor = default)
        {
            baseColor = baseColor == default ? Color.FromArgb(30, 10, 0) : baseColor;
            var noiseGen = new NoiseGenerator(width, height); // Dynamically sized noise

            var marbleBitmap = new DirectBitmap(width, height);
            var pixelData = new List<(int x, int y, Color color)>();

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                // Replace fixed NoiseWidth/NoiseHeight with width/height
                var xyValue = (x * xPeriod / width + y * yPeriod / height) +
                              turbulencePower * noiseGen.Turbulence(x, y, turbulenceSize) / 128.0 +
                              (Math.Sin((x + y) * 0.1) * 0.5); // Slight random distortion

                var sineValue = 255 * Math.Abs(Math.Sin(xyValue * Math.PI * 2));

                var r = Math.Clamp(baseColor.R + (int)sineValue, 0, 255);
                var g = Math.Clamp(baseColor.G + (int)sineValue, 0, 255);
                var b = Math.Clamp(baseColor.B + (int)sineValue, 0, 255);

                pixelData.Add((x, y, Color.FromArgb(alpha, r, g, b)));
            }

            // Use SIMD-based bulk pixel setting
            marbleBitmap.SetPixelsSimd(pixelData.ToArray());
            pixelData.Clear();

            return marbleBitmap.Bitmap;
        }

        /// <summary>
        ///     Generates the wood bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="xyPeriod">The xy period.</param>
        /// <param name="turbulencePower">The turbulence power.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <param name="baseColor">Color of the base.</param>
        /// <returns>
        ///     Texture Bitmap
        /// </returns>
        internal static Bitmap GenerateWoodBitmap(
            int width,
            int height,
            int alpha = 255,
            double xyPeriod = 12.0,
            double turbulencePower = 0.1,
            double turbulenceSize = 32.0,
            Color baseColor = default)
        {
            baseColor = baseColor == default ? Color.FromArgb(80, 30, 30) : baseColor;
            var noiseGen = new NoiseGenerator(width, height);

            var woodBitmap = new DirectBitmap(width, height);
            var pixelData = new List<(int x, int y, Color color)>();

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var xValue = (x - width / 2.0) / width;
                var yValue = (y - height / 2.0) / height;
                var distValue = Math.Sqrt(xValue * xValue + yValue * yValue) +
                                turbulencePower * noiseGen.Turbulence(x, y, turbulenceSize) / 256.0;
                var sineValue = 128.0 * Math.Abs(Math.Sin(2 * xyPeriod * distValue * Math.PI));

                var r = Math.Clamp(baseColor.R + (int)sineValue, 0, 255);
                var g = Math.Clamp(baseColor.G + (int)sineValue, 0, 255);
                var b = Math.Clamp((int)baseColor.B, 0, 255);

                var color = Color.FromArgb(alpha, r, g, b);
                pixelData.Add((x, y, color));
            }

            // Convert list to array for SIMD processing
            var pixelArray = pixelData.ToArray();
            woodBitmap.SetPixelsSimd(pixelArray);
            pixelData.Clear();

            return woodBitmap.Bitmap;
        }

        /// <summary>
        ///     Generates the wave bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="xyPeriod">The xy period.</param>
        /// <param name="turbulencePower">The turbulence power.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <returns>
        ///     Texture Bitmap
        /// </returns>
        internal static Bitmap GenerateWaveBitmap(
            int width,
            int height,
            int alpha = 255,
            double xyPeriod = 12.0,
            double turbulencePower = 0.1,
            double turbulenceSize = 32.0)
        {
            var noiseGen = new NoiseGenerator(width, height);

            var waveBitmap = new DirectBitmap(width, height);
            var pixelData = new List<(int x, int y, Color color)>();

            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var turbulenceValue = noiseGen.Turbulence(x, y, turbulenceSize);
                var xValue = (x - width / 2.0) / width + turbulencePower * turbulenceValue / 256.0;
                var yValue = (y - height / 2.0) / height +
                             turbulencePower * noiseGen.Turbulence(height - y, width - x, turbulenceSize) / 256.0;

                var sineValue = 22.0 *
                                Math.Abs(Math.Sin(xyPeriod * xValue * Math.PI) +
                                         Math.Sin(xyPeriod * yValue * Math.PI));
                var hsvColor = new ColorHsv(sineValue, 1.0, 1.0, alpha);

                pixelData.Add((x, y, hsvColor.GetDrawingColor()));
            }

            // Convert list to array for SIMD processing
            var pixelArray = pixelData.ToArray();
            waveBitmap.SetPixelsSimd(pixelArray);
            pixelData.Clear();

            return waveBitmap.Bitmap;
        }

        /// <summary>
        ///     Generates a crosshatch texture bitmap.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="lineSpacing">The spacing between lines.</param>
        /// <param name="lineColor">The color of the lines.</param>
        /// <param name="lineThickness">The thickness of the lines.</param>
        /// <param name="angle1">The angle of the first set of lines, in degrees.</param>
        /// <param name="angle2">The angle of the second set of lines, in degrees.</param>
        /// <param name="alpha">The alpha value for the color.</param>
        /// <returns>Texture Bitmap</returns>
        internal static Bitmap GenerateCrosshatchBitmap(
            int width,
            int height,
            int lineSpacing = 10,
            Color lineColor = default,
            int lineThickness = 1,
            double angle1 = 45.0,
            double angle2 = 135.0,
            int alpha = 255)
        {
            lineColor = lineColor == default ? Color.Black : lineColor;

            var crosshatchBitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(crosshatchBitmap);
            graphics.Clear(Color.White); // Background color

            using var pen = new Pen(Color.FromArgb(alpha, lineColor), lineThickness);
            // Convert angles from degrees to radians
            var radAngle1 = angle1 * Math.PI / 180.0;
            var radAngle2 = angle2 * Math.PI / 180.0;

            // Calculate the line direction vectors
            var dx1 = Math.Cos(radAngle1);
            var dy1 = Math.Sin(radAngle1);
            var dx2 = Math.Cos(radAngle2);
            var dy2 = Math.Sin(radAngle2);

            // Draw first set of lines
            for (var y = 0; y < height; y += lineSpacing)
                graphics.DrawLine(
                    pen,
                    0, y,
                    (int)(width * dx1 + width * dy1),
                    (int)(y * dx1 + width * dy1));

            for (var x = 0; x < width; x += lineSpacing)
                graphics.DrawLine(
                    pen,
                    x, 0,
                    (int)(x * dx1 + height * dx1),
                    (int)(height * dx1 + height * dy1));

            // Draw second set of lines
            for (var y = 0; y < height; y += lineSpacing)
                graphics.DrawLine(
                    pen,
                    0, y,
                    (int)(width * dx2 + height * dy2),
                    (int)(y * dx2 + height * dy2));

            for (var x = 0; x < width; x += lineSpacing)
                graphics.DrawLine(
                    pen,
                    x, 0,
                    (int)(x * dx2 + width * dx2),
                    (int)(width * dx2 + height * dy2));

            return crosshatchBitmap;
        }

        /// <summary>
        /// Generates a concrete texture bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="minValue">The minimum grayscale value.</param>
        /// <param name="maxValue">The maximum grayscale value.</param>
        /// <param name="alpha">The alpha transparency level.</param>
        /// <param name="xPeriod">The x period, Defines repetition of marble veins.</param>
        /// <param name="yPeriod">The y period,  Defines direction of veins.</param>
        /// <param name="turbulencePower">The turbulence power.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <returns>
        /// Concrete Texture Bitmap
        /// </returns>
        internal static Bitmap GenerateConcreteBitmap(
            int width,
            int height,
            int minValue = 50,
            int maxValue = 200,
            int alpha = 255,
            double xPeriod = 5.0,
            double yPeriod = 10.0,
            double turbulencePower = 5.0,
            double turbulenceSize = 16)
        {
            var noiseGen = new NoiseGenerator(width, height);
            var bitmap = new Bitmap(width, height);

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var xyValue = (x * xPeriod / width) + (y * yPeriod / height) +
                                  (turbulencePower * noiseGen.Turbulence(x, y, turbulenceSize) / 256.0);

                    var sineValue = 256 * Math.Abs(Math.Sin(xyValue * Math.PI));

                    var grayscale = Math.Clamp((int)sineValue, minValue, maxValue);
                    var color = Color.FromArgb(alpha, grayscale, grayscale, grayscale);
                    bitmap.SetPixel(x, y, color);
                }
            }

            return bitmap;
        }

        /// <summary>
        ///     Generates a canvas texture bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="lineSpacing">The spacing between fibers.</param>
        /// <param name="lineColor">The color of the fibers.</param>
        /// <param name="lineThickness">The thickness of the fibers.</param>
        /// <param name="alpha">The alpha transparency level.</param>
        /// <returns>Canvas Texture Bitmap</returns>
        internal static Bitmap GenerateCanvasBitmap(
            int width,
            int height,
            int lineSpacing = 8,
            Color lineColor = default,
            int lineThickness = 1,
            int alpha = 255)
        {
            var canvasBitmap = new DirectBitmap(width, height);


            //no Simd for now
            using (var g = Graphics.FromImage(canvasBitmap.Bitmap))
            {
                g.Clear(Color.White);

                // Draw vertical fibers
                for (var x = 0; x < width; x += lineSpacing)
                    using (var fiberBrush = new SolidBrush(Color.FromArgb(alpha, lineColor)))
                    {
                        g.FillRectangle(fiberBrush, x, 0, lineThickness, height);
                    }

                // Draw horizontal fibers
                for (var y = 0; y < height; y += lineSpacing)
                    using (var fiberBrush = new SolidBrush(Color.FromArgb(alpha, lineColor)))
                    {
                        g.FillRectangle(fiberBrush, 0, y, width, lineThickness);
                    }
            }

            return canvasBitmap.Bitmap;
        }

        /// <summary>
        /// HSL to RGB.
        /// </summary>
        /// <param name="h">The h.</param>
        /// <param name="s">The s.</param>
        /// <param name="l">The l.</param>
        /// <returns>The converted color.</returns>
        private static Color HsLtoRgb(double h, double s, double l)
        {
            h /= 360.0; // Normalize Hue to [0,1]
            s /= 255.0; // Normalize Saturation to [0,1]
            l /= 255.0; // Normalize Lightness to [0,1]

            double r, g, b;

            if (s == 0)
            {
                // Grayscale (no saturation)
                r = g = b = l;
            }
            else
            {
                var q = l < 0.5 ? l * (1 + s) : (l + s - l * s);
                var p = 2 * l - q;

                r = HueToRgb(p, q, h + 1.0 / 3.0);
                g = HueToRgb(p, q, h);
                b = HueToRgb(p, q, h - 1.0 / 3.0);
            }

            return Color.FromArgb(
                255, // Full alpha
                (int)(r * 255),
                (int)(g * 255),
                (int)(b * 255)
            );
        }

        /// <summary>
        /// Hues to RGB.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="q">The q.</param>
        /// <param name="t">The t.</param>
        /// <returns>Hue to Rgb</returns>
        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
            return p;
        }
    }
}