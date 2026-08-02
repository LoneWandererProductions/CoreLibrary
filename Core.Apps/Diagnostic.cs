/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps
 * FILE:        Diagnostic.cs
 * PURPOSE:     Class representing a diagnostic result.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global

using Core.Apps.Enums;
using System.IO;

namespace Core.Apps
{
    /// <summary>
    /// Diagnostic Result
    /// </summary>
    public sealed class Diagnostic
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Diagnostic" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="message">The message.</param>
        /// <param name="impact">The impact.</param>
        /// <param name="source">The source.</param>
        public Diagnostic(string name, DiagnosticSeverity severity, string? filePath, int lineNumber, string message,
            DiagnosticImpact? impact = null, string? source = "core.rules")
        {
            Name = name;
            Severity = severity;
            FilePath = filePath;
            LineNumber = lineNumber;
            Message = message;
            Impact = impact;
            Source = source ?? "core.rules";
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the severity.
        /// </summary>
        public DiagnosticSeverity Severity { get; }

        /// <summary>
        /// Gets the level string representation.
        /// </summary>
        public string Level => Severity.ToString();

        /// <summary>
        /// Gets the impact.
        /// </summary>
        public DiagnosticImpact? Impact { get; }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        public string? FilePath { get; }

        /// <summary>
        /// Gets the file name only (extracted from FilePath).
        /// </summary>
        public string FileName => string.IsNullOrEmpty(FilePath) ? string.Empty : Path.GetFileName(FilePath);

        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets the severity symbol (Emoji).
        /// </summary>
        public string SeveritySymbol => Severity switch
        {
            DiagnosticSeverity.Error => "\U0001F534", // 🔴
            DiagnosticSeverity.Warning => "\U0001F7E1", // 🟡
            DiagnosticSeverity.Info => "\U0001F535", // 🔵
            _ => "\u26AA" // ⚪
        };

        /// <summary>
        /// Converts to string.
        /// </summary>
        public override string ToString()
        {
            return Impact.HasValue
                ? $"{Name}, {Severity}, {FilePath}({LineNumber}): {Message} [{Impact.Value}]"
                : $"{Name}, {Severity}, {FilePath}({LineNumber}): {Message}";
        }
    }
}
