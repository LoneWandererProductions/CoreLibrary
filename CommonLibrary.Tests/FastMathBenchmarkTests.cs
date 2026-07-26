/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrary.Tests
 * FILE:        FastMathBenchmarkTests.cs
 * PURPOSE:     Some Benchmarks for Math.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using Mathematics;
using Mathematics.Constants;

namespace CommonLibrary.Tests
{
    [TestClass]
    public class FastMathBenchmarkTests
    {
        // Constants for benchmarking
        private const int Iterations = 100000;
        private const float InputFloat = 123.456f; // Example float input for sin, cos, tan
        private const int InputInt = 123456; // Example int input for log2

        /// <summary>
        /// Benchmarks all functions.
        /// </summary>
        [TestMethod]
        public void BenchmarkAllFunctions()
        {
            ComparePerformance("Log2 Benchmark", FastLog2, Math.Log2);
            ComparePerformance("Fast Sin Benchmark", FastSinF, Math.Sin);
            ComparePerformance("Sin Benchmark", ExtSinF, Math.Sin);
            ComparePerformance("Cos Benchmark", ExtCosF, Math.Cos);
            ComparePerformance("Tan Benchmark", ExtTanF, Math.Tan);
        }

        /// <summary>
        /// Compare FastLog2 to standard Math.Log2
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="customFunc">The custom function.</param>
        /// <param name="standardFunc">The standard function.</param>
        private static void ComparePerformance(string methodName, Func<int, int> customFunc,
            Func<double, double> standardFunc)
        {
            // Warm up
            customFunc(InputInt);
            standardFunc(InputInt);

            // Measure custom function performance
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < Iterations; i++)
            {
                customFunc(InputInt);
            }

            stopwatch.Stop();
            var customTime = stopwatch.ElapsedMilliseconds;

            // Measure standard function performance
            stopwatch.Restart();
            for (var i = 0; i < Iterations; i++)
            {
                standardFunc(InputInt);
            }

            stopwatch.Stop();
            var standardTime = stopwatch.ElapsedMilliseconds;

            // Output results
            PrintResults(methodName, customTime, standardTime);
        }

        /// <summary>
        /// Compare FastSinF to standard Math.Sin
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="customFunc">The custom function.</param>
        /// <param name="standardFunc">The standard function.</param>
        private static void ComparePerformance(string methodName, Func<float, float> customFunc,
            Func<double, double> standardFunc)
        {
            // Warm up
            customFunc(InputFloat);
            standardFunc(InputFloat);

            // Measure custom function performance
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < Iterations; i++)
            {
                customFunc(InputFloat);
            }

            stopwatch.Stop();
            var customTime = stopwatch.ElapsedMilliseconds;

            // Measure standard function performance
            stopwatch.Restart();
            for (var i = 0; i < Iterations; i++)
            {
                standardFunc(InputFloat);
            }

            stopwatch.Stop();
            var standardTime = stopwatch.ElapsedMilliseconds;

            // Output results
            PrintResults(methodName, customTime, standardTime);
        }

        /// <summary>
        /// Prints the results.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="customTime">The custom time.</param>
        /// <param name="standardTime">The standard time.</param>
        private static void PrintResults(string methodName, long customTime, long standardTime)
        {
            Console.WriteLine($"{methodName}:");
            Console.WriteLine($"  Custom function time: {customTime} ms");
            Console.WriteLine($"  Standard function time: {standardTime} ms");
            Console.WriteLine($"  Performance ratio (faster is better): {(double)standardTime / customTime:F2}");
            Console.WriteLine();
        }

        /// <summary>
        /// Example fast log2 function (integer approximation)
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        private static int FastLog2(int x)
        {
            return FastMath.FastLog2(x);
        }

        /// <summary>
        /// Example fast sine function (float)
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        private static float FastSinF(float x)
        {
            return FastMath.FastSin(x); // Replace with actual fast implementation
        }

        /// <summary>
        /// Example fast sine function (float)
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        private static float ExtSinF(float x)
        {
            var y = (int)x;
            return ExtendedMath.CalcSinF(y); // Replace with actual fast implementation
        }

        /// <summary>
        /// Example fast cosine function (float)
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        private static float ExtCosF(float x)
        {
            var y = (int)x;
            return ExtendedMath.CalcCosF(y); // Replace with actual fast implementation
        }

        /// <summary>
        /// Example fast tangent function (float)
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns></returns>
        private static float ExtTanF(float x)
        {
            var y = (int)x;
            return (float)ExtendedMath.CalcTan(y); // Replace with actual fast implementation
        }
    }
}
