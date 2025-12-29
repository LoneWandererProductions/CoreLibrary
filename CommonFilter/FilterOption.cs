/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonFilter
 * FILE:        CommonFilter/FilterOption.cs
 * PURPOSE:     Filter Object
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace CommonFilter
{
    /// <summary>
    ///     Filter Options
    /// </summary>
    public sealed class FilterOption
    {
        /// <summary>
        ///     Gets or sets the selected compare operator.
        /// </summary>
        /// <value>
        ///     The selected operator.
        /// </value>
        internal CompareOperator SelectedCompareOperator { get; init; }

        /// <summary>
        ///     Gets or sets the logical operator options.
        /// </summary>
        /// <value>
        ///     The logical operator options.
        /// </value>
        internal LogicOperator SelectedLogicalOperator { get; init; }

        /// <summary>
        ///     Gets or sets the entry text.
        /// </summary>
        /// <value>
        ///     The entry text.
        /// </value>
        internal string EntryText { get; init; }
    }
}
