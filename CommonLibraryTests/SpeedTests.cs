/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/SpeedTests.cs
 * PURPOSE:     Mostly speed tests and some Algorithm's we compare.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using DataFormatter;
using ExtendedSystemObjects;
using Imaging;
using Mathematics;

namespace CommonLibraryTests
{
    internal static class SpeedTests
    {
        /// <summary>
        ///     Tests one. Matrix Multiplication
        /// </summary>
        /// <param name="mOne">The m one.</param>
        /// <param name="mTwo">The m two.</param>
        /// <returns>Matrix Multiplications</returns>
        internal static BaseMatrix TestOne(BaseMatrix mOne, BaseMatrix mTwo)
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
        ///     Gets the cif.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        internal static Cif GetCif(string path)
        {
            var csv = CsvHandler.ReadCsv(path, ImagingResources.Separator);

            int height = 0, width = 0;

            var compressed = GetInfo(csv[0], ref height, ref width);

            var cif = new Cif
            {
                Height = height,
                Width = width,
                Compressed = false,
                CifImage = new Dictionary<Color, SortedSet<int>>()
            };

            if (compressed == true)
            {
                Parallel.ForEach(csv, line =>
                {
                    var hex = line[0];

                    var check = int.TryParse(line[1], out var a);

                    if (!check)
                    {
                        return;
                    }

                    var converter = new ColorHsv(hex, a);

                    var color = Color.FromArgb((byte)converter.A, (byte)converter.R, (byte)converter.G,
                        (byte)converter.B);

                    //get coordinates
                    for (var i = 2; i < line.Count; i++)
                    {
                        if (line[i].Contains(ImagingResources.IntervalSplitter))
                        {
                            //split get start and end
                            var lst = line[i].Split(ImagingResources.CifSeparator).ToList();

                            var sequence = GetStartEndPoint(lst);

                            if (sequence == null)
                            {
                                continue;
                            }

                            //paint area
                            for (var idMaster = sequence.Start; idMaster <= sequence.End; idMaster++)
                            {
                                cif.CifImage.Add(color, idMaster);
                            }
                        }
                        else
                        {
                            check = int.TryParse(line[i], out var idMaster);

                            if (!check)
                            {
                                continue;
                            }

                            cif.CifImage.Add(color, idMaster);
                        }
                    }
                });
            }
            else
            {
                Parallel.ForEach(csv, line =>
                {
                    var hex = line[0];

                    var check = int.TryParse(line[1], out var a);

                    if (!check)
                    {
                        return;
                    }

                    var converter = new ColorHsv(hex, a);

                    var color = Color.FromArgb((byte)converter.A, (byte)converter.R, (byte)converter.G,
                        (byte)converter.B);

                    //get coordinates
                    for (var i = 2; i < line.Count; i++)
                    {
                        check = int.TryParse(line[i], out var idMaster);
                        if (!check)
                        {
                            continue;
                        }

                        cif.CifImage.Add(color, idMaster);
                    }
                });
            }

            return cif;
        }

        /// <summary>
        ///     Tests the two.
        /// </summary>
        /// <param name="m1">The m1.</param>
        /// <param name="m2">The m2.</param>
        /// <returns>Base Matrix multiplied</returns>
        internal static BaseMatrix TestTwo(double[,] m1, double[,] m2)
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

        /// <summary>
        ///     Gets the information.
        /// </summary>
        /// <param name="csv">The CSV.</param>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <returns>All needed values for our Image</returns>
        private static bool? GetInfo(IReadOnlyList<string> csv, ref int height, ref int width)
        {
            //get image size
            var check = int.TryParse(csv[0], out var h);
            if (!check)
            {
                return null;
            }

            height = h;

            check = int.TryParse(csv[1], out var w);
            if (!check)
            {
                return null;
            }

            width = w;

            return csv[2] == ImagingResources.CifCompressed;
        }

        /// <summary>
        ///     Gets the start end point.
        /// </summary>
        /// <param name="lst">The LST.</param>
        /// <returns>start and End Point as Tuple</returns>
        [return: MaybeNull]
        private static CifProcessing.StartEndPoint? GetStartEndPoint(IReadOnlyList<string> lst)
        {
            var check = int.TryParse(lst[0], out var start);
            if (!check)
            {
                return null;
            }

            check = int.TryParse(lst[1], out var end);

            return !check ? null : new CifProcessing.StartEndPoint(start, end);
        }
    }
}
