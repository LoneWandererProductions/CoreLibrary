/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/HelperMethods.cs
 * PURPOSE:     Helper Methods
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using ExtendedSystemObjects;
using Mathematics;

namespace CommonLibraryTests;

/// <summary>
///     The helper methods class.
/// </summary>
internal static class HelperMethods
{
    /// <summary>
    ///     Creates some Dummy Files we will delete
    /// </summary>
    /// <param name="path">target Path</param>
    /// <param name="fileExtList">Extension List</param>
    internal static void CreateFiles(string path, IEnumerable<string> fileExtList)
    {
        if (!Directory.Exists(path))
        {
            _ = Directory.CreateDirectory(path);
        }

        var fileName = 0;

        foreach (var ext in fileExtList)
        {
            fileName++;

            var file = path + Path.DirectorySeparatorChar + fileName + ext;

            if (File.Exists(file))
            {
                continue;
            }

            using var fs = File.Create(file);
            for (byte i = 0; i < 10; i++)
            {
                fs.WriteByte(i);
            }
        }
    }

    /// <summary>
    ///     Create a Dummy Files we will delete
    /// </summary>
    /// <param name="path">target Path</param>
    internal static void CreateFile(string path)
    {
        var folder = Path.GetDirectoryName(path);
        if (folder == null)
            throw new ArgumentException("Path must contain a directory.", nameof(path));

        Directory.CreateDirectory(folder); // safe even if exists

        if (File.Exists(path))
            File.Delete(path);

        using var fs = File.Create(path);
        for (byte i = 0; i < 100; i++)
            fs.WriteByte(i);

        fs.Flush(true); // ensure fully written to disk
    }

    /// <summary>
    ///     Tests one. Matrix Multiplication
    /// </summary>
    /// <param name="mOne">The m one.</param>
    /// <param name="mTwo">The m two.</param>
    /// <returns>Matrix Multiplications</returns>
    internal static BaseMatrix MatrixTestOne(BaseMatrix mOne, BaseMatrix mTwo)
    {
        var h = mOne.Height;
        var w = mTwo.Width;
        var l = mOne.Width;
        var result = new BaseMatrix(h, w);
        var spanOne = mOne.Matrix.ToSpan();
        var spanTwo = mTwo.Matrix.ToSpan();
        var spanResult = result.Matrix.ToSpan();

        for (var i = 0; i < h; i++)
        {
            var iOne = i * l;

            for (var j = 0; j < w; j++)
            {
                var iTwo = j;

                double res = 0;

                for (var k = 0; k < l; k++, iTwo += w)
                {
                    res += spanOne[iOne + k] * spanTwo[iTwo];
                }

                spanResult[(i * w) + j] = res;
            }
        }

        return result;
    }

    /// <summary>
    ///     Bresenham Algorithm to plot a line.
    ///     Implementation based on Wikipedia. So the implementation is considered to be correct
    /// </summary>
    /// <param name="from">From.</param>
    /// <param name="to">To.</param>
    /// <returns>Line between two points.</returns>
    internal static List<Coordinate2D> BresenhamPlotLine(Coordinate2D from, Coordinate2D to)
    {
        if (from.Equals(to))
        {
            return null;
        }

        if (to.X == from.X)
        {
            return null;
        }

        if (Math.Abs(to.Y - from.Y) < Math.Abs(to.X - from.X))
        {
            return from.X > to.X ? PlotLineLow(to, from) : PlotLineLow(from, to);
        }

        return from.Y > to.Y ? PlotLineHigh(to, from) : PlotLineHigh(from, to);
    }

    /// <summary>
    ///     Tests the two.
    /// </summary>
    /// <param name="m1">The m1.</param>
    /// <param name="m2">The m2.</param>
    /// <returns>Base Matrix multiplied</returns>
    internal static BaseMatrix MatrixTestTwo(double[,] m1, double[,] m2)
    {
        var newMatrix = new double[m1.GetLength(0), m2.GetLength(1)];

        for (var i = 0; i < m1.GetLength(0); i++)
        for (var j = 0; j < m2.GetLength(1); j++)
        {
            double sum = 0;

            for (var k = 0; k < m1.GetLength(1); k++)
            {
                sum += m1[i, k] * m2[k, j];
            }

            newMatrix[i, j] = sum;
        }

        return newMatrix;
    }

    /// <summary>
    ///     Plots the line if it is falling.
    /// </summary>
    /// <param name="from">Start coordinate.</param>
    /// <param name="to">End coordinate.</param>
    /// <returns>List of points on the line between both coordinates</returns>
    private static List<Coordinate2D> PlotLineLow(Coordinate2D from, Coordinate2D to)
    {
        var lst = new List<Coordinate2D>();

        var dx = to.X - from.X;
        var dy = to.Y - from.Y;

        var yi = 1;

        if (dy < 0)
        {
            yi = -1;
            dy = -dy;
        }

        var d = (2 * dy) - dx;
        var y = from.Y;

        for (var i = from.X; i <= to.X; i++)
        {
            var point = new Coordinate2D { X = i, Y = y };
            lst.Add(point);

            if (d > 0)
            {
                y += yi;
                d += 2 * (dy - dx);
            }
            else
            {
                d += 2 * dy;
            }
        }

        return lst;
    }

    /// <summary>
    ///     Plots the line if the line is rising.
    /// </summary>
    /// <param name="from">Start coordinate.</param>
    /// <param name="to">End coordinate.</param>
    /// <returns>List of points on the line between both coordinates</returns>
    private static List<Coordinate2D> PlotLineHigh(Coordinate2D from, Coordinate2D to)
    {
        var lst = new List<Coordinate2D>();

        var dx = to.X - from.X;
        var dy = to.Y - from.Y;

        var xi = 1;

        if (dx < 0)
        {
            xi = -1;
            dx = -dx;
        }

        var d = (2 * dx) - dy;
        var x = from.X;

        for (var i = from.Y; i < to.Y; i++)
        {
            var point = new Coordinate2D { X = x, Y = i };
            lst.Add(point);

            if (d > 0)
            {
                x += xi;
                d += 2 * (dx - dy);
            }
            else
            {
                d += 2 * dx;
            }
        }

        return lst;
    }
}
