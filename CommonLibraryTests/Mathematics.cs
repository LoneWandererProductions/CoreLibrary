/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/Mathematics.cs
 * PURPOSE:     Tests for the Math Tools
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ExtendedSystemObjects;
using Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     Test some image related stuff
    /// </summary>
    [TestClass]
    public class Mathematics
    {
        /// <summary>
        ///     Test the custom DirectBitmap and how it works
        /// </summary>
        [TestMethod]
        public void Fractures()
        {
            var one = new ExtendedMath.Fraction(14, 2);
            Assert.AreEqual(7, one.Numerator, string.Concat("Test Failed: ", one.Numerator));
            Assert.AreEqual(1, one.Denominator, string.Concat("Test Failed: ", one.Denominator));
            Assert.AreEqual(0, one.Exponent, string.Concat("Test Failed: ", one.Exponent));
            Assert.AreEqual(7, one.ExponentNumerator, string.Concat("Test Failed: ", one.ExponentNumerator));
            Assert.AreEqual(7, one.Decimal, string.Concat("Test Failed: ", one.Decimal));

            one = new ExtendedMath.Fraction(14, 8);
            Assert.AreEqual(3, one.Numerator, string.Concat("Test Failed: ", one.Numerator));
            Assert.AreEqual(4, one.Denominator, string.Concat("Test Failed: ", one.Denominator));
            Assert.AreEqual(1, one.Exponent, string.Concat("Test Failed: ", one.Exponent));
            Assert.AreEqual(7, one.ExponentNumerator, string.Concat("Test Failed: ", one.ExponentNumerator));
            Assert.AreEqual((decimal)1.75, one.Decimal, string.Concat("Test Failed: ", one.Decimal));

            one = new ExtendedMath.Fraction(0, 1, 2);
            Assert.AreEqual(0, one.Numerator, string.Concat("Test Failed: ", one.Numerator));
            Assert.AreEqual(1, one.Denominator, string.Concat("Test Failed: ", one.Denominator));
            Assert.AreEqual(0, one.Exponent, string.Concat("Test Failed: ", one.Exponent));
            Assert.AreEqual(0, one.ExponentNumerator, string.Concat("Test Failed: ", one.ExponentNumerator));
            Assert.AreEqual(0, one.Decimal, string.Concat("Test Failed: ", one.Decimal));
        }

        /// <summary>
        ///     Test the custom DirectBitmap and how it works
        /// </summary>
        [TestMethod]
        public void FracturesOperations()
        {
            var one = new ExtendedMath.Fraction(1, 2, 2);
            Assert.AreEqual(5, one.ExponentNumerator, string.Concat("Test Failed: ", one.ExponentNumerator));
            var two = new ExtendedMath.Fraction(1, 2, 2);
            Assert.AreEqual(5, two.ExponentNumerator, string.Concat("Test Failed: ", two.ExponentNumerator));

            var result = one + two;

            Assert.AreEqual(5, result.Numerator, string.Concat("Test Failed: ", result.Numerator));
            Assert.AreEqual(1, result.Denominator, string.Concat("Test Failed: ", result.Denominator));
            Assert.AreEqual(0, result.Exponent, string.Concat("Test Failed: ", result.Exponent));
            Assert.AreEqual(5, result.ExponentNumerator, string.Concat("Test Failed: ", result.ExponentNumerator));
            Assert.AreEqual(5, result.Decimal, string.Concat("Test Failed: ", result.Decimal));

            result = one - two;

            Assert.AreEqual(0, result.Numerator, string.Concat("Test Failed: ", result.Numerator));
            Assert.AreEqual(4, result.Denominator, string.Concat("Test Failed: ", result.Denominator));
            Assert.AreEqual(0, result.Exponent, string.Concat("Test Failed: ", result.Exponent));
            Assert.AreEqual(0, result.ExponentNumerator, string.Concat("Test Failed: ", result.ExponentNumerator));
            Assert.AreEqual(0, result.Decimal, string.Concat("Test Failed: ", result.Decimal));

            one = new ExtendedMath.Fraction(4, 2);
            Assert.AreEqual(2, one.ExponentNumerator, string.Concat("Test Failed: ", one.ExponentNumerator));
            Assert.AreEqual(2, one.Numerator, string.Concat("Test Failed: ", one.Numerator));
            Assert.AreEqual(1, one.Denominator, string.Concat("Test Failed: ", one.Denominator));

            two = new ExtendedMath.Fraction(1, 1, 4);
            Assert.AreEqual(4, two.ExponentNumerator, string.Concat("Test Failed: ", two.ExponentNumerator));

            result = one * two;

            Assert.AreEqual(8, result.Numerator, string.Concat("Test Failed: ", result.Numerator));
            Assert.AreEqual(1, result.Denominator, string.Concat("Test Failed: ", result.Denominator));
            Assert.AreEqual(0, result.Exponent, string.Concat("Test Failed: ", result.Exponent));
            Assert.AreEqual(8, result.ExponentNumerator, string.Concat("Test Failed: ", result.ExponentNumerator));
            Assert.AreEqual(8, result.Decimal, string.Concat("Test Failed: ", result.Decimal));

            result = one / two;

            Assert.AreEqual(1, result.Numerator, string.Concat("Test Failed: ", result.Numerator));
            Assert.AreEqual(2, result.Denominator, string.Concat("Test Failed: ", result.Denominator));
            Assert.AreEqual(0, result.Exponent, string.Concat("Test Failed: ", result.Exponent));
            Assert.AreEqual(1, result.ExponentNumerator, string.Concat("Test Failed: ", result.ExponentNumerator));
            Assert.AreEqual((decimal)0.5, result.Decimal, string.Concat("Test Failed: ", result.Decimal));

            one = new ExtendedMath.Fraction(2, 2);
            // 2 1/2                                                this is the add-on
            two = new ExtendedMath.Fraction(1, 2, 1);

            result = one + two;

            Assert.AreEqual(1, result.Numerator, string.Concat("Test Failed: ", result.Numerator));
            Assert.AreEqual(2, result.Denominator, string.Concat("Test Failed: ", result.Denominator));
            Assert.AreEqual(2, result.Exponent, string.Concat("Test Failed: ", result.Exponent));
            Assert.AreEqual(5, result.ExponentNumerator, string.Concat("Test Failed: ", result.ExponentNumerator));
            Assert.AreEqual((decimal)2.5, result.Decimal, string.Concat("Test Failed: ", result.Decimal));

            // 1/2
            one = new ExtendedMath.Fraction(-1, 2);
            // 3/2  1,5
            two = new ExtendedMath.Fraction(1, 2, -1);
            //4/3   1.25
            var three = new ExtendedMath.Fraction(1, 4, -1);

            //-1/2 * 3/2 = 3 / 4
            result = one * two;
            Assert.AreEqual(3, result.Numerator, string.Concat("Test Failed: ", result.Numerator));
            Assert.AreEqual(4, result.Denominator, string.Concat("Test Failed: ", result.Denominator));
            Assert.AreEqual(0, result.Exponent, string.Concat("Test Failed: ", result.Exponent));
            Assert.AreEqual(3, result.ExponentNumerator, string.Concat("Test Failed: ", result.ExponentNumerator));
            Assert.AreEqual((decimal)0.75, result.Decimal, string.Concat("Test Failed: ", result.Decimal));

            //-1/2 * -5/4 = 5 / 8
            result = one * three;
            Assert.AreEqual(5, result.Numerator, string.Concat("Test Failed: ", result.Numerator));
            Assert.AreEqual(8, result.Denominator, string.Concat("Test Failed: ", result.Denominator));
            Assert.AreEqual(0, result.Exponent, string.Concat("Test Failed: ", result.Exponent));
            Assert.AreEqual(5, result.ExponentNumerator, string.Concat("Test Failed: ", result.ExponentNumerator));
            Assert.AreEqual((decimal)0.625, result.Decimal, string.Concat("Test Failed: ", result.Decimal));

            //todo add +/-
        }

        /// <summary>
        ///     Matrix multiplications.
        /// </summary>
        [TestMethod]
        public void MatrixMultiplications()
        {
            double[,] x = { { 1, 1, 1 } };
            var m1 = new BaseMatrix { Matrix = x };

            double[,] y = { { 2 }, { 2 }, { 2 } };
            var m2 = new BaseMatrix { Matrix = y };

            var result = MatrixUtility.UnsafeMultiplication(m1, m2);

            Assert.AreEqual(result[0, 0], 6, "First");

            x = new double[,] { { 2, 1, 1 }, { 1, 2, 1 }, { 1, 1, 2 } };
            m1 = new BaseMatrix { Matrix = x };

            y = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
            m2 = new BaseMatrix { Matrix = y };

            result = MatrixUtility.UnsafeMultiplication(m1, m2);

            var timer = new Stopwatch();
            timer.Start();

            var compare1 = MathSpeedTests.TestOne(m1, m2);

            timer.Stop();
            Trace.WriteLine(string.Concat("Test one (span Version): ", timer.Elapsed));

            timer = new Stopwatch();
            timer.Start();

            var compare2 = MatrixUtility.UnsafeMultiplication(m1, m2);

            timer.Stop();
            Trace.WriteLine(string.Concat("Test two (unsafe multiplication): ", timer.Elapsed));

            Assert.AreEqual(result[0, 0], 2, "00");
            Assert.AreEqual(result[1, 0], 1, "10");
            Assert.AreEqual(result[2, 0], 1, "20");
            Assert.AreEqual(result[0, 1], 1, "01");
            Assert.AreEqual(result[1, 1], 2, "11");
            Assert.AreEqual(result[2, 1], 1, "21");
            Assert.AreEqual(result[0, 2], 1, "02");
            Assert.AreEqual(result[1, 2], 1, "12");
            Assert.AreEqual(result[2, 2], 2, "22");

            Assert.AreEqual(compare1[0, 0], 2, "00");
            Assert.AreEqual(compare1[1, 0], 1, "10");
            Assert.AreEqual(compare1[2, 0], 1, "20");
            Assert.AreEqual(compare1[0, 1], 1, "01");
            Assert.AreEqual(compare1[1, 1], 2, "11");
            Assert.AreEqual(compare1[2, 1], 1, "21");
            Assert.AreEqual(compare1[0, 2], 1, "02");
            Assert.AreEqual(compare1[1, 2], 1, "12");
            Assert.AreEqual(compare1[2, 2], 2, "22");

            Assert.AreEqual(compare2[0, 0], 2, "00");
            Assert.AreEqual(compare2[1, 0], 1, "10");
            Assert.AreEqual(compare2[2, 0], 1, "20");
            Assert.AreEqual(compare2[0, 1], 1, "01");
            Assert.AreEqual(compare2[1, 1], 2, "11");
            Assert.AreEqual(compare2[2, 1], 1, "21");
            Assert.AreEqual(compare2[0, 2], 1, "02");
            Assert.AreEqual(compare2[1, 2], 1, "12");
            Assert.AreEqual(compare2[2, 2], 2, "22");

            x = new double[,] { { 0, -1, 0 }, { 1, 0, 0 }, { 0, 0, 1 } };
            m1 = new BaseMatrix { Matrix = x };
            y = new double[,] { { 1 }, { 0 }, { 0 } };
            m2 = new BaseMatrix { Matrix = y };

            result = m1 * m2;

            Assert.AreEqual(result[0, 0], 0, "00");
            Assert.AreEqual(result[1, 0], 1, "10");
            Assert.AreEqual(result[2, 0], 0, "20");

            double[,] matrix = { { 1, 1, 3, 1 } };
            m1 = new BaseMatrix(matrix);

            double[,] scale = { { 320, 0, 0, 0 }, { 0, 240, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 1 } };

            m2 = new BaseMatrix { Matrix = scale };

            result = m1 * m2;

            var compare = new double[,] { { 320, 240, 0, 1 } };

            var check = compare.Equal(result.Matrix);

            Assert.IsTrue(check, "4*4");
        }

        /// <summary>
        ///     Matrix additions.
        /// </summary>
        [TestMethod]
        public void MatrixAdditions()
        {
            double[,] x = { { 8, 4 }, { 3, 2 } };
            var m1 = new BaseMatrix { Matrix = x };

            double[,] y = { { 3, 5 }, { 1, 2 } };
            var m2 = new BaseMatrix { Matrix = y };

            var result = MatrixUtility.UnsafeAddition(m1, m2);

            Assert.AreEqual(result[0, 0], 11, "11");
            Assert.AreEqual(result[0, 1], 9, "9");
            Assert.AreEqual(result[1, 0], 4, "4");
            Assert.AreEqual(result[1, 1], 4, "4");
        }

        /// <summary>
        ///     Matrix subtraction.
        /// </summary>
        [TestMethod]
        public void MatrixSubtraction()
        {
            double[,] x = { { 8, 4 }, { 3, 2 } };
            var m1 = new BaseMatrix { Matrix = x };

            double[,] y = { { 3, 5 }, { 1, 2 } };
            var m2 = new BaseMatrix { Matrix = y };

            var result = MatrixUtility.UnsafeSubtraction(m1, m2);

            Assert.AreEqual(result[0, 0], 5, "5");
            Assert.AreEqual(result[0, 1], -1, "-1");
            Assert.AreEqual(result[1, 0], 2, "2");
            Assert.AreEqual(result[1, 1], 0, "0");
        }

        /// <summary>
        ///     Matrix inversion.
        /// </summary>
        [TestMethod]
        public void MatrixInversion()
        {
            double[,] x = { { 8, 4 }, { 3, 2 } };

            var i = MatrixUtility.MatrixIdentity(2);

            var m1 = new BaseMatrix { Matrix = x };

            var m2 = m1.Inverse();

            var result = m1 * m2;

            Assert.IsTrue(result[0, 0].Equals(1), "00");
            Assert.IsTrue(result[0, 1].Equals(0), "00");
            Assert.IsTrue(result[1, 0].Equals(0), "00");
            Assert.IsTrue(result[1, 1].Equals(1), "00");

            var cache = result.Matrix;
            //check compare of arrays
            var check = cache.Equal(i.Matrix);
            Assert.IsTrue(check, "Inverse Matrix");
        }

        /// <summary>
        ///     Vector3D Test.
        /// </summary>
        [TestMethod]
        public void Vector3D()
        {
            //dot product
            var one = new Vector3D(8, 4, 5);
            var two = new Vector3D(1, 2, 4);
            var scalar = one * two; //36
            Assert.AreEqual(scalar, 36, "Dot Product");

            //scalar product
            var vector = one * 3;
            Assert.AreEqual(vector.X, 24, "X scalar Product");
            Assert.AreEqual(vector.Y, 12, "Y scalar Product");
            Assert.AreEqual(vector.Z, 15, "Z scalar Product");

            //scalar division
            vector = two / 2;
            Assert.AreEqual(vector.X, 0.5, "X scalar Division");
            Assert.AreEqual(vector.Y, 1, "Y scalar Division");
            Assert.AreEqual(vector.Z, 2, "Z scalar Division");

            //magnitude (vector length)
            scalar = one.VectorLength(); //10.247
            Assert.AreEqual(Math.Round(scalar,3), 10.247, "Vector length");

            //Vector Addition
            vector = one +  two;
            Assert.AreEqual(vector.X, 9, "X Addition");
            Assert.AreEqual(vector.Y, 6, "Y Addition");
            Assert.AreEqual(vector.Z, 9, "Z Addition");

            //Vector subtraction
            vector = one -  two;
            Assert.AreEqual(vector.X, 7, "X Subtraction");
            Assert.AreEqual(vector.Y, 2, "Y Subtraction");
            Assert.AreEqual(vector.Z, 1, "Z Subtraction");

            //Vector Cross Product
            vector = one.CrossProduct(two);
            Assert.AreEqual(vector.X, 6, "X Cross Product");
            Assert.AreEqual(vector.Y, -27, "Y Cross Product");
            Assert.AreEqual(vector.Z, 12, "Z Cross Product");

            //negation
            vector = -one;
            Assert.AreEqual(vector.X, -8, "X Negation");
            Assert.AreEqual(vector.Y, -4, "Y Negation");
            Assert.AreEqual(vector.Z, -5, "Z Negation");

            //Angle between Vector
            scalar = one.Angle(two); //39.946
            scalar = scalar * 180 / Math.PI;
            Assert.AreEqual(Math.Round(scalar,3), 39.946, "Vector Angle");

            //normalize Vector, Unit Vector
            vector = one.Normalize();
            Assert.AreEqual(Math.Round(vector.X,5), 0.78072, "X Unit Vector");
            Assert.AreEqual(Math.Round(vector.Y,5), 0.39036, "Y Unit Vector");
            Assert.AreEqual(Math.Round(vector.Z,5), 0.48795, "Z Unit Vector");

            //inequality
            Assert.IsTrue(one != two, "Inequality");
            //inequality
            Assert.IsTrue(one == one, "Equality, first");
            Assert.IsTrue(one.Equals(one), "Equality, second");
        }

        /// <summary>
        ///     Vector transformations.
        /// </summary>
        [TestMethod]
        public void VectorTransformations()
        {
            var vector = new Vector3D(1, 1, 1);

            //scale Test
            var result = Projection3D.Scale(vector, 2);
            var v1 = (Vector3D)result;

            Assert.IsTrue(v1.X.Equals(2), "X");
            Assert.IsTrue(v1.Y.Equals(2), "Y");
            Assert.IsTrue(v1.Z.Equals(2), "Z");

            result = Projection3D.Scale(vector, 2, 2, 2);
            v1 = (Vector3D)result;

            Assert.IsTrue(v1.X.Equals(2), "X");
            Assert.IsTrue(v1.Y.Equals(2), "Y");
            Assert.IsTrue(v1.Z.Equals(2), "Z");

            vector = new Vector3D(2, 4, 5);

            result = Projection3D.Scale(vector, 4, 2.5, 6);
            v1 = (Vector3D)result;

            Assert.IsTrue(v1.X.Equals(8), "X");
            Assert.IsTrue(v1.Y.Equals(10), "Y");
            Assert.IsTrue(v1.Z.Equals(30), "Z");

            //translate
            vector = new Vector3D(1, 1, 1);
            var target = new Vector3D(2, 2, 2);

            result = Projection3D.Translate(vector, target);
            v1 = (Vector3D)result;

            Assert.IsTrue(v1.X.Equals(3), "X");
            Assert.IsTrue(v1.Y.Equals(3), "Y");
            Assert.IsTrue(v1.Z.Equals(3), "Z");

            vector = new Vector3D(1, 0, 0);

            //https://en.wikipedia.org/wiki/Rotation_matrix
            result = Projection3D.RotateZ(vector, 90);
            v1 = (Vector3D)result;

            var inputValue = Math.Round(v1.X, 2);
            Assert.AreEqual(inputValue, 0, "X");
            Assert.IsTrue(v1.Y.Equals(1), "Y");
            Assert.IsTrue(v1.Z.Equals(0), "Z");

            vector = new Vector3D(1, 1, 1);
            result = Projection3D.RotateX(vector, 90);
            v1 = (Vector3D)result;

            inputValue = Math.Round(v1.Y, 2);

            Assert.IsTrue(v1.X.Equals(1), "X");
            Assert.AreEqual(inputValue, -1, "Y");
            Assert.IsTrue(v1.Z.Equals(1), "Z");

            vector = new Vector3D(1, 1, 1);
            result = Projection3D.RotateY(vector, 90);
            v1 = (Vector3D)result;

            inputValue = Math.Round(v1.Z, 2);

            Assert.IsTrue(v1.X.Equals(1), "X");
            Assert.IsTrue(v1.Y.Equals(1), "Y");
            Assert.AreEqual(inputValue, -1, "Z");

            vector = new Vector3D(1, 1, 1);
            result = Projection3D.RotateZ(vector, 90);
            v1 = (Vector3D)result;

            inputValue = Math.Round(v1.X, 2);

            Assert.AreEqual(inputValue, -1, "X");
            Assert.IsTrue(v1.Y.Equals(1), "Y");
            Assert.IsTrue(v1.Z.Equals(1), "Z");
        }

        /// <summary>
        ///     Projections from 3d to 2d.
        /// </summary>
        [TestMethod]
        public void ProjectionTo2D()
        {
            var vector = new Vector3D(8, 4, 5);

            const int screenHeight = 240;
            const int screenWidth = 256;

            // Projection Matrix
            const float fNear = 0.1f;
            const float fFar = 1000.0f;
            const float fFov = 90.0f;
            const double fAspectRatio = (double)screenHeight / screenWidth;
            var fFovRad = 1.0f / Math.Tan(fFov * 0.5f / 180.0f * 3.14159f);

            Projection3DRegister.Width = screenWidth;
            Projection3DRegister.Height = screenHeight;
            Projection3DRegister.Angle = fFov;

            var matProj = new BaseMatrix();

            var m1 = new double[4, 4];

            m1[0, 0] = fAspectRatio * fFovRad;
            m1[1, 1] = fFovRad;
            m1[2, 2] = fFar / (fFar - fNear);
            m1[3, 2] = -fFar * fNear / (fFar - fNear);
            m1[2, 3] = 1.0f;
            m1[3, 3] = 0.0f;
            matProj.Matrix = m1;

            var vec = MultiplyMatrixVector(vector, matProj);

            var comp = Projection3DCamera.ProjectionTo3D(vector);

            Assert.IsTrue(Math.Abs(vec.X - comp.X) < 0.00001, "Basic check one, X");
            Assert.IsTrue(Math.Abs(vec.Y - comp.Y) < 0.00001, "Basic check one, Y");
            Assert.IsTrue(Math.Abs(vec.Z - comp.Z) < 0.00001, "Basic check one, Z");

            //known values: 1,1,3 expected: 1/3, 1/3, 1

            vector = new Vector3D(1, 1, 3);

            vec = MultiplyMatrixVector(vector, matProj);

            comp = Projection3DCamera.ProjectionTo3D(vector);

            Assert.IsTrue(Math.Abs(vec.X - comp.X) < 0.00001, "Basic check one, X");
            Assert.IsTrue(Math.Abs(vec.Y - comp.Y) < 0.00001, "Basic check one, Y");
            Assert.IsTrue(Math.Abs(vec.Z - comp.Z) < 0.00001, "Basic check one, Z");
        }

        /// <summary>
        ///     Multiplies the matrix vector.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <param name="matProj">The mat proj.</param>
        /// <returns>The Projection Vecctor</returns>
        private static Vector3D MultiplyMatrixVector(Vector3D i, BaseMatrix matProj)
        {
            var o = new Vector3D
            {
                X = (i.X * matProj.Matrix[0, 0]) + (i.Y * matProj.Matrix[1, 0]) + (i.Z * matProj.Matrix[2, 0]) +
                    matProj.Matrix[3, 0],
                Y = (i.X * matProj.Matrix[0, 1]) + (i.Y * matProj.Matrix[1, 1]) + (i.Z * matProj.Matrix[2, 1]) +
                    matProj.Matrix[3, 1],
                Z = (i.X * matProj.Matrix[0, 2]) + (i.Y * matProj.Matrix[1, 2]) + (i.Z * matProj.Matrix[2, 2]) +
                    matProj.Matrix[3, 2]
            };

            var w = (i.X * matProj.Matrix[0, 3]) + (i.Y * matProj.Matrix[1, 3]) + (i.Z * matProj.Matrix[2, 3]) +
                    matProj.Matrix[3, 3];

            w = Math.Round(w, 2);

            if (w == 0.0f)
            {
                return o;
            }

            o.X /= w;
            o.Y /= w;
            o.Z /= w;

            return o;
        }

        /// <summary>
        ///     Permutations this instance.
        /// </summary>
        [TestMethod]
        public void Permutation()
        {
            var lst = new List<string> { "A", "B", "C" };
            var result = lst.GetCombination();

            foreach (var items in result)
            {
                foreach (var item in items)
                {
                    Trace.Write(item);
                }

                Trace.WriteLine(string.Empty);
            }

            Assert.AreEqual(7, result.Count(), "Right amount");
        }

        /// <summary>
        ///     Permutations of k in n Elements.
        /// </summary>
        [TestMethod]
        public void PermutationNk()
        {
            Trace.WriteLine("First Test");

            var a = new List<string> { "A", "B", "C" };

            var lst = a.CombinationsWithRepetition(3);

            foreach (var item in lst)
            {
                Trace.WriteLine(item);
            }

            Assert.AreEqual(27, lst.Count(), "Right amount");
        }
    }
}
