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
            // Display welcome message
            Console.WriteLine("Core Console Application");

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: CoreConsole <operation> <projectPath> [options]");
                Console.WriteLine("Operations:");
                Console.WriteLine("  header <directoryPath>       Insert headers into C# files");
                Console.WriteLine("  resxtract <projectPath> <outputResourceFile> [<ignoreListFile> <ignorePatternFile>]");
                return;
            }

            string operation = args[0];

            if (operation == "header" && args.Length == 2)
            {
                // Header insertion operation
                string directoryPath = args[1];

                IHeaderExtractor headerExtractor = new HeaderExtractor();
                headerExtractor.ProcessFiles(directoryPath);
            }
            else if (operation == "resxtract" && args.Length >= 3)
            {
                // ResXtract operation
                string projectPath = args[1];
                string outputResourceFile = args[2];

                List<string> ignoreList = new List<string>();
                List<Regex> ignorePatterns = new List<Regex>();

                // Optionally read ignore list from file
                if (args.Length > 3 && File.Exists(args[3]))
                {
                    ignoreList = new List<string>(File.ReadAllLines(args[3]));
                    Console.WriteLine($"Loaded {ignoreList.Count} files to ignore.");
                }

                // Optionally read ignore patterns from file
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

                // Create an instance of ResXtractExtractor
                IResourceExtractor resXtractExtractor = new ResXtract(ignoreList, ignorePatterns);
                resXtractExtractor.ProcessProject(projectPath, outputResourceFile);
            }
            else
            {
                Console.WriteLine("Invalid arguments or operation.");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
