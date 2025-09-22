/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGuiTests
 * FILE:        CommonLibraryGuiTests/CommonCtrl.cs
 * PURPOSE:     Tests for CommonCtrl some Controls, not all yet
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedVariable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CommonControls;
using Imaging;
using InterOp;
using NUnit.Framework;
using RenderEngine;

namespace CommonLibraryGuiTests;

/// <summary>
///     The common Controls unit test class.
/// </summary>
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

        try
        {
            _ = new OpenTkControl();
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
        }

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

        Assert.AreEqual(184, exGrid.Height, "Test failed Height: " + exGrid.Height);
        Assert.AreEqual(368, exGrid.Width, "Test failed Width: " + exGrid.Width);

        Assert.AreEqual(4, ExtendedGrid.Columns, "Test failed Columns: " + ExtendedGrid.Columns);
        Assert.AreEqual(2, ExtendedGrid.Rows, "Test failed Rows: " + ExtendedGrid.Rows);

        Assert.AreEqual(4, exGrid.ColumnDefinitions.Count,
            "Test failed Columns:" + exGrid.ColumnDefinitions.Count);

        Assert.AreEqual(2, exGrid.RowDefinitions.Count, "Test failed Rows: " + exGrid.RowDefinitions.Count);

        exGrid = ExtendedGrid.ExtendGrid(1, 2, 92, 92, true);

        Assert.AreEqual(184, exGrid.Height, "Test failed Height: " + exGrid.Height);
        Assert.AreEqual(92, exGrid.Width, "Test failed Width: " + exGrid.Width);

        Assert.AreEqual(1, ExtendedGrid.Columns, "Test failed Columns: " + ExtendedGrid.Columns);
        Assert.AreEqual(2, ExtendedGrid.Rows, "Test failed Rows: " + ExtendedGrid.Rows);

        Assert.AreEqual(1, exGrid.ColumnDefinitions.Count,
            "Test failed Columns: " + exGrid.ColumnDefinitions.Count);
        Assert.AreEqual(2, exGrid.RowDefinitions.Count, "Test failed Rows: " + exGrid.RowDefinitions.Count);
    }

    /// <summary>
    ///     The exGrid simple.
    /// </summary>
    [Test]
    [Apartment(ApartmentState.STA)]
    public void ExtendedGridSimple()
    {
        var exGrid = ExtendedGrid.ExtendGrid(4, 2, true);

        Assert.AreEqual(200, exGrid.Height, "Test failed Height: " + exGrid.Height);
        Assert.AreEqual(400, exGrid.Width, "Test failed Width: " + exGrid.Width);

        Assert.AreEqual(4, ExtendedGrid.Columns, "Test failed Columns: " + ExtendedGrid.Columns);
        Assert.AreEqual(2, ExtendedGrid.Rows, "Test failed Rows: " + ExtendedGrid.Rows);

        ExtendedGrid.CellSize = 1;
        exGrid = ExtendedGrid.ExtendGrid(4, 2, true);

        Assert.AreEqual(2, exGrid.Height, "Test failed Height: " + exGrid.Height);
        Assert.AreEqual(4, exGrid.Width, "Test failed Width: " + exGrid.Width);

        Assert.AreEqual(4, ExtendedGrid.Columns, "Test failed Columns: " + ExtendedGrid.Columns);
        Assert.AreEqual(2, ExtendedGrid.Rows, "Test failed Rows: " + ExtendedGrid.Rows);
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

        Assert.AreEqual(3, exGrid.ColumnDefinitions.Count,
            "Test failed Columns: " + exGrid.ColumnDefinitions.Count);
        Assert.AreEqual(5, exGrid.RowDefinitions.Count, "Test failed Rows: " + exGrid.RowDefinitions.Count);

        Assert.AreEqual(160, exGrid.Height, "Test failed Height: " + exGrid.Height);
        Assert.AreEqual(100, exGrid.Width, "Test failed Width: " + exGrid.Width);
    }

    /// <summary>
    ///     Test some stuff in the WinAPI
    /// </summary>
    [Test]
    public void Win32Api()
    {
        //TODO Test in Live environment
        Assert.AreNotEqual(512, Win32Enums.MouseEvents.WmMousemove, "checked out");
    }
}
