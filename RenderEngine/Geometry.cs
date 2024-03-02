/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        RenderEngine/Geometry.cs
 * PURPOSE:     Basic Attributes for all objects
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using Mathematics;
using SkiaSharp;

namespace RenderEngine
{
    /// <summary>
    ///     Basic needed Attributes
    /// </summary>
    public class Geometry
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Geometry" /> class.
        /// </summary>
        protected Geometry()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Geometry" /> class.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="color">The color.</param>
        public Geometry(Coordinate2D start, SKColor color)
        {
            Start = start;
            Color = color;
        }

        /// <summary>
        ///     Gets or sets the start.
        /// </summary>
        /// <value>
        ///     The start.
        /// </value>
        public Coordinate2D Start { get; set; }

        /// <summary>
        ///     Gets or sets the color.
        /// </summary>
        /// <value>
        ///     The color.
        /// </value>
        public SKColor Color { get; set; } = SKColors.Blue;

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Start.Id);
        }
    }
}
