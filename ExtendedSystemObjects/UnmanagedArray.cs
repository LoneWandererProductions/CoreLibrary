/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/IUnmanagedArray.cs
 * PURPOSE:     A high-performance array implementation with reduced features. Limited to unmanaged Types, very similiar to IntArray.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System;
using System.Runtime.InteropServices;

namespace ExtendedSystemObjects
{
    /// <inheritdoc cref="IDisposable" />
    /// <summary>
    ///     Unsafe array
    /// </summary>
    /// <typeparam name="T">Generic Type, must be unmanaged</typeparam>
    /// <seealso cref="T:System.IDisposable" />
    public sealed unsafe class UnmanagedArray<T> : IUnmanagedArray<T> where T : unmanaged
    {
        /// <summary>
        /// The buffer
        /// </summary>
        private IntPtr _buffer;

        /// <summary>
        /// The pointer 
        /// </summary>
        private T* _ptr;

        /// <summary>
        /// The capacity
        /// </summary>
        private int _capacity;


        /// <summary>
        /// The disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnmanagedArray{T}" /> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public UnmanagedArray(int size)
        {
            _capacity = size;
            Length = size;
            _buffer = Marshal.AllocHGlobal(size * sizeof(T));
            _ptr = (T*)_buffer;
            Clear();
        }

        /// <summary>
        ///     Gets or sets the <see cref="T" /> at the specified index.
        /// </summary>
        /// <value>
        ///     The <see cref="T" />.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The value at the specified index.</returns>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
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

        /// <summary>
        ///     Resizes the internal array to the specified new size.
        ///     Contents will be preserved up to the minimum of old and new size.
        /// </summary>
        /// <param name="newSize">The new size of the array.</param>
        public void Resize(int newSize)
        {
            _buffer = Marshal.ReAllocHGlobal(_buffer, (IntPtr)(newSize * sizeof(T)));
            _ptr = (T*)_buffer;
            _capacity = newSize;
            if (Length > newSize)
                Length = newSize;
        }

        /// <summary>
        ///     Clears the array by setting all elements to zero.
        /// </summary>
        public void Clear()
        {
            // Use Span<T>.Clear for safety and type correctness
            AsSpan().Clear();
        }

        /// <summary>
        /// Ensures the capacity.
        /// </summary>
        /// <param name="minCapacity">The minimum capacity.</param>
        public void EnsureCapacity(int minCapacity)
        {
            if (minCapacity <= _capacity)
                return;

            int newCapacity = _capacity == 0 ? 4 : _capacity;
            while (newCapacity < minCapacity)
                newCapacity *= 2;

            Resize(newCapacity);
        }

        /// <summary>
        /// Ases the span.
        /// </summary>
        /// <returns>Return all Values as Span</returns>
        public Span<T> AsSpan()
        {
            return new(_ptr, Length);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="UnmanagedArray{T}"/> class.
        /// </summary>
        ~UnmanagedArray()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            // Only unmanaged cleanup here.
            if (_buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_buffer);
                _buffer = IntPtr.Zero;
                _ptr = null;
                Length = 0;
                _capacity = 0;
            }

            _disposed = true;

            // 'disposing' parameter unused but required by pattern.
            _ = disposing;
        }
    }
}
