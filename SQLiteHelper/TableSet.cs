/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        TableSet.cs
 * PURPOSE:     Used in add Statement and Select
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal

using System.Collections.Generic;

namespace SqliteHelper
{
    /// <summary>
    ///     Used for the Add Statement
    /// </summary>
    public sealed class TableSet
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TableSet" /> class.
        /// </summary>
        public TableSet()
        {
            Row = new List<string>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TableSet" /> class.
        /// </summary>
        /// <param name="lst">The list.</param>
        public TableSet(IEnumerable<string> lst)
        {
            Row = new List<string>(lst);
        }

        /// <summary>
        ///     Gets or sets the row.
        /// </summary>
        public List<string?> Row { get; init; }
    }
}
