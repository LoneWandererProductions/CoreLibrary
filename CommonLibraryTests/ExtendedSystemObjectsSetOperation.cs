/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/ExtendedSystemObjectsSetOperation.cs
 * PURPOSE:     Tests for ExtendedSystemObjects, Set Operations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */


using System.Collections.Generic;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
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
        ///     Symmetrics the difference retains common elements when inverted.
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
    }
}
