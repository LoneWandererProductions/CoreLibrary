/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     TrigBenchmarkTests
 * FILE:        TrigBenchmarkTests/TrigBenchmarkTests.cs
 * PURPOSE:     Test some speed options
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    /// Tests for some simplifications
    /// </summary>
    [TestClass]
    public class TrigBenchmarkTests
    {
        /// <summary>
        /// The iterations
        /// </summary>
        private const int iterations = 1_000_000;

        /// <summary>
        /// The x
        /// </summary>
        private const float x = 1.2345f;

        /// <summary>
        /// Compares the sin functions.
        /// </summary>
        [TestMethod]
        public void CompareSinFunctions()
        {
            Compare(nameof(FastMath.FastSin), FastMath.FastSin, MathF.Sin);
            Compare(nameof(FastMath.MediumSin), FastMath.MediumSin, MathF.Sin);
            Compare(nameof(FastMath.LUTSin), FastMath.LUTSin, MathF.Sin);
        }

        /// <summary>
        /// Compares the cos functions.
        /// </summary>
        [TestMethod]
        public void CompareCosFunctions()
        {
            Compare(nameof(FastMath.FastCos), FastMath.FastCos, MathF.Cos);
        }

        /// <summary>
        /// Compares the specified method name.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="fastFunc">The fast function.</param>
        /// <param name="refFunc">The reference function.</param>
        private static void Compare(string methodName, Func<float, float> fastFunc, Func<float, float> refFunc)
        {
            var fastResult = 0f;
            var refResult = 0f;
            var errorSum = 0f;

            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
            {
                fastResult = fastFunc(x);
                refResult = refFunc(x);
                errorSum += MathF.Abs(0 - refResult);
            }

            sw.Stop();
            Trace.WriteLine($"{methodName}: {sw.ElapsedMilliseconds} ms, avg error={errorSum / iterations}");
        }
    }
}
