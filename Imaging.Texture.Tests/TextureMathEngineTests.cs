/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Texture
 * FILE:        Imaging.Texture.Tests.cs
 * PURPOSE:     Mostly visual tests for the texture generation methods in the TextureMathEngine class.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace Imaging.Texture.Tests
{
    /// <summary>
    /// Texture testing class for visual verification of texture generation methods.
    /// </summary>
    [TestClass]
    public class TextureMathEngineTests
    {
        /// <summary>
        /// The test width
        /// </summary>
        private const int TestWidth = 256;

        /// <summary>
        /// The test height
        /// </summary>
        private const int TestHeight = 256;

        /// <summary>
        /// The output directory
        /// </summary>
        private string _outputDirectory;

        /// <summary>
        /// The noise generator
        /// </summary>
        private NoiseGenerator _noiseGenerator;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TextureVisualTests");
            if (!Directory.Exists(_outputDirectory))
            {
                Directory.CreateDirectory(_outputDirectory);
            }

            // Initialize your actual noise map based on the test dimensions
            _noiseGenerator = new NoiseGenerator(TestWidth, TestHeight);
            Console.WriteLine($"Textures will be saved to: {_outputDirectory}");
        }

        /// <summary>
        /// Generates the noise visual test.
        /// </summary>
        [TestMethod]
        public void GenerateNoise_VisualTest()
        {
            var buffer = TextureMathEngine.GenerateNoise(TestWidth, TestHeight, _noiseGenerator, useTurbulence: true);
            SaveBufferToImage(buffer, "01_Noise.png");
        }

        /// <summary>
        /// Generates the wood visual test.
        /// </summary>
        [TestMethod]
        public void GenerateWood_VisualTest()
        {
            var buffer = TextureMathEngine.GenerateWood(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "02_Wood.png");
        }

        /// <summary>
        /// Generates the crosshatch visual test.
        /// </summary>
        [TestMethod]
        public void GenerateCrosshatch_VisualTest()
        {
            var buffer =
                TextureMathEngine.GenerateCrosshatch(TestWidth, TestHeight, bgR: 255, bgG: 255, bgB: 255, bgA: 255);
            SaveBufferToImage(buffer, "03_Crosshatch.png");
        }

        /// <summary>
        /// Generates the concrete visual test.
        /// </summary>
        [TestMethod]
        public void GenerateConcrete_VisualTest()
        {
            var buffer = TextureMathEngine.GenerateConcrete(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "04_Concrete.png");
        }

        /// <summary>
        /// Generates the canvas visual test.
        /// </summary>
        [TestMethod]
        public void GenerateCanvas_VisualTest()
        {
            var buffer =
                TextureMathEngine.GenerateCanvas(TestWidth, TestHeight, bgR: 255, bgG: 255, bgB: 255, bgA: 255);
            SaveBufferToImage(buffer, "05_Canvas.png");
        }

        /// <summary>
        /// Generates the clouds visual test.
        /// </summary>
        [TestMethod]
        public void GenerateClouds_VisualTest()
        {
            var buffer = TextureMathEngine.GenerateClouds(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "06_Clouds.png");
        }

        /// <summary>
        /// Generates the marble visual test.
        /// </summary>
        [TestMethod]
        public void GenerateMarble_VisualTest()
        {
            var buffer = TextureMathEngine.GenerateMarble(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "07_Marble.png");
        }

        /// <summary>
        /// Generates the wave visual test.
        /// </summary>
        [TestMethod]
        public void GenerateWave_VisualTest()
        {
            var buffer = TextureMathEngine.GenerateWave(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "08_Wave.png");
        }

        /// <summary>
        /// Generates the cracked ice visual test.
        /// </summary>
        [TestMethod]
        public void GenerateCrackedIce_VisualTest()
        {
            // F2-F1 Voronoi
            var buffer = TextureMathEngine.GenerateAdvancedCellular(
                TestWidth, TestHeight, 64, 255,
                new byte[] { 230, 245, 255 }, // Center (Ridge)
                new byte[] { 10, 40, 80 }); // Edge (Cell)

            SaveBufferToImage(buffer, "09_CrackedIce.png");
        }

        /// <summary>
        /// Generates the magic portal visual test.
        /// </summary>
        [TestMethod]
        public void GenerateMagicPortal_VisualTest()
        {
            // Domain Warping
            var colorRamp = new byte[] { 0, 0, 10, 40, 10, 120, 150, 40, 255, 255, 200, 255 };
            var buffer = TextureMathEngine.GenerateWarpedMapped(
                TestWidth, TestHeight, _noiseGenerator, colorRamp,
                64.0, 128.0, 4.0, 255);

            SaveBufferToImage(buffer, "10_MagicPortal.png");
        }

        /// <summary>
        /// Generates the plasma arc visual test.
        /// </summary>
        [TestMethod]
        public void GeneratePlasmaArc_VisualTest()
        {
            // Ridged Multifractal
            var colorRamp = new byte[] { 0, 0, 0, 40, 0, 80, 0, 200, 255, 255, 255, 255 };
            var buffer = TextureMathEngine.GenerateRidgedMapped(
                TestWidth, TestHeight, _noiseGenerator, colorRamp,
                128.0, 5, 0.5, 255);

            SaveBufferToImage(buffer, "11_PlasmaArc.png");
        }

        /// <summary>
        /// Generates the furrowed tree bark visual test.
        /// </summary>
        [TestMethod]
        public void GenerateTreeBark_VisualTest()
        {
            // Anisotropic domain warped vertical grain lines
            var buffer = TextureMathEngine.GenerateTreeBark(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "12_TreeBark.png");
        }

        /// <summary>
        /// Generates the pointed leaf foliage visual test.
        /// </summary>
        [TestMethod]
        public void GenerateFoliage_VisualTest()
        {
            // Distance-pinched cellular matrix mapping
            var buffer = TextureMathEngine.GenerateFoliage(TestWidth, TestHeight);
            SaveBufferToImage(buffer, "13_Foliage.png");
        }

        /// <summary>
        /// Generates the longitudinal wooden plank board visual test.
        /// </summary>
        [TestMethod]
        public void GenerateWoodPlank_VisualTest()
        {
            // Longitudinal sawn tree trunk ellipse mapping
            var buffer = TextureFactory.GenerateWoodPlank(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "14_WoodPlank.png");
        }

        /// <summary>
        /// Converts the RawTextureBuffer span (BGRA) into a standard PNG file.
        /// </summary>
        private void SaveBufferToImage(RawTextureBuffer buffer, string filename)
        {
            Assert.IsNotNull(buffer, "Texture buffer was null.");

            using var bmp = new Bitmap(TestWidth, TestHeight, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, TestWidth, TestHeight);
            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);

            var span = buffer.AsSpan();

            // Unsafe block for high-performance memory copy
            unsafe
            {
                fixed (byte* ptr = span)
                {
                    Buffer.MemoryCopy(ptr, (void*)bmpData.Scan0, span.Length, bmpData.Stride * TestHeight);
                }
            }

            bmp.UnlockBits(bmpData);

            var filePath = Path.Combine(_outputDirectory, filename);
            bmp.Save(filePath, ImageFormat.Png);

            Trace.WriteLine($"Saved: {filePath}");
        }
    }
}
