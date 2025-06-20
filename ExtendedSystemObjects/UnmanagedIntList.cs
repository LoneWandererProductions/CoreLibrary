/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/UnmanagedIntList.cs
 * PURPOSE:     A high-performance List implementation with reduced features. Limited to integer Values.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ExtendedSystemObjects.Helper;
using ExtendedSystemObjects.Interfaces;

namespace ExtendedSystemObjects
{
    /// <inheritdoc cref="IUnmanagedArray" />
    /// <summary>
    ///     A high-performance list of integers backed by unmanaged memory.
    ///     Supports fast adding, popping, and random access with minimal overhead.
    ///     Designed for scenarios where manual memory management is needed.
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    public sealed unsafe class UnmanagedIntList : IUnmanagedArray<int>, IEnumerable<int>
    {
        /// <summary>
        ///     The buffer
        /// </summary>
        private IntPtr _buffer;

        /// <summary>
        ///     The disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     Pointer to the unmanaged buffer holding the integer elements.
        /// </summary>
        private int* _ptr;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnmanagedIntList" /> class with the specified initial capacity.
        /// </summary>
        /// <param name="initialCapacity">The initial number of elements the list can hold without resizing. Default is 16.</param>
        public UnmanagedIntList(int initialCapacity = 16)
        {
            Capacity = initialCapacity > 0 ? initialCapacity : 16;
            _buffer = UnmanagedMemoryHelper.Allocate<int>(Capacity);
            _ptr = (int*)_buffer;
            Clear();
        }

        /// <summary>
        ///     The capacity
        /// </summary>
        public int Capacity { get; private set; }

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
        ///     Gets the number of elements contained in the <see cref="UnmanagedIntList" />.
        /// </summary>
        public int Length { get; private set; }

        /// <inheritdoc />
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
                if (i < 0 || i >= Length)
                {
                    throw new IndexOutOfRangeException();
                }
#endif
                return _ptr[i];
            }
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
        ///     Removes at.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count we want to remove. Optional.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">index</exception>
        public void RemoveAt(int index, int count = 1)
        {
#if DEBUG
            if (index < 0 || index >= Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
#endif

            if (index < Length - 1)
            {
                // Shift elements left by 1 to fill the gap
                UnmanagedMemoryHelper.ShiftLeft(_ptr, index, 1, Length);
            }

            Length--;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Resizes the specified new size.
        /// </summary>
        /// <param name="newSize">The new size.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">newSize</exception>
        public void Resize(int newSize)
        {
            if (newSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(newSize));
            }

            EnsureCapacity(newSize);
            Length = newSize;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Removes all elements from the list. The capacity remains unchanged.
        /// </summary>
        public void Clear()
        {
            Length = 0;

            // Clear the entire allocated capacity, not just Length items
            UnmanagedMemoryHelper.Clear<int>(_buffer, Capacity);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Frees unmanaged resources used by the <see cref="T:ExtendedSystemObjects.IntList" />.
        ///     After calling this method, the instance should not be used.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        ///     Binaries the search.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Index of value in the list.</returns>
        public int BinarySearch(int value)
        {
            var span = AsSpan();
            return span.BinarySearch(value);
        }

        /// <summary>
        ///     Sorts this instance.
        /// </summary>
        public void Sort()
        {
            AsSpan().Sort(); // Uses Array.Sort internally
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
            EnsureCapacity(Length + 1);
            _ptr[Length++] = value;
        }

        /// <summary>
        ///     Removes and returns the last element from the list.
        /// </summary>
        /// <returns>The last integer element in the list.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the list is empty.</exception>
        public int Pop()
        {
            if (Length == 0)
            {
                throw new InvalidOperationException("Stack empty");
            }

            return _ptr[--Length];
        }

        /// <summary>
        ///     Returns the last element without removing it from the list.
        /// </summary>
        /// <returns>The last integer element in the list.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the list is empty.</exception>
        public int Peek()
        {
            if (Length == 0)
            {
                throw new InvalidOperationException("Stack empty");
            }

            return _ptr[Length - 1];
        }

        /// <summary>
        ///     Inserts at.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        /// <param name="count">The count.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">index</exception>
        public void InsertAt(int index, int value, int count = 1)
        {
#if DEBUG
            if (index < 0 || index > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
#endif
            if (count <= 0)
            {
                return;
            }

            EnsureCapacity(Length + count);

            // Shift elements to the right
            UnmanagedMemoryHelper.ShiftRight(_ptr, index, count, Length, Capacity);

            // Fill with value
            for (var i = 0; i < count; i++)
            {
                _ptr[index + i] = value;
            }

            Length += count;
        }

        /// <summary>
        ///     Returns a span over the valid elements of the list.
        ///     Allows fast, safe access to the underlying data.
        /// </summary>
        /// <returns>A <see cref="Span{Int32}" /> representing the list's contents.</returns>
        public Span<int> AsSpan()
        {
            return new Span<int>(_ptr, Capacity);
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="UnmanagedIntList" /> class, releasing unmanaged resources.
        /// </summary>
        ~UnmanagedIntList()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            // Free unmanaged resources
            if (_buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_buffer);
                _buffer = IntPtr.Zero;
                _ptr = null;
                Capacity = 0;
                Length = 0;
            }

            // If you had managed disposable members and disposing is true,
            // dispose them here. None exist for now.

            _disposed = true; // Always set to true after dispose

            // Suppress unused parameter warning
            _ = disposing;
        }

        /// <summary>
        ///     Ensures the capacity of the internal buffer is at least the specified minimum size.
        ///     Resizes the buffer if necessary by doubling its capacity or setting it to the minimum required size.
        /// </summary>
        /// <param name="min">The minimum capacity required.</param>
        private void EnsureCapacity(int min)
        {
            if (min <= Capacity)
            {
                return;
            }

            var newCapacity = Capacity * 2;
            if (newCapacity < min)
            {
                newCapacity = min;
            }

            _buffer = UnmanagedMemoryHelper.Reallocate<int>(_buffer, newCapacity);
            _ptr = (int*)_buffer;
            Capacity = newCapacity;
        }
    }
}
