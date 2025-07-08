/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     CoreConsole
* FILE:        CoreConsole/Program.cs
* PURPOSE:     Basic Console app, to get my own tools running
* PROGRAMMER:  Peter Geinitz (Wayfarer)
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CoreBuilder;
using Interpreter;
using Microsoft.CodeAnalysis.CSharp;

namespace CoreConsole
{
    /// <summary>
    ///     Entry Point for my tools and future utilities.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        ///     The prompt
        /// </summary>
        private static Prompt _prompt;

        /// <summary>
        ///     The console lock
        /// </summary>
        private static readonly object ConsoleLock = new();

        /// <summary>
        ///     The is event triggered
        /// </summary>
        private static bool _isEventTriggered;

        /// <summary>
        ///     The analyzers
        /// </summary>
        private static readonly List<ICodeAnalyzer> Analyzers = new();

        /// <summary>
        ///     Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            //add our analyzers
            Analyzers.Add(new DoubleNewlineAnalyzer());
            Analyzers.Add(new LicenseHeaderAnalyzer());
            if (args.Length < 2)
            {
                Initiate();
            }
            else
            {
                var operation = args[0];
                if (operation == ConResources.ResourceHeader && args.Length == 2)
                {
                    var directoryPath = args[1];
                    IHeaderExtractor headerExtractor = new HeaderExtractor();
                    var message = headerExtractor.ProcessFiles(directoryPath, true);
                    Console.WriteLine(message);
                }
                else if (operation == ConResources.ResourceResxtract && args.Length >= 3)
                {
                    var projectPath = args[1];
                    var outputResourceFile = args[2];
                    var ignoreList = new List<string>();
                    var ignorePatterns = new List<string>();
                    if (args.Length > 3 && File.Exists(args[3]))
                    {
                        ignoreList = new List<string>(File.ReadAllLines(args[3]));
                        Console.WriteLine(ConResources.MessageFilesIgnored, ignoreList.Count);
                    }

                    if (args.Length > 4 && File.Exists(args[4]))
                    {
                        foreach (var pattern in File.ReadAllLines(args[4]))
                        {
                            try
                            {
                                ignorePatterns.Add(pattern);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ConResources.ErrorRegexpattern, pattern, ex.Message);
                            }
                        }

                        Console.WriteLine(ConResources.MessageOutputIgnore, ignorePatterns.Count);
                    }

                    IResourceExtractor resXtractExtractor = new ResXtract(ignoreList, ignorePatterns);
                    resXtractExtractor.ProcessProject(projectPath, outputResourceFile);
                }
                else
                {
                    Console.WriteLine(ConResources.InformationInvalidArgument);
                }
            }

            Console.WriteLine(ConResources.ResourceInput);
            Console.ReadKey();
        }

        /// <summary>
        ///     Initiates this instance.
        /// </summary>
        private static void Initiate()
        {
            _prompt = new Prompt();
            _prompt.SendLogs += SendLogs;
            _prompt.SendCommands += SendCommands;
            _prompt.Callback(ConResources.MessageInfo);
            _prompt.Initiate(ConResources.DctCommandOne, ConResources.UserSpaceCode);
            _prompt.AddCommands(ConResources.DctCommandOne, ConResources.UserSpaceCode);
            _prompt.ConsoleInput(ConResources.ResourceUsingCmd);
            _prompt.Callback(Environment.NewLine);
            _prompt.ConsoleInput(ConResources.ResourceListCmd);

            while (true)
            {
                lock (ConsoleLock)
                {
                    if (!_isEventTriggered)
                    {
                        _prompt.Callback(ConResources.ResourceInput);
                        var input = Console.ReadLine();
                        _prompt.ConsoleInput(input);
                    }
                    else
                    {
                        _prompt.Callback(ConResources.ResourceEventWait);
                    }
                }

                Thread.Sleep(500); // Small delay to prevent tight loop
            }
        }

        /// <summary>
        ///     Sends the commands.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private static void SendCommands(object sender, OutCommand e)
        {
            lock (ConsoleLock)
            {
                _isEventTriggered = true;
                // Simulate event processing
                _prompt.Callback(ConResources.ResourceEventWait);
                HandleCommands(e);
                _prompt.Callback(ConResources.ResourceEventProcessing);
                _isEventTriggered = false;
            }
        }

        /// <summary>
        ///     Handles the commands.
        /// </summary>
        /// <param name="outCommand">The out command.</param>
        private static void HandleCommands(OutCommand outCommand)
        {
            if (outCommand.Command == -1)
            {
                _prompt.Callback(outCommand.ErrorMessage);
            }

            if (outCommand.Command == 99)
            {
                // Simulate some work
                _prompt.Callback(ConResources.MessageClose);
                _prompt?.Dispose();
                // Introduce a small delay before closing
                Thread.Sleep(3000); // Delay for 3000 milliseconds (3 seconds)
                // Close the console application
                Environment.Exit(0);
            }

            string result;
            switch (outCommand.Command)
            {
                //Just show some stuff
                case ConResources.Header:
                    result = HandleHeader(outCommand);
                    _prompt.Callback(result);
                    break;
                case ConResources.Resxtract:
                    result = HandleResxtract(outCommand);
                    _prompt.Callback(result);
                    break;
                case ConResources.ResXtractOverload:
                    result = HandleResxtract(outCommand);
                    _prompt.Callback(result);
                    break;
                case ConResources.Analyzer:
                    result = RunAnalyzers(outCommand);
                    _prompt.Callback(result);
                    break;
                default:
                    _prompt.Callback(ConResources.ErrorCommandNotFound);
                    break;
            }
        }

        /// <summary>
        ///     Handles the header.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns>Added headers.</returns>
        private static string HandleHeader(OutCommand package)
        {
            var directoryPath = CleanPath(package.Parameter[0]);
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return ConResources.InformationDirectoryMissing;
            }

            if (!Directory.Exists(directoryPath))
            {
                return string.Format(ConResources.ErrorDirectory, directoryPath);
            }

            IHeaderExtractor headerExtractor = new HeaderExtractor();
            return headerExtractor.ProcessFiles(directoryPath, true);
        }

        /// <summary>
        ///     Handles the resource xtract.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns>Result of the extraction.</returns>
        private static string HandleResxtract(OutCommand package)
        {
            if (package.Parameter.Count == 0)
            {
                return ConResources.ErrorProjectPathMissing;
            }

            var projectPath = CleanPath(package.Parameter[0]);
            var outputResourceFile = package.Parameter.Count >= 2 ? CleanPath(package.Parameter[1]) : null;
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                return ConResources.ErrorProjectPathMissing;
            }

            if (!Directory.Exists(projectPath))
            {
                return string.Format(ConResources.ErrorProjectPath, projectPath);
            }

            // Only validate output file path if provided
            if (!string.IsNullOrWhiteSpace(outputResourceFile))
            {
                try
                {
                    var outputDir = Path.GetDirectoryName(outputResourceFile);
                    if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    {
                        return string.Format(ConResources.ErrorDirectoryOutput, outputDir);
                    }

                    // Optional: warn if file exists
                    if (File.Exists(outputResourceFile))
                    {
                        // Could add a warning here
                    }
                }
                catch (Exception ex)
                {
                    return string.Format(ConResources.ErrorAccessFile, ex.Message);
                }
            }

            var ignoreList = new List<string>();
            var ignorePatterns = new List<string> {};

            IResourceExtractor extractor = new ResXtract(ignoreList, ignorePatterns);
            var changedFiles =
                extractor.ProcessProject(projectPath, outputResourceFile, replace: true); // `null` is okay here
            if (changedFiles.Count == 0)
            {
                return ConResources.ResxtractFinished;
            }

            var actualOutputFile = changedFiles.Last(); // Last item is outputResourceFile (by design)
            var changedFilesList = string.Join(Environment.NewLine + ConResources.MessageSeparator,
                changedFiles.Take(changedFiles.Count - 1));
            return string.Format(ConResources.ResourceResxtractOutput, actualOutputFile, Environment.NewLine) +
                   string.Format(ConResources.MessageChangedFiles, Environment.NewLine, changedFilesList);
        }

        /// <summary>
        ///     Runs the analyzers.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns>Result of code analysis.</returns>
        private static string RunAnalyzers(OutCommand package)
        {
            var path = CleanPath(package.Parameter[0]);
            if (!Directory.Exists(path))
            {
                return string.Format(ConResources.ErrorDirectory, path);
            }

            var files = Directory.GetFiles(path, ConResources.ResourceCsExtension, SearchOption.AllDirectories);
            var result = string.Empty;
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                var syntaxTree = CSharpSyntaxTree.ParseText(content);
                foreach (var analyzer in Analyzers)
                {
                    foreach (var diagnostic in analyzer.Analyze(file, content))
                    {
                        result += string.Concat(diagnostic.ToString(), Environment.NewLine);
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Removes enclosing double quotes from a path if present.
        /// </summary>
        private static string CleanPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            path = path.Trim();
            if (path.StartsWith(ConResources.Quotes) && path.EndsWith(ConResources.Quotes) && path.Length > 1)
            {
                path = path.Substring(1, path.Length - 2);
            }

            return path;
        }

        /// <summary>
        ///     Listen to Messages
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private static void SendLogs(object sender, string e)
        {
            lock (ConsoleLock)
            {
                _isEventTriggered = true;
                Console.WriteLine(e);
                _isEventTriggered = false;
            }
        }
    }
}
