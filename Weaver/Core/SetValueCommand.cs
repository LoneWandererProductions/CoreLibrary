/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        SetValueCommand.cs
 * PURPOSE:     Sets a value and type for the Scripter Engine. Uses the internal Scripter Variable Storage.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core
{
    /// <inheritdoc />
    /// <summary>
    ///     Internal command, sets a typed value in the Scripter registry.
    /// </summary>
    public sealed class SetValueCommand : ICommand
    {
        /// <summary>
        /// The registry
        /// </summary>
        private readonly IVariableRegistry _registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetValueCommand"/> class.
        /// </summary>
        /// <param name="registry">The variable registry to store the value in.</param>
        public SetValueCommand(IVariableRegistry registry)
        {
            _registry = registry;
        }

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public string Name => "SetValue";

        /// <inheritdoc />
        public string Description =>
            "Sets a typed value in the registry. Usage: setValue([key],[value],[type]). Overwrites if key exists.";

        /// <inheritdoc />
        public int ParameterCount => 3;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <inheritdoc />
        public IReadOnlyDictionary<string, int>? Extensions => null;

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            // Expecting: key, value, type
            if (args.Length != 3)
            {
                return CommandResult.Fail("Usage: setValue([key],[value],[type])");
            }

            var key = args[0];
            var valueString = args[1];
            var typeString = args[2].ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(key))
                return CommandResult.Fail("Key cannot be empty.");

            if (string.IsNullOrWhiteSpace(typeString))
                return CommandResult.Fail("Type must be specified (string, int, double, bool).");

            // Determine type and try parse
            object? value;
            EnumTypes type;

            switch (typeString)
            {
                case "string":
                case "wstring":
                    value = valueString;
                    type = EnumTypes.Wstring;
                    break;

                case "int":
                case "wint":
                    if (!int.TryParse(valueString, out var i))
                        return CommandResult.Fail($"Invalid int value: {valueString}");

                    value = i;
                    type = EnumTypes.Wint;
                    break;

                case "double":
                case "wdouble":
                    if (!double.TryParse(valueString, out var d))
                        return CommandResult.Fail($"Invalid double value: {valueString}");

                    value = d;
                    type = EnumTypes.Wdouble;
                    break;

                case "bool":
                case "wbool":
                    if (!bool.TryParse(valueString, out var b))
                        return CommandResult.Fail($"Invalid bool value: {valueString}");

                    value = b;
                    type = EnumTypes.Wbool;
                    break;

                default:
                    return CommandResult.Fail($"Unknown type '{typeString}'. Supported: string, int, double, bool.");
            }

            // Store in registry
            _registry.Set(key, value, type);

            return CommandResult.Ok(
                $"Registered key '{key}' with type {type} and value '{value}'.",
                value,
                type
            );
        }

        /// <inheritdoc />
        public CommandResult InvokeExtension(string extensionName, params string[] args)
        {
            return CommandResult.Fail($"'{Name}' has no extensions.");
        }
    }
}