/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        ImmutableLookupMapAllTests.cs
 * PURPOSE:     Unified test suite for both ImmutableLookupMap and ImmutableLookupMapUnmanaged
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonExtendedObjectsTests
{
    /// <summary>
    ///     Unified tests for both ImmutableLookupMap and ImmutableLookupMapUnmanaged.
    /// </summary>
    [TestClass]
    public class ImmutableLookupMapAllTests
    {
        /// <summary>
        /// The string map data
        /// </summary>
        private Dictionary<int, string> _stringMapData;

        /// <summary>
        /// The float map data
        /// </summary>
        private Dictionary<int, float> _floatMapData;

        /// <summary>
        /// The string map
        /// </summary>
        private ImmutableLookupMap<int, string> _stringMap;

        /// <summary>
        /// The read only map
        /// </summary>
        private ReadOnlyDictionary<int, string> _readOnlyMap;

        /// <summary>
        /// The unmanaged map
        /// </summary>
        private ImmutableLookupMapUnmanaged<int, float> _unmanagedMap;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _stringMapData = new Dictionary<int, string>();
            _floatMapData = new Dictionary<int, float>();

            for (var i = 0; i < 10_000; i++)
            {
                _stringMapData[i] = $"Value_{i}";
                _floatMapData[i] = i * 1.5f;
            }

            _stringMap = new ImmutableLookupMap<int, string>(_stringMapData);
            _readOnlyMap = new ReadOnlyDictionary<int, string>(_stringMapData);
            _unmanagedMap = new ImmutableLookupMapUnmanaged<int, float>(_floatMapData);
        }

        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            _unmanagedMap.Dispose();
        }

        // ------------ STRING MAP TESTS ----------------

        /// <summary>
        /// Tests the string map initialization.
        /// </summary>
        [TestMethod]
        public void TestStringMapInitialization()
        {
            var stopwatch = Stopwatch.StartNew();
            var map = new ImmutableLookupMap<int, string>(_stringMapData);
            stopwatch.Stop();

            Trace.WriteLine($"ImmutableLookupMap (string) Initialization: {stopwatch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Tests the read only dictionary initialization.
        /// </summary>
        [TestMethod]
        public void TestReadOnlyDictionaryInitialization()
        {
            var stopwatch = Stopwatch.StartNew();
            var map = new ReadOnlyDictionary<int, string>(_stringMapData);
            stopwatch.Stop();

            Trace.WriteLine($"ReadOnlyDictionary Initialization: {stopwatch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Tests the string map lookup.
        /// </summary>
        [TestMethod]
        public void TestStringMapLookup()
        {
            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < 10_000; i++)
            {
                var value = _stringMap.Get(i);
                Assert.AreEqual($"Value_{i}", value);
            }

            stopwatch.Stop();
            Trace.WriteLine($"ImmutableLookupMap (string) Lookup Time: {stopwatch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Tests the read only dictionary lookup.
        /// </summary>
        [TestMethod]
        public void TestReadOnlyDictionaryLookup()
        {
            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < 10_000; i++)
            {
                var value = _readOnlyMap[i];
                Assert.AreEqual($"Value_{i}", value);
            }

            stopwatch.Stop();
            Trace.WriteLine($"ReadOnlyDictionary Lookup Time: {stopwatch.ElapsedMilliseconds} ms");
        }

        // ------------ UNMANAGED MAP TESTS ----------------

        /// <summary>
        /// Tests the unmanaged map initialization.
        /// </summary>
        [TestMethod]
        public void TestUnmanagedMapInitialization()
        {
            var stopwatch = Stopwatch.StartNew();
            using var map = new ImmutableLookupMapUnmanaged<int, float>(_floatMapData);
            stopwatch.Stop();

            Trace.WriteLine($"ImmutableLookupMapUnmanaged (float) Initialization: {stopwatch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Tests the unmanaged map lookup.
        /// </summary>
        [TestMethod]
        public void TestUnmanagedMapLookup()
        {
            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < 10_000; i++)
            {
                var value = _unmanagedMap.Get(i);
                Assert.AreEqual(i * 1.5f, value);
            }

            stopwatch.Stop();
            Trace.WriteLine($"ImmutableLookupMapUnmanaged (float) Lookup Time: {stopwatch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// Tests the unmanaged map try get value.
        /// </summary>
        [TestMethod]
        public void TestUnmanagedMapTryGetValue()
        {
            Assert.IsTrue(_unmanagedMap.TryGetValue(123, out var value));
            Assert.AreEqual(123 * 1.5f, value);

            Assert.IsFalse(_unmanagedMap.TryGetValue(-1, out _));
        }

        /// <summary>
        /// Tests the unmanaged map enumeration.
        /// </summary>
        [TestMethod]
        public void TestUnmanagedMapEnumeration()
        {
            var count = 0;

            foreach (var kvp in _unmanagedMap)
            {
                Assert.AreEqual(kvp.Key * 1.5f, kvp.Value);
                count++;
            }

            Assert.AreEqual(10_000, count);
        }

        /// <summary>
        /// Tests the initialization and lookup performance comparison.
        /// </summary>
        [TestMethod]
        public void TestInitializationAndLookupPerformanceComparison()
        {
            const int iterations = 100_000;
            var stringData = new Dictionary<int, string>(iterations);
            var floatData = new Dictionary<int, float>(iterations);

            for (var i = 0; i < iterations; i++)
            {
                stringData[i] = $"Value_{i}";
                floatData[i] = i * 1.5f;
            }

            // --- Initialization: ImmutableLookupMap<string>
            var swInit1 = Stopwatch.StartNew();
            var stringMap = new ImmutableLookupMap<int, string>(stringData);
            swInit1.Stop();

            // --- Initialization: ReadOnlyDictionary<string>
            var swInit2 = Stopwatch.StartNew();
            var readOnlyMap = new ReadOnlyDictionary<int, string>(stringData);
            swInit2.Stop();

            // --- Initialization: ImmutableLookupMapUnmanaged<float>
            var swInit3 = Stopwatch.StartNew();
            var unmanagedMap = new ImmutableLookupMapUnmanaged<int, float>(floatData);
            swInit3.Stop();

            // --- Lookup: ImmutableLookupMap<string>
            var swLookup1 = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                _ = stringMap.Get(i);
            }
            swLookup1.Stop();

            // --- Lookup: ReadOnlyDictionary<string>
            var swLookup2 = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                _ = readOnlyMap[i];
            }
            swLookup2.Stop();

            // --- Lookup: ImmutableLookupMapUnmanaged<float>
            var swLookup3 = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                _ = unmanagedMap.Get(i);
            }
            swLookup3.Stop();

            // Dispose unmanaged
            unmanagedMap.Dispose();

            // --- Output Results
            Trace.WriteLine("====== INITIALIZATION TIME ======");
            Trace.WriteLine($"ImmutableLookupMap<string>:      {swInit1.Elapsed.TotalMilliseconds:F3} ms");
            Trace.WriteLine($"ReadOnlyDictionary<string>:      {swInit2.Elapsed.TotalMilliseconds:F3} ms");
            Trace.WriteLine($"ImmutableLookupMapUnmanaged<float>: {swInit3.Elapsed.TotalMilliseconds:F3} ms");

            Trace.WriteLine("====== LOOKUP TIME ======");
            Trace.WriteLine($"ImmutableLookupMap<string>:      {swLookup1.Elapsed.TotalMilliseconds:F3} ms");
            Trace.WriteLine($"ReadOnlyDictionary<string>:      {swLookup2.Elapsed.TotalMilliseconds:F3} ms");
            Trace.WriteLine($"ImmutableLookupMapUnmanaged<float>: {swLookup3.Elapsed.TotalMilliseconds:F3} ms");

            // Basic sanity assertions
            Assert.AreEqual(iterations, stringMap.ToList().Count);
            Assert.AreEqual(iterations, readOnlyMap.Count);
        }

    }
}
