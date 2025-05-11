using System.Collections.Generic;
using Interpreter;

namespace CoreConsole
{
    internal static class ConResources
    {
        internal static readonly Dictionary<int, InCommand> DctCommandOne = new()
        {
            {
                0, new InCommand {Command = "header", ParameterCount = 1, Description = "Insert headers into C# files"}
            },
            {
                1,
                new InCommand
                {
                    Command = "resxtract", ParameterCount = 2, Description = "Extract resources from project files"
                }
            }
        };
    }
}
