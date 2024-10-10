/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/SQLiteGuiWindow.xaml.cs
 * PURPOSE:     SqlLite graphical Front-end, not yet complety cleared out into an View
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Windows;

namespace SQLiteGui
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     Starter Window, here we forward all Interactions
    /// </summary>
    public sealed partial class SqLiteGuiWindow
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:SQLiteGui.SQLiteGuiWindow" /> class.
        /// </summary>
        public SqLiteGuiWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Loaded event of the SqLiteGuiWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void SqLiteGuiWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Subscribe to events for TableView and TableDetailView
            TableView.listBoxSelectionChanged += TableViewListBoxSelectionChanged;
            TableDetailView.RefreshTable += RefreshTable;
            TableView.refreshDatabase += RefreshDatabase;

            // Initialize your processing here if needed
            _ = new SqLiteGuiProcessing(DbInfo);
        }

        /// <summary>
        ///     Active Database changed
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void TableViewListBoxSelectionChanged(object sender, TableDetails e)
        {
            // Load the table
            SqLiteGuiProcessing.SelectTable(e.TableAlias);
            TableDetailView.SetData();
        }

        /// <summary>
        ///     Load Table again
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void RefreshTable(object sender, EventArgs e)
        {
            // Refresh the table
            SqLiteGuiProcessing.SelectTable(Register.TableAlias);
            TableDetailView.SetData();
        }

        /// <summary>
        ///     Refresh Grid and Table, a bit Hack but it works
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void RefreshDatabase(object sender, EventArgs e)
        {
            SqLiteGuiProcessing.OpenDatabase(Register.ActiveDb);
            // Refresh Data table
            LoadDatabase();
            // Clean Table Overview
            TableView.SetData();
            // Clean Detail View
            TableDetailView.SetData();
        }

        /// <summary>
        ///     Load/Reload Database
        /// </summary>
        private void LoadDatabase()
        {
            // Open our Database
            SqLiteGuiProcessing.OpenDatabase(Register.ActiveDb);
            // Clean our Register
            Register.StartNew();
            // Load into ViewModel
            TableView.SetData();
        }
    }
}
