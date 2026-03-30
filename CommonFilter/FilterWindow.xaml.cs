/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonFilter
 * FILE:        CommonFilter/FilterWindow.xaml.cs
 * PURPOSE:     Filter Window, Container for all Search Parameters
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;

namespace CommonFilter
{
    /// <inheritdoc cref="FilterWindow" />
    /// <summary>
    ///     Frontend for the filter
    /// </summary>
    internal sealed partial class FilterWindow
    {
        /// <summary>
        ///     The interface
        /// </summary>
        private readonly Filter _interface;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:CommonControls.Filters.FilterWindow" /> class.
        /// </summary>
        /// <param name="filter">The Filter</param>
        public FilterWindow(Filter filter)
        {
            InitializeComponent();
            View.Reference = this;
            _interface = filter;

            // Add the first blank filter row on startup
            View.AddCommand.Execute(null);
        }

        /// <summary>
        ///     Gets the conditions.
        /// </summary>
        /// <param name="conditions">The conditions.</param>
        public void GetConditions(List<FilterOption> conditions)
        {
            _interface.Conditions = conditions;
            _interface.Done();
        }
    }
}
