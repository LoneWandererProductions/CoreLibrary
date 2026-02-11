/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/ImmutableLookupMap.cs
 * PURPOSE:     A high-performance, immutable lookup map that uses an array-based internal structure for fast key-value lookups.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using ExtendedSystemObjects.Helper;

namespace ExtendedSystemObjects
{
    /// <inheritdoc />
    /// <summary>
    ///     A high-performance, immutable lookup map using an array-based internal structure for key-value lookups.
    /// </summary>
    public sealed class ImmutableLookupMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : struct, IEquatable<TKey>
    {
        /// <summary>
        ///     The key presence
        /// </summary>
        private readonly bool[] _keyPresence;

        /// <summary>
        ///     The keys
        /// </summary>
        private readonly TKey[] _keys;

        /// <summary>
        ///     The values
        /// </summary>
        private readonly TValue[] _values;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImmutableLookupMap{TKey, TValue}" /> class.
        ///     Uses Power-of-2 sizing and Linear Probing for O(1) lookups and high cache locality.
        /// </summary>
        /// <param name="data">A dictionary containing the key-value pairs to initialize the map.</param>
        /// <exception cref="ArgumentNullException">data is null.</exception>
        /// <exception cref="InvalidOperationException">Duplicate key detected.</exception>
        public ImmutableLookupMap(IDictionary<TKey, TValue> data)
        {
            ArgumentNullException.ThrowIfNull(data);

            // 1. Calculate capacity as a Power of 2 (at least double the count to keep load factor < 50%)
            int capacity = (int)BitOperations.RoundUpToPowerOf2((uint)data.Count * 2);
            if (capacity < 16) capacity = 16;

            // 2. The mask replaces the modulo operator: (hash % capacity) becomes (hash & mask)
            int mask = capacity - 1;

            // 3. Initialize the internal arrays (Struct of Arrays approach)
            _keys = new TKey[capacity];
            _values = new TValue[capacity];
            _keyPresence = new bool[capacity];

            // 4. Populate the arrays using Linear Probing
            foreach (var kvp in data)
            {
                TKey key = kvp.Key;
                TValue value = kvp.Value;

                // Get initial raw hash
                int hash = GetHash(key);
                bool placed = false;

                for (int i = 0; i < capacity; i++)
                {
                    // The mask here handles everything (sign bit, wrapping, and range)
                    int index = (int)((uint)(hash + i) & (uint)mask);

                    if (!_keyPresence[index])
                    {
                        _keys[index] = key;
                        _values[index] = value;
                        _keyPresence[index] = true;
                        placed = true;
                        break;
                    }

                    // Safety check for duplicate keys (since Dictionary allows them via different refs sometimes)
                    if (_keys[index].Equals(key))
                    {
                        throw new InvalidOperationException(string.Format(SharedResources.ErrorDuplicateKey, key));
                    }
                }

                if (!placed)
                {
                    // This should be mathematically impossible with Linear Probing and Load Factor < 100%
                    throw new InvalidOperationException("Internal map overflow.");
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns an enumerator that iterates through the key-value pairs in the map.
        /// </summary>
        /// <returns>An enumerator for the map.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (var i = 0; i < _keys.Length; i++)
            {
                if (_keyPresence[i])
                {
                    yield return new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to lookup.</param>
        /// <returns>The value associated with the key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key is not found in the map.</exception>
        public TValue Get(TKey key)
        {
            var capacity = _keys.Length;
            var mask = capacity - 1;
            var hash = GetHash(key);

            for (var i = 0; i < capacity; i++)
            {
                var index = (hash + i) & mask;

                // If we hit an empty slot, the key definitely isn't here
                if (!_keyPresence[index]) break;

                if (_keys[index].Equals(key))
                {
                    return _values[index];
                }
            }

            throw new KeyNotFoundException(SharedResources.ErrorValueNotFound);
        }

        /// <summary>
        ///     Attempts to retrieve the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to lookup.</param>
        /// <param name="value">
        ///     When this method returns, contains the value associated with the key, if found; otherwise, the
        ///     default value.
        /// </param>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            var capacity = _keys.Length;
            var mask = capacity - 1;
            var hash = GetHash(key);

            for (var i = 0; i < capacity; i++)
            {
                var index = (hash + i) & mask;

                if (!_keyPresence[index]) break; // Hit an empty slot; key doesn't exist

                if (_keys[index].Equals(key))
                {
                    value = _values[index];
                    return true;
                }
            }

            value = default;
            return false;
        }

        // Helper Methods

        /// <summary>
        /// Gets the hash.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// Hash Value
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetHash(TKey key)
        {
            // Return the raw hash code. Do NOT use Math.Abs.
            return key.GetHashCode();
        }
    }
}
