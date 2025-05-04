/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/Prompt.cs
 * PURPOSE:     handles the converted Output of the Interpreter
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Interpreter
{
    /// <summary>
    ///     Only simple Methods with Parameter that are not Collections for now
    /// </summary>
    public sealed class OutCommand
    {
        /// <summary>
        ///     Gets the used name space.
        /// </summary>
        /// <value>
        ///     The used name space.
        /// </value>
        public string UsedNameSpace { get; internal init; }

        /// <summary>
        ///     Gets or sets the command.
        /// </summary>
        public int Command { get; internal init; }

        /// <summary>
        ///     Gets or sets the parameter.
        /// </summary>
        public List<string> Parameter { get; internal init; }

        /// <summary>
        ///     Gets a value indicating whether [extension used].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [extension used]; otherwise, <c>false</c>.
        /// </value>
        public bool ExtensionUsed => ExtensionCommand != null;

        /// <summary>
        ///     Gets the error message.
        /// </summary>
        /// <value>
        ///     The error message.
        /// </value>
        public string ErrorMessage { get; internal init; }

        /// <summary>
        ///     Gets the extension command.
        ///     only relevant if ExtensionUsed is true
        /// </summary>
        /// <value>
        ///     The extension command.
        /// </value>
        public ExtensionCommands ExtensionCommand { get; internal init; }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(UsedNameSpace)}: {UsedNameSpace}");
            sb.AppendLine($"{nameof(Command)}: {Command}");

            if (Parameter?.Any() == true) sb.AppendLine($"{nameof(Parameter)}: {string.Join(", ", Parameter)}");

            sb.AppendLine($"{nameof(ExtensionUsed)}: {ExtensionUsed}");

            if (ExtensionUsed) sb.AppendLine($"{nameof(ExtensionCommand)}: {ExtensionCommand}");

            if (!string.IsNullOrEmpty(ErrorMessage)) sb.AppendLine($"{nameof(ErrorMessage)}: {ErrorMessage}");

            return sb.ToString();
        }
    }
}