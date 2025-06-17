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
        private IntPtr _buffer;
        private T* _ptr;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UnmanagedArray{T}" /> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public UnmanagedArray(int size)
        {
            Length = size;
            _buffer = Marshal.AllocHGlobal(size * sizeof(T));
            _ptr = (T*)_buffer;
        }

        public int Length { get; private set; }

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

        public void Dispose()
        {
            if (_buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_buffer);
                _buffer = IntPtr.Zero;
                _ptr = null;
                Length = 0;
            }
        }

        public Span<T> AsSpan()
        {
            return new(_ptr, Length);
        }
    }
}
