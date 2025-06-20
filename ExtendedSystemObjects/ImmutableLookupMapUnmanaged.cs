/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/ImmutableLookupMap.cs
 * PURPOSE:     A high-performance, immutable lookup map that uses an array-based internal structure for fast key-value lookups. Tis one is for unmanaged only. It uses my UnmanagedArray.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
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
    /// <summary>
    /// A high-performance, immutable lookup map using unmanaged arrays.
    /// </summary>
    public sealed unsafe class ImmutableLookupMapUnmanaged<TKey, TValue> : IDisposable, IEnumerable<KeyValuePair<TKey, TValue>>
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        private readonly UnmanagedArray<TKey> _keys;
        private readonly UnmanagedArray<TValue> _values;
        private readonly UnmanagedArray<byte> _keyPresence;

        private readonly int _capacity;

        public int Count { get; }

        public ImmutableLookupMapUnmanaged(IDictionary<TKey, TValue> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Count = data.Count;
            _capacity = FindNextPrime(Count * 2);

            _keys = new UnmanagedArray<TKey>(_capacity);
            _values = new UnmanagedArray<TValue>(_capacity);
            _keyPresence = new UnmanagedArray<byte>(_capacity);

            foreach (var (key, value) in data)
            {
                for (var i = 0; i < _capacity; i++)
                {
                    var hash = (GetHash(key, _capacity) + i * i) % _capacity;

                    if (_keyPresence[hash] == 0)
                    {
                        _keys[hash] = key;
                        _values[hash] = value;
                        _keyPresence[hash] = 1;
                        break;
                    }

                    if (_keys[hash].Equals(key))
                        throw new InvalidOperationException($"Duplicate key detected: {key}");
                }
            }
        }

        public TValue Get(TKey key)
        {
            var hash = GetHash(key, _capacity);
            var originalHash = hash;

            while (_keyPresence[hash] != 0)
            {
                if (_keys[hash].Equals(key))
                    return _values[hash];

                hash = (hash + 1) % _capacity;
                if (hash == originalHash)
                    break;
            }

            throw new KeyNotFoundException($"Key '{key}' not found in lookup.");
        }

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
                    break;
            }

            value = default;
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (var i = 0; i < _capacity; i++)
            {
                if (_keyPresence[i] != 0)
                    yield return new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            _keys.Dispose();
            _values.Dispose();
            _keyPresence.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetHash(TKey key, int capacity)
        {
            return Math.Abs(key.GetHashCode() % capacity);
        }

        private static int FindNextPrime(int number)
        {
            while (!IsPrime(number))
                number++;
            return number;
        }

        private static bool IsPrime(int number)
        {
            if (number < 2) return false;

            foreach (var prime in SharedResources.SmallPrimes)
            {
                if (number == prime) return true;
                if (number % prime == 0) return false;
            }

            for (var i = 201; i * i <= number; i += 2)
                if (number % i == 0)
                    return false;

            return true;
        }
    }
}
