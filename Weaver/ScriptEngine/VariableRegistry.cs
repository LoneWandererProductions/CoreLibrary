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
        public IReadOnlyDictionary<string, VMValue> GetAll() => _registry;

        /// <summary>
        /// The registry
        /// </summary>
        private readonly Dictionary<string, VMValue> _registry = new();

        /// <inheritdoc />
        public string Get(string key)
        {
            return _registry[key].ToString();
        }

        /// <inheritdoc />
        public void Set(string key, VMValue value)
        {
            _registry[key] = value;
        }

        /// <inheritdoc />
        public bool TryGet(string key, out VMValue value)
        {
            return _registry.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public bool Remove(string key) => _registry.Remove(key);

        /// <inheritdoc />
        public void Clear() => _registry.Clear();

        /// <summary>
        /// Typed getter for Int64 values
        /// </summary>
        public bool TryGetInt(string key, out long value)
        {
            if (TryGet(key, out var vm) && vm.Type == EnumTypes.Wint)
            {
                value = vm.Int64;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Typed getter for Double values
        /// </summary>
        public bool TryGetDouble(string key, out double value)
        {
            if (TryGet(key, out var vm) && vm.Type == EnumTypes.Wdouble)
            {
                value = vm.Double;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Typed getter for Bool values
        /// </summary>
        public bool TryGetBool(string key, out bool value)
        {
            if (TryGet(key, out var vm) && vm.Type == EnumTypes.Wbool)
            {
                value = vm.Bool;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Typed getter for String values
        /// </summary>
        public bool TryGetString(string key, out string? value)
        {
            if (TryGet(key, out var vm) && vm.Type == EnumTypes.Wstring)
            {
                value = vm.String;
                return true;
            }
            value = null;
            return false;
        }

        /// <inheritdoc cref="IVariableRegistry" />
        public override string ToString()
        {
            if (_registry.Count == 0) return "Registry is empty.";

            var sb = new StringBuilder();
            foreach (var kvp in _registry)
            {
                sb.AppendLine($"{kvp.Key} = {kvp.Value} ({kvp.Value.Type})");
            }

            return sb.ToString();
        }

        /// <inheritdoc />
        public void Set(string key, object value, EnumTypes type)
        {
            VMValue vm = type switch
            {
                EnumTypes.Wint => VMValue.FromInt(Convert.ToInt64(value)),
                EnumTypes.Wdouble => VMValue.FromDouble(Convert.ToDouble(value)),
                EnumTypes.Wbool => VMValue.FromBool(Convert.ToBoolean(value)),
                EnumTypes.Wstring => VMValue.FromString(value?.ToString()),
                _ => VMValue.FromString(value?.ToString())
            };

            _registry[key] = vm;
        }

        /// <inheritdoc />
        public bool TryGet(string key, out object? value, out EnumTypes type)
        {
            if (_registry.TryGetValue(key, out var vm))
            {
                type = vm.Type;
                value = vm.Type switch
                {
                    EnumTypes.Wint => vm.Int64,
                    EnumTypes.Wdouble => vm.Double,
                    EnumTypes.Wbool => vm.Bool,
                    EnumTypes.Wstring => vm.String,
                    _ => null
                };
                return true;
            }

            value = null;
            type = EnumTypes.Wstring;
            return false;
        }

    }
}
