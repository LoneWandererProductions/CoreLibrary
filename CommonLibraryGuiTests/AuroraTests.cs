﻿using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
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


        [Test]
        [Apartment(ApartmentState.STA)]
        public void CombineBitMap()
        {
            var bmpBase = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "Tile.png"));
            var bmpLayerOne = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "layerone.png"));
            var bmpLayertwo = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "Layertwo.png"));
            var bmResultOne = new Bitmap(Path.Combine(SampleImagesFolder.FullName, "ResultOne.png"));

            var lst = new List<Bitmap> { bmpBase , bmpLayerOne, bmpLayertwo, bmResultOne };

        }
    }
}
