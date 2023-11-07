﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Wvg
 * FILE:        LightVector/VgProcessing.cs
 * PURPOSE:     Have some Fun with Vectors
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Debugger;
using ExtendedSystemObjects;
using Mathematics;

namespace LightVector
{
    /// <summary>
    ///     The Vector Graphics processing class.
    /// </summary>
    internal static class VgProcessing
    {
        /// <summary>
        ///     Get the vector lines.
        /// </summary>
        /// <param name="container">Image Container</param>
        /// <returns>The <see cref="List{LineObject}" />.</returns>
        internal static List<LineObject> GetVectorLines(SaveContainer container)
        {
            try
            {
                return
                    container.SavedLines.AsParallel().Select(vector => GenerateLine(vector, container.Width)).ToList();
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                {
                    DebugLog.CreateLogFile(string.Concat(WvgResources.ErrorParallelThreading, ex.Message), 0);
                }

                return null;
            }
        }

        /// <summary>
        ///     Get the vector curves.
        /// </summary>
        /// <param name="container">Image Container</param>
        /// <returns>The <see cref="List{CurveObject}" />.</returns>
        internal static List<CurveObject> GetVectorCurves(SaveContainer container)
        {
            try
            {
                return
                    container.SavedCurves.AsParallel().Select(vector => GenerateCurve(vector, container.Width))
                        .ToList();
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                {
                    DebugLog.CreateLogFile(string.Concat(WvgResources.ErrorParallelThreading, ex.Message), 0);
                }

                return null;
            }
        }

        /// <summary>
        ///     The generate lines.
        /// </summary>
        /// <param name="lines">The lines.</param>
        /// <param name="width">length of the Picture</param>
        /// <returns>The <see cref="List{LineVector}" />.</returns>
        internal static List<LineVector> GenerateLines(List<LineObject> lines, int width)
        {
            if (lines.IsNullOrEmpty())
            {
                DebugLog.CreateLogFile(WvgResources.ErrorLineEmpty, 0);
                return null;
            }

            try
            {
                return lines.AsParallel().Select(line => GenerateLine(line, width)).ToList();
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                {
                    DebugLog.CreateLogFile(string.Concat(WvgResources.ErrorParallelThreading, ex.Message), 0);
                }

                return null;
            }
        }

        /// <summary>
        ///     The generate curves.
        /// </summary>
        /// <param name="curves">The curves.</param>
        /// <param name="width">length of the Picture</param>
        /// <returns>The <see cref="List{CurveVector}" />.</returns>
        internal static List<CurveVector> GenerateCurves(List<CurveObject> curves, int width)
        {
            if (curves.IsNullOrEmpty())
            {
                DebugLog.CreateLogFile(WvgResources.ErrorCurveEmpty, 0);
                return null;
            }

            try
            {
                return curves.AsParallel().Select(curve => GenerateCurve(curve, width)).ToList();
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                {
                    DebugLog.CreateLogFile(string.Concat(WvgResources.ErrorParallelThreading, ex.Message), 0);
                }

                return null;
            }
        }

        /// <summary>
        ///     https://docs.microsoft.com/de-de/dotnet/standard/parallel-programming/how-to-handle-exceptions-in-a-plinq-query
        /// </summary>
        /// <param name="lines">The lines.</param>
        /// <param name="factor">Scale Factor</param>
        /// <param name="width">length of the Picture</param>
        internal static List<LineObject> LinesScale(List<LineObject> lines, int factor, int width)
        {
            //no need to do anything
            if (factor == 1)
            {
                return lines;
            }

            if (lines.IsNullOrEmpty())
            {
                DebugLog.CreateLogFile(WvgResources.ErrorEmptyVectorList, 0);
                return null;
            }

            try
            {
                return lines.AsParallel().Select(line => LineScale(line, factor, width)).ToList();
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                {
                    DebugLog.CreateLogFile(string.Concat(WvgResources.ErrorParallelThreading, ex.Message), 0);
                }

                return null;
            }
        }

        /// <summary>
        ///     The curves scale.
        /// </summary>
        /// <param name="curves">The curves.</param>
        /// <param name="factor">Scale Factor</param>
        /// <returns>The <see cref="T:List{CurveObject}" />.</returns>
        internal static List<CurveObject> CurvesScale(List<CurveObject> curves, int factor)
        {
            //no need to do anything
            if (factor == 1)
            {
                return curves;
            }

            if (curves.IsNullOrEmpty())
            {
                DebugLog.CreateLogFile(WvgResources.ErrorEmptyVectorList, 0);
                return curves;
            }

            try
            {
                return curves.AsParallel().Select(curve => CurveScale(curve, factor)).ToList();
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                {
                    DebugLog.CreateLogFile(string.Concat(WvgResources.ErrorParallelThreading, ex.Message), 0);
                }

                return curves;
            }
        }

