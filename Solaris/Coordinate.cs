/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Solaris/Cursor.cs
 * PURPOSE:     Object that contains information about the click on the display
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Solaris
{
    /// <summary>
    ///     Holds the coordinates on the screen
    /// </summary>
    public sealed class Cursor
    {
        /// <summary>
        ///     Gets or sets the x.
        /// </summary>
        /// <value>
        ///     The x.
        /// </value>
        public int X { get; set; }

        /// <summary>
        ///     Gets or sets the y.
        /// </summary>
        /// <value>
        ///     The y.
        /// </value>
        public int Y { get; set; }
    }
}
