﻿/*
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
using System.Runtime.CompilerServices;
using ExtendedSystemObjects.Helper;

namespace ExtendedSystemObjects;

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
    /// </summary>
    /// <param name="data">A dictionary containing the key-value pairs to initialize the map.</param>
    /// <exception cref="ArgumentNullException">data</exception>
    public ImmutableLookupMap(IDictionary<TKey, TValue> data)
    {
        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        // Double the capacity and find the next prime number
        var capacity = FindNextPrime(data.Count * 2);

        // Initialize the internal arrays
        _keys = new TKey[capacity];
        _values = new TValue[capacity];
        _keyPresence = new bool[capacity];

        // Populate the arrays with a brute-force quadratic approach
        foreach (var (key, value) in data)
        {
            for (var i = 0; i < capacity; i++)
            {
                var hash = (GetHash(key, capacity) + (i * i)) % capacity; // Quadratic probing formula

                if (!_keyPresence[hash])
                {
                    _keys[hash] = key;
                    _values[hash] = value;
                    _keyPresence[hash] = true;
                    break;
                }

                if (_keys[hash].Equals(key))
                {
                    throw new InvalidOperationException(string.Format(SharedResources.ErrorDuplicateKey, key));
                }
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
        var hash = GetHash(key, _keys.Length);
        var originalHash = hash;

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
        var hash = GetHash(key, _keys.Length);
        var originalHash = hash;

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

    // Helper Methods

    /// <summary>
    ///     Gets the hash.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="capacity">The capacity.</param>
    /// <returns>Hash Value</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetHash(TKey key, int capacity)
    {
        return Math.Abs(key.GetHashCode() % capacity);
    }

    /// <summary>
    ///     Finds the next prime.
    /// </summary>
    /// <param name="number">The number.</param>
    /// <returns>Next prime number</returns>
    private static int FindNextPrime(int number)
    {
        while (!IsPrime(number))
        {
            number++;
        }

        return number;
    }

    /// <summary>
    ///     Determines whether the specified number is prime.
    ///     Uses an internal dictionary for smaller Primes, to speed up the process.
    /// </summary>
    /// <param name="number">The number.</param>
    /// <returns>
    ///     <c>true</c> if the specified number is prime; otherwise, <c>false</c>.
    /// </returns>
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

        for (var i = 49; i * i <= number; i += 2)
        {
            if (number % i == 0)
            {
                return false;
            }
        }

        return true;
    }
}
