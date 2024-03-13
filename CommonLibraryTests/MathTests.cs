/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/MathTests.cs
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
    public class MathTests
    {
        /// <summary>
        ///     Test the custom DirectBitmap and how it works
        /// </summary>
        [TestMethod]
        public void Fractures()
        {
            var one = new ExtendedMath.Fraction(14, 2);
            Assert.AreEqual(7, one.Numerator, $"Test Failed: {one.Numerator}");
            Assert.AreEqual(1, one.Denominator, $"Test Failed: {one.Denominator}");
            Assert.AreEqual(0, one.Exponent, $"Test Failed: {one.Exponent}");
            Assert.AreEqual(7, one.ExponentNumerator, $"Test Failed: {one.ExponentNumerator}");
            Assert.AreEqual(7, one.Decimal, $"Test Failed: {one.Decimal}");

            one = new ExtendedMath.Fraction(14, 8);
            Assert.AreEqual(3, one.Numerator, $"Test Failed: {one.Numerator}");
            Assert.AreEqual(4, one.Denominator, $"Test Failed: {one.Denominator}");
            Assert.AreEqual(1, one.Exponent, $"Test Failed: {one.Exponent}");
            Assert.AreEqual(7, one.ExponentNumerator, $"Test Failed: {one.ExponentNumerator}");
            Assert.AreEqual((decimal)1.75, one.Decimal, $"Test Failed: {one.Decimal}");

            one = new ExtendedMath.Fraction(0, 1, 2);
            Assert.AreEqual(0, one.Numerator, $"Test Failed: {one.Numerator}");
            Assert.AreEqual(1, one.Denominator, $"Test Failed: {one.Denominator}");
            Assert.AreEqual(0, one.Exponent, $"Test Failed: {one.Exponent}");
            Assert.AreEqual(0, one.ExponentNumerator, $"Test Failed: {one.ExponentNumerator}");
            Assert.AreEqual(0, one.Decimal, $"Test Failed: {one.Decimal}");
        }

        /// <summary>
        ///     Test the custom DirectBitmap and how it works
        /// </summary>
        [TestMethod]
        public void FracturesOperations()
        {
            var one = new ExtendedMath.Fraction(1, 2, 2);
            Assert.AreEqual(5, one.ExponentNumerator, $"Test Failed: {one.ExponentNumerator}");
            var two = new ExtendedMath.Fraction(1, 2, 2);
            Assert.AreEqual(5, two.ExponentNumerator, $"Test Failed: {two.ExponentNumerator}");

            var result = one + two;

            Assert.AreEqual(5, result.Numerator, $"Test Failed: {result.Numerator}");
            Assert.AreEqual(1, result.Denominator, $"Test Failed: {result.Denominator}");
            Assert.AreEqual(0, result.Exponent, $"Test Failed: {result.Exponent}");
            Assert.AreEqual(5, result.ExponentNumerator, $"Test Failed: {result.ExponentNumerator}");
            Assert.AreEqual(5, result.Decimal, $"Test Failed: {result.Decimal}");

            result = one - two;

            Assert.AreEqual(0, result.Numerator, $"Test Failed: {result.Numerator}");
            Assert.AreEqual(4, result.Denominator, $"Test Failed: {result.Denominator}");
            Assert.AreEqual(0, result.Exponent, $"Test Failed: {result.Exponent}");
            Assert.AreEqual(0, result.ExponentNumerator, $"Test Failed: {result.ExponentNumerator}");
            Assert.AreEqual(0, result.Decimal, $"Test Failed: {result.Decimal}");

            one = new ExtendedMath.Fraction(4, 2);
            Assert.AreEqual(2, one.ExponentNumerator, $"Test Failed: {one.ExponentNumerator}");
            Assert.AreEqual(2, one.Numerator, $"Test Failed: {one.Numerator}");
            Assert.AreEqual(1, one.Denominator, $"Test Failed: {one.Denominator}");

            two = new ExtendedMath.Fraction(1, 1, 4);
            Assert.AreEqual(4, two.ExponentNumerator, $"Test Failed: {two.ExponentNumerator}");

            result = one * two;

            Assert.AreEqual(8, result.Numerator, $"Test Failed: {result.Numerator}");
            Assert.AreEqual(1, result.Denominator, $"Test Failed: {result.Denominator}");
            Assert.AreEqual(0, result.Exponent, $"Test Failed: {result.Exponent}");
            Assert.AreEqual(8, result.ExponentNumerator, $"Test Failed: {result.ExponentNumerator}");
            Assert.AreEqual(8, result.Decimal, $"Test Failed: {result.Decimal}");

            result = one / two;

            Assert.AreEqual(1, result.Numerator, $"Test Failed: {result.Numerator}");
            Assert.AreEqual(2, result.Denominator, $"Test Failed: {result.Denominator}");
            Assert.AreEqual(0, result.Exponent, $"Test Failed: {result.Exponent}");
            Assert.AreEqual(1, result.ExponentNumerator, $"Test Failed: {result.ExponentNumerator}");
            Assert.AreEqual((decimal)0.5, result.Decimal, $"Test Failed: {result.Decimal}");

            one = new ExtendedMath.Fraction(2, 2);
            // 2 1/2                                                this is the add-on
            two = new ExtendedMath.Fraction(1, 2, 1);

            result = one + two;

            Assert.AreEqual(1, result.Numerator, $"Test Failed: {result.Numerator}");
            Assert.AreEqual(2, result.Denominator, $"Test Failed: {result.Denominator}");
            Assert.AreEqual(2, result.Exponent, $"Test Failed: {result.Exponent}");
            Assert.AreEqual(5, result.ExponentNumerator, $"Test Failed: {result.ExponentNumerator}");
            Assert.AreEqual((decimal)2.5, result.Decimal, $"Test Failed: {result.Decimal}");

            // 1/2
            one = new ExtendedMath.Fraction(-1, 2);
            // 3/2  1,5
            two = new ExtendedMath.Fraction(1, 2, -1);
            //4/3   1.25
            var three = new ExtendedMath.Fraction(1, 4, -1);

            //-1/2 * 3/2 = 3 / 4
            result = one * two;
            Assert.AreEqual(3, result.Numerator, $"Test Failed: {result.Numerator}");
            Assert.AreEqual(4, result.Denominator, $"Test Failed: {result.Denominator}");
            Assert.AreEqual(0, result.Exponent, $"Test Failed: {result.Exponent}");
            Assert.AreEqual(3, result.ExponentNumerator, $"Test Failed: {result.ExponentNumerator}");
            Assert.AreEqual((decimal)0.75, result.Decimal, $"Test Failed: {result.Decimal}");

            //-1/2 * -5/4 = 5 / 8
            result = one * three;
            Assert.AreEqual(5, result.Numerator, $"Test Failed: {result.Numerator}");
            Assert.AreEqual(8, result.Denominator, $"Test Failed: {result.Denominator}");
            Assert.AreEqual(0, result.Exponent, $"Test Failed: {result.Exponent}");
            Assert.AreEqual(5, result.ExponentNumerator, $"Test Failed: {result.ExponentNumerator}");
            Assert.AreEqual((decimal)0.625, result.Decimal, $"Test Failed: {result.Decimal}");

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
            Trace.WriteLine($"Test one (span Version): {timer.Elapsed}");

            timer = new Stopwatch();
            timer.Start();

            var compare2 = MatrixUtility.UnsafeMultiplication(m1, m2);

            timer.Stop();
            Trace.WriteLine($"Test two (unsafe multiplication): {timer.Elapsed}");

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

            //modelMatrix={1 0 0 0  0 1 0 0  0 0 1 0  0 0 3 1  }
            matrix = new double[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 3, 1 } };
            m1 = new BaseMatrix { Matrix = matrix };
            var v = new double[,] { { -1, -1, -1, 0 } };
            m2 = new BaseMatrix { Matrix = v };

            var oldM = MathSpeedTests.TestTwo(v, matrix);
            var newM = m2 * m1;

            check = oldM.Equals(newM);
            Assert.IsTrue(check, "Something is horrible wrong");
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

            Assert.IsTrue(Math.Abs(result[0, 0] - 1) < 0.00001, "00");
            Assert.IsTrue(Math.Abs(result[0, 1] - 0) < 0.00001, "01");
            Assert.IsTrue(Math.Abs(result[1, 0] - 0) < 0.00001, "10");
            Assert.IsTrue(Math.Abs(result[1, 1] - 1) < 0.00001, "11");

            //check compare of arrays
            var check = result.Equals(i);
            Assert.IsTrue(check, "Inverse Matrix");

            // 0.408248246, 0.872871578, 0.267261237, 0
            // -0.816496491, 0.218217969, 0.534522474, 0
            // 0.408248276, -0.436435580, 0.801783681, 0
            // 0, 0, 0, 1
            x = new double[,] { { 1, 2, 3, 0 }, { -4, 2, 2, 0 }, { 1, 1, 1, 0 }, { 2, 1, -1, 1 } };

            //   0 -1 / 6    1 / 3   0
            //   1  1 / 3    7 / 3   0
            //   1 -1 / 6   -5 / 3   0
            //   2 -1 / 6  -14 / 3   1

            m1 = new BaseMatrix { Matrix = x };
            m2 = m1.Inverse();

            var data = m1 * m2;

            Assert.IsTrue(Math.Abs(data[0, 0] - 1) < 0.00001, "00");
            Assert.IsTrue(Math.Abs(data[1, 1] - 1) < 0.00001, "11");
            Assert.IsTrue(Math.Abs(data[2, 2] - 1) < 0.00001, "22");
            Assert.IsTrue(Math.Abs(data[3, 3] - 1) < 0.00001, "33");

            Assert.IsTrue(Math.Abs(data[0, 1]) < 0.00001, "01");
            Assert.IsTrue(Math.Abs(data[0, 2]) < 0.00001, "02");
            Assert.IsTrue(Math.Abs(data[0, 3]) < 0.00001, "03");

            Assert.IsTrue(Math.Abs(data[1, 0]) < 0.00001, "10");
            Assert.IsTrue(Math.Abs(data[1, 2]) < 0.00001, "12");
            Assert.IsTrue(Math.Abs(data[1, 3]) < 0.00001, "13");

            Assert.IsTrue(Math.Abs(data[2, 0]) < 0.00001, "20");
            Assert.IsTrue(Math.Abs(data[2, 1]) < 0.00001, "21");
            Assert.IsTrue(Math.Abs(data[2, 3]) < 0.00001, "23");

            Assert.IsTrue(Math.Abs(data[3, 0]) < 0.00001, "30");
            Assert.IsTrue(Math.Abs(data[3, 1]) < 0.00001, "31");
            Assert.IsTrue(Math.Abs(data[3, 2]) < 0.00001, "32");

            //check our new Compare
            i = MatrixUtility.MatrixIdentity(4);
            check = data == i;

            Assert.IsTrue(check, "Compare Check passed");
        }

        /// <summary>
        ///     Matrix Determinant.
        /// </summary>
        [TestMethod]
        public void MatrixDeterminant()
        {
            var x = new double[,] { { 1, 2, 3, 0 }, { -4, 2, 2, 0 }, { 1, 1, 1, 0 }, { 2, 1, -1, 1 } };

            var m1 = new BaseMatrix { Matrix = x };
            var determinant = m1.Determinant();
            Assert.AreEqual(-6, Math.Round(determinant, 1), "Wrong Determinant");

            x = new double[,] { { 1, 2 }, { -4, 2 } };

            m1 = new BaseMatrix { Matrix = x };
            determinant = m1.Determinant();

            Assert.AreEqual(10, determinant, "Wrong Determinant");
        }

        /// <summary>
        ///     Matrix Decompose.
        /// </summary>
        [TestMethod]
        public void MatrixDecompose()
        {
            double[,] matrix = { { 2, -1, -2 }, { -4, 6, 3 }, { -4, -2, 8 } };

            var cache = MatrixInverse.LuDecomposition(matrix);

            double[,] upper = { { 1, 0, 0 }, { -2, 1, 0 }, { -2, -1, 1 } };

            var check = upper.Equal(cache.Key);

            Assert.IsTrue(check, "Upper not correct");

            double[,] lower = { { 2, -1, -2 }, { 0, 4, -1 }, { 0, 0, 3 } };

            check = lower.Equal(cache.Value);

            Assert.IsTrue(check, "Lower not correct");
        }


        /// <summary>
        ///     Matrix additions.
        /// </summary>
        [TestMethod]
        public void MatrixToString()
        {
            double[,] x = { { 8, 4 }, { 3, 2 } };
            var m1 = new BaseMatrix { Matrix = x };

            var cache = m1.ToString();

            var str = $"8 , 4{Environment.NewLine}3 , 2{Environment.NewLine}";
            var check = string.Equals(cache, str);

            Assert.IsTrue(check, "Done");
        }

        /// <summary>
        ///     Vector3D Test.
        /// </summary>
        [TestMethod]
        public void Vector3DTest()
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

            vector = 3 * one;
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
            Assert.AreEqual(Math.Round(scalar, 3), 10.247, "Vector length");

            //Vector Addition
            vector = one + two;
            Assert.AreEqual(vector.X, 9, "X Addition");
            Assert.AreEqual(vector.Y, 6, "Y Addition");
            Assert.AreEqual(vector.Z, 9, "Z Addition");

            //Vector subtraction
            vector = one - two;
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
            Assert.AreEqual(Math.Round(scalar, 3), 39.946, "Vector Angle");

            //normalize Vector, Unit Vector
            vector = one.Normalize();
            Assert.AreEqual(Math.Round(vector.X, 5), 0.78072, "X Unit Vector");
            Assert.AreEqual(Math.Round(vector.Y, 5), 0.39036, "Y Unit Vector");
            Assert.AreEqual(Math.Round(vector.Z, 5), 0.48795, "Z Unit Vector");

            //inequality
            Assert.IsTrue(one != two, "Inequality");
            //inequality
            // ReSharper disable once EqualExpressionComparison
