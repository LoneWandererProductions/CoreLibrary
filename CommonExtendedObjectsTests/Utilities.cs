/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        CommonExtendedObjectsTests/Utilities.cs
 * PURPOSE:     Test for utility Class
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonExtendedObjectsTests
{
    /// <summary>
    ///     Basic tests for the utility Class
    /// </summary>
    [TestClass]
    public class Utilities
    {
        /// <summary>
        ///     Checks if we get the correct Indexes
        /// </summary>
        [TestMethod]
        public void GetAvailableIndex()
        {
            var lst = new List<int>
            {
                0,
                2,
                3,
                4,
                6
            };

            var index = Utility.GetFirstAvailableIndex(lst);

            Assert.AreEqual(1, index, $"Index was false{index}");

            var keys = Utility.GetAvailableIndexes(lst, 3);

            Assert.AreEqual(1, keys[0], $"Index was false{keys[0]}");
            Assert.AreEqual(5, keys[1], $"Index was false{keys[1]}");
            Assert.AreEqual(7, keys[2], $"Index was false{keys[2]}");
        }

        /// <summary>
        ///     Check the Previous and  the next element.
        /// </summary>
        [TestMethod]
        public void PreviousNextElement()
        {
            var lst = new List<int> { 0, 3, 6 };

            var index = Utility.GetNextElement(3, lst);
            Assert.AreEqual(6, index, $"Index was false{index}");
            index = Utility.GetNextElement(6, lst);
            Assert.AreEqual(0, index, $"Index was false{index}");
            index = Utility.GetPreviousElement(3, lst);
            Assert.AreEqual(0, index, $"Index was false{index}");
            index = Utility.GetPreviousElement(0, lst);
            Assert.AreEqual(6, index, $"Index was false{index}");
            index = Utility.GetPreviousElement(1, lst);
            Assert.AreEqual(6, index, $"Index was false{index}");
            index = Utility.GetNextElement(1, lst);
            Assert.AreEqual(0, index, $"Index was false{index}");
        }

        /// <summary>
        ///     Check if we can find sequences in an int list
        /// </summary>
        [TestMethod]
        public void Sequencer()
        {
            var lst = new List<int>
            {
                0,
                2,
                3,
                4,
                6,
                7,
                9
            };

            var result = Utility.Sequencer(lst, 2);

            Assert.AreEqual(2, result.Count, "Sequence 2");

            result = Utility.Sequencer(lst, 3);

            Assert.AreEqual(3, result.Count, 3, "Sequence 4");

            //Image width = 2
            //0,2,4,6,8,10
            //xx
            //xx
            //xx
            //xx
            lst = new List<int>
            {
                0,
                2,
                3,
                5,
                4,
                6
            };

            result = Utility.Sequencer(lst, 2, 2);

            Assert.AreEqual(2, result.Count, "Sequence 2");

            Assert.AreEqual(6, result[0].Value, "Sequence 2");
            Assert.AreEqual(5, result[1].Value, "Sequence 2");
        }

        /// <summary>
        ///     Finds the sequences should return correct result when there are sequences.
        /// </summary>
        [TestMethod]
        public void FindSequencesShouldReturnCorrectResultWhenThereAreSequences()
        {
            // Arrange
            var numbers = new List<int>
            {
                1,
                1,
                2,
                4,
                4,
                5,
                6,
                6,
                2,
                3,
                3
            };

            // Act
            var result = Utility.FindSequences(numbers);

            // Assert
            Assert.AreEqual(7, result.Count);
            Assert.AreEqual((0, 1, 1), result[0]);
            Assert.AreEqual((2, 2, 2), result[1]);
            Assert.AreEqual((3, 4, 4), result[2]);
            Assert.AreEqual((5, 5, 5), result[3]);
            Assert.AreEqual((6, 7, 6), result[4]);
            Assert.AreEqual((8, 8, 2), result[5]);
            Assert.AreEqual((9, 10, 3), result[6]);
        }


        /// <summary>
        ///     Finds the sequences should return empty when list is empty.
        /// </summary>
        [TestMethod]
        public void FindSequencesShouldReturnEmptyWhenListIsEmpty()
        {
            // Arrange
            var numbers = new List<int>();

            // Act
            var result = Utility.FindSequences(numbers);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        /// <summary>
        ///     Finds the sequences should return single sequence when all values are same.
        /// </summary>
        [TestMethod]
        public void FindSequencesShouldReturnSingleSequenceWhenAllValuesAreSame()
        {
            // Arrange
            var numbers = new List<int> { 7, 7, 7, 7 };

            // Act
            var result = Utility.FindSequences(numbers);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual((0, 3, 7), result[0]);
        }

        /// <summary>
        ///     Finds the sequences should handle single element list.
        /// </summary>
        [TestMethod]
        public void FindSequencesShouldHandleSingleElementList()
        {
            // Arrange
            var numbers = new List<int> { 10 };

            // Act
            var result = Utility.FindSequences(numbers);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual((0, 0, 10), result[0]);
        }

        /// <summary>
        ///     Finds the sequences should return correct result when no sequences.
        /// </summary>
        [TestMethod]
        public void FindSequencesShouldReturnCorrectResultWhenNoSequences()
        {
            // Arrange
            var numbers = new List<int>
            {
                1,
                2,
                3,
                4,
                5
            };

            // Act
            var result = Utility.FindSequences(numbers);

            // Assert
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual((0, 0, 1), result[0]);
            Assert.AreEqual((1, 1, 2), result[1]);
            Assert.AreEqual((2, 2, 3), result[2]);
            Assert.AreEqual((3, 3, 4), result[3]);
            Assert.AreEqual((4, 4, 5), result[4]);
        }

        /// <summary>
        ///     Finds the sequences should return correct result when list has mixed sequences.
        /// </summary>
        [TestMethod]
        public void FindSequencesShouldReturnCorrectResultWhenListHasMixedSequences()
        {
            // Arrange
            var numbers = new List<int>
            {
                1,
                1,
                2,
                4,
                4,
                5,
                5,
                5,
                6,
                6,
                7
            };

            // Act
            var result = Utility.FindSequences(numbers);

            // Assert
            Assert.AreEqual(6, result.Count);
            Assert.AreEqual((0, 1, 1), result[0]);
            Assert.AreEqual((2, 2, 2), result[1]);
            Assert.AreEqual((3, 4, 4), result[2]);
            Assert.AreEqual((5, 7, 5), result[3]);
            Assert.AreEqual((8, 9, 6), result[4]);
            Assert.AreEqual((10, 10, 7), result[5]);
        }

        /// <summary>
        ///     Binaries the search performance and correctness.
        /// </summary>
        [TestMethod]
        public void BinarySearchPerformanceAndCorrectness()
        {
            const int n = 1_000_000;
            var sortedKeys = new int[n];
            for (var i = 0; i < n; i++)
            {
                sortedKeys[i] = i * 2; // even numbers sorted
            }

            var keysSpan = sortedKeys.AsSpan();

            // Warm-up to avoid JIT bias
            for (var i = 0; i < 10_000; i++)
            {
                Utility.BinarySearch(keysSpan, i * 2);
            }

            var sw = Stopwatch.StartNew();

            var foundCount = 0;
            for (var i = 0; i < n; i++)
            {
                var val = i * 2;
                var idx = Utility.BinarySearch(keysSpan, val);

                Assert.IsTrue(idx >= 0, $"Key {val} should be found.");
                Assert.AreEqual(val, sortedKeys[idx]);

                foundCount++;
            }

            sw.Stop();

            Trace.WriteLine($"BinarySearch found {foundCount} keys out of {n} in {sw.ElapsedMilliseconds} ms");

            // Optional: Assert performance threshold (example: must finish under 200ms)
            Assert.IsTrue(sw.ElapsedMilliseconds < 1000, $"BinarySearch took too long: {sw.ElapsedMilliseconds} ms");
        }

        /// <summary>
        ///     Compares the custom vs array binary search performance.
        /// </summary>
        [TestMethod]
        public void CompareCustomVsArrayBinarySearchPerformance()
        {
            const int n = 1_000_000;
            var sortedKeys = new int[n];
            for (var i = 0; i < n; i++)
            {
                sortedKeys[i] = i * 2; // even numbers sorted
            }

            var keysSpan = sortedKeys.AsSpan();

            // Warm-up both methods
            for (var i = 0; i < 10_000; i++)
            {
                Utility.BinarySearch(keysSpan, i * 2);
                Array.BinarySearch(sortedKeys, i * 2);
            }

            // Custom BinarySearch benchmark
            var swCustom = Stopwatch.StartNew();
            for (var i = 0; i < n; i++)
            {
                var val = i * 2;
                var idx = Utility.BinarySearch(keysSpan, n, val);
                Assert.IsTrue(idx >= 0);
            }

            swCustom.Stop();

            // Array.BinarySearch benchmark
            var swArray = Stopwatch.StartNew();
            for (var i = 0; i < n; i++)
            {
                var val = i * 2;
                var idx = Array.BinarySearch(sortedKeys, val);
                Assert.IsTrue(idx >= 0);
            }

            swArray.Stop();

            Trace.WriteLine($"Custom BinarySearch: {swCustom.ElapsedMilliseconds} ms");
            Trace.WriteLine($"Array.BinarySearch: {swArray.ElapsedMilliseconds} ms");

            // Optionally assert your method is within some factor of Array.BinarySearch
            Assert.IsTrue(swCustom.ElapsedMilliseconds < swArray.ElapsedMilliseconds * 3,
                $"Custom BinarySearch is too slow: {swCustom.ElapsedMilliseconds} ms vs {swArray.ElapsedMilliseconds} ms");
        }

        /// <summary>
        ///     Binaries the search bug.
        /// </summary>
        [TestMethod]
        public void BinarySearchBug()
        {
            var sortedKeys = new int[128];

            for (var i = 0; i < 10; i++)
            {
                sortedKeys[i] = i + 1;
            }

            for (var i = 10; i < 128; i++)
            {
                sortedKeys[i] = 0;
            }

            var idx = Utility.BinarySearch(sortedKeys, 14, 10);

            Assert.AreEqual(-15, idx, "Found the bug.");
        }
    }
}
