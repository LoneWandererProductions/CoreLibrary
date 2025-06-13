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
        ///     The user space fore code
        /// </summary>
        internal const string UserSpaceCode = "CodeUtilities";

        /// <summary>
        ///     The header
        /// </summary>
        internal const int Header = 0;

        /// <summary>
        ///     The resxtract
        /// </summary>
        internal const int Resxtract = 1;

        /// <summary>
        ///     The resxtract overload
        /// </summary>
        internal const int ResxtractOverload = 2;

        /// <summary>
        ///     The analyzer
        /// </summary>
        internal const int Analyzer = 3;

        /// <summary>
        ///     The available commands
        /// </summary>
        internal static readonly Dictionary<int, InCommand> DctCommandOne = new()
        {
            {
                Header, new InCommand
                {
                    Command = "header",
                    ParameterCount = 1,
                    Description =
                        "Inserts standard headers into all C# source files in the specified project directory. (1 parameter: <projectPath>)"
                }
            },
            {
                Resxtract, new InCommand
                {
                    Command = "resxtract",
                    ParameterCount = 2,
                    Description =
                        "Extracts string literals from project files and writes them to the specified resource file. (2 parameters: <projectPath> <outputResxFile>)"
                }
            },
            {
                ResxtractOverload, new InCommand
                {
                    Command = "resxtract",
                    ParameterCount = 1,
                    Description =
                        "Extracts string literals and generates a .resx file with an automatically determined name and location. (1 parameter: <projectPath>)"
                }
            },
            {
                Analyzer, new InCommand
                {
                    Command = "analyzer",
                    ParameterCount = 1,
                    Description =
                        "Performs basic static analysis on all C# files in the specified directory. (1 parameter: <directoryPath>)"
                }
            }
        };

        /// <summary>
        ///     For commands that need your feedback
        /// </summary>
        internal static readonly Dictionary<int, UserFeedback> Feedback = new() { { 1, ReplaceFeedback } };

        internal static readonly Dictionary<int, InCommand> ExtensionCommands = new()
        {
            {
                Header, new InCommand
                {
                    Command = "dryrun",
                    ParameterCount = 0,
                    FeedbackId =1,
                    Description =
                        "Show results and optional run commands"
                }
            }
        };

        private static readonly UserFeedback ReplaceFeedback = new()
        {
            Before = true,
            Message = "Do you want to commit the following changes?",
            Options = new Dictionary<AvailableFeedback, string>
            {
                { AvailableFeedback.Yes, "If you want to execute the Command type yes" },
                { AvailableFeedback.No, " If you want to stop executing the Command type no." }
            }
        };

        /// <summary>
        /// The resource1
        /// </summary>
        internal const string ResourceHeader = "header";

        internal const string ResourceEventTriggered = "Event triggered. Processing...";

        internal const string ResourceEventProcessing = "Event processing completed.";

        internal const string ResourceCsExtension = "*.cs";

        internal const string ResourceListCmd = "list";

        internal const string ResourceInput = "Enter something: ";

        internal const string ResourceResxtract = "resxtract";

        internal const string ResourceResxtractOutput = "Resxtract operation completed successfully: {0} created.{1}";

        internal const string ResourceEventWait = "Event is processing. Please wait...";

        internal const string ResourceUsingCmd = "using";

        internal const string ErrorDirectory = "Error: Directory path '{0}' does not exist.";

        internal const string ErrorProjectPath = "Error: The project path '{0}' does not exist.";

        internal const string ErrorDirectoryOutput = "Error: The directory for output resource file '{0}' does not exist.";

        internal const string ErrorAccessFile = "Error accessing output resource file: {0}";

        internal const string Resource12 = "The application will close after a short delay.";
        internal const string Resource13 = "Error: Command not found.";
        internal const string Resource14 = "Directory path is required.";
        internal const string Resource15 = "Error: Project path is required.";
        internal const string Resource16 = "Resxtract operation completed: No string literals found to extract.";
        internal const string Resource17 = "  - ";

        internal const string Resource19 = "\"";

        internal const string Resource20 = "Loaded {0} files to ignore.";
        internal const string Resource21 = "Error loading regex pattern: {0}. Exception: {1}";
        internal const string Resource22 = "Loaded {0} ignore patterns.";


        internal const string Resource28 = "Changed files:{0}  - {1}";
        internal const string Resource3 = "Invalid arguments or operation.";
        internal const string Resource4 = "Press any key to exit...";
        internal const string Resource5 = "Core Console Application";

    }
}
