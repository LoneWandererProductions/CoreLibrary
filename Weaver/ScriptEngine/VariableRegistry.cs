/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.ScriptEngine
 * FILE:        VariableRegistry.cs
 * PURPOSE:     Default variable registry implementation for the Scripter Engine.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Text;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.ScriptEngine
{
    /// <inheritdoc />
    /// <summary>
    /// Default implementation of the variable registry.
    /// </summary>
    public sealed class VariableRegistry : IVariableRegistry
    {


        /// <summary>
        /// The registry itself
        /// for future Reference:
        /// public readonly struct VMValue
        /// {
        ///     public readonly EnumTypes Type;
        ///     public readonly long Int64;
        ///     public readonly double Double;
        ///     public readonly bool Bool;
        ///     public readonly string? String;
        /// }
        /// Dictionary<string, VMValue>
        /// </summary>
        /// <returns>
        /// The registry itself
        /// </returns>
        public IReadOnlyDictionary<string, (object Value, EnumTypes Type)> GetAll() => _registry;

        /// <summary>
        /// The registry
        /// </summary>
        private readonly Dictionary<string, (object Value, EnumTypes Type)> _registry = new();

        /// <inheritdoc />>
        public void Set(string key, object value, EnumTypes type)
        {
            _registry[key] = (value, type);
        }

        /// <inheritdoc />
        public bool TryGet(string key, out object? value, out EnumTypes type)
        {
            if (_registry.TryGetValue(key, out var entry))
            {
                value = entry.Value;
                type = entry.Type;
                return true;
            }

            value = null;
            type = EnumTypes.Wstring;
            return false;
        }

        /// <inheritdoc />
        public bool Remove(string key) => _registry.Remove(key);

        /// <inheritdoc />
        public void Clear() => _registry.Clear();

        /// <inheritdoc cref="IVariableRegistry" />
        public override string ToString()
        {
            if (_registry.Count == 0) return "Registry is empty.";

            var sb = new StringBuilder();
            foreach (var kvp in _registry)
            {
                sb.AppendLine($"{kvp.Key} = {kvp.Value.Value} ({kvp.Value.Type})");
            }

            return sb.ToString();
        }
    }
}