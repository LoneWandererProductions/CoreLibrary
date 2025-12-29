/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects.Helper
 * FILE:        EntryGenericEnumerator.cs
 * PURPOSE:     Custom enumerator for my unsage ManagedMap
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections;
using System.Collections.Generic;

namespace ExtendedSystemObjects.Helper
{
    /// <inheritdoc />
    /// <summary>
    /// Generic enumerator for iterating over entries in a collection of type <see cref="T:ExtendedSystemObjects.Helper.EntryGeneric`1" />.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="!:System.Collections.Generic.IEnumerator&lt;(System.Int32, TValue)&gt;" />
    public unsafe struct EntryGenericEnumerator<TValue> : IEnumerator<(int, TValue)> where TValue : unmanaged
    {
        /// <summary>
        ///     The entries
        /// </summary>
        private readonly EntryGeneric<TValue>* _entries;

        /// <summary>
        ///     The capacity
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     The index
        /// </summary>
        private int _index;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EntryGenericEnumerator{TValue}" /> struct.
        /// </summary>
        /// <param name="entries">The entries.</param>
        /// <param name="capacity">The capacity.</param>
        public EntryGenericEnumerator(EntryGeneric<TValue>* entries, int capacity)
        {
            _entries = entries;
            _capacity = capacity;
            _index = -1;
            Current = default;
        }

        /// <summary>
        ///     Gets the current.
        /// </summary>
        /// <value>
        ///     The current.
        /// </value>
        public (int, TValue) Current { get; private set; }

        /// <summary>
        ///     Gets the current.
        /// </summary>
        /// <value>
        ///     The current.
        /// </value>
        readonly object IEnumerator.Current => Current;

        /// <summary>
        ///     Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" />
        ///     if the enumerator has passed the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            while (++_index < _capacity)
            {
                var entry = _entries[_index];
                if (entry.Used != SharedResources.Occupied)
                {
                    continue;
                }

                Current = (entry.Key, entry.Value);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            _index = -1;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public readonly void Dispose() { }
    }
}
