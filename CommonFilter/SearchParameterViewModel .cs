/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonFilter
 * FILE:        CommonFilter/SearchParameterViewModel.xaml.cs
 * PURPOSE:     View for SearchParameterControl
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Windows.Input;
using ViewModel;

namespace CommonFilter
{
    /// <inheritdoc />
    /// <summary>
    ///     Search View
    /// </summary>
    internal sealed class SearchParameterViewModel : ViewModelBase
    {
        /// <summary>
        ///     The entry text
        /// </summary>
        private string _entryText;

        /// <summary>
        ///     The selected compare operator
        /// </summary>
        private CompareOperator _selectedCompareOperator;

        /// <summary>
        ///     The selected logical operator
        /// </summary>
        private LogicOperator _selectedLogicalOperator;

        /// <summary>
        ///     Gets the options.
        /// </summary>
        /// <value>
        ///     The options.
        /// </value>
        internal FilterOption Options => GetOptions();

        /// <summary>
        /// The delete callback
        /// </summary>
        private readonly Action<SearchParameterViewModel> _deleteCallback;

        /// <summary>
        ///     Gets or sets the entry text.
        /// </summary>
        /// <value>
        ///     The entry text.
        /// </value>
        public string EntryText
        {
            get => _entryText;
            set
            {
                _entryText = value;
                OnPropertyChanged(nameof(EntryText));
            }
        }

        /// <summary>
        ///     Gets or sets the selected compare operator.
        /// </summary>
        /// <value>
        ///     The selected operator.
        /// </value>
        public CompareOperator SelectedCompareOperator
        {
            get => _selectedCompareOperator;
            set
            {
                _selectedCompareOperator = value;
                OnPropertyChanged(nameof(SelectedCompareOperator));
            }
        }

        /// <summary>
        ///     Gets or sets the selected logical operator.
        /// </summary>
        /// <value>
        ///     The selected logical operator.
        /// </value>
        public LogicOperator SelectedLogicalOperator
        {
            get => _selectedLogicalOperator;
            set
            {
                _selectedLogicalOperator = value;
                OnPropertyChanged(nameof(SelectedLogicalOperator));
            }
        }

        /// <summary>
        ///     Gets the operator options.
        /// </summary>
        /// <value>
        ///     The operator options.
        /// </value>
        public IEnumerable<CompareOperator> OperatorOptions =>
            Enum.GetValues<CompareOperator>();

        /// <summary>
        ///     Gets the logical operator options.
        /// </summary>
        /// <value>
        ///     The logical operator options.
        /// </value>
        public IEnumerable<LogicOperator> LogicalOperatorOptions =>
            Enum.GetValues<LogicOperator>();

        /// <summary>
        ///     Gets the delete command.
        /// </summary>
        /// <value>
        ///     The delete command.
        /// </value>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchParameterViewModel"/> class.
        /// </summary>
        /// <param name="deleteCallback">The delete callback.</param>
        public SearchParameterViewModel(Action<SearchParameterViewModel> deleteCallback)
        {
            _deleteCallback = deleteCallback;
            DeleteCommand = new DelegateCommand<object>(DeleteAction, CanExecute);
        }

        /// <summary>
        ///     Gets the options.
        /// </summary>
        /// <returns>All selected parameter</returns>
        private FilterOption GetOptions()
        {
            return new FilterOption
            {
                SelectedLogicalOperator = SelectedLogicalOperator,
                SelectedCompareOperator = SelectedCompareOperator,
                EntryText = EntryText
            };
        }

        /// <summary>
        ///     Gets a value indicating whether this instance can execute.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///     <c>true</c> if this instance can execute the specified object; otherwise, <c>false</c>.
        /// </returns>
        /// <value>
        ///     <c>true</c> if this instance can execute; otherwise, <c>false</c>.
        /// </value>
        public bool CanExecute(object obj)
        {
            // check if executing is allowed, not used right now
            return true;
        }

        /// <summary>
        ///     Deletes the action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void DeleteAction(object obj)
        {
            _deleteCallback?.Invoke(this);
        }
    }
}
