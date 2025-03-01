using System.Collections.Generic;
using System.Numerics;
using LightVector;
using NUnit.Framework;

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
