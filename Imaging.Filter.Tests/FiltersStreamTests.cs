/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Texture
 * FILE:        FiltersStreamTests.cs
 * PURPOSE:     Mostly visual tests for the filter generation methods in FiltersStream.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Imaging.Enums;
using Imaging.Helpers;

namespace Imaging.Filter.Tests
{
    /// <summary>
    /// Filter testing class for visual verification of image filters.
    /// </summary>
    [TestClass]
    public class FiltersStreamTests
    {
        /// <summary>
        /// The test width
        /// </summary>
        private const int TestWidth = 256;

        /// <summary>
        /// The test height
        /// </summary>
        private const int TestHeight = 256;

        /// <summary>
        /// The output directory
        /// </summary>
        private string? _outputDirectory;

        /// <summary>
        /// The test image
        /// </summary>
        private Bitmap? _testImage;

        /// <summary>
        /// Sets up the visual test directory and generates a standard test image pattern.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FilterVisualTests");
            if (!Directory.Exists(_outputDirectory))
            {
                Directory.CreateDirectory(_outputDirectory);
            }

            // Generate the base image in memory for the current test
            _testImage = GenerateTestPattern(TestWidth, TestHeight);

            var controlPath = Path.Combine(_outputDirectory, "00_Original_Control.png");

            // Only save the control image if it hasn't been written by another test yet
            if (!File.Exists(controlPath))
            {
                try
                {
                    _testImage.Save(controlPath, ImageFormat.Png);
                    Trace.WriteLine($"Saved Control Image: {controlPath}");
                }
                catch (System.Runtime.InteropServices.ExternalException)
                {
                    // Suppress the exception if another parallel test thread locked the file first.
                    // The image is identical, so we don't care which thread successfully saves it.
                }
            }

            Trace.WriteLine($"Filtered images will be saved to: {_outputDirectory}");
        }

        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            // Dispose of the base image to prevent memory leaks during large test runs
            _testImage?.Dispose();
        }

        /// <summary>
        /// Filters the gray scale visual test.
        /// </summary>
        [TestMethod]
        public void Filter_GrayScale_VisualTest()
        {
            using var result = FiltersStream.FilterImage(_testImage, FiltersType.GrayScale);
            SaveBitmapToImage(result, "01_GrayScale.png");
        }

        /// <summary>
        /// Filters the invert visual test.
        /// </summary>
        [TestMethod]
        public void Filter_Invert_VisualTest()
        {
            using var result = FiltersStream.FilterImage(_testImage, FiltersType.Invert);
            SaveBitmapToImage(result, "02_Invert.png");
        }

        /// <summary>
        /// Filters the contour visual test.
        /// </summary>
        [TestMethod]
        public void Filter_Contour_VisualTest()
        {
            // Tests the Sobel implementation
            using var result = FiltersStream.FilterImage(_testImage, FiltersType.Contour);
            SaveBitmapToImage(result, "03_Contour.png");
        }

        /// <summary>
        /// Filters the gaussian blur visual test.
        /// </summary>
        [TestMethod]
        public void Filter_GaussianBlur_VisualTest()
        {
            using var result = FiltersStream.FilterImage(_testImage, FiltersType.GaussianBlur);
            SaveBitmapToImage(result, "04_GaussianBlur.png");
        }

        /// <summary>
        /// Filters the edge enhance visual test.
        /// </summary>
        [TestMethod]
        public void Filter_EdgeEnhance_VisualTest()
        {
            using var result = FiltersStream.FilterImage(_testImage, FiltersType.EdgeEnhance);
            SaveBitmapToImage(result, "05_EdgeEnhance.png");
        }

        /// <summary>
        /// Filters the pencil sketch effect visual test.
        /// </summary>
        [TestMethod]
        public void Filter_PencilSketchEffect_VisualTest()
        {
            using var result = FiltersStream.FilterImage(_testImage, FiltersType.PencilSketchEffect);
            SaveBitmapToImage(result, "06_PencilSketch.png");
        }

        /// <summary>
        /// Filters the floyd steinberg dithering visual test.
        /// </summary>
        [TestMethod]
        public void Filter_FloydSteinbergDithering_VisualTest()
        {
            using var result = FiltersStream.FilterImage(_testImage, FiltersType.FloydSteinbergDithering);
            SaveBitmapToImage(result, "07_FloydSteinberg.png");
        }

        /// <summary>
        /// Pixelates the visual test.
        /// </summary>
        [TestMethod]
        public void Pixelate_VisualTest()
        {
            // Pixelate is a separate internal static method, so we test it directly
            using var result = FiltersStream.Pixelate(_testImage, 8); // Step width of 8
            SaveBitmapToImage(result, "08_Pixelate.png");
        }

        /// <summary>
        /// Creates a test pattern with gradients, text, and geometric shapes to test various filter types.
        /// </summary>
        private Bitmap GenerateTestPattern(int width, int height)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);

            // 1. Diagonal Gradient Background (Tests banding and color matrix shifts)
            using (var brush = new LinearGradientBrush(new Point(0, 0), new Point(width, height), Color.DarkBlue, Color.Orange))
            {
                g.FillRectangle(brush, 0, 0, width, height);
            }

            // 2. High-contrast geometric shapes (Tests convolution edge detection like Sobel/Laplacian)
            g.FillEllipse(Brushes.Red, 30, 30, 80, 80);
            g.FillRectangle(Brushes.LimeGreen, 120, 150, 100, 50);

            using (var pen = new Pen(Color.White, 4))
            {
                g.DrawLine(pen, 10, height - 10, width - 10, 10);
            }

            // 3. Noise/Detail area (Tests blurring and sharpening)
            var rand = new Random(42); // Fixed seed for reproducible tests
            for (int i = 0; i < 500; i++)
            {
                int x = rand.Next(width / 2, width);
                int y = rand.Next(0, height / 2);
                bmp.SetPixel(x, y, Color.Yellow);
            }

            return bmp;
        }

        /// <summary>
        /// Saves the resulting bitmap to a standard PNG file.
        /// </summary>
        private void SaveBitmapToImage(Image? bmp, string filename)
        {
            Assert.IsNotNull(bmp, $"Filter resulted in a null bitmap for {filename}");

            if (_outputDirectory == null)
            {
                return;
            }

            var filePath = Path.Combine(_outputDirectory, filename);
            bmp.Save(filePath, ImageFormat.Png);

            Trace.WriteLine($"Saved: {filePath}");
        }
    }
}
