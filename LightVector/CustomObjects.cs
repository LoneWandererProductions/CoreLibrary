/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/CustomObjects.cs
 * PURPOSE:     Generate specific Graphic Objects
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LightVector
{
    /// <summary>
    ///     The shared class.
    /// </summary>
    internal static class CustomObjects
    {
        /// <summary>
        ///     Make a curve.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="tension">The tension.</param>
        /// <returns>The <see cref="Path" />.</returns>
        internal static Path ReturnBezierCurve(List<Point> points, double tension)
        {
            if (points.Count < 2)
            {
                return null;
            }

            var resultPoints = MakeCurvePoints(points, tension);

            // Use the points to create the path.
            return MakeBezierPath(resultPoints.ToArray());
        }

        /// <summary>
        ///     The make Bezier path.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>The <see cref="Path" />.</returns>
        private static Path MakeBezierPath(IReadOnlyList<Point> points)
        {
            // Create a Path to hold the geometry.
            var path = new Path();

            // Add a PathGeometry.
            var pathGeometry = new PathGeometry();
            path.Data = pathGeometry;

            // Create a PathFigure.
            var pathFigure = new PathFigure();
            pathGeometry.Figures.Add(pathFigure);

            // Start at the first point.
            pathFigure.StartPoint = points[0];

            // Create a PathSegmentCollection.
            var pathSegmentCollection =
                new PathSegmentCollection();
            pathFigure.Segments = pathSegmentCollection;

            // Add the rest of the points to a PointCollection.
            var pointCollection =
                new PointCollection(points.Count - 1);
            for (var i = 1; i < points.Count; i++)
            {
                pointCollection.Add(points[i]);
            }

            // Make a PolyBezierSegment from the points.
            var bezierSegment = new PolyBezierSegment { Points = pointCollection };

            // Add the PolyBezierSegment to other segment collection.
            pathSegmentCollection.Add(bezierSegment);

            return path;
        }

        /// <summary>
        ///     The make curve points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <param name="tension">The tension.</param>
        /// <returns>The <see cref="T:Point[]" />.</returns>
        private static IEnumerable<Point> MakeCurvePoints(IReadOnlyList<Point> points, double tension)
        {
            if (points.Count < 2)
            {
                return null;
            }

            var controlScale = tension / 0.5 * 0.175;

            // Make a list containing the points and
            // appropriate control points.
            var resultPoints = new List<Point> { points[0] };

            for (var i = 0; i < points.Count - 1; i++)
            {
                // Get the point and its neighbors.
                var ptBefore = points[Math.Max(i - 1, 0)];
                var pt = points[i];
                var ptAfter = points[i + 1];
                var ptAfter2 = points[Math.Min(i + 2, points.Count - 1)];

                var p4 = ptAfter;

                var dx = ptAfter.X - ptBefore.X;
                var dy = ptAfter.Y - ptBefore.Y;
                var p2 = new Point(
                    pt.X + (controlScale * dx),
                    pt.Y + (controlScale * dy));

                dx = ptAfter2.X - pt.X;
                dy = ptAfter2.Y - pt.Y;
                var p3 = new Point(
                    ptAfter.X - (controlScale * dx),
                    ptAfter.Y - (controlScale * dy));

                // Save points p2, p3, and p4.
                resultPoints.Add(p2);
                resultPoints.Add(p3);
                resultPoints.Add(p4);
            }

            // Return the points.
            return resultPoints.ToArray();
        }
    }
}
