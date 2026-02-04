/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        TableOverviewViewModel.cs
 * PURPOSE:     View Model for TableOverview
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ViewModel;

namespace SQLiteGui
{
    /// <inheritdoc />
    /// <summary>
    ///     View Model
    /// </summary>
    /// <seealso cref="ViewModelBase" />
    public sealed class TableOverviewViewModel : ViewModelBase
    {
        /// <summary>
        ///     The selected table
        /// </summary>
        private TableDetails _selectedTable;

        /// <summary>
        ///     The tables
        /// </summary>
        private ObservableCollection<TableDetails> _tables;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TableOverviewViewModel" /> class.
        /// </summary>
        /// <param name="dataOverviewViewModel">The data overview view model.</param>
        public TableOverviewViewModel(DataOverviewViewModel dataOverviewViewModel)
        {
            // Initialize collections and commands
            Tables = new ObservableCollection<TableDetails>();
            //add our Reference to the Data Overview
            DataOverviewViewModel = dataOverviewViewModel;

            // Pass the selected table as a parameter to each command
            TruncateTableCommand = new DelegateCommand<TableDetails>(TruncateTable, CanExecuteCommand);
            DropTableCommand = new DelegateCommand<TableDetails>(DropTable, CanExecuteCommand);
            CopyTableCommand = new DelegateCommand<TableDetails>(CopyTable, CanExecuteCommand);
            RenameTableCommand = new DelegateCommand<TableDetails>(RenameTable, CanExecuteCommand);
            AddTableCommand = new DelegateCommand<TableDetails>(AddTable);
        }

        /// <summary>
        ///     Gets the data overview view model.
        /// </summary>
        /// <value>
        ///     The data overview view model.
        /// </value>
        public DataOverviewViewModel DataOverviewViewModel { get; }

        /// <summary>
        ///     Gets or sets the tables.
        /// </summary>
        /// <value>
        ///     The tables.
        /// </value>
        public ObservableCollection<TableDetails> Tables
        {
            get => _tables;
            set
            {
                if (_tables == value)
                {
                    return;
                }

                _tables = value;
                OnPropertyChanged(nameof(Tables)); // Notify that the collection has changed
            }
        }

        /// <summary>
        ///     Gets or sets the selected table.
        /// </summary>
        public TableDetails SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (_selectedTable == value)
                {
                    return;
                }

                _selectedTable = value;
                OnPropertyChanged(nameof(SelectedTable));
                OnSelectedTableChanged(); // Custom logic when selection changes
            }
        }

        /// <summary>
        ///     Gets the truncate table command.
        /// </summary>
        /// <value>
        ///     The truncate table command.
        /// </value>
        public ICommand TruncateTableCommand { get; }

        /// <summary>
        ///     Gets the drop table command.
        /// </summary>
        /// <value>
        ///     The drop table command.
        /// </value>
        public ICommand DropTableCommand { get; }

        /// <summary>
        ///     Gets the copy table command.
        /// </summary>
        /// <value>
        ///     The copy table command.
        /// </value>
        public ICommand CopyTableCommand { get; }

        /// <summary>
        ///     Gets the rename table command.
        /// </summary>
        /// <value>
        ///     The rename table command.
        /// </value>
        public ICommand RenameTableCommand { get; }

        /// <summary>
        ///     Gets the add table command.
        /// </summary>
        /// <value>
        ///     The add table command.
        /// </value>
        public ICommand AddTableCommand { get; }

        /// <summary>
        ///     Sets the tables.
        /// </summary>
        /// <param name="tables">The tables.</param>
        internal void SetTables(IEnumerable<TableDetails> tables)
        {
            Tables = new ObservableCollection<TableDetails>(tables);
        }

        /// <summary>
        ///     Determines whether this instance [can execute command] the specified selected table.
        ///     Enable or disable the commands based on the selected table
        /// </summary>
        /// <param name="selectedTable">The selected table.</param>
        /// <returns>
        ///     <c>true</c> if this instance [can execute command] the specified selected table; otherwise, <c>false</c>.
        /// </returns>
        private bool CanExecuteCommand(TableDetails selectedTable)
        {
            return selectedTable != null;
        }

        /// <summary>
        ///     Truncates the table.
        /// </summary>
        /// <param name="selectedTable">The selected table.</param>
        private void TruncateTable(TableDetails selectedTable)
        {
            // Logic to truncate the table
            var check = SqLiteGuiProcessing.TruncateTable(selectedTable.TableAlias);

            Register.Info.AppendInfo(check ? "Table was truncated." : "Table was not truncated.");
            DataChanged();
        }

        /// <summary>
        ///     Drops the table.
        /// </summary>
        /// <param name="selectedTable">The selected table.</param>
        private void DropTable(TableDetails selectedTable)
        {
            // Logic to drop the table
            var check = SqLiteGuiProcessing.DropTable(selectedTable.TableAlias);

            Register.Info.AppendInfo(check ? "Table was dropped." : "Table was not dropped.");
            DataChanged();
        }

        /// <summary>
        ///     Copies the table.
        /// </summary>
        /// <param name="selectedTable">The selected table.</param>
        private void CopyTable(TableDetails selectedTable)
        {
            // Logic to copy the table
            var check = SqLiteGuiProcessing.CopyTable(selectedTable.TableAlias);
            Register.Info.AppendInfo(check ? "Table was copied." : "Table was not copied.");
            DataChanged();
        }

        /// <summary>
        ///     Renames the table.
        /// </summary>
        /// <param name="selectedTable">The selected table.</param>
        private void RenameTable(TableDetails selectedTable)
        {
            var check = SqLiteGuiProcessing.RenameTable(selectedTable.TableAlias);
            Register.Info.AppendInfo(check ? "Table was renamed." : "Table was not renamed.");
            DataChanged();
        }

        /// <summary>
        ///     Adds the table.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void AddTable(object param)
        {
            var check = SqLiteGuiProcessing.AddTable();
            Register.Info.AppendInfo(check ? "Table was added." : "Table was not added.");
            DataChanged();
        }

        /// <summary>
        ///     Updates the View of the Tables
        /// </summary>
        private void DataChanged()
        {
            var tables = SqLiteGuiProcessing.GetTableDetails();
            Tables = new ObservableCollection<TableDetails>(tables);
        }

        /// <summary>
        ///     Called when [selected table changed].
        ///     This method is called whenever the SelectedTable changes
        /// </summary>
        private void OnSelectedTableChanged()
        {
            if (SelectedTable != null)
            {
                DataOverviewViewModel.DataChanged(SelectedTable);
            }
        }
    }
}
