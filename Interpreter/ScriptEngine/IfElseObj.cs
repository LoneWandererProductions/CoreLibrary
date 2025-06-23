/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Interpreter.ScriptEngine
 * FILE:        Interpreter.ScriptEngine/IfElseObj.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using ExtendedSystemObjects;

namespace Interpreter.ScriptEngine
{
    internal sealed class IfElseObj
    {
        internal int Id { get; init; }
        internal int ParentId { get; init; }

        internal int Position { get; init; }

        internal int Layer { get; init; }
        internal bool Else { get; init; }

        internal bool Nested { get; set; }

        /// <summary>
        ///     Gets or sets the commands.
        ///     int is the key and id and, string is the category, int is the position of master entry
        /// </summary>
        /// <value>
        ///     The commands.
        /// </value>
        internal CategorizedDictionary<int, string> Commands { get; set; }

        internal string Input { get; init; }

        public override string ToString()
        {
            var commandsString = Commands != null ? string.Join(", ", Commands) : "No commands";

            return $"IfElseObj: Id = {Id}, ParentId = {ParentId}, Position = {Position}, Layer = {Layer}, " +
                   $"Else = {Else}, Nested = {Nested}, Commands = [{commandsString}], Input = \"{Input}\"";
        }
    }
}
