/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare.Compare
 * FILE:        ImageCompareData.cs
 * PURPOSE:     Compare results of 2 images
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBeInternal

namespace Imaging.Compare
{
    /// <summary>
    ///     Compare results between two images
    /// </summary>
    public sealed class ImageCompareData
    {
        /// <summary>
        ///     Gets the image one.
        /// </summary>
        /// <value>
        ///     The image one.
        /// </value>
        public string ImageOne { get; internal set; }

        /// <summary>
        ///     Gets the image two.
        /// </summary>
        /// <value>
        ///     The image two.
        /// </value>
        public string ImageTwo { get; internal set; }

        /// <summary>
        ///     Gets the similarity.
        /// </summary>
        /// <value>
        ///     The similarity.
        /// </value>
        public double Similarity { get; init; }
    }
}
