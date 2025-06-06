/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        LicenseHeaderAnalyzer.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CoreBuilder
{
    public class LicenseHeaderAnalyzer : ICodeAnalyzer
    {
        public string Name => "LicenseHeader";

        public IEnumerable<Diagnostic> Analyze(string filePath, string fileContent, SyntaxTree syntaxTree)
        {
            if (!fileContent.StartsWith("// Licensed under", StringComparison.OrdinalIgnoreCase))
            {
                yield return new Diagnostic(filePath, 1, "Missing license header.");
            }
        }
    }
}
