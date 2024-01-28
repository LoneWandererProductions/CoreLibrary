/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/MathSpeedTests.cs
 * PURPOSE:     Mostly speed tests and some Algorithm's we compare.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using ExtendedSystemObjects;
using Mathematics;

namespace CommonLibraryTests
{
    internal static class MathSpeedTests
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
        /// Bresenham Algorithm to plot a line.
        /// Implementation based on Wikipedia. So the implementation is considered to be correct
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns>Line between two points.</returns>
        internal static List<Coordinate2D> BresenhamPlotLine(Coordinate2D from, Coordinate2D to)
        {
            if (from.Equals(to)) return null;
            if (to.X == from.X) return null;

            if (Math.Abs(to.Y - from.Y) < Math.Abs(to.X - from.X))
            {
                return from.X > to.X ? PlotLineLow(to, from) : PlotLineLow(from, to);
            }

            return from.Y > to.Y ? PlotLineHigh(to, from) : PlotLineHigh(from, to);
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
}
