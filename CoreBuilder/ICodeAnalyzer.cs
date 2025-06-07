/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        ICodeAnalyzer.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CoreBuilder
{
    /// <summary>
    ///     Analyzer Interface, that will be shared around.
    /// </summary>
    public interface ICodeAnalyzer
    {
        /// <summary>
        ///     Gets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        string Name { get; }

        /// <summary>
        ///     Analyzes the specified file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="fileContent">Content of the file.</param>
        /// <param name="syntaxTree">The syntax tree.</param>
        /// <returns>Code Analyzer results.</returns>
        IEnumerable<Diagnostic> Analyze(string filePath, string fileContent, SyntaxTree syntaxTree);
    }
}
