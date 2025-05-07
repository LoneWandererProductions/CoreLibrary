/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        CoreBuilder/HeaderExtractor.cs
 * PURPOSE:     String Resource Interface.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.IO;
using System.Text.RegularExpressions;

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
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>
        ///     <c>true</c> if the specified content contains header; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsHeader(string content)
        {
            // Simple check for the presence of "COPYRIGHT" or similar keywords to detect existing headers
            return Regex.IsMatch(content, @"^\s*/\*\s*\*COPYRIGHT.*\*/", RegexOptions.IgnoreCase);
        }

        /// <summary>
        ///     Method to extract namespace from the C# file content
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>Extracted Namespace</returns>
        public string ExtractNamespace(string content)
        {
            // Find the first namespace declaration in the file
            var match = Regex.Match(content, @"namespace\s+(\S+);");
            return match.Success ? match.Groups[1].Value : "UnknownNamespace";
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
        ///     Method to process a list of files and insert headers where necessary
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        public void ProcessFiles(string directoryPath)
        {
            // Loop through all .cs files in the directory
            foreach (var file in Directory.GetFiles(directoryPath, "*.cs", SearchOption.AllDirectories))
            {
                // Read the file content
                var fileContent = File.ReadAllText(file);

                // Skip file if it already contains a header
                if (ContainsHeader(fileContent))
                {
                    Console.WriteLine($"Skipping {file}, header already exists.");
                    continue;
                }

                // Create the header and insert it
                var updatedContent = InsertHeader(fileContent, Path.GetFileName(file), "Your file purpose here",
                    "Your name here");

                // Save the updated content back to the file
                File.WriteAllText(file, updatedContent);
                Console.WriteLine($"Header inserted in {file}");
            }
        }
    }
}
