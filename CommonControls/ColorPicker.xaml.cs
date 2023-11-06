/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/ColorPicker.xaml.cs
 * PURPOSE:     Base UserControl for the ColorPicker
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Imaging;
using Mathematics;

namespace CommonControls
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     ColorPicker Control
    /// </summary>
    public sealed partial class ColorPicker : INotifyPropertyChanged
    {
        /// <summary>
        ///     The Color change delegate.
        /// </summary>
        /// <param name="colorHsv">The color HSV.</param>
        public delegate void DelegateColor(ColorHsv colorHsv);

        /// <summary>
        ///     The h
        /// </summary>
        public static readonly DependencyProperty H = DependencyProperty.Register(nameof(H),
            typeof(string),
            typeof(ColorPicker), null);

        /// <summary>
        ///     The s
        /// </summary>
        public static readonly DependencyProperty S = DependencyProperty.Register(nameof(S),
            typeof(string),
            typeof(ColorPicker), null);

        /// <summary>
        ///     The v
        /// </summary>
        public static readonly DependencyProperty V = DependencyProperty.Register(nameof(V),
            typeof(string),
            typeof(ColorPicker), null);

        /// <summary>
        ///     The alpha
        /// </summary>
        private int _alpha = 255;

        /// <summary>
        ///     The b
        /// </summary>
        private int _b;

        /// <summary>
        ///     The g
        /// </summary>
        private int _g;

        /// <summary>
        ///     The hexadecimal
        /// </summary>
        private string _hex;

        /// <summary>
        ///     The hue
        /// </summary>
        private double _hue;

        /// <summary>
        ///     The r
        /// </summary>
        private int _r;

        /// <summary>
        ///     The sat
        /// </summary>
        private double _sat;

        /// <summary>
        ///     The value
        /// </summary>
        private double _val;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="ColorPicker" /> class.
        /// </summary>
        public ColorPicker()
        {
            InitializeComponent();
            DataContext = this;
            LoadColors();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="ColorPicker" /> class.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="alpha">The alpha.</param>
        public ColorPicker(int r, int g, int b, int alpha)
        {
            R = r;
            G = g;
            B = b;
            Alpha = alpha;
            InitializeComponent();
            DataContext = this;

            var hsv = new ColorHsv(R, G, B, Alpha);

            Hue = hsv.H;
            Val = hsv.V;
            Sat = hsv.S;
            Hex = hsv.Hex;

            ColorPickerRegister.Colors = hsv;

            ColorPick.Initiate(_hue, _sat, _val);
            LoadColors();
            SetPreview();
        }

        /// <summary>
        ///     Gets the colors.
        /// </summary>
        /// <value>
        ///     The colors.
        /// </value>
        public static ColorHsv Colors => ColorPickerRegister.Colors;

        /// <summary>
        ///     Gets or sets the hue.
        /// </summary>
        /// <value>
        ///     The hue.
        ///     Max Value 360, min 0, Hue is in rad!
        /// </value>
        public double Hue
        {
            get => _hue;
            set
            {
                if (_hue.IsEqualTo(value, 10) || value * 180 / Math.PI is > 360 or < 0)
                {
                    return;
                }

                ColorPickerRegister.ColorChanged = true;

                _hue = value;
                OnPropertyChanged(nameof(Hue));
            }
        }

        /// <summary>
        ///     Gets or sets the sat.
        /// </summary>
        /// <value>
        ///     The sat.
        ///     Max 1, Min 0
        /// </value>
        public double Sat
        {
            get => _sat;
            set
            {
                if (_sat.IsEqualTo(value, 10) || value is > 1 or < 0)
                {
                    return;
                }

                ColorPickerRegister.ColorChanged = true;

                _sat = value;
                OnPropertyChanged(nameof(Sat));
            }
        }

        /// <summary>
        ///     Gets or sets the replacement string.
        /// </summary>
        /// <value>
        ///     The replacement string.
        ///     Max 1, Min 0
        /// </value>
        public double Val
        {
            get => _val;
            set
            {
                if (_val.IsEqualTo(value, 10) || value is > 1 or < 0)
                {
                    return;
                }

                ColorPickerRegister.ColorChanged = true;

                _val = value;
                OnPropertyChanged(nameof(Val));
            }
        }

        /// <summary>
        ///     Gets or sets the r.
        /// </summary>
        /// <value>
        ///     The r.
        ///     Max 255, Min 0
        /// </value>
        public int R
        {
            get => _r;
            set
            {
                if (_r == value || value is > 255 or < 0)
                {
                    return;
                }

                ColorPickerRegister.ColorChanged = true;

                _r = value;
                OnPropertyChanged(nameof(R));
            }
        }

        /// <summary>
        ///     Gets or sets the g.
        /// </summary>
        /// <value>
        ///     The g.
        ///     Max 255, Min 0
        /// </value>
        public int G
        {
            get => _g;
            set
            {
                if (_g == value || value is > 255 or < 0)
                {
                    return;
                }

                ColorPickerRegister.ColorChanged = true;

                _g = value;
                OnPropertyChanged(nameof(G));
            }
        }

        /// <summary>
        ///     Gets or sets the b.
        /// </summary>
        /// <value>
        ///     The b.
        ///     Max 255, Min 0
        /// </value>
        public int B
        {
            get => _b;
            set
            {
                if (_b == value || value is > 255 or < 0)
                {
                    return;
                }

                ColorPickerRegister.ColorChanged = true;

                _b = value;
                OnPropertyChanged(nameof(B));
            }
        }

        /// <summary>
        ///     Gets or sets the alpha.
        /// </summary>
        /// <value>
        ///     The alpha.
        /// </value>
        public int Alpha
        {
            get => _alpha;
            set
            {
                if (_alpha == value || value is > 255 or < 0)
                {
                    return;
                }

                ColorPickerRegister.ColorChanged = true;

                _alpha = value;
                OnPropertyChanged(nameof(Alpha));
            }
        }

        /// <summary>
        ///     Gets the alpha percentage.
        /// </summary>
        /// <value>
        ///     The alpha percentage.
        /// </value>
        public int AlphaPercentage => (int)((double)_alpha / 255 * 100);

        /// <summary>
        ///     Gets or sets the hexadecimal
        /// </summary>
        /// <value>
        ///     The hexadecimal.
        /// </value>
        public string Hex
        {
            get => _hex;
            set
            {
                if (_hex == value)
                {
                    return;
                }

                ColorPickerRegister.ColorChanged = true;

                _hex = value;
                OnPropertyChanged(nameof(Hex));
            }
        }

        /// <summary>
        ///     Gets or sets the Dependency hue.
        /// </summary>
        /// <value>
        ///     The Dependency hue.
        /// </value>
        public double DepHue
        {
            get => (double)GetValue(H);
            set => SetValue(H, value);
        }

        /// <summary>
        ///     Gets or sets the Dependency sat.
        /// </summary>
        /// <value>
        ///     The Dependency saturation.
        /// </value>
        public double DepSat
        {
            get => (double)GetValue(S);
            set => SetValue(S, value);
        }

        /// <summary>
        ///     Gets or sets the Dependency value.
        /// </summary>
        /// <value>
        ///     The Dependency value.
        /// </value>
        public double DepVal
        {
            get => (double)GetValue(V);
            set => SetValue(V, value);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Triggers if an Attribute gets changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     An Image was clicked <see cref="DelegateColor" />.
        /// </summary>
        public event DelegateColor ColorChanged;

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Loads the colors.
        /// </summary>
        private void LoadColors()
        {
            using var img = ColorPick.InitiateBackGround();
            ImageOne.Source = img.ToBitmapImage();
            //set Cursor
            using var imgTwo = ColorPick.InitiateCursor();
            ImageTwo.Source = imgTwo.ToBitmapImage();
        }

        /// <summary>
        ///     Handles the MouseDown event of the ImageTwo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        private void ImageTwo_MouseDown(object sender, MouseEventArgs e)
        {
            var x = e.GetPosition(ImageOne).X;
            var y = e.GetPosition(ImageOne).Y;

            ColorPick.Selection(x, y, Alpha);

            if (!Hue.IsEqualTo(ColorPickerRegister.Colors.H, 10))
            {
                using var img = ColorPick.InitiateBackGround();
                ImageOne.Source = img.ToBitmapImage();
                //set Cursor
                using var imgTwo = ColorPick.InitiateCursor();
                ImageTwo.Source = imgTwo.ToBitmapImage();
            }

            if (!Val.IsEqualTo(ColorPickerRegister.Colors.V, 10) || !Sat.IsEqualTo(ColorPickerRegister.Colors.S, 10))
            {
                //set Cursor
                using var imgTwo = ColorPick.InitiateCursor();
                ImageTwo.Source = imgTwo.ToBitmapImage();
            }

            Hue = ColorPickerRegister.Colors.H;
            Val = ColorPickerRegister.Colors.V;
            Sat = ColorPickerRegister.Colors.S;

            R = ColorPickerRegister.Colors.R;
            G = ColorPickerRegister.Colors.G;
            B = ColorPickerRegister.Colors.B;
            Alpha = ColorPickerRegister.Colors.A;

            Hex = ColorPickerRegister.Colors.Hex;

            SetPreview();
        }

        /// <summary>
        ///     Sets the preview Color.
        /// </summary>
        private void SetPreview()
        {
            CanvasPreview.Children.Clear();

            var rectangle = ColorPickerHelper.GetColorPreview(Colors);

            _ = CanvasPreview.Children.Add(rectangle);

            if (ColorPickerRegister.ColorChanged)
            {
                OnColorChanged();
            }
        }

        /// <summary>
        ///     Called when [color changed].
        /// </summary>
        private void OnColorChanged()
        {
            ColorChanged?.Invoke(ColorPickerRegister.Colors);
        }
    }
}
