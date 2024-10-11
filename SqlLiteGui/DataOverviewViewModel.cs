/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/DataOverviewViewModel.cs
 * PURPOSE:     DataOverview View Model
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows.Input;
using ViewModel;

namespace SQLiteGui
{
    public class DataOverviewViewModel : INotifyPropertyChanged
    {
        /// <summary>
        ///     Element added,deleted,updated
        /// </summary>
        internal EventHandler refreshTable;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public ObservableCollection<DataView> Items { get; } = new ObservableCollection<DataView>();

        /// <summary>
        /// The selected item
        /// </summary>
        private DataView _selectedItem;

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>
        /// The selected item.
        /// </value>
        public DataView SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged(nameof(SelectedItem));
            }
        }

        /// <summary>
        /// Gets the update command.
        /// </summary>
        /// <value>
        /// The update command.
        /// </value>
        public ICommand UpdateCommand { get; }

        /// <summary>
        /// Gets the delete command.
        /// </summary>
        /// <value>
        /// The delete command.
        /// </value>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Gets the add command.
        /// </summary>
        /// <value>
        /// The add command.
        /// </value>
        public ICommand AddCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataOverviewViewModel"/> class.
        /// </summary>
        public DataOverviewViewModel()
        {
            UpdateCommand = new DelegateCommand<object>(UpdateItem, CanUpdateItem);
            DeleteCommand = new DelegateCommand<object>(DeleteItem, CanDeleteItem);
            AddCommand = new DelegateCommand<DataView>(AddItem);

            // Load initial data if necessary
            LoadData();
        }

        /// <summary>
        /// Loads the data.
        /// </summary>
        private void LoadData()
        {
            // Load your data into the Items collection
        }

        /// <summary>
        /// Updates the item.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void UpdateItem(object parameter)
        {
            // Update logic here using the provided parameter if needed
            // After updating, refresh the Items collection if necessary
            if (SelectedItem == null)
            {
                Register.SelectionChanged(true, SelectedItem);
                var check = SqLiteGuiProcessing.UpdateItem();

                if (check)
                {
                    refreshTable?.Invoke(this, EventArgs.Empty);

                }
            }
        }

        /// <summary>
        /// Determines whether this instance [can update item] the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can update item] the specified parameter; otherwise, <c>false</c>.
        /// </returns>
        private bool CanUpdateItem(object parameter)
        {
            // Determine if the item can be updated
            // Return true or false based on your logic
            return true; // Adjust based on your criteria
        }

        /// <summary>
        /// Deletes the item.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void DeleteItem(object parameter)
        {
            // Delete logic here using the provided parameter if needed
            // After deleting, refresh the Items collection if necessary
            var check = SqLiteGuiProcessing.DeleteItem();

            if (check)
            {
                refreshTable?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Determines whether this instance [can delete item] the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can delete item] the specified parameter; otherwise, <c>false</c>.
        /// </returns>
        private bool CanDeleteItem(object parameter)
        {
            // Determine if the item can be deleted
            // Return true or false based on your logic
            return true; // Adjust based on your criteria
        }

        /// <summary>
        /// Adds the item.
        /// </summary>
        /// <param name="item">The item.</param>
        private void AddItem(DataView item)
        {
            // Add logic here
            // Example: you may want to create a new DataView and add it
            var check = SqLiteGuiProcessing.AddItem();

            if (check)
            {
                refreshTable?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
