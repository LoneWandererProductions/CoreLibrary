/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        UnmanagedListTests.cs
 * PURPOSE:     Tests for our UnmanagedList class, ensuring correct behavior of all operations including edge cases.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtendedSystemObjects;

namespace CommonExtendedObjectsTests
{
    /// <summary>
    /// General tests for the UnmanagedList class, covering construction, basic list operations (add, remove, insert),
    /// stack operations (push, pop, peek), and edge cases such as out-of-range access and disposal behavior.
    /// </summary>
    [TestClass]
    public class UnmanagedListTests
    {
        /// <summary>
        /// A simple unmanaged struct to test generic capabilities
        /// </summary>
        private struct PointD
        {
            public double X;
            public double Y;
        }

        /// <summary>
        /// Constructors the initializes correctly.
        /// </summary>
        [TestMethod]
        public void Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            using var list = new UnmanagedList<int>(10);

            // Assert
            Assert.AreEqual(0, list.Length);
            Assert.AreEqual(10, list.Capacity);
        }

        /// <summary>
        /// Adds the increases length and expands capacity.
        /// </summary>
        [TestMethod]
        public void Add_IncreasesLengthAndExpandsCapacity()
        {
            // Arrange
            using var list = new UnmanagedList<int>(2);

            // Act
            list.Add(10);
            list.Add(20);
            list.Add(30); // Should trigger EnsureCapacity

            // Assert
            Assert.AreEqual(3, list.Length);
            Assert.IsTrue(list.Capacity >= 3);
            Assert.AreEqual(10, list[0]);
            Assert.AreEqual(20, list[1]);
            Assert.AreEqual(30, list[2]);
        }

        /// <summary>
        /// Removes at shifts elements correctly.
        /// </summary>
        [TestMethod]
        public void RemoveAt_ShiftsElementsCorrectly()
        {
            // Arrange
            using var list = new UnmanagedList<int>(5);
            list.Add(1);
            list.Add(2);
            list.Add(3);
            list.Add(4);

            // Act
            list.RemoveAt(1, 2); // Remove '2' and '3'

            // Assert
            Assert.AreEqual(2, list.Length);
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(4, list[1]);
        }

        /// <summary>
        /// Inserts at shifts elements and fills correctly.
        /// </summary>
        [TestMethod]
        public void InsertAt_ShiftsElementsAndFillsCorrectly()
        {
            // Arrange
            using var list = new UnmanagedList<int>(5);
            list.Add(10);
            list.Add(40);

            // Act
            list.InsertAt(1, 20, 2); // Insert two '20's at index 1

            // Assert
            Assert.AreEqual(4, list.Length);
            Assert.AreEqual(10, list[0]);
            Assert.AreEqual(20, list[1]);
            Assert.AreEqual(20, list[2]);
            Assert.AreEqual(40, list[3]);
        }

        /// <summary>
        /// Stacks the operations push pop peek work as expected.
        /// </summary>
        [TestMethod]
        public void StackOperations_PushPopPeek_WorkAsExpected()
        {
            // Arrange
            using var list = new UnmanagedList<int>(5);

            // Act & Assert
            list.Push(100);
            list.Push(200);

            Assert.AreEqual(200, list.Peek());
            Assert.AreEqual(2, list.Length);

            var popped = list.Pop();
            Assert.AreEqual(200, popped);
            Assert.AreEqual(1, list.Length);
            Assert.AreEqual(100, list.Peek());
        }

        /// <summary>
        /// Pops the on empty list throws invalid operation exception.
        /// </summary>
        [TestMethod]
        public void Pop_OnEmptyList_ThrowsInvalidOperationException()
        {
            using var list = new UnmanagedList<int>();
            Assert.ThrowsException<InvalidOperationException>(() => list.Pop());
        }

        /// <summary>
        /// Structures the support works with custom unmanaged types.
        /// </summary>
        [TestMethod]
        public void StructSupport_WorksWithCustomUnmanagedTypes()
        {
            // Arrange
            using var list = new UnmanagedList<PointD>(2);
            var p1 = new PointD { X = 1.5, Y = 2.5 };
            var p2 = new PointD { X = 3.0, Y = 4.0 };

            // Act
            list.Add(p1);
            list.Add(p2);

            // Assert
            Assert.AreEqual(2, list.Length);
            Assert.AreEqual(1.5, list[0].X);
            Assert.AreEqual(4.0, list[1].Y);
        }

        /// <summary>
        /// Tries the get returns true for valid index false for invalid.
        /// </summary>
        [TestMethod]
        public void TryGet_ReturnsTrueForValidIndex_FalseForInvalid()
        {
            using var list = new UnmanagedList<int>(5);
            list.Add(42);

            Assert.IsTrue(list.TryGet(0, out var val));
            Assert.AreEqual(42, val);

            Assert.IsFalse(list.TryGet(1, out _));
            Assert.IsFalse(list.TryGet(-1, out _));
        }

        /// <summary>
        /// Ranges the indexer returns correct span.
        /// </summary>
        [TestMethod]
        public void RangeIndexer_ReturnsCorrectSpan()
        {
            // Arrange
            using var list = new UnmanagedList<int>(5);
            for (var i = 0; i < 5; i++) list.Add(i * 10); // 0, 10, 20, 30, 40

            // Act
            var span = list[1..4]; // Should be 10, 20, 30

            // Assert
            Assert.AreEqual(3, span.Length);
            Assert.AreEqual(10, span[0]);
            Assert.AreEqual(30, span[2]);
        }

        /// <summary>
        /// Disposes the prevents further access.
        /// </summary>
        [TestMethod]
        public void Dispose_PreventsFurtherAccess()
        {
            // Arrange
            var list = new UnmanagedList<int>(5);
            list.Add(1);

            // Act
            list.Dispose();

            // Assert
            Assert.ThrowsException<ObjectDisposedException>(() => list.Add(2));
            Assert.ThrowsException<ObjectDisposedException>(() =>
            {
                var x = list[0];
            });
            Assert.ThrowsException<ObjectDisposedException>(() => list.Clear());
        }

        /// <summary>
        /// Clears the resets length but keeps capacity.
        /// </summary>
        [TestMethod]
        public void Clear_ResetsLengthButKeepsCapacity()
        {
            // Arrange
            using var list = new UnmanagedList<int>(10);
            list.Add(1);
            list.Add(2);
            var initialCapacity = list.Capacity;

            // Act
            list.Clear();

            // Assert
            Assert.AreEqual(0, list.Length);
            Assert.AreEqual(initialCapacity, list.Capacity);
        }
    }
}
