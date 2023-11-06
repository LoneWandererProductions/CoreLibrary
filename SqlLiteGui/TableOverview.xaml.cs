/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        MenuBand.xaml.cs
 * PURPOSE:     SqlLite graphical Front-end, User-control for the Table Part
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal, can't be made internal since we need it public as User-control

using System;
using System.Windows;
using System.Windows.Controls;

namespace SQLiteGui
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     General Overview Window of the Tables inside the Database
    /// </summary>
    public sealed partial class TableOverview
    {
        /// <summary>
        ///     Event Handler for Selection Change
        /// </summary>
        internal EventHandler<TableDetails> listBoxSelectionChanged;

        /// <summary>
        ///     Element added,deleted,updated
        /// </summary>
        internal EventHandler refreshDatabase;

        /// <summary>
        ///     Element added,deleted,updated
        /// </summary>
        internal EventHandler refreshTable;

        /// <inheritdoc />
        /// <summary>
        ///     Initiate Table Overview
        /// </summary>
        public TableOverview()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Initiate Data
        /// </summary>
        internal void SetData()
        {
            Tables.ItemsSource = TablesViewModel.Item;
        }

        /// <summary>
        ///     Get Selected Item
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void DataGrd_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetSelectedItem();
        }

        /// <summary>
        ///     Truncate Table
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void TableTruncate_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedItem();
            var check = SqLiteGuiProcessing.TruncateTable();
            if (check)
            {
                refreshTable?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Drop Table
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void TableDrop_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedItem();
            var check = SqLiteGuiProcessing.DropTable();
            if (check)
            {
                refreshDatabase?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Copy Table
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void TableCopy_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedItem();
            var check = SqLiteGuiProcessing.CopyTable();
            if (check)
            {
                refreshDatabase?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Rename Table
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void TableRename_Click(object sender, RoutedEventArgs e)
        {
            GetSelectedItem();
            var check = SqLiteGuiProcessing.RenameTable();
            if (check)
            {
                refreshDatabase?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Add a new Table
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void AddTable_Click(object sender, RoutedEventArgs e)
        {
            var check = SqLiteGuiProcessing.AddTable();
            if (check)
            {
                refreshDatabase?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Notify Subscribers of the Change
        /// </summary>
        /// <param name="args">Data of the selected Element</param>
        private void OnListBoxSelectionChanged(TableDetails args)
        {
            listBoxSelectionChanged?.Invoke(this, args);
        }

        /// <summary>
        ///     Define Selected Item
        /// </summary>
        private void GetSelectedItem()
        {
            //TODO refine this is ugly
            var cellInfo = Tables.SelectedCells[0];

            if (cellInfo.Column.GetCellContent(cellInfo.Item) is not TextBlock content)
            {
                return;
            }

            var tbl = new TableDetails {TableAlias = content.Text};

            OnListBoxSelectionChanged(tbl);
        }
    }
}
