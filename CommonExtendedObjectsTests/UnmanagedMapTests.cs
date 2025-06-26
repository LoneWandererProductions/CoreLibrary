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
        private UnmanagedMap<int> _map;
        private const int Iterations = 100_000;

        [TestInitialize]
        public void Setup()
        {
            _map = new UnmanagedMap<int>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _map.Dispose();
        }

        [TestMethod]
        public void Set_ThenTryGetValue_ReturnsValue()
        {
            _map.Set(42, 99);
            Assert.IsTrue(_map.TryGetValue(42, out var value));
            Assert.AreEqual(99, value);
        }

        [TestMethod]
        public void OverwriteValue_UpdatesCorrectly()
        {
            _map.Set(1, 10);
            _map.Set(1, 20);
            Assert.IsTrue(_map.TryGetValue(1, out var value));
            Assert.AreEqual(20, value);
        }

        [TestMethod]
        public void TryRemove_RemovesEntry()
        {
            _map.Set(123, 456);
            Assert.IsTrue(_map.TryRemove(123));
            Assert.IsFalse(_map.ContainsKey(123));
        }

        [TestMethod]
        public void Resize_StillKeepsAllEntries()
        {
            for (int i = 0; i < 200; i++)
                _map.Set(i, i * 10);

            for (int i = 0; i < 200; i++)
            {
                Assert.IsTrue(_map.TryGetValue(i, out var val));
                Assert.AreEqual(i * 10, val);
            }
        }

        [TestMethod]
        public void Compact_ReclaimsSpace()
        {
            for (int i = 0; i < 50; i++)
                _map.Set(i, i);

            for (int i = 0; i < 25; i++)
                _map.TryRemove(i);

            int before = _map.Count;
            _map.Compact();

            Assert.AreEqual(25, _map.Count);
        }

        [TestMethod]
        public void Enumerator_YieldsOnlyOccupied()
        {
            _map.Set(1, 100);
            _map.Set(2, 200);
            _map.Set(3, 300);
            _map.TryRemove(2);

            var values = new List<int>();
            foreach (var entry in _map)
                values.Add(entry.Item2);

            CollectionAssert.AreEquivalent(new[] { 100, 300 }, values);
        }

        [TestMethod]
        public void Benchmark_Insert_CompareWithDictionary()
        {
            var dict = new Dictionary<int, int>(Iterations);
            var map = new UnmanagedMap<int>(18); // 2^17 = 131072

            var swDict = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
                dict[i] = i;
            swDict.Stop();

            var swMap = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
                map.Set(i, i);
            swMap.Stop();

            map.Dispose();

            Trace.WriteLine($"Dictionary.Insert: {swDict.Elapsed.TotalMilliseconds:F3} ms");
            Trace.WriteLine($"UnmanagedIntMap.Insert: {swMap.Elapsed.TotalMilliseconds:F3} ms");

            Assert.IsTrue(swMap.Elapsed.TotalMilliseconds < swDict.Elapsed.TotalMilliseconds * 2,
                "UnmanagedIntMap insert is unreasonably slow");
        }

        [TestMethod]
        public void Benchmark_Lookup_CompareWithDictionary()
        {
            var dict = new Dictionary<int, int>(Iterations);
            var map = new UnmanagedMap<int>(17);

            for (int i = 0; i < Iterations; i++)
            {
                dict[i] = i;
                map.Set(i, i);
            }

            var swDict = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
                _ = dict.TryGetValue(i, out _);
            swDict.Stop();

            var swMap = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
                _ = map.TryGetValue(i, out _);
            swMap.Stop();

            map.Dispose();

            Trace.WriteLine($"Dictionary.Lookup: {swDict.Elapsed.TotalMilliseconds:F3} ms");
            Trace.WriteLine($"UnmanagedIntMap.Lookup: {swMap.Elapsed.TotalMilliseconds:F3} ms");

            Assert.IsTrue(swMap.Elapsed.TotalMilliseconds < swDict.Elapsed.TotalMilliseconds * 2,
                "UnmanagedIntMap lookup is unreasonably slow");
        }
    }
}
