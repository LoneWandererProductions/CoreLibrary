/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        BezierCurveFactory.cs
 * PURPOSE:     Generate specific Graphic Output Information
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Media;

namespace LightVector
{
    /// <summary>
    ///     Factory for generating Bezier geometries from vector-based points.
    ///     Converts Cardinal Splines (Tension-based) into Cubic Beziers (Control-point based).
    /// </summary>
    internal static class BezierCurveFactory
    {
        /// <summary>
        ///     Generates a Bezier geometry from a list of points with a specified tension.
        /// </summary>
        /// <param name="points">The vector points to define the curve.</param>
        /// <param name="tension">The tension factor (0.0 to 1.0 usually).</param>
        /// <returns>The generated <see cref="Geometry" />.</returns>
        internal static Geometry CreateBezierGeometry(List<Vector2> points, double tension)
        {
            if (points == null || points.Count < 2)
            {
                return Geometry.Empty;
            }

            // 1. Calculate the Cubic Bezier points (Control1, Control2, Endpoint, ...)
            var segmentPoints = GenerateBezierPoints(points, tension);

            // 2. Build the Geometry
            var pathGeometry = new PathGeometry();

            // The first point is the StartPoint of the figure
            var startPoint = new Point(points[0].X, points[0].Y);
            var pathFigure = new PathFigure { StartPoint = startPoint, IsClosed = false };

            // Add the segments (groups of 3 points)
            var polyBezierSegment = new PolyBezierSegment
            {
                Points = new PointCollection(segmentPoints)
            };

            pathFigure.Segments.Add(polyBezierSegment);
            pathGeometry.Figures.Add(pathFigure);

            // Freeze for performance since this geometry won't change after creation
            if (pathGeometry.CanFreeze) pathGeometry.Freeze();

            return pathGeometry;
        }

        /// <summary>
        ///     Generates the flattened list of points [C1, C2, End, C1, C2, End...] for WPF.
        /// </summary>
        private static IEnumerable<Point> GenerateBezierPoints(IReadOnlyList<Vector2> points, double tension)
        {
            // Mathematical constant for smoothing Cardinal Splines
            var controlScale = tension / 0.5 * 0.175;

            var resultPoints = new List<Point>();

            for (var i = 0; i < points.Count - 1; i++)
            {
                var ptBefore = points[Math.Max(i - 1, 0)];
                var ptCurrent = points[i];
                var ptAfter = points[i + 1];
                var ptAfter2 = points[Math.Min(i + 2, points.Count - 1)];

                // Calculate Control Point 1 (p2)
                // Tangent based on previous and next point
                var dx1 = ptAfter.X - ptBefore.X;
                var dy1 = ptAfter.Y - ptBefore.Y;
                var p2 = new Point(
                    ptCurrent.X + (controlScale * dx1),
                    ptCurrent.Y + (controlScale * dy1));

                // Calculate Control Point 2 (p3)
                // Tangent based on current and next-next point
                var dx2 = ptAfter2.X - ptCurrent.X;
                var dy2 = ptAfter2.Y - ptCurrent.Y;
                var p3 = new Point(
                    ptAfter.X - (controlScale * dx2),
                    ptAfter.Y - (controlScale * dy2));

                // WPF PolyBezierSegment requires exactly 3 points per segment:
                // 1. Control Point 1
                // 2. Control Point 2
                // 3. End Point
                resultPoints.Add(p2);
                resultPoints.Add(p3);
                resultPoints.Add(new Point(ptAfter.X, ptAfter.Y)); // <-- THIS WAS MISSING
            }

            return resultPoints;
        }
    }
}
