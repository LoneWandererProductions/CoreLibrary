/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        CommonExtendedObjectsTests/SortedKvStoreTests.cs
 * PURPOSE:     A generic test for my SortedKvStore
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using System.Linq;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonExtendedObjectsTests
{
    /// <summary>
    ///     Test my Key Value class
    /// </summary>
    [TestClass]
    public class SortedKvStoreTests
    {
        /// <summary>
        ///     The item count
        /// </summary>
        private const int ItemCount = 100_000;

        /// <summary>
        ///     Adds the and try get works.
        /// </summary>
        [TestMethod]
        public void AddAndTryGetWorks()
        {
            var store = new SortedKvStore();
            store.Add(10, 100);
            store.Add(5, 50);
            store.Add(15, 150);

            Assert.IsTrue(store.TryGetValue(10, out var val10));
            Assert.AreEqual(100, val10);

            Assert.IsTrue(store.TryGetValue(5, out var val5));
            Assert.AreEqual(50, val5);

            Assert.IsTrue(store.TryGetValue(15, out var val15));
            Assert.AreEqual(150, val15);

            Assert.IsFalse(store.TryGetValue(99, out _));
        }

        /// <summary>
        ///     Tries the remove removes existing.
        /// </summary>
        [TestMethod]
        public void TryRemoveRemovesExisting()
        {
            var store = new SortedKvStore();
            store.Add(1, 10);
            store.Add(2, 20);

            Assert.IsTrue(store.TryRemove(1, out var idx));
            Assert.IsFalse(store.TryGetValue(1, out _));
            Assert.IsTrue(idx >= 0);
        }

        /// <summary>
        ///     Removes the many works.
        /// </summary>
        [TestMethod]
        public void RemoveManyWorks()
        {
            var store = new SortedKvStore();
            for (var i = 0; i < 10; i++)
            {
                store.Add(i, i * 10);
            }

            store.RemoveMany(new[] { 2, 4, 6 });

            Assert.IsFalse(store.TryGetValue(2, out _));
            Assert.IsFalse(store.TryGetValue(4, out _));
            Assert.IsFalse(store.TryGetValue(6, out _));

            Assert.IsTrue(store.TryGetValue(3, out _));
        }

        /// <summary>
        ///     Compacts the removes unoccupied.
        /// </summary>
        [TestMethod]
        public void CompactRemovesUnoccupied()
        {
            var store = new SortedKvStore();
            for (var i = 0; i < 10; i++)
            {
                store.Add(i, i * 10);
            }

            store.Remove(1);
            store.Remove(3);
            store.Compact();

            Assert.AreEqual(8, store.Count);
            Assert.IsFalse(store.TryGetValue(1, out _));
        }

        /// <summary>
        ///     Indexers the works.
        /// </summary>
        [TestMethod]
        public void IndexerWorks()
        {
            var store = new SortedKvStore();
            store[42] = 123;
            Assert.AreEqual(123, store[42]);

            store[42] = 456;
            Assert.AreEqual(456, store[42]);
        }

        /// <summary>
        ///     Adds the and try get should return expected values.
        /// </summary>
        [TestMethod]
        public void AddAndTryGetShouldReturnExpectedValues()
        {
            var store = new SortedKvStore();

            for (var i = 0; i < 1000; i++)
            {
                store.Add(i, i * 10);
            }

            for (var i = 0; i < 1000; i++)
            {
                Assert.IsTrue(store.TryGetValue(i, out var value));
                Assert.AreEqual(i * 10, value);
            }

            Assert.IsFalse(store.TryGetValue(1001, out _));
        }

        /// <summary>
        ///     Performances the add and try get.
        /// </summary>
        [TestMethod]
        public void PerformanceAddAndTryGet()
        {
            var store = new SortedKvStore(ItemCount);

            var sw = Stopwatch.StartNew();

            for (var i = 0; i < ItemCount; i++)
            {
                store.Add(i, i * 2);
            }

            sw.Stop();
            Trace.WriteLine($"Add {ItemCount} items: {sw.ElapsedMilliseconds} ms");

            sw.Restart();

            var hits = 0;
            for (var i = 0; i < ItemCount; i++)
            {
                if (store.TryGetValue(i, out var value) && value == i * 2)
                {
                    hits++;
                }
            }

            sw.Stop();
            Trace.WriteLine($"TryGet {ItemCount} items: {sw.ElapsedMilliseconds} ms");

            Assert.AreEqual(ItemCount, hits);
        }

        /// <summary>
        ///     Performances the remove compact.
        /// </summary>
        [TestMethod]
        public void PerformanceRemoveCompact()
        {
            var store = new SortedKvStore(ItemCount);

            for (var i = 0; i < ItemCount; i++)
            {
                store.Add(i, i);
            }

            // Remove half of them
            for (var i = 0; i < ItemCount; i += 2)
            {
                store.Remove(i);
            }

            var sw = Stopwatch.StartNew();
            store.Compact();
            sw.Stop();

            Trace.WriteLine($"Compact after removing half: {sw.ElapsedMilliseconds} ms");
            Assert.AreEqual(ItemCount / 2, store.Count);
        }

        /// <summary>
        /// Inserts the key1 into empty store should keep arrays in synchronize.
        /// </summary>
        [TestMethod]
        public void InsertKey1IntoEmptyStoreShouldKeepArraysInSync()
        {
            // Arrange
            var store = new SortedKvStore();

            // Act
            store.Add(1, 42); // Insert a single key-value pair with key = 1

            // Assert
            Assert.AreEqual(1, store.Count, "Store should have one occupied entry.");

            var keys = store.Keys.ToArray();
            Assert.AreEqual(1, keys.Length, "Keys enumerable should contain one item.");
            Assert.AreEqual(1, keys[0], "Inserted key should be 1.");

            Assert.IsTrue(store.ContainsKey(1), "ContainsKey(1) should return true.");
            Assert.IsFalse(store.ContainsKey(0), "ContainsKey(0) should return false.");

            Assert.AreEqual(42, store[1], "Value for key 1 should be 42.");

            // Enumerate all key-value pairs and check alignment
            var kvList = store.ToList();
            Assert.AreEqual(1, kvList.Count, "Store should enumerate one key-value pair.");
            Assert.AreEqual(1, kvList[0].Key, "Enumerated key should be 1.");
            Assert.AreEqual(42, kvList[0].Value, "Enumerated value should be 42.");
        }

        /// <summary>
        /// Inserts the keys that force shift should keep arrays in synchronize.
        /// </summary>
        [TestMethod]
        public void InsertKeysThatForceShiftShouldKeepArraysInSync()
        {
            // Arrange
            var store = new SortedKvStore();

            // Insert a higher key first, which will go at index 0
            store.Add(2, 100);

            // Now insert a smaller key, which should shift existing elements
            store.Add(1, 50); // Will be inserted at index 0, causes shift right

            // Assert
            Assert.AreEqual(2, store.Count, "Store should have two occupied entries.");

            var keys = store.Keys.ToArray();
            CollectionAssert.AreEqual(new[] { 1, 2 }, keys, "Keys should be sorted and aligned.");

            var values = store.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Assert.AreEqual(50, values[1], "Value for key 1 should be 50.");
            Assert.AreEqual(100, values[2], "Value for key 2 should be 100.");

            Assert.IsTrue(store.ContainsKey(1), "ContainsKey(1) should return true.");
            Assert.IsTrue(store.ContainsKey(2), "ContainsKey(2) should return true.");
        }

        /// <summary>
        /// Inserts the remove compact reinsert should keep consistency.
        /// </summary>
        [TestMethod]
        public void InsertRemoveCompactReinsertShouldKeepConsistency()
        {
            var store = new SortedKvStore();

            store.Add(1, 10);
            store.Add(2, 20);

            store.Remove(1);      // Mark key 1 as removed
            store.Compact();      // Actually removes unoccupied entries

            store.Add(1, 30);     // Reinsert key 1

            var keys = store.Keys.ToArray();
            CollectionAssert.AreEqual(new[] { 1, 2 }, keys);

            Assert.AreEqual(30, store[1]);
            Assert.AreEqual(20, store[2]);
        }


    }
}
