/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Plugin
 * FILE:        Plugin/Command.cs
 * PURPOSE:     Describes the Commands the Plugin can handle
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global


using System.Collections.Generic;

namespace Plugin
{
    /// <summary>
    ///     Describes the command, the role and possible Input values
    /// </summary>
    public sealed class Command
    {
        /// <summary>
        ///     Gets the input.
        ///     The  input is numbered and accesses the DataRegister by number as an identifier.
        ///     Also optional.
        /// </summary>
        /// <value>
        ///     The input.
        /// </value>
        public List<int> Input { get; init; }

        /// <summary>
        ///     Gets the description.
        ///     Optional Information
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description { get; init; }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="Command" /> has a return value.
        ///     Optional
        /// </summary>
        /// <value>
        ///     <c>true</c> if return; otherwise, <c>false</c>.
        /// </value>
        public bool Return { get; init; }
    }
}
