/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreConsole
 * FILE:        ConResources.cs
 * PURPOSE:     Namespaces and Commands for my command line Tool
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using Interpreter;

namespace CoreConsole
{
    /// <summary>
    ///     Resource File
    /// </summary>
    internal static class ConResources
    {
        /// <summary>
        /// The user space fore code
        /// </summary>
        internal const string UserSpaceCode = "CodeUtilities";

        /// <summary>
        /// The header
        /// </summary>
        internal const int Header = 0;

        /// <summary>
        /// The resxtract
        /// </summary>
        internal const int Resxtract = 1;

        /// <summary>
        /// The resxtract overload
        /// </summary>
        internal const int ResxtractOverload = 2;

        /// <summary>
        /// The analyzer
        /// </summary>
        internal const int Analyzer = 3;

        /// <summary>
        ///     The available commands
        /// </summary>
        internal static readonly Dictionary<int, InCommand> DctCommandOne = new()
        {
            {
                Header,
                new InCommand
                {
                    Command = "header",
                    ParameterCount = 1,
                    Description = "Inserts standard headers into all C# source files in the specified project directory. (1 parameter: <projectPath>)"
                }
            },
            {
                Resxtract,
                new InCommand
                {
                    Command = "resxtract",
                    ParameterCount = 2,
                    Description = "Extracts string literals from project files and writes them to the specified resource file. (2 parameters: <projectPath> <outputResxFile>)"
                }
            },
            {
                ResxtractOverload,
                new InCommand
                {
                    Command = "resxtract",
                    ParameterCount = 1,
                    Description = "Extracts string literals and generates a .resx file with an automatically determined name and location. (1 parameter: <projectPath>)"
                }
            },
            {
                Analyzer,
                new InCommand
                {
                    Command = "analyzer",
                    ParameterCount = 1,
                    Description = "Performs basic static analysis on all C# files in the specified directory. (1 parameter: <directoryPath>)"
                }
            }
        };

    }
}
