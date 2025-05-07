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
using System.Text.RegularExpressions;
using CoreBuilder;

namespace CoreConsole
{
    /// <summary>
    ///     Entry Point for my tools and future utilities.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        ///     Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            Console.WriteLine("Core Console Application");

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: CoreConsole <operation> <projectPath> [options]");
                Console.WriteLine("Operations:");
                Console.WriteLine("  header <directoryPath>       Insert headers into C# files");
                Console.WriteLine(
                    "  resxtract <projectPath> <outputResourceFile> [<ignoreListFile> <ignorePatternFile>]");

                Console.WriteLine();
                Console.Write("No valid arguments provided. Would you like to enter them manually? (y/n): ");
                var response = Console.ReadLine()?.Trim().ToLower();
                if (response != "y")
                {
                    Console.WriteLine("Exiting...");
                    return;
                }

                Console.Write("Enter operation (header/resxtract): ");
                var operationInput = Console.ReadLine()?.Trim().ToLower();

                if (operationInput == "header")
                {
                    Console.Write("Enter directory path: ");
                    var directoryPath = Console.ReadLine();

                    if (!string.IsNullOrWhiteSpace(directoryPath))
                    {
                        IHeaderExtractor headerExtractor = new HeaderExtractor();
                        headerExtractor.ProcessFiles(directoryPath);
                    }
                    else
                    {
                        Console.WriteLine("Directory path is required.");
                    }
                }
                else if (operationInput == "resxtract")
                {
                    Console.Write("Enter project path: ");
                    var projectPath = Console.ReadLine();

                    Console.Write("Enter output resource file path: ");
                    var outputResourceFile = Console.ReadLine();

                    var ignoreList = new List<string>();
                    var ignorePatterns = new List<Regex>();

                    Console.Write("Enter ignore list file path (optional): ");
                    var ignoreListFile = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(ignoreListFile) && File.Exists(ignoreListFile))
                    {
                        ignoreList = new List<string>(File.ReadAllLines(ignoreListFile));
                        Console.WriteLine($"Loaded {ignoreList.Count} files to ignore.");
                    }

                    Console.Write("Enter ignore pattern file path (optional): ");
                    var ignorePatternFile = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(ignorePatternFile) && File.Exists(ignorePatternFile))
                    {
                        foreach (var pattern in File.ReadAllLines(ignorePatternFile))
                        {
                            try
                            {
                                ignorePatterns.Add(new Regex(pattern, RegexOptions.IgnoreCase));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error loading regex pattern: {pattern}. Exception: {ex.Message}");
                            }
                        }

                        Console.WriteLine($"Loaded {ignorePatterns.Count} ignore patterns.");
                    }

                    if (!string.IsNullOrWhiteSpace(projectPath) && !string.IsNullOrWhiteSpace(outputResourceFile))
                    {
                        IResourceExtractor resXtractExtractor = new ResXtract(ignoreList, ignorePatterns);
                        resXtractExtractor.ProcessProject(projectPath, outputResourceFile);
                    }
                    else
                    {
                        Console.WriteLine("Project path and output resource file are required.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid operation entered.");
                }
            }
            else
            {
                var operation = args[0];

                if (operation == "header" && args.Length == 2)
                {
                    var directoryPath = args[1];
                    IHeaderExtractor headerExtractor = new HeaderExtractor();
                    headerExtractor.ProcessFiles(directoryPath);
                }
                else if (operation == "resxtract" && args.Length >= 3)
                {
                    var projectPath = args[1];
                    var outputResourceFile = args[2];

                    var ignoreList = new List<string>();
                    var ignorePatterns = new List<Regex>();

                    if (args.Length > 3 && File.Exists(args[3]))
                    {
                        ignoreList = new List<string>(File.ReadAllLines(args[3]));
                        Console.WriteLine($"Loaded {ignoreList.Count} files to ignore.");
                    }

                    if (args.Length > 4 && File.Exists(args[4]))
                    {
                        foreach (var pattern in File.ReadAllLines(args[4]))
                        {
                            try
                            {
                                ignorePatterns.Add(new Regex(pattern, RegexOptions.IgnoreCase));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error loading regex pattern: {pattern}. Exception: {ex.Message}");
                            }
                        }

                        Console.WriteLine($"Loaded {ignorePatterns.Count} ignore patterns.");
                    }

                    IResourceExtractor resXtractExtractor = new ResXtract(ignoreList, ignorePatterns);
                    resXtractExtractor.ProcessProject(projectPath, outputResourceFile);
                }
                else
                {
                    Console.WriteLine("Invalid arguments or operation.");
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
