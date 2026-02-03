/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core.Commands
 * FILE:        EvaluateCommand.cs
 * PURPOSE:     Does double duty as internal command and extension to evaluate expressions. It can be either used as calculator or expression evaluator.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Globalization;
using Weaver.Interfaces;
using Weaver.Messages;
using Weaver.Registry;

namespace Weaver.Core.Commands
{
    /// <summary>
    /// Simple Commmand to evaluate expressions or do simple calculations.
    /// </summary>
    /// <seealso cref="ICommand" />
    internal class EvaluateCommand : ICommand
    {
        /// <inheritdoc />
        public string Name => "Evaluate";

        /// <inheritdoc />
        public string Description =>
            "In the Scriptengine this command provides an easier way to evaluate expressions or do simple calculation and expression evaluation.";

        /// <inheritdoc />
        public string Namespace => WeaverResources.GlobalNamespace;

        /// <inheritdoc />
        public int ParameterCount => -1;

        /// <inheritdoc />
        public CommandSignature Signature => new(Namespace, Name, ParameterCount);

        /// <summary>
        /// The registry
        /// </summary>
        private readonly IVariableRegistry? _registry;

        /// <summary>
        /// The evaluator
        /// </summary>
        private readonly IEvaluator _evaluator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluateCommand" /> class.
        /// </summary>
        /// <param name="evaluator">The evaluator.</param>
        /// <param name="registry">The registry. Optional only relevant for the Scriptengine.</param>
        public EvaluateCommand(IEvaluator evaluator, IVariableRegistry? registry = null)
        {
            _registry = registry;
            _evaluator = evaluator;
        }

        /// <inheritdoc />
        public CommandResult Execute(string[] args)
        {
            var expression = args.Length > 0 ? args[0] : null;
            var targetVar = args.Length > 1 ? args[1] : null;


            // If no expression, maybe store previous pipeline value or return null
            if (string.IsNullOrWhiteSpace(expression))
            {
                return CommandResult.Ok(null);
            }

            // Resolve variables from registry as before, if registry is provided
            if (_registry != null) expression = _registry.ReplaceVariablesInExpression(expression);

            object? result;
            EnumTypes type;

            var isBooleanExpr = _evaluator.IsBooleanExpression(expression);

            try
            {
                if (isBooleanExpr)
                {
                    result = _evaluator.Evaluate(expression); // returns bool
                    type = EnumTypes.Wbool;
                }
                else
                {
                    result = _evaluator.EvaluateNumeric(expression); // returns double
                    type = EnumTypes.Wdouble;
                }
            }
            catch (Exception ex)
            {
                return CommandResult.Fail(
                    $"Cannot evaluate expression '{expression}': {ex.Message}. " +
                    "Usage: evaluate(<expression> [, <targetVar>])"
                );
            }

            // Store in registry if requested
            if (!string.IsNullOrWhiteSpace(targetVar))
            {
                if (_registry == null)
                {
                    return CommandResult.Fail(
                        "Cannot store result: registry not provided. " +
                        "Usage: evaluate(<expression>, <targetVar>) requires registry in constructor."
                    );
                }

                _registry.Set(targetVar, result!, type);

                return CommandResult.Ok(
                    $"Stored '{result}' in '{targetVar}'.",
                    result,
                    type
                );
            }

            // Return computed result
            var message = type switch
            {
                EnumTypes.Wbool => result?.ToString() ?? "false",
                EnumTypes.Wdouble => Convert.ToDouble(result).ToString(CultureInfo.InvariantCulture),
                _ => result?.ToString() ?? "null"
            };

            return CommandResult.Ok(message, result, type);
        }
    }
}