/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/ItemColors.xaml.cs
 * PURPOSE:     UserControl, that holds the ItemColor controls
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;

namespace Debugger
{
    /// <summary>
    ///     ItemColors UserControl
    /// </summary>
    public partial class ItemColors
    {
        /// <summary>
        ///     The filter option
        /// </summary>
        private readonly Dictionary<int, ItemColor> _filterOption = new();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemColors" /> class.
        /// </summary>
        public ItemColors()
        {
            InitializeComponent();
            View.Reference = this;
            View.Filter = _filterOption;
            _filterOption = new Dictionary<int, ItemColor>();
            AddFilter();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemColors"/> class.
        /// </summary>
        /// <param name="filterOption">The filter options we want to display.</param>
        public ItemColors(List<ColorOption> filterOption)
        {
            InitializeComponent();
            View.Reference = this;
            _filterOption = new Dictionary<int, ItemColor>();
            AddFilter(filterOption);
        }

        /// <summary>
        ///     Adds the filter.
        /// </summary>
        public void AddFilter()
        {
            var id = GetFirstAvailableIndex(_filterOption.Keys.ToList());

            var itemControl = new ItemColor(id);
            itemControl.DeleteLogic += ItemControl_DeleteLogic;
            _ = ColorList.Items.Add(itemControl);
            _filterOption.Add(id, itemControl);
        }

        /// <summary>
        ///     Items the control delete logic.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="id">The identifier.</param>
        private void ItemControl_DeleteLogic(object sender, int id)
        {
            if (id == 0)
            {
                return;
            }

            ColorList.Items.Remove(sender);
            _filterOption.Remove(id);
        }

        /// <summary>
        /// Gets the color options.
        /// </summary>
        /// <returns>List of Color Options</returns>
        internal List<ColorOption> GetColorOptions()
        {
            return _filterOption.Values.Select(option => option.GetOption()).ToList();
        }

        /// <summary>
        /// Adds the filter.
        /// </summary>
        /// <param name="filterOption">The filter option.</param>
        private void AddFilter(List<ColorOption> filterOption)
        {
            foreach (var item in filterOption)
            {
                var id = GetFirstAvailableIndex(_filterOption.Keys.ToList());

                var itemControl = new ItemColor(id, item);
                itemControl.DeleteLogic += ItemControl_DeleteLogic;
                _ = ColorList.Items.Add(itemControl);
                _filterOption.Add(id, itemControl);
            }
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
    }
}
