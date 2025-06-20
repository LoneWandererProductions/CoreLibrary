/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/IntArray.cs
 * PURPOSE:     A high-performance array implementation with reduced features. Limited to integer Values.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ExtendedSystemObjects.Helper;
using ExtendedSystemObjects.Interfaces;

namespace ExtendedSystemObjects
{
    /// <inheritdoc cref="IDisposable" />
    /// <summary>
    ///     Represents a high-performance, low-overhead array of integers
    ///     backed by unmanaged memory. Designed for performance-critical
    ///     scenarios where garbage collection overhead must be avoided.
    /// </summary>
    public sealed unsafe class IntArray : IUnmanagedArray<int>, IEnumerable<int>
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
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            Capacity = size;
            Length = size;

            _buffer = Marshal.AllocHGlobal(size * sizeof(int));
            _ptr = (int*)_buffer;

            Clear(); // Optional: zero out memory on allocation
        }

        /// <summary>
        ///     Gets the current allocated capacity.
        /// </summary>
        public int Capacity { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the current number of elements in the array.
        /// </summary>
        public int Length { get; private set; }

        /// <inheritdoc />
        /// <summary>
        ///     Gets or sets the <see cref="!:T" /> at the specified index.
        /// </summary>
        /// <value>
        ///     The <see cref="!:T" />.
        /// </value>
        /// <param name="i">The i.</param>
        /// <returns>Value at index.</returns>
        /// <exception cref="T:System.IndexOutOfRangeException"></exception>
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
        ///     Resizes the internal buffer to the new capacity.
        ///     If newSize is smaller than current Length, Length is reduced.
        /// </summary>
        public void Resize(int newSize)
        {
            if (newSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newSize));
            }

            if (newSize == Capacity)
            {
                return;
            }

            var newBuffer = Marshal.ReAllocHGlobal(_buffer, (IntPtr)(newSize * sizeof(int)));
            var newPtr = (int*)newBuffer;

            // If growing, zero out the newly allocated portion
            if (newSize > Capacity)
            {
                var newRegion = new Span<int>(newPtr + Capacity, newSize - Capacity);
                newRegion.Clear();
            }

            _buffer = newBuffer;
            _ptr = newPtr;
            Capacity = newSize;

            if (Length > newSize)
            {
                Length = newSize;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<int> GetEnumerator()
        {
            return new Enumerator<int>(_ptr, Length);
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Clears all elements to zero.
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < Length; i++)
            {
                _ptr[i] = 0;
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Frees unmanaged memory.
        /// </summary>
        public void Dispose()
        {
            if (_buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_buffer);
                _buffer = IntPtr.Zero;
                _ptr = null;
                Length = 0;
                Capacity = 0;
            }

            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Removes the element at the specified index by shifting remaining elements left.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
#if DEBUG
            if (index < 0 || index >= Length)
            {
                throw new IndexOutOfRangeException();
            }
#endif
            for (var i = index; i < Length - 1; i++)
            {
                _ptr[i] = _ptr[i + 1];
            }

            Length--;
        }

        /// <summary>
        ///     Inserts 'count' copies of 'value' at the given index.
        /// </summary>
        public void InsertAt(int index, int value, int count = 1)
        {
            if (index < 0 || index > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            EnsureCapacity(Length + count);

            var shiftCount = Length - index;
            if (shiftCount > 0)
            {
                Buffer.MemoryCopy(
                    _ptr + index,
                    _ptr + index + count,
                    (Capacity - index) * sizeof(int), // size of dest from index onward
                    shiftCount * sizeof(int));
            }

            for (var i = 0; i < count; i++)
            {
                _ptr[index + i] = value;
            }

            Length += count;
        }

        /// <summary>
        ///     Removes multiple elements efficiently, given sorted indices.
        /// </summary>
        public void RemoveMultiple(ReadOnlySpan<int> indices)
        {
            if (indices.Length == 0)
            {
                return;
            }

            // Fast path for consecutive indices
            if (indices.Length > 1 && indices[^1] - indices[0] == indices.Length - 1)
            {
                var start = indices[0];
                var count = indices.Length;

#if DEBUG
                if (start < 0 || start + count > Length)
                {
                    throw new IndexOutOfRangeException();
                }
#endif

                var moveCount = Length - (start + count);
                if (moveCount > 0)
                {
                    Buffer.MemoryCopy(
                        _ptr + start + count,
                        _ptr + start,
                        (Capacity - start) * sizeof(int),
                        moveCount * sizeof(int));
                }

                Length -= count;
                return;
            }

            // General path: compact by skipping removed indices
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
        ///     Returns a Span over the used portion of the array.
        /// </summary>
        public Span<int> AsSpan()
        {
            return new(_ptr, Length);
        }

        /// <summary>
        ///     Ensures capacity to hold at least minCapacity elements.
        ///     Grows capacity exponentially if needed.
        /// </summary>
        public void EnsureCapacity(int minCapacity)
        {
            if (minCapacity <= Capacity)
            {
                return;
            }

            var newCapacity = Capacity == 0 ? 4 : Capacity;
            while (newCapacity < minCapacity)
            {
                newCapacity *= 2;
            }

            Resize(newCapacity);
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
