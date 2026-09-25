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
        /// <summary>Initializes a new instance of the <see cref="DocumentFormatException" /> class.</summary>
        public DocumentFormatException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DocumentFormatException" /> class.</summary>
        public DocumentFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
