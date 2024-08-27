/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/ImageFilterConfig.cs
 * PURPOSE:     Object that will be used for finetuning the filters
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMember.Global

namespace Imaging
{
    /// <summary>
    /// Settings for the Filter
    /// </summary>
    public class ImageFilterConfig
    {
        /// <summary>
        /// Gets or sets the factor.
        /// </summary>
        /// <value>
        /// The factor.
        /// </value>
        public double Factor { get; set; } = 1.0;
        /// <summary>
        /// Gets or sets the bias.
        /// </summary>
        /// <value>
        /// The bias.
        /// </value>
        public double Bias { get; set; } = 0.0;

    }
}
