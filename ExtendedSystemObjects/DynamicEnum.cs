/*
 * COPYRIGHT:   See COPYING in the top-level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/DynamicEnum.cs
 * PURPOSE:     Framework for our Dynamic Enum
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBeInternal

using System;
using System.Collections.Generic;

namespace ExtendedSystemObjects
{
    /// <summary>
    /// A dynamic enum framework that allows for the creation of custom enums with dynamic
    /// addition, removal, and retrieval of entries at runtime.
    /// </summary>
    /// <typeparam name="T">The type that inherits from <see cref="DynamicEnum{T}"/>. This is typically a class that defines specific instances of the enum.</typeparam>
    public class DynamicEnum<T> where T : DynamicEnum<T>
    {
        /// <summary>
        /// A dictionary that holds all the enum values, indexed by their names.
        /// </summary>
        private static readonly Dictionary<string, T> Values = new();

        /// <summary>
        /// Gets the name of the enum entry.
        /// </summary>
        /// <value>
        /// A string representing the name of the enum entry.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the integer value associated with the enum entry.
        /// </summary>
        /// <value>
        /// An integer value representing the enum entry.
        /// </value>
        public int Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicEnum{T}"/> class.
        /// This constructor is called when a new enum entry is created.
        /// </summary>
        /// <param name="name">The name of the enum entry.</param>
        /// <param name="value">The integer value of the enum entry.</param>
        protected DynamicEnum(string name, int value)
        {
            Name = name;
            Value = value;
            Values[name] = (T)this;
        }

        /// <summary>
        /// Adds a new entry to the enum or returns the existing entry if it already exists.
        /// </summary>
        /// <param name="name">The name of the enum entry to add.</param>
        /// <param name="value">The integer value of the enum entry.</param>
        /// <returns>
        /// The <typeparamref name="T"/> instance representing the newly added or existing enum entry.
        /// </returns>
        /// <remarks>
        /// If the entry already exists, this method will return the existing entry without creating a new instance.
        /// </remarks>
        public static T Add(string name, int value)
        {
            if (!Values.ContainsKey(name))
            {
                return (T)Activator.CreateInstance(typeof(T), name, value)!;
            }

            return Values[name];
        }

        /// <summary>
        /// Removes the specified enum entry by name.
        /// </summary>
        /// <param name="name">The name of the enum entry to remove.</param>
        /// <returns>
        /// A boolean value indicating whether the removal was successful.
        /// </returns>
        /// <remarks>
        /// If the entry does not exist, this method returns false and does not affect the dictionary.
        /// </remarks>
        public static bool Remove(string name) => Values.Remove(name);

        /// <summary>
        /// Attempts to retrieve an enum entry by name.
        /// </summary>
        /// <param name="name">The name of the enum entry to retrieve.</param>
        /// <param name="result">The resulting enum entry, if found.</param>
        /// <returns>
        /// A boolean value indicating whether the enum entry was found.
        /// </returns>
        /// <remarks>
        /// If the entry does not exist, <paramref name="result"/> will be set to null.
        /// </remarks>
        public static bool TryGet(string name, out T result) => Values.TryGetValue(name, out result);

        /// <summary>
        /// Retrieves all enum entries.
        /// </summary>
        /// <returns>
        /// A read-only collection of all <typeparamref name="T"/> enum entries.
        /// </returns>
        /// <remarks>
        /// The collection is read-only, meaning entries cannot be modified directly.
        /// </remarks>
        public static IReadOnlyCollection<T> GetAll() => Values.Values;

        /// <summary>
        /// Converts the enum entry to its string representation.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> representing the name of the enum entry.
        /// </returns>
        public override string ToString() => Name;
    }
}
