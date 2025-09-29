/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     InterOp
 * FILE:        InterOp/FileInfoFetcher.cs
 * PURPOSE:     Faster way to look for files compared to Directory.GetFiles() or FileInfo.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace InterOp;

/// <summary>
///     File Fetcher Class to get basic information fast.
/// </summary>
public static class FileInfoFetcher
{
    /// <summary>
    ///     Constant representing the file attribute for directories (0x10).
    ///     Used to distinguish files from directories during file searching.
    /// </summary>
    private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;

    /// <summary>
    ///     P/Invoke declaration for FindFirstFile function from kernel32.dll.
    ///     This function is used to find the first file matching the provided pattern.
    ///     See:
    ///     https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-findfirstfilew
    /// </summary>
    /// <param name="lpFileName">The directory or file pattern to search for.</param>
    /// <param name="lpFindFileData">The structure to hold file data returned by the function.</param>
    /// <returns>Handle to the search operation if successful, IntPtr.Zero otherwise.</returns>
    [DllImport(InterOpResources.KernelDll, CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr FindFirstFile(string lpFileName, ref WIN32_FIND_DATA lpFindFileData);

    /// <summary>
    ///     P/Invoke declaration for FindNextFile function from kernel32.dll.
    ///     This function is used to find the next file matching the search pattern.
    ///     See:
    ///     https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-findnextfilew
    /// </summary>
    /// <param name="hFindFile">Handle to the search operation.</param>
    /// <param name="lpFindFileData">The structure to hold the file data returned by the function.</param>
    /// <returns>True if the next file is found, false otherwise.</returns>
    [DllImport(InterOpResources.KernelDll, SetLastError = true)]
    internal static extern bool FindNextFile(IntPtr hFindFile, ref WIN32_FIND_DATA lpFindFileData);

    /// <summary>
    ///     P/Invoke declaration for FindClose function from kernel32.dll.
    ///     This function closes the handle to the search operation.
    ///     See:
    ///     https://learn.microsoft.com/en-us/windows/win32/api/fileapi/nf-fileapi-findclose
    /// </summary>
    /// <param name="hFindFile">Handle to the search operation to be closed.</param>
    /// <returns>True if successful, false otherwise.</returns>
    [DllImport(InterOpResources.KernelDll, SetLastError = true)]
    internal static extern bool FindClose(IntPtr hFindFile);

    /// <summary>
    ///     Retrieves a list of files (not directories) in the specified directory along with their metadata.
    /// </summary>
    /// <param name="directory">The directory to search for files in.</param>
    /// <returns>
    ///     A list of FileData objects containing file information for each file found.
    /// </returns>
    public static List<FileData> GetFiles(string directory)
    {
        // List to hold the file data
        var files = new List<FileData>();

        // Structure to hold information about the file being retrieved
        var findFileData = new WIN32_FIND_DATA();

        // Initiate the search for files in the specified directory
        var findHandle = FindFirstFile(directory + @"\*", ref findFileData);

        // Ensure that we found at least one file
        if (findHandle != IntPtr.Zero)
        {
            do
            {
                // Check if the entry is a file (not a directory)
                if ((findFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) == 0)
                {
                    // Convert the file time structure to DateTime
                    var creationTime = DateTime.FromFileTimeUtc(
                        ((long)findFileData.ftCreationTime.dwHighDateTime << 32) |
                        (uint)findFileData.ftCreationTime.dwLowDateTime);
                    var lastAccessTime = DateTime.FromFileTimeUtc(
                        ((long)findFileData.ftLastAccessTime.dwHighDateTime << 32) |
                        (uint)findFileData.ftLastAccessTime.dwLowDateTime);
                    var lastWriteTime = DateTime.FromFileTimeUtc(
                        ((long)findFileData.ftLastWriteTime.dwHighDateTime << 32) |
                        (uint)findFileData.ftLastWriteTime.dwLowDateTime);

                    // Create a new FileData object to hold the file's metadata
                    var file = new FileData(
                        findFileData.cFileName,
                        ((long)findFileData.nFileSizeHigh << 32) | findFileData.nFileSizeLow,
                        creationTime,
                        lastAccessTime,
                        lastWriteTime
                    );

                    // Add the file data to the list
                    files.Add(file);
                }
            } while (FindNextFile(findHandle, ref findFileData)); // Continue with the next file

            // Close the search handle
            FindClose(findHandle);
        }

        // Return the list of file data
        return files;
    }

    /// <summary>
    ///     Structure that represents the file metadata returned by the FindFirstFile and FindNextFile functions.
    ///     See:
    ///     https://learn.microsoft.com/en-us/windows/win32/api/minwinbase/ns-minwinbase-win32_find_dataa
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct WIN32_FIND_DATA
    {
        /// <summary>
        ///     File attributes (e.g., directory, hidden, etc.)
        /// </summary>
        public readonly uint dwFileAttributes;

        /// <summary>
        ///     Creation time of the file.
        /// </summary>
        public FILETIME ftCreationTime;

        /// <summary>
        ///     Last access time of the file.
        /// </summary>
        public FILETIME ftLastAccessTime;

        /// <summary>
        ///     Last write time (modification time) of the file.
        /// </summary>
        public FILETIME ftLastWriteTime;

        /// <summary>
        ///     High part of the file size.
        /// </summary>
        public readonly uint nFileSizeHigh;

        /// <summary>
        ///     Low part of the file size.
        /// </summary>
        public readonly uint nFileSizeLow;

        /// <summary>
        ///     Reserved values for future use (not used in this case).
        /// </summary>
        public readonly uint dwReserved0;

        public readonly uint dwReserved1;

        /// <summary>
        ///     Full file name (including path).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public readonly string cFileName;

        /// <summary>
        ///     Alternate file name (in 8.3 format).
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
        public readonly string cAlternateFileName;
    }

    /// <summary>
    ///     Custom class to hold useful file information like name, size, and timestamps.
    /// </summary>
    public sealed class FileData
    {
        /// <summary>
        ///     Constructor to initialize the file information.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileSize">Size of the file in bytes.</param>
        /// <param name="creationTime">Creation timestamp of the file.</param>
        /// <param name="lastAccessTime">Last access timestamp of the file.</param>
        /// <param name="lastWriteTime">Last write timestamp of the file.</param>
        public FileData(string fileName, long fileSize, DateTime creationTime, DateTime lastAccessTime,
            DateTime lastWriteTime)
        {
            FileName = fileName;
            FileSize = fileSize;
            CreationTime = creationTime;
            LastAccessTime = lastAccessTime;
            LastWriteTime = lastWriteTime;
        }

        /// <summary>
        ///     Name of the file (including path).
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        ///     Size of the file in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        ///     Creation timestamp of the file.
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        ///     Last access timestamp of the file.
        /// </summary>
        public DateTime LastAccessTime { get; set; }

        /// <summary>
        ///     Last write (modification) timestamp of the file.
        /// </summary>
        public DateTime LastWriteTime { get; set; }
    }
}
