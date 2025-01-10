/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/ImmutableLookupMap.cs
 * PURPOSE:     A high-performance, immutable lookup map that uses an array-based internal structure for fast key-value lookups.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace ExtendedSystemObjects
{
    /// <summary>
    /// A high-performance, immutable lookup map using an array-based internal structure for key-value lookups.
    /// </summary>
    public sealed class ImmutableLookupMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : struct, IEquatable<TKey>
    {
        private readonly TKey[] _keys;
        private readonly TValue[] _values;
        private readonly bool[] _keyPresence;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableLookupMap{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="data">A dictionary containing the key-value pairs to initialize the map.</param>
        public ImmutableLookupMap(IDictionary<TKey, TValue> data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            int capacity = FindNextPrime(data.Count * 2); // Use prime number capacity
            _keys = new TKey[capacity];
            _values = new TValue[capacity];
            _keyPresence = new bool[capacity];

            foreach (var kvp in data)
            {
                Add(kvp.Key, kvp.Value, capacity);
            }
        }

        /// <summary>
        /// Retrieves the value associated with the specified key.
        /// </summary>
        public TValue Get(TKey key)
        {
            int hash = GetHash(key, _keys.Length);
            int originalHash = hash;

            while (_keyPresence[hash])
            {
                if (_keys[hash].Equals(key))
                {
                    return _values[hash];
                }

                hash = (hash + 1) % _keys.Length; // Linear probing
                if (hash == originalHash)
                {
                    break; // Full cycle, key not found
                }
            }

            throw new KeyNotFoundException($"The key {key} was not found in the lookup map.");
        }

        /// <summary>
        /// Attempts to retrieve the value associated with the specified key.
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            int hash = GetHash(key, _keys.Length);
            int originalHash = hash;

            while (_keyPresence[hash])
            {
                if (_keys[hash].Equals(key))
                {
                    value = _values[hash];
                    return true;
                }

                hash = (hash + 1) % _keys.Length; // Linear probing
                if (hash == originalHash)
                {
                    break; // Full cycle, key not found
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Returns an enumerator for iterating over the key-value pairs in the map.
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < _keys.Length; i++)
            {
                if (_keyPresence[i])
                {
                    yield return new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // Helper Methods
        private void Add(TKey key, TValue value, int capacity)
        {
            int hash = GetHash(key, capacity);

            while (_keyPresence[hash])
            {
                if (_keys[hash].Equals(key))
                {
                    throw new InvalidOperationException($"Duplicate key detected: {key}");
                }

                hash = (hash + 1) % capacity; // Linear probing
            }

            _keys[hash] = key;
            _values[hash] = value;
            _keyPresence[hash] = true;
        }

        private static int GetHash(TKey key, int capacity)
        {
            return Math.Abs(key.GetHashCode() % capacity);
        }

        private static int FindNextPrime(int number)
        {
            while (!IsPrime(number))
            {
                number++;
            }

            return number;
        }

        private static bool IsPrime(int number)
        {
            if (number < 2) return false;
            if (number == 2 || number == 3) return true;
            if (number % 2 == 0 || number % 3 == 0) return false;

            for (int i = 5; i * i <= number; i += 6)
            {
                if (number % i == 0 || number % (i + 2) == 0) return false;
            }

            return true;
        }
    }
}
