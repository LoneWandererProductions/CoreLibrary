/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        ItemColors.xaml.cs
 * PURPOSE:     UserControl, that holds the ItemColor controls
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ViewModel;

namespace Debugger
{
    internal sealed class ItemColorsView : ViewModelBase
    {
        /// <summary>
        ///     Gets the delete command.
        /// </summary>
        /// <value>
        ///     The delete command.
        /// </value>
        public ICommand AddCommand =>
            new DelegateCommand<object>(AddAction, CanExecute);

        /// <summary>
        ///     Gets or sets the reference.
        /// </summary>
        /// <value>
        ///     The reference.
        /// </value>
        public ItemColors Reference { get; internal set; }

        /// <summary>
        ///     Gets or sets the filter.
        /// </summary>
        /// <value>
        ///     The filter.
        /// </value>
        public Dictionary<int, ItemColor> Filter { get; internal set; }

        /// <summary>
        ///     Adds the action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void AddAction(object obj)
        {
            Reference.AddFilter();
        }

        /// <summary>
        ///     Done action.
        /// </summary>
        /// <returns>All Color Options that were generated.</returns>
        public List<ColorOption> GetOption()
        {
            var options = new List<ColorOption>(Filter.Count);
            options.AddRange(Filter.Values.Select(filter => filter.View.Options));

            return options;
        }
    }
}
