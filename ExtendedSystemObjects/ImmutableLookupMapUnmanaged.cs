/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ImmutableLookupMap.cs
 * PURPOSE:     A high-performance, immutable lookup map that uses an array-based internal structure for fast key-value lookups.
 *              This version is limited to unmanaged types and uses UnmanagedArray<T>.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ExtendedSystemObjects.Helper;

namespace ExtendedSystemObjects
{
    /// <inheritdoc cref="IDisposable" />
    /// <summary>
    ///      A high-performance, immutable lookup map using unmanaged arrays.
    ///      Suitable for value types only. Keys must be unique.
    /// </summary>
    public sealed unsafe class ImmutableLookupMapUnmanaged<TKey, TValue> : IDisposable,
        IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        /// <summary>
        ///      Internal entry structure to ensure data locality (Cache-friendly).
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct Entry
        {
            public byte IsPresent;
            public TKey Key;
            public TValue Value;
        }

        /// <summary>
        ///      The capacity (Power of 2).
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///      The bitwise mask (Capacity - 1).
        /// </summary>
        private readonly int _mask;

        /// <summary>
        ///      The unified unmanaged array of entries.
        /// </summary>
        private readonly UnmanagedArray<Entry> _entries;

        /// <summary>
        /// The disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///      Initializes a new instance of the <see cref="ImmutableLookupMapUnmanaged{TKey, TValue}" /> class
        ///      with the specified key-value data.
        /// </summary>
        /// <param name="data">A dictionary containing the initial key-value pairs.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data" /> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a duplicate key is found.</exception>
        public ImmutableLookupMapUnmanaged(IDictionary<TKey, TValue> data)
        {
            ArgumentNullException.ThrowIfNull(data);

            Count = data.Count;

            // 1. Calculate Power-of-2 capacity for bitwise & performance
            _capacity = (int)BitOperations.RoundUpToPowerOf2((uint)Count * 2);
            if (_capacity < 16) _capacity = 16;
            _mask = _capacity - 1;

            _entries = new UnmanagedArray<Entry>(_capacity);
            var entriesPtr = _entries.Pointer;

            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                var hash = GetHash(key) & _mask;
                var placed = false;

                // 2. Linear Probing for cache line efficiency
                for (var i = 0; i < _capacity; i++)
                {
                    var index = (int)((uint)(hash + i) & (uint)_mask);
                    var entry = entriesPtr + index;

                    if (entry->IsPresent == 0)
                    {
                        entry->Key = key;
                        entry->Value = value;
                        entry->IsPresent = 1;
                        placed = true;
                        break;
                    }

                    if (entry->Key.Equals(key))
                    {
                        throw new InvalidOperationException(string.Format(SharedResources.ErrorDuplicateKey, key));
                    }
                }

                if (!placed) throw new InvalidOperationException("Internal map overflow.");
            }
        }

        /// <summary>
        ///      Gets the number of entries in the map.
        /// </summary>
        public int Count { get; }

        /// <inheritdoc />
        /// <summary>
        ///      Releases the unmanaged memory used by the lookup map.
        /// </summary>
        public void Dispose()
        {
            if(_disposed) return;
            _entries.Dispose();
            _disposed = true;
        }

        /// <inheritdoc />
        /// <summary>
        ///      Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to lookup.</param>
        /// <returns>The value associated with the key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key is not found in the map.</exception>
        public TValue Get(TKey key)
        {
            var hash = GetHash(key) & _mask;
            var entriesPtr = _entries.Pointer;

            for (var i = 0; i < _capacity; i++)
            {
                var index = (hash + i) & _mask;
                var entry = entriesPtr + index;

                if (entry->IsPresent == 0) break;

                if (entry->Key.Equals(key))
                {
                    return entry->Value;
                }
            }

            throw new KeyNotFoundException(SharedResources.ErrorValueNotFound);
        }

        /// <summary>
        ///      Attempts to retrieve the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to lookup.</param>
        /// <param name="value">
        ///      When this method returns, contains the value associated with the key, if found; otherwise, the
        ///      default value.
        /// </param>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            var hash = GetHash(key) & _mask;
            var entriesPtr = _entries.Pointer;

            for (var i = 0; i < _capacity; i++)
            {
                var index = (hash + i) & _mask;
                var entry = entriesPtr + index;

                if (entry->IsPresent == 0) break;

                if (entry->Key.Equals(key))
                {
                    value = entry->Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        ///      Computes a non-negative hash code for the key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A non-negative integer hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetHash(TKey key)
        {
            return key.GetHashCode();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>An enumerator for the lookup map.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_entries.Pointer, _capacity);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// High performance enumerator for iterating over the entries in the lookup map. Uses unsafe code and pointer arithmetic for maximum speed.
        /// </summary>
        /// <seealso cref="System.IDisposable" />
        /// <seealso cref="System.Collections.Generic.IEnumerable&lt;System.Collections.Generic.KeyValuePair&lt;TKey, TValue&gt;&gt;" />
        public unsafe struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly Entry* _entries;
            private readonly int _capacity;
            private int _index;
            private KeyValuePair<TKey, TValue> _current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(Entry* entries, int capacity)
            {
                _entries = entries;
                _capacity = capacity;
                _index = -1;
                _current = default;
            }

            /// <inheritdoc />
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                // Nutzt superschnelle Pointer-Arithmetik statt des Array-Indexers
                while (++_index < _capacity)
                {
                    Entry* entry = _entries + _index;
                    if (entry->IsPresent != 0)
                    {
                        _current = new KeyValuePair<TKey, TValue>(entry->Key, entry->Value);
                        return true;
                    }
                }
                return false;
            }

            /// <inheritdoc />
            public readonly KeyValuePair<TKey, TValue> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _current;
            }

            /// <inheritdoc />
            readonly object IEnumerator.Current => Current;

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                _index = -1;
                _current = default;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public readonly void Dispose()
            {
            }
        }
    }
}
