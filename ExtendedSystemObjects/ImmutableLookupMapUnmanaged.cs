/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/ImmutableLookupMap.cs
 * PURPOSE:     A high-performance, immutable lookup map that uses an array-based internal structure for fast key-value lookups.
 *              This version is limited to unmanaged types and uses UnmanagedArray<T>.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ExtendedSystemObjects.Helper;

namespace ExtendedSystemObjects
{
    /// <inheritdoc cref="IDisposable" />
    /// <summary>
    ///     A high-performance, immutable lookup map using unmanaged arrays.
    ///     Suitable for value types only. Keys must be unique.
    /// </summary>
    public sealed class ImmutableLookupMapUnmanaged<TKey, TValue> : IDisposable, IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        /// <summary>
        ///     The capacity
        /// </summary>
        private readonly int _capacity;

        /// <summary>
        ///     Indicates whether a key is present at the given hash slot.
        /// </summary>
        private readonly UnmanagedArray<byte> _keyPresence;

        /// <summary>
        ///     The keys
        /// </summary>
        private readonly UnmanagedArray<TKey> _keys;

        /// <summary>
        ///     The values
        /// </summary>
        private readonly UnmanagedArray<TValue> _values;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImmutableLookupMapUnmanaged{TKey, TValue}" /> class
        ///     with the specified key-value data.
        /// </summary>
        /// <param name="data">A dictionary containing the initial key-value pairs.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="data" /> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a duplicate key is found.</exception>
        public ImmutableLookupMapUnmanaged(IDictionary<TKey, TValue> data)
        {
            ArgumentNullException.ThrowIfNull(data);

            Count = data.Count;
            _capacity = FindNextPrime(Count * 2);

            _keys = new UnmanagedArray<TKey>(_capacity);
            _values = new UnmanagedArray<TValue>(_capacity);
            _keyPresence = new UnmanagedArray<byte>(_capacity);

            foreach (var (key, value) in data)
            {
                for (var i = 0; i < _capacity; i++)
                {
                    var hash = (GetHash(key, _capacity) + (i * i)) % _capacity;

                    if (_keyPresence[hash] == 0)
                    {
                        _keys[hash] = key;
                        _values[hash] = value;
                        _keyPresence[hash] = 1;
                        break;
                    }

                    if (_keys[hash].Equals(key))
                    {
                        throw new InvalidOperationException(string.Format(SharedResources.ErrorDuplicateKey, key));
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the number of entries in the map.
        /// </summary>
        public int Count { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Releases the unmanaged memory used by the lookup map.
        /// </summary>
        public void Dispose()
        {
            _keys.Dispose();
            _values.Dispose();
            _keyPresence.Dispose();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Returns an enumerator for iterating over the key-value pairs in the map.
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (var i = 0; i < _capacity; i++)
            {
                if (_keyPresence[i] != 0)
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
            var hash = GetHash(key, _capacity);
            var originalHash = hash;

            while (_keyPresence[hash] != 0)
            {
                if (_keys[hash].Equals(key))
                {
                    return _values[hash];
                }

                hash = (hash + 1) % _capacity;
                if (hash == originalHash)
                {
                    break;
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
            var hash = GetHash(key, _capacity);
            var originalHash = hash;

            while (_keyPresence[hash] != 0)
            {
                if (_keys[hash].Equals(key))
                {
                    value = _values[hash];
                    return true;
                }

                hash = (hash + 1) % _capacity;
                if (hash == originalHash)
                {
                    break;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Computes the hash code of a key and reduces it to fit the map's capacity.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="capacity">The internal array size.</param>
        /// <returns>A non-negative integer hash.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetHash(TKey key, int capacity)
        {
            return Math.Abs(key.GetHashCode() % capacity);
        }

        /// <summary>
        ///     Finds the next prime number greater than or equal to the specified number.
        /// </summary>
        /// <param name="number">The minimum value.</param>
        /// <returns>The next prime number ≥ <paramref name="number" />.</returns>
        private static int FindNextPrime(int number)
        {
            while (!IsPrime(number))
            {
                number++;
            }

            return number;
        }

        /// <summary>
        ///     Determines whether a number is prime.
        ///     Uses precomputed small primes for optimization.
        /// </summary>
        /// <param name="number">The number to check.</param>
        /// <returns><c>true</c> if prime; otherwise, <c>false</c>.</returns>
        private static bool IsPrime(int number)
        {
            if (number < 2)
            {
                return false;
            }

            foreach (var prime in SharedResources.SmallPrimes)
            {
                if (number == prime)
                {
                    return true;
                }

                if (number % prime == 0)
                {
                    return false;
                }
            }

            for (var i = 201; i * i <= number; i += 2)
            {
                if (number % i == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
