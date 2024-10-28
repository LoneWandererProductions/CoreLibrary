using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using Imaging;
using Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
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
        ///     Sets the pixels valid pixels updates bitmap.
        /// </summary>
        [TestMethod]
        public void SetPixelsValidPixelsUpdatesBitmap()
        {
            var pixels = new List<PixelData>
            {
                new()
                {
                    X = 0,
                    Y = 0,
                    R = 255,
                    G = 0,
                    B = 0,
                    A = 255
                }, // Red
                new()
                {
                    X = 1,
                    Y = 1,
                    R = 0,
                    G = 255,
                    B = 0,
                    A = 255
                }, // Green
                new()
                {
                    X = 2,
                    Y = 2,
                    R = 0,
                    G = 0,
                    B = 255,
                    A = 255
                } // Blue
            };

            _bitmapImage.SetPixels(pixels);

            // Check that the colors were set correctly in the Bits array
            Assert.AreEqual(unchecked((uint)((255 << 24) | (255 << 16) | (0 << 8) | 0)), _bitmapImage.Bits[0]); // Red
            Assert.AreEqual(unchecked((uint)((255 << 24) | (0 << 16) | (255 << 8) | 0)),
                _bitmapImage.Bits[Width + 1]); // Green
            Assert.AreEqual(unchecked((uint)((255 << 24) | (0 << 16) | (0 << 8) | 255)),
                _bitmapImage.Bits[(Width * 2) + 2]); // Blue
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
                (1, 0, new Color { A = 255, R = 0, G = 255, B = 0 }) // Green pixel
            };

            // Act
            _bitmapImage.SetPixelsSimd(pixels);

            // Assert
            Assert.AreEqual(unchecked((uint)((255 << 24) | (255 << 16) | (0 << 8) | 0)),
                _bitmapImage.Bits[0]); // Red pixel ARGB
            Assert.AreEqual(unchecked((uint)((255 << 24) | (0 << 16) | (255 << 8) | 0)),
                _bitmapImage.Bits[1]); // Green pixel ARGB
        }

        /// <summary>
        ///     Applies the color matrix valid matrix transforms colors.
        /// </summary>
        [TestMethod]
        public void ApplyColorMatrixValidMatrixTransformsColors()
        {
            _bitmapImage = new DirectBitmapImage(1, 2);
            // Set initial pixel colors
            var initialPixels = new List<PixelData>
            {
                new()
                {
                    X = 0,
                    Y = 0,
                    R = 255,
                    G = 0,
                    B = 0,
                    A = 255
                }, // Red
                new()
                {
                    X = 1,
                    Y = 0,
                    R = 0,
                    G = 255,
                    B = 0,
                    A = 255
                } // Green
            };
            _bitmapImage.SetPixels(initialPixels);
            // Define a color matrix to convert colors to grayscale
            var matrix = new[]
            {
                new[] { 0.3f, 0.3f, 0.3f, 0, 0 }, new[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                new[] { 0.11f, 0.11f, 0.11f, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 0, 0, 0, 0 }
            };
            _bitmapImage.ApplyColorMatrix(matrix);

            var colorValue = _bitmapImage.Bits[0];

            var a = (byte)((colorValue >> 24) & 0xFF); // Alpha
            var r = (byte)((colorValue >> 16) & 0xFF); // Red
            var g = (byte)((colorValue >> 8) & 0xFF); // Green
            var b = (byte)(colorValue & 0xFF); // Blue


            Trace.WriteLine($"A: {a}, R: {r}, G: {g}, B: {b}");

            colorValue = _bitmapImage.Bits[1];

            a = (byte)((colorValue >> 24) & 0xFF); // Alpha
            r = (byte)((colorValue >> 16) & 0xFF); // Red
            g = (byte)((colorValue >> 8) & 0xFF); // Green
            b = (byte)(colorValue & 0xFF); // Blue


            Trace.WriteLine($"A: {a}, R: {r}, G: {g}, B: {b}");

            // Assert that colors were transformed correctly to grayscale
            Assert.AreEqual(4283209244, _bitmapImage.Bits[0]);
            Assert.AreEqual((UInt32)0, _bitmapImage.Bits[1]); // Grayscale for Green
        }

        /// <summary>
        ///     Matrix multiplications.
        /// </summary>
        [TestMethod]
        public void MatrixMultiplicationColor()
        {
            _bitmapImage = new DirectBitmapImage(1, 1);
            // Define the expected packed color
            var expectedPackedColor = unchecked((uint)((255 << 24) | (76 << 16) | (150 << 8) | 28));

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
        ///     Applies the color of the color matrix valid matrix transforms.
        /// </summary>
        [TestMethod]
        public void ApplyColorMatrixValidMatrixTransformsColor()
        {
            _bitmapImage = new DirectBitmapImage(1, 1);
            // Set initial pixel colors
            var initialPixels = new List<PixelData>
            {
                new()
                {
                    X = 0,
                    Y = 0,
                    R = 0,
                    G = 255,
                    B = 0,
                    A = 255
                } // Green
            };
            _bitmapImage.SetPixels(initialPixels);
            // Define a color matrix to convert colors to grayscale
            var matrix = new[]
            {
                new[] { 0.3f, 0.3f, 0.3f, 0, 0 }, new[] { 0.59f, 0.59f, 0.59f, 0, 0 },
                new[] { 0.11f, 0.11f, 0.11f, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 0, 0, 0, 0 }
            };
            _bitmapImage.ApplyColorMatrix(matrix);

            var colorValue = _bitmapImage.Bits[0];

            var a = (byte)((colorValue >> 24) & 0xFF); // Alpha
            var r = (byte)((colorValue >> 16) & 0xFF); // Red
            var g = (byte)((colorValue >> 8) & 0xFF); // Green
            var b = (byte)(colorValue & 0xFF); // Blue

            Trace.WriteLine($"A: {a}, R: {r}, G: {g}, B: {b}");

            // Assert that colors were transformed correctly to grayscale
            Assert.AreEqual(4283209244, _bitmapImage.Bits[0]);
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
                (2, 2, Color.FromArgb(255, 0, 0, 255)) // Blue
            };

            _bitmapImage.SetPixelsSimd(pixels);

            // Check that the colors were set correctly in the Bits array
            Assert.AreEqual(unchecked((uint)((255 << 24) | (255 << 16) | (0 << 8) | 0)), _bitmapImage.Bits[0]); // Red
            Assert.AreEqual(unchecked((uint)((255 << 24) | (0 << 16) | (255 << 8) | 0)),
                _bitmapImage.Bits[Width + 1]); // Green
            Assert.AreEqual(unchecked((uint)((255 << 24) | (0 << 16) | (0 << 8) | 255)),
                _bitmapImage.Bits[(Width * 2) + 2]); // Blue
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
                new()
                {
                    X = 0,
                    Y = 0,
                    R = 255,
                    G = 0,
                    B = 0,
                    A = 255
                }, // Red
                new()
                {
                    X = 1,
                    Y = 0,
                    R = 0,
                    G = 255,
                    B = 0,
                    A = 255
                }, // Green
                new()
                {
                    X = 0,
                    Y = 1,
                    R = 0,
                    G = 0,
                    B = 255,
                    A = 255
                }, // Blue
                new()
                {
                    X = 1,
                    Y = 1,
                    R = 255,
                    G = 255,
                    B = 0,
                    A = 255
                } // Yellow
            };

            // Act
            bitmapImage.SetPixels(pixels);

            // Assert using unsigned integers
            Assert.AreEqual(unchecked((uint)((255 << 24) | (255 << 16) | (0 << 8) | 0)),
                bitmapImage.Bits[0]); // Check Red (0xFFFF0000)
            Assert.AreEqual(unchecked((uint)((255 << 24) | (0 << 16) | (255 << 8) | 0)),
                bitmapImage.Bits[1]); // Check Green (0xFF00FF00)
            Assert.AreEqual(unchecked((uint)((255 << 24) | (0 << 16) | (0 << 8) | 255)),
                bitmapImage.Bits[2]); // Check Blue (0xFF0000FF)
            Assert.AreEqual(unchecked((uint)((255 << 24) | (255 << 16) | (255 << 8) | 0)),
                bitmapImage.Bits[3]); // Check Yellow (0xFFFFFF00)
        }

        /// <summary>
        ///     Constructors the with zero dimensions should throw.
        /// </summary>
        [TestMethod]
        public void ConstructorWithZeroDimensionsShouldThrow()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => new DirectBitmapImage(0, 0));
        }
    }
}
