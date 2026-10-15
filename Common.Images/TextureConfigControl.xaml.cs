/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        TextureConfigControl.xaml.cs
 * PURPOSE:     The main Xaml for Texture Configuration
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Windows.Controls;
using Imaging.Enums;

namespace Common.Images
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///     Texture config Window
    /// </summary>
    public sealed partial class TextureConfigControl
    {
        /// <summary>
        /// The view model
        /// </summary>
        private readonly TextureConfigView _viewModel;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:SlimViews.TextureConfig" /> class.
        /// </summary>
        public TextureConfigControl()
        {
            InitializeComponent();
            _viewModel = new TextureConfigView();
            DataContext = _viewModel;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="TextureConfigControl" /> class with a specific texture type.
        /// </summary>
        /// <param name="texture">The texture type to select upon opening.</param>
        public TextureConfigControl(TextureType texture)
        {
            InitializeComponent();
            _viewModel = new TextureConfigView();
            DataContext = _viewModel;

            // Now we set the property on our explicit ViewModel instance
            _viewModel.SelectedTexture = texture;
        }
    }
}