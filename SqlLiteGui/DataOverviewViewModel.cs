/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        DataOverviewViewModel.cs
 * PURPOSE:     DataOverview View Model
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Data;
using System.Windows.Input;
using ViewModel;

namespace SQLiteGui
{
    /// <inheritdoc />
    /// <summary>
    ///     View Model for the Table Data
    /// </summary>
    /// <seealso cref="T:ViewModel.ViewModelBase" />
    public sealed class DataOverviewViewModel : ViewModelBase
    {
        /// <summary>
        ///     The current table
        /// </summary>
        private string _currentTable;

        /// <summary>
        /// The raw
        /// </summary>
        private DataView _raw;

        /// <summary>
        ///     The selected item
        /// </summary>
        private DataRowView _selectedItem;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataOverviewViewModel" /> class.
        /// </summary>
        public DataOverviewViewModel()
        {
            UpdateCommand = new DelegateCommand<object>(UpdateItem, CanEditItem);
            DeleteCommand = new DelegateCommand<object>(DeleteItem, CanEditItem);
            AddCommand = new DelegateCommand<DataView>(AddItem);
        }

        /// <summary>
        ///     Gets or sets the raw.
        /// </summary>
        /// <value>
        ///     The raw.
        /// </value>
        public DataView Raw
        {
            get => _raw;
            set
            {
                if (_raw == value)
                {
                    return;
                }

                _raw = value;
                OnPropertyChanged(nameof(Raw)); // Notify UI of changes
            }
        }

        /// <summary>
        ///     Gets or sets the selected item.
        /// </summary>
        /// <value>
        ///     The selected item.
        /// </value>
        public DataRowView SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem.Equals(value))
                {
                    return;
                }

                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem)); // Notify UI when selection changes
            }
        }

        /// <summary>
        ///     Gets the update command.
        /// </summary>
        /// <value>
        ///     The update command.
        /// </value>
        public ICommand UpdateCommand { get; }

        /// <summary>
        ///     Gets the delete command.
        /// </summary>
        /// <value>
        ///     The delete command.
        /// </value>
        public ICommand DeleteCommand { get; }

        /// <summary>
        ///     Gets the add command.
        /// </summary>
        /// <value>
        ///     The add command.
        /// </value>
        public ICommand AddCommand { get; }

        /// <summary>
        ///     Change Data.
        /// </summary>
        /// <param name="selectedTable">The selected table.</param>
        internal void DataChanged(TableDetails selectedTable)
        {
            _currentTable = selectedTable.TableAlias;

            Raw = SqLiteGuiProcessing.SelectTable(selectedTable.TableAlias);
        }

        /// <summary>
        ///     Update View
        /// </summary>
        private void DataChanged()
        {
            if (string.IsNullOrWhiteSpace(_currentTable))
            {
                return;
            }

            Raw = SqLiteGuiProcessing.SelectTable(_currentTable);
        }

        /// <summary>
        ///     Updates the item.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void UpdateItem(object parameter)
        {
            // Update logic here using the provided parameter if needed
            // After updating, refresh the Items collection if necessary
            Register.SelectionChanged(true, SelectedItem);
            var check = SqLiteGuiProcessing.UpdateItem();

            Register.Info.AppendInfo(check ? "Entry was updated." : "Entry was not updated.");
            DataChanged();
        }

        /// <summary>
        ///     Determines whether this instance [can update item] the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>
        ///     <c>true</c> if this instance [can update item] the specified parameter; otherwise, <c>false</c>.
        /// </returns>
        private bool CanEditItem(object parameter)
        {
            // Determine if the item can be updated
            return SelectedItem != null;
            // Return true or false based on your logic
        }

        /// <summary>
        ///     Deletes the item.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void DeleteItem(object parameter)
        {
            // Delete logic here using the provided parameter if needed
            // After deleting, refresh the Items collection if necessary
            Register.SelectionChanged(true, SelectedItem);
            var check = SqLiteGuiProcessing.DeleteItem();

            Register.Info.AppendInfo(check ? "Entry was deleted." : "Entry was not deleted.");

            DataChanged();
        }

        /// <summary>
        ///     Adds the item.
        /// </summary>
        /// <param name="item">The item.</param>
        private void AddItem(DataView item)
        {
            // Add logic here
            // Example: you may want to create a new DataView and add it
            var check = SqLiteGuiProcessing.AddItem();

            Register.Info.AppendInfo(check ? "Entry was Added." : "Entry was not Added.");
            DataChanged();
        }
    }
}
