/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects.Helper
 * FILE:        ExtendedSystemObjects.Helper/UnmanagedMemoryHelper.cs
 * PURPOSE:     Provides helper methods for low-level unmanaged memory operations.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExtendedSystemObjects.Helper
{
    /// <summary>
    /// Provides helper methods for allocating, reallocating, and clearing unmanaged memory blocks.
    /// Designed for use with value types (unmanaged types) only.
    /// </summary>
    internal static class UnmanagedMemoryHelper
    {
        /// <summary>
        /// Allocates a block of unmanaged memory large enough to hold the specified number of elements of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The unmanaged value type to allocate memory for.</typeparam>
        /// <param name="count">The number of elements to allocate.</param>
        /// <returns>A pointer to the allocated unmanaged memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IntPtr Allocate<T>(int count) where T : unmanaged
        {
            unsafe
            {
                return Marshal.AllocHGlobal(sizeof(T) * count);
            }
        }

        /// <summary>
        /// Reallocates an existing block of unmanaged memory to hold a new number of elements of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The unmanaged value type used in the memory block.</typeparam>
        /// <param name="ptr">A pointer to the existing unmanaged memory block.</param>
        /// <param name="newCount">The new number of elements to accommodate.</param>
        /// <returns>A pointer to the newly reallocated unmanaged memory block.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static IntPtr Reallocate<T>(IntPtr ptr, int newCount) where T : unmanaged
        {
            unsafe
            {
                return Marshal.ReAllocHGlobal(ptr, (IntPtr)(sizeof(T) * newCount));
            }
        }

        /// <summary>
        /// Clears a block of unmanaged memory by setting its contents to zero.
        /// </summary>
        /// <typeparam name="T">The unmanaged value type used in the memory block.</typeparam>
        /// <param name="buffer">A pointer to the unmanaged memory block.</param>
        /// <param name="count">The number of elements to clear.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Clear<T>(IntPtr buffer, int count) where T : unmanaged
        {
            unsafe
            {
                Span<T> span = new(buffer.ToPointer(), count);
                span.Clear(); // Equivalent to memset 0
            }
        }

        /// <summary>
        /// Shifts the right. Adding data at index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ptr">The PTR.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <param name="length">The length.</param>
        /// <param name="capacity">The capacity.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ShiftRight<T>(T* ptr, int index, int count, int length, int capacity) where T : unmanaged
        {
            int elementsToShift = length - index;
            if (elementsToShift <= 0) return;

            Buffer.MemoryCopy(
                source: ptr + index,
                destination: ptr + index + count,
                destinationSizeInBytes: (capacity - index - count) * sizeof(T),
                sourceBytesToCopy: elementsToShift * sizeof(T));
        }

        /// <summary>
        /// Shifts the left. Delete Element at index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ptr">The PTR.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <param name="length">The length.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ShiftLeft<T>(T* ptr, int index, int count, int length) where T : unmanaged
        {
            int elementsToShift = length - (index + count);
            if (elementsToShift <= 0) return;

            Buffer.MemoryCopy(
                source: ptr + index + count,
                destination: ptr + index,
                destinationSizeInBytes: elementsToShift * sizeof(T),
                sourceBytesToCopy: elementsToShift * sizeof(T));
        }
    }
}
