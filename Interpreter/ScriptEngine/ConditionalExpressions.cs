/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter.ScriptEngine
 * FILE:        Interpreter.ScriptEngine/ConditionalExpressions.cs
 * PURPOSE:     Handle all logical Operations of the parser
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 * 
 * 1. Translate while(condition) {...} to if(condition) {...} else {repeat}:
 *    - Use a custom "repeat" command to return to the if clause.
 *    - Simplify flow control by avoiding complex loop handling.
 * 
 * 2. Implement "repeat" as a generic command:
 *    - When "repeat" is hit, it returns to the calling flow statement.
 *    - Ensure correct flow by tracking execution within the workflow.
 * 
 * 3. Manage the stack:
 *    - Translate input code into an execution workflow.
 *    - Store all translated commands in a dictionary for execution.
 * 
 * 4. Future expansion:
 *    - Add support for "continue" and "break" commands.
 *    - Follow the same approach as "repeat" to manage these in the workflow.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ExtendedSystemObjects;
using Interpreter.Resources;

namespace Interpreter.ScriptEngine
{
    /// <summary>
    ///     Create Category Dictionary and handle nested structuresQ
    /// </summary>
    internal static class ConditionalExpressions
    {
        /// <summary>
        ///     Parses the given code string to extract all If-Else clauses.
        /// </summary>
        /// <param name="input">The input code string</param>
        /// <returns>A dictionary of IfElseObj objects representing each If-Else clause found.</returns>
        public static Dictionary<int, IfElseObj> ParseIfElseClauses(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            var ifElseClauses = new Dictionary<int, IfElseObj>();
            ProcessInput(input, false, -1, 0, 0, ifElseClauses);
            return ifElseClauses;
        }

        /// <summary>
        ///     Processes the input and splits the input into IfElseObj objects.
        /// </summary>
        private static void ProcessInput(string input, bool isElse, int parentId, int layer, int position,
            IDictionary<int, IfElseObj> ifElseClauses)
        {
            Trace.WriteLine($"ProcessInput called: layer={layer}, parentId={parentId}, position={position}");
            Trace.WriteLine($"Input: \"{input}\"");

            var obj = CreateIfElseObj(input, isElse, parentId, layer, position, ifElseClauses);
            Trace.WriteLine($"Created IfElseObj: Id={obj.Id}, Layer={obj.Layer}, ParentId={obj.ParentId}");

            ifElseClauses.Add(obj.Id, obj);

            var commands = IrtKernel.GetBlocks(input);
            Trace.WriteLine($"GetBlocks returned {commands.Count} block(s) for input at layer {layer}");

            obj.Commands ??= new CategorizedDictionary<int, string>();

            foreach (var (key, category, value) in commands)
            {
                Trace.WriteLine($"Processing block: Key={key}, Category={category}, Value=\"{value}\"");

                if ((category.Equals("If", StringComparison.OrdinalIgnoreCase) ||
                     category.Equals("Else", StringComparison.OrdinalIgnoreCase)) &&
                    IrtKernel.ContainsKeywordWithOpenParenthesis(value, IrtConst.InternalIf))
                {
                    int nestedIfIndex = IrtKernel.FindFirstKeywordIndex(value, IrtConst.InternalIf);

                    if (nestedIfIndex != IrtConst.Error)
                    {
                        var nestedIfBlock = value.Substring(nestedIfIndex).Trim();
                        Trace.WriteLine($"Detected nested if-block inside {category} block, recursing into it at layer {obj.Layer + 1}");
                        obj.Nested = true;
                        ProcessInput(nestedIfBlock, category.Equals("Else", StringComparison.OrdinalIgnoreCase), obj.Id, obj.Layer + 1, key, ifElseClauses);
                        continue; // skip adding the raw command, because we parsed it as nested IfElseObj
                    }
                }

                Trace.WriteLine("Adding command block without recursion");
                obj.Commands.Add(category, key, value);
            }

        }

        /// <summary>
        ///     Helper method to create an IfElseObj instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IfElseObj CreateIfElseObj(string input, bool isElse, int parentId, int layer, int position,
            IDictionary<int, IfElseObj> master)
        {
            return new IfElseObj
            {
                Input = input,
                Else = isElse,
                ParentId = parentId,
                Id = master.Count, // Use master.Count for sequential id
                Layer = layer,
                Position = position
            };
        }
    }
}
