/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        Communication/Com.cs
 * PURPOSE:     Entry Point for File Downloads
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System.Collections.Generic;

namespace Communication
{
    /// <inheritdoc />
    /// <summary>
    ///     The com class.
    /// </summary>
    public class Com : ICom
    {
        /// <inheritdoc />
        /// <summary>
        ///     Saves the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="url">The URL.</param>
        /// <returns>
        ///     Success Status
        /// </returns>
        public bool SaveFile(string filePath, string url)
        {
            return FileTransfer.SaveFile(filePath, url);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Saves the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="url">The URL.</param>
        public async void SaveFile(string filePath, IEnumerable<string> url)
        {
            await FileTransfer.SaveFile(filePath, url).ConfigureAwait(false);
        }
    }
}
