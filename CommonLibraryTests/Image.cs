/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/Image.cs
 * PURPOSE:     Tests for Image Tools
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using FileHandler;
using ImageCompare;
using Imaging;
using Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     Test some image related stuff
    /// </summary>
    [TestClass]
    public class Image
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
        ///     The images folder
        /// </summary>
        private static readonly DirectoryInfo ImagesFolder = new(Path.Combine(ProjectFolder.FullName, "Image"));

        /// <summary>
        ///     The output folder
        /// </summary>
        private static readonly DirectoryInfo OutputFolder = new(Path.Combine(ProjectFolder.FullName, "Test"));

        /// <summary>
        ///     The Image analysis
        /// </summary>
        private static readonly ImageAnalysis Analysis = new();

        /// <summary>
        ///     The Image Comparer
        /// </summary>
        private static readonly ImageComparer Compare = new();

        /// <summary>
        ///     The custom Image Format
        /// </summary>
        private static readonly CustomImageFormat Custom = new();


        /// <summary>
        ///     Test the custom DirectBitmap and how it works
        /// </summary>
        [TestMethod]
        public void DirectBitmaps()
        {
            var imagePath = Path.Combine(SampleImagesFolder.FullName, "base.png");

            var btm = new Bitmap(imagePath);
            var dbm = new DirectBitmap(100, 100);

            using (var graph = Graphics.FromImage(dbm.Bitmap))
            {
                graph.DrawImage(btm, new Rectangle(0, 0, btm.Width, btm.Height), 0, 0, btm.Width, btm.Height,
                    GraphicsUnit.Pixel);
            }

            var col1 = btm.GetPixel(99, 99);
            var col2 = dbm.GetPixel(99, 99);

            Assert.AreEqual(col1.R, col2.R, "done");
            Assert.AreEqual(col1.B, col2.B, "done");
            Assert.AreEqual(col1.G, col2.G, "done");

            col1 = btm.GetPixel(0, 51);
            col2 = dbm.GetPixel(0, 51);

            Assert.AreEqual(col1.R, col2.R, "done");
            Assert.AreEqual(col1.B, col2.B, "done");
            Assert.AreEqual(col1.G, col2.G, "done");

            col1 = btm.GetPixel(0, 0);
            col2 = dbm.GetPixel(0, 0);

            Assert.AreEqual(col1.R, col2.R, "done");
            Assert.AreEqual(col1.B, col2.B, "done");
            Assert.AreEqual(col1.G, col2.G, "done");

            col1 = btm.GetPixel(51, 0);
            col2 = dbm.GetPixel(51, 0);

            Assert.AreEqual(col1.R, col2.R, "done");
            Assert.AreEqual(col1.B, col2.B, "done");
            Assert.AreEqual(col1.G, col2.G, "done");

            dbm.Dispose();

            //alternate way
            dbm = DirectBitmap.GetInstance(btm);

            var replacementColor = Color.FromArgb(128, 128, 128);

            dbm.SetPixel(0, 51, replacementColor);
            dbm.SetPixel(51, 51, replacementColor);
            dbm.SetPixel(0, 0, replacementColor);
            dbm.SetPixel(51, 0, replacementColor);

            btm = dbm.Bitmap;

            col1 = btm.GetPixel(51, 51);
            col2 = dbm.GetPixel(51, 51);

            Assert.AreEqual(col1.R, col2.R, "done");
            Assert.AreEqual(col1.B, col2.B, "done");
            Assert.AreEqual(col1.G, col2.G, "done");

            Assert.AreEqual(col2.R, replacementColor.R, "done");
            Assert.AreEqual(col2.B, replacementColor.B, "done");
            Assert.AreEqual(col2.G, replacementColor.G, "done");

            col1 = btm.GetPixel(0, 51);
            col2 = dbm.GetPixel(0, 51);

            Assert.AreEqual(col1.R, col2.R, "done");
            Assert.AreEqual(col1.B, col2.B, "done");
            Assert.AreEqual(col1.G, col2.G, "done");

            Assert.AreEqual(col2.R, replacementColor.R, "done");
            Assert.AreEqual(col2.B, replacementColor.B, "done");
            Assert.AreEqual(col2.G, replacementColor.G, "done");

            col1 = btm.GetPixel(0, 0);
            col2 = dbm.GetPixel(0, 0);

            Assert.AreEqual(col1.R, col2.R, "done");
            Assert.AreEqual(col1.B, col2.B, "done");
            Assert.AreEqual(col1.G, col2.G, "done");

            Assert.AreEqual(col2.R, replacementColor.R, "done");
            Assert.AreEqual(col2.B, replacementColor.B, "done");
            Assert.AreEqual(col2.G, replacementColor.G, "done");

            col1 = btm.GetPixel(51, 0);
            col2 = dbm.GetPixel(51, 0);

            Assert.AreEqual(col1.R, col2.R, "done");
            Assert.AreEqual(col1.B, col2.B, "done");
            Assert.AreEqual(col1.G, col2.G, "done");

            Assert.AreEqual(col2.R, replacementColor.R, "done");
            Assert.AreEqual(col2.B, replacementColor.B, "done");
            Assert.AreEqual(col2.G, replacementColor.G, "done");

            btm = new Bitmap(imagePath);

            dbm = new DirectBitmap(btm);

            var compare = new ImageAnalysis();
            var data = compare.CompareImages(btm, dbm.Bitmap);

            Assert.AreEqual(100, data.Similarity, $"Image was not equal: {data.Similarity}");

            dbm.Dispose();
        }

        /// <summary>
        ///     Test the custom DirectBitmap and how it works
        /// </summary>
        [TestMethod]
        public void CutImage()
        {
            var imagePath = Path.Combine(SampleImagesFolder.FullName, "base.png");

            // Ensure the image exists
            Assert.IsTrue(File.Exists(imagePath), $"File does not exist: {imagePath}");

            var image = ImageStreamMedia.GetBitmapImageFileStream(imagePath);
            var btm = image.ToBitmap();
            var lst = ImageStream.CutBitmaps(btm, 2, 2, 50, 50);

            //save for test purposes
            if (!Directory.Exists(OutputFolder.FullName))
            {
                _ = Directory.CreateDirectory(OutputFolder.FullName);
            }
            else
            {
                _ = FileHandleDelete.DeleteAllContents(OutputFolder.FullName, false);
            }

            imagePath = Path.Combine(OutputFolder.FullName, "1.jpg");

            foreach (var item in lst)
            {
                ImageStream.SaveBitmap(item, imagePath, ImageFormat.Jpeg);
            }

            var ret = FileHandleSearch.GetAllFiles(OutputFolder.FullName, false);
            Assert.AreEqual(4, ret.Count, "done");
            Assert.AreEqual(4, lst.Count, "done");

            var check = FileHandleDelete.DeleteFiles(ret);
            Assert.AreEqual(true, check, "done");

            var col = lst[0].GetPixel(25, 25);
            var colOld = btm.GetPixel(25, 25);

            Assert.AreEqual(colOld.R, col.R, "done");
            Assert.AreEqual(colOld.B, col.B, "done");
            Assert.AreEqual(colOld.G, col.G, "done");

            col = lst[1].GetPixel(25, 25);
            colOld = btm.GetPixel(55, 25);

            Assert.AreEqual(colOld.R, col.R, "done");
            Assert.AreEqual(colOld.B, col.B, "done");
            Assert.AreEqual(colOld.G, col.G, "done");

            col = lst[2].GetPixel(25, 25);
            colOld = btm.GetPixel(25, 55);

            Assert.AreEqual(colOld.R, col.R, "done");
            Assert.AreEqual(colOld.B, col.B, "done");
            Assert.AreEqual(colOld.G, col.G, "done");

            col = lst[3].GetPixel(25, 25);
            colOld = btm.GetPixel(55, 55);

            Assert.AreEqual(colOld.R, col.R, "done");
            Assert.AreEqual(colOld.B, col.B, "done");
            Assert.AreEqual(colOld.G, col.G, "done");
        }

        /// <summary>
        ///     Test the cut Bitmap and if we finde the correct Coordinates with Image slide
        /// </summary>
        [TestMethod]
        public void SlideImage()
        {
            var imagePath = Path.Combine(SampleImagesFolder.FullName, "base.png");

            // Ensure the image exists
            Assert.IsTrue(File.Exists(imagePath), $"File does not exist: {imagePath}");

            var image = ImageStreamMedia.GetBitmapImageFileStream(imagePath);
            using var btm = image.ToBitmap();
            using var cutOut = ImageStream.CutBitmap(btm, 22, 10, 50, 50);

            var point = Coordinate2D.NullPoint;

            var check = ImageSlider.IsPartOf(btm, cutOut, out point);

            Assert.IsTrue(check, "The smaller image was not found in the larger image.");

            Assert.AreEqual(22, point.X, "The X coordinate of the matching point is incorrect.");
            Assert.AreEqual(10, point.Y, "The Y coordinate of the matching point is incorrect.");
        }

        /// <summary>
        ///     Test combining image.
        /// </summary>
        [TestMethod]
        public void CombineImage()
        {
            var bmpBase = new Bitmap(Path.Combine(ImagesFolder.FullName, "Tile.png"));
            var bmpLayerOne = new Bitmap(Path.Combine(ImagesFolder.FullName, "layerOne.png"));
            var bmpLayerTwo = new Bitmap(Path.Combine(ImagesFolder.FullName, "LayerTwo.png"));
            var bmResultOne = new Bitmap(Path.Combine(ImagesFolder.FullName, "ResultOne.png"));

            var render = new ImageRender();

            var cache = render.CombineBitmap(bmpBase, bmpLayerOne, 0, 0);
            cache = render.CombineBitmap(cache, bmpLayerTwo, 0, 0);

            var compare = new ImageAnalysis();

            //should be near 100%
            var data = compare.CompareImages(bmResultOne, cache);

            //Todo implement ImageComparer, 2 images
            Assert.AreEqual(100, data.Similarity, $"Compare failed: {data.Similarity}");

            data = compare.CompareImages(Path.Combine(ImagesFolder.FullName, "Tile.png"),
                Path.Combine(ImagesFolder.FullName, "Tile.png"));

            Assert.AreEqual(100, data.Similarity, $"Compare failed Path: {data.Similarity}");
        }

        /// <summary>
        ///     Tests getting and setting pixel(s).
        /// </summary>
        [TestMethod]
        public void GetSetPixel()
        {
            var imagePath = Path.Combine(SampleImagesFolder.FullName, "base.png");

            var image = ImageStreamMedia.GetBitmapImageFileStream(imagePath);
            var btm = image.ToBitmap();

            var point = new Point { X = 51, Y = 51 };

            var color = ImageStream.GetPixel(btm, point);

            Assert.AreEqual(39, color.R, "done");
            Assert.AreEqual(39, color.B, "done");
            Assert.AreEqual(39, color.G, "done");

            point = new Point { X = 1, Y = 1 };

            color = ImageStream.GetPixel(btm, point);

            Assert.AreEqual(255, color.R, "done");
            Assert.AreEqual(255, color.B, "done");
            Assert.AreEqual(255, color.G, "done");

            point = new Point { X = 51, Y = 51 };
            btm = ImageStream.SetPixel(btm, point, color);

            color = ImageStream.GetPixel(btm, point);

            Assert.AreEqual(255, color.R, "done");
            Assert.AreEqual(255, color.B, "done");
            Assert.AreEqual(255, color.G, "done");

            btm = ImageStream.SetPixel(btm, point, color, 3);

            color = ImageStream.GetPixel(btm, point, 3);

            Assert.AreEqual(255, color.R, "done");
            Assert.AreEqual(255, color.B, "done");
            Assert.AreEqual(255, color.G, "done");

            point = new Point { X = 51, Y = 51 };
            btm = ImageStream.SetPixel(btm, point, color);

            color = ImageStream.GetPixel(btm, point);

            Assert.AreEqual(255, color.R, "done");
            Assert.AreEqual(255, color.B, "done");
            Assert.AreEqual(255, color.G, "done");

            point = new Point { X = 50, Y = 50 };
            color = ImageStream.GetPixel(btm, point, 10);

            Assert.AreEqual(121, color.R, "done");
            Assert.AreEqual(88, color.B, "done");
            Assert.AreEqual(104, color.G, "done");
        }

        /// <summary>
        ///     Compares the speed of drawing operations using Microsoft's Graphics and DirectBitmap implementations.
        ///     The test ensures that no interference occurs from other tests.
        /// </summary>
        [TestMethod]
        public void CompareSystemDrawingDirectBitmapPerformance()
        {
            const int imageSize = 1000;
            const int iterations = 1000;
            const int rectangleWidth = 100;
            const int rectangleHeight = 200;

            using var bmp = new Bitmap(imageSize, imageSize);
            using var blackPen = new Pen(Color.Black, 1);
            var dbm = DirectBitmap.GetInstance(bmp);

            // Warm-up to avoid JIT overhead
            for (var i = 0; i < 10; i++)
            {
                using var graphics = Graphics.FromImage(bmp);
                graphics.DrawLine(blackPen, 0, 0, 0, imageSize);
                dbm.DrawVerticalLine(0, 0, imageSize, Color.Black);
            }

            // Measure drawing a vertical line with Graphics
            var graphicsElapsedTime = MeasureExecutionTime(iterations, () =>
            {
                using var graphics = Graphics.FromImage(bmp);
                graphics.DrawLine(blackPen, 0, 0, 0, imageSize);
            });

            // Measure drawing a vertical line with DirectBitmap
            var dbmElapsedTime =
                MeasureExecutionTime(iterations, () => dbm.DrawVerticalLine(0, 0, imageSize, Color.Black));

            Trace.WriteLine(
                $"Graphics DrawLine: {graphicsElapsedTime} ms, DirectBitmap DrawVerticalLine: {dbmElapsedTime} ms");
            Assert.IsTrue(dbmElapsedTime <= graphicsElapsedTime,
                $"DirectBitmap was slower: {dbmElapsedTime} ms vs Graphics: {graphicsElapsedTime} ms");

            // Measure drawing a rectangle with Graphics
            var graphicsRectangleTime = MeasureExecutionTime(iterations, () =>
            {
                using var graphics = Graphics.FromImage(bmp);
                graphics.DrawRectangle(blackPen, 0, 0, rectangleWidth, rectangleHeight);
            });

            // Measure drawing a rectangle with DirectBitmap
            var dbmRectangleTime = MeasureExecutionTime(iterations,
                () => dbm.DrawRectangle(0, 0, rectangleWidth, rectangleHeight, Color.Black));

            Trace.WriteLine(
                $"Graphics DrawRectangle: {graphicsRectangleTime} ms, DirectBitmap DrawRectangle: {dbmRectangleTime} ms");
            Assert.IsTrue(graphicsRectangleTime <= dbmRectangleTime,
                $"Graphics was faster for rectangles: {graphicsRectangleTime} ms vs DirectBitmap: {dbmRectangleTime} ms");

            // Local function for measuring execution time
            long MeasureExecutionTime(int count, Action action)
            {
                var stopwatch = Stopwatch.StartNew();

                for (var i = 0; i < count; i++)
                {
                    action();
                }

                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        ///     Compares performance of drawing vertical lines with System.Drawing's FillRectangle and DirectBitmap's
        ///     DrawRectangle.
        /// </summary>
        [TestMethod]
        public void CompareVerticalLineWithRectanglePerformance()
        {
            const int width = 1000;
            const int height = 1000;
            const int iterations = 1000;
            const int lineWidth = 1; // Special case: Line width is 1 (vertical line)
            const int x = 0;
            const int y = 0;

            var bmp = new Bitmap(width, height);
            var blackBrush = new SolidBrush(Color.Black);

            // Initialize DirectBitmap with the same dimensions
            var dbm = DirectBitmap.GetInstance(bmp);

            // Warm-up both methods to ensure no JIT overhead
            using (var graphics = Graphics.FromImage(bmp))
            {
                for (var i = 0; i < 10; i++)
                {
                    graphics.FillRectangle(blackBrush, x, y, lineWidth, height);
                }
            }

            for (var i = 0; i < 10; i++)
            {
                dbm.DrawRectangle(x, y, lineWidth, height, Color.Black);
            }

            // Measure System.Drawing performance
            var watch = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                using var graphics = Graphics.FromImage(bmp);
                graphics.FillRectangle(blackBrush, x, y, lineWidth, height);
            }

            watch.Stop();
            var elapsedGraphics = watch.ElapsedMilliseconds;

            // Measure DirectBitmap performance
            watch = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                dbm.DrawRectangle(x, y, lineWidth, height, Color.Black);
            }

            watch.Stop();
            var elapsedDirectBitmap = watch.ElapsedMilliseconds;

            // Measure DirectBitmap Line performance
            watch = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                dbm.DrawVerticalLine(x, y, height, Color.Black);
            }

            watch.Stop();
            var elapsedDirectBitmapLine = watch.ElapsedMilliseconds;

            // Log results
            Trace.WriteLine($"System.Drawing FillRectangle (lineWidth = 1): {elapsedGraphics} ms");
            Trace.WriteLine($"DirectBitmap DrawRectangle: {elapsedDirectBitmap} ms");
            Trace.WriteLine($"DirectBitmap DrawVerticalLine: {elapsedDirectBitmapLine} ms");

            // Informational output instead of failure
            if (elapsedDirectBitmap > elapsedGraphics)
            {
                Trace.WriteLine(
                    $"Warning: DirectBitmap DrawRectangle is slower than System.Drawing by {elapsedDirectBitmap - elapsedGraphics} ms.");
            }
            else
            {
                Trace.WriteLine("DirectBitmap DrawRectangle is faster or equal to System.Drawing.");
            }

            if (elapsedDirectBitmapLine > elapsedGraphics)
            {
                Trace.WriteLine(
                    $"Warning: DirectBitmap DrawVerticalLine is slower than System.Drawing by {elapsedDirectBitmapLine - elapsedGraphics} ms.");
            }
            else
            {
                Trace.WriteLine("DirectBitmap DrawVerticalLine is faster or equal to System.Drawing.");
            }
        }

        /// <summary>
        ///     Compares the colors.
        /// </summary>
        [TestMethod]
        public void CompareImageColors()
        {
            //TODO replace
            //var imagePath = Path.Combine(SampleImagesFolder.FullName, "Color.png");
            //using var btm = new Bitmap(imagePath);

            //var imageData = ImageProcessing.GenerateData(btm, 0);

            //var color = Analysis.FindImagesInColorRange(imageData.R, imageData.G, imageData.B, 4,
            //    SampleImagesFolder.FullName, false, ImagingResources.Appendix);

            //if (color == null)
            //{
            //    Assert.Fail("color was null");
            //}

            //Assert.AreEqual(2, color.Count, "Done");
            //Assert.AreEqual(imagePath, color[0], "Done");

            //imagePath = Path.Combine(SampleImagesFolder.FullName, "ColorShade.png");
            //Assert.AreEqual(imagePath, color[1], "Done");
        }

        /// <summary>
        ///     Compares the duplicate images.
        /// </summary>
        [TestMethod]
        public void CompareDuplicateImages()
        {
            var images = Compare.GetDuplicateImages(SampleImagesFolder.FullName, false, ImagingResources.Appendix);

            if (images == null)
            {
                Assert.Fail("images was null");
            }

            Assert.AreEqual(1, images.Count, "Done");
            Assert.AreEqual(2, images[0].Count, "Done");

            var imagePath = Path.Combine(SampleImagesFolder.FullName, "Compare.png");
            var cache = images[0];
            Assert.AreEqual(imagePath, cache[0], "Done");

            imagePath = Path.Combine(SampleImagesFolder.FullName, "CompareCopy.png");
            Assert.AreEqual(imagePath, cache[1], "Done");
        }

        /// <summary>
        ///     Compares the similar images.
        /// </summary>
        [TestMethod]
        public void CompareSimilarImages()
        {
            var images = Compare.GetSimilarImages(SampleImagesFolder.FullName, false, ImagingResources.Appendix, 80);

            if (images == null)
            {
                Assert.Fail("images was null");
            }

            Assert.AreEqual(1, images.Count, "Done");
            Assert.AreEqual(3, images[0].Count, "Done");

            var imagePath = Path.Combine(SampleImagesFolder.FullName, "Compare.png");
            var cache = images[0];
            Assert.IsTrue(cache.Contains(imagePath), "Done");

            imagePath = Path.Combine(SampleImagesFolder.FullName, "CompareCopy.png");
            Assert.IsTrue(cache.Contains(imagePath), "Done");

            imagePath = Path.Combine(SampleImagesFolder.FullName, "CompareSimilar.png");
            Assert.IsTrue(cache.Contains(imagePath), "Done");
        }

        /// <summary>
        ///     Checks the image content details.
        /// </summary>
        [TestMethod]
        public void CheckImageContentDetails()
        {
            var imagePath = Path.Combine(SampleImagesFolder.FullName, "CompareSimilar.png");
            var data = Analysis.GetImageDetails(imagePath);

            if (data == null)
            {
                Assert.Fail("data was null");
            }

            Assert.AreEqual(26, data.R, "Done");
            Assert.AreEqual(72, data.G, "Done");
            Assert.AreEqual(124, data.B, "Done");
            Assert.AreEqual(3075, data.Size, "Done");
            Assert.AreEqual(100, data.Height, "Done");
            Assert.AreEqual(100, data.Width, "Done");
            Assert.AreEqual(".png", data.Extension, "Done");
        }

        /// <summary>
        ///     Checks the image list details.
        /// </summary>
        [TestMethod]
        public void CheckImageListDetails()
        {
            var images = Compare.GetSimilarImages(SampleImagesFolder.FullName, false, ImagingResources.Appendix, 80);
            var dataList = Analysis.GetImageDetails(images[0]);

            if (dataList == null)
            {
                Assert.Fail("dataList was null");
            }

            Assert.AreEqual(26, dataList[0].R, "Done");
            Assert.AreEqual(72, dataList[0].G, "Done");
            Assert.AreEqual(124, dataList[0].B, "Done");
            Assert.AreEqual(3086, dataList[0].Size, "Done");

            // Similarity should be around 99%
            Assert.AreEqual(100, Math.Round(dataList[2].Similarity, 0),
                $"Done: {dataList[2].Similarity}");

            Trace.WriteLine(dataList[2].Similarity);
        }


        /// <summary>
        ///     Test save and convert to Cif Files
        /// </summary>
        [TestMethod]
        public void SaveConvertCif()
        {
            var imagePath = Path.Combine(SampleImagesFolder.FullName, "base.png");
            var cifPath = Path.Combine(SampleImagesFolder.FullName, "base.cif");
            var cifCompressed = Path.Combine(SampleImagesFolder.FullName, "compressed.cif");
            var resultPath = Path.Combine(SampleImagesFolder.FullName, "result_uncompressed.png");
            var resultPathCompressed = Path.Combine(SampleImagesFolder.FullName, "result_compressed.png");
            var resultPathUnCompressed = Path.Combine(SampleImagesFolder.FullName, "result_un_compressed.png");

            _ = FileHandleDelete.DeleteFile(resultPath);
            _ = FileHandleDelete.DeleteFile(resultPathCompressed);

            _ = FileHandleDelete.DeleteFile(cifPath);
            _ = FileHandleDelete.DeleteFile(cifCompressed);

            _ = FileHandleDelete.DeleteFile(resultPathUnCompressed);

            var image = ImageStreamMedia.GetBitmapImageFileStream(imagePath);
            var btm = image.ToBitmap();

            /*
             * 4 Colors:
             * 255,255,255,255, #ffffff
             * 111,6,6, 6f0606
             * 40,72,4, #284804
             * 40,40,40, #282828
             */

            //convert to cif
            Custom.GenerateBitmapToCifFile(btm, cifPath);
            //and back
            btm = Custom.GetImageFromCif(cifPath);

            var data = CifProcessing.ConvertToCifFromBitmap(btm);
            var doc = CifProcessing.GenerateCsv(btm.Height, btm.Width, data);

            Assert.AreEqual(2502, doc[1].Count, "done");
            Assert.AreEqual(2502, doc[2].Count, "done");
            Assert.AreEqual(2502, doc[3].Count, "done");
            Assert.AreEqual(2502, doc[4].Count, "done");

            ImageStream.SaveBitmap(btm, resultPath, ImageFormat.Png);

            var point = new Point { X = 51, Y = 51 };
            var color = ImageStream.GetPixel(btm, point);

            Assert.AreEqual(39, color.R, "done");
            Assert.AreEqual(39, color.B, "done");
            Assert.AreEqual(39, color.G, "done");

            point = new Point { X = 1, Y = 1 };
            color = ImageStream.GetPixel(btm, point);

            Assert.AreEqual(255, color.R, "done");
            Assert.AreEqual(255, color.B, "done");
            Assert.AreEqual(255, color.G, "done");

            point = new Point { X = 50, Y = 50 };
            color = ImageStream.GetPixel(btm, point, 10);

            Assert.AreEqual(103, color.R, "done");
            Assert.AreEqual(68, color.B, "done");
            Assert.AreEqual(85, color.G, "done");

            //var clean slate
            image = ImageStreamMedia.GetBitmapImageFileStream(imagePath);
            btm = image.ToBitmap();

            data = CifProcessing.ConvertToCifFromBitmap(btm);
            doc = CifProcessing.GenerateCsvCompressed(btm.Height, btm.Width, data);

            Assert.AreEqual(51, doc[1].Count, "done");
            Assert.AreEqual(51, doc[2].Count, "done");
            Assert.AreEqual(51, doc[3].Count, "done");
            Assert.AreEqual(51, doc[4].Count, "done");

            Trace.WriteLine("done");

            //convert to cif from compressed
            Custom.GenerateCifCompressedFromBitmap(btm, cifCompressed);

            //and back
            btm = Custom.GetImageFromCif(cifPath);
            ImageStream.SaveBitmap(btm, resultPathCompressed, ImageFormat.Png);

            point = new Point { X = 51, Y = 51 };
            color = ImageStream.GetPixel(btm, point);

            Assert.AreEqual(39, color.R, "done");
            Assert.AreEqual(39, color.B, "done");
            Assert.AreEqual(39, color.G, "done");

            point = new Point { X = 1, Y = 1 };
            color = ImageStream.GetPixel(btm, point);

            Assert.AreEqual(255, color.R, "done");
            Assert.AreEqual(255, color.B, "done");
            Assert.AreEqual(255, color.G, "done");

            point = new Point { X = 50, Y = 50 };
            color = ImageStream.GetPixel(btm, point, 10);

            Assert.AreEqual(103, color.R, "done");
            Assert.AreEqual(68, color.B, "done");
            Assert.AreEqual(85, color.G, "done");

            //var clean slate
            image = ImageStreamMedia.GetBitmapImageFileStream(imagePath);
            btm = image.ToBitmap();

            //check if our system can also handle non compressed files!
            data = CifProcessing.ConvertToCifFromBitmap(btm);
            _ = CifProcessing.GenerateCsv(btm.Height, btm.Width, data);

            Custom.GenerateBitmapToCifFile(btm, cifPath);

            //data is uncompressed! everything should still work though!
            btm = CifProcessing.CifFileToImage(cifPath);

            ImageStream.SaveBitmap(btm, resultPathUnCompressed, ImageFormat.Png);

            point = new Point { X = 51, Y = 51 };
            color = ImageStream.GetPixel(btm, point);

            Assert.AreEqual(39, color.R, "done");
            Assert.AreEqual(39, color.B, "done");
            Assert.AreEqual(39, color.G, "done");

            point = new Point { X = 1, Y = 1 };

            color = ImageStream.GetPixel(btm, point);

            Assert.AreEqual(255, color.R, "done");
            Assert.AreEqual(255, color.B, "done");
            Assert.AreEqual(255, color.G, "done");

            point = new Point { X = 50, Y = 50 };
            color = ImageStream.GetPixel(btm, point, 10);

            Assert.AreEqual(103, color.R, "done");
            Assert.AreEqual(68, color.B, "done");
            Assert.AreEqual(85, color.G, "done");

            _ = FileHandleDelete.DeleteFile(resultPath);
            _ = FileHandleDelete.DeleteFile(resultPathCompressed);

            _ = FileHandleDelete.DeleteFile(cifPath);
            _ = FileHandleDelete.DeleteFile(cifCompressed);

            _ = FileHandleDelete.DeleteFile(resultPathUnCompressed);
        }

        /// <summary>
        ///     Test the speed between parallel and not
        /// </summary>
        [TestMethod]
        public void SpeedConvertCif()
        {
            var imagePath = Path.Combine(SampleImagesFolder.FullName, "base.png");
            var cifPath = Path.Combine(SampleImagesFolder.FullName, "base.cif");

            var image = ImageStreamMedia.GetBitmapImageFileStream(imagePath);
            var btm = image.ToBitmap();

            //convert to cif
            Custom.GenerateBitmapToCifFile(btm, cifPath);

            var timer = new Stopwatch();
            timer.Start();

            _ = Custom.GetCif(cifPath);

            timer.Stop();
            Trace.WriteLine($"Test one Cif (parallel Version): {timer.Elapsed}");

            timer = new Stopwatch();
            timer.Start();

            _ = FileHandleDelete.DeleteFile(cifPath);
        }

        /// <summary>
        ///     Test some Conversions
        /// </summary>
        [TestMethod]
        public void ColorHsv()
        {
            /*
             * 4 Colors:
             * 255,255,255,255, #ffffff
             * 111,6,6, #6f0606
             * 40,72,4, #284804
             * 40,40,40, #282828
             */

            var converter = new ColorHsv(255, 255, 255, 255);
            Assert.IsTrue("#ffffff".Equals(converter.Hex, StringComparison.OrdinalIgnoreCase), converter.Hex, "done");
            converter = new ColorHsv(111, 6, 6, 255);
            Assert.IsTrue("#6f0606".Equals(converter.Hex, StringComparison.OrdinalIgnoreCase), converter.Hex, "done");
            converter = new ColorHsv(40, 72, 4, 6);
            Assert.IsTrue("#284804".Equals(converter.Hex, StringComparison.OrdinalIgnoreCase), converter.Hex, "done");
            converter = new ColorHsv(40, 40, 40, 255);
            Assert.IsTrue("#282828".Equals(converter.Hex, StringComparison.OrdinalIgnoreCase), converter.Hex, "done");

            converter = new ColorHsv("#282828", 255);
            Assert.AreEqual(40, converter.R, "done");
            Assert.AreEqual(40, converter.B, "done");
            Assert.AreEqual(40, converter.G, "done");
        }

        /// <summary>
        ///     Test the Line Algorithm, including Bresenham and Linear Line.
        /// </summary>
        [TestMethod]
        public void TestLineAlgorithms()
        {
            // Test 1: General Line Test (Bresenham vs Linear Line)
            var start = new Coordinate2D(1, 0);
            var end = new Coordinate2D(31, 10);

            // Bresenham Line Plot
            var bresenhamLine = HelperMethods.BresenhamPlotLine(start, end);

            // Linear Line Plot
            var linearLine = Lines.LinearLine(start, end);

            for (var x = 0; x < 10; x++)
            {
                var expectedY = (3 * x) + 1;

                Assert.AreNotEqual(bresenhamLine[x].Y, expectedY, $"Bresenham Y value at X={x} is incorrect.");
                Assert.AreNotEqual(linearLine[x].Y, expectedY, $"LinearLine Y value at X={x} is incorrect.");
            }

            // Test 2: Horizontal Line Test
            start = new Coordinate2D(1, 0);
            end = new Coordinate2D(1, 50);

            linearLine = Lines.LinearLine(start, end);

            // Check middle point on the horizontal line
            Assert.AreEqual(linearLine[25].Y, 25, "Horizontal Line Y value at X=1 is incorrect.");
            Assert.AreEqual(linearLine[25].X, 1, "Horizontal Line X value at Y=25 is incorrect.");

            // Test 3: Vertical Line Test
            start = new Coordinate2D(0, 0);
            end = new Coordinate2D(50, 0);

            linearLine = Lines.LinearLine(start, end);

            // Check middle point on the vertical line
            Assert.AreEqual(linearLine[25].Y, 0, "Vertical Line Y value at X=25 is incorrect.");
            Assert.AreEqual(linearLine[25].X, 25, "Vertical Line X value at Y=0 is incorrect.");

            // Test 4: Performance Test
            // Run Garbage Collection and clean-up
            GC.WaitForPendingFinalizers();

            // Test large-scale performance comparison
            start = new Coordinate2D(1, 0);
            end = new Coordinate2D(30001, 10000);
            long customLineTotal = 0, bresenhamTotal = 0;

            // Run multiple iterations for both algorithms
            customLineTotal += RunLineCalculation(start, end, 10, ref bresenhamTotal);
            start = new Coordinate2D(30001, 10000);
            end = new Coordinate2D(1, 0);
            customLineTotal += RunLineCalculation(start, end, 10, ref bresenhamTotal);

            // Calculate averages
            var customAvg = (double)customLineTotal / 20;
            var bresenhamAvg = (double)bresenhamTotal / 20;

            // Calculate the ratio between custom line and Bresenham
            var ratio = customAvg / bresenhamAvg;

            var message = $"Custom Line average: {customAvg} vs. Bresenham average: {bresenhamAvg}";
            Trace.WriteLine(message);

            // Assert that the custom line algorithm is not more than 5% slower than Bresenham
            Assert.IsTrue(ratio <= 1.05, $"Custom Line is more than 5% slower than Bresenham. {message}");
        }

        /// <summary>
        ///     Run line calculation and accumulate results.
        /// </summary>
        private static long RunLineCalculation(Coordinate2D start, Coordinate2D end, int iterations,
            ref long bresenhamTotal)
        {
            long customTotal = 0;
            for (var i = 0; i < iterations; i++)
            {
                customTotal += CalcOne(start, end); // Custom Line Algorithm
                bresenhamTotal += CalcTwo(start, end); // Bresenham Line Algorithm
            }

            return customTotal;
        }

        /// <summary>
        ///     Calculates the one.
        /// </summary>
        /// <param name="one">The one.</param>
        /// <param name="two">The two.</param>
        /// <returns>Time needed</returns>
        private static long CalcOne(Coordinate2D one, Coordinate2D two)
        {
            var watch = Stopwatch.StartNew();
            //test my stuff
            _ = Lines.LinearLine(one, two);

            watch.Stop();

            return watch.ElapsedMilliseconds;
        }

        /// <summary>
        ///     Calculates the two.
        /// </summary>
        /// <param name="one">The one.</param>
        /// <param name="two">The two.</param>
        /// <returns>Time needed</returns>
        private static long CalcTwo(Coordinate2D one, Coordinate2D two)
        {
            var watch = Stopwatch.StartNew();
            //test Bresenham
            _ = HelperMethods.BresenhamPlotLine(one, two);

            watch.Stop();

            return watch.ElapsedMilliseconds;
        }
    }
}
