/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/IntList.cs
 * PURPOSE:     A high-performance List implementation with reduced features. Limited to integer Values.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal

using System;
using System.Runtime.InteropServices;

namespace ExtendedSystemObjects
{
    /// <inheritdoc />
    /// <summary>
    ///     A high-performance list of integers backed by unmanaged memory.
    ///     Supports fast adding, popping, and random access with minimal overhead.
    ///     Designed for scenarios where manual memory management is needed.
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    public sealed unsafe class IntList : IDisposable
    {
        /// <summary>
        ///     The buffer
        /// </summary>
        private IntPtr _buffer;

        /// <summary>
        ///     The capacity
        /// </summary>
        private int _capacity;

        /// <summary>
        ///     Pointer to the unmanaged buffer holding the integer elements.
        /// </summary>
        private int* _ptr;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IntList" /> class with the specified initial capacity.
        /// </summary>
        /// <param name="initialCapacity">The initial number of elements the list can hold without resizing. Default is 16.</param>
        public IntList(int initialCapacity = 16)
        {
            _capacity = initialCapacity > 0 ? initialCapacity : 16;
            _buffer = Marshal.AllocHGlobal(_capacity * sizeof(int));
            _ptr = (int*)_buffer;
        }

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="IntList" />.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        ///     Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="i">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown in debug builds if the index is out of bounds.</exception>
        public int this[int i]
        {
            get
            {
#if DEBUG
                if (i < 0 || i >= Count)
                {
                    throw new IndexOutOfRangeException();
                }
#endif
                return _ptr[i];
            }
            set
            {
#if DEBUG
                if (i < 0 || i >= Count)
                {
                    throw new IndexOutOfRangeException();
                }
#endif
                _ptr[i] = value;
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Frees unmanaged resources used by the <see cref="T:ExtendedSystemObjects.IntList" />.
        ///     After calling this method, the instance should not be used.
        /// </summary>
        public void Dispose()
        {
            if (_buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_buffer);
                _buffer = IntPtr.Zero;
            }

            Count = 0;
            _capacity = 0;
        }

        /// <summary>
        ///     Pushes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Push(int value)
        {
            Add(value);
        }

        /// <summary>
        ///     Adds an integer value to the end of the list, resizing if necessary.
        /// </summary>
        /// <param name="value">The integer value to add.</param>
        public void Add(int value)
        {
            EnsureCapacity(Count + 1);
            _ptr[Count++] = value;
        }

        /// <summary>
        ///     Removes and returns the last element from the list.
        /// </summary>
        /// <returns>The last integer element in the list.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the list is empty.</exception>
        public int Pop()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Stack empty");
            }

            return _ptr[--Count];
        }

        /// <summary>
        ///     Returns the last element without removing it from the list.
        /// </summary>
        /// <returns>The last integer element in the list.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the list is empty.</exception>
        public int Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Stack empty");
            }

            return _ptr[Count - 1];
        }

        /// <summary>
        ///     Ensures the capacity of the internal buffer is at least the specified minimum size.
        ///     Resizes the buffer if necessary by doubling its capacity or setting it to the minimum required size.
        /// </summary>
        /// <param name="min">The minimum capacity required.</param>
        private void EnsureCapacity(int min)
        {
            if (min <= _capacity)
            {
                return;
            }

            var newCapacity = _capacity * 2;
            if (newCapacity < min)
            {
                newCapacity = min;
            }

            _buffer = Marshal.ReAllocHGlobal(_buffer, (IntPtr)(newCapacity * sizeof(int)));
            _ptr = (int*)_buffer;
            _capacity = newCapacity;
        }

        /// <summary>
        ///     Removes all elements from the list. The capacity remains unchanged.
        /// </summary>
        public void Clear()
        {
            Count = 0;
        }

        /// <summary>
        ///     Returns a span over the valid elements of the list.
        ///     Allows fast, safe access to the underlying data.
        /// </summary>
        /// <returns>A <see cref="Span{Int32}" /> representing the list's contents.</returns>
        public Span<int> AsSpan()
        {
            return new((void*)_buffer, Count);
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="IntList" /> class, releasing unmanaged resources.
        /// </summary>
        ~IntList()
        {
            Dispose();
        }
    }
}
