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
    /// Benchmark tests for FastMath
    /// </summary>
    [TestClass]
    public class FastMathBenchmarkTests
    {
        /// <summary>
        /// The number of iterations for benchmarking
        /// </summary>
        private const int iterations = 1_000_000;

        /// <summary>
        /// The test input value
        /// </summary>
        private const float inputValue = 1.2f; // Example input for sin, cos, etc.

        /// <summary>
        /// Tests FastSin against MathF.Sin
        /// </summary>
        [TestMethod]
        public void CompareFastSin()
        {
            Compare(nameof(FastMath.FastSin), FastMath.FastSin, MathF.Sin);
        }

        /// <summary>
        /// Tests FastLog2 against Math.Log2
        /// </summary>
        [TestMethod]
        public void CompareFastLog2()
        {
            Compare(nameof(FastMath.FastLog2), (x) => FastMath.FastLog2((int)x), (x) => (int)Math.Log2(x));
        }

        /// <summary>
        /// Generic benchmarking method for floating-point functions
        /// </summary>
        private static void Compare(string methodName, Func<float, float> fastFunc, Func<float, float> standardFunc)
        {
            // Warm up
            fastFunc(inputValue);
            standardFunc(inputValue);

            // Measure fast function
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
                fastFunc(inputValue);
            stopwatch.Stop();
            long fastTime = stopwatch.ElapsedMilliseconds;

            // Measure standard function
            stopwatch.Restart();
            for (int i = 0; i < iterations; i++)
                standardFunc(inputValue);
            stopwatch.Stop();
            long standardTime = stopwatch.ElapsedMilliseconds;

            // Output results
            PrintResults(methodName, fastTime, standardTime);
        }

        /// <summary>
        /// Overload for integer-based functions
        /// </summary>
        private static void Compare(string methodName, Func<int, int> fastFunc, Func<int, int> standardFunc)
        {
            // Warm up
            fastFunc(256);
            standardFunc(256);

            // Measure fast function
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
                fastFunc(256);
            stopwatch.Stop();
            long fastTime = stopwatch.ElapsedMilliseconds;

            // Measure standard function
            stopwatch.Restart();
            for (int i = 0; i < iterations; i++)
                standardFunc(256);
            stopwatch.Stop();
            long standardTime = stopwatch.ElapsedMilliseconds;

            // Output results
            PrintResults(methodName, fastTime, standardTime);
        }

        /// <summary>
        /// Prints the results in a cleaner format
        /// </summary>
        private static void PrintResults(string methodName, long fastTime, long standardTime)
        {
            string output = $"{methodName} -> Fast: {fastTime,5}ms | Standard: {standardTime,5}ms";
            Trace.WriteLine(output); // Log to trace output if necessary
        }
    }
}
