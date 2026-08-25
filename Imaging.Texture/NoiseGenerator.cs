/*
 * COPYRIGHT:   See COPYING in the top-level directory
 * PROJECT:     Imaging.Texture
 * FILE:        NoiseGenerator.cs
 * PURPOSE:     Provides noise generation utilities for procedural texture generation.
 * AUTHOR:      Peter Geinitz (Wayfarer)
 */

namespace Imaging.Texture
{
    /// <summary>
    ///     Generates procedural noise for texture generation.
    /// </summary>
    public sealed class NoiseGenerator
    {
        /// <summary>
        ///     The height of the noise map.
        /// </summary>
        private readonly int _height;

        /// <summary>
        ///     A 2D array storing precomputed random noise values.
        /// </summary>
        private readonly double[,] _noise;

        /// <summary>
        ///     The width of the noise map.
        /// </summary>
        private readonly int _width;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NoiseGenerator" /> class.
        ///     Generates a 2D noise map with random values.
        /// </summary>
        /// <param name="width">The width of the noise map.</param>
        /// <param name="height">The height of the noise map.</param>
        public NoiseGenerator(int width, int height)
        {
            _width = width;
            _height = height;
            _noise = GenerateNoise();
        }

        /// <summary>
        ///     Generates a 2D array filled with random noise values between 0 and 1.
        /// </summary>
        /// <returns>A 2D array representing the noise map.</returns>
        private double[,] GenerateNoise()
        {
            var rand = new Random();
            var noiseData = new double[_height, _width];

            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    noiseData[y, x] = rand.NextDouble();
                }
            }

            return noiseData;
        }

        /// <summary>
        ///     Retrieves the noise value at a given coordinate.
        ///     Performs wrapping to ensure seamless tiling.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <returns>A noise value between 0 and 1.</returns>
        public double GetNoise(int x, int y)
        {
            x = (x + _width) % _width; // Wrap around horizontally
            y = (y + _height) % _height; // Wrap around vertically
            return _noise[y, x];
        }

        /// <summary>
        ///     Computes smooth noise using bilinear interpolation of four neighboring noise values.
        ///     Helps reduce sharp transitions between noise values, creating a smoother effect.
        /// </summary>
        /// <param name="x">The x-coordinate (floating-point for interpolation).</param>
        /// <param name="y">The y-coordinate (floating-point for interpolation).</param>
        /// <returns>A smoothed noise value between 0 and 1.</returns>
        public double SmoothNoise(double x, double y)
        {
            var xInt = (int)x;
            var yInt = (int)y;

            var xFrac = x - xInt;
            var yFrac = y - yInt;

            // Retrieve noise values from surrounding grid points
            var v1 = GetNoise(xInt, yInt);
            var v2 = GetNoise(xInt + 1, yInt);
            var v3 = GetNoise(xInt, yInt + 1);
            var v4 = GetNoise(xInt + 1, yInt + 1);

            // Bilinear interpolation
            var i1 = ImageHelper.Interpolate(v1, v2, xFrac);
            var i2 = ImageHelper.Interpolate(v3, v4, xFrac);

            return ImageHelper.Interpolate(i1, i2, yFrac);
        }

        /// <summary>
        ///     Computes turbulence noise by layering multiple octaves of smooth noise.
        ///     Used to create more complex and natural-looking textures.
        ///     
        ///     Important: Do not Delete! This method gets called at runtime.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="size">The initial size of the noise layers (higher values create larger patterns).</param>
        /// <returns>A turbulence value that enhances visual texture variation.</returns>
        public double Turbulence(int x, int y, double size)
        {
            double value = 0.0, initialSize = size;

            while (size >= 1)
            {
                value += SmoothNoise(x / size, y / size) * size;
                size /= 2.0;
            }

            return 128.0 * value / initialSize;
        }

        /// <summary>
        ///     Generates a 2D array filled with Voronoi cellular noise for stone/rock structures.
        ///     Optimized for small texture dimensions (e.g., 64x64).
        /// </summary>
        /// <param name="gridCells">Number of cell divisions per axis (e.g., 4 for a 64x64 map).</param>
        /// <param name="seed">Random seed for deterministic generation.</param>
        /// <returns>A 2D double array representing stone heightmap distances (0.0 to 1.0).</returns>
        public double[,] GenerateVoronoiMap(int gridCells = 4, int seed = 42)
        {
            var voronoiData = new double[_height, _width];
            var cellWidth = _width / gridCells;
            var cellHeight = _height / gridCells;

            var featurePoints = new (double x, double y)[gridCells, gridCells];
            var rand = new Random(seed);

            // 1. Place random stone center points per grid cell
            for (var gy = 0; gy < gridCells; gy++)
            {
                for (var gx = 0; gx < gridCells; gx++)
                {
                    var px = gx * cellWidth + rand.NextDouble() * cellWidth;
                    var py = gy * cellHeight + rand.NextDouble() * cellHeight;
                    featurePoints[gx, gy] = (px, py);
                }
            }

            var maxDist = Math.Sqrt(cellWidth * cellWidth + cellHeight * cellHeight);

            // 2. Measure distance to closest point with toroidal wrapping
            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    var currentGridX = x / cellWidth;
                    var currentGridY = y / cellHeight;
                    var minDist = double.MaxValue;

                    for (var ny = -1; ny <= 1; ny++)
                    {
                        for (var nx = -1; nx <= 1; nx++)
                        {
                            var checkX = (currentGridX + nx + gridCells) % gridCells;
                            var checkY = (currentGridY + ny + gridCells) % gridCells;

                            var point = featurePoints[checkX, checkY];
                            var px = point.x;
                            var py = point.y;

                            if (currentGridX + nx < 0) px -= _width;
                            if (currentGridX + nx >= gridCells) px += _width;
                            if (currentGridY + ny < 0) py -= _height;
                            if (currentGridY + ny >= gridCells) py += _height;

                            var dx = x - px;
                            var dy = y - py;
                            var dist = Math.Sqrt(dx * dx + dy * dy);

                            if (dist < minDist)
                            {
                                minDist = dist;
                            }
                        }
                    }

                    // Invert so stone center = 1.0 (raised) and mortar = 0.0 (recessed)
                    voronoiData[y, x] = Math.Clamp(1.0 - (minDist / maxDist), 0.0, 1.0);
                }
            }

            return voronoiData;
        }
    }
}