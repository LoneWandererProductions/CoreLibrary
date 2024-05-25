using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Interpreter
{
    public static class IrtExtension
    {
        /// <summary>
        ///     Command Register
        /// </summary>
        private static Dictionary<int, InCommand> _com;

        /// <summary>
        ///     Namespace of Commands
        /// </summary>
        private static string _nameSpace;

        internal static void HandleInput(string inputString)
        {
            inputString = inputString.Trim();

            var parts = inputString.Split('.');
            var baseCommand = parts[0];
            var extensions = parts.Skip(1).ToList();

            if (!TryExecuteCommand(baseCommand, out var result))
            {
                //OnStatus(IrtConst.ErrorCommandNotFound);
                return;
            }

            foreach (var extension in extensions)
            {
                if (!TryExecuteExtension(result, extension, out result))
                {
                    //OnStatus(IrtConst.ErrorExtensionNotFound);
                    return;
                }
            }

            //OnStatus(result?.ToString());
        }

        private static bool TryExecuteCommand(string input, out object result)
        {
            result = null;

            // Existing logic to parse the base command and its parameters
            // ...

            //if (!_com.TryGetValue(key, out var command))
            {
                return false;
            }

            //var parameters = GetParameters(input, command.ParameterCount);

            //result = command.Execute(parameters);
            return true;
        }

        private static bool TryExecuteExtension(object baseResult, string extensionInput, out object result)
        {
            result = null;

            var match = Regex.Match(extensionInput, @"(\w+)\((.*?)\)");
            if (!match.Success)
            {
                return false;
            }

            var extensionName = match.Groups[1].Value;
            var parameterString = match.Groups[2].Value;
            var parameters = parameterString.Split(',').Select(p => p.Trim()).ToList();

            var baseCommand = _com.Values.FirstOrDefault(cmd => cmd.Extensions.ContainsKey(extensionName));
            if (baseCommand == null)
            {
                return false;
            }

            var extension = baseCommand.Extensions[extensionName];
            result = extension(baseResult, parameters);
            return true;
        }

        private static List<(string command, List<string> parameters)> ParseChainedCommands(string input)
        {
            var result = new List<(string, List<string>)>();
            var parts = input.Split('.');

            foreach (var part in parts)
            {
                var match = Regex.Match(part, @"(\w+)\((.*?)\)");

                if (match.Success)
                {
                    var command = match.Groups[1].Value;
                    var parameterString = match.Groups[2].Value;
                    var parameters = parameterString.Split(',').Select(p => p.Trim()).ToList();

                    result.Add((command, parameters));
                }
            }

            return result;
        }

        private static void SetResult(int key, List<string> parameters)
        {
            var command = _com[key];
            var result = command.Execute(parameters);

            foreach (var extension in command.Extensions)
            {
                result = extension.Value(result, parameters);
            }

            //OnCommand(new OutCommand { Command = key, Parameter = parameters, Result = result });
        }
    }
}
