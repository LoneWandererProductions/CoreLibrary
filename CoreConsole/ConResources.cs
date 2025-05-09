using System.Collections.Generic;
using Interpreter;

namespace CoreConsole
{
    internal static class ConResources
    {
        internal static Dictionary<int, InCommand> commands = new()
        {
                { 0, new InCommand { Command = "header", ParameterCount = 2, Description = "Help com1" } },
                { 1, new InCommand { Command = "extract", ParameterCount = 2, Description = "Help com2" } },
                { 2, new InCommand { Command = "extract", ParameterCount = 3, Description = "Help com2" } }
            };
    }
}
