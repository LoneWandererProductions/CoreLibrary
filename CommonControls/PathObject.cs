/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/PathObject.cs
 * PURPOSE:     Helper Object for FileIoHandler
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global, not yet used put still Useful
// ReSharper disable UnusedMember.Global

using System.IO;

namespace CommonControls
{
    /// <summary>
    ///     Save some Time and return basic Values
    /// </summary>
    public sealed class PathObject
    {
        /// <summary>
        ///     Gets or sets the file path. Full Path.
        /// </summary>
        public string FilePath { get; internal init; }

        /// <summary>
        ///     Gets the folder.
        /// </summary>
        public string Folder => GetFolder();

        /// <summary>
        ///     Gets the file name.
        /// </summary>
        public string FileName => GetGetFileName();

        /// <summary>
        ///     Gets the file name without ext.
        /// </summary>
        public string FileNameWithoutExt => GetGetFileNameWithoutExtension();

        /// <summary>
        ///     Gets the File extension.
        /// </summary>
        /// <value>
        ///     The extension.
        /// </value>
        public string Extension => GetFileExtension();

        /// <summary>
        ///     Get the folder.
        /// </summary>
        /// <returns>The Folder as <see cref="string" />.</returns>
        private string GetFolder()
        {
            return Path.GetDirectoryName(FilePath);
        }

        /// <summary>
        ///     Get the get file name.
        /// </summary>
        /// <returns>The File Name as<see cref="string" />.</returns>
        private string GetGetFileName()
        {
            return Path.GetFileName(FilePath);
        }

        /// <summary>
        ///     Get the get file name without extension.
        /// </summary>
        /// <returns>The  File Name without extension as<see cref="string" />.</returns>
        private string GetGetFileNameWithoutExtension()
        {
            return Path.GetFileNameWithoutExtension(FileName);
        }

        /// <summary>
        ///     Gets the file extension.
        /// </summary>
        /// <returns>the file Extension</returns>
        private string GetFileExtension()
        {
            return Path.GetExtension(FileName);
        }
    }
}
