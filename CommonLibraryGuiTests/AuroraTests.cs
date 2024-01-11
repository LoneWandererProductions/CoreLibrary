/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGuiTests
 * FILE:        CommonLibraryGuiTests/AuroraTests.cs
 * PURPOSE:     Tests the Aurorae Display
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using Aurorae;
using ImageCompare;
using Imaging;
using NUnit.Framework;

namespace CommonLibraryGuiTests
{
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

            var lst = new List<Bitmap> { bmpBase , bmpLayerOne, bmpLayerTwo, bmResultOne };

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
            var aurora = new Aurora();
            var polaris = new Polaris();


        }
    }
}
