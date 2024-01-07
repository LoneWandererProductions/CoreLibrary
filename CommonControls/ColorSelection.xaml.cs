/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/ColorPicker.xaml.cs
 * PURPOSE:     Basic Color Picker Control
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

// ReSharper disable UnusedMember.Global, well we will use it
// ReSharper disable PossibleNullReferenceException, well this one should be quite impossible
// ReSharper disable UnusedAutoPropertyAccessor.Global, we use it
// ReSharper disable MemberCanBePrivate.Global, we will use it

namespace CommonControls
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     The Color Picker class.
    /// </summary>
    public sealed partial class ColorSelection
    {
        /// <summary>
        ///     DependencyProperty: DepColor
        ///     The selected Color (readonly). Value: DependencyProperty.Register StartColor
        /// </summary>
        public static readonly DependencyProperty DepColor = DependencyProperty.Register(nameof(DepColor),
            typeof(string),
            typeof(ColorSelection), null);

        /// <summary>
        ///     The color Dictionary.
        /// </summary>
        private Dictionary<string, Color> _colorDct;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:CommonControls.ColorPicker" /> class.
        /// </summary>
        public ColorSelection()
        {
            InitializeComponent();
            Initiate();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:CommonControls.ColorPicker" /> class.
        /// </summary>
        /// <param name="color">The color.</param>
        public ColorSelection(string color)
        {
            Color = color;
            InitializeComponent();
            Initiate();
        }

        /// <summary>
        ///     Gets or sets the Start color.
        /// </summary>
        public string Color
        {
            get => (string)GetValue(DepColor);
            set
            {
                SetValue(DepColor, value);
                SwitchColor();
            }
        }

        /// <summary>
        ///     Gets the color palette.
        /// </summary>
        public List<string> ColorPalette { get; private set; }

        /// <summary>
        ///     The initiate.
        ///     Sadly can't be handled onLoaded
        /// </summary>
        private void Initiate()
        {
            //Fill the ComboBox
            CmbColor.ItemsSource = typeof(Colors).GetProperties();

            //Generate Color Dictionary
            _colorDct = InitiateColors();

            //Generate a possible List of Colors we can use from Code
            ColorPalette = _colorDct.Keys.ToList();
        }

        /// <summary>
        ///     The Combo Box color selection changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void CmbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbColor?.SelectedItem is not PropertyInfo property) return;

            var selectedColor = (Color)property.GetValue(null, null);
            Color = _colorDct.FirstOrDefault(x => x.Value == selectedColor).Key;
        }

        /// <summary>
        ///     Switch the color of the Control.
        /// </summary>
        private void SwitchColor()
        {
            if (Color != null) CmbColor.SelectedItem = typeof(Colors).GetProperty(Color);
        }

        /// <summary>
        ///     Initiate colors.
        /// </summary>
        /// <returns>The <see cref="T:Dictionary{string, Color}" />.</returns>
        private static Dictionary<string, Color> InitiateColors()
        {
            return typeof(Colors).GetProperties().ToDictionary(property => property.Name,
                property => (Color)ColorConverter.ConvertFromString(property.Name));
        }
    }
}