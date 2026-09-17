/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Tests
 * FILE:        CustomImageFormatTests.cs
 * PURPOSE:     Tests for the Cif Object.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Drawing;
using Imaging.Cifs;

namespace Imaging.Tests
{
    /// <summary>
    ///     Tests for CustomImageFormat class implementation.
    /// </summary>
    [TestClass]
    public class CustomImageFormatTests
    {
        /// <summary>
        /// The format
        /// </summary>
        private CustomImageFormat _format;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _format = new CustomImageFormat();
        }

        /// <summary>
        ///     Tests GenerateCifFromBitmap returning populated Cif object.
        /// </summary>
        [TestMethod]
        public void GenerateCifFromBitmap_ValidBitmap_ReturnsPopulatedCif()
        {
            using var bitmap = new Bitmap(2, 2);
            bitmap.SetPixel(0, 0, Color.Red);
            bitmap.SetPixel(1, 0, Color.Red);
            bitmap.SetPixel(0, 1, Color.Blue);
            bitmap.SetPixel(1, 1, Color.Blue);

            var cif = _format.GenerateCifFromBitmap(bitmap);

            Assert.IsNotNull(cif);
            Assert.AreEqual(2, cif.Width);
            Assert.AreEqual(2, cif.Height);
            Assert.IsFalse(cif.Compressed);
            Assert.AreEqual(2, cif.NumberOfColors);
        }
    }
}
