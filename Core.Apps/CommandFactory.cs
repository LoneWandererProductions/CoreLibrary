/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps
 * FILE:        CommandFactory.cs
 * PURPOSE:     Return all available code analyzers.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Core.Apps.Development;
using Core.Apps.Extensions;
using Core.Apps.FileManager;
using Core.Apps.Interface;
using Core.Apps.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using Weaver;
using Weaver.Interfaces;

namespace Core.Apps
{
    /// <summary>
    /// Simple factory to return all available code analyzers and commands.
    /// </summary>
    public static class CommandFactory
    {
        /// <summary>
        /// Gets the commands.
        /// </summary>
        /// <param name="weave">The weave.</param>
        /// <returns>
        /// All commands.
        /// </returns>
        public static IReadOnlyList<ICommand> GetCommands(Weave? weave = null)
        {
            ICommand[] modules =
            {
                new DirectorySizeAnalyzer(weave.Runtime.Variables), new LogTailCommand(), new HeaderExtractor(),
                new ResXtract(), new AllocationAnalyzer(), new DisposableAnalyzer(), new DoubleNewlineAnalyzer(),
                new DuplicateStringLiteralAnalyzer(), new EventHandlerAnalyzer(), new HotPathAnalyzer(),
                new LicenseHeaderAnalyzer(), new UnusedClassAnalyzer(), new UnusedConstantAnalyzer(),
                new UnusedLocalVariableAnalyzer(), new UnusedParameterAnalyzer(), new UnusedPrivateFieldAnalyzer(),
                new DocCommentCoverageCommand(), new DeadReferenceAnalyzer(), new ApiExplorerCommand(),
                new FileLockScanner(weave.Runtime.Variables), new SmartPingPro(),
                new WhoAmI(weave.Runtime.Variables), new Tree()
            };

            return modules;
        }

        /// <summary>
        /// Gets the commands.
        /// </summary>
        /// <param name="userSpace">The user space.</param>
        /// <param name="weave">The weave.</param>
        /// <returns>
        /// All commands by Namespace.
        /// </returns>
        public static IReadOnlyList<ICommand> GetCommands(string userSpace, Weave? weave = null)
        {
            ICommand[] modules =
            {
                new DirectorySizeAnalyzer(weave.Runtime.Variables), new LogTailCommand(), new HeaderExtractor(),
                new ResXtract(), new AllocationAnalyzer(), new DisposableAnalyzer(), new DoubleNewlineAnalyzer(),
                new DuplicateStringLiteralAnalyzer(), new EventHandlerAnalyzer(), new HotPathAnalyzer(),
                new LicenseHeaderAnalyzer(), new UnusedClassAnalyzer(), new UnusedConstantAnalyzer(),
                new UnusedLocalVariableAnalyzer(), new UnusedParameterAnalyzer(), new UnusedPrivateFieldAnalyzer(),
                new DocCommentCoverageCommand(), new DeadReferenceAnalyzer(), new ApiExplorerCommand(),
                new FileLockScanner(weave.Runtime.Variables), new SmartPingPro(),
                new WhoAmI(weave.Runtime.Variables), new Tree()
            };

            // Filter by Namespace
            return modules
                .Where(m => string.Equals(m.Namespace, userSpace, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Gets the extensions.
        /// </summary>
        /// <returns>
        /// All Extensions
        /// </returns>
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