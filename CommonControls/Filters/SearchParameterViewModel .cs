﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/Filters/SearchParameterViewModel.xaml.cs
 * PURPOSE:     
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using ViewModel;

namespace CommonControls.Filters
{
    /// <inheritdoc />
    /// <summary>
    ///     Search View
    /// </summary>
    /// <seealso cref="T:System.ComponentModel.INotifyPropertyChanged" />
    internal sealed class SearchParameterViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     The add command
        /// </summary>
        private ICommand _deleteCommand;

        /// <summary>
        ///     The entry text
        /// </summary>
        private string _entryText;

        /// <summary>
        ///     The selected logical operator
        /// </summary>
        private string _selectedLogicalOperator;

        /// <summary>
        ///     The selected operator
        /// </summary>
        private string _selectedOperator;

        /// <summary>
        ///     Gets the options.
        /// </summary>
        /// <value>
        ///     The options.
        /// </value>
        internal FilterOption Options => GetOptions();

        /// <summary>
        ///     Gets or sets the selected operator.
        /// </summary>
        /// <value>
        ///     The selected operator.
        /// </value>
        public string SelectedOperator
        {
            get => _selectedOperator;
            set
            {
                _selectedOperator = value;
                OnPropertyChanged(nameof(SelectedOperator));
            }
        }

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
        ///     Gets or sets the selected logical operator.
        /// </summary>
        /// <value>
        ///     The selected logical operator.
        /// </value>
        public string SelectedLogicalOperator
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
        public List<string> OperatorOptions { get; } = new() { "Like", "not Like" };

        /// <summary>
        ///     Gets the logical operator options.
        /// </summary>
        /// <value>
        ///     The logical operator options.
        /// </value>
        public List<string> LogicalOperatorOptions { get; } = new() { "And", "And not" };

        /// <summary>
        ///     Gets the okay command.
        /// </summary>
        /// <value>
        ///     The okay command.
        /// </value>
        public ICommand DeleteCommand =>
            _deleteCommand ??= new DelegateCommand<object>(DeleteAction, CanExecute);

        /// <summary>
        ///     Gets or sets the reference.
        /// </summary>
        /// <value>
        ///     The reference.
        /// </value>
        public SearchParameterControl Reference { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Triggers if an Attribute gets changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Gets the options.
        /// </summary>
        /// <returns>All selected parameter</returns>
        private FilterOption GetOptions()
        {
            return new FilterOption
            {
                SelectedLogicalOperator = SelectedLogicalOperator,
                SelectedOperator = SelectedOperator,
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
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Deletes the action.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void DeleteAction(object obj)
        {
            // Perform delete action logic
            // Raise the event
            Reference.DeleteClicked();
        }
    }
}
