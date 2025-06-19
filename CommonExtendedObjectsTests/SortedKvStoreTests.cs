/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        CommonExtendedObjectsTests/SortedKvStoreTests.cs
 * PURPOSE:     A generic test for my SortedKvStore
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
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

            Assert.IsTrue(store.TryTryGetValueGet(10, out var val10));
            Assert.AreEqual(100, val10);

            Assert.IsTrue(store.TryTryGetValueGet(5, out var val5));
            Assert.AreEqual(50, val5);

            Assert.IsTrue(store.TryTryGetValueGet(15, out var val15));
            Assert.AreEqual(150, val15);

            Assert.IsFalse(store.TryTryGetValueGet(99, out _));
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
            Assert.IsFalse(store.TryTryGetValueGet(1, out _));
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

            Assert.IsFalse(store.TryTryGetValueGet(2, out _));
            Assert.IsFalse(store.TryTryGetValueGet(4, out _));
            Assert.IsFalse(store.TryTryGetValueGet(6, out _));

            Assert.IsTrue(store.TryTryGetValueGet(3, out _));
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
            Assert.IsFalse(store.TryTryGetValueGet(1, out _));
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
                Assert.IsTrue(store.TryTryGetValueGet(i, out var value));
                Assert.AreEqual(i * 10, value);
            }

            Assert.IsFalse(store.TryTryGetValueGet(1001, out _));
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
                if (store.TryTryGetValueGet(i, out var value) && value == i * 2)
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
    }
}
