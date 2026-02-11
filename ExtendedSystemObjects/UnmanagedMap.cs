/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:      ExtendedSystemObjects
 * FILE:         UnmanagedMap.cs
 * PURPOSE:      A high-performance unmanaged key-value store similar to a Dictionary.
 * Unlike typical dictionaries, entries are marked as deleted (tombstoned)
 * and only physically removed during explicit compaction, improving
 * insertion and deletion performance by avoiding frequent reallocations.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ExtendedSystemObjects.Helper;

namespace ExtendedSystemObjects
{
    /// <summary>
    ///      Represents a high-performance, unmanaged hash map with integer keys and unmanaged values.
    ///      Uses open addressing with linear probing for collision resolution and
    ///      supports lazy deletion (tombstoning) to improve performance on removals.
    /// </summary>
    /// <typeparam name="TValue">The type of values stored, must be unmanaged.</typeparam>
    [DebuggerDisplay("{ToString()}")]
    public sealed unsafe class UnmanagedMap<TValue> : IEnumerable<(int, TValue)>, IDisposable where TValue : unmanaged
    {
        private const int MinPowerOf2 = 4;
        private const int MaxPowerOf2 = 20;

        /// <summary>
        /// The capacity power of2
        /// </summary>
        private int _capacityPowerOf2;

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
        public UnmanagedMap(int? capacityPowerOf2 = null)
        {
            var power = capacityPowerOf2 ?? 8;
            power = Math.Clamp(power, MinPowerOf2, MaxPowerOf2);

            Capacity = 1 << power;
            _capacityPowerOf2 = power;

            _entries = (EntryGeneric<TValue>*)Marshal.AllocHGlobal(sizeof(EntryGeneric<TValue>) * Capacity);
            Unsafe.InitBlock(_entries, 0, (uint)(sizeof(EntryGeneric<TValue>) * Capacity));
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
        /// <returns></returns>
        public TValue this[int key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Free();
            GC.SuppressFinalize(this);
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
            // Trigger Resize or Compact based on total used slots (including tombstones)
            // 0.7 is the standard threshold for linear probing performance health
            if (_usedCount >= Capacity * 0.7f)
            {
                // If tombstones make up more than half the used slots, just Compact
                if (_usedCount - Count > Count) Compact();
                else Resize();
            }

            var mask = Capacity - 1;
            var hashIndex = key & mask;
            var firstTombstone = -1;

            for (var i = 0; i < Capacity; i++)
            {
                var idx = (hashIndex + i) & mask;
                ref var slot = ref _entries[idx];

                if (slot.Used == SharedResources.Empty)
                {
                    // Reclaim first tombstone found in the chain, or use this empty slot
                    int targetIdx = (firstTombstone != -1) ? firstTombstone : idx;

                    ref var target = ref _entries[targetIdx];
                    target.Key = key;
                    target.Value = value;
                    target.Used = SharedResources.Occupied;

                    Count++;
                    // Only increment usedCount if we took a truly empty slot
                    if (firstTombstone == -1) _usedCount++;
                    return;
                }

                if (slot.Used == SharedResources.Tombstone)
                {
                    if (firstTombstone == -1) firstTombstone = idx;
                }
                else if (slot.Key == key)
                {
                    slot.Value = value;
                    return;
                }
            }
        }

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Key {key} not found.</exception>
        public TValue Get(int key)
        {
            var idx = FindIndex(key);
            if (idx >= 0) return _entries[idx].Value;
            throw new KeyNotFoundException($"Key {key} not found.");
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public bool TryGetValue(int key, out TValue value)
        {
            var idx = FindIndex(key);
            if (idx >= 0)
            {
                value = _entries[idx].Value;
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
        /// <returns></returns>
        public bool TryRemove(int key, out TValue value)
        {
            var idx = FindIndex(key);
            if (idx >= 0)
            {
                ref var slot = ref _entries[idx];
                value = slot.Value;
                slot.Used = SharedResources.Tombstone;
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
        /// <returns></returns>
        public bool TryRemove(int key) => TryRemove(key, out _);

        /// <summary>
        /// Finds the index.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int FindIndex(int key)
        {
            var mask = Capacity - 1;
            var startIndex = key & mask;

            for (var i = 0; i < Capacity; i++)
            {
                var idx = (startIndex + i) & mask;
                ref var slot = ref _entries[idx];

                // CRITICAL: We must stop at Empty, but SKIP Tombstones
                if (slot.Used == SharedResources.Empty) return -1;

                if (slot.Used == SharedResources.Occupied && slot.Key == key)
                {
                    return idx;
                }
            }
            return -1;
        }

        /// <summary>
        /// Resizes this instance.
        /// </summary>
        public void Resize()
        {
            if (_capacityPowerOf2 >= MaxPowerOf2) return;
            Rehash(_capacityPowerOf2 + 1);
        }

        /// <summary>
        /// Compacts this instance.
        /// </summary>
        public void Compact()
        {
            // Only shrink if we are well below 25% load factor
            if (Count / (float)Capacity >= 0.25f && _usedCount < Capacity * 0.7f)
            {
                // If we aren't shrinking, we still rehash to same size to clear tombstones
                Rehash(_capacityPowerOf2);
                return;
            }

            var targetPower = Math.Max(MinPowerOf2, _capacityPowerOf2 - 1);
            Rehash(targetPower);
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

            _entries = (EntryGeneric<TValue>*)Marshal.AllocHGlobal(sizeof(EntryGeneric<TValue>) * Capacity);
            Unsafe.InitBlock(_entries, 0, (uint)(sizeof(EntryGeneric<TValue>) * Capacity));

            int oldCount = Count;
            Count = 0;
            _usedCount = 0;

            for (var i = 0; i < oldCapacity; i++)
            {
                if (oldEntries[i].Used == SharedResources.Occupied)
                {
                    Set(oldEntries[i].Key, oldEntries[i].Value);
                }
            }

            Marshal.FreeHGlobal((IntPtr)oldEntries);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            if (_entries != null)
            {
                Unsafe.InitBlock(_entries, 0, (uint)(sizeof(EntryGeneric<TValue>) * Capacity));
                Count = 0;
                _usedCount = 0;
            }
        }

        /// <summary>
        /// Frees this instance.
        /// </summary>
        public void Free()
        {
            if (_entries != null)
            {
                Marshal.FreeHGlobal((IntPtr)_entries);
                _entries = null;
            }
            Capacity = 0;
            Count = 0;
            _usedCount = 0;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public EntryGenericEnumerator<TValue> GetEnumerator() => new(_entries, Capacity);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        IEnumerator<(int, TValue)> IEnumerable<(int, TValue)>.GetEnumerator() => GetEnumerator();

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
        public override string ToString() => $"Count = {Count}, Capacity = {Capacity}, LoadFactor = {Count / (float)Capacity:P1}";

        /// <summary>
        /// Gets the keys snapshot.
        /// </summary>
        /// <returns></returns>
        private List<int> GetKeysSnapshot()
        {
            var keys = new List<int>(Count);
            for (var i = 0; i < Capacity; i++)
            {
                if (_entries[i].Used == SharedResources.Occupied) keys.Add(_entries[i].Key);
            }
            return keys;
        }

        ~UnmanagedMap() => Free();
    }
}
