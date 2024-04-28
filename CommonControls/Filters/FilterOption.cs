/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/Filters/FilterOption.cs
 * PURPOSE:     Filter Object
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

namespace CommonControls.Filters
{
    /// <summary>
    /// Filter Options
    /// </summary>
    public sealed class FilterOption
    {
        /// <summary>
        /// Gets or sets the selected operator.
        /// </summary>
        /// <value>
        /// The selected operator.
        /// </value>
        internal string SelectedOperator { get; set; }

        /// <summary>
        /// Gets or sets the logical operator options.
        /// </summary>
        /// <value>
        /// The logical operator options.
        /// </value>
        internal List<string> LogicalOperatorOptions { get; set; }

        /// <summary>
        /// Gets or sets the entry text.
        /// </summary>
        /// <value>
        /// The entry text.
        /// </value>
        internal string EntryText { get; set; }
    }
}
