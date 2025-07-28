/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/Dialogs.cs
 * PURPOSE:     Reused for SQLiteGui Project only
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using Microsoft.Win32;

namespace SQLiteGui;

/// <summary>
///     The dialogs class.
/// </summary>
internal static class Dialogs
{
    /// <summary>
    ///     Looks up a file, optional Ask if we want to overwrite
    ///     Returns the FilePath
    ///     No Start Folder
    /// </summary>
    /// <param name="appendage">The appendage.</param>
    /// <param name="loader">If True we search a file if not we just ask if we want to overwrite a file, optional.</param>
    /// <returns>
    ///     String of target File
    /// </returns>
    public static string HandleFile(string appendage, bool loader = true)
    {
        var root = string.Empty;

        if (loader)
        {
            var openFile = new OpenFileDialog { Filter = appendage };
            if (openFile.ShowDialog() == true)
            {
                root = openFile.FileName;
            }

            return root;
        }

        var saveFile = new SaveFileDialog { Filter = appendage };

        if (saveFile.ShowDialog() == true)
        {
            root = saveFile.FileName;
        }

        return root;
    }
}
