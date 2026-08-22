/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     FileHandler
 * FILE:        FileHandlerException.cs
 * PURPOSE:     Exception Class
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

using System;

namespace FileHandler
{
    /// <inheritdoc />
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

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:FileHandler.FileHandlerException" /> class.
        /// </summary>
        public FileHandlerException()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:FileHandler.FileHandlerException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message describing the error.</param>
        public FileHandlerException(string message)
            : base(message)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:FileHandler.FileHandlerException" /> class with a specified
        /// error message and a reference to the inner exception that caused this exception.
        /// </summary>
        /// <param name="message">The message describing the error.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        public FileHandlerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:FileHandler.FileHandlerException" /> class with a specified
        /// error message and an associated file path.
        /// </summary>
        /// <param name="message">The message describing the error.</param>
        /// <param name="filePath">The file path related to the error.</param>
        public FileHandlerException(string message, string filePath)
            : base(message)
        {
            FilePath = filePath;
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:FileHandler.FileHandlerException" /> class with a specified
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
