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
        /// <param name="filter"></param>
        public FilterWindow(Filter filter)
        {
            InitializeComponent();
            View.Reference = this;
            View.Filter = new Dictionary<int, SearchParameterControl>();
            _interface = filter;
            AddFilter();
        }

        /// <summary>
        ///     Adds the filter.
        /// </summary>
        public void AddFilter()
        {
            var id = GetFirstAvailableIndex(View.Filter.Keys.ToList());

            var searchParameterControl = new SearchParameterControl(id);
            searchParameterControl.DeleteLogic += SearchParameterControl_DeleteLogic;

            // Add the control to the ListBox's Items
            _ = FilterList.Items.Add(searchParameterControl);

            View.Filter.Add(id, searchParameterControl);
        }

        /// <summary>
        ///     Gets the first index of the available.
        ///     See ExtendedSystemObjects.
        /// </summary>
        /// <param name="lst">The List of elements.</param>
        /// <returns>First available free id.</returns>
        private static int GetFirstAvailableIndex(IEnumerable<int> lst)
        {
            return Enumerable.Range(0, int.MaxValue)
                .Except(lst)
                .FirstOrDefault();
        }

        /// <summary>
        ///     Searches the parameter control delete logic.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="id">The identifier.</param>
        private void SearchParameterControl_DeleteLogic(object sender, int id)
        {
            if (id == 0)
            {
                return;
            }

            FilterList.Items.Remove(sender);
            View.Filter.Remove(id);
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
