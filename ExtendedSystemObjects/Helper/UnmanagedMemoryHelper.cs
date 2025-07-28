/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects.Helper
 * FILE:        ExtendedSystemObjects.Helper/UnmanagedMemoryHelper.cs
 * PURPOSE:     Provides helper methods for low-level unmanaged memory operations.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExtendedSystemObjects.Helper;

/// <summary>
///     Provides helper methods for allocating, reallocating, and clearing unmanaged memory blocks.
///     Designed for use with value types (unmanaged types) only.
/// </summary>
internal static class UnmanagedMemoryHelper
{
    /// <summary>
    ///     Allocates a block of unmanaged memory large enough to hold the specified number of elements of type
    ///     <typeparamref name="T" />.
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
    ///     Reallocates an existing block of unmanaged memory to hold a new number of elements of type
    ///     <typeparamref name="T" />.
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
    ///     Clears a block of unmanaged memory by setting its contents to zero.
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
    ///     Shifts the right. Adding data at index.
    /// </summary>
    /// <typeparam name="T">Generic Parameter</typeparam>
    /// <param name="ptr">The PTR.</param>
    /// <param name="index">The index.</param>
    /// <param name="count">The count.</param>
    /// <param name="length">The length.</param>
    /// <param name="capacity">The capacity.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe void ShiftRight<T>(T* ptr, int index, int count, int length, int capacity)
        where T : unmanaged
    {
        var elementsToShift = length - index;
        if (elementsToShift <= 0 || count <= 0)
        {
            return;
        }

        // Start copying from the end to avoid overwriting
        Buffer.MemoryCopy(
            ptr + index,
            ptr + index + count,
            (capacity - index - count) * sizeof(T), // should be fine if debug check passes
            elementsToShift * sizeof(T));
    }

    /// <summary>
    ///     Shifts the left. Delete Element at index
    /// </summary>
    /// <typeparam name="T">Generic Parameter</typeparam>
    /// <param name="ptr">The PTR.</param>
    /// <param name="index">The index.</param>
    /// <param name="count">The count.</param>
    /// <param name="length">The length.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe void ShiftLeft<T>(T* ptr, int index, int count, int length) where T : unmanaged
    {
        var elementsToShift = length - (index + count);
        if (elementsToShift <= 0)
        {
            return;
        }

        var dstSize = (length - index) * sizeof(T); // full space after index

        Buffer.MemoryCopy(
            ptr + index + count,
            ptr + index,
            dstSize,
            elementsToShift * sizeof(T));
    }

    /// <summary>
    ///     Copies a block of unmanaged memory from source to destination.
    ///     Similar to memcpy.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe void Copy<T>(T* source, T* destination, int count) where T : unmanaged
    {
        Buffer.MemoryCopy(source, destination, count * sizeof(T), count * sizeof(T));
    }

    /// <summary>
    ///     Allocates and clones a block of unmanaged memory from a given source.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe IntPtr Clone<T>(T* source, int count) where T : unmanaged
    {
        var size = sizeof(T) * count;
        var dest = Marshal.AllocHGlobal(size);
        Buffer.MemoryCopy(source, dest.ToPointer(), size, size);
        return dest;
    }

    /// <summary>
    ///     Fills a block of unmanaged memory with a given value.
    ///     Equivalent to memset with a pattern.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe void Fill<T>(T* ptr, T value, int count) where T : unmanaged
    {
        for (var i = 0; i < count; i++)
        {
            ptr[i] = value;
        }
    }

    /// <summary>
    ///     Searches for the first occurrence of a value in unmanaged memory.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe int IndexOf<T>(T* ptr, T value, int length) where T : unmanaged, IEquatable<T>
    {
        for (var i = 0; i < length; i++)
        {
            if (ptr[i].Equals(value))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    ///     Swaps two elements in unmanaged memory.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static unsafe void Swap<T>(T* ptr, int indexA, int indexB) where T : unmanaged
    {
        if (indexA == indexB)
        {
            return;
        }

        (ptr[indexA], ptr[indexB]) = (ptr[indexB], ptr[indexA]);
    }
}
