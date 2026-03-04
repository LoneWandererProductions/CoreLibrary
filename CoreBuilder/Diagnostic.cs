/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        Diagnostic.cs
 * PURPOSE:     Class representing a diagnostic result.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global

using CoreBuilder.Enums;

namespace CoreBuilder
{
    /// <summary>
    ///     Diagnostic Result
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
        public Diagnostic(string name, DiagnosticSeverity severity, string filePath, int lineNumber, string message,
            DiagnosticImpact? impact = null)
        {
            Name = name;
            Severity = severity;
            FilePath = filePath;
            LineNumber = lineNumber;
            Message = message;
            Impact = impact;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the severity.
        /// </summary>
        /// <value>
        /// The severity.
        /// </value>
        public DiagnosticSeverity Severity { get; }

        /// <summary>
        /// Gets the impact.
        /// </summary>
        /// <value>
        /// The impact.
        /// </value>
        public DiagnosticImpact? Impact { get; } // nullable

        /// <summary>
        ///     Gets the file path.
        /// </summary>
        /// <value>
        ///     The file path.
        /// </value>
        public string FilePath { get; }

        /// <summary>
        ///     Gets the line number.
        /// </summary>
        /// <value>
        ///     The line number.
        /// </value>
        public int LineNumber { get; }

        /// <summary>
        ///     Gets the message.
        /// </summary>
        /// <value>
        ///     The message.
        /// </value>
        public string Message { get; }

        /// <summary>
        ///     Converts to string.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Impact.HasValue
                ? $"{Name}, {Severity}, {FilePath}({LineNumber}): {Message} [{Impact.Value}]"
                : $"{Name}, {Severity}, {FilePath}({LineNumber}): {Message}";
        }
    }
}