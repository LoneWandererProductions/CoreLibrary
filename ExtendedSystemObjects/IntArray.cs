/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/IntArray.cs
 * PURPOSE:     A high-performance array implementation with reduced features. Limited to integer Values.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExtendedSystemObjects
{
    /// <inheritdoc cref="IDisposable" />
    /// <summary>
    ///     Represents a high-performance, low-overhead array of integers
    ///     backed by unmanaged memory. Designed for performance-critical
    ///     scenarios where garbage collection overhead must be avoided.
    /// </summary>
    public sealed unsafe class IntArray : IUnmanagedArray<int>, IDisposable
    {
        private IntPtr _buffer;
        private int* _ptr;

        private int _capacity;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IntArray" /> class with the specified size.
        /// </summary>
        /// <param name="size">The number of elements to allocate.</param>
        public IntArray(int size)
        {
            if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));

            _capacity = size;
            Length = size;

            _buffer = Marshal.AllocHGlobal(size * sizeof(int));
            _ptr = (int*)_buffer;

            Clear(); // Optional: zero out memory on allocation
        }

        /// <summary>
        ///     Gets the current number of elements in the array.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        ///     Gets the current allocated capacity.
        /// </summary>
        public int Capacity => _capacity;

        public int this[int i]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
#if DEBUG
                if (i < 0 || i >= Length) throw new IndexOutOfRangeException();
#endif
                return _ptr[i];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
#if DEBUG
                if (i < 0 || i >= Length) throw new IndexOutOfRangeException();
#endif
                _ptr[i] = value;
            }
        }

        /// <summary>
        /// Inserts 'count' copies of 'value' at the given index.
        /// </summary>
        public void InsertAt(int index, int value, int count = 1)
        {
            if (index < 0 || index > Length) throw new ArgumentOutOfRangeException(nameof(index));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));

            EnsureCapacity(Length + count);

            int shiftCount = Length - index;
            if (shiftCount > 0)
            {
                Buffer.MemoryCopy(
                    _ptr + index,
                    _ptr + index + count,
                    (_capacity - (index + count)) * sizeof(int),
                    shiftCount * sizeof(int));
            }

            for (int i = 0; i < count; i++)
            {
                _ptr[index + i] = value;
            }

            Length += count;
        }

        /// <summary>
        /// Removes the element at the specified index by shifting remaining elements left.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
#if DEBUG
            if (index < 0 || index >= Length) throw new IndexOutOfRangeException();
#endif
            for (int i = index; i < Length - 1; i++)
            {
                _ptr[i] = _ptr[i + 1];
            }

            Length--;
        }

        /// <summary>
        /// Removes multiple elements efficiently, given sorted indices.
        /// </summary>
        public void RemoveMultiple(ReadOnlySpan<int> indices)
        {
            if (indices.Length == 0) return;

            // Fast path for consecutive indices
            if (indices.Length > 1 && indices[^1] - indices[0] == indices.Length - 1)
            {
                int start = indices[0];
                int count = indices.Length;

#if DEBUG
                if (start < 0 || start + count > Length) throw new IndexOutOfRangeException();
#endif

                int moveCount = Length - (start + count);
                if (moveCount > 0)
                {
                    Buffer.MemoryCopy(
                        _ptr + start + count,
                        _ptr + start,
                        (_capacity - start) * sizeof(int),
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
        /// Resizes the internal buffer to the new capacity.
        /// If newSize is smaller than current Length, Length is reduced.
        /// </summary>
        public void Resize(int newSize)
        {
            if (newSize < 0) throw new ArgumentOutOfRangeException(nameof(newSize));

            _buffer = Marshal.ReAllocHGlobal(_buffer, (IntPtr)(newSize * sizeof(int)));
            _ptr = (int*)_buffer;
            _capacity = newSize;

            if (Length > newSize)
                Length = newSize;
        }

        /// <summary>
        /// Clears all elements to zero.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < Length; i++)
            {
                _ptr[i] = 0;
            }
        }

        /// <summary>
        /// Returns a Span over the used portion of the array.
        /// </summary>
        public Span<int> AsSpan()
        {
            return new Span<int>(_ptr, Length);
        }

        /// <summary>
        /// Ensures capacity to hold at least minCapacity elements.
        /// Grows capacity exponentially if needed.
        /// </summary>
        public void EnsureCapacity(int minCapacity)
        {
            if (minCapacity <= _capacity) return;

            int newCapacity = _capacity == 0 ? 4 : _capacity;
            while (newCapacity < minCapacity)
                newCapacity *= 2;

            Resize(newCapacity);
        }

        /// <summary>
        /// Frees unmanaged memory.
        /// </summary>
        public void Dispose()
        {
            if (_buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_buffer);
                _buffer = IntPtr.Zero;
                _ptr = null;
                Length = 0;
                _capacity = 0;
            }

            GC.SuppressFinalize(this);
        }

        ~IntArray()
        {
            Dispose();
        }
    }
}
