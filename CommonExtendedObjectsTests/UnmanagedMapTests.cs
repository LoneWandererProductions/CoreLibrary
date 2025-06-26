/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        UnmanagedMapTests.cs
 * PURPOSE:     Some basic function tests for UnmanagedMap
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Diagnostics;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonExtendedObjectsTests
{
    [TestClass]
    public class UnmanagedMapTests
    {
        /// <summary>
        ///     The iterations
        /// </summary>
        private const int Iterations = 100_000;

        /// <summary>
        ///     The test object.
        /// </summary>
        private UnmanagedMap<int> _map;

        /// <summary>
        ///     Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _map = new UnmanagedMap<int>();
        }

        /// <summary>
        ///     Cleanups this instance.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            _map.Dispose();
        }

        /// <summary>
        ///     Sets the then try get value returns value.
        /// </summary>
        [TestMethod]
        public void SetThenTryGetValueReturnsValue()
        {
            _map.Set(42, 99);
            Assert.IsTrue(_map.TryGetValue(42, out var value));
            Assert.AreEqual(99, value);
        }

        /// <summary>
        ///     Overwrites the value updates correctly.
        /// </summary>
        [TestMethod]
        public void OverwriteValueUpdatesCorrectly()
        {
            _map.Set(1, 10);
            _map.Set(1, 20);
            Assert.IsTrue(_map.TryGetValue(1, out var value));
            Assert.AreEqual(20, value);
        }

        /// <summary>
        ///     Tries the remove removes entry.
        /// </summary>
        [TestMethod]
        public void TryRemoveRemovesEntry()
        {
            _map.Set(123, 456);
            Assert.IsTrue(_map.TryRemove(123));
            Assert.IsFalse(_map.ContainsKey(123));
        }

        /// <summary>
        ///     Resizes the still keeps all entries.
        /// </summary>
        [TestMethod]
        public void ResizeStillKeepsAllEntries()
        {
            for (var i = 0; i < 200; i++)
            {
                _map.Set(i, i * 10);
            }

            for (var i = 0; i < 200; i++)
            {
                Assert.IsTrue(_map.TryGetValue(i, out var val));
                Assert.AreEqual(i * 10, val);
            }
        }

        /// <summary>
        ///     Compacts the reclaims space.
        /// </summary>
        [TestMethod]
        public void CompactReclaimsSpace()
        {
            for (var i = 0; i < 50; i++)
            {
                _map.Set(i, i);
            }

            for (var i = 0; i < 25; i++)
            {
                _map.TryRemove(i);
            }

            var before = _map.Count;
            _map.Compact();

            Assert.AreEqual(25, _map.Count);
        }

        /// <summary>
        ///     Enumerators the yields only occupied.
        /// </summary>
        [TestMethod]
        public void EnumeratorYieldsOnlyOccupied()
        {
            _map.Set(1, 100);
            _map.Set(2, 200);
            _map.Set(3, 300);
            _map.TryRemove(2);

            var values = new List<int>();
            foreach (var entry in _map)
            {
                values.Add(entry.Item2);
            }

            CollectionAssert.AreEquivalent(new[] { 100, 300 }, values);
        }

        /// <summary>
        ///     Benchmarks the insert compare with dictionary.
        /// </summary>
        [TestMethod]
        public void BenchmarkInsertCompareWithDictionary()
        {
            var dict = new Dictionary<int, int>(Iterations);
            var map = new UnmanagedMap<int>(18); // 2^17 = 131072

            var swDict = Stopwatch.StartNew();
            for (var i = 0; i < Iterations; i++)
            {
                dict[i] = i;
            }

            swDict.Stop();

            var swMap = Stopwatch.StartNew();
            for (var i = 0; i < Iterations; i++)
            {
                map.Set(i, i);
            }

            swMap.Stop();

            map.Dispose();

            Trace.WriteLine($"Dictionary.Insert: {swDict.Elapsed.TotalMilliseconds:F3} ms");
            Trace.WriteLine($"UnmanagedIntMap.Insert: {swMap.Elapsed.TotalMilliseconds:F3} ms");

            Assert.IsTrue(swMap.Elapsed.TotalMilliseconds < swDict.Elapsed.TotalMilliseconds * 3,
                "UnmanagedIntMap insert is unreasonably slow");
        }

        [TestMethod]
        public void Benchmark_Lookup_CompareWithDictionary()
        {
            var dict = new Dictionary<int, int>(Iterations);
            var map = new UnmanagedMap<int>(17);

            for (var i = 0; i < Iterations; i++)
            {
                dict[i] = i;
                map.Set(i, i);
            }

            var swDict = Stopwatch.StartNew();
            for (var i = 0; i < Iterations; i++)
            {
                _ = dict.TryGetValue(i, out _);
            }

            swDict.Stop();

            var swMap = Stopwatch.StartNew();
            for (var i = 0; i < Iterations; i++)
            {
                _ = map.TryGetValue(i, out _);
            }

            swMap.Stop();

            map.Dispose();

            Trace.WriteLine($"Dictionary.Lookup: {swDict.Elapsed.TotalMilliseconds:F3} ms");
            Trace.WriteLine($"UnmanagedIntMap.Lookup: {swMap.Elapsed.TotalMilliseconds:F3} ms");

            Assert.IsTrue(swMap.Elapsed.TotalMilliseconds < swDict.Elapsed.TotalMilliseconds * 3,
                "UnmanagedIntMap lookup is unreasonably slow");
        }
    }
}
