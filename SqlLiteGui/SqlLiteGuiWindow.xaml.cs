/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/SQLiteGuiWindow.xaml.cs
 * PURPOSE:     SqlLite graphical Front-end
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.IO;
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
        ///     Operations done, wen die Window is loaded
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TableView.listBoxSelectionChanged += TableViewListBoxSelectionChanged;

            TableView.refreshTable += RefreshTable;
            TableDetailView.RefreshTable += RefreshTable;
            TableView.refreshDatabase += RefreshDatabase;

            //TODO improve
            _ = new SqLiteGuiProcessing(DbInfo);
        }

        /// <summary>
        ///     Create a new Database
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void NewDatabase_Click(object sender, RoutedEventArgs e)
        {
            var path = Dialogs.HandleFile(SqLiteGuiResource.DbFilter, false);
            //nothing was selected
            if (path.Length == 0)
            {
                return;
            }

            var location = Directory.GetParent(path)?.ToString();
            var dbName = Path.GetFileName(path);

            //Clean our Register
            Register.StartNew();
            SqLiteGuiProcessing.CreateDatabase(location, dbName);
        }

        /// <summary>
        ///     Open the Database
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void OpenDatabase_Click(object sender, RoutedEventArgs e)
        {
            Register.ActiveDb = Dialogs.HandleFile(SqLiteGuiResource.DbFilter, true);
            LoadDatabase();
        }

        /// <summary>
        ///     Display the logs of the Current Database
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void ViewLogs_Click(object sender, RoutedEventArgs e)
        {
            SqLiteGuiProcessing.ShowLogs();
        }

        /// <summary>
        ///     Close Windows
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Active Database changed
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void TableViewListBoxSelectionChanged(object sender, TableDetails e)
        {
            //Load the table
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
            //Refresh the table
            SqLiteGuiProcessing.SelectTable(Register.Tablealias);
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
            //Refresh Data table
            LoadDatabase();
            //Clean Table Overview
            TableView.SetData();
            //Clean Detail View
            TableDetailView.SetData();
        }

        /// <summary>
        ///     Load/Reload Database, doesn't work quite right with drop Table
        /// </summary>
        private void LoadDatabase()
        {
            //Open our Database
            SqLiteGuiProcessing.OpenDatabase(Register.ActiveDb);
            //Clean our Register
            Register.StartNew();
            //Load into ViewModel
            TableView.SetData();
        }
    }
}
