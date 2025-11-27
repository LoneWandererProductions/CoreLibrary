/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Lector
 * FILE:        Program.cs
 * PURPOSE:     Entry point for executing commands via the Weaver engine.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using CoreBuilder;
using Weaver;

namespace Lector
{
    internal static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            var weave = new Weave();
            RegisterApps(weave);

            // Initial demo commands
            var result = weave.ProcessInput("help()");
            Console.WriteLine(result.Message);

            result = weave.ProcessInput("list()");
            Console.WriteLine(result.Message);

            var exit = false;

            do
            {
                Console.Write("> ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    exit = true;
                }
                else
                {
                    // Execute a command with optional namespace and extension
                    result = weave.ProcessInput(input);

                    // The engine ensures feedback is handled internally
                    Console.WriteLine(result.Message);
                }
            } while (!exit);
        }

        /// <summary>
        /// Registers the apps.
        /// </summary>
        /// <param name="weave">The weave.</param>
        private static void RegisterApps(Weave weave)
        {
            var modules = AnalyzerFactory.GetCommands();

            foreach (var module in modules)
                weave.Register(module);
        }
    }
}