using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Imaging
{
    public static class Textures
    {
        private const int NoiseWidth = 320; // Width for noise generation
        private const int NoiseHeight = 240; // Height for noise generation
        private static readonly double[,] Noise = new double[NoiseHeight, NoiseWidth];

        public static Bitmap GenerateNoiseBitmap(
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


        public static Bitmap GenerateCloudsBitmap(
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
                    var L = Math.Clamp(minValue + (int)(value / 4.0), minValue, maxValue);
                    var color = Color.FromArgb(alpha, L, L, L);
                    pixelData.Add((x, y, color));
                }
            }

            // Convert list to array for SIMD processing
            var pixelArray = pixelData.ToArray();
            cloudsBitmap.SetPixelsSimd(pixelArray);

            return cloudsBitmap.Bitmap;
        }


        public static Bitmap GenerateMarbleBitmap(
            int width,
            int height,
            double xPeriod = 5.0,
            double yPeriod = 10.0,
            double turbPower = 5.0,
            double turbSize = 32.0,
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
                                  (turbPower * Turbulence(x, y, turbSize) / 256.0);
                    var sineValue = 226 * Math.Abs(Math.Sin(xyValue * Math.PI));
                    var r = Math.Clamp(baseColor.R + (int)sineValue, 0, 255);
                    var g = Math.Clamp(baseColor.G + (int)sineValue, 0, 255);
                    var b = Math.Clamp(baseColor.B + (int)sineValue, 0, 255);

                    var color = Color.FromArgb(255, r, g, b);
                    pixelData.Add((x, y, color));
                }
            }

            // Convert list to array for SIMD processing
            var pixelArray = pixelData.ToArray();
            marbleBitmap.SetPixelsSimd(pixelArray);

            return marbleBitmap.Bitmap;
        }


        public static Bitmap GenerateWoodBitmap(
            int width,
            int height,
            double xyPeriod = 12.0,
            double turbPower = 0.1,
            double turbSize = 32.0,
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
                                    (turbPower * Turbulence(x, y, turbSize) / 256.0);
                    var sineValue = 128.0 * Math.Abs(Math.Sin(2 * xyPeriod * distValue * Math.PI));

                    var r = Math.Clamp(baseColor.R + (int)sineValue, 0, 255);
                    var g = Math.Clamp(baseColor.G + (int)sineValue, 0, 255);
                    var b = Math.Clamp((int)baseColor.B, 0, 255);

                    var color = Color.FromArgb(255, r, g, b);
                    pixelData.Add((x, y, color));
                }
            }

            // Convert list to array for SIMD processing
            var pixelArray = pixelData.ToArray();
            woodBitmap.SetPixelsSimd(pixelArray);

            return woodBitmap.Bitmap;
        }

        public static Bitmap GenerateWaveBitmap(
            int width,
            int height,
            double xyPeriod = 12.0,
            double turbPower = 0.1,
            double turbSize = 32.0)
        {
            GenerateBaseNoise();

            var waveBitmap = new DirectBitmap(width, height);
            var pixelData = new List<(int x, int y, Color color)>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var xValue = ((x - (width / 2.0)) / width) + (turbPower * Turbulence(x, y, turbSize) / 256.0);
                    var yValue = ((y - (height / 2.0)) / height) +
                                 (turbPower * Turbulence(height - y, width - x, turbSize) / 256.0);

                    var sineValue = 22.0 *
                                    Math.Abs(Math.Sin(xyPeriod * xValue * Math.PI) +
                                             Math.Sin(xyPeriod * yValue * Math.PI));
                    var hsvColor = new ColorHsv(sineValue, 1.0, 1.0, 255);

                    pixelData.Add((x, y, hsvColor.GetDrawingColor()));
                }
            }

            // Convert list to array for SIMD processing
            var pixelArray = pixelData.ToArray();
            waveBitmap.SetPixelsSimd(pixelArray);

            return waveBitmap.Bitmap;
        }


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

        private static void GenerateNoise(DirectBitmap bitmap, int width, int height, int minValue, int maxValue,
            int alpha, bool useSmoothNoise, bool useTurbulence, double turbulenceSize)
        {
            var vectorCount = Vector<int>.Count;
            var pixelData = new List<(int x, int y, Color color)>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x += vectorCount)
                {
                    var indices = new int[vectorCount];
                    var colors = new int[vectorCount];

                    // Load data into vectors
                    for (var j = 0; j < vectorCount; j++)
                    {
                        if (x + j < width)
                        {
                            double value;

                            if (useTurbulence)
                            {
                                value = Turbulence(x + j, y, turbulenceSize);
                            }
                            else if (useSmoothNoise)
                            {
                                value = SmoothNoise(x + j, y);
                            }
                            else
                            {
                                value = Noise[y % NoiseHeight, (x + j) % NoiseWidth];
                            }

                            var colorValue = minValue + (int)((maxValue - minValue) * value);
                            colorValue = Math.Max(minValue, Math.Min(maxValue, colorValue));
                            var color = Color.FromArgb(alpha, colorValue, colorValue, colorValue);

                            indices[j] = x + j + (y * width);
                            colors[j] = color.ToArgb();
                        }
                        else
                        {
                            indices[j] = 0;
                            colors[j] = Color.Transparent.ToArgb();
                        }
                    }

                    // Write data to Bits array
                    for (var j = 0; j < vectorCount; j++)
                    {
                        if (x + j < width)
                        {
                            bitmap.SetPixel(x + j, y, Color.FromArgb(colors[j]));
                        }
                    }
                }
            }
        }

        private static void GenerateClouds(DirectBitmap bitmap, int width, int height, int minValue, int maxValue,
            int alpha, double turbulenceSize)
        {
            var vectorCount = Vector<int>.Count;
            var pixelData = new List<(int x, int y, Color color)>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x += vectorCount)
                {
                    var indices = new int[vectorCount];
                    var colors = new int[vectorCount];

                    // Load data into vectors
                    for (var j = 0; j < vectorCount; j++)
                    {
                        if (x + j < width)
                        {
                            var value = Turbulence(x + j, y, turbulenceSize);
                            var L = minValue + (int)(value / 4.0);
                            L = Math.Max(minValue, Math.Min(maxValue, L)); // Clamp L value

                            var color = Color.FromArgb(alpha, L, L, L);

                            indices[j] = x + j + (y * width);
                            colors[j] = color.ToArgb();
                        }
                        else
                        {
                            indices[j] = 0;
                            colors[j] = Color.Transparent.ToArgb();
                        }
                    }

                    // Write data to Bits array
                    for (var j = 0; j < vectorCount; j++)
                    {
                        if (x + j < width)
                        {
                            bitmap.SetPixel(x + j, y, Color.FromArgb(colors[j]));
                        }
                    }
                }
            }
        }

        private static void GenerateMarble(DirectBitmap bitmap, int width, int height, double xPeriod, double yPeriod,
            double turbPower, double turbSize, Color baseColor)
        {
            var vectorCount = Vector<int>.Count;
            var pixelData = new List<(int x, int y, Color color)>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x += vectorCount)
                {
                    var indices = new int[vectorCount];
                    var colors = new int[vectorCount];

                    // Load data into vectors
                    for (var j = 0; j < vectorCount; j++)
                    {
                        if (x + j < width)
                        {
                            var xyValue = ((x + j) * xPeriod / NoiseWidth) + (y * yPeriod / NoiseHeight) +
                                          (turbPower * Turbulence(x + j, y, turbSize) / 256.0);
                            var sineValue = 226 * Math.Abs(Math.Sin(xyValue * Math.PI));

                            var r = baseColor.R + (int)sineValue;
                            var g = baseColor.G + (int)sineValue;
                            var b = baseColor.B + (int)sineValue;

                            r = Math.Max(0, Math.Min(255, r));
                            g = Math.Max(0, Math.Min(255, g));
                            b = Math.Max(0, Math.Min(255, b));

                            var color = Color.FromArgb(255, r, g, b);

                            indices[j] = x + j + (y * width);
                            colors[j] = color.ToArgb();
                        }
                        else
                        {
                            indices[j] = 0;
                            colors[j] = Color.Transparent.ToArgb();
                        }
                    }

                    // Write data to Bits array
                    for (var j = 0; j < vectorCount; j++)
                    {
                        if (x + j < width)
                        {
                            bitmap.SetPixel(x + j, y, Color.FromArgb(colors[j]));
                        }
                    }
                }
            }
        }

        private static void GenerateWood(DirectBitmap bitmap, int width, int height, double xyPeriod, double turbPower,
            double turbSize, Color baseColor)
        {
            var vectorCount = Vector<int>.Count;
            var pixelData = new List<(int x, int y, Color color)>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x += vectorCount)
                {
                    var indices = new int[vectorCount];
                    var colors = new int[vectorCount];

                    // Load data into vectors
                    for (var j = 0; j < vectorCount; j++)
                    {
                        if (x + j < width)
                        {
                            var xValue = (x + j - (width / 2.0)) / width;
                            var yValue = (y - (height / 2.0)) / height;
                            var distValue = Math.Sqrt((xValue * xValue) + (yValue * yValue)) +
                                            (turbPower * Turbulence(x + j, y, turbSize) / 256.0);
                            var sineValue = 128.0 * Math.Abs(Math.Sin(2 * xyPeriod * distValue * Math.PI));

                            var r = baseColor.R + (int)sineValue;
                            var g = baseColor.G + (int)sineValue;
                            var b = (int)baseColor.B;

                            r = Math.Max(0, Math.Min(255, r));
                            g = Math.Max(0, Math.Min(255, g));
                            b = Math.Max(0, Math.Min(255, b));

                            var color = Color.FromArgb(255, r, g, b);

                            indices[j] = x + j + (y * width);
                            colors[j] = color.ToArgb();
                        }
                        else
                        {
                            indices[j] = 0;
                            colors[j] = Color.Transparent.ToArgb();
                        }
                    }

                    // Write data to Bits array
                    for (var j = 0; j < vectorCount; j++)
                    {
                        if (x + j < width)
                        {
                            bitmap.SetPixel(x + j, y, Color.FromArgb(colors[j]));
                        }
                    }
                }
            }
        }

        private static void GenerateWave(DirectBitmap bitmap, int width, int height, double xyPeriod, double turbPower,
            double turbSize)
        {
            var vectorCount = Vector<int>.Count;
            var pixelData = new List<(int x, int y, Color color)>();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x += vectorCount)
                {
                    var indices = new int[vectorCount];
                    var colors = new int[vectorCount];

                    // Load data into vectors
                    for (var j = 0; j < vectorCount; j++)
                    {
                        if (x + j < width)
                        {
                            var xValue = ((x + j - (width / 2.0)) / width) +
                                         (turbPower * Turbulence(x + j, y, turbSize) / 256.0);
                            var yValue = ((y - (height / 2.0)) / height) +
                                         (turbPower * Turbulence(height - y, width - (x + j), turbSize) / 256.0);

                            var sineValue = 22.0 * Math.Abs(Math.Sin(xyPeriod * xValue * Math.PI) +
                                                            Math.Sin(xyPeriod * yValue * Math.PI));

                            var hsvColor = new ColorHsv(sineValue, 1.0, 1.0, 255);

                            indices[j] = x + j + (y * width);
                            colors[j] = hsvColor.GetDrawingColor().ToArgb();
                        }
                        else
                        {
                            indices[j] = 0;
                            colors[j] = Color.Transparent.ToArgb();
                        }
                    }

                    // Write data to Bits array
                    for (var j = 0; j < vectorCount; j++)
                    {
                        if (x + j < width)
                        {
                            bitmap.SetPixel(x + j, y, Color.FromArgb(colors[j]));
                        }
                    }
                }
            }
        }

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

        private static double Interpolate(double a, double b, double t)
        {
            return (a * (1 - t)) + (b * t);
        }
    }
}
