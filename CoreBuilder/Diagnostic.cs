/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        Diagnostic.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace CoreBuilder
{
    /// <summary>
    /// Diagnostic Result
    /// </summary>
    public sealed class Diagnostic
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Diagnostic" /> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="message">The message.</param>
        public Diagnostic(string filePath, int lineNumber, string message)
        {
            FilePath = filePath;
            LineNumber = lineNumber;
            Message = message;
        }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public string FilePath { get; }

        /// <summary>
        /// Gets the line number.
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        public int LineNumber { get; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{FilePath}({LineNumber}): {Message}";
        }
    }
}
