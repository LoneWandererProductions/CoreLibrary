/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/UnmanagedIntArray.cs
 * PURPOSE:     A high-performance array implementation with reduced features. Limited to integer Values.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
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
    public sealed unsafe class UnmanagedIntArray : IUnmanagedArray<int>, IEnumerable<int>
    {
        /// <summary>
        ///     The buffer
        /// </summary>
        private IntPtr _buffer;

        /// <summary>
        ///     Check if we disposed the object
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     The pointer
        /// </summary>
        private int* _ptr;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnmanagedIntArray" /> class with the specified size.
        /// </summary>
        /// <param name="size">The number of elements to allocate.</param>
        public UnmanagedIntArray(int size)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(size);

            Capacity = size;
            Length = size;

            _buffer = UnmanagedMemoryHelper.Allocate<int>(size);
            _ptr = (int*)_buffer;

            UnmanagedMemoryHelper.Clear<int>(_buffer, size); // Zero out memory on allocation
        }

        /// <summary>
        ///     Gets a value indicating whether [use simd].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [use simd]; otherwise, <c>false</c>.
        /// </value>
        private static bool UseSimd => Vector.IsHardwareAccelerated;

        /// <summary>
        ///     Gets the current allocated capacity.
        /// </summary>
        public int Capacity { get; set; }


        /// <inheritdoc />
        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<int> GetEnumerator()
        {
            return new Enumerator<int>(_ptr, Length);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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
            ArgumentOutOfRangeException.ThrowIfNegative(newSize);

            if (newSize == Capacity)
            {
                return;
            }

            _buffer = UnmanagedMemoryHelper.Reallocate<int>(_buffer, newSize);
            _ptr = (int*)_buffer;

            // If growing, clear the newly allocated portion
            if (newSize > Capacity)
            {
                UnmanagedMemoryHelper.Clear<int>(_buffer + (Capacity * sizeof(int)), newSize - Capacity);
            }

            Capacity = newSize;

            if (Length > newSize)
            {
                Length = newSize;
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Clears the array by setting all elements to zero.
        /// </summary>
        public void Clear()
        {
            UnmanagedMemoryHelper.Clear<int>(_buffer, Length);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Removes the element at the specified index by shifting remaining elements left.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count we want to remove. Optional.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index, int count = 1)
        {
#if DEBUG
            if (index < 0 || index >= Length)
            {
                throw new IndexOutOfRangeException();
            }
#endif
            // Shift elements left by 'count' starting at 'index'
            for (var i = index; i < Length - count; i++)
            {
                _ptr[i] = _ptr[i + count];
            }

            Length -= count;
        }


        /// <inheritdoc />
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Indexes the of.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Value at the index.</returns>
        public int IndexOf(int value)
        {
            var span = AsSpan();
            var vectorSize = Vector<int>.Count;

            if (UseSimd && span.Length >= vectorSize)
            {
                var vTarget = new Vector<int>(value);
                var i = 0;

                for (; i <= span.Length - vectorSize; i += vectorSize)
                {
                    var vData = new Vector<int>(span.Slice(i, vectorSize));
                    var vCmp = Vector.Equals(vData, vTarget);

                    if (Vector.EqualsAll(vCmp, Vector<int>.Zero))
                    {
                        continue;
                    }

                    for (var j = 0; j < vectorSize; j++)
                    {
                        if (vCmp[j] != 0)
                        {
                            return i + j;
                        }
                    }
                }

                // Scalar fallback
                for (; i < span.Length; i++)
                {
                    if (span[i] == value)
                    {
                        return i;
                    }
                }

                return -1;
            }

            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] == value)
                {
                    return i;
                }
            }

            return -1;
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

            ArgumentOutOfRangeException.ThrowIfNegative(count);

            EnsureCapacity(Length + count);

            // Shift elements to the right
            UnmanagedMemoryHelper.ShiftRight(_ptr, index, count, Length, Capacity);

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
            return new Span<int>(_ptr, Length);
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
        ///     Finalizes an instance of the <see cref="UnmanagedIntArray" /> class.
        /// </summary>
        ~UnmanagedIntArray()
        {
            Dispose(false);
        }

        /// <inheritdoc cref="IDisposable" />
        /// <summary>
        ///     Frees unmanaged memory.
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (_buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_buffer);
                _buffer = IntPtr.Zero;
                _ptr = null;
                Length = 0;
                Capacity = 0;
            }

            _disposed = true;

            // 'disposing' parameter unused but required by pattern.
            _ = disposing;
        }
    }
}
