using System;
using CoreBuilder;

namespace CoreConsole
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // Display welcome message
            Console.WriteLine("ResXtract Console Application");

            // Ensure the user has provided the necessary arguments
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: ResXtractConsole <projectPath> <outputResourceFile>");
                return;
            }

            string projectPath = args[0];
            string outputResourceFile = args[1];

            // Create an instance of the ResXtract tool
            var extractor = new ResXtract();

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
        }
    }
}
