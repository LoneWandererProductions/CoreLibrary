using System.Drawing;
using System.Drawing.Imaging;

// using Imaging.Texture; // Ensure your texture engine namespace is referenced

namespace Imaging.Texture.Tests
{
    [TestClass]
    public class TextureMathEngineTests
    {
        private const int TestWidth = 256;
        private const int TestHeight = 256;
        private string _outputDirectory;

        // Now using your actual NoiseGenerator
        private NoiseGenerator _noiseGenerator;

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

        [TestMethod]
        public void GenerateNoise_VisualTest()
        {
            var buffer = TextureMathEngine.GenerateNoise(TestWidth, TestHeight, _noiseGenerator, useTurbulence: true);
            SaveBufferToImage(buffer, "01_Noise.png");
        }

        [TestMethod]
        public void GenerateWood_VisualTest()
        {
            var buffer = TextureMathEngine.GenerateWood(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "02_Wood.png");
        }

        [TestMethod]
        public void GenerateCrosshatch_VisualTest()
        {
            var buffer =
                TextureMathEngine.GenerateCrosshatch(TestWidth, TestHeight, bgR: 255, bgG: 255, bgB: 255, bgA: 255);
            SaveBufferToImage(buffer, "03_Crosshatch.png");
        }

        [TestMethod]
        public void GenerateConcrete_VisualTest()
        {
            var buffer = TextureMathEngine.GenerateConcrete(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "04_Concrete.png");
        }

        [TestMethod]
        public void GenerateCanvas_VisualTest()
        {
            var buffer =
                TextureMathEngine.GenerateCanvas(TestWidth, TestHeight, bgR: 255, bgG: 255, bgB: 255, bgA: 255);
            SaveBufferToImage(buffer, "05_Canvas.png");
        }

        [TestMethod]
        public void GenerateClouds_VisualTest()
        {
            var buffer = TextureMathEngine.GenerateClouds(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "06_Clouds.png");
        }

        [TestMethod]
        public void GenerateMarble_VisualTest()
        {
            var buffer = TextureMathEngine.GenerateMarble(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "07_Marble.png");
        }

        [TestMethod]
        public void GenerateWave_VisualTest()
        {
            var buffer = TextureMathEngine.GenerateWave(TestWidth, TestHeight, _noiseGenerator);
            SaveBufferToImage(buffer, "08_Wave.png");
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

            string filePath = Path.Combine(_outputDirectory, filename);
            bmp.Save(filePath, ImageFormat.Png);

            Console.WriteLine($"Saved: {filePath}");
        }
    }
}