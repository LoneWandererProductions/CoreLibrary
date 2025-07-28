/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        IntArrayTests.cs
 * PURPOSE:     Tests for my custom array
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using ExtendedSystemObjects;
using ExtendedSystemObjects.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonExtendedObjectsTests
{
    [TestClass]
    public class IntArrayTests
    {
        /// <summary>
        ///     The size
        /// </summary>
        private const int Size = 1_000_000;

        /// <summary>
        ///     Indexing the should get and set values correctly.
        /// </summary>
        [TestMethod]
        public void IndexingShouldGetAndSetValuesCorrectly()
        {
            var arr = new UnmanagedIntArray(5);
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = i * 10;
            }

            for (var i = 0; i < arr.Length; i++)
            {
                Assert.AreEqual(i * 10, arr[i]);
            }

            arr.Dispose();
        }

        /// <summary>
        ///     Removes at should remove element and shift left.
        /// </summary>
        [TestMethod]
        public void RemoveAtShouldRemoveElementAndShiftLeft()
        {
            var arr = new UnmanagedIntArray(5);
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = i;
            }

            arr.RemoveAt(2);

            Assert.AreEqual(4, arr.Length);
            Assert.AreEqual(3, arr[2]);

            arr.Dispose();
        }

        /// <summary>
        ///     Resizes the should change length and preserve old data.
        /// </summary>
        [TestMethod]
        public void ResizeShouldChangeLengthAndPreserveOldData()
        {
            var arr = new UnmanagedIntArray(3);
            arr[0] = 1;
            arr[1] = 2;
            arr[2] = 3;

            arr.Resize(5);

            Assert.AreEqual(3, arr.Length);
            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(2, arr[1]);
            Assert.AreEqual(3, arr[2]);

            arr.Dispose();
        }

        /// <summary>
        ///     Clears the should zero out all elements.
        /// </summary>
        [TestMethod]
        public void ClearShouldZeroOutAllElements()
        {
            var arr = new UnmanagedIntArray(4);
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = 42;
            }

            arr.Clear();

            foreach (var t in arr)
            {
                Assert.AreEqual(0, t);
            }

            arr.Dispose();
        }

        /// <summary>
        ///     Compares the int array vs int array dot net performance.
        /// </summary>
        [TestMethod]
        public void CompareIntArrayvsIntArrayDotNetPerformance()
        {
            var stopwatch = new Stopwatch();

            // === IntArray Test ===
            var intArray = new UnmanagedIntArray(Size);
            stopwatch.Restart();

            for (var i = 0; i < Size; i++)
            {
                intArray[i] = i;
            }

            long intArraySum = 0;
            for (var i = 0; i < Size; i++)
            {
                intArraySum += intArray[i];
            }

            stopwatch.Stop();
            var customTime = stopwatch.ElapsedMilliseconds;
            intArray.Dispose();

            Trace.WriteLine($"IntArray (unmanaged) time: {customTime} ms");

            // === Native int[] Test ===
            var nativeArray = new int[Size];
            stopwatch.Restart();

            for (var i = 0; i < Size; i++)
            {
                nativeArray[i] = i;
            }

            long nativeSum = 0;
            for (var i = 0; i < Size; i++)
            {
                nativeSum += nativeArray[i];
            }

            stopwatch.Stop();
            var nativeTime = stopwatch.ElapsedMilliseconds;

            Trace.WriteLine($"int[] (managed) time: {nativeTime} ms");

            // === Sanity Check ===
            const long expected = (long)(Size - 1) * Size / 2;
            Assert.AreEqual(expected, intArraySum, "IntArray sum mismatch");
            Assert.AreEqual(expected, nativeSum, "int[] sum mismatch");

            // Optional performance sanity check (loose threshold)
            Assert.IsTrue(customTime < 2000, $"IntArray took too long: {customTime}ms");
            Assert.IsTrue(nativeTime < 2000, $"int[] took too long: {nativeTime}ms");

            // === UnmanagedArray<T> Test ===
            var genericArray = new UnmanagedArray<int>(Size);
            stopwatch.Restart();

            for (var i = 0; i < Size; i++)
            {
                genericArray[i] = i;
            }

            long genericSum = 0;
            for (var i = 0; i < Size; i++)
            {
                genericSum += genericArray[i];
            }

            stopwatch.Stop();
            var genericTime = stopwatch.ElapsedMilliseconds;
            genericArray.Dispose();

            Trace.WriteLine($"UnmanagedArray<int> time: {genericTime} ms");
            Assert.AreEqual(expected, genericSum, "UnmanagedArray sum mismatch");
        }

        /// <summary>
        ///     Indexings the should work for int array.
        /// </summary>
        [TestMethod]
        public void IndexingShouldWorkForIntArray()
        {
            RunIndexingTest(new UnmanagedIntArray(5));
        }

        /// <summary>
        ///     Indexings the should work for unmanaged array.
        /// </summary>
        [TestMethod]
        public void IndexingShouldWorkForUnmanagedArray()
        {
            RunIndexingTest(new UnmanagedArray<int>(5));
        }

        /// <summary>
        ///     Runs the indexing test.
        /// </summary>
        /// <param name="arr">The arr.</param>
        private static void RunIndexingTest(IUnmanagedArray<int> arr)
        {
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = i * 10;
            }

            for (var i = 0; i < arr.Length; i++)
            {
                Assert.AreEqual(i * 10, arr[i]);
            }

            arr.Dispose();
        }

        /// <summary>
        ///     Removes the multiple should remove sequential indices.
        /// </summary>
        [TestMethod]
        public void RemoveMultipleShouldRemoveSequentialIndices()
        {
            var arr = new UnmanagedIntArray(10);
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = i + 1; // [1..10]
            }

            var toRemove = new[] { 3, 4, 5 }; // remove elements at indices 3,4,5 (4th,5th,6th elements)

            var sw = Stopwatch.StartNew();
            arr.RemoveMultiple(toRemove);
            sw.Stop();

            Console.WriteLine($"RemoveMultiple (sequential) took {sw.ElapsedTicks} ticks");

            Assert.AreEqual(7, arr.Length);

            // Remaining should be: 1,2,3,7,8,9,10 (indices:0,1,2,3,4,5,6)
            Assert.AreEqual(1, arr[0]);
            Assert.AreEqual(2, arr[1]);
            Assert.AreEqual(3, arr[2]);
            Assert.AreEqual(7, arr[3]);
            Assert.AreEqual(8, arr[4]);
            Assert.AreEqual(9, arr[5]);
            Assert.AreEqual(10, arr[6]);

            arr.Dispose();
        }

        /// <summary>
        ///     Removes the multiple should remove non sequential indices.
        /// </summary>
        [TestMethod]
        public void RemoveMultipleShouldRemoveNonSequentialIndices()
        {
            var arr = new UnmanagedIntArray(10);
            for (var i = 0; i < arr.Length; i++)
            {
                arr[i] = i + 1; // [1..10]
            }

            var toRemove = new[] { 1, 3, 6 }; // remove elements at indices 1,3,6

            var sw = Stopwatch.StartNew();
            arr.RemoveMultiple(toRemove);
            sw.Stop();

            Console.WriteLine($"RemoveMultiple (non-sequential) took {sw.ElapsedTicks} ticks");

            Assert.AreEqual(7, arr.Length);

            // Remaining elements: indices 0,2,4,5,7,8,9
            // Values: 1, 3, 5, 6, 8, 9, 10
            int[] expected = { 1, 3, 5, 6, 8, 9, 10 };
            for (var i = 0; i < arr.Length; i++)
            {
                Assert.AreEqual(expected[i], arr[i]);
            }

            arr.Dispose();
        }
    }
}