#pragma warning disable CS1718 // Vergleich erfolgte mit derselben Variable
            Assert.IsTrue(one == one, "Equality, first");
#pragma warning restore CS1718 // Vergleich erfolgte mit derselben Variable
            Assert.IsTrue(one.Equals(one), "Equality, second");

            var nullVector = Vector3D.ZeroVector;
            Assert.AreEqual(nullVector.X, 0, "X Addition");
            Assert.AreEqual(nullVector.Y, 0, "Y Addition");
            Assert.AreEqual(nullVector.Z, 0, "Z Addition");

            var unitVector = Vector3D.UnitVector;
            Assert.AreEqual(unitVector.X, 1, "X Addition");
            Assert.AreEqual(unitVector.Y, 1, "Y Addition");
            Assert.AreEqual(unitVector.Z, 1, "Z Addition");
        }

        /// <summary>
        ///     Vector2D Test.
        /// </summary>
        [TestMethod]
        public void Vector2DTest()
        {
            //dot product
            var one = new Vector2D(8, 4);
            var two = new Vector2D(1, 2);
            var scalar = one * two;
            Assert.AreEqual(scalar, 16, "Dot Product");

            //scalar multiplication
            var vector = two * 2;
            Assert.AreEqual(vector.X, 2, "X scalar Multiplication.");
            Assert.AreEqual(vector.Y, 4, "Y scalar Multiplication.");

            //scalar multiplication
            vector = 2 * two;
            Assert.AreEqual(vector.X, 2, "X scalar Multiplication.");
            Assert.AreEqual(vector.Y, 4, "Y scalar Multiplication.");

            //scalar division
            vector = one / 2;
            Assert.AreEqual(vector.X, 4, "X scalar Division.");
            Assert.AreEqual(vector.Y, 2, "Y scalar Division.");

            //negation
            vector = -one;
            Assert.AreEqual(vector.X, -8, "X Negation");
            Assert.AreEqual(vector.Y, -4, "Y Negation");

            //Vector Addition
            vector = one + two;
            Assert.AreEqual(vector.X, 9, "X Addition");
            Assert.AreEqual(vector.Y, 6, "Y Addition");

            //normalize Vector, Unit Vector
            vector = one.Normalize();
            Assert.AreEqual(Math.Round(vector.X, 5), 0, 89443, "X Unit Vector");
            Assert.AreEqual(Math.Round(vector.Y, 5), 0, 44721, "Y Unit Vector");

            //Angle between Vector
            scalar = one.Angle(two);
            scalar = scalar * 180 / Math.PI;
            Assert.AreEqual(Math.Round(scalar, 3), 36.87, "Vector Angle");

            //inequality
            Assert.IsTrue(one != two, "Inequality");
            //inequality
            // ReSharper disable once EqualExpressionComparison
#pragma warning disable CS1718 // Vergleich erfolgte mit derselben Variable
            Assert.IsTrue(one == one, "Equality, first");
#pragma warning restore CS1718 // Vergleich erfolgte mit derselben Variable
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
