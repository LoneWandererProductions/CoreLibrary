/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.ExtendedObject.Tests
 * FILE:        BiMapTestss.cs
 * PURPOSE:     Tests for ExtendedSystemObjects
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using ExtendedSystemObjects;

namespace Common.ExtendedObject.Tests
{
    /// <summary>
    /// Some generic tests for BiMap. These tests cover basic functionality such as adding pairs, retrieving values, and ensuring that duplicate values are not allowed.
    /// </summary>
    [TestClass]
    public class BiMapTests
    {
        /// <summary>
        /// Adds the valid pair adds to both sides.
        /// </summary>
        [TestMethod]
        public void Add_ValidPair_AddsToBothSides()
        {
            var map = new BiMap<string> { { "Key", "Value" } };

            Assert.AreEqual("Value", map.GetForward("Key"));
            Assert.AreEqual("Key", map.GetReverse("Value"));
        }

        /// <summary>
        /// Adds the duplicate value throws exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_DuplicateValue_ThrowsException()
        {
            var map = new BiMap<string>
            {
                { "A", "Shared" }, { "B", "Shared" } // This should trigger your duplicate check
            };
        }

        /// <summary>
        /// Removes the removes from both sides.
        /// </summary>
        [TestMethod]
        public void Remove_RemovesFromBothSides()
        {
            var map = new BiMap<int> { { 1, 100 } };
            map.RemoveByLeft(1);

            Assert.IsFalse(map.Contains(1));
            Assert.IsFalse(map.TryGetReverse(100, out _));
        }
    }
}
