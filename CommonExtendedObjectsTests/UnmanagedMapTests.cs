/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        UnmanagedMapTests.cs
 * PURPOSE:     Some basic function tests for UnmanagedMap
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
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
        private UnmanagedMap<int>? _map;

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
            _map?.Dispose();
        }

        /// <summary>
        ///     Sets the then try get value returns value.
        /// </summary>
        [TestMethod]
        public void SetThenTryGetValueReturnsValue()
        {
            _map?.Set(42, 99);
            Assert.IsNotNull(_map);
            Assert.IsTrue(_map.TryGetValue(42, out var value));
            Assert.AreEqual(99, value);
        }

        /// <summary>
        ///     Overwrites the value updates correctly.
        /// </summary>
        [TestMethod]
        public void OverwriteValueUpdatesCorrectly()
        {
            _map?.Set(1, 10);
            _map?.Set(1, 20);
            Assert.IsNotNull(_map);
            Assert.IsTrue(_map.TryGetValue(1, out var value));
            Assert.AreEqual(20, value);
        }

        /// <summary>
        ///     Tries the remove removes entry.
        /// </summary>
        [TestMethod]
        public void TryRemoveRemovesEntry()
        {
            _map?.Set(123, 456);
            Assert.IsNotNull(_map);
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
                _map?.Set(i, i * 10);
            }

            for (var i = 0; i < 200; i++)
            {
                Assert.IsNotNull(_map);
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
                _map?.Set(i, i);
            }

            for (var i = 0; i < 25; i++)
            {
                _map?.TryRemove(i);
            }

            Assert.IsNotNull(_map);

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
            _map?.Set(1, 100);
            _map?.Set(2, 200);
            _map?.Set(3, 300);
            _map?.TryRemove(2);

            Assert.IsNotNull(_map);

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
        public void BenchmarkInsertCompareWithDictionary_Stable()
        {
            const int iterations = 100_000; // keep reasonable
            var dict = new Dictionary<int, int>(iterations);
            var map = new UnmanagedMap<int>(18); // 2^17

            // Warm-up: ensures JIT and caches are ready
            for (int i = 0; i < 1000; i++)
            {
                dict[i] = i;
                map.Set(i, i);
            }

            // Clear containers for real benchmark
            dict.Clear();
            map.Clear();

            // Benchmark dictionary
            var swDict = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                dict[i] = i;
            }
            swDict.Stop();

            // Benchmark UnmanagedMap
            var swMap = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                map.Set(i, i);
            }
            swMap.Stop();

            map.Dispose();

            Trace.WriteLine($"Dictionary.Insert: {swDict.Elapsed.TotalMilliseconds:F3} ms");
            Trace.WriteLine($"UnmanagedMap.Insert: {swMap.Elapsed.TotalMilliseconds:F3} ms");

            // Relaxed assertion: ensures we catch huge regressions without flakiness
            var ratio = swMap.Elapsed.TotalMilliseconds / swDict.Elapsed.TotalMilliseconds;
            Assert.IsTrue(ratio < 5.0, $"UnmanagedMap insert is too slow (ratio: {ratio:F2})");
        }


        /// <summary>
        /// Benchmarks the lookup compare with dictionary.
        /// </summary>
        [TestMethod]
        public void BenchmarkLookupCompareWithDictionary()
        {
            // Prepare
            var dict = new Dictionary<int, int>(Iterations);
            var map = new UnmanagedMap<int>(17);

            for (var i = 0; i < Iterations; i++)
            {
                dict[i] = i;
                map.Set(i, i);
            }

            // Force GC and clean up memory/cache before timing
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            const int loops = 5;
            double totalDictMs = 0;
            double totalMapMs = 0;

            // Pre-warm
            for (var i = 0; i < Iterations; i++)
            {
                _ = dict.TryGetValue(i, out _);
                _ = map.TryGetValue(i, out _);
            }

            // Benchmark Dictionary
            for (int loop = 0; loop < loops; loop++)
            {
                var swDict = Stopwatch.StartNew();
                for (var i = 0; i < Iterations; i++)
                    _ = dict.TryGetValue(i, out _);
                swDict.Stop();
                totalDictMs += swDict.Elapsed.TotalMilliseconds;
            }
            double avgDictMs = totalDictMs / loops;

            // Benchmark UnmanagedMap
            for (int loop = 0; loop < loops; loop++)
            {
                var swMap = Stopwatch.StartNew();
                for (var i = 0; i < Iterations; i++)
                    _ = map.TryGetValue(i, out _);
                swMap.Stop();
                totalMapMs += swMap.Elapsed.TotalMilliseconds;
            }
            double avgMapMs = totalMapMs / loops;

            map.Dispose();

            Trace.WriteLine($"Dictionary.Lookup (avg over {loops} loops): {avgDictMs:F3} ms");
            Trace.WriteLine($"UnmanagedMap.Lookup (avg over {loops} loops): {avgMapMs:F3} ms");

            // Allow up to 4x slower for very large maps
            Assert.IsTrue(avgMapMs < avgDictMs * 4,
                "UnmanagedMap lookup is unreasonably slow");
        }
    }
}
