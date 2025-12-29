/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/MatrixTests.cs
 * PURPOSE:     Tests for the Math Tools
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using ExtendedSystemObjects;
using Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     Test some image related stuff
    /// </summary>
    [TestClass]
    public class MatrixTests
    {
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

            var compare1 = HelperMethods.MatrixTestOne(m1, m2);

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

            var oldM = HelperMethods.MatrixTestTwo(v, matrix);
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
    }
}
