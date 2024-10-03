/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/ColorPickerMenu.xaml.cs
 * PURPOSE:     Menu Item for the ColorPicker
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedType.Global

using System.Windows;
using System.Windows.Controls;
using Imaging;

namespace CommonControls
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///     Menu ITem for ColorPicker
    /// </summary>
    /// <seealso cref="UserControl" />
    public sealed partial class ColorPickerMenu
    {
        /// <summary>
        ///     The Color change delegate.
        /// </summary>
        /// <param name="colorHsv">The color HSV.</param>
        public delegate void DelegateColor(ColorHsv colorHsv);

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="ColorPickerMenu" /> class.
        /// </summary>
        public ColorPickerMenu()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets the colors.
        /// </summary>
        /// <value>
        ///     The colors.
        /// </value>
        public ColorHsv Colors => ColorPickerRegister.Colors;

        /// <summary>
        ///     An Image was clicked <see cref="DelegateColor" />.
        /// </summary>
        public event DelegateColor ColorChanged;

        /// <summary>
        ///     Set Colors.
        ///     Call back
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="alpha"></param>
        public void SetColors(int r, int g, int b, int alpha)
        {
            GridPicker.Children.Clear();

            var colorPick = new ColorPicker(r, g, b, alpha);
            AddColor(ColorPickerRegister.Colors);
            _ = GridPicker.Children.Add(colorPick);
        }

        /// <summary>
        ///     Handles the Click event of the Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Pop.IsOpen = true;
        }

        /// <summary>
        ///     Colors the picker menu color changed.
        /// </summary>
        /// <param name="colorHsv">he color Hsv.</param>
        private void ColorPickerMenu_ColorChanged(ColorHsv colorHsv)
        {
            AddColor(colorHsv);
            ColorChanged?.Invoke(colorHsv);
        }

        /// <summary>
        ///     Adds the color.
        /// </summary>
        /// <param name="colorHsv">The color HSV.</param>
        private void AddColor(ColorHsv colorHsv)
        {
            CanvasPreview.Children.Clear();

            var rectangle = ColorPickerHelper.GetColorPreview(colorHsv);

            _ = CanvasPreview.Children.Add(rectangle);
        }
    }
}