/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/Texture.cs
 * PURPOSE:     Basic stuff for generating textures
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Sources:     https://lodev.org/cgtutor/randomnoise.html
 */

using System;
using System.Collections.Generic;
using System.Drawing;

// ReSharper disable UnusedMember.Local

namespace Imaging
{
    /// <summary>
    ///     Class that generates Textures
    /// </summary>
    internal static class Texture
    {
        /// <summary>
        ///     The noise width
        /// </summary>
        private const int NoiseWidth = 320; // Width for noise generation

        /// <summary>
        ///     The noise height
        /// </summary>
        private const int NoiseHeight = 240; // Height for noise generation

        /// <summary>
        ///     The noise
        /// </summary>
        private static readonly double[,] Noise = new double[NoiseHeight, NoiseWidth];

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
            ValidateParameters(minValue, maxValue, alpha);

            // Generate base noise
            GenerateBaseNoise();

            // Create DirectBitmap
            var noiseBitmap = new DirectBitmap(width, height);

            // Create an enumerable to collect pixel data
            var pixelData = new List<(int x, int y, Color color)>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    double value;

                    if (useTurbulence)
                    {
                        value = Turbulence(x, y, turbulenceSize);
                    }
                    else if (useSmoothNoise)
                    {
                        value = SmoothNoise(x, y);
                    }
                    else
                    {
                        value = Noise[y % NoiseHeight, x % NoiseWidth];
                    }

                    var colorValue = minValue + (int)((maxValue - minValue) * value);
                    colorValue = Math.Max(minValue, Math.Min(maxValue, colorValue));
                    var color = Color.FromArgb(alpha, colorValue, colorValue, colorValue);

