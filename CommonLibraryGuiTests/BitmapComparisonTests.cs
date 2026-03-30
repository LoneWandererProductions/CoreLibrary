/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGuiTests
 * FILE:        BitmapRenderingTests.cs
 * PURPOSE:     Mostly Performance tests for our Wpf BitmapImage Viewer
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;
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
            const int updateCount = 100;
            const double toleranceMultiplier = 1.10; // Allow Native to be up to 10% slower due to random noise

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
            Assert.IsTrue(nativeDisplayTime <= (mediaImageTime * toleranceMultiplier),
                $"NativeBitmapDisplay ({nativeDisplayTime}ms) should be roughly as fast or faster than Media.Image ({mediaImageTime}ms).");
        }

        /// <summary>
        /// Measures the native bitmap rendering.
        /// </summary>
        /// <param name="updateCount">The update count.</param>
        /// <returns></returns>
        public long MeasureNativeBitmapRendering(int updateCount)
        {
            // 1. Initialize the UI Control
            var display = new NativeBitmapDisplay();

            // 2. Initialize your Shared Memory (DirectBitmap)
            // Let's assume a 800x600 canvas
            using var directBitmap = new DirectBitmap(800, 600, Color.Black);

            // 3. THE HANDSHAKE (Do this ONLY ONCE)
            // This passes the shared memory canvas to the WinForms picture box.
            display.Bitmap = directBitmap.UnsafeBitmap;

            var stopwatch = Stopwatch.StartNew();

            // 4. THE RENDER LOOP
            for (var i = 0; i < updateCount; i++)
            {
                // A. Mutate the pixels directly in RAM (Zero allocations!)
                // In a real app, this might be copying a video frame array.
                // Here, we just draw a line to simulate work.
                directBitmap.DrawHorizontalLine(0, i % 600, 800, Color.Red);

                // B. Tell the UI to repaint what is in memory
                display.InvalidateCanvas();

                // (Optional: In a UI test, you sometimes need to pump the WPF Dispatcher
                // here to force it to actually draw to the screen immediately, otherwise
                // it just queues up 100 invalidate requests and draws once at the end).
                DoEvents(display);
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Helper method to force WPF to actually render during a synchronous test loop
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
                _ = new System.Windows.Controls.Image { Source = bitmapSource };
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        ///     Bitmaps to bitmap source.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns>BitmapSource</returns>
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
