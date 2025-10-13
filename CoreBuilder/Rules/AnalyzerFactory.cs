/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        Rules/AllocationAnalyzer.cs
 * PURPOSE:     Helper that provides factory methods for creating code analyzers.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using CoreBuilder.Interface;

namespace CoreBuilder.Rules
{
    /// <summary>
    /// Static Helper class that provides factory methods for creating code analyzers.
    /// </summary>
    public static class AnalyzerFactory
    {
        /// <summary>
        /// Gets all analyzers.
        /// </summary>
        /// <returns>List of all Analyzers</returns>
        public static IReadOnlyList<ICodeAnalyzer> GetAllAnalyzers() => _analyzers;

        /// <summary>
        /// The analyzers
        /// </summary>
        private static readonly List<ICodeAnalyzer> _analyzers = new()
        {
                new UnusedConstantAnalyzer(),
                new AllocationAnalyzer(),
                new LicenseHeaderAnalyzer(),
                new DoubleNewlineAnalyzer(),
                new UnusedLocalVariableAnalyzer(),
                new UnusedParameterAnalyzer(),
                new UnusedPrivateFieldAnalyzer(),
                new HotPathAnalyzer(),
                new DisposableAnalyzer(),
                new EventHandlerAnalyzer(),
                new UnusedClassAnalyzer(),
                new TodoCommentAnalyzer(),
                new EmptyCatchBlockAnalyzer(),
                new UnawaitedAsyncCallAnalyzer(),
                new EventSubscriptionAnalyzer(),
                new FieldNamingConventionAnalyzer()
                // Add more analyzers here as needed
        };
    }
}