                    pixelData.Add((x, y, color));
                }
            }

            // Set pixels using SIMD
            noiseBitmap.SetPixelsSimd(pixelData);

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
            ValidateParameters(minValue, maxValue, alpha);
            GenerateBaseNoise();

            var cloudsBitmap = new DirectBitmap(width, height);
            var pixelData = new List<(int x, int y, Color color)>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var value = Turbulence(x, y, turbulenceSize);
                    var l = Math.Clamp(minValue + (int)(value / 4.0), minValue, maxValue);
                    var color = Color.FromArgb(alpha, l, l, l);
                    pixelData.Add((x, y, color));
                }
            }

            // Convert list to array for SIMD processing
            var pixelArray = pixelData.ToArray();
            cloudsBitmap.SetPixelsSimd(pixelArray);

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
            GenerateBaseNoise();

            var marbleBitmap = new DirectBitmap(width, height);
            var pixelData = new List<(int x, int y, Color color)>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var xyValue = (x * xPeriod / NoiseWidth) + (y * yPeriod / NoiseHeight) +
                                  (turbulencePower * Turbulence(x, y, turbulenceSize) / 256.0);
                    var sineValue = 226 * Math.Abs(Math.Sin(xyValue * Math.PI));
                    var r = Math.Clamp(baseColor.R + (int)sineValue, 0, 255);
                    var g = Math.Clamp(baseColor.G + (int)sineValue, 0, 255);
                    var b = Math.Clamp(baseColor.B + (int)sineValue, 0, 255);

                    var color = Color.FromArgb(alpha, r, g, b);
                    pixelData.Add((x, y, color));
                }
            }

            // Convert list to array for SIMD processing
            var pixelArray = pixelData.ToArray();
            marbleBitmap.SetPixelsSimd(pixelArray);

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
            GenerateBaseNoise();

            var woodBitmap = new DirectBitmap(width, height);
            var pixelData = new List<(int x, int y, Color color)>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var xValue = (x - (width / 2.0)) / width;
                    var yValue = (y - (height / 2.0)) / height;
                    var distValue = Math.Sqrt((xValue * xValue) + (yValue * yValue)) +
                                    (turbulencePower * Turbulence(x, y, turbulenceSize) / 256.0);
                    var sineValue = 128.0 * Math.Abs(Math.Sin(2 * xyPeriod * distValue * Math.PI));

                    var r = Math.Clamp(baseColor.R + (int)sineValue, 0, 255);
                    var g = Math.Clamp(baseColor.G + (int)sineValue, 0, 255);
                    var b = Math.Clamp((int)baseColor.B, 0, 255);

                    var color = Color.FromArgb(alpha, r, g, b);
                    pixelData.Add((x, y, color));
                }
            }

            // Convert list to array for SIMD processing
            var pixelArray = pixelData.ToArray();
            woodBitmap.SetPixelsSimd(pixelArray);

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
            GenerateBaseNoise();

            var waveBitmap = new DirectBitmap(width, height);
            var pixelData = new List<(int x, int y, Color color)>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var xValue = ((x - (width / 2.0)) / width) +
                                 (turbulencePower * Turbulence(x, y, turbulenceSize) / 256.0);
                    var yValue = ((y - (height / 2.0)) / height) +
                                 (turbulencePower * Turbulence(height - y, width - x, turbulenceSize) / 256.0);

                    var sineValue = 22.0 *
                                    Math.Abs(Math.Sin(xyPeriod * xValue * Math.PI) +
                                             Math.Sin(xyPeriod * yValue * Math.PI));
                    var hsvColor = new ColorHsv(sineValue, 1.0, 1.0, alpha);

                    pixelData.Add((x, y, hsvColor.GetDrawingColor()));
                }
            }

            // Convert list to array for SIMD processing
            var pixelArray = pixelData.ToArray();
            waveBitmap.SetPixelsSimd(pixelArray);

            return waveBitmap.Bitmap;
        }

        /// <summary>
        ///     Validates the parameters.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="alpha">The alpha.</param>
        /// <exception cref="ArgumentException">
        ///     minValue and maxValue must be between 0 and 255, and minValue must not be greater than maxValue.
        ///     or
        ///     Alpha must be between 0 and 255.
        /// </exception>
        private static void ValidateParameters(int minValue, int maxValue, int alpha)
        {
            if (minValue is < 0 or > 255 || maxValue is < 0 or > 255 || minValue > maxValue)
            {
                throw new ArgumentException(
                    "minValue and maxValue must be between 0 and 255, and minValue must not be greater than maxValue.");
            }

            if (alpha is < 0 or > 255)
            {
                throw new ArgumentException("Alpha must be between 0 and 255.");
            }
        }

        /// <summary>
        ///     Generates the base noise.
        /// </summary>
        private static void GenerateBaseNoise()
        {
            var random = new Random();
            for (var y = 0; y < NoiseHeight; y++)
            {
                for (var x = 0; x < NoiseWidth; x++)
                {
                    Noise[y, x] = random.NextDouble(); // Random value between 0.0 and 1.0
                }
            }
        }

        /// <summary>
        ///     Turbulence the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="size">The size.</param>
        /// <returns>Generate Turbulence</returns>
        private static double Turbulence(int x, int y, double size)
        {
            var value = 0.0;
            var initialSize = size;
            while (size >= 1)
            {
                value += SmoothNoise(x / size, y / size) * size;
                size /= 2;
            }

            return Math.Abs(value / initialSize);
        }

        /// <summary>
        ///     Smooths the noise.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>Generate Noise</returns>
        private static double SmoothNoise(double x, double y)
        {
            var intX = (int)x;
            var fracX = x - intX;
            var intY = (int)y;
            var fracY = y - intY;

            var v1 = Noise[intY % NoiseHeight, intX % NoiseWidth];
            var v2 = Noise[intY % NoiseHeight, (intX + 1) % NoiseWidth];
            var v3 = Noise[(intY + 1) % NoiseHeight, intX % NoiseWidth];
            var v4 = Noise[(intY + 1) % NoiseHeight, (intX + 1) % NoiseWidth];

            var i1 = Interpolate(v1, v2, fracX);
            var i2 = Interpolate(v3, v4, fracX);

            return Interpolate(i1, i2, fracY);
        }

        /// <summary>
        ///     Interpolates the specified a.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="t">The t.</param>
        /// <returns>Interpolation</returns>
        private static double Interpolate(double a, double b, double t)
        {
            return (a * (1 - t)) + (b * t);
        }
    }
}
