/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Registry
 * FILE:        RegistryExtensions.cs
 * PURPOSE:     Evaluation Helpers for expression evaluation. Is shared between Evaluator and Commands.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Globalization;
using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Registry
{
    public static class RegistryExtensions
    {
        /// <summary>
        /// Replaces all variables in the expression with their literal values.
        /// Returns the replaced expression string.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// Expression replaced with values from registry.
        /// </returns>
        public static string ReplaceVariablesInExpression(
            this IVariableRegistry registry,
            string expression)
        {
            if (registry == null || string.IsNullOrWhiteSpace(expression))
                return expression;

            foreach (var variable in registry.GetAll())
            {
                var key = variable.Key;
                var (valueObj, valueType) = variable.Value;

                if (valueObj == null)
                    continue;

                var replacement = valueType switch
                {
                    EnumTypes.Wint =>
                        Convert.ToInt64(valueObj).ToString(),

                    EnumTypes.Wdouble =>
                        Convert.ToDouble(valueObj)
                            .ToString(CultureInfo.InvariantCulture),

                    EnumTypes.Wbool =>
                        Convert.ToBoolean(valueObj) ? "1" : "0",

                    EnumTypes.Wstring =>
                        valueObj.ToString() ?? string.Empty,

                    _ =>
                        valueObj.ToString() ?? string.Empty
                };

                expression = expression.Replace(
                    key,
                    replacement,
                    StringComparison.OrdinalIgnoreCase);
            }

            return expression;
        }

        /// <summary>
        /// Attempts to get a variable as a bool, returns false if not found.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="registry">The registry.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        /// <returns>Checks if variable can be found in registry.</returns>
        public static bool TryEvaluateAsBool(
            this IVariableRegistry registry,
            string name,
            out bool value)
        {
            value = false;

            if (registry == null || string.IsNullOrWhiteSpace(name))
                return false;

            if (registry.TryGet(name, out var vm))
            {
                value = ToBool(vm);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether [is numeric type] [the specified variable].
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <param name="variable">The variable.</param>
        /// <returns>
        ///   <c>true</c> if [is numeric type] [the specified variable]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumericType(
            this IVariableRegistry registry,
            string variable)
        {
            if (registry == null)
                return false;

            return registry.TryGet(variable, out _, out var type)
                   && (type == EnumTypes.Wint
                       || type == EnumTypes.Wdouble
                       || type == EnumTypes.Wbool);
        }

        /// <summary>
        /// Converts a variable object from the registry to a boolean value.
        /// </summary>
        /// <param name="vm">The vm.</param>
        /// <returns>Converts bool to a more calculateable numeric.</returns>
        private static bool ToBool(VmValue vm)
        {
            return vm.Type switch
            {
                EnumTypes.Wbool => vm.Bool,
                EnumTypes.Wint => vm.Int64 != 0,
                EnumTypes.Wdouble => vm.Double != 0.0,
                EnumTypes.Wstring => !string.IsNullOrEmpty(vm.String),
                _ => throw new ArgumentException($"Unsupported variable type: {vm.Type}")
            };
        }

        /// <summary>
        /// Tries the get pointer.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="pointer">The pointer.</param>
        /// <returns>Get type and Key to Pointer.</returns>
        public static bool TryGetPointer(
            this IVariableRegistry registry,
            string name,
            out EnumTypes type,
            out string pointer)
        {
            type = default;
            pointer = null;

            if (registry == null || string.IsNullOrWhiteSpace(name))
                return false;

            if (!registry.TryGet(name, out var vm))
                return false;

            type = vm.Type;
            if (string.IsNullOrEmpty(vm.String))
                return false;

            pointer = vm.String;
            return true;
        }
    }
}