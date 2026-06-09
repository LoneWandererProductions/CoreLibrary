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

namespace ExtendedSystemObjects.Helper
{
    /// <summary>
    ///     Provides helper methods for allocating, reallocating, and clearing unmanaged memory blocks.
    ///     Designed for use with value types (unmanaged types) only.
    /// </summary>
    internal static unsafe class UnmanagedMemoryHelper
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
            return (nint)(T*)NativeMemory.Alloc((nuint)count, (nuint)sizeof(T));
        }

        /// <summary>
        /// Allocates a block of zero-initialized unmanaged memory.
        /// Equivalent to calloc.
        /// </summary>
        /// <typeparam name="T">The unmanaged value type to allocate memory for.</typeparam>
        /// <param name="count">The number of elements to allocate.</param>
        /// <returns>A pointer to the allocated zero-initialized unmanaged memory.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T* AllocateZeroed<T>(int count) where T : unmanaged
        {
            return (T*)NativeMemory.AllocZeroed((nuint)count, (nuint)sizeof(T));
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
        internal static T* Reallocate<T>(T* ptr, int newCount) where T : unmanaged
        {
            // Maps directly to C-style realloc.
            return (T*)NativeMemory.Realloc(ptr, (nuint)(sizeof(T) * newCount));
        }

        /// <summary>
        /// Frees a block of unmanaged memory.
        /// </summary>
        /// <param name="ptr">The pointer to the unmanaged memory block.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Free(void* ptr)
        {
            // Standardized free operation.
            NativeMemory.Free(ptr);
        }

        /// <summary>
        ///     Clears a block of unmanaged memory by setting its contents to zero.
        /// </summary>
        /// <typeparam name="T">The unmanaged value type used in the memory block.</typeparam>
        /// <param name="buffer">A pointer to the unmanaged memory block.</param>
        /// <param name="count">The number of elements to clear.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Clear<T>(T* buffer, int count) where T : unmanaged
        {
            NativeMemory.Clear(buffer, (nuint)count * (nuint)sizeof(T));
        }

        /// <summary>
        /// Shifts the right. Adding data at index.
        /// </summary>
        /// <typeparam name="T">Generic Parameter</typeparam>
        /// <param name="ptr">The PTR.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <param name="length">The length.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ShiftRight<T>(T* ptr, int index, int count, int length) where T : unmanaged
        {
            int elementsToShift = length - index;
            if (elementsToShift <= 0 || count <= 0)
            {
                return;
            }

            // Buffer.MemoryCopy acts like memmove, inherently handling overlapping memory regions safely.
            Buffer.MemoryCopy(
                ptr + index,
                ptr + index + count,
                elementsToShift * (long)sizeof(T),
                elementsToShift * (long)sizeof(T));
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
        internal static void ShiftLeft<T>(T* ptr, int index, int count, int length) where T : unmanaged
        {
            int elementsToShift = length - (index + count);
            if (elementsToShift <= 0)
            {
                return;
            }

            Buffer.MemoryCopy(
                ptr + index + count,
                ptr + index,
                (length - index) * sizeof(T),
                elementsToShift * sizeof(T));
        }

        /// <summary>
        ///     Copies a block of unmanaged memory from source to destination.
        ///     Similar to memcpy.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Copy<T>(T* source, T* destination, int count) where T : unmanaged
        {
            nuint byteCount = (nuint)(count * sizeof(T));
            Buffer.MemoryCopy(source, destination, byteCount, byteCount);
        }

        /// <summary>
        ///     Allocates and clones a block of unmanaged memory from a given source.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T* Clone<T>(T* source, int count) where T : unmanaged
        {
            nuint byteCount = (nuint)(sizeof(T) * count);
            T* dest = (T*)NativeMemory.Alloc(byteCount);
            Buffer.MemoryCopy(source, dest, byteCount, byteCount);
            return dest;
        }

        /// <summary>
        ///     Fills a block of unmanaged memory with a given value.
        ///     Equivalent to memset with a pattern.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Fill<T>(T* ptr, T value, int count) where T : unmanaged
        {
            new Span<T>(ptr, count).Fill(value);
        }

        /// <summary>
        ///     Searches for the first occurrence of a value in unmanaged memory.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int IndexOf<T>(T* ptr, T value, int length) where T : unmanaged, IEquatable<T>
        {
            return new ReadOnlySpan<T>(ptr, length).IndexOf(value);
        }

        /// <summary>
        ///     Swaps two elements in unmanaged memory.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Swap<T>(T* ptr, int indexA, int indexB) where T : unmanaged
        {
            if (indexA == indexB) return;

            (ptr[indexA], ptr[indexB]) = (ptr[indexB], ptr[indexA]);
        }
    }
}
