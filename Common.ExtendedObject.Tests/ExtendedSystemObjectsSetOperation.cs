/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.ExtendedObject.Tests
 * FILE:        ExtendedSystemObjectsSetOperation.cs
 * PURPOSE:     Tests for ExtendedSystemObjects, Set Operations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using ExtendedSystemObjects;

namespace Common.ExtendedObject.Tests
{
    /// <summary>
    ///     Test for Base Set Operations
    /// </summary>
    [TestClass]
    public sealed class ExtendedSystemObjectsSetOperation
    {
        /// <summary>
        ///     Here we Test our List AddReplace
        /// </summary>
        [TestMethod]
        public void ExtendedListRemoveListRange()
        {
            var baseLst = new List<int> { 1, 2, 3 };
            var removeList = new List<int> { 1, 2 };
            baseLst.Difference(removeList);
            Assert.AreEqual(1, baseLst.Count, "Not removed");
            Assert.AreEqual(3, baseLst[0], "Right Element removed");

            baseLst.Add(5);
            baseLst.Add(6);

            var removeLstAlt = new List<int> { 1, 2 };
            baseLst.Difference(removeLstAlt);
            Assert.AreEqual(3, baseLst.Count, "Not removed");
        }

        /// <summary>
        ///     Differences the removes elements in range.
        /// </summary>
        [TestMethod]
        public void DifferenceRemovesElementsInRange()
        {
            var list = new List<int>
            {
                1,
                2,
                3,
                4,
                5
            };
            var range = new List<int> { 2, 4 };
            list.Difference(range);

            CollectionAssert.AreEqual(new List<int> { 1, 3, 5 }, list);
        }

        /// <summary>
        ///     Unions the adds elements from range without duplicates.
        /// </summary>
        [TestMethod]
        public void UnionAddsElementsFromRangeWithoutDuplicates()
        {
            var list = new List<int> { 1, 2, 3 };
            var range = new List<int> { 2, 3, 4, 5 };
            list.Union(range);

            CollectionAssert.AreEquivalent(new List<int>
            {
                1,
                2,
                3,
                4,
                5
            }, list);
        }

        /// <summary>
        ///     Intersections the retains only common elements.
        /// </summary>
        [TestMethod]
        public void IntersectionRetainsOnlyCommonElements()
        {
            var list = new List<int>
            {
                1,
                2,
                3,
                4,
                5
            };
            var range = new List<int> { 2, 4, 6, 8 };
            list.Intersection(range);

            CollectionAssert.AreEqual(new List<int> { 2, 4 }, list);
        }

        /// <summary>
        ///     Symmetrics the difference retains elements not in both lists.
        /// </summary>
        [TestMethod]
        public void SymmetricDifferenceRetainsElementsNotInBothLists()
        {
            var list = new List<int>
            {
                1,
                2,
                3,
                4,
                5
            };
            var range = new List<int> { 2, 4, 6, 8 };
            list.SymmetricDifference(range);

            CollectionAssert.AreEquivalent(new List<int>
            {
                1,
                3,
                5,
                6,
                8
            }, list);
        }

        /// <summary>
        ///     Intersections the retains non common elements when inverted.
        /// </summary>
        [TestMethod]
        public void IntersectionRetainsNonCommonElementsWhenInverted()
        {
            var list = new List<int>
            {
                1,
                2,
                3,
                4,
                5
            };
            var range = new List<int> { 2, 4, 6, 8 };
            list.Intersection(range, true);

            CollectionAssert.AreEquivalent(new List<int>
            {
                1,
                3,
                5,
                6,
                8
            }, list);
        }

        /// <summary>
        ///     Differences the adds elements in range when inverted.
        /// </summary>
        [TestMethod]
        public void DifferenceAddsElementsInRangeWhenInverted()
        {
            var list = new List<int> { 1, 2, 3 };
            var range = new List<int> { 2, 4 };
            list.Difference(range, true);

            CollectionAssert.AreEquivalent(new List<int> { 1, 2, 3, 4 }, list);
        }

        /// <summary>
        /// Unions the removes elements from range when inverted.
        /// </summary>
        [TestMethod]
        public void UnionRemovesElementsFromRangeWhenInverted()
        {
            var list = new List<int>
            {
                1,
                2,
                3,
                4,
                5
            };
            var range = new List<int> { 2, 4 };
            list.Union(range, true);

            CollectionAssert.AreEquivalent(new List<int> { 1, 3, 5 }, list);
        }


        /// <summary>
        ///     Symmetric difference retains common elements when inverted.
        /// </summary>
        [TestMethod]
        public void SymmetricDifferenceRetainsCommonElementsWhenInverted()
        {
            var list = new List<int>
            {
                1,
                2,
                3,
                4,
                5
            };
            var range = new List<int> { 2, 4, 6, 8 };
            list.SymmetricDifference(range, true);

            CollectionAssert.AreEqual(new List<int> { 2, 4 }, list);
        }

        /// <summary>
        /// Removes the fast item in middle removes item and swaps last.
        /// </summary>
        [TestMethod]
        public void RemoveFast_ItemInMiddle_RemovesItemAndSwapsLast()
        {
            // Arrange
            var list = new List<string> { "A", "B", "C", "D" };

            // Act
            list.RemoveFast("B");

            // Assert
            Assert.AreEqual(3, list.Count);
            Assert.IsFalse(list.Contains("B"));
            Assert.AreEqual("D", list[1]); // "D" should have moved to index 1
        }

        /// <summary>
        /// Removes the fast item not found does nothing.
        /// </summary>
        [TestMethod]
        public void RemoveFast_ItemNotFound_DoesNothing()
        {
            // Arrange
            var list = new List<int> { 1, 2, 3 };

            // Act
            list.RemoveFast(99);

            // Assert
            Assert.AreEqual(3, list.Count);
        }

        /// <summary>
        /// Removes at fast valid index removes and swaps.
        /// </summary>
        [TestMethod]
        public void RemoveAtFast_ValidIndex_RemovesAndSwaps()
        {
            // Arrange
            var list = new List<int> { 10, 20, 30, 40, 50 };

            // Act
            list.RemoveAtFast(1); // Removing 20

            // Assert
            Assert.AreEqual(4, list.Count);
            Assert.AreEqual(50, list[1]); // Last item (50) should now be at index 1
        }

        /// <summary>
        /// Removes at fast last index removes correctly.
        /// </summary>
        [TestMethod]
        public void RemoveAtFast_LastIndex_RemovesCorrectly()
        {
            // Arrange
            var list = new List<string> { "X", "Y", "Z" };

            // Act
            list.RemoveAtFast(2); // Removing "Z"

            // Assert
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("Y", list[1]);
            Assert.IsFalse(list.Contains("Z"));
        }

        /// <summary>
        /// Removes at fast invalid index does not throw.
        /// </summary>
        [TestMethod]
        public void RemoveAtFast_InvalidIndex_DoesNotThrow()
        {
            // Arrange
            var list = new List<int> { 1, 2 };

            // Act & Assert (Should not throw exception based on implementation)
            list.RemoveAtFast(-1);
            list.RemoveAtFast(10);

            Assert.AreEqual(2, list.Count);
        }
    }
}
