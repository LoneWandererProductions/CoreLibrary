/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBuilder
 * FILE:        Diagnostic.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace CoreBuilder
{
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

        public string FilePath { get; }
        public int LineNumber { get; }
        public string Message { get; }

        public override string ToString()
        {
            return $"{FilePath}({LineNumber}): {Message}";
        }
    }
}
