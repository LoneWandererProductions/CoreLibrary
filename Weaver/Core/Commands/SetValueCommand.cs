/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core.Commands
 * FILE:        SetValueCommand.cs
 * PURPOSE:     Sets a value and type for the Scripter Engine. Uses the internal Scripter Variable Storage.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Weaver.Interfaces;
using Weaver.Messages;

namespace Weaver.Core.Commands
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
        /// The evaluator
        /// </summary>
        private readonly IEvaluator _evaluator;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="SetValueCommand"/> class.
        /// </summary>
        /// <param name="registry">The registry.</param>
        /// <param name="evaluator">The evaluator.</param>
        public SetValueCommand(IVariableRegistry registry, IEvaluator evaluator)
        {
            _registry = registry;
            _evaluator = evaluator;
        }

        /// <inheritdoc />
        public CommandResult Execute(params string[] args)
        {
            if (args.Length != 3)
                return CommandResult.Fail("Usage: setValue([key],[value],[type])");

            var key = args[0];
            var valueString = args[1];
            var typeString = args[2].ToLowerInvariant();

            object value;
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
                    value = Convert.ToInt64(_evaluator.EvaluateNumeric(valueString));
                    type = EnumTypes.Wint;
                    break;

                case "double":
                case "wdouble":
                    value = _evaluator.EvaluateNumeric(valueString);
                    type = EnumTypes.Wdouble;
                    break;

                case "bool":
                case "wbool":
                    value = _evaluator.Evaluate(valueString);
                    type = EnumTypes.Wbool;
                    break;

                default:
                    return CommandResult.Fail($"Unknown type '{typeString}'");
            }

            _registry.Set(key, value, type);

            return CommandResult.Ok(
                $"Registered key '{key}' with type {type} and value '{value}'.",
                value,
                type
            );
        }
    }
}