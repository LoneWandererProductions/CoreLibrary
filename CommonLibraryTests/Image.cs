﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using FileHandler;
using ImageCompare;
using Imaging;
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
        public void BitLock()
        {
            var imagePath = Path.Combine(SampleImagesFolder.FullName, "base.png");

            var image = ImageStream.GetBitmapImageFileStream(imagePath);
            var btm = image.ToBitmap();

            Assert.AreNotEqual(null, image, "done");

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

            dbm.Dispose();
        }

        /// <summary>
        ///     Test the custom DirectBitmap and how it works
        /// </summary>
        [TestMethod]
        public void CutImage()
        {
            var imagePath = Path.Combine(SampleImagesFolder.FullName, "base.png");

            var image = ImageStream.GetBitmapImageFileStream(imagePath);
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
        ///     Tests getting and setting pixel(s).
        /// </summary>
        [TestMethod]
        public void GetSetPixel()
        {
            var imagePath = Path.Combine(SampleImagesFolder.FullName, "base.png");

            var image = ImageStream.GetBitmapImageFileStream(imagePath);
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

            var x1 = 0;
            var y1 = 0;
            var x2 = 0;
            var y2 = 1000;

            var watch = Stopwatch.StartNew();
            // the code that you want to measure comes here

            for (var i = 0; i < 100; i++)
            {
                // Draw line to screen.
                using var graphics = Graphics.FromImage(bmp);
                graphics.DrawLine(blackPen, x1, y1, x2, y2);
            }

            watch.Stop();

            var elapsedOne = watch.ElapsedMilliseconds;

            Trace.WriteLine(string.Concat("First, DrawLine: ", elapsedOne));

            var dbm = DirectBitmap.GetInstance(bmp);

            watch = Stopwatch.StartNew();

            for (var i = 0; i < 100; i++)
            {
                // Draw line to screen.
                dbm.DrawVerticalLine(x1, y1, 1000, Color.Black);
            }

            watch.Stop();

            var elapsedTwo = watch.ElapsedMilliseconds;

            Trace.WriteLine(string.Concat("Second DirectBitmap: ", elapsedTwo));

            Trace.WriteLine(string.Concat("Second DirectBitmap: ", elapsedTwo, " DrawLine: ", elapsedOne));
            //Assert.IsTrue(elapsedOne > elapsedTwo, "Was faster");


            watch = Stopwatch.StartNew();

            for (var i = 0; i < 100; i++)
            {
                // Draw line to screen.
                dbm.DrawRectangle(x1, y1, 1, 1000, Color.Black);
            }

            watch.Stop();

            var elapsed = watch.ElapsedMilliseconds;

            Trace.WriteLine(string.Concat("Rectangle draw Line: ", elapsed));


            watch = Stopwatch.StartNew();
            // the code that you want to measure comes here

            for (var i = 0; i < 100; i++)
            {
                // Draw line to screen.
                using var graphics = Graphics.FromImage(bmp);
                graphics.DrawRectangle(blackPen, x1, y1, 100, 200);
            }

            watch.Stop();

            var elapsedThree = watch.ElapsedMilliseconds;

            Trace.WriteLine(string.Concat("Three Rectangle: ", elapsedThree));

            dbm = DirectBitmap.GetInstance(bmp);

            watch = Stopwatch.StartNew();

            for (var i = 0; i < 100; i++)
            {
                // Draw line to screen.
                dbm.DrawRectangle(x1, y1, 100, 200, Color.Black);
            }

            watch.Stop();

            var elapsedFour = watch.ElapsedMilliseconds;

            Trace.WriteLine(string.Concat("Four DirectBitmap: ", elapsedFour));

            Assert.Inconclusive(string.Concat("Results: ", elapsedThree, "Rectangle Microsoft: ", elapsedFour));
        }

        /// <summary>
        ///     Compares the colors.
        /// </summary>
        [TestMethod]
        public void CompareColors()
        {
            var imagePath = Path.Combine(SampleImagesFolder.FullName, "Color.png");

            using var btm = new Bitmap(imagePath);

            var imageData = ImageProcessing.GenerateData(btm, 0);

            var color = Analysis.FindImagesInColorRange(imageData.R, imageData.G, imageData.B, 4,
                SampleImagesFolder.FullName, false, ResourcesGeneral.Appendix);

            Assert.AreEqual(2, color.Count, "Done");

            Assert.AreEqual(imagePath, color[0], "Done");
            imagePath = Path.Combine(SampleImagesFolder.FullName, "ColorShade.png");
            Assert.AreEqual(imagePath, color[1], "Done");

            //compare and Similar Images

            var images = Compare.GetDuplicateImages(SampleImagesFolder.FullName, false, ResourcesGeneral.Appendix);

            Assert.AreEqual(1, images.Count, "Done");
            Assert.AreEqual(2, images[0].Count, "Done");

            imagePath = Path.Combine(SampleImagesFolder.FullName, "Compare.png");
            var cache = images[0];
            Assert.AreEqual(imagePath, cache[0], "Done");
            imagePath = Path.Combine(SampleImagesFolder.FullName, "CompareCopy.png");
            Assert.AreEqual(imagePath, cache[1], "Done");

            images = Compare.GetSimilarImages(SampleImagesFolder.FullName, false, ResourcesGeneral.Appendix, 80);

            Assert.AreEqual(1, images.Count, "Done");
            Assert.AreEqual(3, images[0].Count, "Done");

            imagePath = Path.Combine(SampleImagesFolder.FullName, "Compare.png");
            cache = images[0];
            Assert.IsTrue(cache.Contains(imagePath), "Done");
            imagePath = Path.Combine(SampleImagesFolder.FullName, "CompareCopy.png");
            Assert.IsTrue(cache.Contains(imagePath), "Done");
            imagePath = Path.Combine(SampleImagesFolder.FullName, "CompareSimilar.png");
            Assert.IsTrue(cache.Contains(imagePath), "Done");

            //Check Content
            var data = Analysis.GetImageDetails(imagePath);

            Assert.AreEqual(26, data.R, "Done");

            Assert.AreEqual(72, data.G, "Done");

            Assert.AreEqual(124, data.B, "Done");

            Assert.AreEqual(3075, data.Size, "Done");

            Assert.AreEqual(100, data.Height, "Done");

            Assert.AreEqual(100, data.Width, "Done");

            Assert.AreEqual(".png", data.Extension, "Done");

            var dataList = Analysis.GetImageDetails(images[0]);

            Assert.AreEqual(26, dataList[0].R, "Done");

            Assert.AreEqual(72, dataList[0].G, "Done");

            Assert.AreEqual(124, dataList[0].B, "Done");

            Assert.AreEqual(3075, data.Size, "Done");

            //should be around 99%
            Assert.AreEqual(100, Math.Round(dataList[2].Similarity, 0),
                string.Concat("Done: ", dataList[2].Similarity));

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

            var image = ImageStream.GetBitmapImageFileStream(imagePath);
            var btm = image.ToBitmap();

            /*
             * 4 Colors:
             * 255,255,255,255, #ffffff
             * 111,6,6, 6f0606
             * 40,72,4, #284804
             * 40,40,40, #282828
             */

            //convert to cif
            Custom.SaveToCifFile(btm, cifPath);
            //and back
            btm = Custom.GetCifFile(cifPath);

            var data = CifProcessing.ConvertToCif(btm);
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
            image = ImageStream.GetBitmapImageFileStream(imagePath);
            btm = image.ToBitmap();

            data = CifProcessing.ConvertToCif(btm);
            doc = CifProcessing.GenerateCsvCompressed(btm.Height, btm.Width, data);

            Assert.AreEqual(51, doc[1].Count, "done");
            Assert.AreEqual(51, doc[2].Count, "done");
            Assert.AreEqual(51, doc[3].Count, "done");
            Assert.AreEqual(51, doc[4].Count, "done");

            Trace.WriteLine("done");

            //convert to cif from compressed
            Custom.CompressedToCifFile(btm, cifCompressed);

            //and back
            btm = Custom.GetCifFile(cifPath);
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
            image = ImageStream.GetBitmapImageFileStream(imagePath);
            btm = image.ToBitmap();

            //check if our system can also handle non compressed files!
            data = CifProcessing.ConvertToCif(btm);
            doc = CifProcessing.GenerateCsv(btm.Height, btm.Width, data);

            //data is uncompressed! everything should still work though!
            btm = CifProcessing.CifToImageCompressed(doc);

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
    }
}
