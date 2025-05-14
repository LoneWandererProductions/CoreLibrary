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
