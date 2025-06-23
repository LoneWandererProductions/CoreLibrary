/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        CommonExtendedObjectsTests/SortedKvStoreTests.cs
 * PURPOSE:     A generic test for my SortedKvStore
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Documents;
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

            // Dump internal state for debug (optional)
            Trace.WriteLine(store.ToString());

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

            // Dump internal state for debug (optional)
            Trace.WriteLine(store.ToString());

            CollectionAssert.AreEqual(new[] { 1, 2 }, keys);

            Assert.AreEqual(30, store[1]);
            Assert.AreEqual(20, store[2]);
        }

        /// <summary>
        /// Inserts the keys starting at one should keep arrays aligned.
        /// </summary>
        [TestMethod]
        public void InsertKeysStartingAtOne_ShouldKeepArraysAligned()
        {
            var store = new SortedKvStore();

            // Insert keys starting from 1 (not 0)
            store.Add(1, 100);
            store.Add(3, 300);
            store.Add(2, 200); // Insert out of order to force shift

            // Validate count
            Assert.AreEqual(3, store.Count, "Count should be 3 after inserts.");

            // Validate keys are sorted
            var keys = store.Keys.ToArray();
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, keys, "Keys should be sorted.");

            // Validate values match keys
            Assert.AreEqual(100, store[1], "Value for key 1 should be 100.");
            Assert.AreEqual(200, store[2], "Value for key 2 should be 200.");
            Assert.AreEqual(300, store[3], "Value for key 3 should be 300.");

            // Remove key 2 and check
            store.Remove(2);
            Assert.IsFalse(store.ContainsKey(2), "Key 2 should be removed.");

            // Compact to physically remove unoccupied entries
            store.Compact();

            Assert.AreEqual(2, store.Count, "Count should be 2 after removal and compact.");

            keys = store.Keys.ToArray();
            CollectionAssert.AreEqual(new[] { 1, 3 }, keys, "Keys after removal should be 1 and 3.");

            // Reinsert key 2 to check insert after compact works
            store.Add(2, 250);
            Assert.IsTrue(store.ContainsKey(2), "Key 2 should exist after reinsertion.");
            Assert.AreEqual(250, store[2], "Value for key 2 should be updated to 250.");

            // Dump internal state for debug (optional)
            Trace.WriteLine(store.ToString());

            // Final keys check to confirm sorted order
            keys = store.Keys.ToArray();
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, keys, "Final keys should be sorted 1, 2, 3.");
        }

        /// <summary>
        /// Removes the many should update count correctly.
        /// </summary>
        [TestMethod]
        public void RemoveManyShouldUpdateCountCorrectly()
        {
            var store = new SortedKvStore
            {
                // Insert 5 keys
                { 1, 100 },
                { 2, 200 },
                { 3, 300 },
                { 4, 400 },
                { 5, 500 }
            };

            Assert.AreEqual(5, store.Count);

            // Remove keys 2 and 4
            var keysToRemove = new int[] { 2, 4 };
            store.RemoveMany(keysToRemove);

            // Dump internal state for debug (optional)
            Trace.WriteLine(store.ToString());

            // Check keys 2 and 4 are removed
            Assert.IsFalse(store.ContainsKey(2));
            Assert.IsFalse(store.ContainsKey(4));

            // Check keys 1, 3, 5 are still present
            Assert.IsTrue(store.ContainsKey(1));
            Assert.IsTrue(store.ContainsKey(3));
            Assert.IsTrue(store.ContainsKey(5));

            // THIS WILL FAIL IF Count IS NOT UPDATED
            Assert.AreEqual(5, store.Count, "Count should NOT be updated after RemoveMany");
        }

        /// <summary>
        /// Removes the many with unsorted keys removes correctly.
        /// </summary>
        [TestMethod]
        public void RemoveManyWithUnsortedKeysRemovesCorrectly()
        {
            var store = new SortedKvStore();

            // Add keys in sorted order
            int[] keys = { 1, 3, 5, 7, 9 };
            foreach (var key in keys)
            {
                store.Add(key, key * 10);
            }

            // Confirm initial state
            Assert.AreEqual(keys.Length, store.Count);
            foreach (var key in keys)
            {
                Assert.IsTrue(store.ContainsKey(key), $"Should contain key {key} initially.");
            }

            // Remove keys (unsorted input)
            int[] keysToRemove = { 7, 3 };
            store.RemoveMany(keysToRemove);

            // Dump internal state for debug (optional)
            Trace.WriteLine(store.ToString());

            // Check keys that should remain
            int[] expectedRemaining = { 1, 5, 9 };
            foreach (var key in expectedRemaining)
            {
                Assert.IsTrue(store.ContainsKey(key), $"Expected to contain key {key} after RemoveMany.");
            }

            // Check keys that should be removed
            foreach (var key in keysToRemove)
            {
                Assert.IsFalse(store.ContainsKey(key), $"Expected NOT to contain key {key} after RemoveMany.");
            }
        }

        /// <summary>
        /// Removes the and compact test.
        /// </summary>
        [TestMethod]
        public void RemoveAndCompactTest()
        {
            var store = new SortedKvStore
            {
                // Add some entries
                { 1, 10 },
                { 3, 30 },
                { 5, 50 },
                { 7, 70 },
                { 9, 90 }
            };

            Assert.IsTrue(store.ContainsKey(3));
            Assert.AreEqual(5, store.Count);

            // Remove key 3 logically
            store.Remove(3);

            // Immediately after remove, key is logically gone
            Assert.IsFalse(store.ContainsKey(3));

            // Count still includes all, including logically removed
            Assert.AreEqual(5, store.Count);

            // Compact physically removes unoccupied entries
            store.Compact();

            // After compact, key is definitely gone
            Assert.IsFalse(store.ContainsKey(3));

            // Count updated to reflect physical removal
            Assert.AreEqual(4, store.Count);

            // Dump internal state for debug (optional)
            Trace.WriteLine(store.ToString());

            // Optional: check that other keys are still present
            Assert.IsTrue(store.ContainsKey(1));
            Assert.IsTrue(store.ContainsKey(5));
            Assert.IsTrue(store.ContainsKey(7));
            Assert.IsTrue(store.ContainsKey(9));
        }

        /// <summary>
        /// Removes the and compact test.
        /// </summary>
        [TestMethod]
        public void BinarySearch()
        {
            var store = new SortedKvStore(128);

            for (int i = 0; i < 10; i++)
            {
                store.Add(i, i);
            }

            for (int i = 5; i < 10; i++)
            {
                store.Remove(i);
            }

            Trace.WriteLine(store.ToString());
            var position = store.BinarySearch(9);

            Assert.AreEqual(9, position, "Wrong position.");
        }
    }
}
