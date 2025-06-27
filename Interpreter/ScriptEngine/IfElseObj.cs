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

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var indent = new string(' ', Layer * 2);
            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"{indent}IfElseObj: Id = {Id}, ParentId = {ParentId}, Position = {Position}, Layer = {Layer}, Else = {Else}, Nested = {Nested}");
            sb.AppendLine($"{indent}Input = \"{Input}\"");

            if (Commands != null && Commands.Count > 0)
            {
                sb.AppendLine($"{indent}Commands:");
                foreach (var (key, category, value) in Commands)
                {
                    sb.AppendLine($"{indent}  [Key: {key}, Category: {category}, Value: {value}]");
                }
            }
            else
            {
                sb.AppendLine($"{indent}Commands: <none>");
            }

            return sb.ToString();
        }
    }
}
