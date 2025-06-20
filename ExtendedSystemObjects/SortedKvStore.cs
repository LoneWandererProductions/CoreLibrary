/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/SortedKvStore.cs
 * PURPOSE:     Represents a sorted key-value store with integer keys and integer values. Key must be unique. Occupied internally manages how to handle deletions.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal

using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace ExtendedSystemObjects
{
    /// <inheritdoc cref="IDisposable" />
    /// <summary>
    ///     Represents a sorted key-value store with integer keys and integer values.
    ///     Keys are kept sorted internally to allow efficient binary search operations.
    ///     The class supports insertion, removal, and lookup operations with dynamic storage growth.
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    public sealed class SortedKvStore : IDisposable, IEnumerable<KeyValuePair<int, int>>
    {
        /// <summary>
        ///     The keys
        /// </summary>
        private readonly UnmanagedIntArray _keys;

        /// <summary>
        ///     The occupied Array, 0/1 flags
        /// </summary>
        private readonly UnmanagedIntArray _occupied;

        /// <summary>
        ///     The values
        /// </summary>
        private readonly UnmanagedIntArray _values;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SortedKvStore" /> class with a specified initial capacity.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the store.</param>
        public SortedKvStore(int initialCapacity = 16)
        {
            _keys = new UnmanagedIntArray(initialCapacity);
            _values = new UnmanagedIntArray(initialCapacity);
            _occupied = new UnmanagedIntArray(initialCapacity);
        }

        /// <summary>
        ///     Gets the number of active (occupied) key-value pairs stored.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        ///     Gets the free capacity.
        /// </summary>
        /// <value>
        ///     The free capacity.
        /// </value>
        public int FreeCapacity => _keys.Capacity - Count;

        /// <summary>
        ///     Gets an enumerable collection of all keys currently in the store.
        /// </summary>
        /// <value>
        ///     The keys of the key-value pairs.
        /// </value>
        public IEnumerable<int> Keys
        {
            get
            {
                for (var i = 0; i < Count; i++)
                {
                    if (_occupied[i] != 0)
                    {
                        yield return _keys[i];
                    }
                }
            }
        }

        /// <summary>
        ///     Gets or sets the value associated with the specified key.
        ///     If the key does not exist on get, a <see cref="KeyNotFoundException" /> is thrown.
        ///     If the key exists on set, its value is updated; otherwise, the key-value pair is added.
        /// </summary>
        /// <value>
        ///     The <see cref="System.Int32" />.
        /// </value>
        /// <param name="key">The key to locate or add.</param>
        /// <returns>
        ///     The value associated with the specified key.
        /// </returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Key {key} not found.</exception>
        public int this[int key]
        {
            get
            {
                if (TryTryGetValueGet(key, out var value))
                {
                    return value;
                }

                throw new KeyNotFoundException($"Key {key} not found.");
            }
            set => Add(key, value); // Add already handles update or insert
        }

        /// <inheritdoc />
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _keys.Dispose();
            _values.Dispose();
            _occupied.Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the enumerator.
        /// </summary>
        /// <returns>All active elements as Key Value pair.</returns>
        public IEnumerator<KeyValuePair<int, int>> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                if (_occupied[i] != 0)
                {
                    yield return new KeyValuePair<int, int>(_keys[i], _values[i]);
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the enumerator.
        /// </summary>
        /// <returns>An enumerator to iterate though the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Adds a new key-value pair to the store, or updates the value if the key already exists.
        /// </summary>
        /// <param name="key">The key to add or update.</param>
        /// <param name="value">The value associated with the key.</param>
        public void Add(int key, int value)
        {
            var idx = BinarySearch(key);

            if (idx >= 0)
            {
                if (_occupied[idx] == 0)
                {
                    _occupied[idx] = 1;
                    _values[idx] = value;
                    Count++;
                }
                else
                {
                    _values[idx] = value;
                }

                return;
            }

            idx = ~idx;

            EnsureCapacity();

            _keys.InsertAt(idx, key);
            _values.InsertAt(idx, value);
            _occupied.InsertAt(idx, 1);

            Count++;
        }

        /// <summary>
        ///     Tries to get the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the key, if found; otherwise, the
        ///     default value.
        /// </param>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        public bool TryTryGetValueGet(int key, out int value)
        {
            int left = 0, right = Count - 1;
            var keysSpan = _keys.AsSpan()[..Count];
            var occupiedSpan = _occupied.AsSpan()[..Count];
            var valuesSpan = _values.AsSpan()[..Count];

            while (left <= right)
            {
                var mid = left + ((right - left) >> 1);
                var midKey = keysSpan[mid];
                if (midKey == key)
                {
                    if (occupiedSpan[mid] != 0)
                    {
                        value = valuesSpan[mid];
                        return true;
                    }

                    break;
                }

                if (midKey < key)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Marks the specified key as removed. The item is not physically removed until <see cref="Compact" /> is called.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        public void Remove(int key)
        {
            var idx = BinarySearch(key);
            if (idx >= 0 && _occupied[idx] != 0)
            {
                _occupied[idx] = 0;
            }
        }

        /// <summary>
        ///     Tries to remove the specified key.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <param name="index">When this method returns, contains the index of the removed key if successful; otherwise, -1.</param>
        /// <returns><c>true</c> if the key was removed; otherwise, <c>false</c>.</returns>
        public bool TryRemove(int key, out int index)
        {
            index = BinarySearch(key);
            if (index >= 0 && _occupied[index] != 0)
            {
                _occupied[index] = 0;
                return true;
            }

            index = -1;
            return false;
        }

        /// <summary>
        ///     Removes multiple keys in one batch. Uses optimized path if the input span is sorted.
        ///     Keys are marked as unoccupied but not physically removed.
        /// </summary>
        /// <param name="keysToRemove">A span of keys to remove.</param>
        public void RemoveMany(ReadOnlySpan<int> keysToRemove)
        {
            if (keysToRemove.Length == 0)
            {
                return;
            }

            var occSpan = _occupied.AsSpan()[..Count];
            var keysSpan = _keys.AsSpan()[..Count];

            // Optional: if keysToRemove is sorted, do a linear merge
            // This path is faster than HashSet-based if input is sorted and large
            var isSorted = true;
            for (var i = 1; i < keysToRemove.Length; i++)
            {
                if (keysToRemove[i] >= keysToRemove[i - 1])
                {
                    continue;
                }

                isSorted = false;
                break;
            }

            if (isSorted)
            {
                int i = 0, j = 0;
                while (i < Count && j < keysToRemove.Length)
                {
                    var currentKey = keysSpan[i];
                    var removeKey = keysToRemove[j];

                    if (currentKey < removeKey)
                    {
                        i++;
                    }
                    else if (currentKey > removeKey)
                    {
                        j++;
                    }
                    else
                    {
                        occSpan[i] = 0;

                        i++;
                        j++;
                    }
                }
            }
            else
            {
                // Fall back to binary search (cheap because keys are sorted in store)
                foreach (var t in keysToRemove)
                {
                    var idx = BinarySearch(t);
                    if (idx >= 0 && occSpan[idx] != 0)
                    {
                        occSpan[idx] = 0;
                    }
                }
            }
        }

        /// <summary>
        ///     Physically removes all unoccupied entries to compact the underlying arrays.
        ///     Reduces memory usage and improves lookup performance.
        /// </summary>
        public void Compact()
        {
            var occSpan = _occupied.AsSpan()[..Count];
            var maxRemoved = Count; // worst case: all are removed

            var rented = ArrayPool<int>.Shared.Rent(maxRemoved);
            var removedCount = 0;

            for (var i = 0; i < Count; i++)
            {
                if (occSpan[i] == 0)
                {
                    rented[removedCount++] = i;
                }
            }

            if (removedCount == 0)
            {
                ArrayPool<int>.Shared.Return(rented);
                return;
            }

            var indicesToRemove = new Span<int>(rented, 0, removedCount);

            _keys.RemoveMultiple(indicesToRemove);
            _values.RemoveMultiple(indicesToRemove);
            _occupied.RemoveMultiple(indicesToRemove);

            Count -= removedCount;

            ArrayPool<int>.Shared.Return(rented);
        }

        /// <summary>
        ///     Removes all entries from the store.
        /// </summary>
        public void Clear()
        {
            _keys.Clear();
            _values.Clear();
            _occupied.Clear();
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="SortedKvStore" /> class.
        /// </summary>
        ~SortedKvStore()
        {
            Dispose();
        }

        /// <summary>
        ///     Ensures the underlying arrays have sufficient capacity to hold at least one more entry.
        /// </summary>
        private void EnsureCapacity()
        {
            // Delegate to IntArray.EnsureCapacity, using Count + 1 since we add one item
            _keys.EnsureCapacity(Count + 1);
            _values.EnsureCapacity(Count + 1);
            _occupied.EnsureCapacity(Count + 1);
        }

        /// <summary>
        ///     Performs a binary search for the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>
        ///     The index of the key if found; otherwise, the bitwise complement of the index at which the key should be inserted.
        /// </returns>
        private int BinarySearch(int key)
        {
            return Utility.BinarySearch(_keys.AsSpan(), Count, key);
        }
    }
}
