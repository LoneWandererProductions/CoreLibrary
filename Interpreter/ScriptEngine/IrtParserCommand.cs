/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter.ScriptEngine
 * FILE:        Interpreter.ScriptEngine/IrtParserCommand.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using ExtendedSystemObjects;
using Interpreter.Resources;

namespace Interpreter.ScriptEngine
{
    internal static class IrtParserCommand
    {
        /// <summary>
        ///     Builds a categorized dictionary of commands from the given input string.
        /// </summary>
        /// <param name="input">The input string containing command definitions.</param>
        /// <returns>A CategorizedDictionary containing the parsed commands.</returns>
        internal static CategorizedDictionary<int, string> BuildCommand(string input)
        {
            // Remove unnecessary characters from the input string
            input = IrtKernel.CutLastOccurrence(input, IrtConst.AdvancedClose);
            input = IrtKernel.RemoveFirstOccurrence(input, IrtConst.AdvancedOpen);
            input = input.Trim();

            var formattedBlocks = new List<string>();
            var keepParsing = true;

            while (keepParsing)
            {
                var ifIndex = IrtKernel.FindFirstKeywordIndex(input, "if");
                if (ifIndex == -1)
                {
                    // No more if-else blocks; add the remaining input as a command block
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        continue;
                    }

                    var commands = IrtKernel.SplitParameter(input, IrtConst.NewCommand);
                    formattedBlocks.AddRange(commands);
                    keepParsing = false;
                }
                else
                {
                    // Extract the command block before the if-statement
                    var beforeIf = input.Substring(0, ifIndex).Trim();
                    if (!string.IsNullOrWhiteSpace(beforeIf))
                    {
                        formattedBlocks.Add(beforeIf);
                    }

                    // Isolate the if-else block and the remaining input
                    //TODO else is unused
                    input = input.Substring(ifIndex);
                    var (ifElseBlock, elsePosition) = IrtKernel.ExtractFirstIfElse(input);

                    // Ensure the blocks are properly added, add the whole else structure as a whole
                    formattedBlocks.Add(ifElseBlock.Trim());

                    // Remove the processed if-else block from the input string
                    input = input.Substring(ifElseBlock.Length).Trim();
                }
            }

            // Initialize a categorized dictionary for commands
            var commandRegister = new CategorizedDictionary<int, string>();

            foreach (var block in formattedBlocks)
            {
                var command = GetCommandType(block);

                switch (command)
                {
                    case IrtConst.InternalIf: // Handling if/else
                        //TODO error here
                        var rest = IrtParserIfElse.GenerateIfElseCommands(block);
                        break;
                    default: // Handling regular commands and splitting by new command separator
                        commandRegister.Add(command, commandRegister.Count, block);
                        break;
                }
            }

            // Process the if-else blocks with the external method
            //ProcessIfElseBlocks(formattedBlocks, commandRegister, ref commandIndex);

            return commandRegister;
        }

        /// <summary>
        ///     Gets the type of the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>The Command Type</returns>
        private static string GetCommandType(string command)
        {
            command = command.Trim();
            var keywordIndex = IrtKernel.GetCommandIndex(command, IrtConst.InternContainerCommands);
            return keywordIndex == -1 ? "Command" : IrtConst.InternContainerCommands[keywordIndex].Command;
        }
    }
}
