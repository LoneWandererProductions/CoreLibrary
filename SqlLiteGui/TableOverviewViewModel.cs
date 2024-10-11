/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/TableOverviewViewModel.cs
 * PURPOSE:     View Model for TableOverview
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using ViewModel;

namespace SQLiteGui
{
    /// <summary>
    /// View Model
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class TableOverviewViewModel : INotifyPropertyChanged
    {
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
        /// Gets or sets the tables.
        /// </summary>
        /// <value>
        /// The tables.
        /// </value>
        internal ObservableCollection<TableDetails> Tables { get; set; }

        /// <summary>
        /// The selected table
        /// </summary>
        private TableDetails _selectedTable;

        /// <summary>
        /// Gets or sets the selected table.
        /// </summary>
        internal TableDetails SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (_selectedTable != value)
                {
                    _selectedTable = value;
                    OnPropertyChanged(nameof(SelectedTable));
                }
            }
        }

        /// <summary>
        /// Element added,deleted,updated
        /// </summary>
        internal EventHandler refreshDatabase;

        // Commands

        /// <summary>
        /// Gets the truncate table command.
        /// </summary>
        /// <value>
        /// The truncate table command.
        /// </value>
        public ICommand TruncateTableCommand { get; }

        /// <summary>
        /// Gets the drop table command.
        /// </summary>
        /// <value>
        /// The drop table command.
        /// </value>
        public ICommand DropTableCommand { get; }

        /// <summary>
        /// Gets the copy table command.
        /// </summary>
        /// <value>
        /// The copy table command.
        /// </value>
        public ICommand CopyTableCommand { get; }

        /// <summary>
        /// Gets the rename table command.
        /// </summary>
        /// <value>
        /// The rename table command.
        /// </value>
        public ICommand RenameTableCommand { get; }

        /// <summary>
        /// Gets the add table command.
        /// </summary>
        /// <value>
        /// The add table command.
        /// </value>
        public ICommand AddTableCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableOverviewViewModel"/> class.
        /// </summary>
        public TableOverviewViewModel()
        {
            // Initialize collections and commands
            Tables = new ObservableCollection<TableDetails>();

            // Pass the selected table as a parameter to each command
            TruncateTableCommand = new DelegateCommand<TableDetails>(TruncateTable, CanExecuteCommand);
            DropTableCommand = new DelegateCommand<TableDetails>(DropTable, CanExecuteCommand);
            CopyTableCommand = new DelegateCommand<TableDetails>(CopyTable, CanExecuteCommand);
            RenameTableCommand = new DelegateCommand<TableDetails>(RenameTable, CanExecuteCommand);
            AddTableCommand = new DelegateCommand<object>(AddTable);
        }

        /// <summary>
        /// Determines whether this instance [can execute command] the specified selected table.
        /// Enable or disable the commands based on the selected table
        /// </summary>
        /// <param name="selectedTable">The selected table.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can execute command] the specified selected table; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecuteCommand(TableDetails selectedTable)
        {
            return selectedTable != null;
        }

        /// <summary>
        /// Truncates the table.
        /// </summary>
        /// <param name="selectedTable">The selected table.</param>
        private void TruncateTable(TableDetails selectedTable)
        {
            if (selectedTable != null)
            {
                // Logic to truncate the table
                var check = SqLiteGuiProcessing.TruncateTable(selectedTable.TableAlias);
                if (check)
                {
                    refreshDatabase?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Drops the table.
        /// </summary>
        /// <param name="selectedTable">The selected table.</param>
        private void DropTable(TableDetails selectedTable)
        {
            if (selectedTable != null)
            {
                // Logic to drop the table
                var check = SqLiteGuiProcessing.DropTable(selectedTable.TableAlias);
                if (check)
                {
                    refreshDatabase?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Copies the table.
        /// </summary>
        /// <param name="selectedTable">The selected table.</param>
        private void CopyTable(TableDetails selectedTable)
        {
            if (selectedTable != null)
            {
                // Logic to copy the table
                var check = SqLiteGuiProcessing.CopyTable(selectedTable.TableAlias);
                if (check)
                {
                    refreshDatabase?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Renames the table.
        /// </summary>
        /// <param name="selectedTable">The selected table.</param>
        private void RenameTable(TableDetails selectedTable)
        {
            if (selectedTable != null)
            {
                var check = SqLiteGuiProcessing.RenameTable(selectedTable.TableAlias);
                if (check)
                {
                    refreshDatabase?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Adds the table.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void AddTable(object param)
        {
            var check = SqLiteGuiProcessing.AddTable();
            if (check)
            {
                refreshDatabase?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
