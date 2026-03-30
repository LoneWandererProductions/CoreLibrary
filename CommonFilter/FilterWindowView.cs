/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonFilter
 * FILE:        CommonFilter/FilterWindowView.cs
 * PURPOSE:     View for Filter Window
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ViewModel;

namespace CommonFilter
{
    /// <inheritdoc />
    /// <summary>
    ///     FilterWindow View
    /// </summary>
    /// <seealso cref="T:System.ComponentModel.INotifyPropertyChanged" />
    internal sealed class FilterWindowView : ViewModelBase
    {
        /// <summary>
        ///     Gets or sets the reference.
        /// </summary>
        /// <value>
        ///     The reference.
        /// </value>
        public FilterWindow Reference { get; set; } = null!;

        /// <summary>
        ///     Gets the add command.
        /// </summary>
        /// <value>
        ///     The add command.
        /// </value>
        public ICommand AddCommand =>
            new DelegateCommand<object>(AddAction, CanExecute);

        /// <summary>
        ///     Gets the done command.
        /// </summary>
        /// <value>
        ///     The done command.
        /// </value>
        public ICommand DoneCommand =>
            new DelegateCommand<object>(DoneAction, CanExecute);

        /// <summary>
        ///     Gets or sets the filter.
        /// </summary>
        /// <value>
        ///     The filter.
        /// </value>
        public Dictionary<int, SearchParameterControl> Filter { get; set; }

        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        /// <value>
        /// The filters.
        /// </value>
        public ObservableCollection<SearchParameterViewModel> Filters { get; set; }
            = new ObservableCollection<SearchParameterViewModel>();

        /// <summary>
        ///     Adds action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void AddAction(object obj)
        {
            var newItem = new SearchParameterViewModel(RemoveItem);
            Filters.Add(newItem);
        }


        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="item">The item.</param>
        private void RemoveItem(SearchParameterViewModel item)
        {
            Filters.Remove(item);
        }

        /// <summary>
        /// Done action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void DoneAction(object obj)
        {
            var options = Filters.Select(filter => filter.Options).ToList();
            Reference.GetConditions(options);
        }
    }
}
