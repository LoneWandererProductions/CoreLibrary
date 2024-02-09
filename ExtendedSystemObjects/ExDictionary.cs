/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/ExDictionary.cs
 * PURPOSE:     Example Implementation for a Dictionary
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Sources:     https://stackoverflow.com/questions/963068/want-to-create-a-custom-class-of-type-dictionaryint-t/63980746#63980746
 *              https://developerslogblog.wordpress.com/2019/07/30/implementing-ienumerable-and-ienumerator-in-c/       
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ExtendedSystemObjects
{
    /// <inheritdoc />
    /// <summary>
    /// Generic extended Dictionary
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="T:System.Collections.IEnumerable" />
    public sealed class ExDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        /// <summary>
        /// The values
        /// </summary>
        private LinkedList<KeyValuePair<TKey, TValue>>[] _values;

        /// <summary>
        /// The capacity
        /// </summary>
        private int _capacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExDictionary{TKey, TValue}"/> class.
        /// </summary>
        public ExDictionary()
        {
            _values = new LinkedList<KeyValuePair<TKey, TValue>>[15];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="count">The count.</param>
        public ExDictionary(int count)
        {
            _values = new LinkedList<KeyValuePair<TKey, TValue>>[count];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="exDct">The extended Dictionary.</param>
        public ExDictionary(ExDictionary<TKey, TValue> exDct)
        {
            ResizeCollection(exDct.Count);
            _values = exDct._values;
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _values.Length;

        /// <summary>
        /// Gets or sets the <see cref="TValue"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="TValue"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns>Value at Key.</returns>
        /// <exception cref="KeyNotFoundException">Keys not found</exception>
        public TValue this[TKey key]
        {
            get
            {
                var h = GetHashValue(key);
                if (_values[h] == null)
                {
                    throw new KeyNotFoundException("Keys not found");
                }

                return _values[h].FirstOrDefault(p => p.Key.Equals(key)).Value;
            }
            set
            {
                var h = GetHashValue(key);
                _values[h] = new LinkedList<KeyValuePair<TKey, TValue>>();
                _values[h].AddLast(new KeyValuePair<TKey, TValue>
                    (key, value));
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return (from collections in _values
                where collections != null
                from item in collections
                select item).GetEnumerator();
        }

        /// <inheritdoc />
        /// <summary>
        ///     https://developerslogblog.wordpress.com/2019/07/30/implementing-ienumerable-and-ienumerator-in-c/
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The value.</param>
        /// <exception cref="ArgumentException">Duplicate key has been found</exception>
        public void Add(TKey key, TValue val)
        {
            var hash = GetHashValue(key);

            _values[hash] ??= new LinkedList<KeyValuePair<TKey, TValue>>();

            var keyPresent = _values[hash].Any(p => p.Key.Equals(key));

            if (keyPresent)
            {
                throw new ArgumentException("Duplicate key has been found");
            }

            var newValue = new KeyValuePair<TKey, TValue>(key, val);

            _values[hash].AddLast(newValue);
            _capacity++;

            if (Count <= _capacity)
            {
                ResizeCollection();
            }
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="dct">The extended Dictionary.</param>
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> dct)
        {
            var amount = Count + dct.Count() - _capacity;

            if (amount > 0) ResizeCollection(amount);

            foreach (var (key, value) in dct)
            {
                Add(key, value);
            }
        }

        /// <summary>
        /// Resizes the collection.
        /// </summary>
        private void ResizeCollection()
        {
            //add one Element, expensive
            _values = new LinkedList<KeyValuePair<TKey, TValue>>[Count + 1];
        }

        /// <summary>
        /// Resizes the collection.
        /// </summary>
        /// <param name="amount">The amount.</param>
        private void ResizeCollection(int amount)
        {
            //amount of Elements, expensive
            _values = new LinkedList<KeyValuePair<TKey, TValue>>[Count + amount];
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(TKey key)
        {
            var hash = GetHashValue(key);
            return _values[hash] != null && _values[hash].Any(p => p.Key.Equals(key));
        }

        public TValue GetValue(TKey key)
        {
            var hash = GetHashValue(key);
            return _values[hash] == null ? default : _values[hash].First(m => m.Key.Equals(key)).Value;
        }

        public void Clear()
        {
            _values = new LinkedList<KeyValuePair<TKey, TValue>>[15];
        }

        private int GetHashValue(TKey key)
        {
            return (Math.Abs(key.GetHashCode())) % _values.Length;
        }
    }
}
