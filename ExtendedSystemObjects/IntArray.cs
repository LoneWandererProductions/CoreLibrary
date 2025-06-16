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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExtendedSystemObjects
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a high-performance, low-overhead array of integers
    /// backed by unmanaged memory. Designed for performance-critical
    /// scenarios where garbage collection overhead must be avoided.
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    public sealed unsafe class IntArray : IDisposable
    {
        /// <summary>
        /// The buffer
        /// </summary>
        private IntPtr _buffer;

        /// <summary>
        /// The length
        /// </summary>
        private int _length;

        /// <summary>
        /// The pointer
        /// </summary>
        private int* _ptr;

        /// <summary>
        /// Gets the current number of elements in the array.
        /// </summary>
        public int Length => _length;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntArray"/> class with the specified size.
        /// </summary>
        /// <param name="size">The number of elements to allocate.</param>
        public IntArray(int size)
        {
            _length = size;
            _buffer = Marshal.AllocHGlobal(size * sizeof(int));
            _ptr = (int*)_buffer;
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
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
                if (i < 0 || i >= _length) throw new IndexOutOfRangeException();
#endif
                return _ptr[i];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
#if DEBUG
                if (i < 0 || i >= _length) throw new IndexOutOfRangeException();
#endif
                _ptr[i] = value;
            }
        }

        /// <summary>
        /// Removes the element at the specified index by shifting remaining elements left.
        /// </summary>
        /// <param name="index">The index of the element to remove.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAt(int index)
        {
#if DEBUG
            if (index < 0 || index >= _length) throw new IndexOutOfRangeException();
#endif
            var ptr = (int*)_buffer;

            for (var i = index; i < _length - 1; i++)
            {
                ptr[i] = ptr[i + 1];
            }

            _length--; // Reduces logical size, but memory is not freed
        }

        /// <summary>
        /// Resizes the internal array to the specified new size.
        /// Contents will be preserved up to the minimum of old and new size.
        /// </summary>
        /// <param name="newSize">The new size of the array.</param>
        public void Resize(int newSize)
        {
            _buffer = Marshal.ReAllocHGlobal(_buffer, (IntPtr)(newSize * sizeof(int)));
            _ptr = (int*)_buffer;
            _length = newSize;
        }

        /// <summary>
        /// Clears the array by setting all elements to zero.
        /// </summary>
        public void Clear()
        {
            var ptr = (int*)_buffer;
            for (var i = 0; i < _length; i++)
            {
                ptr[i] = 0;
            }
        }

        /// <summary>
        /// Returns a span over the current memory buffer, enabling safe, fast access.
        /// </summary>
        /// <returns>A <see cref="Span{Int32}"/> over the internal buffer.</returns>
        public Span<int> AsSpan() => new((void*)_buffer, _length);

        /// <summary>
        /// Finalizes an instance of the <see cref="IntArray"/> class.
        /// </summary>
        ~IntArray()
        {
            Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        /// Frees the unmanaged memory held by the array.
        /// After disposal, the instance should not be used.
        /// </summary>
        public void Dispose()
        {
            Marshal.FreeHGlobal(_buffer);
            _buffer = IntPtr.Zero;
            _length = 0;
        }
    }
}
