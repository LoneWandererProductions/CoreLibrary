/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Tests
 * FILE:        CifTests.cs
 * PURPOSE:     Tests for my image Format, cif.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using FileHandler;
using Imaging.Cifs;
using Imaging.Helpers;

namespace Imaging.Tests
{
    /// <summary>
    ///     Test some image related stuff
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class CifTests
    {
        /// <summary>
        ///     The executable folder
        /// </summary>
        private static readonly string ExecutionFolder = AppContext.BaseDirectory;

        /// <summary>
        ///     The project folder
        /// </summary>
        private static readonly DirectoryInfo ProjectFolder =
            new(Directory.GetParent(ExecutionFolder)?.FullName ?? string.Empty);

        /// <summary>
        ///     The sample images folder in the output directory
        /// </summary>
        private static readonly DirectoryInfo SampleImagesFolder = new(Path.Combine(ExecutionFolder, "Images"));

        /// <summary>
        ///     The custom Image Format
        /// </summary>
        private static readonly CustomImageFormat Custom = new();

        #region Isolated Unit Tests

        /// <summary>
        ///     Tests Constructor with null image throwing ArgumentNullException.
        /// </summary>
        [TestMethod]
        public void Constructor_NullImage_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Cif(image: null));
        }

        /// <summary>
        ///     Tests Constructor with null interface throwing ArgumentNullException.
        /// </summary>
        [TestMethod]
        public void Constructor_NullInterfaceWithPath_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new Cif("test.cif", imageFormat: null));
        }

        /// <summary>
        ///     Tests PixelCount property calculation.
        /// </summary>
        [TestMethod]
        public void PixelCount_CalculatesHeightTimesWidth()
        {
            using var bitmap = new Bitmap(10, 20);
            var cif = new Cif(bitmap);

            Assert.AreEqual(10, cif.Width);
            Assert.AreEqual(20, cif.Height);
            Assert.AreEqual(200, cif.PixelCount);
        }

        /// <summary>
        ///     Tests GetColor with valid ID returning expected color.
        /// </summary>
        [TestMethod]
        public void GetColor_ValidId_ReturnsCorrectColor()
        {
            using var bitmap = new Bitmap(2, 2);
            bitmap.SetPixel(0, 0, Color.Red);
            bitmap.SetPixel(1, 0, Color.Blue);

            var cif = new Cif(bitmap);

            Assert.AreEqual(Color.FromArgb(Color.Red.ToArgb()), cif.GetColor(0));
            Assert.AreEqual(Color.FromArgb(Color.Blue.ToArgb()), cif.GetColor(1));
        }

        /// <summary>
        ///     Tests GetColor out of bounds throwing ArgumentOutOfRangeException.
        /// </summary>
        [TestMethod]
        public void GetColor_OutOfBoundsId_ThrowsArgumentOutOfRangeException()
        {
            using var bitmap = new Bitmap(2, 2);
            var cif = new Cif(bitmap);

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => cif.GetColor(-1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => cif.GetColor(4));
        }

        /// <summary>
        ///     Tests ChangeColor by coordinates successfully updating pixel color.
        /// </summary>
        [TestMethod]
        public void ChangeColor_ByCoordinates_SuccessfullyUpdatesPixel()
        {
            using var bitmap = new Bitmap(2, 2);
            for (var x = 0; x < 2; x++)
                for (var y = 0; y < 2; y++)
                    bitmap.SetPixel(x, y, Color.Red);

            var cif = new Cif(bitmap);

            var success = cif.ChangeColor(0, 0, Color.Blue);

            Assert.IsTrue(success);
            // Compare ARGB values to avoid named vs unnamed System.Drawing.Color equality mismatches
            Assert.AreEqual(Color.Blue.ToArgb(), cif.GetColor(0).ToArgb());
        }

        /// <summary>
        ///     Tests ChangeColor with invalid coordinates returning false.
        /// </summary>
        [TestMethod]
        public void ChangeColor_InvalidCoordinates_ReturnsFalse()
        {
            using var bitmap = new Bitmap(2, 2);
            var cif = new Cif(bitmap);

            var result = cif.ChangeColor(5, 5, Color.Green);

            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Tests ChangeColor replacing entire color key in dictionary.
        /// </summary>
        [TestMethod]
        public void ChangeColor_ReplaceEntireColor_UpdatesDictionary()
        {
            using var bitmap = new Bitmap(2, 2);
            bitmap.SetPixel(0, 0, Color.Red);
            bitmap.SetPixel(1, 0, Color.Red);

            var cif = new Cif(bitmap);

            // DirectBitmap creates unnamed ARGB keys; convert named colors to ARGB to match Dictionary keys
            var redArgb = Color.FromArgb(Color.Red.ToArgb());
            var greenArgb = Color.FromArgb(Color.Green.ToArgb());

            var result = cif.ChangeColor(redArgb, greenArgb);

            Assert.IsTrue(result);
            Assert.IsFalse(cif.CifImage.ContainsKey(redArgb));
            Assert.IsTrue(cif.CifImage.ContainsKey(greenArgb));
        }

        #endregion

        #region Integration & File Tests

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
            if (image == null) Assert.Fail("No image loaded.");
            btm = image.ToBitmap();

            //check if our system can also handle non compressed files!
            data = CifProcessing.ConvertToCifFromBitmap(btm);
            if (data == null) Assert.Fail("Data was not converted.");
            if (btm == null) Assert.Fail("No image loaded.");

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
            if (image == null) Assert.Fail("No image loaded.");

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

        #endregion
    }
}
