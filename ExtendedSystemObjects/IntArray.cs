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
        public void RemoveMultiple(int[] indicesToRemove)
        {
            if (indicesToRemove == null || indicesToRemove.Length == 0)
            {
                return;
            }

            // Sort indices ascending (important!)
            Array.Sort(indicesToRemove);

            // Validate indices within bounds
#if DEBUG
            foreach (var idx in indicesToRemove)
            {
                if (idx < 0 || idx >= Length)
                {
                    throw new IndexOutOfRangeException($"Index {idx} is out of range.");
                }
            }
#endif

            if (IsSequential(indicesToRemove))
            {
                // Optimized path for continuous block removal
                var start = indicesToRemove[0];
                var count = indicesToRemove.Length;

                // Move tail elements left by count positions
                var tailLength = Length - (start + count);
                if (tailLength > 0)
                {
                    Buffer.MemoryCopy(
                        _ptr + start + count,
                        _ptr + start,
                        tailLength * sizeof(int),
                        tailLength * sizeof(int)
                    );
                }

                Length -= count;
            }
            else
            {
                // General multi-block removal, no assumption on indices continuity

                var srcIndex = 0;
                var dstIndex = 0;

                foreach (var removeIndex in indicesToRemove)
                {
                    var lengthToCopy = removeIndex - srcIndex;

                    if (lengthToCopy > 0)
                    {
                        Buffer.MemoryCopy(
                            _ptr + srcIndex,
                            _ptr + dstIndex,
                            (Length - dstIndex) * sizeof(int),
                            lengthToCopy * sizeof(int)
                        );
                        dstIndex += lengthToCopy;
                    }

                    srcIndex = removeIndex + 1;
                }

                // Copy remaining tail block after last removed index
                var tailLength = Length - srcIndex;
                if (tailLength > 0)
                {
                    Buffer.MemoryCopy(
                        _ptr + srcIndex,
                        _ptr + dstIndex,
                        (Length - dstIndex) * sizeof(int),
                        tailLength * sizeof(int)
                    );
                    dstIndex += tailLength;
                }

                Length = dstIndex;
            }
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
        ///     Determines whether the specified sorted indices is sequential.
        /// </summary>
        /// <param name="sortedIndices">The sorted indices.</param>
        /// <returns>
        ///     <c>true</c> if the specified sorted indices is sequential; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsSequential(IReadOnlyList<int> sortedIndices)
        {
            if (sortedIndices is not { Count: > 1 })
            {
                return true;
            }

            for (var i = 1; i < sortedIndices.Count; i++)
            {
                if (sortedIndices[i] != sortedIndices[i - 1] + 1)
                {
                    return false;
                }
            }

            return true;
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
