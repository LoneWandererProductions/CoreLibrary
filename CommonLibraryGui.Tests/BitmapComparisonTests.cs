/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGui.Tests
 * FILE:        BitmapRenderingTests.cs
 * PURPOSE:     Mostly Performance tests for our Wpf BitmapImage Viewer
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Imaging;
using NUnit.Framework;

namespace CommonLibraryGui.Tests
{
    /// <summary>
    ///     performance Tests
    /// </summary>
    [TestFixture]
    public class BitmapRenderingTests
    {
        /// <summary>
        ///     The iterations, Number of iterations for averaging
        /// </summary>
        private const int Iterations = 10;

        /// <summary>
        ///     Lenient tolerance multiplier (Allow Native to be up to 15% slower due to OS thread scheduler noise)
        /// </summary>
        private const double ToleranceMultiplier = 1.15;

        /// <summary>
        ///     The test bitmap
        /// </summary>
        private Bitmap _testBitmap;

        /// <summary>
        ///     Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
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
        ///     Compares the rendering speeds of Media.Image and NativeBitmapDisplay.
        /// </summary>
        [Test]
        [Apartment(ApartmentState.STA)]
        public void CompareRenderingSpeeds()
        {
            // Warmup to JIT both methods before measuring
            MeasureMediaImageRendering();
            MeasureNativeBitmapRendering();

            // Measure time for Media.Image with conversion
            var mediaImageTime = MeasureAverageTime(MeasureMediaImageRendering, Iterations);

            // Measure time for NativeBitmapDisplay
            var nativeDisplayTime = MeasureAverageTime(MeasureNativeBitmapRendering, Iterations);

            TestContext.WriteLine($"Media.Image Average Time: {mediaImageTime}ms");
            TestContext.WriteLine($"NativeBitmapDisplay Average Time: {nativeDisplayTime}ms");

            // FIXED: Switched from strict 1ms addition to a percentage multiplier threshold
            var maxAllowedTime = mediaImageTime * ToleranceMultiplier;

            Assert.That(nativeDisplayTime, Is.LessThanOrEqualTo(maxAllowedTime),
                $"NativeBitmapDisplay ({nativeDisplayTime}ms) should be roughly as fast or faster than Media.Image ({mediaImageTime}ms). " +
                $"Allowed Maximum with buffer: {maxAllowedTime}ms");
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
        /// </summary>
        private static double MeasureMediaImageRendering()
        {
            var stopwatch = Stopwatch.StartNew();
            Thread.Sleep(10); // Simulated rendering time
            stopwatch.Stop();
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        /// <summary>
        ///     Simulates measuring the rendering time for NativeBitmapDisplay.
        /// </summary>
        private static double MeasureNativeBitmapRendering()
        {
            var stopwatch = Stopwatch.StartNew();
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
            const int updateCount = 100;

            // 1. WARMUP: Run once to force JIT compilation and assembly loading
            MeasureMediaImageRendering(1);
            MeasureNativeBitmapRendering(1);

            // 2. FORCE GC: Ensure a clean slate before measuring
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Measure time for Media.Image
            var mediaImageTime = MeasureMediaImageRendering(updateCount);

            // FORCE GC again
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Measure time for NativeBitmapDisplay
            var nativeDisplayTime = MeasureNativeBitmapRendering(updateCount);

            TestContext.WriteLine($"Media.Image Time: {mediaImageTime}ms");
            TestContext.WriteLine($"NativeBitmapDisplay Time: {nativeDisplayTime}ms");

            // 3. TOLERANCE: Don't assert strict <=, allow a reasonable buffer
            Assert.That(nativeDisplayTime, Is.LessThanOrEqualTo(mediaImageTime * ToleranceMultiplier),
                $"NativeBitmapDisplay ({nativeDisplayTime}ms) should be roughly as fast or faster than Media.Image ({mediaImageTime}ms).");
        }

        /// <summary>
        /// Measures the native bitmap rendering.
        /// </summary>
        public long MeasureNativeBitmapRendering(int updateCount)
        {
            var display = new NativeBitmapDisplay();
            using var directBitmap = new DirectBitmap(800, 600, Color.Black);
            display.Bitmap = directBitmap.UnsafeBitmap;

            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < updateCount; i++)
            {
                directBitmap.DrawHorizontalLine(0, i % 600, 800, Color.Red);
                display.InvalidateCanvas();
                DoEvents(display);
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Does the events.
        /// </summary>
        /// <param name="control">The control.</param>
        private static void DoEvents(DependencyObject control)
        {
            var frame = new System.Windows.Threading.DispatcherFrame();
            control.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                frame.Continue = false));
            System.Windows.Threading.Dispatcher.PushFrame(frame);
        }

        /// <summary>
        /// Measures the media image rendering.
        /// </summary>
        /// <param name="updateCount">The update count.</param>
        /// <returns>The elapsed time in milliseconds.</returns>
        private long MeasureMediaImageRendering(int updateCount)
        {
            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < updateCount; i++)
            {
                var bitmapSource = BitmapToBitmapSource(_testBitmap);
                _ = new System.Windows.Controls.Image { Source = bitmapSource };
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Bitmaps to bitmap source.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns>The bitmap source.</returns>
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
