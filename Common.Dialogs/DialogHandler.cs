/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Dialogs
 * FILE:        DialogHandler.cs
 * PURPOSE:     Extension for Dialogs, some smaller extras and Extensions like a Folder View
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common.Dialogs
{
    /// <summary>
    ///     Loads all the basic Files on StartUp
    /// </summary>
    public static class DialogHandler
    {
        /// <summary>
        /// Makes a string safe to assign to <see cref="FileDialog.Filter"/>.
        ///
        /// <see cref="FileDialog.Filter"/> requires the WPF/Win32
        /// "description|pattern|description|pattern..." format and *throws*
        /// <see cref="ArgumentException"/> the instant it's assigned anything else - in
        /// particular, a bare pattern like "*.txt" has zero '|' characters, which is an
        /// odd number of segments, which the setter rejects outright. Every call site in
        /// this file used to pass a bare pattern straight through, so File &gt; Open and
        /// Save As crashed on every single use. This wraps a bare pattern into a valid
        /// filter automatically; a string that's already a real "description|pattern"
        /// filter is passed through unchanged.
        /// </summary>
        private static string NormalizeFilter(string? appendage)
        {
            if (string.IsNullOrWhiteSpace(appendage)) return ComDlgResources.AppendixFull;
            if (appendage.Contains('|')) return appendage; // already a well-formed filter string

            var extension = appendage.TrimStart('*', '.').ToUpperInvariant();
            var description = string.IsNullOrEmpty(extension) ? "All Files" : $"{extension} Files";
            return $"{description} ({appendage})|{appendage}|{ComDlgResources.AppendixFull}";
        }

        /// <summary>
        ///     Show a Folder Dialog, displaying Folder structure
        /// </summary>
        /// <param name="folder">Folder, optional parameter, uses CurrentDictionary as fallback</param>
        /// <returns>Selected Path</returns>
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
        ///     Shows the login screen.
        /// </summary>
        /// <returns>Sql Connection String Builder</returns>
        public static SqlConnect? ShowLoginScreen()
        {
            var login = new SqlLogin();
            _ = login.ShowDialog();

            return login.View.Connection;
        }

        /// <summary>
        ///     The Error dialog.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The source.</param>
        /// <param name="details">The details.</param>
        /// <param name="title">The title.</param>
        public static void ErrorDialog(string message, string source = "", string details = "", string title = "Error")
        {
            var dispatcher = System.Windows.Application.Current?.Dispatcher;

            // 1. Redirect background thread calls to the UI thread
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke(new Action(() => ErrorDialog(message, source, details, title)));
                return;
            }

            // 2. Defer execution so WPF finishes its layout/render cycle before opening a modal window
            dispatcher?.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
            {
                var safeTitle = string.IsNullOrWhiteSpace(title) ? "Error" : title;
                var safeMessage = string.IsNullOrWhiteSpace(message) ? "An unexpected error occurred." : message;
                var safeSource = source ?? string.Empty;
                var safeDetails = details ?? string.Empty;

                // 3. Truncate extreme stack traces (prevents WPF TextBlock/Run layout overflow)
                if (safeDetails.Length > 8000)
                {
                    safeDetails = safeDetails.Substring(0, 8000) + "\n\n[Details truncated...]";
                }

                try
                {
                    var error = new ErrorDialog(safeTitle, safeMessage, safeSource, safeDetails);
                    error.ShowDialog();
                }
                catch
                {
                    // Fallback if custom ErrorDialog XAML fails to initialize
                    System.Windows.MessageBox.Show($"{safeMessage}\n\n{safeDetails}", safeTitle,
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }));
        }

        /// <summary>
        ///     Shows the input box.
        /// </summary>
        /// <param name="header">The header.</param>
        /// <param name="description">The description.</param>
        /// <returns>Input string</returns>
        public static string ShowInputBox(string header, string description)
        {
            var input = new InputBox(header, description);
            _ = input.ShowDialog();

            return string.IsNullOrEmpty(input.InputText) ? string.Empty : input.InputText;
        }

        /// <summary>
        ///     Looks up a file
        ///     Returns the PathObject
        ///     With Start Folder
        /// </summary>
        /// <param name="appendage">File Extension we allow</param>
        /// <param name="folder">Folder, optional parameter, uses CurrentDictionary as fallback</param>
        /// <returns>PathObject with basic File Parameters</returns>
        public static PathObject? HandleFileOpen(string appendage, string? folder = "")
        {
            if (!Directory.Exists(folder))
            {
                folder = Directory.GetCurrentDirectory();
            }

            var openFile = new OpenFileDialog { Filter = NormalizeFilter(appendage), InitialDirectory = folder };

            if (openFile.ShowDialog() != true)
            {
                return null;
            }

            var path = openFile.FileName;

            return new PathObject { FilePath = path };
        }

        /// <summary>
        ///     Looks up multiple files
        ///     Returns a list of PathObjects
        ///     With Start Folder
        /// </summary>
        /// <param name="appendage">File Extension we allow</param>
        /// <param name="folder">Folder, optional parameter, uses CurrentDirectory as fallback</param>
        /// <returns>A List of PathObjects, or null if canceled</returns>
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
                Multiselect = true // This enables multi-selection
            };

            if (openFile.ShowDialog() != true)
            {
                return null;
            }

            // Convert the array of selected paths into a List of PathObjects
            return openFile.FileNames
                .Select(path => new PathObject { FilePath = path })
                .ToList();
        }

        /// <summary>
        ///     Looks up a file, asks if we want to overwrite
        ///     Returns the PathObject
        ///     With Start Folder
        /// </summary>
        /// <param name="appendage">File Extension we allow</param>
        /// <param name="folder">Folder, optional parameter, uses CurrentDictionary as fallback</param>
        /// <returns>PathObject with basic File Parameters</returns>
        public static PathObject? HandleFileSave(string appendage, string? folder = "")
        {
            if (!Directory.Exists(folder))
            {
                folder = Directory.GetCurrentDirectory();
            }

            var saveFile = new SaveFileDialog
            {
                Filter = NormalizeFilter(appendage), InitialDirectory = folder, OverwritePrompt = true
            };

            if (saveFile.ShowDialog() != true)
            {
                return null;
            }

            var path = saveFile.FileName;

            return new PathObject { FilePath = path };
        }
    }
}
