/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Communication
 * FILE:        Communication/FileTransfer.cs
 * PURPOSE:     Entry Point for File Downloads, defined as Interface
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global

using System.Collections.Generic;

namespace Communication
{
    /// <summary>
    ///     The ICom interface.
    /// </summary>
    internal interface ICom
    {
        /// <summary>
        ///     Saves the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="url">The URL.</param>
        /// <returns>Success Status</returns>
        bool SaveFile(string filePath, string url);

        /// <summary>
        ///     Saves the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="url">The URL.</param>
        void SaveFile(string filePath, IEnumerable<string> url);
    }
}
