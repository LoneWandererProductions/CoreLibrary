/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        CommandFactory.cs
 * PURPOSE:     Return all available code analyzers.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder.Development;
using CoreBuilder.Extensions;
using CoreBuilder.FileManager;
using CoreBuilder.Interface;
using CoreBuilder.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using Weaver.Interfaces;

namespace CoreBuilder
{
    /// <summary>
    /// Simple factory to return all available code analyzers and commands.
    /// </summary>
    public static class CommandFactory
    {
        /// <summary>
        /// Gets the commands.
        /// </summary>
        /// <returns>All commands.</returns>
        public static IReadOnlyList<ICommand> GetCommands()
        {
            ICommand[] modules =
            {
                new DirectorySizeAnalyzer(), new LogTailCommand(), new HeaderExtractor(), new ResXtract(),
                new AllocationAnalyzer(), new DisposableAnalyzer(), new DoubleNewlineAnalyzer(),
                new DuplicateStringLiteralAnalyzer(), new EventHandlerAnalyzer(), new HotPathAnalyzer(),
                new LicenseHeaderAnalyzer(), new UnusedClassAnalyzer(), new UnusedConstantAnalyzer(),
                new UnusedLocalVariableAnalyzer(), new UnusedParameterAnalyzer(), new UnusedPrivateFieldAnalyzer(),
                new DocCommentCoverageCommand(), new DeadReferenceAnalyzer(), new ApiExplorerCommand(),
                new FileLockScanner(), new SmartPingPro(), new WhoAmI(), new Tree()
            };

            return modules;
        }

        /// <summary>
        /// Gets the commands.
        /// </summary>
        /// <param name="userspace">The userspace.</param>
        /// <returns>All commands by Namespace.</returns>
        public static IReadOnlyList<ICommand> GetCommands(string userspace)
        {
            ICommand[] modules =
            {
                new DirectorySizeAnalyzer(), new DirectorySizeAnalyzer(), new LogTailCommand(),
                new HeaderExtractor(), new ResXtract(), new AllocationAnalyzer(), new DisposableAnalyzer(),
                new DoubleNewlineAnalyzer(), new DuplicateStringLiteralAnalyzer(), new EventHandlerAnalyzer(),
                new HotPathAnalyzer(), new LicenseHeaderAnalyzer(), new UnusedClassAnalyzer(),
                new UnusedConstantAnalyzer(), new UnusedLocalVariableAnalyzer(), new UnusedParameterAnalyzer(),
                new UnusedPrivateFieldAnalyzer(), new DocCommentCoverageCommand(), new DeadReferenceAnalyzer(),
                new ApiExplorerCommand(), new FileLockScanner(), new SmartPingPro(), new WhoAmI(), new Tree()
            };

            // Filter by Namespace
            return modules
                .Where(m => string.Equals(m.Namespace, userspace, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Gets the extensions.
        /// </summary>
        /// <returns>All Extensions</returns>
        public static IReadOnlyList<ICommandExtension> GetExtensions()
        {
            ICommandExtension[] modules = { new WhoAmIExtension() };

            return modules;
        }

        /// <summary>
        /// Gets all analyzers.
        /// </summary>
        /// <returns>All Code Analyzers</returns>
        public static IReadOnlyList<ICodeAnalyzer> GetAllAnalyzers()
        {
            ICodeAnalyzer[] modules =
            {
                new AllocationAnalyzer(), new DisposableAnalyzer(), new DoubleNewlineAnalyzer(),
                new DuplicateStringLiteralAnalyzer(), new EventHandlerAnalyzer(), new HotPathAnalyzer(),
                new LicenseHeaderAnalyzer(), new UnusedClassAnalyzer(), new UnusedConstantAnalyzer(),
                new UnusedLocalVariableAnalyzer(), new UnusedParameterAnalyzer(), new UnusedPrivateFieldAnalyzer(),
                new DocCommentCoverageCommand(), new DeadReferenceAnalyzer()
            };

            return modules;
        }
    }
}
