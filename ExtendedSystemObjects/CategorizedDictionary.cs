/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:      ExtendedSystemObjects
 * FILE:         ExtendedSystemObjects/CategorizedDictionary.cs
 * PURPOSE:      Extended Dictionary with a Category.
 * PROGRAMER:    Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace ExtendedSystemObjects
{
    /// <summary>
    /// A thread-safe dictionary that associates each key with a category.
    /// Provides fast lookups by key or by category.
    /// </summary>
    /// <typeparam name="TK">Type of dictionary keys.</typeparam>
    /// <typeparam name="TV">Type of dictionary values.</typeparam>
    /// <seealso cref="System.Collections.Generic.IEnumerable&lt;(TK Key, System.String Category, TV Value)&gt;" />
    [Serializable]
    public sealed class CategorizedDictionary<TK, TV> : IEnumerable<(TK Key, string Category, TV Value)>
    {
        /// <summary>
        /// Internal storage mapping keys to (Category, Value) pairs.
        /// </summary>
        private readonly Dictionary<TK, (string Category, TV Value)> _data;

        /// <summary>
        /// Secondary index mapping categories to sets of keys for fast category lookups.
        /// </summary>
        private readonly Dictionary<string, HashSet<TK>> _categories;

        /// <summary>
        /// Normalizes the category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns>string Empty if category was empty.</returns>
        private static string NormalizeCategory(string category) => category ?? string.Empty;

        /// <summary>
        /// Lock for thread-safety.
        /// </summary>
        private readonly ReaderWriterLockSlim _lock;

        /// <summary>
        /// Initializes a new empty instance of <see cref="CategorizedDictionary{TK,TV}" />.
        /// </summary>
        public CategorizedDictionary()
        {
            _data = new Dictionary<TK, (string, TV)>();
            _categories = new Dictionary<string, HashSet<TK>>(StringComparer.OrdinalIgnoreCase);
            _lock = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Gets the number of entries in the dictionary.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try { return _data.Count; }
                finally { _lock.ExitReadLock(); }
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// Setting a key that does not exist adds it with an empty category.
        /// </summary>
        /// <value>
        /// The <see cref="TV"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Key '{key}' not found.</exception>
        public TV this[TK key]
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    if (_data.TryGetValue(key, out var entry)) return entry.Value;

                    throw new KeyNotFoundException($"Key '{key}' not found.");
                }
                finally { _lock.ExitReadLock(); }
            }
            set
            {
                _lock.EnterWriteLock();
                try
                {
                    if (_data.TryGetValue(key, out var old))
                    {
                        _data[key] = (old.Category, value);
                    }
                    else
                    {
                        AddInternal(string.Empty, key, value);
                    }
                }
                finally { _lock.ExitWriteLock(); }
            }
        }

        /// <summary>
        /// Adds a value with an empty category.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(TK key, TV value) => Add(string.Empty, key, value);

        /// <summary>
        /// Adds a value under the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(string category, TK key, TV value)
        {
            _lock.EnterWriteLock();
            try { AddInternal(NormalizeCategory(category), key, value); }
            finally { _lock.ExitWriteLock(); }
        }

        /// <summary>
        /// Internal helper for adding a new entry. Updates both _data and _categories.
        /// Throws <see cref="ArgumentException"/> if the key already exists.
        /// </summary>
        private void AddInternal(string category, TK key, TV value)
        {
            if (_data.ContainsKey(key))
                throw new ArgumentException($"Key '{key}' already exists.");

            _data[key] = (category, value);

            if (!_categories.TryGetValue(category, out var set))
            {
                set = new HashSet<TK>();
                _categories[category] = set;
            }

            set.Add(key);
        }

        /// <summary>
        /// Removes the entry with the specified key.
        /// Returns true if the key existed and was removed; false otherwise.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Removes key, returns true if a key was removed.</returns>
        public bool Remove(TK key)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_data.TryGetValue(key, out var entry)) return false;

                _data.Remove(key);

                // Safe to assume category exists and set is not null because of AddInternal logic
                if (_categories.TryGetValue(entry.Category, out var set))
                {
                    set.Remove(key);
                    if (set.Count == 0)
                        _categories.Remove(entry.Category);
                }

                return true;
            }
            finally { _lock.ExitWriteLock(); }
        }

        /// <summary>
        /// Returns true if the dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(TK key)
        {
            _lock.EnterReadLock();
            try { return _data.ContainsKey(key); }
            finally { _lock.ExitReadLock(); }
        }

        /// <summary>
        /// Tries to get the value associated with the specified key.
        /// Returns true if found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>Tries to get value and if it does it returns it.</returns>
        public bool TryGetValue(TK key, out TV value)
        {
            _lock.EnterReadLock();
            try
            {
                if (_data.TryGetValue(key, out var entry))
                {
                    value = entry.Value;
                    return true;
                }

                value = default;
                return false;
            }
            finally { _lock.ExitReadLock(); }
        }

        /// <summary>
        /// Tries to get the category of a given key.
        /// Returns true if found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="category">The category.</param>
        /// <returns>Checks if category exists and returns if it does.</returns>
        public bool TryGetCategory(TK key, out string category)
        {
            _lock.EnterReadLock();
            try
            {
                if (_data.TryGetValue(key, out var entry))
                {
                    category = entry.Category;
                    return true;
                }

                category = null;
                return false;
            }
            finally { _lock.ExitReadLock(); }
        }

        /// <summary>
        /// Gets the category associated with a key.
        /// Throws <see cref="KeyNotFoundException" /> if the key does not exist.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Categroy of the value</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException"></exception>
        public string GetCategory(TK key)
        {
            _lock.EnterReadLock();
            try
            {
                if (_data.TryGetValue(key, out var entry)) return entry.Category;

                throw new KeyNotFoundException();
            }
            finally { _lock.ExitReadLock(); }
        }

        /// <summary>
        /// Updates the category of an existing key.
        /// Returns true if successful; false if the key does not exist.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="newCategory">The new category.</param>
        /// <returns>True if success.</returns>
        public bool SetCategory(TK key, string newCategory)
        {
            // Fix: Normalize input category to ensure consistency with Add()
            newCategory = NormalizeCategory(newCategory);

            _lock.EnterWriteLock();
            try
            {
                if (!_data.TryGetValue(key, out var entry)) return false;

                // Optimization: Don't do anything if category hasn't changed
                if (string.Equals(entry.Category, newCategory, StringComparison.Ordinal))
                    return true;

                // Remove from old category
                if (_categories.TryGetValue(entry.Category, out var oldSet))
                {
                    oldSet.Remove(key);
                    if (oldSet.Count == 0)
                        _categories.Remove(entry.Category);
                }

                // Add to new category
                _data[key] = (newCategory, entry.Value);
                if (!_categories.TryGetValue(newCategory, out var newSet))
                {
                    newSet = new HashSet<TK>();
                    _categories[newCategory] = newSet;
                }

                newSet.Add(key);

                return true;
            }
            finally { _lock.ExitWriteLock(); }
        }

        /// <summary>
        /// Returns all existing categories.
        /// </summary>
        /// <returns>list of all categories.</returns>
        public IEnumerable<string> GetCategories()
        {
            _lock.EnterReadLock();
            try
            {
                // Fix: Must snapshot keys inside the lock to allow safe iteration outside
                return new List<string>(_categories.Keys);
            }
            finally { _lock.ExitReadLock(); }
        }

        /// <summary>
        /// Returns all keys in a given category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns>All keys by category</returns>
        public IEnumerable<TK> GetKeys(string category)
        {
            category = NormalizeCategory(category);
            _lock.EnterReadLock();
            try
            {
                if (_categories.TryGetValue(category, out var set))
                {
                    // Fix: Must snapshot the HashSet to allow safe iteration outside
                    return new List<TK>(set);
                }

                return Array.Empty<TK>();
            }
            finally { _lock.ExitReadLock(); }
        }

        /// <summary>
        /// Returns all keys in the dictionary regardless of category.
        /// </summary>
        /// <returns>
        /// All keys
        /// </returns>
        public IEnumerable<TK> GetKeys()
        {
            _lock.EnterReadLock();
            try
            {
                // Fix: Must snapshot keys inside the lock
                return new List<TK>(_data.Keys);
            }
            finally { _lock.ExitReadLock(); }
        }

        /// <summary>
        /// Returns the category and value for a given key, or null if key not found.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>Category and value.</returns>
        public (string Category, TV Value)? GetCategoryAndValue(TK key)
        {
            _lock.EnterReadLock();
            try
            {
                if (_data.TryGetValue(key, out var entry))
                    return entry;

                return null;
            }
            finally { _lock.ExitReadLock(); }
        }


        /// <summary>
        /// Returns a dictionary of key-value pairs in the specified category.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <returns>Get Key Value by category</returns>
        public Dictionary<TK, TV> GetCategory(string category)
        {
            category = NormalizeCategory(category);
            _lock.EnterReadLock();
            try
            {
                if (!_categories.TryGetValue(category, out var keys))
                    return new Dictionary<TK, TV>();

                // This already creates a new Dictionary (snapshot), so it is thread-safe
                var dict = new Dictionary<TK, TV>(keys.Count);
                foreach (var key in keys) dict[key] = _data[key].Value;
                return dict;
            }
            finally { _lock.ExitReadLock(); }
        }

        /// <summary>
        /// Clears all entries and categories.
        /// </summary>
        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _data.Clear();
                _categories.Clear();
            }
            finally { _lock.ExitWriteLock(); }
        }

        /// <summary>
        /// Returns an enumerator over (Key, Category, Value) tuples.
        /// Enumeration is thread-safe via snapshotting.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<(TK Key, string Category, TV Value)> GetEnumerator()
        {
            List<(TK, string, TV)> snapshot;
            _lock.EnterReadLock();
            try
            {
                snapshot = new List<(TK, string, TV)>(_data.Count);
                foreach (var kvp in _data)
                    snapshot.Add((kvp.Key, kvp.Value.Category, kvp.Value.Value));
            }
            finally { _lock.ExitReadLock(); }

            // Iterate outside the lock to avoid deadlocks or contention
            foreach (var item in snapshot) yield return item;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
