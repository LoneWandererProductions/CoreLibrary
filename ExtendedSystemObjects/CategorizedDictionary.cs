/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/CategorizedDictionary.cs
 * PURPOSE:     Extended Dictionary with an Category.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBeInternal

using System;
using System.Collections.Generic;
using System.Linq;

namespace ExtendedSystemObjects
{
    /// <summary>
    /// Dictionary with an category
    /// </summary>
    /// <typeparam name="K">Key Value</typeparam>
    /// <typeparam name="V">Value with Category</typeparam>
    public sealed class CategorizedDictionary<K, V>
    {
        /// <summary>
        /// The internal data of our custom Dictionary
        /// </summary>
        private readonly Dictionary<K, (string Category, V Value)> _data = new();

        /// <summary>
        /// Adds a value to the dictionary under the specified category.
        /// </summary>
        /// <param name="category">The category under which to add the key-value pair. Can be null.</param>
        /// <param name="key">The key of the value to add.</param>
        /// <param name="value">The value to add.</param>
        public void Add(string category, K key, V value)
        {
            _data[key] = (category, value);
        }

        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(K key, V value)
        {
            Add(string.Empty, key, value);
        }

        /// <summary>
        /// Gets a value from the dictionary based on the key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value if found, otherwise the default value for the type.</returns>
        public V Get(K key)
        {
            return _data.TryGetValue(key, out var entry) ? entry.Value : default(V);
        }

        /// <summary>
        /// Gets the category and value from the dictionary based on the key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>A tuple containing the category and value if found, otherwise null.</returns>
        public (string Category, V Value)? GetCategoryAndValue(K key)
        {
            return _data.TryGetValue(key, out var entry) ? entry : null;
        }

        /// <summary>
        /// Gets all key-value pairs under the specified category.
        /// </summary>
        /// <param name="category">The category to retrieve values from. Can be null.</param>
        /// <returns>A dictionary of key-value pairs in the specified category.</returns>
        public Dictionary<K, V> GetCategory(string category)
        {
            return _data
                .Where(entry => entry.Value.Category == category)
                .ToDictionary(entry => entry.Key, entry => entry.Value.Value);
        }

        /// <summary>
        /// Gets all categories.
        /// </summary>
        /// <returns>An enumerable of all categories.</returns>
        public IEnumerable<string> GetCategories()
        {
            return _data.Values
                .Select(entry => entry.Category)
                .Distinct();
        }

        /// <summary>
        /// Returns a string representation of the dictionary's contents.
        /// </summary>
        /// <returns>A string representing the dictionary's contents.</returns>
        public override string ToString()
        {
            var entries = _data.Select(entry =>
                $"Key: {entry.Key}, Category: {entry.Value.Category}, Value: {entry.Value.Value}");

            return string.Join(Environment.NewLine, entries);
        }
    }
}
