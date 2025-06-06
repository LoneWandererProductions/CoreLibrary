/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        DoubleNewlineAnalyzer.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CoreBuilder
{
    /// <summary>
    /// Finds double linebreaks.
    /// </summary>
    /// <seealso cref="CoreBuilder.ICodeAnalyzer" />
    public class DoubleNewlineAnalyzer : ICodeAnalyzer
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name => "DoubleNewline";

        /// <summary>
        /// Analyzes the specified file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="fileContent">Content of the file.</param>
        /// <param name="syntaxTree">The syntax tree.</param>
        /// <returns>Diagnostic results.</returns>
        public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent, SyntaxTree syntaxTree)
        {
            var lines = fileContent.Split('\n');
            for (var i = 1; i < lines.Length - 1; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]) && string.IsNullOrWhiteSpace(lines[i - 1]))
                {
                    yield return new Diagnostic(filePath, i + 1, "Multiple blank lines in a row.");
                }
            }
        }
    }
}
