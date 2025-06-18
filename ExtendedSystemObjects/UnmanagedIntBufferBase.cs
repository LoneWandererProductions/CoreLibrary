using System;
using System.Runtime.InteropServices;

namespace ExtendedSystemObjects
{
    public abstract unsafe class UnmanagedIntBufferBase : IDisposable
    {
        /// <summary>
        /// The buffer
        /// </summary>
        private IntPtr _buffer;

        /// <summary>
        /// The pointer
        /// </summary>
        private int* _ptr;

        protected int _capacity;
        protected bool _disposed;

        /// <summary>
        ///     Gets the current number of elements in the array.
        /// </summary>
        public int Length { get; protected set; }

        /// <summary>
        /// Gets or sets the <see cref="T" /> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="T" />.
        /// </value>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        /// <exception cref="System.IndexOutOfRangeException"></exception>
        public int this[int index]
        {
            get
            {
#if DEBUG
                if (index < 0 || index >= Length) throw new IndexOutOfRangeException();
#endif
                return _ptr[index];
            }
            set
            {
#if DEBUG
                if (index < 0 || index >= Length) throw new IndexOutOfRangeException();
#endif
                _ptr[index] = value;
            }
        }

        protected void Allocate(int initialCapacity)
        {
            _capacity = initialCapacity > 0 ? initialCapacity : 4;
            _buffer = Marshal.AllocHGlobal(_capacity * sizeof(int));
            _ptr = (int*)_buffer;
        }

        /// <summary>
        ///     Ensures capacity to hold at least minCapacity elements.
        ///     Grows capacity exponentially if needed.
        /// </summary>
        protected void EnsureCapacity(int min)
        {
            if (min <= _capacity) return;

            var newCapacity = _capacity * 2;
            if (newCapacity < min) newCapacity = min;

            _buffer = Marshal.ReAllocHGlobal(_buffer, (IntPtr)(newCapacity * sizeof(int)));
            _ptr = (int*)_buffer;
            _capacity = newCapacity;
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        protected void Clear()
        {
            for (var i = 0; i < Length; i++)
            {
                _ptr[i] = 0;
            }
        }

        /// <summary>
        ///     Returns a Span over the used portion of the array.
        /// </summary>
        public Span<int> AsSpan() => new Span<int>(_ptr, Length);

        /// <summary>
        ///     Frees unmanaged memory.
        /// </summary>
        public virtual void Dispose()
        {
            if (_disposed) return;

            if (_buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_buffer);
                _buffer = IntPtr.Zero;
                _ptr = null;
                _capacity = 0;
                Length = 0;
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~UnmanagedIntBufferBase() => Dispose();
    }

}
