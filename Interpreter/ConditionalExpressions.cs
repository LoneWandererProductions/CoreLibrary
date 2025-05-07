/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter
 * FILE:        Interpreter/ConditionalExpressions.cs
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
using System.Runtime.CompilerServices;
using ExtendedSystemObjects;

namespace Interpreter
{
    /// <summary>
    ///     Create Category Dictionary and handle nested structures
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
            ProcessInput(input, false, -1, -1, 0, ifElseClauses);
            return ifElseClauses;
        }

        /// <summary>
        ///     Processes the input and splits the input into IfElseObj objects.
        /// </summary>
        private static void ProcessInput(string input, bool isElse, int parentId, int layer, int position,
            IDictionary<int, IfElseObj> ifElseClauses)
        {
            var obj = CreateIfElseObj(input, isElse, parentId, layer, position, ifElseClauses);
            ifElseClauses.Add(obj.Id, obj);

            var commands = IrtKernel.GetBlocks(input);
            obj.Commands ??= new CategorizedDictionary<int, string>();

            // Process each block of commands
            foreach (var (key, category, value) in commands)
            {
                var containsIf = IrtKernel.ContainsKeywordWithOpenParenthesis(value, IrtConst.InternalIf);
                if (!containsIf)
                {
                    //position indicates current block pretty useless for now
                    obj.Commands.Add(category, key, position.ToString()); // Add the block if it doesn't contain 'if'
                    continue;
                }

                // Add a reference to the current block as a nested "if" block
                obj.Commands.Add(ConditionalResources.InternalCategoryNested, obj.Commands.Count, value);
                obj.Nested = true;
                var isElseBlock = category.Equals(IrtConst.InternalElse, StringComparison.OrdinalIgnoreCase);
                ProcessInput(value, isElseBlock, obj.Id, obj.Layer, key, ifElseClauses);
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
                Layer = layer + 1,
                Position = position
            };
        }
    }
}
