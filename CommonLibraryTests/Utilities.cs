﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/Utilities.cs
 * PURPOSE:     Test for utility Class
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
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
    }
}
