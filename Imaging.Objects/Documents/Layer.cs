/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        Layer.cs
 * PURPOSE:     The Layer Data Structure.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Imaging.Objects.Interfaces;

namespace Imaging.Objects.Documents
{
    /// <summary>
    ///     Base class of all layers. Layers are plain data holders; changes that should be undoable go
    ///     through <see cref="IDocumentCommand" />s.
    /// </summary>
    public abstract class Layer
    {
        /// <summary>
        /// The name
        /// </summary>
        private string? _name;

        /// <summary>
        /// The opacity
        /// </summary>
        private double _opacity = 1.0;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Layer" /> class.
        /// </summary>
        protected Layer(Guid id, string? name)
        {
            Id = id;
            _name = name ?? string.Empty;
        }

        /// <summary>
        /// Gets the stable identity of the layer.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public Guid Id { get; }

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string? Name
        {
            get => _name;
            set => _name = value ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the layer takes part in rendering.
        /// </summary>
        /// <value>
        ///   <c>true</c> if visible; otherwise, <c>false</c>.
        /// </value>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Gets or sets the opacity, clamped to 0..1.
        /// </summary>
        /// <value>
        /// The opacity.
        /// </value>
        public double Opacity
        {
            get => _opacity;
            set => _opacity = double.IsFinite(value) ? Math.Clamp(value, 0d, 1d) : 1d;
        }

        /// <summary>
        /// Gets or sets the blend mode.
        /// </summary>
        /// <value>
        /// The blend.
        /// </value>
        public BlendMode Blend { get; set; } = BlendMode.Normal;

        /// <summary>
        /// Gets or sets all user-visible settings at once.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        /// <exception cref="System.ArgumentNullException"></exception>
        public LayerProperties Properties
        {
            get => new(Name, Visible, Opacity, Blend);
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                Name = value.Name;
                Visible = value.Visible;
                Opacity = value.Opacity;
                Blend = value.Blend;
            }
        }

        /// <summary>
        /// Creates an independent copy with a new id (name defaults to "&lt;name&gt; copy").
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The duplicated layer.</returns>
        public abstract Layer Duplicate(string? name = null);

        /// <summary>
        /// Copies visibility, opacity and blend mode to <paramref name="target" />.
        /// </summary>
        /// <param name="target">The target.</param>
        protected void CopySettingsTo(Layer target)
        {
            target.Visible = Visible;
            target.Opacity = Opacity;
            target.Blend = Blend;
        }
    }
}