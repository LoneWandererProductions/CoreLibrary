/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/ImmutableLookupMap.cs
 * PURPOSE:     A high-performance, immutable lookup map that uses an array-based internal structure for fast key-value lookups.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;

namespace ExtendedSystemObjects
{
    /// <summary>
    /// A high-performance, immutable lookup map that uses an array-based internal structure for fast key-value lookups.
    /// Only types implementing <see cref="IEquatable{TKey}"/> and overriding <see cref="GetHashCode"/> correctly should be used as keys.
    /// </summary>
    /// <typeparam name="TKey">The type of the key, which must be a value type (struct) and implement <see cref="IEquatable{TKey}"/> and <see cref="GetHashCode"/>.</typeparam>
    /// <typeparam name="TValue">The type of the value stored in the lookup map.</typeparam>
    public sealed class ImmutableLookupMap<TKey, TValue> where TKey : struct
    {
        private readonly TValue[] _lookupTable;
        private readonly int _size;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableLookupMap{TKey, TValue}"/> class.
        /// The key type <typeparamref name="TKey"/> must implement <see cref="IEquatable{TKey}"/> and override <see cref="GetHashCode"/> correctly.
        /// This is crucial for ensuring proper hashing and equality comparisons.
        /// </summary>
        /// <param name="data">A dictionary containing the key-value pairs to initialize the map.</param>
        /// <exception cref="ArgumentException">Thrown when the key type does not implement <see cref="IEquatable{TKey}"/> or override <see cref="GetHashCode"/>.</exception>
        public ImmutableLookupMap(IDictionary<TKey, TValue> data)
        {
            if (!typeof(TKey).IsValueType)
            {
                throw new ArgumentException("Key type must be a value type (struct).", nameof(TKey));
            }

            _size = data.Count;
            _lookupTable = new TValue[_size];

            var index = 0;

            foreach (var kvp in data)
            {
                // Convert key to an integer-like hash for array access.
                var hash = Convert.ToInt32(kvp.Key);  // Ensure TKey is a struct or integer-like type
                _lookupTable[hash] = kvp.Value;
            }
        }

        /// <summary>
        /// Retrieves the value associated with the specified key.
        /// Assumes the key is valid and present in the map.
        /// </summary>
        /// <param name="key">The key for which the value is to be retrieved.</param>
        /// <returns>The value associated with the specified key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key is not found in the map.</exception>
        public TValue Get(TKey key)
        {
            var hash = Convert.ToInt32(key);  // Convert key to integer for direct array access
            if (hash < 0 || hash >= _lookupTable.Length)
            {
                throw new KeyNotFoundException($"The key {key} was not found in the lookup map.");
            }
            return _lookupTable[hash];
        }

        /// <summary>
        /// Attempts to retrieve the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key for which the value is to be retrieved.</param>
        /// <param name="value">The value associated with the specified key, or the default value if the key is not found.</param>
        /// <returns>True if the key is found; otherwise, false.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            var hash = Convert.ToInt32(key);
            if (hash >= 0 && hash < _lookupTable.Length && _lookupTable[hash] != null)
            {
                value = _lookupTable[hash];
                return true;
            }

            value = default;
            return false;
        }
    }
}
