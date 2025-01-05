/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGuiTests
 * FILE:        CommonLibraryGuiTests/BitmapRenderingTests.cs
 * PURPOSE:     Mostly Performance tests for our Wpf BitmapImage Viewer
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using Imaging;
using NUnit.Framework;

namespace CommonLibraryGuiTests
{
    /// <summary>
    /// performance Tests
    /// </summary>
    [TestFixture]
    public class BitmapRenderingTests
    {
        /// <summary>
        /// The test bitmap
        /// </summary>
        private Bitmap _testBitmap;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Load a test bitmap (adjust the path to your test image)
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Image", "Landscape.png");
            _testBitmap = new Bitmap(path);
        }

        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        [TearDown]
        public void Cleanup()
        {
            _testBitmap.Dispose();
        }

        /// <summary>
        /// Compares the rendering speeds.
        /// </summary>
        [Test]
        [Apartment(ApartmentState.STA)]
        public void CompareRenderingSpeeds()
        {
            // Measure time for Media.Image with conversion
            var mediaImageTime = MeasureMediaImageRendering();

            // Measure time for NativeBitmapDisplay
            var nativeDisplayTime = MeasureNativeBitmapRendering();

            TestContext.WriteLine($"Media.Image Time: {mediaImageTime}ms");
            TestContext.WriteLine($"NativeBitmapDisplay Time: {nativeDisplayTime}ms");

            Assert.IsTrue(nativeDisplayTime <= mediaImageTime, "NativeBitmapDisplay should be as fast or faster than Media.Image rendering.");
        }

        /// <summary>
        /// Compares the rendering speeds for multiple updates.
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

            Assert.IsTrue(nativeDisplayTime <= mediaImageTime, "NativeBitmapDisplay should be as fast or faster than Media.Image rendering.");
        }

        /// <summary>
        /// Measures the media image rendering.
        /// </summary>
        /// <param name="updateCount">The update count.</param>
        /// <returns>elapsed Time</returns>
        private long MeasureMediaImageRendering(int updateCount)
        {
            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < updateCount; i++)
            {
                // Convert Bitmap to BitmapSource
                BitmapSource bitmapSource = BitmapToBitmapSource(_testBitmap);

                // Simulate rendering in Media.Image
                new System.Windows.Controls.Image { Source = bitmapSource };
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Measures the native bitmap rendering.
        /// </summary>
        /// <param name="updateCount">The update count.</param>
        /// <returns>elapsed Time</returns>
        private long MeasureNativeBitmapRendering(int updateCount)
        {
            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < updateCount; i++)
            {
                // Simulate rendering in NativeBitmapDisplay
                new NativeBitmapDisplay { Bitmap = _testBitmap };
            }

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Measures the media image rendering.
        /// </summary>
        /// <returns></returns>
        private long MeasureMediaImageRendering()
        {
            var stopwatch = Stopwatch.StartNew();

            // Convert Bitmap to BitmapSource
            BitmapSource bitmapSource = BitmapToBitmapSource(_testBitmap);

            // Simulate rendering in Media.Image
            // Normally, we'd add this to a visual tree in WPF, but here we're testing only the conversion/rendering logic.
            var imageControl = new System.Windows.Controls.Image { Source = bitmapSource };

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Measures the native bitmap rendering.
        /// </summary>
        /// <returns></returns>
        private long MeasureNativeBitmapRendering()
        {
            var stopwatch = Stopwatch.StartNew();

            // Simulate rendering in NativeBitmapDisplay
            new NativeBitmapDisplay { Bitmap = _testBitmap };

            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Bitmaps to bitmap source.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns></returns>
        private static BitmapSource BitmapToBitmapSource(Image bitmap)
        {
            using var memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
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
