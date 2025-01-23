/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/DirectBitmapTests.cs
 * PURPOSE:     Validate some functions in my   DirectBitmap
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageCompare;
using Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     Validate Vertical lines mostly
    /// </summary>
    [TestClass]
    public class DirectBitmapTests
    {
        private static readonly string Codebase = Directory.GetCurrentDirectory();
        private static readonly DirectoryInfo ExeFolder = new(Path.GetDirectoryName(Codebase) ?? string.Empty);
        private static readonly DirectoryInfo ProjectFolder = ExeFolder.Parent?.Parent;
        private static readonly DirectoryInfo SampleImagesFolder = new(Path.Combine(ProjectFolder?.FullName ?? string.Empty, "Images"));

        /// <summary>
        /// Directs the bitmap operations should match colors correctly.
        /// </summary>
        [TestMethod]
        public void DirectBitmapOperations_ShouldMatchColorsCorrectly()
        {
            var imagePath = Path.Combine(SampleImagesFolder.FullName, "base.png");
            using var sourceBitmap = new Bitmap(imagePath);
            var directBitmap = new DirectBitmap(100, 100);

            using (var graphics = Graphics.FromImage(directBitmap.Bitmap))
            {
                graphics.DrawImage(sourceBitmap, new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                    0, 0, sourceBitmap.Width, sourceBitmap.Height, GraphicsUnit.Pixel);
            }

            VerifyColorsMatch(sourceBitmap, directBitmap, (99, 99), (0, 51), (0, 0), (51, 0));
            directBitmap.Dispose();

            // Alternate initialization and color replacement
            directBitmap = DirectBitmap.GetInstance(sourceBitmap);
            var replacementColor = Color.FromArgb(128, 128, 128);

            ApplyReplacementColors(directBitmap, replacementColor, (0, 51), (51, 51), (0, 0), (51, 0));

            using var updatedBitmap = directBitmap.Bitmap;
            VerifyReplacementColors(updatedBitmap, directBitmap, replacementColor, (0, 51), (51, 51), (0, 0), (51, 0));

            // Comparison with original image
            using var originalBitmap = new Bitmap(imagePath);
            var analysis = new ImageAnalysis();
            var comparisonResult = analysis.CompareImages(originalBitmap, directBitmap.Bitmap);
            Assert.AreEqual(100, comparisonResult.Similarity, $"Image comparison failed. Similarity: {comparisonResult.Similarity}");

            directBitmap.Dispose();
        }

        /// <summary>
        /// Performances the comparison system drawing vs direct bitmap.
        /// </summary>
        [TestMethod]
        public void PerformanceComparisonSystemDrawingVsDirectBitmap()
        {
            const int imageSize = 1000, iterations = 1000;
            using var bitmap = new Bitmap(imageSize, imageSize);
            using var blackPen = new Pen(Color.Black, 1);
            var directBitmap = DirectBitmap.GetInstance(bitmap);

            WarmUpDrawing(bitmap, blackPen, directBitmap);

            var graphicsTime = MeasurePerformance(iterations, () =>
            {
                using var graphics = Graphics.FromImage(bitmap);
                graphics.DrawLine(blackPen, 0, 0, 0, imageSize);
            });

            var directBitmapTime = MeasurePerformance(iterations, () => directBitmap.DrawVerticalLine(0, 0, imageSize, Color.Black));
            Assert.IsTrue(directBitmapTime <= graphicsTime, $"DirectBitmap slower: {directBitmapTime}ms vs {graphicsTime}ms");
        }

        /// <summary>
        /// Verticals the line drawing performance comparison.
        /// </summary>
        [TestMethod]
        public void VerticalLineDrawingPerformanceComparison()
        {
            const int width = 1000, height = 1000, iterations = 1000, lineWidth = 1;
            using var bitmap = new Bitmap(width, height);
            using var brush = new SolidBrush(Color.Black);
            var directBitmap = DirectBitmap.GetInstance(bitmap);

            WarmUpVerticalLineDrawing(bitmap, brush, directBitmap, lineWidth, height);

            var systemTime = MeasurePerformance(iterations, () =>
            {
                using var graphics = Graphics.FromImage(bitmap);
                graphics.FillRectangle(brush, 0, 0, lineWidth, height);
            });

            var directBitmapTime = MeasurePerformance(iterations, () => directBitmap.DrawRectangle(0, 0, lineWidth, height, Color.Black));
            AssertPerformanceResults("Vertical Line", systemTime, directBitmapTime);
        }

        /// <summary>
        /// Draws the single vertical line should modify bits correctly.
        /// </summary>
        [TestMethod]
        public void DrawSingleVerticalLineShouldModifyBitsCorrectly()
        {
            const int width = 1, height = 10;
            var target = new DirectBitmap(width, height);
            var color = Color.Red;
            var lines = new List<(int x, int y, int finalY, Color color)>
        {
            (0, 2, 8, color)
        };

            target.DrawVerticalLines(lines);

            for (var y = 2; y <= 8; y++)
            {
                Assert.AreEqual(color.ToArgb(), target.Bits[y * width]);
            }
        }

        /// <summary>
        /// Draws the single vertical line within bounds should modify bits correctly.
        /// </summary>
        [TestMethod]
        public void DrawSingleVerticalLineWithinBounds_ShouldModifyBitsCorrectly()
        {
            const int width = 10, height = 10;
            var target = new DirectBitmap(width + 1, height + 1);
            var color = Color.Red;
            var lines = new List<(int x, int y, int finalY, Color color)>
        {
            (5, 2, 8, color)
        };

            target.DrawVerticalLines(lines);

            for (var y = 2; y <= 8; y++)
            {
                Assert.AreEqual(color.ToArgb(), target.Bits[5 + y * (width + 1)]);
            }
        }

        /// <summary>
        /// Verifies the colors match.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="coordinates">The coordinates.</param>
        private static void VerifyColorsMatch(Bitmap source, DirectBitmap target, params (int x, int y)[] coordinates)
        {
            foreach (var (x, y) in coordinates)
            {
                var sourceColor = source.GetPixel(x, y);
                var targetColor = target.GetPixel(x, y);
                Assert.AreEqual(sourceColor.ToArgb(), targetColor.ToArgb());
            }
        }

        /// <summary>
        /// Applies the replacement colors.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="replacementColor">Color of the replacement.</param>
        /// <param name="coordinates">The coordinates.</param>
        private static void ApplyReplacementColors(DirectBitmap target, Color replacementColor, params (int x, int y)[] coordinates)
        {
            foreach (var (x, y) in coordinates)
            {
                target.SetPixel(x, y, replacementColor);
            }
        }

        /// <summary>
        /// Verifies the replacement colors.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="replacementColor">Color of the replacement.</param>
        /// <param name="coordinates">The coordinates.</param>
        private static void VerifyReplacementColors(Bitmap source, DirectBitmap target, Color replacementColor, params (int x, int y)[] coordinates)
        {
            foreach (var (x, y) in coordinates)
            {
                var sourceColor = source.GetPixel(x, y);
                var targetColor = target.GetPixel(x, y);
                Assert.AreEqual(replacementColor.ToArgb(), targetColor.ToArgb());
                Assert.AreEqual(replacementColor.ToArgb(), sourceColor.ToArgb());
            }
        }

        /// <summary>
        /// Warms up drawing.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="pen">The pen.</param>
        /// <param name="directBitmap">The direct bitmap.</param>
        private static void WarmUpDrawing(Image bitmap, Pen pen, DirectBitmap directBitmap)
        {
            for (var i = 0; i < 10; i++)
            {
                using var graphics = Graphics.FromImage(bitmap);
                graphics.DrawLine(pen, 0, 0, 0, bitmap.Height);
                directBitmap.DrawVerticalLine(0, 0, bitmap.Height, Color.Black);
            }
        }

        /// <summary>
        /// Warms up vertical line drawing.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="brush">The brush.</param>
        /// <param name="directBitmap">The direct bitmap.</param>
        /// <param name="lineWidth">Width of the line.</param>
        /// <param name="height">The height.</param>
        private static void WarmUpVerticalLineDrawing(Image bitmap, Brush brush, DirectBitmap directBitmap, int lineWidth, int height)
        {
            for (var i = 0; i < 10; i++)
            {
                using var graphics = Graphics.FromImage(bitmap);
                graphics.FillRectangle(brush, 0, 0, lineWidth, height);
                directBitmap.DrawRectangle(0, 0, lineWidth, height, Color.Black);
            }
        }

        /// <summary>
        /// Measures the performance.
        /// </summary>
        /// <param name="iterations">The iterations.</param>
        /// <param name="action">The action.</param>
        /// <returns>Time used.</returns>
        private static long MeasurePerformance(int iterations, Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++) action();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Asserts the performance results.
        /// </summary>
        /// <param name="testName">Name of the test.</param>
        /// <param name="systemTime">The system time.</param>
        /// <param name="directBitmapTime">The direct bitmap time.</param>
        private static void AssertPerformanceResults(string testName, long systemTime, long directBitmapTime)
        {
            Trace.WriteLine($"{testName}: System.Drawing: {systemTime}ms, DirectBitmap: {directBitmapTime}ms");
            Assert.IsTrue(
                directBitmapTime <= systemTime,
                $"{testName} performance mismatch. System: {systemTime}ms, DirectBitmap: {directBitmapTime}ms"
            );
        }
    }

}
