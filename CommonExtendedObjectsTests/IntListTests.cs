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
    [TestClass]
    public class IntListTests
    {
        /// <summary>
        ///     Adds the pop peek behavior.
        /// </summary>
        [TestMethod]
        public void AddPopPeekBehavior()
        {
            using var list = new IntList(4);

            list.Add(10);
            list.Add(20);
            list.Add(30);

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
            using var list = new IntList();
            list.Pop();
        }

        /// <summary>
        ///     Peeks the empty throws.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PeekEmptyThrows()
        {
            using var list = new IntList();
            list.Peek();
        }

        /// <summary>
        ///     Capacities the resizes correctly.
        /// </summary>
        [TestMethod]
        public void CapacityResizesCorrectly()
        {
            using var list = new IntList(2);
            for (var i = 0; i < 1000; i++)
            {
                list.Add(i);
            }

            Assert.AreEqual(1000, list.Length);
            Assert.AreEqual(999, list[999]);
        }

        /// <summary>
        ///     Performances the benchmark.
        /// </summary>
        [TestMethod]
        public void PerformanceBenchmark()
        {
            const int n = 1_000_000;

            // IntList timing
            using var intList = new IntList(n);
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
    }
}
