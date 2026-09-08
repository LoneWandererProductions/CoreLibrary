/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Dialogs
 * FILE:        DialogHandler.cs
 * PURPOSE:     Extension for Dialogs, file dialog wrappers, and error logging display.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Common.Dialogs
{
    /// <summary>
    ///     Wrapper class providing standard dialog handling, file/folder pickers, and exception dialog dispatching.
    /// </summary>
    public static class DialogHandler
    {
        /// <summary>
        ///     Normalizes a raw extension string or existing filter into a valid WPF <see cref="FileDialog.Filter"/> string.
        /// </summary>
        /// <param name="appendage">The input extension or filter pattern.</param>
        /// <returns>A pipe-formatted WPF filter string.</returns>
        private static string NormalizeFilter(string? appendage)
        {
            if (string.IsNullOrWhiteSpace(appendage))
            {
                return ComDlgResources.Appendix;
            }

            // Already a well-formed filter string (e.g. "PNG Files (*.png)|*.png")
            if (appendage.Contains('|'))
            {
                return appendage;
            }

            var extension = appendage.TrimStart('*', '.').ToUpperInvariant();
            var description = string.IsNullOrEmpty(extension) ? "All Files" : $"{extension} Files";

            return $"{description} ({appendage})|{appendage}|{ComDlgResources.Appendix}";
        }

        /// <summary>
        ///     Shows a custom Folder Browser dialog.
        /// </summary>
        /// <param name="folder">Optional target folder path; defaults to current directory if invalid.</param>
        /// <returns>The selected directory path, or null if canceled.</returns>
        public static string? ShowFolder(string? folder = "")
        {
            if (!Directory.Exists(folder))
            {
                folder = Directory.GetCurrentDirectory();
            }

            var browser = new FolderBrowser(folder);
            _ = browser.ShowDialog();

            return browser.Root;
        }

        /// <summary>
        ///     Shows the SQL login dialog.
        /// </summary>
        /// <returns>The generated SQL connection object, or null if canceled.</returns>
        public static SqlConnect? ShowLoginScreen()
        {
            var login = new SqlLogin();
            _ = login.ShowDialog();

            return login.View.Connection;
        }

        /// <summary>
        ///     Displays an error dialog safely, automatically dispatching to the UI thread if called from a background worker.
        /// </summary>
        /// <param name="message">The main error message.</param>
        /// <param name="source">The originating component or method.</param>
        /// <param name="details">Extended stack trace or detail logs.</param>
        /// <param name="title">The window title.</param>
        public static void ErrorDialog(string message, string source = "", string details = "", string title = "Error")
        {
            var dispatcher = Application.Current?.Dispatcher;

            // Redirect background thread calls safely to the main UI thread
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() => ErrorDialog(message, source, details, title)));
                return;
            }

            void ShowDialogAction()
            {
                var safeTitle = string.IsNullOrWhiteSpace(title) ? "Error" : title;
                var safeMessage = string.IsNullOrWhiteSpace(message) ? "An unexpected error occurred." : message;
                var safeSource = source ?? string.Empty;
                var safeDetails = details ?? string.Empty;

                // Truncate extreme stack traces to prevent WPF layout measure chokes
                if (safeDetails.Length > 8000)
                {
                    safeDetails = string.Concat(safeDetails.AsSpan(0, 8000), "\n\n[Details truncated...]");
                }

                try
                {
                    var error = new ErrorDialog(safeTitle, safeMessage, safeSource, safeDetails);
                    error.ShowDialog();
                }
                catch
                {
                    // Fallback to native MessageBox if custom XAML or resource initialization fails
                    MessageBox.Show($"{safeMessage}\n\n{safeDetails}", safeTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (dispatcher != null)
            {
                dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(ShowDialogAction));
            }
            else
            {
                ShowDialogAction();
            }
        }

        /// <summary>
        ///     Shows a standard single-line user input dialog.
        /// </summary>
        /// <param name="header">Header title text.</param>
        /// <param name="description">Input prompt instruction.</param>
        /// <returns>User input string, or empty string if canceled.</returns>
        public static string ShowInputBox(string header, string description)
        {
            var input = new InputBox(header, description);
            _ = input.ShowDialog();

            return string.IsNullOrEmpty(input.InputText) ? string.Empty : input.InputText;
        }

        /// <summary>
        ///     Opens a File Open dialog for selecting a single file.
        /// </summary>
        /// <param name="appendage">File extension pattern or filter string.</param>
        /// <param name="folder">Initial target directory.</param>
        /// <returns>A <see cref="PathObject"/> containing the file path, or null if canceled.</returns>
        public static PathObject? HandleFileOpen(string appendage, string? folder = "")
        {
            if (!Directory.Exists(folder))
            {
                folder = Directory.GetCurrentDirectory();
            }

            var openFile = new OpenFileDialog
            {
                Filter = NormalizeFilter(appendage),
                InitialDirectory = folder
            };

            if (openFile.ShowDialog() != true)
            {
                return null;
            }

            return new PathObject { FilePath = openFile.FileName };
        }

        /// <summary>
        ///     Opens a File Open dialog configured for multi-selection.
        /// </summary>
        /// <param name="appendage">File extension pattern or filter string.</param>
        /// <param name="folder">Initial target directory.</param>
        /// <returns>A list of <see cref="PathObject"/> instances, or null if canceled.</returns>
        public static List<PathObject>? HandleFilesOpen(string appendage, string folder = "")
        {
            if (!Directory.Exists(folder))
            {
                folder = Directory.GetCurrentDirectory();
            }

            var openFile = new OpenFileDialog
            {
                Filter = NormalizeFilter(appendage),
                InitialDirectory = folder,
                Multiselect = true
            };

            if (openFile.ShowDialog() != true)
            {
                return null;
            }

            return openFile.FileNames
                .Select(path => new PathObject { FilePath = path })
                .ToList();
        }

        /// <summary>
        ///     Opens a File Save dialog with overwrite prompt verification.
        /// </summary>
        /// <param name="appendage">File extension pattern or filter string.</param>
        /// <param name="folder">Initial target directory.</param>
        /// <returns>A <see cref="PathObject"/> containing the target path, or null if canceled.</returns>
        public static PathObject? HandleFileSave(string appendage, string? folder = "")
        {
            if (!Directory.Exists(folder))
            {
                folder = Directory.GetCurrentDirectory();
            }

            var saveFile = new SaveFileDialog
            {
                Filter = NormalizeFilter(appendage),
                InitialDirectory = folder,
                OverwritePrompt = true
            };

            if (saveFile.ShowDialog() != true)
            {
                return null;
            }

            return new PathObject { FilePath = saveFile.FileName };
        }
    }
}
