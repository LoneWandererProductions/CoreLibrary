/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Messages
 * FILE:        ParsedCommand.cs
 * PURPOSE:     Results of parsing a raw command input.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;

namespace Weaver.Messages
{
    /// <summary>
    /// Represents the result of parsing a raw command input.
    /// example: "delete(file.txt).saveTo(backupFolder)"
    /// returns ParsedCommand:
    /// Name = "delete", Args = ["file.txt"], Extension = "saveTo", ExtensionArgs = ["backupFolder"]
    /// </summary>
    [DebuggerDisplay("{ToString(),nq}")]
    public sealed class ParsedCommand
    {
        /// <summary>
        /// The optional namespace of the command (e.g. "system").
        /// </summary>
        public string Namespace { get; init; } = string.Empty;

        /// <summary>
        /// The command name (e.g. "echo").
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// The arguments for the main command.
        /// </summary>
        public string[] Args { get; init; } = Array.Empty<string>();

        /// <summary>
        /// The optional extension name (e.g. "help").
        /// </summary>
        public string Extension { get; init; } = string.Empty;

        /// <summary>
        /// The arguments for the extension, if any.
        /// </summary>
        public string[] ExtensionArgs { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Returns a human-readable string representation of the parsed command,
        /// including namespace, main args, extension, and extension args.
        /// </summary>
        public override string ToString()
        {
            var nsPart = string.IsNullOrEmpty(Namespace) ? string.Empty : $"{Namespace}:";
            var argsPart = Args.Length == 0 ? "()" : $"({string.Join(", ", Args)})";
            var extPart = string.IsNullOrEmpty(Extension)
                ? string.Empty
                : $".{Extension}({string.Join(", ", ExtensionArgs)})";

            return $"{nsPart}{Name}{argsPart}{extPart}";
        }
    }
}