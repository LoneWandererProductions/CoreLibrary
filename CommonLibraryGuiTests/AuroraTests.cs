/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGuiTests
 * FILE:        CommonLibraryGuiTests/AuroraTests.cs
 * PURPOSE:     Tests the Aurorae Display
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Aurorae;
using ImageCompare;
using Imaging;
using NUnit.Framework;

namespace CommonLibraryGuiTests
{
    /// <summary>
    /// Some Basic tests for the Aurora Engine and the image Combining
    /// </summary>
    public sealed class AuroraTests
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
        private static readonly DirectoryInfo SampleImagesFolder = new(Path.Combine(ProjectFolder.FullName, "Image"));


        /// <summary>
        /// Test combining bitmaps.
        /// </summary>
        [Test]
        [Apartment(ApartmentState.STA)]
        public void CombineBitMap()
        {
            var bmpBase = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "Tile.png"));
            var bmpLayerOne = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "layerOne.png"));
            var bmpLayerTwo = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "LayerTwo.png"));
            var bmResultOne = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "ResultOne.png"));
            var bmResultLayerOne = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "result_layer_one.png"));
            var bmResultLayerTwo = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "result_layer_two.png"));
            var bmResultLayerOther = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "result_layer_other.png"));
            var bmResultBase = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "result_base.png"));

            var imageList = new List<Bitmap> { bmpBase , bmpLayerOne, bmpLayerTwo, bmResultOne, bmResultLayerOne, bmResultLayerTwo, bmResultLayerOther, bmResultBase };

            var render = new ImageRender();

            var cache = render.CombineBitmap(bmpBase, bmpLayerOne, 0, 0);
            cache = render.CombineBitmap(cache, bmpLayerTwo, 0, 0);

            var compare = new ImageAnalysis();

            //should be near 100%
            var data = compare.CompareImages(bmResultOne, cache);

            //Todo implement ImageComparer, 2 images
            Assert.AreEqual(100, data.Similarity, string.Concat("Compare failed: ", data.Similarity));

            data = compare.CompareImages(Path.Combine(SampleImagesFolder.FullName, "Tile.png"), Path.Combine(SampleImagesFolder.FullName, "Tile.png"));

            Assert.AreEqual(100, data.Similarity, string.Concat("Compare failed Path: ", data.Similarity));

            //new Test with UI
            //generate texture Dictionary, and all the other data;
            var map = new Dictionary<int, List<int>>
            {
                {0, new List<int> { 0 } },
                {1, new List<int> { 0 } },
                {2, new List<int> { 0 } },
                {3, new List<int> { 0 } },
                {4, new List<int> { 0 } },
                {5, new List<int> { 0 } }
            };

            var aurora = new Aurora
            {
                DependencyTextures = new Dictionary<int, Texture>
                {
                    {
                        0, new Texture
                        {
                            Layer = 0, Id = 0, Path = Path.Combine(SampleImagesFolder.FullName, "Tile.png")
                        }
                    },
                    {
                        1, new Texture
                        {
                            Layer = 1, Id = 1, Path = Path.Combine(SampleImagesFolder.FullName, "layerOne.png")
                        }
                    },
                    {
                        2, new Texture
                        {
                            Layer = 1, Id = 1, Path = Path.Combine(SampleImagesFolder.FullName, "LayerTwo.png")
                        }
                    },
                },
                DependencyTextureSize = 100,
                DependencyHeight = 2,
                DependencyWidth = 3,
                DependencyMap = map
            };

            //way hacky but works for for now....
            aurora.Initiate();

            data = compare.CompareImages(bmResultBase, aurora.BitmapLayerOne);

            Assert.AreEqual(100, data.Similarity, string.Concat("Map was not correct: ", data.Similarity));

            map = new Dictionary<int, List<int>>
            {
                {0, new List<int> { 0 , 1 , 2 } },
                {1, new List<int> { 0 , 1 } },
                {2, new List<int> { 0 , 1 } },
                {3, new List<int> { 0 , 1 } },
                {4, new List<int> { 0 , 1 } },
                {5, new List<int> { 0 } }
            };

            aurora.DependencyMap = map;
            aurora.Initiate();

            aurora.BitmapLayerOne.Save(string.Concat(SampleImagesFolder, "/example.png"), ImageFormat.Png);

            data = compare.CompareImages(bmResultLayerOther, aurora.BitmapLayerOne);

            Assert.AreEqual(100, data.Similarity, string.Concat("Aurora Map was not correct: ", data.Similarity));

            //TODO add tests and more functions

            //new Test with other UI
            var polaris = new Polaris
            {
                DependencyTextures = new Dictionary<int, Texture>
                {
                    {
                        0, new Texture
                        {
                            Layer = 0, Id = 0, Path = Path.Combine(SampleImagesFolder.FullName, "Tile.png")
                        }
                    },
                    {
                        1, new Texture
                        {
                            Layer = 1, Id = 1, Path = Path.Combine(SampleImagesFolder.FullName, "layerOne.png")
                        }
                    },
                    {
                        2, new Texture
                        {
                            Layer = 1, Id = 1, Path = Path.Combine(SampleImagesFolder.FullName, "LayerTwo.png")
                        }
                    },
                },
                DependencyTextureSize = 100,
                DependencyHeight = 2,
                DependencyWidth = 3
            };

            //0
            var keyValue = new KeyValuePair<int, int>(0, 0);
            polaris.DependencyAdd =keyValue;
            keyValue = new KeyValuePair<int, int>(0, 1);
            polaris.DependencyAdd = keyValue;
            keyValue = new KeyValuePair<int, int>(0, 2);
            polaris.DependencyAdd = keyValue;
            //1
            keyValue = new KeyValuePair<int, int>(1, 0);
            polaris.DependencyAdd = keyValue;
            keyValue = new KeyValuePair<int, int>(1, 1);
            polaris.DependencyAdd = keyValue;
            //2
            keyValue = new KeyValuePair<int, int>(2, 0);
            polaris.DependencyAdd = keyValue;
            keyValue = new KeyValuePair<int, int>(2, 1);
            polaris.DependencyAdd = keyValue;
            //3
            keyValue = new KeyValuePair<int, int>(3, 0);
            polaris.DependencyAdd = keyValue;
            keyValue = new KeyValuePair<int, int>(3, 1);
            polaris.DependencyAdd = keyValue;
            //4
            keyValue = new KeyValuePair<int, int>(4, 0);
            polaris.DependencyAdd = keyValue;
            keyValue = new KeyValuePair<int, int>(4, 1);
            polaris.DependencyAdd = keyValue;
            //5
            keyValue = new KeyValuePair<int, int>(5, 0);
            polaris.DependencyAdd = keyValue;

            //Todo compare map

            polaris.BitmapLayerOne.Save(string.Concat(SampleImagesFolder, "/example Polaris.png"), ImageFormat.Png);

            data = compare.CompareImages(bmResultLayerOther, polaris.BitmapLayerOne);

            Assert.AreEqual(100, data.Similarity, string.Concat("Map Polaris was not correct: ", data.Similarity));
        }
    }
}
