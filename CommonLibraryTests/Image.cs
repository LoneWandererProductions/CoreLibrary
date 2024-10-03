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

/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/Image.cs
 * PURPOSE:     Tests for Image Tools
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

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
                FileHandleDelete.DeleteAllContents(OutputFolder.FullName, false);
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
        ///     Speed compare.
        ///     We use TestCleanup to execute it at the end, so no other test execution interferes
        /// </summary>
        [TestMethod]
        public void SpeedCompare()
        {
            var bmp = new Bitmap(1000, 1000);
            var blackPen = new Pen(Color.Black, 1);

            const int x1 = 0;
            const int y1 = 0;
            const int x2 = 0;
            const int y2 = 1000;

            var watch = Stopwatch.StartNew();
            // the code that you want to measure comes here

            for (var i = 0; i < 1000; i++)
            {
                // Draw line to screen.
                using var graphics = Graphics.FromImage(bmp);
                graphics.DrawLine(blackPen, x1, y1, x2, y2);
            }

            watch.Stop();
            //microsoft way
            var elapsedOne = watch.ElapsedMilliseconds;

            var dbm = DirectBitmap.GetInstance(bmp);

            watch = Stopwatch.StartNew();

            for (var i = 0; i < 1000; i++)
            {
                // Draw line to screen.
                dbm.DrawVerticalLine(x1, y1, 1000, Color.Black);
            }

            watch.Stop();
            var elapsedTwo = watch.ElapsedMilliseconds;

            Trace.WriteLine($"Second DrawVerticalLine: {elapsedTwo} First DrawLine: {elapsedOne}");

            //for now it depends... sometimes it is slower sometimes it is faster ...
            Trace.WriteLine(elapsedOne < elapsedTwo, "Was faster, I made an breakthrough. ");

            watch = Stopwatch.StartNew();

            for (var i = 0; i < 1000; i++)
            {
                // Draw line to screen.
                dbm.DrawRectangle(x1, y1, 1, 1000, Color.Black);
            }

            watch.Stop();

            // the code that you want to measure comes here

            for (var i = 0; i < 1000; i++)
            {
                // Draw line to screen.
                using var graphics = Graphics.FromImage(bmp);
                graphics.DrawRectangle(blackPen, x1, y1, 100, 200);
            }

            // Warm up both methods to avoid JIT overhead
            for (var i = 0; i < 10; i++)
            {
                dbm.DrawRectangle(x1, y1, 100, 200, Color.Black);
            }

            // Variables to track total elapsed time for each method
            long totalElapsedMicrosoft = 0;
            long totalElapsedDbm = 0;
            const int iterations = 100; // Number of iterations to average results

            for (var i = 0; i < iterations; i++)
            {
                // Measure time for the first method (Microsoft's generic way)
                watch = Stopwatch.StartNew();

                // Perform operation here (replace with your Microsoft rectangle draw code)
                dbm.DrawRectangle(x1, y1, 100, 200, Color.Black);

                watch.Stop();
                totalElapsedMicrosoft += watch.ElapsedMilliseconds; // Or use ElapsedTicks for higher precision

                // Measure time for the second method (DirectBitmap custom method)
                watch = Stopwatch.StartNew();

                // Perform custom drawing method
                dbm.DrawRectangle(x1, y1, 100, 200, Color.Black);

                watch.Stop();
                totalElapsedDbm += watch.ElapsedMilliseconds; // Or use ElapsedTicks
            }

            // Calculate average times for both methods
            var averageMicrosoft = totalElapsedMicrosoft / iterations;
            var averageDbm = totalElapsedDbm / iterations;

            // Log the results
            Trace.WriteLine(string.Concat("Average Rectangle Microsoft: ", averageMicrosoft,
                " ms, Average DrawRectangle dbm: ", averageDbm, " ms"));

            // Perform the assertion based on average values
            Assert.IsTrue(averageMicrosoft <= averageDbm,
                $"Results: {averageMicrosoft} ms (Microsoft) vs {averageDbm} ms (DirectBitmap)");
        }

        /// <summary>
        ///     Compares the colors.
        /// </summary>
        [TestMethod]
        public void CompareImageColors()
        {
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
        /// Compares the duplicate images.
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
        /// Compares the similar images.
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
        /// Checks the image content details.
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
        /// Checks the image list details.
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

            FileHandleDelete.DeleteFile(resultPath);
            FileHandleDelete.DeleteFile(resultPathCompressed);

            FileHandleDelete.DeleteFile(cifPath);
            FileHandleDelete.DeleteFile(cifCompressed);

            FileHandleDelete.DeleteFile(resultPathUnCompressed);

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
            CifProcessing.GenerateCsv(btm.Height, btm.Width, data);

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

            FileHandleDelete.DeleteFile(resultPath);
            FileHandleDelete.DeleteFile(resultPathCompressed);

            FileHandleDelete.DeleteFile(cifPath);
            FileHandleDelete.DeleteFile(cifCompressed);

            FileHandleDelete.DeleteFile(resultPathUnCompressed);
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

            Custom.GetCif(cifPath);

            timer.Stop();
            Trace.WriteLine($"Test one Cif (parallel Version): {timer.Elapsed}");

            timer = new Stopwatch();
            timer.Start();

            FileHandleDelete.DeleteFile(cifPath);
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
        ///     Test our Line Algorithm.
        /// </summary>
        [TestMethod]
        public void Lines()
        {
            var one = new Coordinate2D(1, 0);
            var two = new Coordinate2D(31, 10);

            // Test Bresenham
            var lstOne = SpeedTests.BresenhamPlotLine(one, two);
            var lstTwo = Mathematics.Lines.LinearLine(one, two);

            for (var x = 0; x < 10; x++)
            {
                var y = (3 * x) + 1;

                Assert.AreNotEqual(lstOne[x].Y, y, "Bresenham Y value is incorrect.");
                Assert.AreNotEqual(lstTwo[x].Y, y, "LinearLine Y value is incorrect.");
            }

            // Horizontal Line Test
            one = new Coordinate2D(1, 0);
            two = new Coordinate2D(1, 50);

            lstTwo = Mathematics.Lines.LinearLine(one, two);

            Assert.AreEqual(lstTwo[25].Y, 25, "Horizontal Line Y value is incorrect.");
            Assert.AreEqual(lstTwo[25].X, 1, "Horizontal Line X value is incorrect.");

            // Vertical Line Test
            one = new Coordinate2D(0, 0);
            two = new Coordinate2D(50, 0);

            lstTwo = Mathematics.Lines.LinearLine(one, two);

            Assert.AreEqual(lstTwo[25].Y, 0, "Vertical Line Y value is incorrect.");
            Assert.AreEqual(lstTwo[25].X, 25, "Vertical Line X value is incorrect.");

            // Speed Test
            // Garbage Collector
            GC.WaitForPendingFinalizers();
            // Remove processor affinity setting for consistency in CI environment
            // Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);

            one = new Coordinate2D(1, 0);
            two = new Coordinate2D(30001, 10000);

            long mineTotal = 0, bresenhamTotal = 0;

            for (var i = 0; i < 10; i++)
            {
                mineTotal += CalcOne(one, two);
                bresenhamTotal += CalcTwo(one, two);
            }

            one = new Coordinate2D(30001, 10000);
            two = new Coordinate2D(1, 0);

            for (var i = 0; i < 10; i++)
            {
                mineTotal += CalcOne(one, two);
                bresenhamTotal += CalcTwo(one, two);
            }

            var mineAvg = (double)mineTotal / 20;
            var bresenhamAvg = (double)bresenhamTotal / 20;
            var ratio = mineAvg / bresenhamAvg;

            var message = $"Mine average: {mineAvg} vs. Bresenham average: {bresenhamAvg}";
            Trace.WriteLine(message);

            // Check that the performance ratio is within an acceptable range (e.g., 5% slower)
            Assert.IsTrue(ratio <= 1.05, $"Mine is more than 5% slower than Bresenham. {message}");
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
            _ = Mathematics.Lines.LinearLine(one, two);

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
            _ = SpeedTests.BresenhamPlotLine(one, two);

            watch.Stop();

            return watch.ElapsedMilliseconds;
        }
    }
}
