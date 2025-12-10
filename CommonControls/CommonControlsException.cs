/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/CommonControlsException.cs
 * PURPOSE:     CommonControls Exception Class
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Runtime.Serialization;

// ReSharper disable MemberCanBeInternal

namespace CommonControls;

/// <inheritdoc />
/// <summary>
///     The common controls exception class.
/// </summary>
[Serializable]
public sealed class CommonControlsException : Exception
{
    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="T:CommonControls.CommonControlsException" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    internal CommonControlsException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="T:CommonControls.CommonControlsException" /> class.
    /// </summary>
    /// <param name="info">The info.</param>
    /// <param name="context">The context.</param>
    private CommonControlsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="T:CommonControls.CommonControlsException" /> class.
    /// </summary>
    public CommonControlsException()
    {
    }

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="T:CommonControls.CommonControlsException" /> class.
    /// </summary>
    /// <param name="message">The message we declare</param>
    /// <param name="innerException">
    ///     The Exception that caused the Exception or a null reference <see langword="Nothing" /> in
    ///     Visual Basic), if there is no inner Exception.
    /// </param>
    public CommonControlsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}