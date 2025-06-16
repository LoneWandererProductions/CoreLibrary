/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/IntArray.cs
 * PURPOSE:     A high-performance array implementation with reduced features. Limited to integer Values.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBeInternal

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExtendedSystemObjects
{
    /// <inheritdoc />
    /// <summary>
    ///     Represents a high-performance, low-overhead array of integers
    ///     backed by unmanaged memory. Designed for performance-critical
    ///     scenarios where garbage collection overhead must be avoided.
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    public sealed unsafe class IntArray : IDisposable
    {
        /// <summary>
        ///     The buffer
        /// </summary>
        private IntPtr _buffer;

        /// <summary>
        ///     The pointer
        /// </summary>
        private int* _ptr;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IntArray" /> class with the specified size.
        /// </summary>
        /// <param name="size">The number of elements to allocate.</param>
        public IntArray(int size)
        {
            Length = size;
            _buffer = Marshal.AllocHGlobal(size * sizeof(int));
            _ptr = (int*)_buffer;
        }

        /// <summary>
        ///     Gets the current number of elements in the array.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        ///     Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="i">The index of the element.</param>
        /// <returns>The value at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown in debug mode when the index is out of bounds.</exception>
        public int this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG
                if (i < 0 || i >= Length)
                {
                    throw new IndexOutOfRangeException();
                }
#endif
                return _ptr[i];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
#if DEBUG
                if (i < 0 || i >= Length)
                {
                    throw new IndexOutOfRangeException();
                }
#endif
                _ptr[i] = value;
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Frees the unmanaged memory held by the array.
        ///     After disposal, the instance should not be used.
        /// </summary>
        public void Dispose()
        {
            Marshal.FreeHGlobal(_buffer);
            _buffer = IntPtr.Zero;
            Length = 0;
        }

        /// <summary>
        ///     Removes the element at the specified index by shifting remaining elements left.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
#if DEBUG
            if (index < 0 || index >= Length)
            {
                throw new IndexOutOfRangeException();
            }
#endif
            var ptr = (int*)_buffer;

            for (var i = index; i < Length - 1; i++)
            {
                ptr[i] = ptr[i + 1];
            }

            Length--; // Reduces logical size, but memory is not freed
        }


        /// <summary>
        ///     Remove multiple indices from IntArray efficiently
        ///     Assumes indicesToRemove contains zero or more indices (in any order)
        ///     Removes all specified indices from the array
        /// </summary>
        /// <param name="indicesToRemove">The indices to remove.</param>
        /// <summary>
        /// Removes multiple elements efficiently. If the indices are sequential and sorted,
        /// uses a fast bulk-removal path. Otherwise falls back to element-wise compaction.
        /// </summary>
        /// <param name="indices">Indices to remove. Must be sorted and within bounds.</param>
        public void RemoveMultiple(ReadOnlySpan<int> indices)
        {
            if (indices.Length == 0) return;

            // === Fast-path: Trivial sequential sequence ===
            // If [3,4,5,6] then indices[^1] - indices[0] == indices.Length - 1
            if (indices.Length > 1 && indices[^1] - indices[0] == indices.Length - 1)
            {
                int start = indices[0];
                int count = indices.Length;

#if DEBUG
                if (start < 0 || start + count > Length)
                    throw new IndexOutOfRangeException();
#endif

                int moveCount = Length - (start + count);
                if (moveCount > 0)
                    Buffer.MemoryCopy(_ptr + start + count, _ptr + start, moveCount * sizeof(int), moveCount * sizeof(int));

                Length -= count;
                return;
            }

            // === Fallback: Compact array by skipping indices ===
            int readIndex = 0, writeIndex = 0, removeIndex = 0;

            while (readIndex < Length)
            {
                if (removeIndex < indices.Length && readIndex == indices[removeIndex])
                {
                    readIndex++;
                    removeIndex++;
                }
                else
                {
                    _ptr[writeIndex++] = _ptr[readIndex++];
                }
            }

            Length = writeIndex;
        }


        /// <summary>
        ///     Resizes the internal array to the specified new size.
        ///     Contents will be preserved up to the minimum of old and new size.
        /// </summary>
        /// <param name="newSize">The new size of the array.</param>
        public void Resize(int newSize)
        {
            _buffer = Marshal.ReAllocHGlobal(_buffer, (IntPtr)(newSize * sizeof(int)));
            _ptr = (int*)_buffer;
            Length = newSize;
        }

        /// <summary>
        ///     Clears the array by setting all elements to zero.
        /// </summary>
        public void Clear()
        {
            var ptr = (int*)_buffer;
            for (var i = 0; i < Length; i++)
            {
                ptr[i] = 0;
            }
        }

        /// <summary>
        ///     Returns a span over the current memory buffer, enabling safe, fast access.
        /// </summary>
        /// <returns>A <see cref="Span{Int32}" /> over the internal buffer.</returns>
        public Span<int> AsSpan()
        {
            return new((void*)_buffer, Length);
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="IntArray" /> class.
        /// </summary>
        ~IntArray()
        {
            Dispose();
        }
    }
}
