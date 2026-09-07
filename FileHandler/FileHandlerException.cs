/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     FileHandler
 * FILE:        FileHandlerException.cs
 * PURPOSE:     Exception Class
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;

namespace FileHandler
{
    /// <summary>
    /// Represents errors that occur within the <c>FileHandler</c> library.
    /// </summary>
    /// <remarks>
    /// This exception is thrown for library-specific errors, providing a consistent type
    /// for users to catch and handle separately from standard .NET exceptions.
    /// </remarks>
    public sealed class FileHandlerException : Exception
    {
        /// <summary>
        /// Gets the path of the file associated with this exception, if any.
        /// </summary>
        public string? FilePath { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHandlerException"/> class.
        /// </summary>
        public FileHandlerException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHandlerException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message describing the error.</param>
        public FileHandlerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHandlerException"/> class with a specified
        /// error message and a reference to the inner exception that caused this exception.
        /// </summary>
        /// <param name="message">The message describing the error.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        public FileHandlerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHandlerException"/> class with a specified
        /// error message and an associated file path.
        /// </summary>
        /// <param name="message">The message describing the error.</param>
        /// <param name="filePath">The file path related to the error.</param>
        public FileHandlerException(string message, string filePath)
            : base(message)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHandlerException"/> class with a specified
        /// error message, associated file path, and a reference to the inner exception that caused this exception.
        /// </summary>
        /// <param name="message">The message describing the error.</param>
        /// <param name="filePath">The file path related to the error.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        public FileHandlerException(string message, string filePath, Exception innerException)
            : base(message, innerException)
        {
            FilePath = filePath;
        }
    }
}