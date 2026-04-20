/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:      ExtendedSystemObjects
 * FILE:         BiMap.cs
 * PURPOSE:      Bi-directional map implementation that allows for efficient lookups in both directions.
 * PROGRAMER:    Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace ExtendedSystemObjects
{
    /// <summary>
    /// BiMap is a bi-directional map implementation that allows for efficient lookups in both directions. 
    /// It maintains two internal dictionaries to store the forward and reverse mappings, ensuring that each key-value pair is unique across both sides. 
    /// The class is thread-safe, allowing for concurrent access and modifications without risking data corruption. 
    /// It implements IReadOnlyCollection to provide enumeration capabilities over the key-value pairs.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the BiMap.</typeparam>
    /// <seealso cref="System.Collections.Generic.IReadOnlyCollection&lt;System.Collections.Generic.KeyValuePair&lt;T, T&gt;&gt;" />
    public class BiMap<T> : IReadOnlyCollection<KeyValuePair<T, T>> where T : notnull
    {
        /// <summary>
        /// The forward
        /// </summary>
        private readonly Dictionary<T, T> _forward = new();

        /// <summary>
        /// The reverse
        /// </summary>
        private readonly Dictionary<T, T> _reverse = new();

        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count => _forward.Count;

        /// <summary>
        /// The lock
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<T, T>> GetEnumerator()
        {
            // We take a snapshot to ensure thread-safety during iteration
            Dictionary<T, T> snapshot;
            lock (_lock)
            {
                snapshot = new Dictionary<T, T>(_forward);
            }
            return snapshot.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Initializes a new instance of the <see cref="BiMap{T}"/> class.
        /// </summary>
        public BiMap() : this(0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiMap{T}"/> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public BiMap(int size)
        {
            _forward = new Dictionary<T, T>(size);
            _reverse = new Dictionary<T, T>(size);
        }

        /// <summary>
        /// Adds the specified left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <exception cref="System.ArgumentException">Duplicate detected. Both values must be unique across their respective sides.</exception>
        public void Add(T left, T right)
        {
            lock (_lock)
            {
                if (_forward.ContainsKey(left) || _reverse.ContainsKey(right))
                    throw new ArgumentException("Duplicate detected. Both values must be unique.");

                _forward.Add(left, right);
                _reverse.Add(right, left);
            }
        }

        /// <summary>
        /// Gets the forward.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Indexer for the "Forward" lookup: map[key] -> value</returns>
        public T GetForward(T key)
        {
            lock (_lock) return _forward[key];
        }

        /// <summary>
        /// Gets the reverse.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Indexer for the "Reverse" lookup: map[value] -> key</returns>
        public T GetReverse(T value)
        {
            lock (_lock) return _reverse[value];
        }

        /// <summary>
        /// Tries the get forward.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        public bool TryGetForward(T key, out T? value)
        {
            lock (_lock) return _forward.TryGetValue(key, out value);
        }

        /// <summary>
        /// Tries the get reverse.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the value was found; otherwise, <c>false</c>.</returns>
        public bool TryGetReverse(T value, out T? key)
        {
            lock (_lock) return _reverse.TryGetValue(value, out key);
        }

        /// <summary>
        /// Removes the by left.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <returns><c>true</c> if the element was successfully removed; otherwise, <c>false</c>.</returns>
        public bool RemoveByLeft(T left)
        {
            lock (_lock)
            {
                if (!_forward.TryGetValue(left, out T? right)) return false;

                _forward.Remove(left);
                _reverse.Remove(right);
                return true;
            }
        }

        /// <summary>
        /// Determines whether this instance contains the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(T value)
        {
            lock (_lock) return _forward.ContainsKey(value) || _reverse.ContainsKey(value);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _forward.Clear();
                _reverse.Clear();
            }
        }
    }
}
