using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    [TestClass]
    public class RenderingTests
    {
        /// <summary>
        ///     The codebase
        /// </summary>
        private static readonly string Codebase = Directory.GetCurrentDirectory();

        /// <summary>
        ///     The executable folder
        /// </summary>
        private static readonly DirectoryInfo ExeFolder = new(Path.GetDirectoryName(Codebase) ?? string.Empty);

        /// <summary>
        ///     The project folder
        /// </summary>
        private static readonly DirectoryInfo ProjectFolder = ExeFolder.Parent?.Parent;


        /// <summary>
        ///     The sample images folder
        /// </summary>
        private static readonly DirectoryInfo SampleImagesFolder = new(Path.Combine(ProjectFolder.FullName, "Images"));

        /// <summary>
        ///     Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Setup code if any, such as initializing resources
        }

        /// <summary>
        ///     Cleanups this instance.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            // Cleanup code if needed
        }

        /// <summary>
        ///     Test to compare the processing speed between DirectBitmap and SkiaSharp.
        /// </summary>
        [TestMethod]
        public void CompareImageProcessingSpeed()
        {
            var imagePath = Path.Combine(SampleImagesFolder.FullName, "base.png");

            // Measure processing time with DirectBitmap
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            ProcessWithDirectBitmap(imagePath);
            stopwatch.Stop();
            var directBitmapTime = stopwatch.ElapsedMilliseconds;


            // Log times for comparison
            Console.WriteLine($"DirectBitmap processing time: {directBitmapTime} ms");
        }

        private void ProcessWithDirectBitmap(string imagePath)
        {
            var btm = new Bitmap(imagePath);
            // Load the image using DirectBitmap (replace with your method)
            var directBitmap = new DirectBitmap(btm);

            // Example operation: convert to grayscale
            for (var x = 0; x < directBitmap.Width; x++)
            {
                for (var y = 0; y < directBitmap.Height; y++)
                {
                    var color = directBitmap.GetPixel(x, y);
                    var grayValue = (int)((color.R * 0.3) + (color.G * 0.59) + (color.B * 0.11));
                    directBitmap.SetPixel(x, y, Color.FromArgb(grayValue, grayValue, grayValue));
                }
            }
        }
    }
}
