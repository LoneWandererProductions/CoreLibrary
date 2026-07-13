/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Texture
 * FILE:        TextureMathEngine.cs
 * PURPOSE:     General math engine for generating procedural textures.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Runtime.CompilerServices;

namespace Imaging.Texture
{
    /// <summary>
    /// General math engine for generating procedural textures.
    /// </summary>
    public static class TextureMathEngine
    {
        /// <summary>
        /// Generates the noise.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGenInstance">The noise gen instance.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="useSmoothNoise">if set to <c>true</c> [use smooth noise].</param>
        /// <param name="useTurbulence">if set to <c>true</c> [use turbulence].</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <returns>RawTextureBuffer containing the generated noise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static RawTextureBuffer GenerateNoise(int width,
            int height,
            object noiseGenInstance, // Pass your custom NoiseGenerator wrapper
            int minValue = 0,
            int maxValue = 255,
            int alpha = 255,
            bool useSmoothNoise = false,
            bool useTurbulence = false,
            double turbulenceSize = 64)
        {
            var buffer = new RawTextureBuffer(width, height);
            var span = buffer.AsSpan();
            dynamic noiseGen = noiseGenInstance; // Resolves method call dynamically based on your class structure

            var idx = 0;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var value = useTurbulence
                        ? (double)noiseGen.Turbulence(x, y, turbulenceSize)
                        : useSmoothNoise
                            ? (double)noiseGen.SmoothNoise(x, y)
                            : (double)noiseGen.GetNoise(x, y);

                    var normalized = Math.Clamp(value / 255.0, 0.0, 1.0);
                    var colorVal = (byte)Math.Clamp(minValue + (int)((maxValue - minValue) * normalized), minValue,
                        maxValue);

                    span[idx++] = colorVal; // B
                    span[idx++] = colorVal; // G
                    span[idx++] = colorVal; // R
                    span[idx++] = (byte)alpha; // A
                }
            }

            return buffer;
        }

        /// <summary>
        /// Generates the wood.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGenInstance">The noise gen instance.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="xyPeriod">The xy period.</param>
        /// <param name="turbulencePower">The turbulence power.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <param name="baseR">The base r.</param>
        /// <param name="baseG">The base g.</param>
        /// <param name="baseB">The base b.</param>
        /// <returns>RawTextureBuffer containing the generated wood texture.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static RawTextureBuffer GenerateWood(int width,
            int height,
            object noiseGenInstance,
            int alpha = 255,
            double xyPeriod = 12.0,
            double turbulencePower = 0.1,
            double turbulenceSize = 32.0,
            byte baseR = 80,
            byte baseG = 30,
            byte baseB = 30)
        {
            var buffer = new RawTextureBuffer(width, height);
            var span = buffer.AsSpan();
            dynamic noiseGen = noiseGenInstance;

            var idx = 0;
            var halfW = width / 2.0;
            var halfH = height / 2.0;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var xValue = (x - halfW) / width;
                    var yValue = (y - halfH) / height;

                    var distValue = Math.Sqrt(xValue * xValue + yValue * yValue) +
                                    turbulencePower * (double)noiseGen.Turbulence(x, y, turbulenceSize) / 256.0;

                    var sineValue = 128.0 * Math.Abs(Math.Sin(2.0 * xyPeriod * distValue * Math.PI));

                    span[idx++] = baseB; // B (Natural wood grain accent bypass)
                    span[idx++] = (byte)Math.Clamp(baseG + (int)sineValue, 0, 255); // G
                    span[idx++] = (byte)Math.Clamp(baseR + (int)sineValue, 0, 255); // R
                    span[idx++] = (byte)alpha; // A
                }
            }

            return buffer;
        }

        /// <summary>
        /// Generates the crosshatch.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="lineSpacing">The line spacing.</param>
        /// <param name="lineThickness">The line thickness.</param>
        /// <param name="anglePrimary">The angle primary.</param>
        /// <param name="angleSecondary">The angle secondary.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="lineR">The line r.</param>
        /// <param name="lineG">The line g.</param>
        /// <param name="lineB">The line b.</param>
        /// <param name="bgR">The bg r.</param>
        /// <param name="bgG">The bg g.</param>
        /// <param name="bgB">The bg b.</param>
        /// <param name="bgA">The bg a.</param>
        /// <returns>RawTextureBuffer containing the generated crosshatch texture.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static RawTextureBuffer GenerateCrosshatch(int width,
            int height,
            int lineSpacing = 50,
            int lineThickness = 1,
            double anglePrimary = 45.0,
            double angleSecondary = -45.0,
            int alpha = 255,
            byte lineR = 0,
            byte lineG = 0,
            byte lineB = 0,
            byte bgR = 0,
            byte bgG = 0,
            byte bgB = 0,
            byte bgA = 0) // Defaults to pure transparent background
        {
            var buffer = new RawTextureBuffer(width, height);
            var span = buffer.AsSpan();

            var radPrimary = anglePrimary * Math.PI / 180.0;
            var radSecondary = angleSecondary * Math.PI / 180.0;

            double cosP = Math.Cos(radPrimary), sinP = Math.Sin(radPrimary);
            double cosS = Math.Cos(radSecondary), sinS = Math.Sin(radSecondary);
            var halfThickness = lineThickness / 2.0;

            var idx = 0;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    // Compute coordinate projections along the continuous line axes (keep signs)
                    var projP = x * sinP - y * cosP;
                    var projS = x * sinS - y * cosS;

                    // Find the distance to the absolute nearest line center to avoid origin mirror thickness anomalies
                    var nearestP = Math.Round(projP / lineSpacing) * lineSpacing;
                    var nearestS = Math.Round(projS / lineSpacing) * lineSpacing;

                    var distP = Math.Abs(projP - nearestP);
                    var distS = Math.Abs(projS - nearestS);

                    // Normalize distances to true pixel steps using the dominant projection angle component.
                    // This forces lines to maintain an identical, uniform pixel width across the entire texture canvas.
                    var pixelDistP = distP / Math.Max(Math.Abs(sinP), Math.Abs(cosP));
                    var pixelDistS = distS / Math.Max(Math.Abs(sinS), Math.Abs(cosS));

                    var isLine = (pixelDistP <= halfThickness) || (pixelDistS <= halfThickness);

                    if (isLine)
                    {
                        span[idx++] = lineB;
                        span[idx++] = lineG;
                        span[idx++] = lineR;
                        span[idx++] = (byte)alpha;
                    }
                    else
                    {
                        span[idx++] = bgB;
                        span[idx++] = bgG;
                        span[idx++] = bgR;
                        span[idx++] = bgA;
                    }
                }
            }

            return buffer;
        }

        /// <summary>
        /// Generates industrial solid concrete stone patterns.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGenInstance">The noise gen instance.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="xPeriod">The x period.</param>
        /// <param name="yPeriod">The y period.</param>
        /// <param name="turbulencePower">The turbulence power.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <returns>A raw texture buffer containing the generated concrete texture.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static RawTextureBuffer GenerateConcrete(int width,
            int height,
            object noiseGenInstance,
            int minValue = 50,
            int maxValue = 200,
            int alpha = 255,
            double xPeriod = 5.0,
            double yPeriod = 10.0,
            double turbulencePower = 5.0,
            double turbulenceSize = 16)
        {
            var buffer = new RawTextureBuffer(width, height);
            var span = buffer.AsSpan();
            dynamic noiseGen = noiseGenInstance;

            var idx = 0;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var xyValue = x * xPeriod / width + y * yPeriod / height +
                                  turbulencePower * (double)noiseGen.Turbulence(x, y, turbulenceSize) / 256.0;

                    var sineValue = 256 * Math.Abs(Math.Sin(xyValue * Math.PI));
                    var grayscale = (byte)Math.Clamp((int)sineValue, minValue, maxValue);

                    span[idx++] = grayscale; // B
                    span[idx++] = grayscale; // G
                    span[idx++] = grayscale; // R
                    span[idx++] = (byte)alpha; // A
                }
            }

            return buffer;
        }

        /// <summary>
        /// Generates interlocking woven canvas fabric textures with deterministic line cutoffs.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="lineSpacing">The line spacing.</param>
        /// <param name="lineThickness">The line thickness.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="waveFrequency">The wave frequency.</param>
        /// <param name="waveAmplitude">The wave amplitude.</param>
        /// <param name="randomizationFactor">The randomization factor.</param>
        /// <param name="edgeJaggednessLimit">The edge jaggedness limit.</param>
        /// <param name="jaggednessThreshold">The jaggedness threshold.</param>
        /// <param name="fiberR">The fiber r.</param>
        /// <param name="fiberG">The fiber g.</param>
        /// <param name="fiberB">The fiber b.</param>
        /// <param name="bgR">The bg r.</param>
        /// <param name="bgG">The bg g.</param>
        /// <param name="bgB">The bg b.</param>
        /// <param name="bgA">The bg a.</param>
        /// <returns>A raw texture buffer containing the generated canvas texture.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static RawTextureBuffer GenerateCanvas(int width,
            int height,
            int lineSpacing = 8,
            int lineThickness = 1,
            int alpha = 255,
            double waveFrequency = 0.02,
            double waveAmplitude = 3.0,
            double randomizationFactor = 1.5,
            int edgeJaggednessLimit = 20,
            int jaggednessThreshold = 10,
            byte fiberR = 0,
            byte fiberG = 0,
            byte fiberB = 0,
            byte bgR = 0,
            byte bgG = 0,
            byte bgB = 0,
            byte bgA = 0)
        {
            var buffer = new RawTextureBuffer(width, height);
            var span = buffer.AsSpan();

            // Pre-calculate line limits using standard Random seed matching to fully duplicate legacy logic
            var rand = new Random();
            var halfThickness = lineThickness / 2.0;

            // Precompute cutoffs for all layout coordinates
            var limitY = height / Math.Max(1, edgeJaggednessLimit);
            var limitX = width / Math.Max(1, edgeJaggednessLimit);

            var maxLinesX = width / lineSpacing + 1;
            var vStart = new int[maxLinesX];
            var vEnd = new int[maxLinesX];
            for (var i = 0; i < maxLinesX; i++)
            {
                var cut = rand.Next(0, 100) < jaggednessThreshold;
                vStart[i] = cut ? rand.Next(0, limitY) : 0;
                vEnd[i] = cut ? height - rand.Next(0, limitY) : height;
            }

            var maxLinesY = height / lineSpacing + 1;
            var hStart = new int[maxLinesY];
            var hEnd = new int[maxLinesY];
            for (var i = 0; i < maxLinesY; i++)
            {
                var cut = rand.Next(0, 100) < jaggednessThreshold;
                hStart[i] = cut ? rand.Next(0, limitX) : 0;
                hEnd[i] = cut ? width - rand.Next(0, limitX) : width;
            }

            // Execute raw scanning step pipeline pass
            var idx = 0;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var matchFiber = false;

                    // Evaluate matching alignments for Vertical fibers
                    var vLineIdx = x / lineSpacing;
                    if (y >= vStart[Math.Clamp(vLineIdx, 0, maxLinesX - 1)] &&
                        y < vEnd[Math.Clamp(vLineIdx, 0, maxLinesX - 1)])
                    {
                        var xOffset = waveAmplitude * Math.Sin(waveFrequency * y) +
                                      randomizationFactor * (rand.NextDouble() - 0.5);
                        var dist = Math.Abs((x - xOffset) % lineSpacing);
                        if (dist <= halfThickness || dist >= lineSpacing - halfThickness) matchFiber = true;
                    }

                    // Evaluate matching alignments for Horizontal fibers
                    var hLineIdx = y / lineSpacing;
                    if (!matchFiber && x >= hStart[Math.Clamp(hLineIdx, 0, maxLinesY - 1)] &&
                        x < hEnd[Math.Clamp(hLineIdx, 0, maxLinesY - 1)])
                    {
                        var yOffset = waveAmplitude * Math.Sin(waveFrequency * x) +
                                      randomizationFactor * (rand.NextDouble() - 0.5);
                        var dist = Math.Abs((y - yOffset) % lineSpacing);
                        if (dist <= halfThickness || dist >= lineSpacing - halfThickness) matchFiber = true;
                    }

                    if (matchFiber)
                    {
                        span[idx++] = fiberB;
                        span[idx++] = fiberG;
                        span[idx++] = fiberR;
                        span[idx++] = (byte)alpha;
                    }
                    else
                    {
                        span[idx++] = bgB;
                        span[idx++] = bgG;
                        span[idx++] = bgR;
                        span[idx++] = bgA;
                    }
                }
            }

            return buffer;
        }

        /// <summary>
        /// Generates the clouds texture.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGenInstance">The noise gen instance.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <returns>Texture buffer with clouds pattern.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static RawTextureBuffer GenerateClouds(int width,
            int height,
            object noiseGenInstance,
            int minValue = 0,
            int maxValue = 255,
            int alpha = 255,
            double turbulenceSize = 64)
        {
            var buffer = new RawTextureBuffer(width, height);
            var span = buffer.AsSpan();
            dynamic noiseGen = noiseGenInstance;

            var idx = 0;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var turbulenceValue = (double)noiseGen.Turbulence(x, y, turbulenceSize);
                    var l = Math.Clamp(192 + (int)(turbulenceValue / 4), 192, 230);

                    // Light blue sky parameters (H=190, S=200)
                    var (r, g, b) = ColorConverter.HslToRgbTuple(190, 200, l);

                    span[idx++] = b;
                    span[idx++] = g;
                    span[idx++] = r;
                    span[idx++] = (byte)alpha;
                }
            }

            return buffer;
        }

        /// <summary>
        /// Generates the marble texture.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGenInstance">The noise gen instance.</param>
        /// <param name="xPeriod">The x period.</param>
        /// <param name="yPeriod">The y period.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="turbulencePower">The turbulence power.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <param name="baseR">The base r.</param>
        /// <param name="baseG">The base g.</param>
        /// <param name="baseB">The base b.</param>
        /// <returns>Texture buffer with marble pattern.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static RawTextureBuffer GenerateMarble(int width,
            int height,
            object noiseGenInstance,
            double xPeriod = 5.0,
            double yPeriod = 10.0,
            int alpha = 255,
            double turbulencePower = 5.0,
            double turbulenceSize = 32.0,
            byte baseR = 30,
            byte baseG = 10,
            byte baseB = 0)
        {
            var buffer = new RawTextureBuffer(width, height);
            var span = buffer.AsSpan();
            dynamic noiseGen = noiseGenInstance;

            var idx = 0;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var xyValue = x * xPeriod / width + y * yPeriod / height +
                                  turbulencePower * (double)noiseGen.Turbulence(x, y, turbulenceSize) / 128.0 +
                                  Math.Sin((x + y) * 0.1) * 0.5;

                    var sineValue = 255 * Math.Abs(Math.Sin(xyValue * Math.PI * 2));

                    span[idx++] = (byte)Math.Clamp(baseB + (int)sineValue, 0, 255); // B
                    span[idx++] = (byte)Math.Clamp(baseG + (int)sineValue, 0, 255); // G
                    span[idx++] = (byte)Math.Clamp(baseR + (int)sineValue, 0, 255); // R
                    span[idx++] = (byte)alpha; // A
                }
            }

            return buffer;
        }

        /// <summary>
        /// Generates the wave texture.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGenInstance">The noise gen instance.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="xyPeriod">The xy period.</param>
        /// <param name="turbulencePower">The turbulence power.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <returns>Texture buffer with wave pattern.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static RawTextureBuffer GenerateWave(int width,
            int height,
            object noiseGenInstance,
            int alpha = 255,
            double xyPeriod = 12.0,
            double turbulencePower = 0.1,
            double turbulenceSize = 32.0)
        {
            var buffer = new RawTextureBuffer(width, height);
            var span = buffer.AsSpan();
            dynamic noiseGen = noiseGenInstance;

            var idx = 0;
            var halfW = width / 2.0;
            var halfH = height / 2.0;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var turbulenceValue = (double)noiseGen.Turbulence(x, y, turbulenceSize);
                    var xValue = (x - halfW) / width + turbulencePower * turbulenceValue / 256.0;
                    var yValue = (y - halfH) / height +
                                 turbulencePower * (double)noiseGen.Turbulence(height - y, width - x, turbulenceSize) /
                                 256.0;

                    var sineValue = 22.0 *
                                    Math.Abs(Math.Sin(xyPeriod * xValue * Math.PI) +
                                             Math.Sin(xyPeriod * yValue * Math.PI));

                    var hue = sineValue % 360.0;
                    if (hue < 0) hue += 360.0;

                    var (r, g, b) = ColorConverter.HsvToRgbTuple(hue, 1.0, 1.0);

                    span[idx++] = b;
                    span[idx++] = g;
                    span[idx++] = r;
                    span[idx++] = (byte)alpha;
                }
            }

            return buffer;
        }

        /// <summary>
        /// Generates Cellular (Voronoi/Worley) noise. Perfect for cobblestone, dragon scales, or cracked earth.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="cellSize">Size of the cell.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="centerR">The center r.</param>
        /// <param name="centerG">The center g.</param>
        /// <param name="centerB">The center b.</param>
        /// <param name="edgeR">The edge r.</param>
        /// <param name="edgeG">The edge g.</param>
        /// <param name="edgeB">The edge b.</param>
        /// <returns>The generated texture buffer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static RawTextureBuffer GenerateCellular(int width,
            int height,
            int cellSize = 32,
            int alpha = 255,
            byte centerR = 100,
            byte centerG = 100,
            byte centerB = 100,
            byte edgeR = 20,
            byte edgeG = 20,
            byte edgeB = 20)
        {
            var buffer = new RawTextureBuffer(width, height);
            var span = buffer.AsSpan();
            var rand = new Random(12345); // Fixed seed for stable feature points

            // Precompute feature points for each grid cell to speed up rendering
            var gridCols = (width / cellSize) + 2;
            var gridRows = (height / cellSize) + 2;
            var featurePointsX = new int[gridCols, gridRows];
            var featurePointsY = new int[gridCols, gridRows];

            for (var y = 0; y < gridRows; y++)
            {
                for (var x = 0; x < gridCols; x++)
                {
                    featurePointsX[x, y] = (x * cellSize) + rand.Next(0, cellSize);
                    featurePointsY[x, y] = (y * cellSize) + rand.Next(0, cellSize);
                }
            }

            var idx = 0;
            var maxDist = cellSize * 1.2; // Approximate max distance for normalization

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var cellX = x / cellSize;
                    var cellY = y / cellSize;

                    var minDist = double.MaxValue;

                    // Check surrounding 3x3 cells for the closest feature point
                    for (var offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        for (var offsetX = -1; offsetX <= 1; offsetX++)
                        {
                            var checkX = Math.Clamp(cellX + offsetX, 0, gridCols - 1);
                            var checkY = Math.Clamp(cellY + offsetY, 0, gridRows - 1);

                            double distX = x - featurePointsX[checkX, checkY];
                            double distY = y - featurePointsY[checkX, checkY];
                            var dist = Math.Sqrt(distX * distX + distY * distY);

                            if (dist < minDist) minDist = dist;
                        }
                    }

                    // Normalize distance and interpolate between center and edge color
                    var factor = Math.Clamp(minDist / maxDist, 0.0, 1.0);

                    span[idx++] = (byte)(centerB + (edgeB - centerB) * factor); // B
                    span[idx++] = (byte)(centerG + (edgeG - centerG) * factor); // G
                    span[idx++] = (byte)(centerR + (edgeR - centerR) * factor); // R
                    span[idx++] = (byte)alpha; // A
                }
            }

            return buffer;
        }

        /// <summary>
        /// Generates a texture by mapping turbulence noise to a predefined array of colors (Color Ramp).
        /// Ideal for lava, magic auras, and water.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGenInstance">The noise gen instance.</param>
        /// <param name="colorRampRgb">The color ramp RGB.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <param name="alpha">The alpha.</param>
        /// <returns>The generated texture buffer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static RawTextureBuffer GenerateColorMapped(int width,
            int height,
            object noiseGenInstance,
            byte[] colorRampRgb, // Format: [R1,G1,B1, R2,G2,B2, ...]
            double turbulenceSize = 64.0,
            int alpha = 255)
        {
            var buffer = new RawTextureBuffer(width, height);
            var span = buffer.AsSpan();
            dynamic noiseGen = noiseGenInstance;

            var numColors = colorRampRgb.Length / 3;
            var idx = 0;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    // Get normalized turbulence (0.0 to 1.0)
                    var noiseValue = (double)noiseGen.Turbulence(x, y, turbulenceSize) / 255.0;
                    noiseValue = Math.Clamp(noiseValue, 0.0, 0.999);

                    // Map to color ramp
                    var rampPos = noiseValue * (numColors - 1);
                    var index1 = (int)Math.Floor(rampPos);
                    var index2 = Math.Min(index1 + 1, numColors - 1);
                    var blend = rampPos - index1;

                    // Fetch RGB from flattened array
                    int r1 = colorRampRgb[index1 * 3];
                    int g1 = colorRampRgb[index1 * 3 + 1];
                    int b1 = colorRampRgb[index1 * 3 + 2];

                    int r2 = colorRampRgb[index2 * 3];
                    int g2 = colorRampRgb[index2 * 3 + 1];
                    int b2 = colorRampRgb[index2 * 3 + 2];

                    // Lerp colors
                    span[idx++] = (byte)(b1 + (b2 - b1) * blend); // B
                    span[idx++] = (byte)(g1 + (g2 - g1) * blend); // G
                    span[idx++] = (byte)(r1 + (r2 - r1) * blend); // R
                    span[idx++] = (byte)alpha; // A
                }
            }

            return buffer;
        }

        /// <summary>
        /// Generates the advanced cellular.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="cellSize">Size of the cell.</param>
        /// <param name="alpha">The alpha.</param>
        /// <param name="centerRgb">The center RGB.</param>
        /// <param name="edgeRgb">The edge RGB.</param>
        /// <returns>The generated raw texture buffer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static RawTextureBuffer GenerateAdvancedCellular(int width,
            int height,
            int cellSize,
            int alpha,
            byte[] centerRgb,
            byte[] edgeRgb)
        {
            var buffer = new RawTextureBuffer(width, height);
            var span = buffer.AsSpan();
            var rand = new Random(12345);

            var gridCols = (width / cellSize) + 2;
            var gridRows = (height / cellSize) + 2;
            var featurePointsX = new int[gridCols, gridRows];
            var featurePointsY = new int[gridCols, gridRows];

            // Generate stable feature points
            for (var y = 0; y < gridRows; y++)
            {
                for (var x = 0; x < gridCols; x++)
                {
                    featurePointsX[x, y] = (x * cellSize) + rand.Next(0, cellSize);
                    featurePointsY[x, y] = (y * cellSize) + rand.Next(0, cellSize);
                }
            }

            var idx = 0;
            var normalizationScale = cellSize * 0.7;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var cellX = x / cellSize;
                    var cellY = y / cellSize;

                    var f1 = double.MaxValue; // Closest
                    var f2 = double.MaxValue; // Second closest

                    // FIX: Pre-clamp loop bounds globally instead of clamping inside the loop.
                    // Clamping inside caused border cells to evaluate the same cell index multiple times,
                    // corrupting the second-closest (f2) tracker calculation.
                    var startX = Math.Max(0, cellX - 1);
                    var endX = Math.Min(gridCols - 1, cellX + 1);
                    var startY = Math.Max(0, cellY - 1);
                    var endY = Math.Min(gridRows - 1, cellY + 1);

                    for (var checkY = startY; checkY <= endY; checkY++)
                    {
                        for (var checkX = startX; checkX <= endX; checkX++)
                        {
                            double distX = x - featurePointsX[checkX, checkY];
                            double distY = y - featurePointsY[checkX, checkY];
                            var dist = Math.Sqrt(distX * distX + distY * distY);

                            if (dist < f1)
                            {
                                f2 = f1;
                                f1 = dist;
                            }
                            else if (dist < f2)
                            {
                                f2 = dist;
                            }
                        }
                    }

                    // F2 - F1 ridge math
                    var rawValue = f2 - f1;

                    // Normalize and invert so ridges are 1.0 (bright) and deep cells are 0.0
                    var factor = 1.0 - Math.Clamp(rawValue / normalizationScale, 0.0, 1.0);

                    // FIX: Corrected inverted color interpolation layout to match expected parameters.
                    // factor = 0.0 (Deep cell center) -> draws centerRgb
                    // factor = 1.0 (Ridge crystal border) -> draws edgeRgb
                    span[idx++] = (byte)(centerRgb[2] + (edgeRgb[2] - centerRgb[2]) * factor); // B
                    span[idx++] = (byte)(centerRgb[1] + (edgeRgb[1] - centerRgb[1]) * factor); // G
                    span[idx++] = (byte)(centerRgb[0] + (edgeRgb[0] - centerRgb[0]) * factor); // R
                    span[idx++] = (byte)alpha; // A
                }
            }

            return buffer;
        }

        /// <summary>
        /// Generates the warped mapped.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGenInstance">The noise gen instance.</param>
        /// <param name="colorRampRgb">The color ramp RGB.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <param name="warpScale">The warp scale.</param>
        /// <param name="warpStrength">The warp strength.</param>
        /// <param name="alpha">The alpha.</param>
        /// <returns>The generated raw texture buffer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static RawTextureBuffer GenerateWarpedMapped(int width,
            int height,
            object noiseGenInstance,
            byte[] colorRampRgb,
            double turbulenceSize,
            double warpScale,
            double warpStrength,
            int alpha = 255)
        {
            var buffer = new RawTextureBuffer(width, height);
            var span = buffer.AsSpan();
            dynamic noiseGen = noiseGenInstance;

            var numColors = colorRampRgb.Length / 3;
            var idx = 0;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    // 1. Generate Domain Warp offsets (offset the X/Y slightly so they warp differently)
                    // We cast back to int to ensure compatibility with your dynamic noise Gen methods
                    var warpX = (int)(((double)noiseGen.Turbulence(x + 53, y + 17, warpScale) / 255.0) * warpStrength);
                    var warpY = (int)(((double)noiseGen.Turbulence(x - 28, y + 84, warpScale) / 255.0) * warpStrength);

                    // 2. Sample noise at warped coordinates
                    var noiseValue = (double)noiseGen.Turbulence(x + warpX, y + warpY, turbulenceSize) / 255.0;
                    noiseValue = Math.Clamp(noiseValue, 0.0, 0.999);

                    // 3. Map to Color Ramp
                    var rampPos = noiseValue * (numColors - 1);
                    var index1 = (int)Math.Floor(rampPos);
                    var index2 = Math.Min(index1 + 1, numColors - 1);
                    var blend = rampPos - index1;

                    int r1 = colorRampRgb[index1 * 3];
                    int g1 = colorRampRgb[index1 * 3 + 1];
                    int b1 = colorRampRgb[index1 * 3 + 2];

                    int r2 = colorRampRgb[index2 * 3];
                    int g2 = colorRampRgb[index2 * 3 + 1];
                    int b2 = colorRampRgb[index2 * 3 + 2];

                    span[idx++] = (byte)(b1 + (b2 - b1) * blend);
                    span[idx++] = (byte)(g1 + (g2 - g1) * blend);
                    span[idx++] = (byte)(r1 + (r2 - r1) * blend);
                    span[idx++] = (byte)alpha;
                }
            }

            return buffer;
        }

        /// <summary>
        /// Generates the ridged mapped.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="noiseGenInstance">The noise gen instance.</param>
        /// <param name="colorRampRgb">The color ramp RGB.</param>
        /// <param name="turbulenceSize">Size of the turbulence.</param>
        /// <param name="octaves">The octaves.</param>
        /// <param name="persistence">The persistence.</param>
        /// <param name="alpha">The alpha.</param>
        /// <returns>The generated raw texture buffer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static RawTextureBuffer GenerateRidgedMapped(int width,
            int height,
            object noiseGenInstance,
            byte[] colorRampRgb,
            double turbulenceSize,
            int octaves,
            double persistence,
            int alpha = 255)
        {
            var buffer = new RawTextureBuffer(width, height);
            var span = buffer.AsSpan();
            dynamic noiseGen = noiseGenInstance;

            var numColors = colorRampRgb.Length / 3;
            var idx = 0;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var value = 0.0;
                    var amplitude = 1.0;
                    var weight = 1.0;
                    var maxPossibleValue = 0.0;

                    // Establish frequency coordinates scaled down to generate macro layout structures
                    var freqX = x / turbulenceSize;
                    var freqY = y / turbulenceSize;

                    // Ridged Multifractal Octave Loop
                    for (var i = 0; i < octaves; i++)
                    {
                        // FIX 1: Explicitly cast parameters to (int) to match NoiseGenerator.GetNoise(int, int) and fix the test crash.
                        // FIX 2: Removed the redundant '* turbulenceSize' inside the call. Keeping coordinates scaled down 
                        // preserves macro structural contours, preventing the layers from flattening out into a uniform black sheet.
                        var n = ((double)noiseGen.GetNoise((int)freqX, (int)freqY) / 127.5) - 1.0;

                        // The Ridged Math
                        n = 1.0 - Math.Abs(n);
                        n *= n;
                        n *= weight;

                        // Keep ridges sharp
                        weight = Math.Clamp(n * 2.0, 0.0, 1.0);

                        value += n * amplitude;
                        maxPossibleValue += amplitude;

                        freqX *= 2.0;
                        freqY *= 2.0;
                        amplitude *= persistence;
                    }

                    var normalizedValue = Math.Clamp(value / maxPossibleValue, 0.0, 0.999);

                    // Map to Color Ramp
                    var rampPos = normalizedValue * (numColors - 1);
                    var index1 = (int)Math.Floor(rampPos);
                    var index2 = Math.Min(index1 + 1, numColors - 1);
                    var blend = rampPos - index1;

                    int r1 = colorRampRgb[index1 * 3];
                    int g1 = colorRampRgb[index1 * 3 + 1];
                    int b1 = colorRampRgb[index1 * 3 + 2];

                    int r2 = colorRampRgb[index2 * 3];
                    int g2 = colorRampRgb[index2 * 3 + 1];
                    int b2 = colorRampRgb[index2 * 3 + 2];

                    span[idx++] = (byte)(b1 + (b2 - b1) * blend); // B
                    span[idx++] = (byte)(g1 + (g2 - g1) * blend); // G
                    span[idx++] = (byte)(r1 + (r2 - r1) * blend); // R
                    span[idx++] = (byte)alpha; // A
                }
            }

            return buffer;
        }
    }
}
