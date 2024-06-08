/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Serializer
 * FILE:        Serializer/Exceptions.cs
 * PURPOSE:     Serialize, Deserialize Exceptions 
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal

using System;

namespace Serializer
{
    /// <summary>
    /// Holds all Exceptions
    /// </summary>
    public static partial class Serialize
    {
        /// <inheritdoc />
        /// <summary>
        /// Serialization Exception
        /// </summary>
        /// <seealso cref="T:System.Exception" />
        public sealed partial class SerializationException
        {
            /// <inheritdoc />
            /// <summary>
            /// Initializes a new instance of the <see cref="T:Serializer.Serialize.SerializationException" /> class.
            /// </summary>
            /// <param name="message">The error message that explains the reason for the exception.</param>
            /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
            internal SerializationException(string message, Exception innerException)
                : base(message, innerException)
            {
            }

            /// <inheritdoc />
            /// <summary>
            /// Initializes a new instance of the <see cref="T:Serializer.Serialize.SerializationException" /> class.
            /// </summary>
            public SerializationException()
            {
            }

            /// <inheritdoc />
            /// <summary>
            /// Initializes a new instance of the <see cref="T:Serializer.Serialize.SerializationException" /> class.
            /// </summary>
            /// <param name="message">The message that describes the error.</param>
            public SerializationException(string message) : base(message)
            {
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Custom exception for deserialization errors.
        /// </summary>
        public sealed class DeserializationException : Exception
        {
            /// <inheritdoc />
            /// <summary>
            /// Initializes a new instance of the <see cref="T:Serializer.Serialize.DeserializationException" /> class.
            /// </summary>
            /// <param name="message">The error message that explains the reason for the exception.</param>
            /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.</param>
            internal DeserializationException(string message, Exception innerException)
                : base(message, innerException)
            {
            }

            /// <inheritdoc />
            /// <summary>
            /// Initializes a new instance of the <see cref="T:Serializer.Serialize.DeserializationException" /> class.
            /// </summary>
            public DeserializationException()
            {
            }

            /// <inheritdoc />
            /// <summary>
            /// Initializes a new instance of the <see cref="T:Serializer.Serialize.DeserializationException" /> class.
            /// </summary>
            /// <param name="message">The message that describes the error.</param>
            public DeserializationException(string message) : base(message)
            {
            }
        }
    }
}
