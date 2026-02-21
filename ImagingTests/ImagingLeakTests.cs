/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImagingTests
 * FILE:        ImagingLeakTests.cs
 * PURPOSE:     Search for leaks in the ImagingFacade methods by running them multiple times and monitoring memory usage. This is a very basic test and should be used as a starting point for more comprehensive leak detection.
 *              Should be run in a loop or with a profiler for best results, as small leaks might not be detected in a single run.
 *              More calls will amplify the leak if it exists, but be mindful of test execution time.
 *              Should be extended in the future to cover more methods and scenarios, especially those involving unmanaged resources.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Drawing;
using Imaging;

namespace ImagingTests
{
    /// <summary>
    /// Simple Memory checker for the ImagingFacade methods. It runs the method multiple times and checks if the memory usage increased significantly, which would indicate a leak.
    /// </summary>
    [TestClass]
    public class ImagingLeakTests
    {
        /// <summary>
        /// Tests the pixelate for memory leaks.
        /// </summary>
        [TestMethod]
        public void TestPixelateForMemoryLeaks()
        {
            // 1. Setup a dummy bitmap
            using var testBmp = new Bitmap(2000, 2000);

            // 2. Wrap the execution in our Monitor
            LeakDetector.Monitor(() =>
            {
                // Run the operation multiple times in one go to amplify any small leak
                for (var i = 0; i < 5; i++)
                {
                    // We simulate the UI usage: Load -> Process -> Convert -> Dispose
                    using (var result = ImagingFacade.Resize(testBmp, 1000, 1000))
                    {
                        var uiImage = ImagingFacade.ToBitmapImage(result);
                        // uiImage is now managed by WPF, result is disposed here
                    }
                }
            }, "Facade Resize and Convert Leak Test");
        }

        /// <summary>
        /// Tests the filter leak.
        /// </summary>
        [TestMethod]
        public void TestFilterLeak()
        {
            using var testBmp = new Bitmap(1000, 1000);

            LeakDetector.Monitor(() =>
            {
                // Testing if ApplyFilter cleans up its internal DirectBitmap
                var filtered = ImagingFacade.ApplyFilter(testBmp, Imaging.Enums.FiltersType.Sepia);
                filtered?.Dispose();
            }, "Sepia Filter Disposal Test");
        }
    }
}
