/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        CoreBuilder/IHeaderExtractor.cs
 * PURPOSE:     Interface for HeaderExtractor
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace CoreBuilder
{
    /// <summary>
    ///     Interface for HeaderExtractor
    /// </summary>
    public interface IHeaderExtractor
    {
        /// <summary>
        ///     Determines whether the specified content contains header.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        ///     <c>true</c> if the specified content contains header; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsHeader(string content);

        /// <summary>
        ///     Extracts the namespace.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        string ExtractNamespace(string content);

        /// <summary>
        ///     Inserts the header.
        /// </summary>
        /// <param name="fileContent">Content of the file.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="purpose">The purpose.</param>
        /// <param name="programmerName">Name of the programmer.</param>
        /// <returns></returns>
        string InsertHeader(string fileContent, string fileName, string purpose, string programmerName);

        /// <summary>
        ///     Processes the files.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        void ProcessFiles(string directoryPath, bool includeSubdirectories);
    }
}
