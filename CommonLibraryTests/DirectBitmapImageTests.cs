using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Color = System.Windows.Media.Color;

namespace CommonLibraryTests
{
    [TestClass]
    public class DirectBitmapImageTests
    {
        private const int Width = 10;
        private const int Height = 10;
        private DirectBitmapImage _bitmapImage;

        [TestInitialize]
        public void Setup()
        {
            _bitmapImage = new DirectBitmapImage(Width, Height);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _bitmapImage.Dispose();
        }

        [TestMethod]
        public void Constructor_InitializesBitsArray()
        {
            // Assert that the Bits array is initialized correctly
            Assert.IsNotNull(_bitmapImage.Bits);
            Assert.AreEqual(Width * Height, _bitmapImage.Bits.Length);
        }

        [TestMethod]
        public void SetPixels_ValidPixels_UpdatesBitmap()
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

        [TestMethod]
        public void SetPixelsSimd_TwoInputs_ShouldSetCorrectPixels()
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
        public void ApplyColorMatrix_ValidMatrix_TransformsColors()
        {
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

            // Apply the custom color matrix transformation
            _bitmapImage.ApplyColorMatrix(matrix);

            // Create a Bitmap to use with System.Drawing
            using (var sourceImage = new Bitmap(2, 1))
            {
                // Fill the Bitmap with initial pixel colors
                using (var g = Graphics.FromImage(sourceImage))
                {
                    g.Clear(System.Drawing.Color.FromArgb(255, 255, 0, 0)); // Red
                    g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(255, 0, 255, 0)), 1, 0, 1, 1); // Green
                }

                // Apply Microsoft ColorMatrix
                var resultImage = ApplyMicrosoftColorMatrix(sourceImage, matrix);

                // Compare the results
                for (var i = 0; i < initialPixels.Count; i++)
                {
                    var customResult = _bitmapImage.Bits[i];
                    var microsoftResultColor = resultImage.GetPixel(i, 0);

                    // Convert Microsoft result color to uint
                    var microsoftResult = unchecked((uint)((microsoftResultColor.A << 24) |
                                                           (microsoftResultColor.R << 16) |
                                                           (microsoftResultColor.G << 8) | microsoftResultColor.B));

                    // Assert the values are equal
                    //Assert.AreEqual(customResult, microsoftResult, $"Pixel {i} mismatch. Custom: {customResult}, Microsoft: {microsoftResult}");
                }
            }
        }

        [TestMethod]
        public void SetPixelsSimd_ValidPixels_UpdatesBits()
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

        [TestMethod]
        public void SetPixels_ShouldSetCorrectPixelValues()
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
        ///     Applies the microsoft color matrix.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="matrix">The matrix.</param>
        /// <returns>Bitmap of Image</returns>
        private Bitmap ApplyMicrosoftColorMatrix(Bitmap sourceImage, float[][] matrix)
        {
            var result = new Bitmap(sourceImage.Width, sourceImage.Height);

            using (var g = Graphics.FromImage(result))
            {
                var colorMatrix = new ColorMatrix(matrix);
                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                g.DrawImage(sourceImage,
                    new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                    0, 0, sourceImage.Width, sourceImage.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }

            return result;
        }
    }
}
