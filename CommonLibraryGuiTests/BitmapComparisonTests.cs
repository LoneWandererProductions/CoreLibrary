/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGuiTests
 * FILE:        CommonLibraryGuiTests/BitmapRenderingTests.cs
 * PURPOSE:     Mostly Performance tests for our Wpf BitmapImage Viewer
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using Imaging;
using NUnit.Framework;

namespace CommonLibraryGuiTests
{
    /// <summary>
    ///     performance Tests
    /// </summary>
    [TestFixture]
    public class BitmapRenderingTests
    {
        /// <summary>
        ///     Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Load a test bitmap (adjust the path to your test image)
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Image", "Landscape.png");
            _testBitmap = new Bitmap(path);
        }

        /// <summary>
        ///     Cleanups this instance.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            _testBitmap.Dispose();
        }

        /// <summary>
        ///     The iterations, Number of iterations for averaging
        /// </summary>
        private const int Iterations = 10;

        /// <summary>
        ///     The tolerance, Allowable time difference in milliseconds
        /// </summary>
        private const double Tolerance = 1.0;

        /// <summary>
        ///     The test bitmap
        /// </summary>
        private Bitmap _testBitmap;

        /// <summary>
        ///     Compares the rendering speeds of Media.Image and NativeBitmapDisplay.
        /// </summary>
        [Test]
        [Apartment(ApartmentState.STA)]
        public void CompareRenderingSpeeds()
        {
            // Measure time for Media.Image with conversion
            var mediaImageTime = MeasureAverageTime(MeasureMediaImageRendering, Iterations);

            // Measure time for NativeBitmapDisplay
            var nativeDisplayTime = MeasureAverageTime(MeasureNativeBitmapRendering, Iterations);

            TestContext.WriteLine($"Media.Image Average Time: {mediaImageTime}ms");
            TestContext.WriteLine($"NativeBitmapDisplay Average Time: {nativeDisplayTime}ms");

            // Assert with tolerance
            Assert.IsTrue(nativeDisplayTime <= mediaImageTime + Tolerance,
                "NativeBitmapDisplay should be as fast or faster than Media.Image rendering. " +
                $"Difference: {nativeDisplayTime - mediaImageTime}ms (Tolerance: {Tolerance}ms)");
        }

        /// <summary>
        ///     Measures the average execution time of a given action over a specified number of iterations.
        /// </summary>
        private static double MeasureAverageTime(Func<double> measureFunction, int iterations)
        {
            double totalTime = 0;
            for (var i = 0; i < iterations; i++)
            {
                totalTime += measureFunction();
            }

            return totalTime / iterations;
        }

        /// <summary>
        ///     Simulates measuring the rendering time for Media.Image.
        ///     Replace this method with actual rendering logic.
        /// </summary>
        private static double MeasureMediaImageRendering()
        {
            var stopwatch = Stopwatch.StartNew();
            // Simulate rendering logic (replace with actual implementation)
            Thread.Sleep(10); // Simulated rendering time
            stopwatch.Stop();
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        /// <summary>
        ///     Simulates measuring the rendering time for NativeBitmapDisplay.
        ///     Replace this method with actual rendering logic.
        /// </summary>
        private static double MeasureNativeBitmapRendering()
        {
            var stopwatch = Stopwatch.StartNew();
            // Simulate rendering logic (replace with actual implementation)
            Thread.Sleep(8); // Simulated rendering time
            stopwatch.Stop();
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        /// <summary>
        ///     Compares the rendering speeds for multiple updates.
        /// </summary>
        [Test]
        [Apartment(ApartmentState.STA)]
        public void CompareRenderingSpeedsForMultipleUpdates()
        {
            // Number of updates to simulate a slideshow
            const int updateCount = 100;

            // Measure time for Media.Image with conversion
            var mediaImageTime = MeasureMediaImageRendering(updateCount);

            // Measure time for NativeBitmapDisplay
            var nativeDisplayTime = MeasureNativeBitmapRendering(updateCount);

            TestContext.WriteLine($"Media.Image Time for {updateCount} updates: {mediaImageTime}ms");
            TestContext.WriteLine($"NativeBitmapDisplay Time for {updateCount} updates: {nativeDisplayTime}ms");

            Assert.IsTrue(nativeDisplayTime <= mediaImageTime,
                "NativeBitmapDisplay should be as fast or faster than Media.Image rendering.");
        }

        /// <summary>
        ///     Measures the media image rendering.
        /// </summary>
        /// <param name="updateCount">The update count.</param>
        /// <returns>elapsed Time</returns>
        private long MeasureMediaImageRendering(int updateCount)
        {
            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < updateCount; i++)
            {
                // Convert Bitmap to BitmapSource
                var bitmapSource = BitmapToBitmapSource(_testBitmap);

                // Simulate rendering in Media.Image
                new System.Windows.Controls.Image { Source = bitmapSource };
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        ///     Measures the native bitmap rendering.
        /// </summary>
        /// <param name="updateCount">The update count.</param>
        /// <returns>elapsed Time</returns>
        private long MeasureNativeBitmapRendering(int updateCount)
        {
            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < updateCount; i++)
            {
                // Simulate rendering in NativeBitmapDisplay
                _ = new NativeBitmapDisplay { Bitmap = _testBitmap };
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        ///     Bitmaps to bitmap source.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns></returns>
        private static BitmapSource BitmapToBitmapSource(Image bitmap)
        {
            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }
}
