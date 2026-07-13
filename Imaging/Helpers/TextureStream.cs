/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Helpers
 * FILE:        TextureStream.cs
 * PURPOSE:     Basic stuff for generating textures
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using Imaging.Texture;
using System.Drawing;

// ReSharper disable UnusedMember.Local

namespace Imaging.Helpers
{
    /// <summary>
    ///     Class that generates Textures
    /// </summary>
    internal static class TextureStream
    {
        /// <summary>
        /// Generates the noise bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="isMonochrome">if set to <c>true</c> [is monochrome].</param>
        /// <param name="isTiled">if set to <c>true</c> [is tiled].</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <returns>
        /// Texture Bitmap
        /// </returns>
        internal static Bitmap? GenerateNoiseBitmap(
            int width,
            int height,
            int minValue = 0,
            int maxValue = 255,
            int alpha = 255,
            bool isMonochrome = false, // Maps to useSmoothNoise parameter
            bool isTiled = false, // Maps to useTurbulence parameter
            double turbulenceSize = 64)
        {
            ImageHelper.ValidateParameters(minValue, maxValue, alpha);

            var noiseGen = new NoiseGenerator(width, height);
            var buffer = TextureMathEngine.GenerateNoise(
                width, height, noiseGen, minValue, maxValue, alpha, isMonochrome, isTiled, turbulenceSize);
            return buffer.ToManagedBitmap();
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
        internal static Bitmap? GenerateCloudsBitmap(
            int width,
            int height,
            int minValue = 0,
            int maxValue = 255,
            int alpha = 255,
            double turbulenceSize = 64)
        {
            ImageHelper.ValidateParameters(minValue, maxValue, alpha);

            var noiseGen = new NoiseGenerator(width, height);
            var buffer = TextureMathEngine.GenerateClouds(
                width, height, noiseGen, minValue, maxValue, alpha, turbulenceSize);
            return buffer.ToManagedBitmap();
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
        internal static Bitmap? GenerateMarbleBitmap(
            int width,
            int height,
            double xPeriod = 5.0,
            double yPeriod = 10.0,
            int alpha = 255,
            double turbulencePower = 5.0,
            double turbulenceSize = 32.0,
            Color baseColor = default)
        {
            var baseC = baseColor == default ? Color.FromArgb(30, 10, 0) : baseColor;
            var noiseGen = new NoiseGenerator(width, height);
            var buffer = TextureMathEngine.GenerateMarble(
                width, height, noiseGen, xPeriod, yPeriod, alpha, turbulencePower, turbulenceSize, baseC.R, baseC.G,
                baseC.B);
            return buffer.ToManagedBitmap();
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
        internal static Bitmap? GenerateWoodBitmap(
            int width,
            int height,
            int alpha = 255,
            double xyPeriod = 12.0,
            double turbulencePower = 0.1,
            double turbulenceSize = 32.0,
            Color baseColor = default)
        {
            var baseC = baseColor == default ? Color.FromArgb(80, 30, 30) : baseColor;
            var noiseGen = new NoiseGenerator(width, height);
            var buffer = TextureMathEngine.GenerateWood(
                width, height, noiseGen, alpha, xyPeriod, turbulencePower, turbulenceSize, baseC.R, baseC.G, baseC.B);
            return buffer.ToManagedBitmap();
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
        internal static Bitmap? GenerateWaveBitmap(
            int width,
            int height,
            int alpha = 255,
            double xyPeriod = 12.0,
            double turbulencePower = 0.1,
            double turbulenceSize = 32.0)
        {
            var noiseGen = new NoiseGenerator(width, height);
            var buffer = TextureMathEngine.GenerateWave(
                width, height, noiseGen, alpha, xyPeriod, turbulencePower, turbulenceSize);
            return buffer.ToManagedBitmap();
        }

        /// <summary>
        ///     Generates a crosshatch texture bitmap.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="lineSpacing">The spacing between lines.</param>
        /// <param name="lineColor">The color of the lines.</param>
        /// <param name="lineThickness">The thickness of the lines.</param>
        /// <param name="anglePrimary">The angle of the first set of lines, in degrees.</param>
        /// <param name="angleSecondary">The angle of the second set of lines, in degrees.</param>
        /// <param name="alpha">The alpha value for the color.</param>
        /// <returns>Texture Bitmap</returns>
        internal static Bitmap? GenerateCrosshatchBitmap(
            int width,
            int height,
            int lineSpacing = 50,
            Color lineColor = default,
            int lineThickness = 1,
            double anglePrimary = 45.0,
            double angleSecondary = -45.0,
            int alpha = 255)
        {
            var lineC = lineColor == default ? Color.Black : lineColor;
            var buffer = TextureMathEngine.GenerateCrosshatch(
                width, height, lineSpacing, lineThickness, anglePrimary, angleSecondary, alpha, lineC.R, lineC.G,
                lineC.B);
            return buffer.ToManagedBitmap();
        }

        /// <summary>
        ///     Generates a concrete texture bitmap.
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
        ///     Concrete Texture Bitmap
        /// </returns>
        internal static Bitmap? GenerateConcreteBitmap(
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
            var buffer = TextureMathEngine.GenerateConcrete(
                width, height, noiseGen, minValue, maxValue, alpha, xPeriod, yPeriod, turbulencePower, turbulenceSize);
            return buffer.ToManagedBitmap();
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
        /// <param name="waveFrequency">The wave frequency.</param>
        /// <param name="waveAmplitude">The wave amplitude.</param>
        /// <param name="randomizationFactor">The randomization factor.</param>
        /// <param name="edgeJaggednessLimit">The edge jaggedness limit.</param>
        /// <param name="jaggednessThreshold">The jaggedness threshold.</param>
        /// <returns>
        ///     Canvas Texture Bitmap
        /// </returns>
        internal static Bitmap? GenerateCanvasBitmap(
            int width,
            int height,
            int lineSpacing = 8,
            Color lineColor = default,
            int lineThickness = 1,
            int alpha = 255,
            double waveFrequency = 0.02,
            double waveAmplitude = 3,
            double randomizationFactor = 1.5,
            int edgeJaggednessLimit = 20, // Limit jaggedness to within 20% of the image's edges
            int jaggednessThreshold = 10) // Reduced jagged chance for edge lines
        {
            var lineC = lineColor == default ? Color.Black : lineColor;
            var buffer = TextureMathEngine.GenerateCanvas(
                width, height, lineSpacing, lineThickness, alpha, waveFrequency, waveAmplitude, randomizationFactor,
                edgeJaggednessLimit, jaggednessThreshold, lineC.R, lineC.G, lineC.B);
            return buffer.ToManagedBitmap();
        }

        /// <summary>
        /// Generates the cellular bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="cellSize">The size of the cells.</param>
        /// <param name="alpha">The alpha transparency level.</param>
        /// <param name="centerColor">The color of the cell centers.</param>
        /// <param name="edgeColor">The color of the cell edges.</param>
        /// <returns>The generated cellular bitmap.</returns>
        internal static Bitmap? GenerateCellularBitmap(
            int width,
            int height,
            int cellSize = 32,
            int alpha = 255,
            Color centerColor = default,
            Color edgeColor = default)
        {
            var centerC = centerColor == default ? Color.FromArgb(150, 150, 150) : centerColor;
            var edgeC = edgeColor == default ? Color.FromArgb(40, 40, 40) : edgeColor;

            var buffer = TextureMathEngine.GenerateCellular(
                width, height, cellSize, alpha,
                centerC.R, centerC.G, centerC.B,
                edgeC.R, edgeC.G, edgeC.B);

            return buffer.ToManagedBitmap();
        }

        /// <summary>
        /// Generates the color mapped bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="colorRamp">The color ramp.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <param name="alpha">The alpha.</param>
        /// <returns>The generated color mapped bitmap.</returns>
        internal static Bitmap? GenerateColorMappedBitmap(
            int width,
            int height,
            Color[]? colorRamp,
            double turbulenceSize = 64.0,
            int alpha = 255)
        {
            var noiseGen = new NoiseGenerator(width, height);

            // Flatten Color array into a byte array for the high-performance math engine
            var rgbRamp = new byte[colorRamp.Length * 3];
            for (var i = 0; i < colorRamp.Length; i++)
            {
                rgbRamp[i * 3] = colorRamp[i].R;
                rgbRamp[i * 3 + 1] = colorRamp[i].G;
                rgbRamp[i * 3 + 2] = colorRamp[i].B;
            }

            var buffer = TextureMathEngine.GenerateColorMapped(
                width, height, noiseGen, rgbRamp, turbulenceSize, alpha);

            return buffer.ToManagedBitmap();
        }
    }
}
