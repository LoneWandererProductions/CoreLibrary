using System;
using System.Collections.Generic;

namespace CommonFilter
{
    /// <summary>
    ///     Filter Interface implementation
    /// </summary>
    /// <seealso cref="IFilter" />
    public sealed class Filter : IFilter
    {
        /// <summary>
        ///     The filter
        /// </summary>
        private FilterWindow _filter;

        /// <summary>
        ///     Gets or sets the options.
        /// </summary>
        /// <value>
        ///     The options.
        /// </value>
        internal static List<FilterOption> Options { get; set; }

        /// <summary>
        ///     Occurs when [filter changed].
        /// </summary>
        public event EventHandler FilterChanged;

        /// <summary>
        ///     Starts this instance.
        /// </summary>
        public void Start()
        {
            _filter = new FilterWindow(this);
            _filter.ShowDialog();
        }

        /// <summary>
        ///     Checks the filter.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Contition fullfilled?</returns>
        public bool CheckFilter(string input)
        {
            return false;
        }

        /// <summary>
        ///     Done.
        /// </summary>
        internal void Done()
        {
            FilterChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
