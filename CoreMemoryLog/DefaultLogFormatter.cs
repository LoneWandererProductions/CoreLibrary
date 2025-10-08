/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreMemoryLog
 * FILE:        DefaultLogFormatter.cs
 * PURPOSE:     Debug Message Formatter
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreMemoryLog
{
    /// <summary>
    /// Debug message Formatter.
    /// </summary>
    /// <seealso cref="ILogFormatter" />
    public sealed class DefaultLogFormatter : ILogFormatter
    {
        /// <summary>
        /// The order
        /// </summary>
        private readonly List<string> _order;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLogFormatter"/> class.
        /// </summary>
        /// <param name="order">
        /// The order of fields to include in formatted output.
        /// Supported keys: Timestamp, LibraryName, Level, CallerMethod, Message.
        /// </param>
        public DefaultLogFormatter(IEnumerable<string>? order = null)
        {
            _order = order?.ToList() ?? new List<string>
            {
                "Timestamp",
                "LibraryName",
                "Level",
                "CallerMethod",
                "Message"
            };
        }

        /// <summary>
        /// Formats the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns>Formatted debug message.</returns>
        public string Format(ILogEntry entry)
        {
            var parts = new List<string>();

            foreach (var field in _order)
            {
                parts.Add(field switch
                {
                    "Timestamp" => $"[{entry.Timestamp:O}]",
                    "LibraryName" => $"[{entry.LibraryName}]",
                    "Level" => $"[{entry.Level}]",
                    "CallerMethod" => $"{entry.CallerMethod}:",
                    "Message" => entry.Args is { Length: > 0 }
                        ? SafeFormat(entry.Message, entry.Args)
                        : entry.Message ?? string.Empty,
                    _ => string.Empty
                });
            }

            var result = string.Join(" ", parts.Where(p => !string.IsNullOrEmpty(p)));

            // Add exception or stack info if present
            if (!string.IsNullOrEmpty(entry.MethodName))
                result += $"{Environment.NewLine}    at {entry.MethodName} in {entry.FileName}:{entry.LineNumber}";
            if (entry.Exception != null)
                result += $"{Environment.NewLine}    Exception: {entry.Exception}";

            return result;
        }

        /// <summary>
        /// Safes the format.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>Safe formatted message</returns>
        private static string SafeFormat(string? template, object[] args)
        {
            try { return template != null ? string.Format(template, args) : string.Empty; }
            catch { return template ?? string.Empty; }
        }
    }
}
