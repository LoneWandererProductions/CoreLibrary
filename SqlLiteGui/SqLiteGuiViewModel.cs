/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/SqLiteGuiViewModel .xaml.cs
 * PURPOSE:     View Model for: SQLiteGuiWindow
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.IO;
using System.Windows;
using DataFormatter;
using ViewModel;

namespace SQLiteGui
{
    /// <summary>
    /// View Model
    /// </summary>
    public class SqLiteGuiViewModel
    {
        /// <summary>
        /// Creates new databasecommand.
        /// </summary>
        /// <value>
        /// The new database command.
        /// </value>
        public DelegateCommand<object> NewDatabaseCommand { get; }

        /// <summary>
        /// Gets the open database command.
        /// </summary>
        /// <value>
        /// The open database command.
        /// </value>
        public DelegateCommand<object> OpenDatabaseCommand { get; }

        /// <summary>
        /// Gets the view logs command.
        /// </summary>
        /// <value>
        /// The view logs command.
        /// </value>
        public DelegateCommand<object> ViewLogsCommand { get; }

        /// <summary>
        /// Gets the close command.
        /// </summary>
        /// <value>
        /// The close command.
        /// </value>
        public DelegateCommand<object> CloseCommand { get; }

        /// <summary>
        /// Gets the import CSV command.
        /// </summary>
        /// <value>
        /// The import CSV command.
        /// </value>
        public DelegateCommand<object> ImportCsvCommand { get; }

        /// <summary>
        /// Gets the export CSV command.
        /// </summary>
        /// <value>
        /// The export CSV command.
        /// </value>
        public DelegateCommand<object> ExportCsvCommand { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqLiteGuiViewModel"/> class.
        /// </summary>
        public SqLiteGuiViewModel()
        {
            NewDatabaseCommand = new DelegateCommand<object>(NewDatabaseClick);
            OpenDatabaseCommand = new DelegateCommand<object>(OpenDatabaseClick);
            ViewLogsCommand = new DelegateCommand<object>(ViewLogsClick);
            CloseCommand = new DelegateCommand<object>(CloseClick);
            ImportCsvCommand = new DelegateCommand<object>(ImportCsvClick);
            ExportCsvCommand = new DelegateCommand<object>(ExportCsvClick);
        }

        /// <summary>
        /// Creates new database_click.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void NewDatabaseClick(object parameter)
        {
            var path = Dialogs.HandleFile(SqLiteGuiResource.DbFilter, false);
            if (path.Length == 0) return;

            var location = Directory.GetParent(path)?.ToString();
            var dbName = Path.GetFileName(path);

            Register.StartNew();
            SqLiteGuiProcessing.CreateDatabase(location, dbName);
        }

        /// <summary>
        /// Opens the database click.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void OpenDatabaseClick(object parameter)
        {
            Register.ActiveDb = Dialogs.HandleFile(SqLiteGuiResource.DbFilter, true);
            LoadDatabase();
        }

        /// <summary>
        /// Views the logs click.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void ViewLogsClick(object parameter)
        {
            SqLiteGuiProcessing.ShowLogs();
        }

        /// <summary>
        /// Closes the click.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void CloseClick(object parameter)
        {
            Application.Current.Shutdown(); // Close the application properly
        }

        /// <summary>
        /// Imports the CSV click.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void ImportCsvClick(object parameter)
        {
            var csvFilePath = Dialogs.HandleFile("CSV files|*.csv", true);
            if (string.IsNullOrWhiteSpace(csvFilePath)) return;

            try
            {
                var csvData = CsvHandler.ReadCsv(csvFilePath, ',');
                if (csvData != null)
                {
                    var check = SqLiteGuiProcessing.LoadCsv(csvData);
                    MessageBox.Show(check ? "CSV Imported Successfully!" : "Error Importing CSV.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error importing CSV: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports the CSV click.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void ExportCsvClick(object parameter)
        {
            var savePath = Dialogs.HandleFile("CSV files|*.csv", false);
            if (string.IsNullOrWhiteSpace(savePath)) return;

            try
            {
                var csvData = SqLiteGuiProcessing.ExportCvs();
                if (csvData != null)
                {
                    CsvHandler.WriteCsv(savePath, csvData);
                    MessageBox.Show("CSV Exported Successfully!");
                }
                else
                {
                    MessageBox.Show("Error Exporting CSV.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting CSV: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the database.
        /// </summary>
        private void LoadDatabase()
        {
            SqLiteGuiProcessing.OpenDatabase(Register.ActiveDb);
            Register.StartNew();
            // Optionally: Notify UI to refresh data
        }
    }
}
