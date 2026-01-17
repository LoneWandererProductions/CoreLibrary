/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Interfaces
 * FILE:        IVariableRegistry.cs
 * PURPOSE:     Interface for script variable registry.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Messages;
using Weaver.ScriptEngine;

namespace Weaver.Interfaces
{
    /// <summary>
    /// Defines an interface for variable storage used by the Scripter engine.
    /// </summary>
    public interface IVariableRegistry
    {
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>The registry itself</returns>
        IReadOnlyDictionary<string, VMValue> GetAll();

        /// <summary>
        /// Getets the specified key as string for Debug Purposes.
        /// </summary>
        /// <param name="key">The key as string.</param>
        string Get(string key);

        /// <summary>
        /// Sets a value in the registry.
        /// </summary>
        /// <param name="key">Variable key.</param>
        /// <param name="value">Value to store.</param>
        void Set(string key, VMValue value);

        /// <summary>
        /// Attempts to get a value from the registry.
        /// </summary>
        /// <param name="key">Variable key.</param>
        /// <param name="value">Output value.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
        bool TryGet(string key, out VMValue value);

        /// <summary>
        /// Sets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        void Set(string key, object value, EnumTypes type);

        /// <summary>
        /// Tries the get.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
        bool TryGet(string key, out object? value, out EnumTypes type);

        /// <summary>
        /// Removes a key from the registry.
        /// </summary>
        /// <param name="key">Variable key.</param>
        /// <returns><c>true</c> if removed; otherwise, <c>false</c>.</returns>
        bool Remove(string key);

        /// <summary>
        /// Clears all entries.
        /// </summary>
        void Clear();

        /// <summary>
        /// Converts to string.
        /// For pretty-printing all values
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        string ToString();
    }
}
