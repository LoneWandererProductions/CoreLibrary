using System;
using System.Runtime.InteropServices;

namespace ExtendedSystemObjects
{
    /// <summary>
    /// Unsafe array
    /// </summary>
    /// <typeparam name="T">Generic Type, must be unmanaged</typeparam>
    /// <seealso cref="System.IDisposable" />
    public unsafe class UnmanagedArray<T> : IUnmanagedArray<T>, IDisposable where T : unmanaged
    {
        private IntPtr _buffer;
        private T* _ptr;
        private int _length;

        public int Length => _length;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnmanagedArray{T}"/> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public UnmanagedArray(int size)
        {
            _length = size;
            _buffer = Marshal.AllocHGlobal(size * sizeof(T));
            _ptr = (T*)_buffer;
        }

        /// <summary>
        /// Gets or sets the <see cref="T"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="T"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The value at the specified index.</returns>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public T this[int index]
        {
            get
            {
#if DEBUG
                if (index < 0 || index >= _length) throw new IndexOutOfRangeException();
#endif
                return _ptr[index];
            }
            set
            {
#if DEBUG
                if (index < 0 || index >= _length) throw new IndexOutOfRangeException();
#endif
                _ptr[index] = value;
            }
        }


        /// <summary>
        /// Resizes the internal array to the specified new size.
        /// Contents will be preserved up to the minimum of old and new size.
        /// </summary>
        /// <param name="newSize">The new size of the array.</param>
        public void Resize(int newSize)
        {
            _buffer = Marshal.ReAllocHGlobal(_buffer, (IntPtr)(newSize * sizeof(T)));
            _ptr = (T*)_buffer;
            _length = newSize;
        }

        /// <summary>
        /// Clears the array by setting all elements to zero.
        /// </summary>
        public void Clear()
        {
            // Use Span<T>.Clear for safety and type correctness
            AsSpan().Clear();
        }

        public Span<T> AsSpan() => new Span<T>(_ptr, _length);

        public void Dispose()
        {
            if (_buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_buffer);
                _buffer = IntPtr.Zero;
                _ptr = null;
                _length = 0;
            }
        }
    }
}
