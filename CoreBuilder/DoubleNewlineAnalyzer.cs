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
    public class DoubleNewlineAnalyzer : ICodeAnalyzer
    {
        public string Name => "DoubleNewline";

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
