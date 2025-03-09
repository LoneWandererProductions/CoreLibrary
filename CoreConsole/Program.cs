/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreConsole
 * FILE:        CoreConsole/Program.cs
 * PURPOSE:     Basic Console app, to get my own tools running
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using CoreBuilder;
using System.IO;
using System.Text.RegularExpressions;

namespace CoreConsole
{
    /// <summary>
    /// Entry Point for my tools and future utilities.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            // Display welcome message
            Console.WriteLine("ResXtract Console Application");

            // Ensure the user has provided the necessary arguments
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ResXtractConsole <projectPath> <outputResourceFile> [<ignoreListFile> <ignorePatternFile>]");
                return;
            }

            string projectPath = args[0];
            string outputResourceFile = args[1];

            // Check if user wants to add any ignore list or patterns
            List<string> ignoreList = new List<string>();
            List<Regex> ignorePatterns = new List<Regex>();

            // Optionally, read the ignore list from a file if provided
            if (args.Length > 2 && File.Exists(args[2]))
            {
                ignoreList = new List<string>(File.ReadAllLines(args[2]));
                Console.WriteLine($"Loaded {ignoreList.Count} files to ignore.");
            }

            // Optionally, read the ignore patterns from a file if provided
            if (args.Length > 3 && File.Exists(args[3]))
            {
                foreach (var pattern in File.ReadAllLines(args[3]))
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

            // Create an instance of the ResXtract tool with the provided ignore list and patterns
            var extractor = new ResXtract(ignoreList, ignorePatterns);

            try
            {
                // Process the project to extract string literals from the code files
                Console.WriteLine($"Processing project at: {projectPath}...");
                extractor.ProcessProject(projectPath, outputResourceFile);

                // Notify the user that the process was successful
                Console.WriteLine($"Resource file has been successfully generated at: {outputResourceFile}");
            }
            catch (Exception ex)
            {
                // If something goes wrong, print an error message
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
