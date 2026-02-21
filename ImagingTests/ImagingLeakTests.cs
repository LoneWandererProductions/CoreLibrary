using System.Drawing;
using Imaging;

namespace ImagingTests
{
    [TestClass]
    public class ImagingLeakTests
    {
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