        /// <summary>
        ///     The lines rotate.
        /// </summary>
        /// <param name="lines">The lines.</param>
        /// <param name="degree">Degree we want to Rotate</param>
        /// <param name="width">length of the Picture</param>
        /// <returns>The <see cref="T:List{LineObject}" />.</returns>
        internal static List<LineObject> LinesRotate(List<LineObject> lines, int degree, int width)
        {
            //no need to do anything
            if (degree is 360 or 0)
            {
                return lines;
            }

            if (lines.IsNullOrEmpty())
            {
                DebugLog.CreateLogFile(WvgResources.ErrorEmptyVectorList, 0);
                return lines;
            }

            try
            {
                return lines.AsParallel().Select(line => GetLineObject(line, degree, width)).ToList();
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                {
                    DebugLog.CreateLogFile(string.Concat(WvgResources.ErrorParallelThreading, ex.Message), 0);
                }

                return lines;
            }
        }

        /// <summary>
        ///     The curves rotate.
        /// </summary>
        /// <param name="curves">The curves.</param>
        /// <param name="degree">Degree we want to Rotate</param>
        /// <returns>The <see cref="T:List{CurveObject}" />.</returns>
        internal static List<CurveObject> CurvesRotate(List<CurveObject> curves, int degree)
        {
            //no need to do anything
            if (degree is 360 or 0)
            {
                return curves;
            }

            if (curves.IsNullOrEmpty())
            {
                DebugLog.CreateLogFile(WvgResources.ErrorEmptyVectorList, 0);
                return null;
            }

            try
            {
                return curves.AsParallel().Select(curve => CurveRotate(curve, degree)).ToList();
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                {
                    DebugLog.CreateLogFile(string.Concat(WvgResources.ErrorParallelThreading, ex.Message), 0);
                }

                return null;
            }
        }

        /// <summary>
        ///     Get the line object.
        ///     https://de.wikipedia.org/wiki/Drehmatrix
        /// </summary>
        /// <param name="line">Line Object</param>
        /// <param name="degree">Degree we want to Rotate</param>
        /// <param name="width">length of the Picture</param>
        /// <returns>The <see cref="LineObject" />.</returns>
        internal static LineObject GetLineObject(LineObject line, int degree, int width)
        {
            //no need to do anything
            if (degree is 360 or 0)
            {
                return line;
            }

            //Convert to Vector Like Format
            var vector = GenerateLine(line, width);

            //We are "clever" we will use pre calculated values to reduce comma errors.
            var cos = ExtendedMath.CalcCos(degree);
            var sin = ExtendedMath.CalcSin(degree);

            var columnX = (int)((vector.ColumnX * cos) - (vector.RowY * sin));
            var rowY = (int)((vector.ColumnX * sin) + (vector.RowY * cos));

            var transVector = new LineVector
            {
                ColumnX = columnX,
                RowY = rowY,
                Fill = vector.Fill,
                Stroke = vector.Stroke,
                StrokeLineJoin = vector.StrokeLineJoin,
                Thickness = vector.Thickness,
                MasterId = vector.MasterId
            };

            //Calculate new Endpoint and return
            return GenerateLine(transVector, width);
        }

        /// <summary>
        ///     https://academo.org/demos/rotation-about-point/
        /// </summary>
        /// <param name="curve">Curve Object</param>
        /// <param name="degree">Degree we want to Rotate</param>
        /// <returns>A curve</returns>
        internal static CurveObject CurveRotate(CurveObject curve, int degree)
        {
            //no need to do anything
            if (degree is 360 or 0)
            {
                return curve;
            }

            //We are "clever" we will use pre calculated values to reduce comma errors.
            var cos = ExtendedMath.CalcCos(degree);
            var sin = ExtendedMath.CalcSin(degree);

            for (var i = 0; i < curve.Points.Count; i++)
            {
                var pnt = curve.Points[i];

                var x = (int)((pnt.X * cos) - (pnt.Y * sin));
                var y = (int)((pnt.Y * cos) + (pnt.X * sin));

                pnt.X = x;
                pnt.Y = y;
                curve.Points[i] = pnt;
            }

            return curve;
        }

        /// <summary>
        ///     The line scale.
        /// </summary>
        /// <param name="line">Line Object</param>
        /// <param name="factor">Scale Factor</param>
        /// <param name="width">length of the Picture</param>
        /// <returns>The <see cref="LineObject" />.</returns>
        internal static LineObject LineScale(LineObject line, int factor, int width)
        {
            //no need to do anything
            if (factor == 1)
            {
                return line;
            }

            //Convert to Vector Like Format
            var vector = GenerateLine(line, width);

            //Scale, possible Division though Null
            vector.ColumnX *= factor;
            vector.RowY *= factor;

            //Calculate new Endpoint and return
            return GenerateLine(vector, width);
        }

