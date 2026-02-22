/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     CommonControls.Images
* FILE:        ColorPicker.xaml.cs
* PURPOSE:     Basic Color Picker Control
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

using System;
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

namespace CommonControls.Images
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     The Color Picker class.
    /// </summary>
    public sealed partial class ColorSelection
    {
        /// <summary>
        /// DependencyProperty: DepColor
        /// The selected Color (readonly). Value: DependencyProperty.Register StartColor
        /// </summary>
        public static readonly DependencyProperty StartColorProperty =
            DependencyProperty.Register(
                nameof(StartColor),
                typeof(string),
                typeof(ColorSelection),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnStartColorChanged));

        /// <summary>
        ///     The color Dictionary.
        /// </summary>
        private Dictionary<string, Color> _colorDct;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="ColorPicker" /> class.
        /// </summary>
        public ColorSelection()
        {
            InitializeComponent();
            Loaded += (s, e) => Initiate(); // Ensure UI elements exist
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:CommonControls.ColorPicker" /> class.
        /// </summary>
        /// <param name="color">The color.</param>
        public ColorSelection(string color)
        {
            InitializeComponent();
            StartColor = color;
        }

        /// <summary>
        ///     Gets or sets the Start color.
        /// </summary>
        public string StartColor
        {
            get => (string)GetValue(StartColorProperty);
            set
            {
                SetValue(StartColorProperty, value);
            }
        }

        /// <summary>
        ///     Gets the color palette.
        /// </summary>
        public List<string> ColorPalette { get; private set; }

        /// <summary>
        ///     Handles the Loaded event of the UserControl ColorSelection.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void ColorSelection_Loaded(object sender, RoutedEventArgs e)
        {
            Initiate();
        }

        /// <summary>
        ///     Occurs when [color changed].
        /// </summary>
        public event EventHandler<string> ColorChanged;

        /// <summary>
        ///     The initiate.
        ///     Sadly can't be handled onLoaded
        /// </summary>
        private void Initiate()
        {
            var properties = typeof(Colors).GetProperties();
            _colorDct = properties.ToDictionary(
                p => p.Name,
                p => (Color)p.GetValue(null, null)
            );

            CmbColor.ItemsSource = properties;
            ColorPalette = _colorDct.Keys.ToList();

            SwitchToStartColor();
        }

        /// <summary>
        ///     The Combo Box color selection changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The selection changed event arguments.</param>
        private void CmbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (CmbColor?.SelectedItem is not PropertyInfo property) return;

                var selectedColor = (Color)property.GetValue(null, null);
                StartColor = _colorDct.FirstOrDefault(x => x.Value == selectedColor).Key;
                ColorChanged?.Invoke(this, StartColor);
            }
            catch (Exception ex) when (ex is ArgumentException or TargetException or TargetException
                                           or MethodAccessException)
            {
                ShowErrorMessageBox(ComCtlResources.ErrorColorSelection, ex);
            }
        }

        /// <summary>
        ///     Switch the color of the Control.
        /// </summary>
        private void SwitchColor()
        {
            try
            {
                SwitchToStartColor();
            }
            catch (Exception ex) when (ex is ArgumentException or AmbiguousMatchException)
            {
                ShowErrorMessageBox(ComCtlResources.ErrorSwitchingColor, ex);
            }
        }

        /// <summary>
        ///     Initiate colors.
        /// </summary>
        /// <returns>The <see cref="T:Dictionary{string, Color}" />.</returns>
        private static Dictionary<string, Color> InitiateColors()
        {
            try
            {
                return typeof(Colors).GetProperties()
                    .ToDictionary(property => property.Name,
                        property => (Color)ColorConverter.ConvertFromString(property.Name));
            }
            catch (Exception ex) when (ex is ArgumentException or FormatException)
            {
                ShowErrorMessageBox(ComCtlResources.ErrorInitializingColorDictionary, ex);
            }

            return new Dictionary<string, Color>();
        }

        /// <summary>
        ///     Switches to start color.
        /// </summary>
        private void SwitchToStartColor()
        {
            // Safety check: if the dictionary isn't built or ComboBox isn't ready, bail.
            if (string.IsNullOrEmpty(StartColor) || _colorDct == null || CmbColor == null)
            {
                return;
            }

            // Use the dictionary to find the property info to ensure consistency
            var property = typeof(Colors).GetProperty(StartColor);
            if (property != null)
            {
                CmbColor.SelectedItem = property;
            }
        }

        /// <summary>
        /// Called when [start color changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnStartColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorSelection picker && e.NewValue is string newColor)
            {
                picker.SwitchColor();
                picker.ColorChanged?.Invoke(picker, newColor);
            }
        }

        /// <summary>
        ///     Shows the error message box.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The ex.</param>
        private static void ShowErrorMessageBox(string message, Exception ex)
        {
            MessageBox.Show($"{message}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
