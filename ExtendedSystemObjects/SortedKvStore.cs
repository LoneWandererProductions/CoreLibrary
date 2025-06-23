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
using System.Diagnostics;

namespace ExtendedSystemObjects
{
    /// <inheritdoc cref="IDisposable" />
    /// <summary>
    ///     Represents a sorted key-value store with integer keys and integer values.
    ///     Keys are kept sorted internally to allow efficient binary search operations.
    ///     The class supports insertion, removal, and lookup operations with dynamic storage growth.
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    [DebuggerDisplay("{ToString()}")]
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
        /// The active indices cache
        /// </summary>
        private UnmanagedIntList _activeIndicesCache;

        /// <summary>
        /// The active indices dirty
        /// </summary>
        private bool _activeIndicesDirty = true;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SortedKvStore" /> class with a specified initial capacity.
        /// </summary>
        /// <param name="initialCapacity">The initial capacity of the store.</param>
        public SortedKvStore(int initialCapacity = 16)
        {
            _keys = new UnmanagedIntArray(initialCapacity);
            _values = new UnmanagedIntArray(initialCapacity);
            _occupied = new UnmanagedIntArray(initialCapacity);
            _activeIndicesCache = new UnmanagedIntList(initialCapacity);
        }

        /// <summary>
        ///     Gets the number of active (occupied) key-value pairs stored.
        /// </summary>
        public int OccupiedCount { get; private set; }

        /// <summary>
        ///     Gets the free capacity.
        /// </summary>
        /// <value>
        ///     The free capacity.
        /// </value>
        public int FreeCapacity => _keys.Capacity - OccupiedCount;

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
                for (var i = 0; i < OccupiedCount; i++)
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
                if (TryGetValue(key, out var value))
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
            _activeIndicesCache.Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the enumerator.
        /// </summary>
        /// <returns>All active elements as Key Value pair.</returns>
        public IEnumerator<KeyValuePair<int, int>> GetEnumerator()
        {
            foreach (var idx in GetActiveIndices())
            {
                yield return new KeyValuePair<int, int>(_keys[idx], _values[idx]);
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
            _activeIndicesDirty = true;

            if (idx >= 0)
            {
                if (_occupied[idx] == 0)
                {
                    _occupied[idx] = 1;
                    _values[idx] = value;
                    OccupiedCount++;
                }
                else
                {
                    _values[idx] = value; // update
                }


                return;
            }

            idx = ~idx;

            // Insert at physical index, so check raw capacity not OccupiedCount
            EnsureCapacity();

            _keys.InsertAt(idx, key);
            _values.InsertAt(idx, value);
            _occupied.InsertAt(idx, 1);

            OccupiedCount++;
        }

        /// <summary>
        /// Determines whether the specified key is contained in the data structure.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(int key)
        {
            var idx = BinarySearch(key);
            return idx >= 0 && _occupied[idx] != 0;
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
        public bool TryGetValue(int key, out int value)
        {
            int left = 0, right = OccupiedCount - 1;
            var keysSpan = _keys.AsSpan()[.._keys.Length];
            var occupiedSpan = _occupied.AsSpan()[..OccupiedCount];
            var valuesSpan = _values.AsSpan()[..OccupiedCount];

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
            if (idx < 0 || _occupied[idx] == 0)
            {
                return;
            }

            _occupied[idx] = 0;
            _activeIndicesDirty = true;
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
                _activeIndicesDirty = true;
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

            var occSpan = _occupied.AsSpan()[..OccupiedCount];
            var keysSpan = _keys.AsSpan()[..OccupiedCount];

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
                while (i < OccupiedCount && j < keysToRemove.Length)
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
                        _activeIndicesDirty = true;
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
                        _activeIndicesDirty = true;
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
            var occSpan = _occupied.AsSpan()[..OccupiedCount];
            var maxRemoved = OccupiedCount; // worst case: all are removed

            var rented = ArrayPool<int>.Shared.Rent(maxRemoved);
            var removedCount = 0;

            for (var i = 0; i < OccupiedCount; i++)
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

            OccupiedCount -= removedCount;

            ArrayPool<int>.Shared.Return(rented);
        }

        /// <summary>
        /// Convert to span.
        /// </summary>
        /// <returns>Span of active Keys.</returns>
        public Span<int> AsSpan()
        {
            using var list = GetActiveKeys();
            return list.AsSpan();
        }

        /// <summary>
        ///     Performs a binary search for the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>
        ///     The index of the key if found; otherwise, the bitwise complement of the index at which the key should be inserted.
        /// </returns>
        public int BinarySearch(int key)
        {
            int left = 0;
            int right = OccupiedCount - 1;
            var keysSpan = _keys.AsSpan()[..OccupiedCount];
            var occupiedSpan = _occupied.AsSpan()[..OccupiedCount];

            while (left <= right)
            {
                int mid = left + ((right - left) >> 1);
                int midKey = keysSpan[mid];

                if (midKey == key)
                {
                    if (occupiedSpan[mid] != 0)
                        return mid; // Found active key

                    // If key found but not occupied, probe neighbors
                    // probe left
                    for (int i = mid - 1; i >= left && keysSpan[i] == key; i--)
                    {
                        if (occupiedSpan[i] != 0)
                            return i;
                    }

                    // probe right
                    for (int i = mid + 1; i <= right && keysSpan[i] == key; i++)
                    {
                        if (occupiedSpan[i] != 0)
                            return i;
                    }

                    // No active key found
                    return ~left;
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

            return ~left; // Not found, return bitwise complement of insertion point
        }


        /// <summary>
        ///     Removes all entries from the store.
        /// </summary>
        public void Clear()
        {
            _keys.Clear();
            _values.Clear();
            _occupied.Clear();
            _activeIndicesCache.Clear();
            _activeIndicesDirty = true;
            OccupiedCount = 0;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Count: {OccupiedCount}");

            for (int i = 0; i < OccupiedCount; i++)
            {
                sb.AppendLine($"Index {i}: Key={_keys[i]}, Value={_values[i]}, Occupied={_occupied[i]}");
            }

            return sb.ToString();
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
            _keys.EnsureCapacity(OccupiedCount + 1);
            _values.EnsureCapacity(OccupiedCount + 1);
            _occupied.EnsureCapacity(OccupiedCount + 1);
        }

        /// <summary>
        /// Gets the active indices.
        /// </summary>
        /// <returns>UnmanagedIntArray of Active Keys</returns>
        private UnmanagedIntList GetActiveIndices()
        {
            if (!_activeIndicesDirty && _activeIndicesCache != null)
            {
                return _activeIndicesCache!;
            }

            GenerateActiveIndices();
            _activeIndicesDirty = false;

            return _activeIndicesCache!;
        }

        /// <summary>
        /// Gets the active keys.
        /// </summary>
        /// <returns>UnmanagedIntArray of Active Keys</returns>
        public UnmanagedIntArray GetActiveKeys()
        {
            var activeIndices = GetActiveIndices();
            var keys = new UnmanagedIntArray(activeIndices.Length);
            for (int i = 0; i < activeIndices.Length; i++)
            {
                keys[i] = _keys[activeIndices[i]];
            }

            return keys;
        }

        /// <summary>
        /// Generates the active indices.
        /// </summary>
        private void GenerateActiveIndices()
        {
            _activeIndicesCache?.Dispose();
            _activeIndicesCache = new UnmanagedIntList(OccupiedCount);

            var occ = _occupied.AsSpan()[..OccupiedCount];
            for (int i = 0; i < occ.Length; i++)
            {
                if (occ[i] != 0)
                {
                    _activeIndicesCache.Add(i);
                }
            }
        }
    }
}