        /// <summary>
        ///     The curve scale.
        /// </summary>
        /// <param name="curve">Curve Object</param>
        /// <param name="factor">Scale Factor</param>
        /// <returns>The <see cref="CurveObject" />.</returns>
        internal static CurveObject CurveScale(CurveObject curve, int factor)
        {
            //no need to do anything
            if (factor == 1)
            {
                return curve;
            }

            var points = new List<Point>(curve.Points.Count);

            foreach (var pointer in curve.Points)
            {
                var pnt = pointer;
                pnt.X *= factor;
                pnt.Y *= factor;

                points.Add(pnt);
            }

            curve.Points = points;

            return curve;
        }

        /// <summary>
        ///     The generate line.
        /// </summary>
        /// <param name="mVector">The mVector.</param>
        /// <param name="width">length of the Picture</param>
        /// <returns>The <see cref="LineObject" />.</returns>
        private static LineObject GenerateLine(LineVector mVector, int width)
        {
            var start = IdToPoint(mVector.MasterId, width);

            var end = new Point { X = start.X + mVector.ColumnX, Y = start.Y + mVector.RowY };

            return new LineObject
            {
                StartPoint = start,
                EndPoint = end,
                Fill = mVector.Fill,
                Stroke = mVector.Stroke,
                StrokeLineJoin = mVector.StrokeLineJoin,
                Thickness = mVector.Thickness
            };
        }

        /// <summary>
        ///     The generate curve.
        /// </summary>
        /// <param name="mVector">The mVector.</param>
        /// <param name="width">The width.</param>
        /// <returns>The <see cref="CurveObject" />.</returns>
        private static CurveObject GenerateCurve(CurveVector mVector, int width)
        {
            var points = new List<Point>(mVector.MasterId.Count);

            for (var i = 0; i < mVector.MasterId.Count; i++)
            {
                var pnt = mVector.MasterId[i];
                var start = IdToPoint(pnt, width);

                points[i] = new Point { X = (int)start.X, Y = (int)start.Y };
            }

            return new CurveObject
            {
                Points = points,
                Fill = mVector.Fill,
                Stroke = mVector.Stroke,
                StrokeLineJoin = mVector.StrokeLineJoin,
                Thickness = mVector.Thickness
            };
        }

        /// <summary>
        ///     Example:
        ///     0,1,2,3
        ///     0,  0,1,2,3
        ///     1,  4,5,6,7
        ///     2,  8,9,10,11
        /// </summary>
        /// <param name="vector">Point on the Map</param>
        /// <param name="width">length of the Picture</param>
        /// <returns>Fitting id of the Coordinate</returns>
        private static int CalculateId(Point vector, int width)
        {
            return (int)((vector.Y * width) + vector.X);
        }

        /// <summary>
        ///     Example:
        ///     0,1,2,3
        ///     0,  0,1,2,3
        ///     1,  4,5,6,7
        ///     2,  8,9,10,11
        /// </summary>
        /// <param name="masterId">Point on the Map</param>
        /// <param name="width">length of the Picture</param>
        /// <returns>Fitting Coordinate of the id</returns>
        private static Point IdToPoint(int masterId, int width)
        {
            if (width == 0)
            {
                DebugLog.CreateLogFile(WvgResources.ErrorZeroDivision, 0);
                return new Point();
            }

            var modulo = masterId % width;
            var yColumn = masterId / width;

            return new Point { X = modulo, Y = yColumn };
        }

        /// <summary>
        ///     The generate line.
        /// </summary>
        /// <param name="line">Line Object</param>
        /// <param name="width">length of the Picture</param>
        /// <returns>The <see cref="LineVector" />.</returns>
        private static LineVector GenerateLine(LineObject line, int width)
        {
            return new LineVector
            {
                MasterId = CalculateId(line.StartPoint, width),
                ColumnX = (int)(line.EndPoint.X - line.StartPoint.X),
                RowY = (int)(line.EndPoint.Y - line.StartPoint.Y),
                Fill = line.Fill,
                Stroke = line.Stroke,
                StrokeLineJoin = line.StrokeLineJoin,
                Thickness = line.Thickness
            };
        }

        /// <summary>
        ///     The generate curve.
        /// </summary>
        /// <param name="curve">Curve Object</param>
        /// <param name="width">length of the Picture</param>
        /// <returns>The <see cref="CurveVector" />.</returns>
        private static CurveVector GenerateCurve(CurveObject curve, int width)
        {
            var masterId = new List<int>(curve.Points.Count);

            masterId.AddRange(curve.Points.Select(pnt => CalculateId(pnt, width)));

            return new CurveVector
            {
                MasterId = masterId,
                Fill = curve.Fill,
                Stroke = curve.Stroke,
                StrokeLineJoin = curve.StrokeLineJoin,
                Thickness = curve.Thickness
            };
        }
    }
}
