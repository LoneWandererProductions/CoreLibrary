using System.Collections.Generic;
using System.Reflection.Metadata;
using Interpreter;

namespace CoreConsole
{
    /// <summary>
    /// Resource File
    /// </summary>
    internal static class ConResources
    {
        internal const int Header = 0;
        internal const int Resxtract = 1;
        internal const int Analyzer = 2;

        /// <summary>
        /// The available commands
        /// </summary>
        internal static readonly Dictionary<int, InCommand> DctCommandOne = new()
        {
            {
                Header, new InCommand { Command = "header", ParameterCount = 1, Description = "Insert headers into C# files" }
            },
            {
                1, new InCommand
                {
                    Command = "resxtract", ParameterCount = 2, Description = "Extract resources from project files"
                }
            },
            {
                Analyzer, new InCommand
                {
                    Command = "analyzer",
                    ParameterCount = 1,
                    Description = "Some basic code Checks for c# source files."
                }
            }
        };
    }
}
