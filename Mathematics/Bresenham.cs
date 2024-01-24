/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/Bresenham.cs
 * PURPOSE:     Implementation of the Bresenham Algorithm, Helps drawing lines.
 *              Alternative Wu's Algorithm TODO
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Source:      https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
 *              https://en.wikipedia.org/wiki/Xiaolin_Wu%27s_line_algorithm
 */

using System;
using System.Collections.Generic;

namespace Mathematics
{
    /// <summary>
    ///     Bresenham Implementation
    /// </summary>
    public static class Bresenham
    {
        /// <summary>
        ///     Plots the line.
        /// </summary>
        /// <param name="from">Start coordinate.</param>
        /// <param name="to">End coordinate.</param>
        /// <returns>List of points on the line between both coordinates</returns>
        public static List<Coordinate2D> PlotLine(Coordinate2D from, Coordinate2D to)
        {
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
