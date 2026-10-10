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
            Trace.WriteLine($"Textures will be saved to: {_outputDirectory}");
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
            var buffer = TextureMathEngine.GenerateFoliage(TestWidth, TestHeight, _noiseGenerator);
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
        /// Generates the directional stone visual test with solid, dark mortar.
        /// Perfect for continuous dungeon walls or paved roads.
        /// </summary>
        [TestMethod]
        public void GenerateDirectionalStone_VisualTest()
        {
            // Calls the factory wrapper we built, ensuring mortar is filled in
            var buffer = TextureFactory.GenerateStoneTexture(TestWidth, TestHeight, _noiseGenerator, fillArea: true);
            SaveBufferToImage(buffer, "15_DirectionalStone_Solid_4.png");
        }

        /// <summary>
        /// Generates the directional stone visual test with transparent gaps.
        /// Designed for dropping loose rocks or rubble over existing terrain tiles.
        /// </summary>
        [TestMethod]
        public void GenerateDirectionalStoneTransparent_VisualTest()
        {
            // By setting fillArea to false, the deep recesses are dropped to Alpha 0
            var buffer = TextureFactory.GenerateStoneTexture(TestWidth, TestHeight, _noiseGenerator, fillArea: false);
            SaveBufferToImage(buffer, "16_DirectionalStone_Transparent_4.png");
        }

        /// <summary>
        /// Generates the directional stone visual test scaled up to 16.
        /// </summary>
        [TestMethod]
        public void GenerateDirectionalStone_VisualTest_Scale()
        {
            var conf = TextureConstants.GetStoneConfig();
            // Clone or create a new config just for this test
            var testConfig = new TextureConfig
            {
                VoronoiGridSize = 16, // Scale up the stones for the larger test image
                RgbRamp = conf.RgbRamp
            };

            var buffer =
                TextureFactory.GenerateStoneTexture(TestWidth, TestHeight, _noiseGenerator, testConfig, fillArea: true);
            SaveBufferToImage(buffer, "17_DirectionalStone_Solid_16.png");
        }

        [TestMethod]
        public void GenerateGenerateCobblestone_VisualTest()
        {
            // By setting fillArea to false, the deep recesses are dropped to Alpha 0
            var buffer = TextureFactory.GenerateCobblestone(TestWidth, TestHeight);
            SaveBufferToImage(buffer, "18_Cobblestone.png");
        }


        /// <summary>
        /// Generates the brushed steel visual test.
        /// </summary>
        [TestMethod]
        public void GenerateSteel_VisualTest()
        {
            var buffer = TextureFactory.GenerateSteel(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "19_BrushedSteel.png");
        }

        /// <summary>
        /// Generates the default and custom base color latex visual tests.
        /// </summary>
        [TestMethod]
        public void GenerateLatex_VisualTest()
        {
            // Default dark latex
            var buffer = TextureFactory.GenerateLatex(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "20_Latex_Default.png");

            // Custom base color test (Red Latex)
            var redLatexConfig = TextureConstants.GetCustomLatexConfig(180, 15, 30);
            var redBuffer = TextureFactory.GenerateLatex(TestWidth, TestHeight, _noiseGenerator, redLatexConfig);
            SaveBufferToImage(redBuffer, "20_Latex_Red.png");
        }

        /// <summary>
        /// Generates the default brown and custom tan leather visual tests.
        /// </summary>
        [TestMethod]
        public void GenerateLeather_VisualTest()
        {
            // Default dark brown leather
            var buffer = TextureFactory.GenerateLeather(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "21_Leather_Default.png");

            // Custom base color test (Tan Leather)
            var tanConfig = TextureConstants.GetCustomLeatherConfig(180, 120, 70);
            var tanBuffer = TextureFactory.GenerateLeather(TestWidth, TestHeight, _noiseGenerator, tanConfig);
            SaveBufferToImage(tanBuffer, "21_Leather_Tan.png");
        }

        /// <summary>
        /// Generates the high polished chrome steel visual test.
        /// </summary>
        [TestMethod]
        public void GeneratePolishedSteel_VisualTest()
        {
            var buffer = TextureFactory.GeneratePolishedSteel(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "22_PolishedSteel.png");
        }

        /// <summary>
        /// Generates the volumetric leaf cloud visual test with transparent canopy gaps.
        /// Ideal for bush clusters and tree canopies.
        /// </summary>
        [TestMethod]
        public void GenerateLeafCloud_VisualTest()
        {
            var buffer = TextureFactory.GenerateLeafCloud(TestWidth, TestHeight, _noiseGenerator, fillArea: false);
            SaveBufferToImage(buffer, "23_LeafCloud_Transparent.png");
        }

        /// <summary>
        /// Generates the volumetric leaf cloud visual test with solid dark inner foliage backing.
        /// </summary>
        [TestMethod]
        public void GenerateLeafCloud_Solid_VisualTest()
        {
            var buffer = TextureFactory.GenerateLeafCloud(TestWidth, TestHeight, _noiseGenerator, fillArea: true);
            SaveBufferToImage(buffer, "23_LeafCloud_Solid.png");
        }

        /// <summary>
        /// Generates the smooth volumetric leaf cloud visual test with transparent canopy gaps.
        /// </summary>
        [TestMethod]
        public void GenerateVolumetricLeafCloud_VisualTest()
        {
            var buffer =
                TextureFactory.GenerateVolumetricLeafCloud(TestWidth, TestHeight, _noiseGenerator, fillArea: false);
            SaveBufferToImage(buffer, "24_VolumetricLeafCloud_Transparent.png");
        }

        /// <summary>
        /// Generates the smooth volumetric leaf cloud visual test with solid dark inner foliage backing.
        /// </summary>
        [TestMethod]
        public void GenerateVolumetricLeafCloud_Solid_VisualTest()
        {
            var buffer =
                TextureFactory.GenerateVolumetricLeafCloud(TestWidth, TestHeight, _noiseGenerator, fillArea: true);
            SaveBufferToImage(buffer, "24_VolumetricLeafCloud_Solid.png");
        }

        /// <summary>
        /// Generates the soft, low-noise terrain grass visual test.
        /// </summary>
        [TestMethod]
        public void GenerateTerrainGrass_VisualTest()
        {
            var buffer = TextureFactory.GenerateTerrainGrass(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "25_TerrainGrass.png");
        }

        /// <summary>
        /// Generates the soft, muted terrain dirt visual test.
        /// </summary>
        [TestMethod]
        public void GenerateTerrainDirt_VisualTest()
        {
            var buffer = TextureFactory.GenerateTerrainDirt(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "26_TerrainDirt.png");
        }

        /// <summary>
        /// Generates the mountain rock visual test.
        /// </summary>
        [TestMethod]
        public void GenerateMountainRock_VisualTest()
        {
            var buffer = TextureFactory.GenerateMountainRock(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "27_MountainRock.png");
        }

        /// <summary>
        /// Generates the layered dungeon sandstone visual test.
        /// </summary>
        [TestMethod]
        public void GenerateDungeonSandstone_VisualTest()
        {
            var buffer = TextureFactory.GenerateDungeonSandstone(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "28_DungeonSandstone.png");
        }

        /// <summary>
        /// Generates the flat, low-noise dungeon sandstone visual test.
        /// </summary>
        [TestMethod]
        public void GenerateDungeonSandstoneFlat_VisualTest()
        {
            var buffer = TextureFactory.GenerateDungeonSandstoneFlat(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "29_DungeonSandstone_Flat.png");
        }

        /// <summary>
        /// Generates the raw cast iron visual test.
        /// </summary>
        [TestMethod]
        public void GenerateRawIron_VisualTest()
        {
            var buffer = TextureFactory.GenerateRawIron(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "30_RawIron.png");
        }

        /// <summary>
        /// Generates the wrought iron visual test.
        /// </summary>
        [TestMethod]
        public void GenerateWroughtIron_VisualTest()
        {
            var buffer = TextureFactory.GenerateWroughtIron(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "31_WroughtIron.png");
        }

        /// <summary>
        /// Generates the rusted iron visual test.
        /// </summary>
        [TestMethod]
        public void GenerateRustedIron_VisualTest()
        {
            var buffer = TextureFactory.GenerateRustedIron(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "32_RustedIron.png");
        }

        /// <summary>
        /// Generates the rippled desert sand visual test.
        /// </summary>
        [TestMethod]
        public void GenerateDesertSand_VisualTest()
        {
            var buffer = TextureFactory.GenerateDesertSand(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "33_DesertSand_Rippled.png");
        }

        /// <summary>
        /// Generates the flat desert sand visual test for pond beds and muted background areas.
        /// </summary>
        [TestMethod]
        public void GenerateDesertSandFlat_VisualTest()
        {
            var buffer = TextureFactory.GenerateDesertSandFlat(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "34_DesertSand_Flat.png");
        }

        /// <summary>
        /// Converts the RawTextureBuffer span (BGRA) into a standard PNG file.
        /// </summary>
        private void SaveBufferToImage(RawTextureBuffer? buffer, string filename)
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

            // Save via MemoryStream to bypass GDI+ native file-locking issues
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            File.WriteAllBytes(filePath, ms.ToArray());

            Trace.WriteLine($"Saved: {filePath}");
        }
    }
}
