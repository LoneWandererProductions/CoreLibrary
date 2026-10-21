/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents
 * FILE:        LayerDto.cs
 * PURPOSE:     Data Transfer Object for layers
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Shapes;

namespace Imaging.Objects.Documents
{
    /// <summary>
    /// Data Transfer Object for layers.
    /// </summary>
    public sealed class LayerDto
    {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LayerDto"/> is visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if visible; otherwise, <c>false</c>.
        /// </value>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Gets or sets the opacity.
        /// </summary>
        /// <value>
        /// The opacity.
        /// </value>
        public double Opacity { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the blend.
        /// </summary>
        /// <value>
        /// The blend.
        /// </value>
        public string? Blend { get; set; }

        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        /// <value>
        /// The file.
        /// </value>
        public string? File { get; set; }

        /// <summary>
        /// Gets or sets the shapes.
        /// </summary>
        /// <value>
        /// The shapes.
        /// </value>
        public List<Shape>? Shapes { get; set; }
    }
}
