/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreConsole
 * FILE:        ConResources.cs
 * PURPOSE:     Namespaces and Commands for my command line Tool
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using Interpreter;
using Interpreter.Resources;

namespace CoreConsole;

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
    internal const int ResXtractOverload = 2;

    /// <summary>
    ///     The analyzer
    /// </summary>
    internal const int Analyzer = 3;

    /// <summary>
    ///     The dir analyzer
    /// </summary>
    internal const int DirAnalyzer = 4;

    /// <summary>
    ///     The resource1
    /// </summary>
    internal const string ResourceHeader = "header";

    /// <summary>
    /// The resource event processing completed.
    /// </summary>
    internal const string ResourceEventProcessing = "Event processing completed.";

    internal const string ResourceCsExtension = "*.cs";

    /// <summary>
    /// The list command
    /// </summary>
    internal const string ResourceListCmd = "list";

    internal const string ResourceInput = "Enter something: ";

    internal const string ResourceResxtract = "resxtract";

    internal const string ResourceResxtractOutput = "Resxtract operation completed successfully: {0} created.{1}";

    internal const string ResourceEventWait = "Event is processing. Please wait...";

    /// <summary>
    /// The using command
    /// </summary>
    internal const string ResourceUsingCmd = "using";

    internal const string ErrorDirectory = "Error: Directory path '{0}' does not exist.";

    internal const string ErrorProjectPath = "Error: The project path '{0}' does not exist.";

    internal const string ErrorDirectoryOutput =
        "Error: The directory for output resource file '{0}' does not exist.";

    internal const string ErrorAccessFile = "Error accessing output resource file: {0}";

    internal const string MessageClose = "The application will close after a short delay.";

    internal const string ErrorCommandNotFound = "Error: Command not found.";

    internal const string InformationDirectoryMissing = "Directory path is required.";

    internal const string ErrorProjectPathMissing = "Error: Project path is required.";

    internal const string ResxtractFinished = "Resxtract operation completed: No string literals found to extract.";

    internal const string MessageSeparator = "  - ";

    internal const string Quotes = "\"";

    internal const string MessageError = "Error";

    internal const string MessageFilesIgnored = "Loaded {0} files to ignore.";

    internal const string ErrorRegexpattern = "Error loading regex pattern: {0}. Exception: {1}";

    internal const string MessageOutputIgnore = "Loaded {0} ignore patterns.";

    internal const string MessageChangedFiles = "Changed files:{0}  - {1}";

    internal const string InformationInvalidArgument = "Invalid arguments or operation.";

    internal const string MessageInfo = "Core Console Application";

    internal const string HeaderTryrunNoChanges = "Header try run, no results.";

    internal const string HeaderTryrunWouldAffect = "Header try run would affect: ";

    internal const string ResxtractTryrunWouldAffect = "Resxtract try run, would affect: ";

    /// <summary>
    ///     The available commands
    /// </summary>
    internal static readonly Dictionary<int, InCommand> CodeCommands = new()
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
            ResXtractOverload, new InCommand
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
        },
        {
            DirAnalyzer, new InCommand
            {
                Command = "dir",
                ParameterCount = 1,
                Description =
                    "Lists all files in the given directory with size and percentage of total. (1 parameter: <directoryPath>)"
            }
        }
    };

    /// <summary>
    ///     The extension commands
    /// </summary>
    internal static readonly Dictionary<int, InCommand> ExtensionCommands = new()
    {
        {
            Header, new InCommand
            {
                Command = "tryrun",
                ParameterCount = 0,
                FeedbackId = 1,
                Description =
                    "Show results and optional run commands"
            }
        }
    };

    /// <summary>
    ///     The replace feedback
    /// </summary>
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
}
