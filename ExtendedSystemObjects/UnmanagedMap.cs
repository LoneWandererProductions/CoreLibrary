/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        UnmanagedMap.cs
 * PURPOSE:     A high-performance unmanaged key-value store similar to a Dictionary.
 *              Unlike typical dictionaries, entries are marked as deleted (tombstoned)
 *              and only physically removed during explicit compaction, improving
 *              insertion and deletion performance by avoiding frequent reallocations.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable OutParameterValueIsAlwaysDiscarded.Global

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ExtendedSystemObjects.Helper;

namespace ExtendedSystemObjects
{
    /// <inheritdoc cref="IEnumerable" />
    /// <summary>
    /// Represents a high-performance, unmanaged hash map with integer keys and unmanaged values.
    /// Uses open addressing with linear probing for collision resolution and
    /// supports lazy deletion (tombstoning) to improve performance on removals.
    /// </summary>
    /// <typeparam name="TValue">The type of values stored, must be unmanaged.</typeparam>
    /// <seealso cref="!:System.Collections.Generic.IEnumerable&lt;(System.Int32, TValue)&gt;" />
    /// <seealso cref="T:System.IDisposable" />
    [DebuggerDisplay("{ToString()}")]
    public sealed unsafe class UnmanagedMap<TValue> : IEnumerable<(int, TValue)>, IDisposable where TValue : unmanaged
    {
        /// <summary>
        /// The minimum power of2
        /// </summary>
        private const int MinPowerOf2 = 4;

        /// <summary>
        /// The maximum power of2
        /// </summary>
        private const int MaxPowerOf2 = 20;

        /// <summary>
        /// The capacity power of2
        /// </summary>
        private int _capacityPowerOf2;

        /// <summary>
        /// The resize threshold
        /// </summary>
        private int _resizeThreshold;

        /// <summary>
        /// The entries
        /// </summary>
        private EntryGeneric<TValue>* _entries;

