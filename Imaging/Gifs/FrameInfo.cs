/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Gifs
 * FILE:        FrameInfo.cs
 * PURPOSE:     Class Container that holds all informations about the frames of the gif in question.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */


using System.Drawing;

namespace Imaging.Gifs
{
    /// <summary>
    ///     Infos about the frame and timing
    /// </summary>
    public sealed class FrameInfo
    {
        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the delay time.
        /// </summary>
        /// <value>
        ///     The delay time.
        /// </value>
        public double DelayTime { get; init; } // Delay time in seconds


        /// <summary>
        ///     Gets or sets the image.
        /// </summary>
        /// <value>
        ///     The image.
        /// </value>
        public Bitmap Image { get; set; } // Image of the frame
    }
}
