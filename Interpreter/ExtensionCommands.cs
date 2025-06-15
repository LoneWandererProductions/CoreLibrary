/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     OutCommand
 * FILE:        Interpreter/ExtensionCommands.cs
 * PURPOSE:     Handles potential Extension Methods from Internal and external
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System.Collections.Generic;

namespace Interpreter
{
    /// <summary>
    ///     Extensions for the existing commands
    /// </summary>
    public sealed class ExtensionCommands
    {
        /// <summary>
        ///     Gets the used name space.
        /// </summary>
        /// <value>
        ///     The used name space.
        /// </value>
        public string ExtensionNameSpace { get; internal set; }

        /// <summary>
        ///     Gets or sets the command.
        /// </summary>
        public int ExtensionCommand { get; internal set; }

        /// <summary>
        ///     Gets or sets the parameter.
        /// </summary>
        public List<string> ExtensionParameter { get; internal set; }

        /// <summary>
        ///     Gets the feedback identifier.
        ///     Define the feedback id
        /// </summary>
        /// <value>
        ///     The feedback identifier.
        /// </value>
        public int FeedBackId { get; internal set; }

        /// <summary>
        ///     Gets or sets the base command.
        ///     Only visible within the Interpreter Namespace
        /// </summary>
        /// <value>
        ///     The base command.
        /// </value>
        internal string BaseCommand { get; set; }
    }
}
