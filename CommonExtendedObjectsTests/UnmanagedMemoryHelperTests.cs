/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        UnmanagedMemoryHelperTests.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Runtime.InteropServices;
using ExtendedSystemObjects.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonExtendedObjectsTests;

[TestClass]
public unsafe class UnmanagedMemoryHelperTests
{
    [TestMethod]
    public void Allocate_Reallocate_Free_Memory()
    {
        const int count = 10;

        var ptr = UnmanagedMemoryHelper.Allocate<int>(count);
        Assert.AreNotEqual(IntPtr.Zero, ptr);

        try
        {
            // Write values to allocated memory
            var intPtr = (int*)ptr.ToPointer();
            for (var i = 0; i < count; i++)
            {
                intPtr[i] = i;
            }

            // Reallocate to bigger size
            const int newCount = 20;
            var newPtr = UnmanagedMemoryHelper.Reallocate<int>(ptr, newCount);
            Assert.AreNotEqual(IntPtr.Zero, newPtr);
            ptr = newPtr;

            intPtr = (int*)ptr.ToPointer();

            // Check old values still there
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(i, intPtr[i]);
            }

            // Write new values
            for (var i = count; i < newCount; i++)
            {
                intPtr[i] = i * 2;
            }

            // Check new values
            for (var i = count; i < newCount; i++)
            {
                Assert.AreEqual(i * 2, intPtr[i]);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    [TestMethod]
    public void Clear_SetsMemoryToZero()
    {
        const int count = 5;
        var ptr = UnmanagedMemoryHelper.Allocate<int>(count);
        try
        {
            var intPtr = (int*)ptr.ToPointer();
            for (var i = 0; i < count; i++)
            {
                intPtr[i] = 123;
            }

            UnmanagedMemoryHelper.Clear<int>(ptr, count);

            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(0, intPtr[i]);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

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
            UnmanagedMemoryHelper.ShiftRight(intPtr, 2, 2, count, capacity);

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
