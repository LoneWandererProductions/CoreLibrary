/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects.Helper
 * FILE:        ExtendedSystemObjects.Helper/Enumerator.cs
 * PURPOSE:     Since I use an older .etn Version I need to use this helper
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ExtendedSystemObjects.Helper
{
    /// <inheritdoc />
    /// <summary>
    ///     Enumerator Helper
    /// </summary>
    /// <typeparam name="T">Generic Type, must be unmanaged</typeparam>
    /// <seealso cref="T:System.Collections.Generic.IEnumerator`1" />
    internal unsafe struct Enumerator<T> : IEnumerator<T> where T : unmanaged
    {
        /// <summary>
        ///     The data
        /// </summary>
        private readonly T* _data;

        /// <summary>
        ///     The length
        /// </summary>
        private readonly int _length;

        /// <summary>
        ///     The index
        /// </summary>
        private int _index;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Enumerator{T}" /> struct.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="length">The length.</param>
        public Enumerator(T* data, int length)
        {
            _data = data;
            _length = length;
            _index = -1;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the current.
        /// </summary>
        /// <value>
        ///     The current.
        /// </value>
        public readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data[_index];
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the current.
        /// </summary>
        /// <value>
        ///     The current.
        /// </value>
        readonly object IEnumerator.Current => Current;

        /// <inheritdoc />
        /// <summary>
        ///     Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" />
        ///     if the enumerator has passed the end of the collection.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            return ++_index < _length;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _index = -1;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public readonly void Dispose()
        {
            /* no resources to clean */
        }
    }
}
