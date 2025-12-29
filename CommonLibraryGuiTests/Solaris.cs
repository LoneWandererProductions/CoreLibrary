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
using ExtendedSystemObjects;
using ExtendedSystemObjects.Helper;
using ImageCompare;
using NUnit.Framework;
using Solaris;

namespace CommonLibraryGuiTests
{
    /// <summary>
    ///     Some Basic tests for the Aurora Engine and the image Combining
    /// </summary>
    public sealed class Solaris
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
        ///     Test Aurora.
        /// </summary>
        [Test]
        [Apartment(ApartmentState.STA)]
        public void Aurora()
        {
            var bmResultLayerOther = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "result_layer_other.png"));
            var bmResultBase = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "result_base.png"));

            var compare = new ImageAnalysis();

            //new Test with UI
            //generate texture Dictionary, and all the other data;
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
                    {
                        0,
                        new Texture { Layer = 0, Id = 0, Path = Path.Combine(SampleImagesFolder.FullName, "Tile.png") }
                    },
                    {
                        1,
                        new Texture
                        {
                            Layer = 1, Id = 1, Path = Path.Combine(SampleImagesFolder.FullName, "layerOne.png")
                        }
                    },
                    {
                        2,
                        new Texture
                        {
                            Layer = 1, Id = 1, Path = Path.Combine(SampleImagesFolder.FullName, "LayerTwo.png")
                        }
                    }
                },
                AuroraTextureSize = 100,
                AuroraHeight = 2,
                AuroraWidth = 3,
                AuroraMap = map
            };

            //way hacky but works for for now....
            aurora.Initiate();

            var data = compare.CompareImages(bmResultBase, aurora.BitmapLayerOne);

            Assert.AreEqual(100, data.Similarity, $"Map was not correct: {data.Similarity}");

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

            aurora.BitmapLayerOne.Save($"{SampleImagesFolder}/example.png", ImageFormat.Png);

            data = compare.CompareImages(bmResultLayerOther, aurora.BitmapLayerOne);

            Assert.AreEqual(100, data.Similarity, $"Aurora Map was not correct: {data.Similarity}");

            //test remove
            aurora.AuroraRemove = new KeyValuePair<int, int>(0, 2);
            aurora.AuroraRemove = new KeyValuePair<int, int>(0, 1);
            aurora.AuroraRemove = new KeyValuePair<int, int>(1, 1);
            aurora.AuroraRemove = new KeyValuePair<int, int>(2, 1);
            aurora.AuroraRemove = new KeyValuePair<int, int>(3, 1);
            aurora.AuroraRemove = new KeyValuePair<int, int>(4, 1);
            aurora.AuroraRemove = new KeyValuePair<int, int>(5, 1);

            data = compare.CompareImages(bmResultBase, aurora.BitmapLayerOne);

            aurora.BitmapLayerOne.Save($"{SampleImagesFolder}/example.png", ImageFormat.Png);

            Assert.AreEqual(100, data.Similarity,
                $"Aurora Map remove was not correct: {data.Similarity}");
        }

        /// <summary>
        ///     Test Polaris.
        /// </summary>
        [Test]
        [Apartment(ApartmentState.STA)]
        public void Polaris()
        {
            var bmResultLayerOther = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "result_layer_other.png"));
            var bmResultBase = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "result_base.png"));

            var compare = new ImageAnalysis();

            //new Test with other UI
            var polaris = new Polaris
            {
                PolarisTextures = new Dictionary<int, Texture>
                {
                    {
                        0,
                        new Texture { Layer = 0, Id = 0, Path = Path.Combine(SampleImagesFolder.FullName, "Tile.png") }
                    },
                    {
                        1,
                        new Texture
                        {
                            Layer = 1, Id = 1, Path = Path.Combine(SampleImagesFolder.FullName, "layerOne.png")
                        }
                    },
                    {
                        2,
                        new Texture
                        {
                            Layer = 1, Id = 1, Path = Path.Combine(SampleImagesFolder.FullName, "LayerTwo.png")
                        }
                    }
                },
                PolarisTextureSize = 100,
                PolarisHeight = 2,
                PolarisWidth = 3
            };

            //way hacky but works for for now....
            polaris.Initiate();
            var blank = polaris.BitmapLayerThree;

            //0
            var keyValue = new KeyValuePair<int, int>(0, 0);
            polaris.PolarisAdd = keyValue;
            keyValue = new KeyValuePair<int, int>(0, 1);
            polaris.PolarisAdd = keyValue;
            keyValue = new KeyValuePair<int, int>(0, 2);
            polaris.PolarisAdd = keyValue;
            //1
            keyValue = new KeyValuePair<int, int>(1, 0);
            polaris.PolarisAdd = keyValue;
            keyValue = new KeyValuePair<int, int>(1, 1);
            polaris.PolarisAdd = keyValue;
            //2
            keyValue = new KeyValuePair<int, int>(2, 0);
            polaris.PolarisAdd = keyValue;
            keyValue = new KeyValuePair<int, int>(2, 1);
            polaris.PolarisAdd = keyValue;
            //3
            keyValue = new KeyValuePair<int, int>(3, 0);
            polaris.PolarisAdd = keyValue;
            keyValue = new KeyValuePair<int, int>(3, 1);
            polaris.PolarisAdd = keyValue;
            //4
            keyValue = new KeyValuePair<int, int>(4, 0);
            polaris.PolarisAdd = keyValue;
            keyValue = new KeyValuePair<int, int>(4, 1);
            polaris.PolarisAdd = keyValue;
            //5
            keyValue = new KeyValuePair<int, int>(5, 0);
            polaris.PolarisAdd = keyValue;

            polaris.BitmapLayerOne.Save($"{SampleImagesFolder}/example Polaris.png", ImageFormat.Png);

            var data = compare.CompareImages(bmResultLayerOther, polaris.BitmapLayerOne);

            Assert.AreEqual(100, data.Similarity, $"Map Polaris was not correct: {data.Similarity}");

            polaris.PolarisAddDisplay = new KeyValuePair<int, int>(0, 0);
            polaris.PolarisAddDisplay = new KeyValuePair<int, int>(1, 0);
            polaris.PolarisAddDisplay = new KeyValuePair<int, int>(2, 0);
            polaris.PolarisAddDisplay = new KeyValuePair<int, int>(3, 0);
            polaris.PolarisAddDisplay = new KeyValuePair<int, int>(4, 0);
            polaris.PolarisAddDisplay = new KeyValuePair<int, int>(5, 0);

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

            Assert.AreEqual(100, data.Similarity, $"Map Polaris was not correct: {data.Similarity}");

            polaris.PolarisRemoveDisplay = 0;
            polaris.PolarisRemoveDisplay = 1;
            polaris.PolarisRemoveDisplay = 2;
            polaris.PolarisRemoveDisplay = 3;
            polaris.PolarisRemoveDisplay = 4;
            polaris.PolarisRemoveDisplay = 5;

            data = compare.CompareImages(blank, polaris.BitmapLayerThree);

            Assert.AreEqual(100, data.Similarity, $"Map Polaris was not correct: {data.Similarity}");

            //this is a duplicate so this should not be added
            polaris.PolarisAddDisplay = new KeyValuePair<int, int>(0, 0);

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
    }
}
