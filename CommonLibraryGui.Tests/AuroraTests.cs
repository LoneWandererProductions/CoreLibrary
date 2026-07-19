/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGuiTests
 * FILE:        AuroraTests.cs
 * PURPOSE:     Tests the Aurorae Display
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ExtendedSystemObjects;
using ExtendedSystemObjects.Helper;
using ImageCompare;
using NUnit.Framework;
using Solaris;

namespace CommonLibraryGuiTests
{
    /// <summary>
    /// Some Basic tests for the Aurora Engine and the image Combining
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public sealed class AuroraTests
    {
        private string _sampleImagesFolder;

        [SetUp]
        public void Setup()
        {
            // Resolve the path dynamically based on the test execution directory
            var baseDirectory = TestContext.CurrentContext.TestDirectory;
            _sampleImagesFolder = Path.Combine(baseDirectory, "Image");

            if (!Directory.Exists(_sampleImagesFolder))
            {
                Assert.Ignore($"The Image folder could not be found at: {_sampleImagesFolder}. " +
                              "Ensure the 'Image' folder is set to 'Copy to Output Directory'.");
            }
        }

        /// <summary>
        /// Test Aurora.
        /// </summary>
        [Test]
        public void Aurora()
        {
            using var bmResultLayerOther = LoadBitmap("result_layer_other.png");
            using var bmResultBase = LoadBitmap("result_base.png");

            var compare = new ImageAnalysis();

            // Generate texture Dictionary, and all the other data
            var map = new Dictionary<int, List<int>>
            {
                { 0, new List<int> { 0 } },
                { 1, new List<int> { 0 } },
                { 2, new List<int> { 0 } },
                { 3, new List<int> { 0 } },
                { 4, new List<int> { 0 } },
                { 5, new List<int> { 0 } }
            };

            var aurora = new Aurora
            {
                AuroraTextures = new Dictionary<int, Texture>
                {
                    { 0, new Texture { Layer = 0, Id = 0, Path = Path.Combine(_sampleImagesFolder, "Tile.png") } },
                    { 1, new Texture { Layer = 1, Id = 1, Path = Path.Combine(_sampleImagesFolder, "layerOne.png") } },
                    { 2, new Texture { Layer = 1, Id = 1, Path = Path.Combine(_sampleImagesFolder, "LayerTwo.png") } }
                },
                AuroraTextureSize = 100,
                AuroraHeight = 2,
                AuroraWidth = 3,
                AuroraMap = map
            };

            aurora.Initiate();

            var data = compare.CompareImages(bmResultBase, aurora.BitmapLayerOne);

            Assert.That(data.Similarity, Is.EqualTo(100), $"Map was not correct: {data.Similarity}");

            map = new Dictionary<int, List<int>>
            {
                { 0, new List<int> { 0, 1, 2 } },
                { 1, new List<int> { 0, 1 } },
                { 2, new List<int> { 0, 1 } },
                { 3, new List<int> { 0, 1 } },
                { 4, new List<int> { 0, 1 } },
                { 5, new List<int> { 0 } }
            };

            aurora.AuroraMap = map;
            aurora.Initiate();

            aurora.BitmapLayerOne.Save(Path.Combine(_sampleImagesFolder, "example.png"), ImageFormat.Png);

            data = compare.CompareImages(bmResultLayerOther, aurora.BitmapLayerOne);

            Assert.That(data.Similarity, Is.EqualTo(100), $"Aurora Map was not correct: {data.Similarity}");

            // Test remove
            aurora.AuroraRemove = new KeyValuePair<int, int>(0, 2);
            aurora.AuroraRemove = new KeyValuePair<int, int>(0, 1);
            aurora.AuroraRemove = new KeyValuePair<int, int>(1, 1);
            aurora.AuroraRemove = new KeyValuePair<int, int>(2, 1);
            aurora.AuroraRemove = new KeyValuePair<int, int>(3, 1);
            aurora.AuroraRemove = new KeyValuePair<int, int>(4, 1);
            aurora.AuroraRemove = new KeyValuePair<int, int>(5, 1);

            data = compare.CompareImages(bmResultBase, aurora.BitmapLayerOne);

            aurora.BitmapLayerOne.Save(Path.Combine(_sampleImagesFolder, "example.png"), ImageFormat.Png);

            Assert.That(data.Similarity, Is.EqualTo(100), $"Aurora Map remove was not correct: {data.Similarity}");
        }

        /// <summary>
        /// Test Polaris.
        /// </summary>
        [Test]
        public void Polaris()
        {
            using var bmResultLayerOther = LoadBitmap("result_layer_other.png");
            using var bmResultBase = LoadBitmap("result_base.png");

            var compare = new ImageAnalysis();

            var polaris = new Polaris
            {
                PolarisTextures = new Dictionary<int, Texture>
                {
                    { 0, new Texture { Layer = 0, Id = 0, Path = Path.Combine(_sampleImagesFolder, "Tile.png") } },
                    { 1, new Texture { Layer = 1, Id = 1, Path = Path.Combine(_sampleImagesFolder, "layerOne.png") } },
                    { 2, new Texture { Layer = 1, Id = 1, Path = Path.Combine(_sampleImagesFolder, "LayerTwo.png") } }
                },
                PolarisTextureSize = 100,
                PolarisHeight = 2,
                PolarisWidth = 3
            };

            polaris.Initiate();

            // Note: If BitmapLayerThree is managed by Polaris and needs disposal, 
            // do not dispose it here while it is in use.
            var blank = polaris.BitmapLayerThree;

            // 0
            polaris.AddTile(new KeyValuePair<int, int>(0, 0));
            polaris.AddTile(new KeyValuePair<int, int>(0, 1));
            polaris.AddTile(new KeyValuePair<int, int>(0, 2));
            // 1
            polaris.AddTile(new KeyValuePair<int, int>(1, 0));
            polaris.AddTile(new KeyValuePair<int, int>(1, 1));
            // 2
            polaris.AddTile(new KeyValuePair<int, int>(2, 0));
            polaris.AddTile(new KeyValuePair<int, int>(2, 1));
            // 3
            polaris.AddTile(new KeyValuePair<int, int>(3, 0));
            polaris.AddTile(new KeyValuePair<int, int>(3, 1));
            // 4
            polaris.AddTile(new KeyValuePair<int, int>(4, 0));
            polaris.AddTile(new KeyValuePair<int, int>(4, 1));
            // 5
            polaris.AddTile(new KeyValuePair<int, int>(5, 0));

            polaris.BitmapLayerOne.Save(Path.Combine(_sampleImagesFolder, "example Polaris.png"), ImageFormat.Png);

            var data = compare.CompareImages(bmResultLayerOther, polaris.BitmapLayerOne);
            Assert.That(data.Similarity, Is.EqualTo(100), $"Map Polaris was not correct: {data.Similarity}");

            polaris.AddDisplay(new KeyValuePair<int, int>(0, 0));
            polaris.AddDisplay(new KeyValuePair<int, int>(1, 0));
            polaris.AddDisplay(new KeyValuePair<int, int>(2, 0));
            polaris.AddDisplay(new KeyValuePair<int, int>(3, 0));
            polaris.AddDisplay(new KeyValuePair<int, int>(4, 0));
            polaris.AddDisplay(new KeyValuePair<int, int>(5, 0));

            data = compare.CompareImages(bmResultBase, polaris.BitmapLayerThree);

            var polarisMap = polaris.PolarisMap;

            var map = new Dictionary<int, List<int>>
            {
                { 0, new List<int> { 0, 1, 2 } },
                { 1, new List<int> { 0, 1 } },
                { 2, new List<int> { 0, 1 } },
                { 3, new List<int> { 0, 1 } },
                { 4, new List<int> { 0, 1 } },
                { 5, new List<int> { 0 } }
            };

            for (var i = 0; i <= 5; i++)
            {
                var lst = polarisMap[i];
                var check = lst.Equal(map[i], EnumerableCompare.IgnoreOrder);
                if (!check)
                {
                    Assert.Fail("wrong map");
                }
            }

            Assert.That(data.Similarity, Is.EqualTo(100), $"Map Polaris was not correct: {data.Similarity}");

            polaris.RemoveDisplay(0);
            polaris.RemoveDisplay(1);
            polaris.RemoveDisplay(2);
            polaris.RemoveDisplay(3);
            polaris.RemoveDisplay(4);
            polaris.RemoveDisplay(5);

            data = compare.CompareImages(blank, polaris.BitmapLayerThree);
            Assert.That(data.Similarity, Is.EqualTo(100), $"Map Polaris was not correct: {data.Similarity}");

            // This is a duplicate so this should not be added
            polaris.AddDisplay(new KeyValuePair<int, int>(0, 0));

            for (var i = 0; i <= 5; i++)
            {
                var lst = polarisMap[i];
                var check = lst.Equal(map[i], EnumerableCompare.IgnoreOrder);
                if (!check)
                {
                    Assert.Fail("wrong map");
                }
            }
        }

        /// <summary>
        /// Loads the bitmap.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>A new Bitmap object.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
        private Bitmap LoadBitmap(string fileName)
        {
            var path = Path.Combine(_sampleImagesFolder, fileName);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Bitmap file not found: {path}");
            }

            // Check if file is empty
            var fileInfo = new FileInfo(path);
            if (fileInfo.Length == 0)
            {
                throw new InvalidDataException($"The file is 0 bytes and cannot be loaded as an image: {path}");
            }

            // Open with FileShare.ReadWrite to prevent locking issues
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                // We create a copy so the stream can be closed immediately
                var bitmap = new Bitmap(stream);
                return new Bitmap(bitmap);
            }
        }
    }
}
