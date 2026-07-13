/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.ExtendedObject.Tests
 * FILE:        UnmanagedMemoryHelperTests.cs
 * PURPOSE:     Test for unmanaged memory helper functions.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Runtime.InteropServices;
using ExtendedSystemObjects.Helper;

namespace Common.ExtendedObject.Tests
{
    /// <summary>
    /// Basic unit tests for UnmanagedMemoryHelper functions to ensure correctness of memory allocation, reallocation, clearing, shifting, filling, indexing, and swapping operations on unmanaged memory blocks.
    /// </summary>
    [TestClass]
    public unsafe class UnmanagedMemoryHelperTests
    {
        /// <summary>
        /// Allocates the reallocate free memory.
        /// </summary>
        [TestMethod]
        public void Allocate_Reallocate_Free_Memory()
        {
            const int count = 10;

            // 1. Allocate directly to a strongly-typed int*
            var ptr = (int*)UnmanagedMemoryHelper.Allocate<int>(count);
            Assert.IsTrue(ptr != null, "Allocation failed; pointer is null.");

            try
            {
                // Write values directly without .ToPointer() boilerplate
                for (var i = 0; i < count; i++)
                {
                    ptr[i] = i;
                }

                // Reallocate to a bigger size
                const int newCount = 20;
                var newPtr = UnmanagedMemoryHelper.Reallocate<int>(ptr, newCount);
                Assert.IsTrue(newPtr != null, "Reallocation failed; pointer is null.");
                ptr = newPtr;

                // Check old values are preserved
                for (var i = 0; i < count; i++)
                {
                    Assert.AreEqual(i, ptr[i]);
                }

                // Write new values into the expanded region
                for (var i = count; i < newCount; i++)
                {
                    ptr[i] = i * 2;
                }

                // Verify new values
                for (var i = count; i < newCount; i++)
                {
                    Assert.AreEqual(i * 2, ptr[i]);
                }
            }
            finally
            {
                // Fixed: Pair custom allocator with its matching Free utility
                UnmanagedMemoryHelper.Free(ptr);
            }
        }

        /// <summary>
        /// Clears the sets memory to zero.
        /// </summary>
        [TestMethod]
        public void Clear_SetsMemoryToZero()
        {
            const int count = 5;
            var ptr = (int*)UnmanagedMemoryHelper.Allocate<int>(count);

            try
            {
                // Pollute the buffer
                for (var i = 0; i < count; i++)
                {
                    ptr[i] = 123;
                }

                // Clear using pure pointer arithmetic
                UnmanagedMemoryHelper.Clear<int>(ptr, count);

                // Assert zero-init block
                for (var i = 0; i < count; i++)
                {
                    Assert.AreEqual(0, ptr[i]);
                }
            }
            finally
            {
                // Fixed: Avoided breaking internal tracking state
                UnmanagedMemoryHelper.Free(ptr);
            }
        }

        /// <summary>
        /// Shifts the right moves elements correctly.
        /// </summary>
        [TestMethod]
        public void ShiftRight_MovesElementsCorrectly()
        {
            const int count = 5;
            const int capacity = 10;
            var ptr = UnmanagedMemoryHelper.Allocate<int>(capacity);
            try
            {
                var intPtr = (int*)ptr.ToPointer();
                for (var i = 0; i < count; i++)
                {
                    intPtr[i] = i + 1; // 1,2,3,4,5
                }

                // Shift right at index 2 by 2 positions
                UnmanagedMemoryHelper.ShiftRight(intPtr, 2, 2, count);

                // Now elements 3,4,5 moved right by 2: positions 4,5,6
                // Index 2 and 3 now free (undefined content)
                Assert.AreEqual(1, intPtr[0]);
                Assert.AreEqual(2, intPtr[1]);
                // We don't expect meaningful values at 2 and 3 since shifted right
                Assert.AreEqual(3, intPtr[4]);
                Assert.AreEqual(4, intPtr[5]);
                Assert.AreEqual(5, intPtr[6]);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// Shifts the left moves elements correctly.
        /// </summary>
        [TestMethod]
        public void ShiftLeft_MovesElementsCorrectly()
        {
            const int count = 5;
            var ptr = UnmanagedMemoryHelper.Allocate<int>(count);
            try
            {
                var intPtr = (int*)ptr.ToPointer();
                for (var i = 0; i < count; i++)
                {
                    intPtr[i] = i + 1; // 1,2,3,4,5
                }

                // Shift left at index 1 by 2 positions (delete 2 elements at index 1 and 2)
                UnmanagedMemoryHelper.ShiftLeft(intPtr, 1, 2, count);

                // After shift left, elements at index 3,4 move to index 1,2
                Assert.AreEqual(1, intPtr[0]);
                Assert.AreEqual(4, intPtr[1]);
                Assert.AreEqual(5, intPtr[2]);
                // Index 3,4 undefined now, not tested
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// Fills the memory with the specified value.
        /// </summary>
        [TestMethod]
        public void Fill_FillsMemoryWithValue()
        {
            const int count = 4;
            var ptr = UnmanagedMemoryHelper.Allocate<int>(count);
            try
            {
                var intPtr = (int*)ptr.ToPointer();
                UnmanagedMemoryHelper.Fill(intPtr, 42, count);

                for (var i = 0; i < count; i++)
                {
                    Assert.AreEqual(42, intPtr[i]);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// Indexes the of finds value correctly.
        /// </summary>
        [TestMethod]
        public void IndexOf_FindsValueCorrectly()
        {
            const int count = 5;
            var ptr = UnmanagedMemoryHelper.Allocate<int>(count);
            try
            {
                var intPtr = (int*)ptr.ToPointer();
                intPtr[0] = 10;
                intPtr[1] = 20;
                intPtr[2] = 30;
                intPtr[3] = 40;
                intPtr[4] = 50;

                var idx = UnmanagedMemoryHelper.IndexOf(intPtr, 30, count);
                Assert.AreEqual(2, idx);

                var notFound = UnmanagedMemoryHelper.IndexOf(intPtr, 99, count);
                Assert.AreEqual(-1, notFound);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// Swaps the swaps elements correctly.
        /// </summary>
        [TestMethod]
        public void Swap_SwapsElementsCorrectly()
        {
            const int count = 3;
            var ptr = UnmanagedMemoryHelper.Allocate<int>(count);
            try
            {
                var intPtr = (int*)ptr.ToPointer();
                intPtr[0] = 1;
                intPtr[1] = 2;
                intPtr[2] = 3;

                UnmanagedMemoryHelper.Swap(intPtr, 0, 2);

                Assert.AreEqual(3, intPtr[0]);
                Assert.AreEqual(2, intPtr[1]);
                Assert.AreEqual(1, intPtr[2]);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
