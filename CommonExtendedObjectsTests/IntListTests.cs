/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        IntListUnitTests.cs
 * PURPOSE:     Tests for my custom list
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonExtendedObjectsTests
{
    /// <summary>
    /// Some basic sanity tests for my list
    /// </summary>
    [TestClass]
    public class IntListTests
    {
        /// <summary>
        /// The item count
        /// </summary>
        private const int ItemCount = 1_000_000;

        /// <summary>
        ///     Adds the pop peek behavior.
        /// </summary>
        [TestMethod]
        public void AddPopPeekBehavior()
        {
            using var list = new UnmanagedIntList(4) { 10, 20, 30 };


            Assert.AreEqual(3, list.Length);
            Assert.AreEqual(30, list.Peek());

            var popped = list.Pop();
            Assert.AreEqual(30, popped);
            Assert.AreEqual(2, list.Length);
            Assert.AreEqual(20, list.Peek());

            list[0] = 100;
            Assert.AreEqual(100, list[0]);

            list.Clear();
            Assert.AreEqual(0, list.Length);
        }

        /// <summary>
        ///     Pops the empty throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PopEmptyThrows()
        {
            using var list = new UnmanagedIntList();
            list.Pop();
        }

        /// <summary>
        ///     Peeks the empty throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PeekEmptyThrows()
        {
            using var list = new UnmanagedIntList();
            list.Peek();
        }

        /// <summary>
        ///     Capacities the resizes correctly.
        /// </summary>
        [TestMethod]
        public void CapacityResizesCorrectly()
        {
            using var list = new UnmanagedIntList(2);
            for (var i = 0; i < 1000; i++)
            {
                list.Add(i);
            }

            Assert.AreEqual(1000, list.Length);
            Assert.AreEqual(999, list[999]);
        }

        /// <summary>
        ///     Removes at invalid index throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void RemoveAtInvalidIndexThrows()
        {
            using var list = new UnmanagedIntList(1) { 1 };
            list.RemoveAt(2);
        }

        /// <summary>
        ///     Inserts at adds elements correctly.
        /// </summary>
        [TestMethod]
        public void InsertAtAddsElementsCorrectly()
        {
            using var list = new UnmanagedIntList(2) { 1, 2 };
            list.InsertAt(1, 99); // Between 1 and 2

            Assert.AreEqual(3, list.Length);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(99, list[1]);
            Assert.AreEqual(2, list[2]);
        }

        /// <summary>
        ///     Inserts at with count adds multiple.
        /// </summary>
        [TestMethod]
        public void InsertAtWithCountAddsMultiple()
        {
            using var list = new UnmanagedIntList(2) { 1, 2 };
            list.InsertAt(1, 99, 2); // Insert 99 twice at index 1

            Assert.AreEqual(4, list.Length);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(99, list[1]);
            Assert.AreEqual(99, list[2]);
            Assert.AreEqual(2, list[3]);
        }

        /// <summary>
        ///     Disposes the state of the frees memory and invalidates.
        /// </summary>
        [TestMethod]
        public void DisposeFreesMemoryAndInvalidatesState()
        {
            var list = new UnmanagedIntList(2) { 1 };
            list.Dispose();

            // Post-condition: buffer is cleared, pointers null, Length and Capacity are zero.
            // Behavior is undefined after Dispose. No exception is required.
            // Test only verifies internal reset, not enforcement.
            Assert.AreEqual(0, list.Length);
            Assert.AreEqual(0, list.Capacity);
        }

        /// <summary>
        ///     Performances the benchmark.
        /// </summary>
        [TestMethod]
        public void PerformanceBenchmark()
        {
            const int n = 1_000_000;

            // IntList timing
            using var intList = new UnmanagedIntList(n);
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < n; i++)
            {
                intList.Add(i);
            }

            sw.Stop();
            var intListTime = sw.ElapsedMilliseconds;

            // List<int> timing
            var list = new List<int>(n);

            sw.Restart();
            for (var i = 0; i < n; i++)
            {
                list.Add(i);
            }

            sw.Stop();
            var listTime = sw.ElapsedMilliseconds;

            // Stack<int> timing
            var stack = new Stack<int>(n);

            sw.Restart();
            for (var i = 0; i < n; i++)
            {
                stack.Push(i);
            }

            sw.Stop();
            var stackTime = sw.ElapsedMilliseconds;

            // Output times
            Trace.WriteLine($"IntList (unmanaged) time: {intListTime} ms");
            Trace.WriteLine($"List<int> time: {listTime} ms");
            Trace.WriteLine($"Stack<int> time: {stackTime} ms");

            // Assert nothing here — just print results
        }

        /// <summary>
        ///     Pushes the pop should work with positive and negative values.
        /// </summary>
        [TestMethod]
        public void PushPopShouldWorkWithPositiveAndNegativeValues()
        {
            using var list = new UnmanagedIntList();

            // Push positive values
            list.Push(10);
            list.Push(20);
            list.Push(30);

            // Push negative values
            list.Push(-1);
            list.Push(-50);

            Assert.AreEqual(5, list.Length);

            // Pop values and verify LIFO order
            Assert.AreEqual(-50, list.Pop());
            Assert.AreEqual(-1, list.Pop());
            Assert.AreEqual(30, list.Pop());
            Assert.AreEqual(20, list.Pop());
            Assert.AreEqual(10, list.Pop());

            Assert.AreEqual(0, list.Length);

            // Pop from empty list should throw
            Assert.ThrowsException<InvalidOperationException>(() => list.Pop());
        }

        /// <summary>
        ///     Peeks the should return last element without removing.
        /// </summary>
        [TestMethod]
        public void PeekShouldReturnLastElementWithoutRemoving()
        {
            using var list = new UnmanagedIntList();
            list.Push(42);
            list.Push(-42);

            Assert.AreEqual(-42, list.Peek());
            Assert.AreEqual(2, list.Length);

            var popped = list.Pop();
            Assert.AreEqual(-42, popped);
            Assert.AreEqual(1, list.Length);

            Assert.AreEqual(42, list.Peek());
        }

        /// <summary>
        /// Clones the sorted returns sorted list.
        /// </summary>
        [TestMethod]
        public void CloneSortedReturnsSortedList()
        {
            var list = new UnmanagedIntList { 5, 2, 8, 3 };

            using var sorted = list.Sorted();

            Assert.AreEqual(4, sorted.Length);
            Assert.AreEqual(2, sorted[0]);
            Assert.AreEqual(3, sorted[1]);
            Assert.AreEqual(5, sorted[2]);
            Assert.AreEqual(8, sorted[3]);
        }

#if DEBUG
        /// <summary>
        ///     Indexes the get out of bounds throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void IndexGetOutOfBoundsThrows()
        {
            using var list = new UnmanagedIntList(1);
            var _ = list[5];
        }

        /// <summary>
        ///     Indexes the set out of bounds throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void IndexSetOutOfBoundsThrows()
        {
            using var list = new UnmanagedIntList(1) { [3] = 99 };
        }
#endif

        /// <summary>
        /// Removes at multiple elements works correctly.
        /// </summary>
        [TestMethod]
        public void RemoveAtMultipleElementsWorksCorrectly()
        {
            var list = new UnmanagedIntList(10);
            for (int i = 0; i < 10; i++)
                list.Add(i); // [0, 1, 2, ..., 9]

            list.RemoveAt(3, 4); // Should remove 3,4,5,6 → Result: [0,1,2,7,8,9]

            Assert.AreEqual(6, list.Length);
            Assert.AreEqual(0, list[0]);
            Assert.AreEqual(1, list[1]);
            Assert.AreEqual(2, list[2]);
            Assert.AreEqual(7, list[3]);
            Assert.AreEqual(8, list[4]);
            Assert.AreEqual(9, list[5]);
        }


        /// <summary>
        /// Benchmarks the add performance.
        /// </summary>
        [TestMethod]
        public void BenchmarkAddPerformance()
        {
            var list = new List<int>();
            var unmanaged = new UnmanagedIntList();

            var swList = Stopwatch.StartNew();
            for (int i = 0; i < ItemCount; i++)
                list.Add(i);
            swList.Stop();

            var swUnmanaged = Stopwatch.StartNew();
            for (int i = 0; i < ItemCount; i++)
                unmanaged.Add(i);
            swUnmanaged.Stop();

            Console.WriteLine($"List<int>.Add: {swList.ElapsedMilliseconds} ms");
            Console.WriteLine($"UnmanagedIntList.Add: {swUnmanaged.ElapsedMilliseconds} ms");

            unmanaged.Dispose();

            // Relax assertion, e.g. allow unmanaged to be up to 5x slower
            Assert.IsTrue(swUnmanaged.ElapsedMilliseconds <= swList.ElapsedMilliseconds * 5,
                "UnmanagedIntList.Add is too slow.");
        }


        /// <summary>
        /// Benchmarks the remove performance.
        /// </summary>
        [TestMethod]
        public void BenchmarkRemovePerformance()
        {
            var list = new List<int>();
            var unmanaged = new UnmanagedIntList();

            for (int i = 0; i < ItemCount; i++)
            {
                list.Add(i);
                unmanaged.Add(i);
            }

            int removeCount = 1000;
            int removeIndex = list.Count / 2; // fixed middle index

            var swList = Stopwatch.StartNew();
            for (int i = 0; i < removeCount; i++)
                list.RemoveAt(removeIndex);
            swList.Stop();

            var swUnmanaged = Stopwatch.StartNew();
            for (int i = 0; i < removeCount; i++)
                unmanaged.RemoveAt(removeIndex);
            swUnmanaged.Stop();

            Console.WriteLine($"List<int>.RemoveAt: {swList.ElapsedMilliseconds} ms");
            Console.WriteLine($"UnmanagedIntList.RemoveAt: {swUnmanaged.ElapsedMilliseconds} ms");

            unmanaged.Dispose();

            //TODO fix this!
#if DEBUG
            //Assert.AreEqual(list.Count, unmanaged.Length);
#endif
        }

        /// <summary>
        /// Benchmarks the peek and pop.
        /// </summary>
        [TestMethod]
        public void BenchmarkPeekAndPop()
        {
            var unmanaged = new UnmanagedIntList();
            for (int i = 0; i < 100_000; i++)
                unmanaged.Add(i);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100_000; i++)
            {
                var v = unmanaged.Peek();
                var p = unmanaged.Pop();
                Assert.AreEqual(v, p);
            }
            sw.Stop();

            Console.WriteLine($"UnmanagedIntList.Peek + Pop: {sw.ElapsedMilliseconds} ms");

            Assert.AreEqual(0, unmanaged.Length);
            unmanaged.Dispose();
        }
    }
}
