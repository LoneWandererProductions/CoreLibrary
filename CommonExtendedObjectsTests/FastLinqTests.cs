using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtendedSystemObjects;

namespace CommonExtendedObjectsTests
{
    [TestClass]
    public class FastLinqTests
    {
        private int[]? largeData;

        [TestInitialize]
        public void Setup()
        {
            // Large array to test performance
            largeData = Enumerable.Range(0, 1_000_000).ToArray();
        }

        [TestMethod]
        public void TestForEachFast()
        {
            long sum1 = 0;
            long sum2 = 0;

            // Standard foreach
            foreach (var v in largeData!)
                sum1 += v;

            // FastLinq
            ReadOnlySpan<int> span = largeData!;
            span.ForEachFast(v => sum2 += v);

            Assert.AreEqual(sum1, sum2);
        }

        [TestMethod]
        public void TestSelectFast()
        {
            var result = new int[largeData!.Length];
            ReadOnlySpan<int> span = largeData;

            span.SelectFast(result.AsSpan(), x => x * 2);

            for (var i = 0; i < largeData.Length; i++)
                Assert.AreEqual(largeData[i] * 2, result[i]);
        }

        [TestMethod]
        public void TestWhereFast()
        {
            var result = new int[largeData!.Length];
            ReadOnlySpan<int> span = largeData;

            int count = span.WhereFast(result.AsSpan(), x => x % 2 == 0);

            for (var i = 0; i < count; i++)
                Assert.AreEqual(0, result[i] % 2);
        }

        [TestMethod]
        public void TestAggregateFast()
        {
            ReadOnlySpan<int> span = largeData!;
            long sum1 = largeData!.Select(x => (long)x).Sum();
            long sum2 = span.AggregateFast(0L, (acc, val) => acc + val);

            Assert.AreEqual(sum1, sum2);
        }

        [TestMethod]
        public void TestAnyAllCountFast()
        {
            ReadOnlySpan<int> span = largeData!;

            Assert.AreEqual(largeData!.Any(), span.AnyFast());
            Assert.AreEqual(largeData.All(x => x >= 0), span.AllFast(x => x >= 0));
            Assert.AreEqual(largeData.Length, span.CountFast());
        }

        [TestMethod]
        public void BenchmarkFastLinq()
        {
            ReadOnlySpan<int> span = largeData!;

            var sw = Stopwatch.StartNew();
            span.ForEachFast(x => _ = x * 2);
            sw.Stop();
            Trace.WriteLine($"ForEachFast elapsed: {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            foreach (var x in largeData!)
                _ = x * 2;
            sw.Stop();
            Trace.WriteLine($"foreach elapsed: {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            long linqSum = largeData!.Select(x => (long)x).Sum();
            sw.Stop();
            Trace.WriteLine($"LINQ Sum (long) elapsed: {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            long fastSum = span.AggregateFast(0L, (acc, val) => acc + val);
            sw.Stop();
            Trace.WriteLine($"AggregateFast (long) elapsed: {sw.ElapsedMilliseconds} ms");
        }
    }
}
