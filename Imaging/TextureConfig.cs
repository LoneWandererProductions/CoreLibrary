﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/TextureConfig.cs
 * PURPOSE:     Basic stuff for generating textures, this class is used for finetuning
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Sources:     https://lodev.org/cgtutor/randomnoise.html
 */

using System.Drawing;

namespace Imaging
{
    /// <summary>
    ///     Attributes for our texture generators
    /// </summary>
    public class TextureConfig
    {
        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        /// <value>
        /// The minimum value.
        /// </value>
        public int MinValue { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        /// <value>
        /// The maximum value.
        /// </value>
        public int MaxValue { get; set; } = 255;

        /// <summary>
        /// Gets or sets the alpha.
        /// </summary>
        /// <value>
        /// The alpha.
        /// </value>
        public int Alpha { get; set; } = 255;

        /// <summary>
        /// Gets or sets the x period.
        /// </summary>
        /// <value>
        /// The x period.
        /// </value>
        public double XPeriod { get; set; } = 5.0;

        /// <summary>
        /// Gets or sets the y period.
        /// </summary>
        /// <value>
        /// The y period.
        /// </value>
        public double YPeriod { get; set; } = 10.0;

        /// <summary>
        /// Gets or sets the turbulence power.
        /// </summary>
        /// <value>
        /// The turbulence power.
        /// </value>
        public double TurbulencePower { get; set; } = 5.0;

        /// <summary>
        /// Gets or sets the size of the turbulence.
        /// </summary>
        /// <value>
        /// The size of the turbulence.
        /// </value>
        public double TurbulenceSize { get; set; } = 64.0;

        /// <summary>
        /// Gets or sets the color of the base.
        /// </summary>
        /// <value>
        /// The color of the base.
        /// </value>
        public Color BaseColor { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is monochrome.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is monochrome; otherwise, <c>false</c>.
        /// </value>
        public bool IsMonochrome { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is tiled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is tiled; otherwise, <c>false</c>.
        /// </value>
        public bool IsTiled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to use smooth noise.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use smooth noise]; otherwise, <c>false</c>.
        /// </value>
        public bool UseSmoothNoise { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to use turbulence.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use turbulence]; otherwise, <c>false</c>.
        /// </value>
        public bool UseTurbulence { get; set; } = false;

        /// <summary>
        /// Gets or sets the xy period, used for wave and wood textures.
        /// </summary>
        /// <value>
        /// The xy period.
        /// </value>
        public double XYPeriod { get; set; } = 12.0;

        /// <summary>
        /// Gets or sets the filter type for generating textures.
        /// </summary>
        /// <value>
        /// The filter type.
        /// </value>
        public TextureType Filter { get; set; } = TextureType.Noise;

        /// <summary>
        /// Gets or sets the shape of the texture.
        /// </summary>
        /// <value>
        /// The texture shape.
        /// </value>
        public TextureShape Shape { get; set; } = TextureShape.Rectangle;

        /// <summary>
        /// Gets or sets the shape parameters.
        /// </summary>
        /// <value>
        /// The shape parameters.
        /// </value>
        public object ShapeParams { get; set; } = null;

        // New parameters for crosshatch texture

        /// <summary>
        /// Gets or sets the spacing between lines for crosshatch texture.
        /// </summary>
        /// <value>
        /// The spacing between lines.
        /// </value>
        public int LineSpacing { get; set; } = 10;

        /// <summary>
        /// Gets or sets the color of the lines for crosshatch texture.
        /// </summary>
        /// <value>
        /// The color of the lines.
        /// </value>
        public Color LineColor { get; set; } = Color.Black;

        /// <summary>
        /// Gets or sets the thickness of the lines for crosshatch texture.
        /// </summary>
        /// <value>
        /// The thickness of the lines.
        /// </value>
        public int LineThickness { get; set; } = 1;

        /// <summary>
        /// Gets or sets the angle of the first set of lines for crosshatch texture, in degrees.
        /// </summary>
        /// <value>
        /// The angle of the first set of lines.
        /// </value>
        public double Angle1 { get; set; } = 45.0;

        /// <summary>
        /// Gets or sets the angle of the second set of lines for crosshatch texture, in degrees.
        /// </summary>
        /// <value>
        /// The angle of the second set of lines.
        /// </value>
        public double Angle2 { get; set; } = 135.0;
    }
}
