/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Core
 * FILE:        CommandExtension.cs
 * PURPOSE:     Extension information for commands.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.Core
{
    /// <summary>
    /// Defines how an Extension for a command looks like.
    /// </summary>
    public sealed class CommandExtension
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; init; } = "";

        /// <summary>
        /// Gets the parameter count.
        /// </summary>
        /// <value>
        /// The parameter count.
        /// </value>
        public int ParameterCount { get; init; }

        /// <summary>
        /// Gets a value indicating whether this instance is internal.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is internal; otherwise, <c>false</c>.
        /// </value>
        public bool IsInternal { get; init; }

        /// <summary>
        /// Gets a value indicating whether this instance is preview.
        /// Only used for tryrun commands.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is preview; otherwise, <c>false</c>.
        /// </value>
        public bool IsPreview { get; init; }
    }
}