/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/DirectBitmapImageTests.cs
 * PURPOSE:     Custom Image Class´, some tests for validations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using Imaging;
using Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     DirectBitmapImage test cases
    /// </summary>
    [TestClass]
    public class DirectBitmapImageTests
    {
        private const int Width = 10;
        private const int Height = 10;
        private DirectBitmapImage _bitmapImage;

        /// <summary>
        ///     Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _bitmapImage = new DirectBitmapImage(Width, Height);
        }

        /// <summary>
        ///     Cleanups this instance.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            _bitmapImage.Dispose();
        }

        /// <summary>
        ///     Sets the pixels valid pixels updates bitmap.
        /// </summary>
        [TestMethod]
        public void SetPixelsValidPixelsUpdatesBitmap()
        {
            var pixels = new List<PixelData>
            {
                new() { X = 0, Y = 0, R = 255, G = 0, B = 0, A = 255 }, // Red
                new() { X = 1, Y = 1, R = 0, G = 255, B = 0, A = 255 }, // Green
                new() { X = 2, Y = 2, R = 0, G = 0, B = 255, A = 255 }  // Blue
            };

            _bitmapImage.SetPixels(pixels);

            // Check Red at (0,0)
            var p0 = _bitmapImage.Bits[0];
            Assert.AreEqual(255, p0.A);
            Assert.AreEqual(255, p0.R);
            Assert.AreEqual(0, p0.G);
            Assert.AreEqual(0, p0.B);

            // Check Green at (1,1)
            var p1 = _bitmapImage.Bits[_bitmapImage.Width + 1];
            Assert.AreEqual(255, p1.A);
            Assert.AreEqual(0, p1.R);
            Assert.AreEqual(255, p1.G);
            Assert.AreEqual(0, p1.B);

            // Check Blue at (2,2)
            var p2 = _bitmapImage.Bits[(_bitmapImage.Width * 2) + 2];
            Assert.AreEqual(255, p2.A);
            Assert.AreEqual(0, p2.R);
            Assert.AreEqual(0, p2.G);
            Assert.AreEqual(255, p2.B);
        }


        /// <summary>
        ///     Constructors the initializes bits array.
        /// </summary>
        [TestMethod]
        public void ConstructorInitializesBitsArray()
        {
            // Assert that the Bits array is initialized correctly
            Assert.IsNotNull(_bitmapImage.Bits);
            Assert.AreEqual(Width * Height, _bitmapImage.Bits.Length);
        }

        /// <summary>
        ///     Sets the pixels simd two inputs should set correct pixels.
        /// </summary>
        [TestMethod]
        public void SetPixelsSimdTwoInputsShouldSetCorrectPixels()
        {
            // Arrange
            var pixels = new List<(int x, int y, Color color)>
            {
                (0, 0, new Color { A = 255, R = 255, G = 0, B = 0 }), // Red pixel
                (1, 0, new Color { A = 255, R = 0, G = 255, B = 0 })  // Green pixel
            };

            // Act
            _bitmapImage.SetPixelsBulk(pixels);

            // Assert using Pixel32 channels
            var p0 = _bitmapImage.Bits[0];
            Assert.AreEqual(255, p0.A);
            Assert.AreEqual(255, p0.R);
            Assert.AreEqual(0, p0.G);
            Assert.AreEqual(0, p0.B);

            var p1 = _bitmapImage.Bits[1];
            Assert.AreEqual(255, p1.A);
            Assert.AreEqual(0, p1.R);
            Assert.AreEqual(255, p1.G);
            Assert.AreEqual(0, p1.B);
        }

        /// <summary>
        ///     Applies the color matrix valid matrix transforms colors.
        /// </summary>
        [TestMethod]
        public void ApplyColorMatrixValidMatrixTransformsColors()
        {
            _bitmapImage = new DirectBitmapImage(2, 1);

            // Set initial pixel colors
            var initialPixels = new List<PixelData>
            {
                new() { X = 0, Y = 0, R = 255, G = 0, B = 0, A = 255 }, // Red
                new() { X = 1, Y = 0, R = 0, G = 255, B = 0, A = 255 }  // Green
            };
            _bitmapImage.SetPixels(initialPixels);

            // Proper grayscale matrix (same row for R=G=B)
            var matrix = new[]
            {
                new[] { 0.3f, 0.59f, 0.11f, 0f }, // R
                new[] { 0.3f, 0.59f, 0.11f, 0f }, // G
                new[] { 0.3f, 0.59f, 0.11f, 0f }, // B
                new[] { 0f,   0f,   0f,   1f }    // A (alpha preserved)
            };


            _bitmapImage.ApplyColorMatrix(matrix);

            var pixel0 = _bitmapImage.Bits[0]; // Pixel32
            var pixel1 = _bitmapImage.Bits[1];

            // Check alpha unchanged
            Assert.AreEqual(255, pixel0.A);
            Assert.AreEqual(255, pixel1.A);

            // Check grayscale: R=G=B
            Assert.AreEqual(pixel0.R, pixel0.G);
            Assert.AreEqual(pixel0.R, pixel0.B);

            Assert.AreEqual(pixel1.R, pixel1.G);
            Assert.AreEqual(pixel1.R, pixel1.B);

            // Optional: assert exact grayscale value
            var expected0 = (byte)Math.Round(0.3 * 255 + 0.59 * 0 + 0.11 * 0);   // Red pixel → 76
            var expected1 = (byte)Math.Round(0.3 * 0 + 0.59 * 255 + 0.11 * 0);   // Green pixel → 150

            Assert.AreEqual(expected0, pixel0.R);
            Assert.AreEqual(expected1, pixel1.R);
        }

        /// <summary>
        ///     Applies the color of the color matrix valid matrix transforms.
        /// </summary>
        [TestMethod]
        public void ApplyColorMatrixValidMatrixTransformsColor()
        {
            _bitmapImage = new DirectBitmapImage(1, 1);

            var originalPixel = new PixelData
            {
                X = 0,
                Y = 0,
                R = 0,
                G = 255,
                B = 0,
                A = 255
            };
            _bitmapImage.SetPixels(new[] { originalPixel });

            var matrix = new[]
            {
                new[] { 0.3f, 0.59f, 0.11f, 0, 0 },
                new[] { 0.3f, 0.59f, 0.11f, 0, 0 },
                new[] { 0.3f, 0.59f, 0.11f, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { 0, 0, 0, 0, 0 }
            };

            _bitmapImage.ApplyColorMatrix(matrix);

            var pixel = _bitmapImage.Bits[0];

            // Check alpha unchanged
            Assert.AreEqual(originalPixel.A, pixel.A);

            // Grayscale: R=G=B
            Assert.AreEqual(pixel.R, pixel.G);
            Assert.AreEqual(pixel.R, pixel.B);

            // Expected grayscale
            var expectedGray = (byte)Math.Round(
                0.3 * originalPixel.R +
                0.59 * originalPixel.G +
                0.11 * originalPixel.B
            );

            Assert.AreEqual(expectedGray, pixel.R);
        }

        /// <summary>
        ///     Matrix multiplications.
        /// </summary>
        [TestMethod]
        public void MatrixMultiplicationColor()
        {
            _bitmapImage = new DirectBitmapImage(1, 1);
            // Define the expected packed color
            const uint expectedPackedColor = unchecked((uint)((255 << 24) | (76 << 16) | (150 << 8) | 28));

            // Define the color transformation matrix for grayscale
            double[,] colorMatrix =
            {
                { 0.3, 0.3, 0.3, 0, 0 }, { 0.59, 0.59, 0.59, 0, 0 }, { 0.11, 0.11, 0.11, 0, 0 }, { 0, 0, 0, 1, 0 },
                { 0, 0, 0, 0, 0 }
            };

            // Define the initial pixel color (Green) as a column vector (5x1)
            double[,] initialPixel =
            {
                { 0 }, // R
                { 255 }, // G
                { 0 }, // B
                { 255 }, // A
                { 1 } // Additional value if needed
            };

            // Perform matrix multiplication
            var result = MatrixUtility.UnsafeMultiplication(colorMatrix, initialPixel);

            // Extract the results
            var alpha = (int)result[3, 0];
            var red = (int)Math.Round(result[0, 0]);
            var green = (int)Math.Round(result[1, 0]);
            var blue = (int)Math.Round(result[2, 0]);

            // Assert the results
            Assert.AreEqual(255, alpha, "Alpha value should be 255.");
            Assert.AreEqual(76, red, "Red value should be 76.");
            Assert.AreEqual(150, green, "Green value should be 150.");
            Assert.AreEqual(28, blue, "Blue value should be 28.");

            // Convert individual components to packed uint
            var packedColor = unchecked((uint)((alpha << 24) | (red << 16) | (green << 8) | blue));

            Assert.AreEqual(packedColor, expectedPackedColor);
        }

        /// <summary>
        ///     Applies the color matrix grayscale all pixels should be gray.
        /// </summary>
        [TestMethod]
        public void ApplyColorMatrixGrayscaleShouldMakeRGBEqual()
        {
            var bmp = new DirectBitmapImage(2, 2);
            bmp.SetPixels(new[]
            {
                new PixelData(0, 0, 255, 0, 0),   // red
                new PixelData(1, 0, 0, 255, 0),   // green
                new PixelData(0, 1, 0, 0, 255),   // blue
                new PixelData(1, 1, 255, 255, 0)  // yellow
            });

            float[][] grayscaleMatrix =
            {
                new[] { 0.299f, 0.587f, 0.114f, 0 },
                new[] { 0.299f, 0.587f, 0.114f, 0 },
                new[] { 0.299f, 0.587f, 0.114f, 0 },
                new float[] { 0, 0, 0, 1 }
            };

            bmp.ApplyColorMatrix(grayscaleMatrix);

            foreach (var pixel in bmp.Bits)
            {
                var r = pixel.R;
                var g = pixel.G;
                var b = pixel.B;

                Assert.IsTrue(Math.Abs(r - g) <= 1 && Math.Abs(g - b) <= 1,
                    $"Pixel RGB mismatch: R={r}, G={g}, B={b}");
            }
        }

        /// <summary>
        ///     Sets the pixels simd valid pixels updates bits.
        /// </summary>
        [TestMethod]
        public void SetPixelsSimdValidPixelsUpdatesBits()
        {
            var pixels = new List<(int x, int y, Color color)>
            {
                (0, 0, Color.FromArgb(255, 255, 0, 0)), // Red
                (1, 1, Color.FromArgb(255, 0, 255, 0)), // Green
                (2, 2, Color.FromArgb(255, 0, 0, 255))  // Blue
            };

            _bitmapImage.SetPixelsBulk(pixels);

            // Check Red at (0,0)
            var p0 = _bitmapImage.Bits[0];
            Assert.AreEqual(255, p0.A);
            Assert.AreEqual(255, p0.R);
            Assert.AreEqual(0, p0.G);
            Assert.AreEqual(0, p0.B);

            // Check Green at (1,1)
            var p1 = _bitmapImage.Bits[_bitmapImage.Width + 1];
            Assert.AreEqual(255, p1.A);
            Assert.AreEqual(0, p1.R);
            Assert.AreEqual(255, p1.G);
            Assert.AreEqual(0, p1.B);

            // Check Blue at (2,2)
            var p2 = _bitmapImage.Bits[(_bitmapImage.Width * 2) + 2];
            Assert.AreEqual(255, p2.A);
            Assert.AreEqual(0, p2.R);
            Assert.AreEqual(0, p2.G);
            Assert.AreEqual(255, p2.B);
        }

        /// <summary>
        ///     Sets the pixels should set correct pixel values.
        /// </summary>
        [TestMethod]
        public void SetPixelsShouldSetCorrectPixelValues()
        {
            // Arrange
            var bitmapImage = new DirectBitmapImage(2, 2);
            var pixels = new List<PixelData>
            {
                new() { X = 0, Y = 0, R = 255, G = 0,   B = 0,   A = 255 }, // Red
                new() { X = 1, Y = 0, R = 0,   G = 255, B = 0,   A = 255 }, // Green
                new() { X = 0, Y = 1, R = 0,   G = 0,   B = 255, A = 255 }, // Blue
                new() { X = 1, Y = 1, R = 255, G = 255, B = 0,   A = 255 }  // Yellow
            };

            // Act
            bitmapImage.SetPixels(pixels);

            // Assert
            var p0 = bitmapImage.Bits[0]; // Red
            Assert.AreEqual(255, p0.A);
            Assert.AreEqual(255, p0.R);
            Assert.AreEqual(0, p0.G);
            Assert.AreEqual(0, p0.B);

            var p1 = bitmapImage.Bits[1]; // Green
            Assert.AreEqual(255, p1.A);
            Assert.AreEqual(0, p1.R);
            Assert.AreEqual(255, p1.G);
            Assert.AreEqual(0, p1.B);

            var p2 = bitmapImage.Bits[2]; // Blue
            Assert.AreEqual(255, p2.A);
            Assert.AreEqual(0, p2.R);
            Assert.AreEqual(0, p2.G);
            Assert.AreEqual(255, p2.B);

            var p3 = bitmapImage.Bits[3]; // Yellow
            Assert.AreEqual(255, p3.A);
            Assert.AreEqual(255, p3.R);
            Assert.AreEqual(255, p3.G);
            Assert.AreEqual(0, p3.B);
        }


        /// <summary>
        ///     Constructors the with zero dimensions should throw.
        /// </summary>
        [TestMethod]
        public void ConstructorWithZeroDimensionsShouldThrow()
        {
            // Act & Assert
            _ = Assert.ThrowsException<ArgumentException>(() => new DirectBitmapImage(0, 0));
        }
    }
}