        // Track total slots that aren't 'Empty' (Occupied + Tombstones)
        // This is the true 'Load Factor' for linear probing performance.
        private int _usedCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnmanagedMap{TValue}"/> class.
        /// </summary>
        /// <param name="capacityPowerOf2">The capacity power of2.</param>
        public UnmanagedMap(int capacityPowerOf2 = 8)
        {
            _capacityPowerOf2 = Math.Clamp(capacityPowerOf2, MinPowerOf2, MaxPowerOf2);
            Capacity = 1 << _capacityPowerOf2;
            _resizeThreshold = (int)(Capacity * 0.7f);

            // NativeMemory.AllocZeroed automatically handles the byte count
            // and clears the memory in one optimized call.
            _entries = (EntryGeneric<TValue>*)NativeMemory.AllocZeroed((nuint)Capacity,
                (nuint)sizeof(EntryGeneric<TValue>));

            if (_entries == null)
            {
                throw new OutOfMemoryException("Failed to allocate unmanaged memory for UnmanagedMap.");
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the capacity.
        /// </summary>
        /// <value>
        /// The capacity.
        /// </value>
        public int Capacity { get; private set; }

        /// <summary>
        /// The disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <value>
        /// The keys.
        /// </value>
        public IEnumerable<int> Keys => GetKeysSnapshot();

        /// <summary>
        /// Gets or sets the <see cref="TValue"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="TValue"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>Value at Key</returns>
        public TValue this[int key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        /// <summary>
        ///     Gets the values.
        /// </summary>
        /// <value>
        ///     The values.
        /// </value>
        public IEnumerable<TValue> Values
        {
            get
            {
                return GetValuesSnapshot();
            }
        }

        /// <summary>
        /// Gets the values snapshot.
        /// </summary>
        /// <returns>List of Values</returns>
        private List<TValue> GetValuesSnapshot()
        {
            var values = new List<TValue>(Count);
            for (var i = 0; i < Capacity; i++)
            {
                var entry = _entries[i];
                if (entry.used == SharedResources.Occupied)
                {
                    values.Add(entry.value);
                }
            }

            return values;
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;
            Free();
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(int key) => FindIndex(key) >= 0;

        /// <summary>
        /// Sets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int key, TValue value)
        {
            // Trigger Resize or Compact based on threshold
            if (_usedCount >= _resizeThreshold)
            {
                if (_usedCount - Count > Count) Compact();
                else Resize();
            }

            // Use a mixing function to prevent clustering
            var mask = Capacity - 1;
            var hashIndex = GenerateHash(key, mask);
            var firstTombstone = -1;

            for (var i = 0; i < Capacity; i++)
            {
                // linear probing
                var idx = (hashIndex + i) & mask;

                ref var slot = ref _entries[idx];

                if (slot.used == SharedResources.Empty)
                {
                    // We hit an empty slot. If we found a tombstone earlier, use that instead.
                    var targetIdx = (firstTombstone != -1) ? firstTombstone : idx;

                    ref var target = ref _entries[targetIdx];
                    target.key = key;
                    target.value = value;
                    target.used = SharedResources.Occupied;

                    Count++;
                    if (firstTombstone == -1) _usedCount++;
                    return;
                }

                if (slot.used == SharedResources.Tombstone)
                {
                    if (firstTombstone == -1) firstTombstone = idx;
                }
                else if (slot.key == key)
                {
                    // Key already exists, update and exit
                    slot.value = value;
                    return;
                }
            }

            // Fallback: If we scanned the whole table and didn't hit Empty,
            // but we found a tombstone, we can safely insert there.
            if (firstTombstone != -1)
            {
                ref var target = ref _entries[firstTombstone];
                target.key = key;
                target.value = value;
                target.used = SharedResources.Occupied;
                Count++;
                return;
            }

            throw new InvalidOperationException("Map is completely full and cannot accept new keys.");
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Value associated with the specified key.</returns>
        /// <exception cref="KeyNotFoundException">Key {key} not found.</exception>
        public TValue Get(int key)
        {
            var idx = FindIndex(key);
            if (idx >= 0) return _entries[idx].value;

            throw new KeyNotFoundException($"Key {key} not found.");
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>Gets Value by Kex and returns if the kex exists or not.</returns>
        public bool TryGetValue(int key, out TValue value)
        {
            var idx = FindIndex(key);
            if (idx != -1) // -1 is the failure case
            {
                value = _entries[idx].value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Tries the remove.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>Success Status if Element was removed</returns>
        public bool TryRemove(int key, out TValue value)
        {
            var idx = FindIndex(key);
            if (idx >= 0)
            {
                ref var slot = ref _entries[idx];
                value = slot.value;
                slot.used = SharedResources.Tombstone;
                Count--;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Tries the remove.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Success Status if Element was removed</returns>
        public bool TryRemove(int key) => TryRemove(key, out _);

        /// <summary>
        /// Finds the index.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Index of Key.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindIndex(int key)
        {
            var mask = Capacity - 1;
            var startIndex = GenerateHash(key, mask);

            for (var i = 0; i < Capacity; i++)
            {
                // linear probing
                var idx = (startIndex + i) & mask;

                ref var slot = ref _entries[idx];

                // CRITICAL: We must stop at Empty, but SKIP Tombstones
                if (slot.used == SharedResources.Empty) return -1;

                if (slot.used == SharedResources.Occupied && slot.key == key)
                {
                    return idx;
                }
            }

            return -1;
        }

        /// <summary>
        /// Resizes this instance.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the map has reached its maximum configured capacity.</exception>
        public void Resize()
        {
            if (_capacityPowerOf2 >= MaxPowerOf2)
            {
                throw new InvalidOperationException(
                    $"The map has reached its maximum capacity of {1 << MaxPowerOf2} entries.");
            }

            Rehash(_capacityPowerOf2 + 1);
        }

        /// <summary>
        /// Compacts this instance.
        /// </summary>
        public void Compact()
        {
            // Only shrink if we are well below 25% load factor
            if (Count / (float)Capacity < 0.25f)
            {
                var targetPower = Math.Max(MinPowerOf2, _capacityPowerOf2 - 1);
                Rehash(targetPower); // genuinely sparse, shrink
                return;
            }

            Rehash(_capacityPowerOf2); // healthy load, just clear tombstones
        }

        /// <summary>
        /// Rehashes the specified new power of2.
        /// </summary>
        /// <param name="newPowerOf2">The new power of2.</param>
        private void Rehash(int newPowerOf2)
        {
            var oldEntries = _entries;
            var oldCapacity = Capacity;

            _capacityPowerOf2 = newPowerOf2;
            Capacity = 1 << _capacityPowerOf2;
            _resizeThreshold = (int)(Capacity * 0.7f);

            // Use NativeMemory to allocate zeroed-out memory efficiently
            _entries = (EntryGeneric<TValue>*)NativeMemory.AllocZeroed((nuint)Capacity,
                (nuint)sizeof(EntryGeneric<TValue>));

            Count = 0;
            _usedCount = 0;

            try
            {
                for (var i = 0; i < oldCapacity; i++)
                {
                    if (oldEntries[i].used == SharedResources.Occupied)
                    {
                        // Optimization: Directly insert into the new table.
                        // We bypass Set() because we don't need to check load factors,
                        // and we can assume no duplicates existed in the old table.
                        InsertRaw(oldEntries[i].key, oldEntries[i].value);
                    }
                }
            }
            finally
            {
                NativeMemory.Free(oldEntries);
            }
        }

        /// <summary>
        /// Inserts the raw.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private void InsertRaw(int key, TValue value)
        {
            var mask = Capacity - 1;
            var hashIndex = GenerateHash(key, mask);

            for (var i = 0; i < Capacity; i++)
            {
                // linear probing
                var idx = (hashIndex + i) & mask;

                if (_entries[idx].used == SharedResources.Empty)
                {
                    _entries[idx].key = key;
                    _entries[idx].value = value;
                    _entries[idx].used = SharedResources.Occupied;
                    Count++;
                    _usedCount++;
                    return;
                }
            }

            throw new InvalidOperationException("InsertRaw failed during rehash.");
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            if (_entries == null)
            {
                return;
            }

            NativeMemory.Clear(_entries, (nuint)Capacity * (nuint)sizeof(EntryGeneric<TValue>));
            Count = 0;
            _usedCount = 0;
        }

        /// <summary>
        /// Frees this instance.
        /// </summary>
        public void Free()
        {
            if (_entries != null)
            {
                NativeMemory.Free(_entries);
                _entries = null;
            }

            Capacity = 0;
            Count = 0;
            _usedCount = 0;
        }

        /// <summary>
        /// Gets the keys snapshot.
        /// </summary>
        /// <returns>List of all available Keys.</returns>
        private List<int> GetKeysSnapshot()
        {
            var keys = new List<int>(Count);
            for (var i = 0; i < Capacity; i++)
            {
                if (_entries[i].used == SharedResources.Occupied) keys.Add(_entries[i].key);
            }

            return keys;
        }

        /// <summary>
        /// Generatehashes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="mask">The mask.</param>
        /// <returns>Returns the generated hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GenerateHash(int key, int mask)
        {
            uint h = (uint)key;
            h ^= h >> 16;
            h *= 0x45d9f3b;
            h ^= h >> 16;
            return (int)(h & (uint)mask);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public EntryGenericEnumerator<TValue> GetEnumerator() => new(_entries, Capacity);

        /// <inheritdoc />
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<(int, TValue)> IEnumerable<(int, TValue)>.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() =>
            $"Count = {Count}, Capacity = {Capacity}, LoadFactor = {_usedCount / (float)Capacity:P1}";

        /// <summary>
        /// Finalizes an instance of the <see cref="UnmanagedMap{TValue}"/> class.
        /// </summary>
        ~UnmanagedMap() => Free();
    }
}
