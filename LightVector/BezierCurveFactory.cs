/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/BezierCurveFactory.cs
 * PURPOSE:     Generate specific Graphic Output Information
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LightVector
{
    /// <summary>
    ///     Factory for generating Bezier curves from vector-based points.
    /// </summary>
    internal static class BezierCurveFactory
    {
        /// <summary>
        ///     Generates a Bezier curve from a list of points with a specified tension.
        /// </summary>
        /// <param name="points">The vector points to define the curve.</param>
        /// <param name="tension">The tension factor to adjust the curve's shape.</param>
        /// <returns>The generated <see cref="Path" /> representing the Bezier curve.</returns>
        internal static Path CreateBezierCurve(List<Vector2> points, double tension)
        {
            if (points == null || points.Count < 2)
            {
                throw new ArgumentException("At least two points are required to create a Bezier curve.");
            }

            var resultPoints = GenerateCurvePoints(points, tension);
            return BuildBezierPath(resultPoints.ToArray());
        }

        /// <summary>
        ///     Builds a Bezier path using the calculated points.
        /// </summary>
        /// <param name="points">The calculated points defining the curve.</param>
        /// <returns>The generated <see cref="Path" /> for the Bezier curve.</returns>
        private static Path BuildBezierPath(IReadOnlyList<Point> points)
        {
            var path = new Path();
            var pathGeometry = new PathGeometry();
            path.Data = pathGeometry;

            var pathFigure = new PathFigure { StartPoint = points[0] };
            pathGeometry.Figures.Add(pathFigure);

            var pathSegmentCollection = new PathSegmentCollection();
            pathFigure.Segments = pathSegmentCollection;

            var pointCollection = new PointCollection();
            for (var i = 1; i < points.Count; i++)
            {
                pointCollection.Add(points[i]);
            }

            var bezierSegment = new PolyBezierSegment { Points = pointCollection };
            pathSegmentCollection.Add(bezierSegment);

            return path;
        }

        /// <summary>
        ///     Generates the points for the Bezier curve based on the input points and tension.
        /// </summary>
        /// <param name="points">The input vector points.</param>
        /// <param name="tension">The tension factor to modify the curve.</param>
        /// <returns>The calculated control points for the curve.</returns>
        private static IEnumerable<Point> GenerateCurvePoints(IReadOnlyList<Vector2> points, double tension)
        {
            if (points.Count < 2)
            {
                return Enumerable.Empty<Point>();
            }

            var controlScale = tension / 0.5 * 0.175;
            var resultPoints = new List<Point> { new(points[0].X, points[0].Y) };

            for (var i = 0; i < points.Count - 1; i++)
            {
                var ptBefore = points[Math.Max(i - 1, 0)];
                var pt = points[i];
                var ptAfter = points[i + 1];
                var ptAfter2 = points[Math.Min(i + 2, points.Count - 1)];

                var dx = ptAfter.X - ptBefore.X;
                var dy = ptAfter.Y - ptBefore.Y;
                var p2 = new Point(pt.X + (controlScale * dx), pt.Y + (controlScale * dy));

                dx = ptAfter2.X - pt.X;
                dy = ptAfter2.Y - pt.Y;

                var p3 = new Point(ptAfter.X - (controlScale * dx), ptAfter.Y - (controlScale * dy));

                resultPoints.Add(p2);
                resultPoints.Add(p3);
            }

            // No need to add the last point (p4) since it's already included in the original list of points
            resultPoints.Add(new Point(points.Last().X, points.Last().Y));

            return resultPoints;
        }
    }
}
