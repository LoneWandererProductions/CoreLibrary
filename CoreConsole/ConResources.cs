using System.Collections.Generic;
using Interpreter;

namespace CoreConsole
{
    /// <summary>
    /// Resource File
    /// </summary>
    internal static class ConResources
    {
        /// <summary>
        /// The DCT command one
        /// </summary>
        internal static readonly Dictionary<int, InCommand> DctCommandOne = new()
        {
            {
                0, new InCommand { Command = "header", ParameterCount = 1, Description = "Insert headers into C# files" }
            },
            {
                1, new InCommand
                {
                    Command = "resxtract", ParameterCount = 2, Description = "Extract resources from project files"
                }
            },
            {
                2, new InCommand
                {
                    Command = "analyzer",
                    ParameterCount = 2,
                    Description = "Some basic code Checks for c# source files."
                }
            }
        };
    }
}
