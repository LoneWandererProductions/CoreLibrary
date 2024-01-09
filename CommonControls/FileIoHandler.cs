/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/FileIoHandler.cs
 * PURPOSE:     Extension for File Dialogs, some smaller extras and Extensions like a Folder View
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Win32;

namespace CommonControls
{
    /// <summary>
    ///     Loads all the basic Files on StartUp
    /// </summary>
    public static class FileIoHandler
    {
        /// <summary>
        ///     Show a Folder Dialog, displaying Folder structure
        /// </summary>
        /// <param name="folder">Start Path</param>
        /// <returns>Selected Path</returns>
        public static string ShowFolder(string folder)
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
        /// <returns>Connection String</returns>
        public static string ShowLoginScreen()
        {
            var login = new SqlLogin();
            _ = login.ShowDialog();

            return login.View.ConnectionString;
        }


        /// <summary>
        ///     Looks up a file
        ///     Returns the PathObject
        ///     With Start Folder
        /// </summary>
        /// <param name="appendage">File Extension we allow</param>
        /// <param name="folder">Folder</param>
        /// <returns>PathObject with basic File Parameters</returns>
        [return: MaybeNull]
        public static PathObject HandleFileOpen(string appendage, string folder)
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
        /// <param name="folder">Folder</param>
        /// <returns>PathObject with basic File Parameters</returns>
        [return: MaybeNull]
        public static PathObject HandleFileSave(string appendage, string folder)
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
