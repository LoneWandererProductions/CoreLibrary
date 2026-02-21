/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        FastLinqTests.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtendedSystemObjects;

namespace CommonExtendedObjectsTests
{
    /// <summary>
    /// Basic unit tests for FastLinq extension methods to ensure correctness and benchmark performance against standard LINQ methods.
    /// </summary>
    [TestClass]
    public class FastLinqTests
    {
        /// <summary>
        /// The large data
        /// </summary>
        private int[]? _largeData;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Large array to test performance
            _largeData = Enumerable.Range(0, 1_000_000).ToArray();
        }

        /// <summary>
        /// Tests for each fast.
        /// </summary>
        [TestMethod]
        public void TestForEachFast()
        {
            long sum1 = 0;
            long sum2 = 0;

            // Standard foreach
            foreach (var v in _largeData!)
                sum1 += v;

            // FastLinq
            ReadOnlySpan<int> span = _largeData!;
            span.ForEachFast(v => sum2 += v);

            Assert.AreEqual(sum1, sum2);
        }

        /// <summary>
        /// Tests the select fast.
        /// </summary>
        [TestMethod]
        public void TestSelectFast()
        {
            var result = new int[_largeData!.Length];
            ReadOnlySpan<int> span = _largeData;

            span.SelectFast(result.AsSpan(), x => x * 2);

            for (var i = 0; i < _largeData.Length; i++)
                Assert.AreEqual(_largeData[i] * 2, result[i]);
        }

        /// <summary>
        /// Tests the where fast.
        /// </summary>
        [TestMethod]
        public void TestWhereFast()
        {
            var result = new int[_largeData!.Length];
            ReadOnlySpan<int> span = _largeData;

            int count = span.WhereFast(result.AsSpan(), x => x % 2 == 0);

            for (var i = 0; i < count; i++)
                Assert.AreEqual(0, result[i] % 2);
        }

        /// <summary>
        /// Tests the aggregate fast.
        /// </summary>
        [TestMethod]
        public void TestAggregateFast()
        {
            ReadOnlySpan<int> span = _largeData!;
            long sum1 = _largeData!.Select(x => (long)x).Sum();
            long sum2 = span.AggregateFast(0L, (acc, val) => acc + val);

            Assert.AreEqual(sum1, sum2);
        }

        /// <summary>
        /// Tests any all count fast.
        /// </summary>
        [TestMethod]
        public void TestAnyAllCountFast()
        {
            ReadOnlySpan<int> span = _largeData!;

            Assert.AreEqual(_largeData!.Any(), span.AnyFast());
            Assert.AreEqual(_largeData.All(x => x >= 0), span.AllFast(x => x >= 0));
            Assert.AreEqual(_largeData.Length, span.CountFast());
        }

        /// <summary>
        /// Benchmarks the fast linq.
        /// </summary>
        [TestMethod]
        public void BenchmarkFastLinq()
        {
            ReadOnlySpan<int> span = _largeData!;

            var sw = Stopwatch.StartNew();
            span.ForEachFast(x => _ = x * 2);
            sw.Stop();
            Trace.WriteLine($"ForEachFast elapsed: {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            foreach (var x in _largeData!)
                _ = x * 2;
            sw.Stop();
            Trace.WriteLine($"foreach elapsed: {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            long linqSum = _largeData!.Select(x => (long)x).Sum();
            sw.Stop();
            Trace.WriteLine($"LINQ Sum (long) elapsed: {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            long fastSum = span.AggregateFast(0L, (acc, val) => acc + val);
            sw.Stop();
            Trace.WriteLine($"AggregateFast (long) elapsed: {sw.ElapsedMilliseconds} ms");
        }
    }
}
