/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Interfaces
 * FILE:        IVariableRegistry.cs
 * PURPOSE:     Interface for script variable registry.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Messages;

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
        IReadOnlyDictionary<string, (object Value, EnumTypes Type)> GetAll();

        /// <summary>
        /// Sets a value in the registry.
        /// </summary>
        /// <param name="key">Variable key.</param>
        /// <param name="value">Value to store.</param>
        /// <param name="type">Type of value.</param>
        void Set(string key, object value, EnumTypes type);

        /// <summary>
        /// Attempts to get a value from the registry.
        /// </summary>
        /// <param name="key">Variable key.</param>
        /// <param name="value">Output value.</param>
        /// <param name="type">Output type.</param>
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