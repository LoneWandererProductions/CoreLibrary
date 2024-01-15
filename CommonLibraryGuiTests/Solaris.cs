﻿/*
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
                DependencyTextures = new Dictionary<int, Texture>
                {
                    {
                        0,
                        new Texture
                        {
                            Layer = 0, Id = 0, Path = Path.Combine(SampleImagesFolder.FullName, "Tile.png")
                        }
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
                DependencyTextureSize = 100,
                DependencyHeight = 2,
                DependencyWidth = 3,
                DependencyMap = map
            };

            //way hacky but works for for now....
            aurora.Initiate();

            var data = compare.CompareImages(bmResultBase, aurora.BitmapLayerOne);

            Assert.AreEqual(100, data.Similarity, string.Concat("Map was not correct: ", data.Similarity));

            map = new Dictionary<int, List<int>>
            {
                { 0, new List<int> { 0, 1, 2 } },
                { 1, new List<int> { 0, 1 } },
                { 2, new List<int> { 0, 1 } },
                { 3, new List<int> { 0, 1 } },
                { 4, new List<int> { 0, 1 } },
                { 5, new List<int> { 0 } }
            };

            aurora.DependencyMap = map;
            aurora.Initiate();

            aurora.BitmapLayerOne.Save(string.Concat(SampleImagesFolder, "/example.png"), ImageFormat.Png);

            data = compare.CompareImages(bmResultLayerOther, aurora.BitmapLayerOne);

            Assert.AreEqual(100, data.Similarity, string.Concat("Aurora Map was not correct: ", data.Similarity));

            //test remove
            aurora.DependencyRemove = new KeyValuePair<int, int>(0, 2);
            aurora.DependencyRemove = new KeyValuePair<int, int>(0, 1);
            aurora.DependencyRemove = new KeyValuePair<int, int>(1, 1);
            aurora.DependencyRemove = new KeyValuePair<int, int>(2, 1);
            aurora.DependencyRemove = new KeyValuePair<int, int>(3, 1);
            aurora.DependencyRemove = new KeyValuePair<int, int>(4, 1);
            aurora.DependencyRemove = new KeyValuePair<int, int>(5, 1);

            data = compare.CompareImages(bmResultBase, aurora.BitmapLayerOne);

            aurora.BitmapLayerOne.Save(string.Concat(SampleImagesFolder, "/example.png"), ImageFormat.Png);

            Assert.AreEqual(100, data.Similarity,
                string.Concat("Aurora Map remove was not correct: ", data.Similarity));
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
                DependencyTextures = new Dictionary<int, Texture>
                {
                    {
                        0,
                        new Texture
                        {
                            Layer = 0, Id = 0, Path = Path.Combine(SampleImagesFolder.FullName, "Tile.png")
                        }
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
                DependencyTextureSize = 100,
                DependencyHeight = 2,
                DependencyWidth = 3
            };

            //way hacky but works for for now....
            polaris.Initiate();
            var blank = polaris.BitmapLayerThree;

            //0
            var keyValue = new KeyValuePair<int, int>(0, 0);
            polaris.DependencyAdd = keyValue;
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

            polaris.BitmapLayerOne.Save(string.Concat(SampleImagesFolder, "/example Polaris.png"), ImageFormat.Png);

            var data = compare.CompareImages(bmResultLayerOther, polaris.BitmapLayerOne);

            Assert.AreEqual(100, data.Similarity, string.Concat("Map Polaris was not correct: ", data.Similarity));

            polaris.DependencyAddDisplay = new KeyValuePair<int, int>(0, 0);
            polaris.DependencyAddDisplay = new KeyValuePair<int, int>(1, 0);
            polaris.DependencyAddDisplay = new KeyValuePair<int, int>(2, 0);
            polaris.DependencyAddDisplay = new KeyValuePair<int, int>(3, 0);
            polaris.DependencyAddDisplay = new KeyValuePair<int, int>(4, 0);
            polaris.DependencyAddDisplay = new KeyValuePair<int, int>(5, 0);

            data = compare.CompareImages(bmResultBase, polaris.BitmapLayerThree);

            var polarisMap = polaris.DependencyMap;
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

            Assert.AreEqual(100, data.Similarity, string.Concat("Map Polaris was not correct: ", data.Similarity));

            polaris.DependencyRemoveDisplay = 0;
            polaris.DependencyRemoveDisplay = 1;
            polaris.DependencyRemoveDisplay = 2;
            polaris.DependencyRemoveDisplay = 3;
            polaris.DependencyRemoveDisplay = 4;
            polaris.DependencyRemoveDisplay = 5;

            data = compare.CompareImages(blank, polaris.BitmapLayerThree);

            Assert.AreEqual(100, data.Similarity, string.Concat("Map Polaris was not correct: ", data.Similarity));

            //this is a duplicate so this should not be added
            polaris.DependencyAddDisplay = new KeyValuePair<int, int>(0, 0);

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