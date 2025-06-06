/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        ImmutableLookupMapTests.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     Generic tests for speed.
    /// </summary>
    [TestClass]
    public class PerformanceTests
    {
        private Dictionary<int, string> _data;
        private ImmutableLookupMap<int, string> _immutableLookupMap;
        private ReadOnlyDictionary<int, string> _readOnlyDictionary;

        /// <summary>
        ///     Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _data = new Dictionary<int, string>();
            for (var i = 0; i < 10_000; i++)
            {
                _data[i] = $"Value_{i}";
            }

            _immutableLookupMap = new ImmutableLookupMap<int, string>(_data);
            _readOnlyDictionary = new ReadOnlyDictionary<int, string>(_data);
        }

        /// <summary>
        ///     Tests the immutable lookup map initialization.
        /// </summary>
        [TestMethod]
        public void TestImmutableLookupMapInitialization()
        {
            var stopwatch = Stopwatch.StartNew();
            _ = new ImmutableLookupMap<int, string>(_data);
            stopwatch.Stop();

            Trace.WriteLine($"ImmutableLookupMap Initialization Time: {stopwatch.ElapsedMilliseconds} ms");
        }


        /// <summary>
        ///     Tests the read only dictionary initialization.
        /// </summary>
        [TestMethod]
        public void TestReadOnlyDictionaryInitialization()
        {
            var stopwatch = Stopwatch.StartNew();
            _ = new ReadOnlyDictionary<int, string>(_data);
            stopwatch.Stop();

            Trace.WriteLine($"ReadOnlyDictionary Initialization Time: {stopwatch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        ///     Tests the immutable lookup map lookup.
        /// </summary>
        [TestMethod]
        public void TestImmutableLookupMapLookup()
        {
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < 10_000; i++)
            {
                var value = _immutableLookupMap.Get(i);
                Assert.AreEqual($"Value_{i}", value); // Verifying correctness
            }

            stopwatch.Stop();
            Trace.WriteLine($"ImmutableLookupMap Lookup Time: {stopwatch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        ///     Tests the read only dictionary lookup.
        /// </summary>
        [TestMethod]
        public void TestReadOnlyDictionaryLookup()
        {
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < 10_000; i++)
            {
                var value = _readOnlyDictionary[i]; // Accessing keys from ReadOnlyDictionary
                Assert.AreEqual($"Value_{i}", value); // Verifying correctness
            }

            stopwatch.Stop();

            Trace.WriteLine($"ReadOnlyDictionary Lookup Time: {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
