using System;
using System.Collections.Generic;
using System.Windows;
using LightVector;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryVectorTests
 * FILE:        VectorInterface.cs
 * PURPOSE:     Test our rather simple Vector Graphics library
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace CommonLibraryVectorTests
{
    /// <summary>
    ///     The vector interface unit test class.
    /// </summary>
    [TestClass]
    public sealed class VectorInterface
    {
        /// <summary>
        ///     The width (const). Value: 100.
        /// </summary>
        private const int Width = 100;

        /// <summary>
        ///     The Vector Interface.
        /// </summary>
        private static Vectors _vctr;

        /// <summary>
        ///     Test Increase Length of Vector
        /// </summary>
        [TestMethod]
        public void InterfaceRotate()
        {
            _vctr = new Vectors(Width);

            var startPoint = new Point {X = 0, Y = 0};

            var endPoint = new Point {X = 0, Y = 1};

            var lne = new LineObject
            {
                StartPoint = new Point(Convert.ToInt32(startPoint.X), Convert.ToInt32(startPoint.Y)),
                EndPoint = new Point(Convert.ToInt32(endPoint.X), Convert.ToInt32(endPoint.Y))
            };

            _vctr.LineAdd(lne);

            _vctr.Rotate(90);

            var lineOne = _vctr.Lines[0];
            var lineTwo = GetRotationLine(startPoint, endPoint, 90);

            Assert.AreEqual(lineTwo.StartPoint.X, lineOne.StartPoint.X, "StartPoint X not correct: 1");
            Assert.AreEqual(lineTwo.StartPoint.Y, lineOne.StartPoint.Y, "StartPoint Y not correct: 2");

            //We don't get the results
            Assert.AreEqual(lineTwo.EndPoint.X, lineOne.EndPoint.X, "EndPoint X not correct: 3");
            Assert.AreEqual(lineTwo.EndPoint.Y, lineOne.EndPoint.Y, "EndPoint Y not correct: 4");
        }

        /// <summary>
        ///     Test Increase Length of Vector
        /// </summary>
        [TestMethod]
        public void ScaleLines()
        {
            var startPoint = new Point {X = 0, Y = 0};

            var endPoint = new Point {X = 1, Y = 1};

            var line = GetScaleLine(startPoint, endPoint, 5);

            Assert.AreEqual(0, line.StartPoint.X, "StartPoint X not correct");
            Assert.AreEqual(0, line.StartPoint.Y, "StartPoint Y not correct");
            Assert.AreEqual(5, line.EndPoint.X, "EndPoint X not correct");
            Assert.AreEqual(5, line.EndPoint.Y, "EndPoint Y not correct");

            startPoint = new Point {X = 2, Y = 2};

            endPoint = new Point {X = 4, Y = 4};

            line = GetScaleLine(startPoint, endPoint, 3);

            Assert.AreEqual(2, line.StartPoint.X, "StartPoint X not correct");
            Assert.AreEqual(2, line.StartPoint.Y, "StartPoint Y not correct");
            Assert.AreEqual(8, line.EndPoint.X, "EndPoint X not correct");
            Assert.AreEqual(8, line.EndPoint.Y, 18, "EndPoint Y not correct");
        }

        /// <summary>
        ///     Test Increase Length of Vector
        /// </summary>
        [TestMethod]
        public void ScaleCurves()
        {
            var startPoint = new Point {X = 0, Y = 0};

            var endPoint = new Point {X = 1, Y = 1};

            var curve = GetScaleCurve(startPoint, endPoint, 5);

            Assert.AreEqual(0, curve.Points[0].X, "Point X not correct");
            Assert.AreEqual(0, curve.Points[0].Y, "Point Y not correct");
            Assert.AreEqual(5, curve.Points[1].X, "Point X not correct");
            Assert.AreEqual(5, curve.Points[1].Y, "Point Y not correct");
        }

        /// <summary>
        ///     Test Rotation of Vector
        /// </summary>
        [TestMethod]
        public void RotateLines()
        {
            var startPoint = new Point {X = 0, Y = 0};

            var endPoint = new Point {X = 0, Y = 1};

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

            startPoint = new Point {X = 0, Y = 0};

            endPoint = new Point {X = 0, Y = 2};

            line = GetRotationLine(startPoint, endPoint, 90);

            Assert.AreEqual(0, line.StartPoint.X, "StartPoint X not correct");
            Assert.AreEqual(0, line.StartPoint.Y, "StartPoint Y not correct");
            Assert.AreEqual(-2, line.EndPoint.X, "EndPoint X not correct");
            Assert.AreEqual(0, line.EndPoint.Y, "EndPoint Y not correct");

            startPoint = new Point {X = 1, Y = 1};

            endPoint = new Point {X = 1, Y = 3};

            line = GetRotationLine(startPoint, endPoint, 90);

            Assert.AreEqual(1, line.StartPoint.X, "StartPoint X not correct");
            Assert.AreEqual(1, line.StartPoint.Y, "StartPoint Y not correct");
            Assert.AreEqual(-1, line.EndPoint.X, "EndPoint X not correct");
            Assert.AreEqual(1, line.EndPoint.Y, "EndPoint Y not correct");
        }

        /// <summary>
        ///     Test Rotation of Vector
        /// </summary>
        [TestMethod]
        public void RotateCurves()
        {
            var startPoint = new Point {X = 0, Y = 0};

            var endPoint = new Point {X = 0, Y = 1};

            var curve = GetRotationCurve(startPoint, endPoint, 90);

            Assert.AreEqual(0, curve.Points[0].X, "Point X not correct");
            Assert.AreEqual(0, curve.Points[0].Y, "Point Y not correct");
            Assert.AreEqual(-1, curve.Points[1].X, "Point X not correct");
            Assert.AreEqual(0, curve.Points[1].Y, "Point Y not correct");

            curve = GetRotationCurve(startPoint, endPoint, -90);

            Assert.AreEqual(0, curve.Points[0].X, "StartPoint X not correct");
            Assert.AreEqual(0, curve.Points[0].Y, "StartPoint Y not correct");
            Assert.AreEqual(1, curve.Points[1].X, "EndPoint X not correct");
            Assert.AreEqual(0, curve.Points[1].Y, "EndPoint Y not correct");
        }

        /// <summary>
        ///     Test Rotation of Vector
        /// </summary>
        [TestMethod]
        public void AlgorithmLines()
        {
            var startPoint = new Point {X = 0, Y = 0};

            var endPoint = new Point {X = 0, Y = 1};

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
        ///     Test Save and Load
        /// </summary>
        [TestMethod]
        public void SaveLoad()
        {
            var vct = new Vectors();

            var startPoint = new Point {X = 0, Y = 0};

            var endPoint = new Point {X = 0, Y = 1};

            var line = GetRotationLine(startPoint, endPoint, 90);
            var points = new List<Point>(2) {startPoint, endPoint};

            vct.LineAdd(line);
            var crv = vct.CurveAdd(points);

            //vct.CurveRemove(curve);

            vct.SaveContainer("test.obj");
            var container = vct.GetVectorImage("test.obj");
        }

        /// <summary>
        ///     Get the scale curve.
        /// </summary>
        /// <param name="startPoint">Start Point Vector</param>
        /// <param name="endPoint">End Point Vector</param>
        /// <param name="factor">Scale Factor</param>
        /// <returns>The <see cref="CurveObject" />Changed Vector.</returns>
        private static CurveObject GetScaleCurve(Point startPoint, Point endPoint, int factor)
        {
            var points = new List<Point>(2) {startPoint, endPoint};
            var curve = new CurveObject {Points = points};

            return VgProcessing.CurveScale(curve, factor);
        }

        /// <summary>
        ///     Get the scale line.
        /// </summary>
        /// <param name="startPoint">Start Point Vector</param>
        /// <param name="endPoint">End Point Vector</param>
        /// <param name="factor">Scale Factor</param>
        /// <returns>The <see cref="LineObject" />Changed Vector.</returns>
        private static LineObject GetScaleLine(Point startPoint, Point endPoint, int factor)
        {
            var trsLine = new LineObject {StartPoint = startPoint, EndPoint = endPoint};

            return VgProcessing.LineScale(trsLine, factor, Width);
        }

        /// <summary>
        ///     Only Lines this time
        /// </summary>
        /// <param name="startPoint">Start Point Vector</param>
        /// <param name="endPoint">End Point Vector</param>
        /// <param name="degree">Degrees</param>
        /// <returns>Changed Vector</returns>
        private static LineObject GetRotationLine(Point startPoint, Point endPoint, int degree)
        {
            var trsLine = new LineObject {StartPoint = startPoint, EndPoint = endPoint};

            return VgProcessing.GetLineObject(trsLine, degree, Width);
        }

        /// <summary>
        ///     Only Curves this Time
        /// </summary>
        /// <param name="startPoint">Start Point Vector</param>
        /// <param name="endPoint">End Point Vector</param>
        /// <param name="degree">Degrees</param>
        /// <returns>Changed Vector</returns>
        private static CurveObject GetRotationCurve(Point startPoint, Point endPoint, int degree)
        {
            var points = new List<Point>(2) {startPoint, endPoint};
            var curve = new CurveObject {Points = points};

            return VgProcessing.CurveRotate(curve, degree);
        }
    }
}
