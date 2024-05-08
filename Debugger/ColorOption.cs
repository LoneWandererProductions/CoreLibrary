/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/ColorOption.cs
 * PURPOSE:     Config Object that handles the colors of the Text of the Debugger
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;

namespace Debugger
{
    /// <summary>
    ///     Container that will hold the text and the corresponding color
    /// </summary>
    public sealed class ColorOption
    {
        /// <summary>
        ///     Gets the name of the color.
        /// </summary>
        /// <value>
        ///     The name of the color.
        /// </value>
        internal string ColorName { get; init; }

        /// <summary>
        ///     Gets or sets the entry text.
        /// </summary>
        /// <value>
        ///     The entry text.
        /// </value>
        internal string EntryText { get; init; }
    }
}
