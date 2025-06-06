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
    public interface ICodeAnalyzer
    {
        string Name { get; }

        IEnumerable<Diagnostic> Analyze(string filePath, string fileContent, SyntaxTree syntaxTree);
    }
}
