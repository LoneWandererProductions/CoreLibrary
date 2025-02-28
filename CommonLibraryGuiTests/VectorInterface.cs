using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Windows;
using LightVector;
using NUnit.Framework;
using static LightVector.LineObject;

/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryVectorTests
 * FILE:        VectorInterface.cs
 * PURPOSE:     Test our rather simple Vector Graphics library
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace CommonLibraryGuiTests
{
    /// <summary>
    ///     The vector interface unit test class.
    /// </summary>
    public sealed class VectorInterface
    {
        /// <summary>
        ///     The width (const). Value: 100.
        /// </summary>
        private const int Width = 100;

        private const int Height = 100;

        /// <summary>
        ///     The Vector Interface.
        /// </summary>
        private static Vectors _vector;

        /// <summary>
        ///     The path
        /// </summary>
        private static readonly string Path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "test.obj");

        /// <summary>
        ///     Test Increase Length of Vector
        /// </summary>
        [Test]
        public void InterfaceRotate()
        {
            // Create a new vector with height and width (assuming this is just for initialization).
            _vector = new Vectors(Height, Width);

            // Define start and end points using Vector2.
            var startPoint = new Vector2(0, 0);
            var endPoint = new Vector2(0, 1);

            // Create the line object and initialize it with start and end points.
            var line = new LineObject
            {
                StartPoint = startPoint,
                EndPoint = endPoint
            };

            // Add the line to the vector object.
            _vector.LineAdd(line);

            // Apply rotation of 90 degrees to the vector.
            _vector.Rotate(90);

            // Get the first line from the vector after rotation.
            var rotatedLine = _vector.Lines[0];

            // Get the expected rotated line based on the manual rotation calculation.
            var lineAfterRotation = GetRotationLine(startPoint, endPoint, 90);

            // Assert that the start point and end point are correct after rotation.
            Assert.AreEqual(lineAfterRotation.StartPoint.X, rotatedLine.StartPoint.X, "StartPoint X not correct: 1");
            Assert.AreEqual(lineAfterRotation.StartPoint.Y, rotatedLine.StartPoint.Y, "StartPoint Y not correct: 2");

            Assert.AreEqual(lineAfterRotation.EndPoint.X, rotatedLine.EndPoint.X, "EndPoint X not correct: 3");
            Assert.AreEqual(lineAfterRotation.EndPoint.Y, rotatedLine.EndPoint.Y, "EndPoint Y not correct: 4");
        }


        /// <summary>
        ///     Test Increase Length of Vector
        /// </summary>
        [Test]
        public void ScaleLines()
        {
            var startPoint = new Vector2 { X = 0, Y = 0 };

            var endPoint = new Vector2 { X = 1, Y = 1 };

            var line = GetScaleLine(startPoint, endPoint, 5);

            Assert.AreEqual(0, line.StartPoint.X, "StartPoint X not correct");
            Assert.AreEqual(0, line.StartPoint.Y, "StartPoint Y not correct");
            Assert.AreEqual(5, line.EndPoint.X, "EndPoint X not correct");
            Assert.AreEqual(5, line.EndPoint.Y, "EndPoint Y not correct");

            startPoint = new Vector2 { X = 2, Y = 2 };

            endPoint = new Vector2 { X = 4, Y = 4 };

            line = GetScaleLine(startPoint, endPoint, 3);

            Assert.AreEqual(2, line.StartPoint.X, "StartPoint X not correct");
            Assert.AreEqual(2, line.StartPoint.Y, "StartPoint Y not correct");
            Assert.AreEqual(8, line.EndPoint.X, "EndPoint X not correct");
            Assert.AreEqual(8, line.EndPoint.Y, 18, "EndPoint Y not correct");
        }

        /// <summary>
        ///     Test Increase Length of Vector
        /// </summary>
        [Test]
        public void ScaleCurves()
        {
            var startPoint = new Vector2 { X = 0, Y = 0 };

            var endPoint = new Vector2 { X = 1, Y = 1 };

            var curve = GetScaleCurve(startPoint, endPoint, 5);

            Assert.AreEqual(0, curve.Vectors[0].X, "Point X not correct");
            Assert.AreEqual(0, curve.Vectors[0].Y, "Point Y not correct");
            Assert.AreEqual(5, curve.Vectors[1].X, "Point X not correct");
            Assert.AreEqual(5, curve.Vectors[1].Y, "Point Y not correct");
        }

        /// <summary>
        ///     Test Rotation of Vector
        /// </summary>
        [Test]
        public void RotateLines()
        {
            var startPoint = new Vector2 { X = 0, Y = 0 };

            var endPoint = new Vector2 { X = 0, Y = 1 };

            var line = GetRotationLine(startPoint, endPoint, 90);

            Assert.AreEqual(0, line.StartPoint.X, "StartPoint X not correct");
            Assert.AreEqual(0, line.StartPoint.Y, "StartPoint Y not correct");
            Assert.AreEqual(-1, line.EndPoint.X, "EndPoint X not correct");
            Assert.AreEqual(0, line.EndPoint.Y, "EndPoint Y not correct");

            line = GetRotationLine(startPoint, endPoint, -90);

            Assert.AreEqual(0, line.StartPoint.X, "StartPoint X not correct");
            Assert.AreEqual(0, line.StartPoint.Y, "StartPoint Y not correct");
            Assert.AreEqual(1, line.EndPoint.X, "EndPoint X not correct");
            Assert.AreEqual(0, line.EndPoint.Y, "EndPoint Y not correct");

            line = GetRotationLine(startPoint, endPoint, 180);

            Assert.AreEqual(0, line.StartPoint.X, "StartPoint X not correct");
            Assert.AreEqual(0, line.StartPoint.Y, "StartPoint Y not correct");
            Assert.AreEqual(0, line.EndPoint.X, "EndPoint X not correct");
            Assert.AreEqual(-1, line.EndPoint.Y, "EndPoint Y not correct");

            line = GetRotationLine(startPoint, endPoint, 270);

            Assert.AreEqual(0, line.StartPoint.X, "StartPoint X not correct");
            Assert.AreEqual(0, line.StartPoint.Y, "StartPoint Y not correct");
            Assert.AreEqual(1, line.EndPoint.X, "EndPoint X not correct");
            Assert.AreEqual(0, line.EndPoint.Y, "EndPoint Y not correct");

            line = GetRotationLine(startPoint, endPoint, 360);

            Assert.AreEqual(0, line.StartPoint.X, "StartPoint X not correct");
            Assert.AreEqual(0, line.StartPoint.Y, "StartPoint Y not correct");
            Assert.AreEqual(0, line.EndPoint.X, "EndPoint X not correct");
            Assert.AreEqual(1, line.EndPoint.Y, "EndPoint Y not correct");

            startPoint = new Vector2 { X = 0, Y = 0 };

            endPoint = new Vector2 { X = 0, Y = 2 };

            line = GetRotationLine(startPoint, endPoint, 90);

            Assert.AreEqual(0, line.StartPoint.X, "StartPoint X not correct");
            Assert.AreEqual(0, line.StartPoint.Y, "StartPoint Y not correct");
            Assert.AreEqual(-2, line.EndPoint.X, "EndPoint X not correct");
            Assert.AreEqual(0, line.EndPoint.Y, "EndPoint Y not correct");

            startPoint = new Vector2 { X = 1, Y = 1 };

            endPoint = new Vector2 { X = 1, Y = 3 };

            line = GetRotationLine(startPoint, endPoint, 90);

            Assert.AreEqual(1, line.StartPoint.X, "StartPoint X not correct");
            Assert.AreEqual(1, line.StartPoint.Y, "StartPoint Y not correct");
            Assert.AreEqual(-1, line.EndPoint.X, "EndPoint X not correct");
            Assert.AreEqual(1, line.EndPoint.Y, "EndPoint Y not correct");
        }

        /// <summary>
        ///     Test Rotation of Vector
        /// </summary>
        [Test]
        public void RotateCurves()
        {
            var startPoint = new Vector2 { X = 0, Y = 0 };

            var endPoint = new Vector2 { X = 0, Y = 1 };

            var curve = GetRotationCurve(startPoint, endPoint, 90);

            Assert.AreEqual(0, curve.Vectors[0].X, "Point X not correct");
            Assert.AreEqual(0, curve.Vectors[0].Y, "Point Y not correct");
            Assert.AreEqual(-1, curve.Vectors[1].X, "Point X not correct");
            Assert.AreEqual(0, curve.Vectors[1].Y, "Point Y not correct");

            curve = GetRotationCurve(startPoint, endPoint, -90);

            Assert.AreEqual(0, curve.Vectors[0].X, "StartPoint X not correct");
            Assert.AreEqual(0, curve.Vectors[0].Y, "StartPoint Y not correct");
            Assert.AreEqual(1, curve.Vectors[1].X, "EndPoint X not correct");
            Assert.AreEqual(0, curve.Vectors[1].Y, "EndPoint Y not correct");
        }

        /// <summary>
        ///     Test Rotation of Vector
        /// </summary>
        [Test]
        public void AlgorithmLines()
        {
            var startPoint = new Vector2 { X = 0, Y = 0 };

            var endPoint = new Vector2 { X = 0, Y = 1 };

            const int degree = -90;
            const double rad = Math.PI / 180.0;
            var cos = Math.Cos(degree * rad);
            var sin = Math.Sin(degree * rad);

            var columnX = endPoint.X - startPoint.X;
            var rowY = endPoint.Y - startPoint.Y;

            var columnEnd = (int)((columnX * cos) - (rowY * sin));
            var rowEnd = (int)((columnX * sin) + (rowY * cos));

            var line = GetRotationLine(startPoint, endPoint, -90);

            Assert.AreEqual(0, line.StartPoint.X, "StartPoint X not correct");
            Assert.AreEqual(0, line.StartPoint.Y, "StartPoint Y not correct");
            Assert.AreEqual(1, line.EndPoint.X, "EndPoint X not correct");
            Assert.AreEqual(0, line.EndPoint.Y, "EndPoint Y not correct");

            //compare to checked Results
            Assert.AreEqual(columnEnd, line.EndPoint.X, "Algorithm EndPoint X not correct");
            Assert.AreEqual(rowEnd, line.EndPoint.Y, "Algorithm EndPoint Y not correct");
        }

        /// <summary>
        ///     Get the scale curve.
        /// </summary>
        /// <param name="startPoint">Start Point Vector</param>
        /// <param name="endPoint">End Point Vector</param>
        /// <param name="factor">Scale Factor</param>
        /// <returns>The <see cref="CurveObject" />Changed Vector.</returns>
        private static CurveObject GetScaleCurve(Vector2 startPoint, Vector2 endPoint, int factor)
        {
            var points = new List<Vector2>(2) { startPoint, endPoint };
            var curve = new CurveObject { Vectors = points };

            return VgProcessing.CurveScale(curve, factor);
        }

        /// <summary>
        ///     Get the scale line.
        /// </summary>
        /// <param name="startPoint">Start Point Vector</param>
        /// <param name="endPoint">End Point Vector</param>
        /// <param name="factor">Scale Factor</param>
        /// <returns>The <see cref="LineObject" />Changed Vector.</returns>
        private static LineObject GetScaleLine(Vector2 startPoint, Vector2 endPoint, int factor)
        {
            var trsLine = new LineObject { StartPoint = startPoint, EndPoint = endPoint };

            return VgProcessing.LineScale(trsLine, factor, Width);
        }

        /// <summary>
        ///     Only Lines this time
        /// </summary>
        /// <param name="startPoint">Start Point Vector</param>
        /// <param name="endPoint">End Point Vector</param>
        /// <param name="degree">Degrees</param>
        /// <returns>Changed Vector</returns>
        private static LineObject GetRotationLine(Vector2 startPoint, Vector2 endPoint, int degree)
        {
            var trsLine = new LineObject { StartPoint = startPoint, EndPoint = endPoint };

            return VgProcessing.LineRotate(trsLine, degree, Width);
        }

        /// <summary>
        ///     Only Curves this Time
        /// </summary>
        /// <param name="startPoint">Start Point Vector</param>
        /// <param name="endPoint">End Point Vector</param>
        /// <param name="degree">Degrees</param>
        /// <returns>Changed Vector</returns>
        private static CurveObject GetRotationCurve(Vector2 startPoint, Vector2 endPoint, int degree)
        {
            var points = new List<Vector2>(2) { startPoint, endPoint };
            var curve = new CurveObject { Vectors = points };

            return VgProcessing.CurveRotate(curve, degree);
        }
    }
}
