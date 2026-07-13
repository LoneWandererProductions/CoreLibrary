/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Texture
 * FILE:        TextureConfig.cs
 * PURPOSE:     Primitive Data Configuration Object for Texture Generation Parameters
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

namespace Imaging.Texture
{
    /// <summary>
    /// A pure primitive configuration object. 
    /// No references to System.Drawing or higher-level Imaging concepts.
    /// </summary>
    public sealed class TextureConfig
    {
        /// <summary>
        /// Gets or sets the size of the turbulence.
        /// </summary>
        /// <value>
        /// The size of the turbulence.
        /// </value>
        public double TurbulenceSize { get; init; }

        /// <summary>
        /// Gets or sets the RGB ramp.
        /// </summary>
        /// <value>
        /// The RGB ramp.
        /// </value>
        public byte[]? RgbRamp { get; init; }

        /// <summary>
        /// Gets or sets the size of the cell.
        /// </summary>
        /// <value>
        /// The size of the cell.
        /// </value>
        public int CellSize { get; init; }

        /// <summary>
        /// Gets or sets the center RGB.
        /// </summary>
        /// <value>
        /// The center RGB.
        /// </value>
        public byte[]? CenterRgb { get; init; }

        /// <summary>
        /// Gets or sets the edge RGB.
        /// </summary>
        /// <value>
        /// The edge RGB.
        /// </value>
        public byte[]? EdgeRgb { get; init; }

        // --- New for Domain Warping ---

        /// <summary>
        /// Gets or sets the warp scale.
        /// </summary>
        /// <value>
        /// The warp scale.
        /// </value>
        public double WarpScale { get; init; }

        /// <summary>
        /// Gets or sets the warp strength.
        /// </summary>
        /// <value>
        /// The warp strength.
        /// </value>
        public double WarpStrength { get; init; }

        // --- New for Ridged Multifractal ---

        /// <summary>
        /// Gets or sets the octaves.
        /// </summary>
        /// <value>
        /// The octaves.
        /// </value>
        public int Octaves { get; init; }

        /// <summary>
        /// Gets or sets the persistence.
        /// </summary>
        /// <value>
        /// The persistence.
        /// </value>
        public double Persistence { get; init; }
    }
}
