/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        DocumentFormatException.cs
 * PURPOSE:     Thrown when a document file is damaged, unsupported or from a newer version.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents
{
    /// <summary>
    ///     The file is not a valid SlimViewer document.
    /// </summary>
    public sealed class DocumentFormatException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFormatException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DocumentFormatException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFormatException" /> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
        public DocumentFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
