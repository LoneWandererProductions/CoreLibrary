/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     OutCommand
 * FILE:        Interpreter/Prompt.cs
 * PURPOSE:     handles the converted Output of the Interpreter
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

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
        public bool ExtensionUsed { get; internal init; }
    }
}
