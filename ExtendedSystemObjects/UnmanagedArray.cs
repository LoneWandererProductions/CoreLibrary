/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/IUnmanagedArray.cs
 * PURPOSE:     A high-performance array implementation with reduced features. Limited to unmanaged Types, very similar to IntArray.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ExtendedSystemObjects.Helper;
using ExtendedSystemObjects.Interfaces;

namespace ExtendedSystemObjects
{
    /// <inheritdoc cref="IDisposable" />
    /// <summary>
    ///     Unsafe array
    /// </summary>
    /// <typeparam name="T">Generic Type, must be unmanaged</typeparam>
    /// <seealso cref="T:System.IDisposable" />
    public sealed unsafe class UnmanagedArray<T> : IUnmanagedArray<T>, IEnumerable<T> where T : unmanaged
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
        private T* _ptr;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnmanagedArray{T}" /> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public UnmanagedArray(int size)
        {
            Capacity = size;
            Length = size;

            _buffer = UnmanagedMemoryHelper.Allocate<T>(size);
            _ptr = (T*)_buffer;
            UnmanagedMemoryHelper.Clear<T>(_buffer, size);
        }

        /// <summary>
        ///     The capacity of the current Array.
        /// </summary>
        /// <value>
        ///     The capacity.
        /// </value>
        public int Capacity { get; private set; }


        /// <inheritdoc />
        /// <summary>
        ///     Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        ///     An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator<T>(_ptr, Length);
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
        /// <inheritdoc />
        /// <summary>
        ///     Gets the length.
        /// </summary>
        /// <value>
        ///     The length.
        /// </value>
        public int Length { get; private set; }

        /// <inheritdoc />
        /// <summary>
        ///     Gets or sets the <see cref="!:T" /> at the specified index.
        /// </summary>
        /// <value>
        ///     The <see cref="!:T" />.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The value at the specified index.</returns>
        /// <exception cref="T:System.IndexOutOfRangeException"></exception>
        public T this[int index]
        {
            get
            {
#if DEBUG
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }
#endif
                return _ptr[index];
            }
            set
            {
#if DEBUG
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }
#endif
                _ptr[index] = value;
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Removes elements starting at the given index.
        /// </summary>
        /// <param name="index">The start index.</param>
        /// <param name="count">Number of elements to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">index or count is invalid.</exception>
        public void RemoveAt(int index, int count = 1)
        {
            if (index < 0 || index >= Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 1 || index + count > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            UnmanagedMemoryHelper.ShiftLeft(_ptr, index, count, Length);
            Length -= count;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Resizes the internal array to the specified new size.
        ///     Contents will be preserved up to the minimum of old and new size.
        /// </summary>
        /// <param name="newSize">The new size of the array.</param>
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

            var newBuffer = UnmanagedMemoryHelper.Reallocate<T>(_buffer, newSize);
            var newPtr = (T*)newBuffer;

            if (newSize > Capacity)
            {
                UnmanagedMemoryHelper.Clear<T>(new IntPtr(newPtr + Capacity), newSize - Capacity);
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
        ///     Clears the array by setting all elements to zero.
        /// </summary>
        public void Clear()
        {
            // Use Span<T>.Clear for safety and type correctness
            UnmanagedMemoryHelper.Clear<T>(_buffer, Length);
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
        ///     Inserts at.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        /// <param name="count">The count.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     index
        ///     or
        ///     count
        /// </exception>
        public void InsertAt(int index, T value, int count = 1)
        {
            if (index < 0 || index > Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count == 0)
            {
                return;
            }

            EnsureCapacity(Length + count);

            // Shift elements to the right
            UnmanagedMemoryHelper.ShiftRight(_ptr, index, count, Length, Capacity);

            // Fill inserted region with 'value'
            for (var i = 0; i < count; i++)
            {
                _ptr[index + i] = value;
            }

            Length += count;
        }

        /// <summary>
        ///     Ensures the capacity.
        /// </summary>
        /// <param name="minCapacity">The minimum capacity.</param>
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
        ///     Access the span.
        /// </summary>
        /// <returns>Return all Values as Span</returns>
        public Span<T> AsSpan()
        {
            return new Span<T>(_ptr, Length);
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="UnmanagedArray{T}" /> class.
        /// </summary>
        ~UnmanagedArray()
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

            // Only unmanaged cleanup here.
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
