/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGui.Tests
 * FILE:        CommonCtrl.cs
 * PURPOSE:     Tests for CommonCtrl some Controls, not all yet
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedVariable

using Common.Controls;
using Common.Images;
using Imaging;
using InterOp;
using NUnit.Framework;

namespace CommonLibraryGui.Tests
{
    /// <summary>
    ///     The common Controls unit test class.
    /// </summary>
    [TestFixture] // Explicitly tells NUnit this class contains tests
    public sealed class CommonCtrl
    {
        /// <summary>
        ///     Creates and initiates all basic custom controls
        ///     It may sound stupid, but some may throw exceptions because of my own stupidity.
        /// </summary>
        [Test]
        [Apartment(ApartmentState.STA)]
        public void Basic()
        {
            _ = new ColorPicker();
            _ = new ColorPickerMenu();
            _ = new ColorSelection();
            _ = new DataList();
            _ = new ImageZoom();
            _ = new ScrollingTextBoxes { Text = "test" };
            _ = new ScrollingRichTextBox();
            _ = new Thumbnails();
            _ = new NativeBitmapDisplay();

            Assert.Pass();
        }

        /// <summary>
        ///     The exGrid Test.
        /// </summary>
        [Test]
        [Apartment(ApartmentState.STA)]
        public void ExtendedGrids()
        {
            var exGrid = ExtendedGrid.ExtendGrid(4, 2, 92, 92, true);

            // NUnit 4 Syntax: Assert.That(actual, Is.EqualTo(expected))
            Assert.That(exGrid.Height, Is.EqualTo(184), "Test failed Height: " + exGrid.Height);
            Assert.That(exGrid.Width, Is.EqualTo(368), "Test failed Width: " + exGrid.Width);

            Assert.That(ExtendedGrid.Columns, Is.EqualTo(4), "Test failed Columns: " + ExtendedGrid.Columns);
            Assert.That(ExtendedGrid.Rows, Is.EqualTo(2), "Test failed Rows: " + ExtendedGrid.Rows);

            Assert.That(exGrid.ColumnDefinitions.Count, Is.EqualTo(4),
                "Test failed Columns:" + exGrid.ColumnDefinitions.Count);

            Assert.That(exGrid.RowDefinitions.Count, Is.EqualTo(2), "Test failed Rows: " + exGrid.RowDefinitions.Count);

            exGrid = ExtendedGrid.ExtendGrid(1, 2, 92, 92, true);

            Assert.That(exGrid.Height, Is.EqualTo(184), "Test failed Height: " + exGrid.Height);
            Assert.That(exGrid.Width, Is.EqualTo(92), "Test failed Width: " + exGrid.Width);

            Assert.That(ExtendedGrid.Columns, Is.EqualTo(1), "Test failed Columns: " + ExtendedGrid.Columns);
            Assert.That(ExtendedGrid.Rows, Is.EqualTo(2), "Test failed Rows: " + ExtendedGrid.Rows);

            Assert.That(exGrid.ColumnDefinitions.Count, Is.EqualTo(1),
                "Test failed Columns: " + exGrid.ColumnDefinitions.Count);
            Assert.That(exGrid.RowDefinitions.Count, Is.EqualTo(2), "Test failed Rows: " + exGrid.RowDefinitions.Count);
        }

        /// <summary>
        ///     The exGrid simple.
        /// </summary>
        [Test]
        [Apartment(ApartmentState.STA)]
        public void ExtendedGridSimple()
        {
            var exGrid = ExtendedGrid.ExtendGrid(4, 2, true);

            Assert.That(exGrid.Height, Is.EqualTo(200), "Test failed Height: " + exGrid.Height);
            Assert.That(exGrid.Width, Is.EqualTo(400), "Test failed Width: " + exGrid.Width);

            Assert.That(ExtendedGrid.Columns, Is.EqualTo(4), "Test failed Columns: " + ExtendedGrid.Columns);
            Assert.That(ExtendedGrid.Rows, Is.EqualTo(2), "Test failed Rows: " + ExtendedGrid.Rows);

            ExtendedGrid.CellSize = 1;
            exGrid = ExtendedGrid.ExtendGrid(4, 2, true);

            Assert.That(exGrid.Height, Is.EqualTo(2), "Test failed Height: " + exGrid.Height);
            Assert.That(exGrid.Width, Is.EqualTo(4), "Test failed Width: " + exGrid.Width);

            Assert.That(ExtendedGrid.Columns, Is.EqualTo(4), "Test failed Columns: " + ExtendedGrid.Columns);
            Assert.That(ExtendedGrid.Rows, Is.EqualTo(2), "Test failed Rows: " + ExtendedGrid.Rows);
        }

        /// <summary>
        ///     The exGrid custom Variation.
        /// </summary>
        [Test]
        [Apartment(ApartmentState.STA)]
        public void ExtendedGridCustom()
        {
            var lstColumn = new List<int> { 60, 30, 10 };
            var lstRow = new List<int>
            {
                50,
                50,
                30,
                20,
                10
            };

            var exGrid = ExtendedGrid.ExtendGrid(lstColumn, lstRow, true);

            Assert.That(exGrid.ColumnDefinitions.Count, Is.EqualTo(3),
                "Test failed Columns: " + exGrid.ColumnDefinitions.Count);
            Assert.That(exGrid.RowDefinitions.Count, Is.EqualTo(5), "Test failed Rows: " + exGrid.RowDefinitions.Count);

            Assert.That(exGrid.Height, Is.EqualTo(160), "Test failed Height: " + exGrid.Height);
            Assert.That(exGrid.Width, Is.EqualTo(100), "Test failed Width: " + exGrid.Width);
        }

        /// <summary>
        ///     Test some stuff in the WinAPI
        /// </summary>
        [Test]
        public void Win32Api()
        {
            Assert.That((int)Win32Enums.MouseEvents.WmMousemove, Is.EqualTo(512), "checked out");
        }
    }
}
