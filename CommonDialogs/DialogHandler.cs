/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonDialogs
 * FILE:        CommonDialogs/DialogHandler.cs
 * PURPOSE:     Extension for Dialogs, some smaller extras and Extensions like a Folder View
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System.IO;
using Microsoft.Win32;

namespace CommonDialogs
{
    /// <summary>
    ///     Loads all the basic Files on StartUp
    /// </summary>
    public static class DialogHandler
    {
        /// <summary>
        ///     Show a Folder Dialog, displaying Folder structure
        /// </summary>
        /// <param name="folder">Folder, optional parameter, uses CurrentDictionary as fallback</param>
        /// <returns>Selected Path</returns>
        public static string ShowFolder(string folder = "")
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
        public static SqlConnect ShowLoginScreen()
        {
            var login = new SqlLogin();
            _ = login.ShowDialog();

            return login.View.Connection;
        }

        public static void ErrorDialog(string message, string source = null, string details = null, string header = null)
        {
            var title = header ?? "Error";
            var error = new CustomErrorDialog(title, message, source, details);

            error.ShowDialog();
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

            if (string.IsNullOrEmpty(input.InputText))
            {
                return string.Empty;
            }

            return input.InputText;
        }

        /// <summary>
        ///     Looks up a file
        ///     Returns the PathObject
        ///     With Start Folder
        /// </summary>
        /// <param name="appendage">File Extension we allow</param>
        /// <param name="folder">Folder, optional parameter, uses CurrentDictionary as fallback</param>
        /// <returns>PathObject with basic File Parameters</returns>
        public static PathObject? HandleFileOpen(string appendage, string folder = "")
        {
            if (string.IsNullOrEmpty(appendage))
            {
                appendage = ComCtlResources.Appendix;
            }

            if (!Directory.Exists(folder))
            {
                folder = Directory.GetCurrentDirectory();
            }

            var openFile = new OpenFileDialog { Filter = appendage, InitialDirectory = folder };

            if (openFile.ShowDialog() != true)
            {
                return null;
            }

            var path = openFile.FileName;

            return new PathObject { FilePath = path };
        }

        /// <summary>
        ///     Looks up a file, asks if we want to overwrite
        ///     Returns the PathObject
        ///     With Start Folder
        /// </summary>
        /// <param name="appendage">File Extension we allow</param>
        /// <param name="folder">Folder, optional parameter, uses CurrentDictionary as fallback</param>
        /// <returns>PathObject with basic File Parameters</returns>
        public static PathObject? HandleFileSave(string appendage, string folder = "")
        {
            if (string.IsNullOrEmpty(appendage))
            {
                appendage = ComCtlResources.Appendix;
            }

            if (!Directory.Exists(folder))
            {
                folder = Directory.GetCurrentDirectory();
            }

            var saveFile = new SaveFileDialog { Filter = appendage, InitialDirectory = folder, OverwritePrompt = true };

            if (saveFile.ShowDialog() != true)
            {
                return null;
            }

            var path = saveFile.FileName;

            return new PathObject { FilePath = path };
        }
    }
}
