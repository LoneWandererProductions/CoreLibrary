/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImagingTests
 * FILE:        DirectBitmapCoreTests.cs
 * PURPOSE:     Test for blending in DirectBitmapCore.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using Imaging;

namespace ImagingTests
{
    [TestClass]
    public unsafe class DirectBitmapCoreTests
    {
        /// <summary>
        /// Blends the int null DST throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BlendInt_NullDst_Throws()
        {
            Pixel32[] dst = null!;
            var src = new uint[1];
            DirectBitmapCore.BlendInt(dst, src);
        }

        /// <summary>
        /// Blends the int null source throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BlendInt_NullSrc_Throws()
        {
            var dst = new Pixel32[1];
            uint[] src = null!;
            DirectBitmapCore.BlendInt(dst, src);
        }

        /// <summary>
        /// Blends the int length mismatch throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BlendInt_LengthMismatch_Throws()
        {
            var dst = new Pixel32[2];
            var src = new uint[1];
            DirectBitmapCore.BlendInt(dst, src);
        }

        /// <summary>
        /// Blends the int fully opaque sets directly.
        /// </summary>
        [TestMethod]
        public void BlendInt_FullyOpaque_SetsDirectly()
        {
            var dst = new Pixel32[1];
            var src = new uint[] { 0xFF112233 }; // ARGB = FF 11 22 33

            DirectBitmapCore.BlendInt(dst, src);

            Assert.AreEqual(0x11, dst[0].R);
            Assert.AreEqual(0x22, dst[0].G);
            Assert.AreEqual(0x33, dst[0].B);
            Assert.AreEqual(0xFF, dst[0].A);
        }

        /// <summary>
        /// Blends the int fully transparent no change.
        /// </summary>
        [TestMethod]
        public void BlendInt_FullyTransparent_NoChange()
        {
            var dst = new Pixel32[] { new Pixel32(10, 20, 30, 40) };
            var src = new uint[] { 0x00000000 }; // fully transparent

            DirectBitmapCore.BlendInt(dst, src);

            Assert.AreEqual(10, dst[0].R);
            Assert.AreEqual(20, dst[0].G);
            Assert.AreEqual(30, dst[0].B);
            Assert.AreEqual(40, dst[0].A);
        }

        /// <summary>
        /// Blends the int partial alpha blends correctly.
        /// </summary>
        [TestMethod]
        public void BlendInt_PartialAlpha_BlendsCorrectly()
        {
            // dst = semi-random
            var dst = new Pixel32[] { new Pixel32(100, 50, 0, 128) };
            // src = semi-transparent red (50% alpha)
            var src = new uint[] { 0x80FF0000 }; // ARGB 128,255,0,0

            DirectBitmapCore.BlendInt(dst, src);

            // Manual computation:
            // sa = 128, sr = 255, sg = 0, sb = 0
            // da = 128, dr = 100, dg = 50, db = 0
            // invA = 127
            // r = (255*128 + 100*127)/255 = (32640 + 12700)/255 = 45340/255 ≈ 178
            // g = (0*128 + 50*127)/255 = 6350/255 ≈ 24
            // b = (0*128 + 0*127)/255 = 0
            // a = 128 + ((128*127)/255) = 128 + 63 ≈ 191

            Assert.IsTrue(Math.Abs(dst[0].R - 178) <= 1);
            Assert.IsTrue(Math.Abs(dst[0].G - 25) <= 1);
            Assert.IsTrue(Math.Abs(dst[0].B - 0) <= 1);
            Assert.IsTrue(Math.Abs(dst[0].A - 191) <= 1);
        }
    }
}
