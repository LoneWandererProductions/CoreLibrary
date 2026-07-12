/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Interfaces
 * FILE:        IImageFrameProvider.cs
 * PURPOSE:     Interface wrapper for animated image formats
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedType.Global

using System;
using System.Windows.Media;

namespace Imaging.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    /// Interface wrapper for animated image formats
    /// </summary>
    /// <seealso cref="T:System.IDisposable" />
    public interface IImageFrameProvider : IDisposable
    {
        /// <summary>
        /// Gets the current frame.
        /// </summary>
        /// <value>
        /// The current frame.
        /// </value>
        ImageSource CurrentFrame { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is animated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is animated; otherwise, <c>false</c>.
        /// </value>
        bool IsAnimated { get; }

        /// <summary>
        /// Occurs when [frame changed].
        /// </summary>
        event EventHandler FrameChanged;

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();
    }
}