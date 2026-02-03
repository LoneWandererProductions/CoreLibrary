/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Registry
 * FILE:        VariableRegistry.cs
 * PURPOSE:     Default variable registry implementation for the Scripter Engine.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Text;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Registry
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
        public IReadOnlyDictionary<string, VmValue> GetAll() => _registry;

        /// <summary>
        /// The registry
        /// </summary>
        private readonly Dictionary<string, VmValue> _registry = new();

        /// <summary>
        /// The look up
        /// </summary>
        private readonly Dictionary<string, VmRange> _lookUp = new();

        /// <summary>
        /// The store
        /// </summary>
        private readonly Dictionary<int, VmValue> _store = new();

        /// <inheritdoc />
        public string Get(string key)
        {
            return _registry[key].ToString();
        }

        /// <inheritdoc />
        public EnumTypes GetType(string key)
        {
            if (_registry.TryGetValue(key, out var vm))
                return vm.Type;

            throw new KeyNotFoundException($"Key '{key}' not found in registry.");
        }

        /// <inheritdoc />
        public void Set(string key, VmValue value)
        {
            _registry[key] = value;
        }

        /// <inheritdoc />
        public bool TryGet(string key, out VmValue value)
        {
            return _registry.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public bool TryGetList(string key, out IReadOnlyList<VmValue>? list)
        {
            list = null;
            if (!_registry.TryGetValue(key, out var vm) || vm.Type != EnumTypes.Wlist) return false;
            if (!_lookUp.TryGetValue(key, out var range)) return false;

            list = Enumerable.Range(range.Start, range.Length)
                .Select(i => _store[i])
                .ToList();
            return true;
        }

        /// <inheritdoc />
        public bool TryGetObject(string key, out IReadOnlyDictionary<string, VmValue>? obj)
        {
            obj = null;

            if (!_registry.TryGetValue(key, out var vm) || vm.Type != EnumTypes.Wobject)
                return false;

            if (!_lookUp.TryGetValue(key, out var range))
                return false;

            var dict = new Dictionary<string, VmValue>();
            for (var i = range.Start; i < range.End; i++)
            {
                var valueVm = _store[i];
                if (valueVm.Attribute == null)
                    continue; // skip values without an attribute

                dict[valueVm.Attribute] = valueVm;
            }

            obj = dict;
            return true;
        }


        /// <inheritdoc />
        public bool TryGetPointer(string key, out object? value, out EnumTypes type)
        {
            value = null;
            type = EnumTypes.Wstring; // default

            if (!_registry.TryGetValue(key, out var vm)) return false;
            if (vm.Type != EnumTypes.Wpointer) return false;
            if (vm.String == null) return false; // pointer key stored in String for simplicity

            var targetKey = vm.String;

            if (!_registry.TryGetValue(targetKey, out var targetVm)) return false;

            type = targetVm.Type;
            switch (type)
            {
                case EnumTypes.Wint:
                    value = targetVm.Int64;
                    break;
                case EnumTypes.Wdouble:
                    value = targetVm.Double;
                    break;
                case EnumTypes.Wbool:
                    value = targetVm.Bool;
                    break;
                case EnumTypes.Wstring:
                    value = targetVm.String;
                    break;
                case EnumTypes.Wlist:
                    if (TryGetList(targetKey, out var l2))
                    {
                        value = l2;
                        return true;
                    }

                    return false;

                case EnumTypes.Wobject:
                    if (TryGetObject(targetKey, out var o2))
                    {
                        value = o2;
                        return true;
                    }

                    return false;

                default: return false;
            }

            return true;
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            var check = _lookUp.TryGetValue(key, out var range);
            if (check)
            {
                for (var i = range.Start; i < range.End; i++)
                {
                    _store.Remove(i);
                }
            }

            _lookUp.Remove(key);


            return _registry.Remove(key);
        }

        /// <inheritdoc />
        public void ClearAll()
        {
            _registry.Clear();
            _lookUp.Clear();
            _store.Clear();
        }

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
            var vm = type switch
            {
                EnumTypes.Wint => VmValue.FromInt(Convert.ToInt64(value)),
                EnumTypes.Wdouble => VmValue.FromDouble(Convert.ToDouble(value)),
                EnumTypes.Wbool => VmValue.FromBool(Convert.ToBoolean(value)),
                EnumTypes.Wstring => VmValue.FromString(value?.ToString()),
                _ => VmValue.FromString(value?.ToString())
            };

            _registry[key] = vm;
        }

        /// <inheritdoc />
        public bool TryGet(string key, out object? value, out EnumTypes type)
        {
            if (_registry.TryGetValue(key, out var vm))
            {
                type = vm.Type;

                switch (vm.Type)
                {
                    case EnumTypes.Wint:
                        value = vm.Int64;
                        return true;

                    case EnumTypes.Wdouble:
                        value = vm.Double;
                        return true;

                    case EnumTypes.Wbool:
                        value = vm.Bool;
                        return true;

                    case EnumTypes.Wstring:
                        value = vm.String;
                        return true;

                    // compound types are NOT representable here
                    case EnumTypes.Wlist:
                    case EnumTypes.Wobject:
                    case EnumTypes.Wpointer:
                        value = null;
                        return false;
                }
            }

            value = null;
            type = EnumTypes.Wstring;
            return false;
        }
    }
}