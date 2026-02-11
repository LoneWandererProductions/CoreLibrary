/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:      ExtendedSystemObjects
 * FILE:         ExtendedSystemObjects/ImmutableLookupMap.cs
 * PURPOSE:      A high-performance, immutable lookup map that uses an array-based internal structure for fast key-value lookups.
 * This version is limited to unmanaged types and uses UnmanagedArray<T>.
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
    public sealed unsafe class ImmutableLookupMapUnmanaged<TKey, TValue> : IDisposable, IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        /// <summary>
        ///      Internal entry structure to ensure data locality (Cache-friendly).
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Entry
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
            Entry* entriesPtr = _entries.Pointer;

            foreach (var kvp in data)
            {
                TKey key = kvp.Key;
                TValue value = kvp.Value;
                int hash = GetHash(key) & _mask;
                bool placed = false;

                // 2. Linear Probing for cache line efficiency
                for (var i = 0; i < _capacity; i++)
                {
                    int index = (int)((uint)(hash + i) & (uint)_mask);
                    Entry* entry = entriesPtr + index;

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
            _entries.Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        ///      Returns an enumerator for iterating over the key-value pairs in the map.
        /// </summary>
        /// <remarks>
        ///      Note: We avoid pointers here because yield return cannot exist in an unsafe context 
        ///      that captures pointers. We use the array indexer instead.
        /// </remarks>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (var i = 0; i < _capacity; i++)
            {
                // We use the safe indexer of UnmanagedArray. 
                // This causes a value-copy of the Entry struct, but it's yield-compatible.
                var entry = _entries[i];

                if (entry.IsPresent != 0)
                {
                    yield return new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///      Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///      An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///      Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to lookup.</param>
        /// <returns>The value associated with the key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key is not found in the map.</exception>
        public TValue Get(TKey key)
        {
            int hash = GetHash(key) & _mask;
            Entry* entriesPtr = _entries.Pointer;

            for (var i = 0; i < _capacity; i++)
            {
                int index = (hash + i) & _mask;
                Entry* entry = entriesPtr + index;

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
            int hash = GetHash(key) & _mask;
            Entry* entriesPtr = _entries.Pointer;

            for (var i = 0; i < _capacity; i++)
            {
                int index = (hash + i) & _mask;
                Entry* entry = entriesPtr + index;

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
    }
}
