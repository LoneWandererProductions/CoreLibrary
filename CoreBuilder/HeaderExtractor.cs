/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        CoreBuilder/HeaderExtractor.cs
 * PURPOSE:     String Resource Interface.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.IO;

namespace CoreBuilder
{
    /// <summary>
    ///     License Header Builder
    /// </summary>
    public class HeaderExtractor : IHeaderExtractor
    {
        /// <summary>
        ///     Define the header template with placeholders for file info
        /// </summary>
        private const string HeaderTemplate = @"/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     {0}
 * FILE:        {1}
 * PURPOSE:     {2}
 * PROGRAMMER:  {3}
 */
";

        /// <summary>
        ///     Method to check if the file content already contains a header
        ///     Simple check for the presence of "COPYRIGHT" or similar keywords to detect existing headers
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        ///     <c>true</c> if the specified content contains header; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsHeader(string content)
        {
            foreach (var line in content.Split('\n'))
            {
                var trimmed = line.Trim().ToLowerInvariant();
                if (trimmed.Contains("copyright", StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }


        /// <summary>
        ///     Method to extract namespace from the C# file content
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>Extracted Namespace</returns>
        public string ExtractNamespace(string content)
        {
            foreach (var line in content.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("namespace ", StringComparison.InvariantCultureIgnoreCase))
                {
                    var parts = trimmed.Split(new[] { ' ', '{' }, StringSplitOptions.RemoveEmptyEntries);
                    return parts.Length > 1 ? parts[1] : "UnknownNamespace";
                }
            }
            return "UnknownNamespace";
        }

        /// <summary>
        ///     Method to insert header into the file content
        /// </summary>
        /// <param name="fileContent">Content of the file.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="purpose">The purpose.</param>
        /// <param name="programmerName">Name of the programmer.</param>
        /// <returns>File with Header.</returns>
        public string InsertHeader(string fileContent, string fileName, string purpose, string programmerName)
        {
            var namespaceName = ExtractNamespace(fileContent);
            var header = string.Format(HeaderTemplate, namespaceName, fileName, purpose, programmerName);

            // Insert the header at the beginning of the file content
            return header + Environment.NewLine + fileContent;
        }

        /// <summary>
        /// Method to process a list of files and insert headers where necessary
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        /// <param name="includeSubdirectories">if set to <c>true</c> [subdirectories].</param>
        public void ProcessFiles(string directoryPath, bool includeSubdirectories)
        {
            var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            foreach (var file in Directory.GetFiles(directoryPath, "*.cs", searchOption))
            {
                var fileContent = File.ReadAllText(file);

                if (ContainsHeader(fileContent))
                {
                    Console.WriteLine($"Skipping {file}, header already exists.");
                    continue;
                }

                var updatedContent = InsertHeader(fileContent, Path.GetFileName(file), "Your file purpose here", "Your name here");

                File.WriteAllText(file, updatedContent);
                Console.WriteLine($"Header inserted in {file}");
            }
        }

    }
}
