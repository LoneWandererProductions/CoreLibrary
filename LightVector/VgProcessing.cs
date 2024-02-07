/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Wvg
 * FILE:        LightVector/VgProcessing.cs
 * PURPOSE:     Have some Fun with Vectors
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using DataFormatter;
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
                return (from graphic in container.Objects
                    where graphic.Type == VectorObjects.Line
                    select graphic.Graphic as LineObject).ToList();
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                {
                    Trace.WriteLine(string.Concat(WvgResources.ErrorParallelThreading, ex.Message));
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
                return (from graphic in container.Objects
                    where graphic.Type == VectorObjects.Curve
                    select graphic.Graphic as CurveObject).ToList();
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                {
                    Trace.WriteLine(string.Concat(WvgResources.ErrorParallelThreading, ex.Message));
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
                Trace.WriteLine(WvgResources.ErrorEmptyVectorList);
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
                    Trace.WriteLine(string.Concat(WvgResources.ErrorParallelThreading, ex.Message));
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
                Trace.WriteLine(WvgResources.ErrorEmptyVectorList);
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
                    Trace.WriteLine(string.Concat(WvgResources.ErrorParallelThreading, ex.Message));
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
                Trace.WriteLine(WvgResources.ErrorEmptyVectorList);
                return lines;
            }

            try
            {
                return lines.AsParallel().Select(line => LineRotate(line, degree, width)).ToList();
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                {
                    Trace.WriteLine(string.Concat(WvgResources.ErrorParallelThreading, ex.Message));
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
                Trace.WriteLine(WvgResources.ErrorEmptyVectorList);
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
                    Trace.WriteLine(string.Concat(WvgResources.ErrorParallelThreading, ex.Message));
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
        internal static LineObject LineRotate(LineObject line, int degree, int width)
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

        internal static Polygons CreatePolygon(ObjFile objFile, Vector3D translation, int angleX, int angleY,
            int angleZ, int scale, int width, int height)
        {
            var projection = new Projection();
            var transform = new Transform();
            var lst = projection.GenerateMesh(objFile, transform, height, width);

            var cache = new List<Vector3D>();

            for (var i = 0; i < lst.Count; i += 2)
            {
                var one = lst[i];
                var two = lst[i + 1];

                var line = Lines.LinearLine(one, two);

                cache.AddRange(line);
            }

            var points = cache.ConvertAll(point => point.ToPoint());

            return new Polygons { Points = points };
        }

        private static Vector3D Convert(TertiaryVector triangle, Vector3D translateVector, int angleX, int angleY,
            int angleZ, int scale)
        {
            var start = new Vector3D { X = triangle.X, Y = triangle.Y, Z = triangle.Z };

            return null; //Projection3DCamera.WorldMatrix(start, translateVector, angleX, angleY, angleZ, scale);
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
                StartPoint = start, EndPoint = end
                //Fill = mVector.Fill,
                //Stroke = mVector.Stroke,
                //StrokeLineJoin = mVector.StrokeLineJoin,
                //Thickness = mVector.Thickness
            };
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
                RowY = (int)(line.EndPoint.Y - line.StartPoint.Y)
                //Fill = line.Fill,
                //Stroke = line.Stroke,
                //StrokeLineJoin = line.StrokeLineJoin,
                //Thickness = line.Thickness
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
                Trace.WriteLine(WvgResources.ErrorZeroDivision);
                return new Point();
            }

            var modulo = masterId % width;
            var yColumn = masterId / width;

            return new Point { X = modulo, Y = yColumn };
        }
    }
}
