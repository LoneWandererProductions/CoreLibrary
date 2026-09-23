/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        CifColorItem.cs
 * PURPOSE:     Helper Object needed for Databinding in WPF.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using SystemDrawingColor = System.Drawing.Color;

namespace Common.Images
{
    /// <summary>
    /// Color Item of the image.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public sealed class CifColorItem : INotifyPropertyChanged
    {
        /// <summary>
        /// The r
        /// </summary>
        private byte _r;

        /// <summary>
        /// The g
        /// </summary>
        private byte _g;

        /// <summary>
        /// The b
        /// </summary>
        private byte _b;

        /// <summary>
        /// Gets the number of pixels using this color.
        /// </summary>
        public int PixelCount { get; }

        /// <summary>
        /// The is visible
        /// </summary>
        private bool _isVisible = true;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets the color of the source.
        /// </summary>
        /// <value>
        /// The color of the source.
        /// </value>
        public SystemDrawingColor SourceColor { get; }

        /// <summary>
        /// Gets the index of the original.
        /// </summary>
        /// <value>
        /// The index of the original.
        /// </value>
        public int OriginalIndex { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CifColorItem" /> class.
        /// </summary>
        /// <param name="originalColor">Color of the original.</param>
        /// <param name="originalIndex">Index of the original.</param>
        /// <param name="pixelCount">The pixel count.</param>
        public CifColorItem(SystemDrawingColor originalColor, int originalIndex = 0, int pixelCount = 0)
        {
            SourceColor = originalColor;
            OriginalIndex = originalIndex;
            PixelCount = pixelCount;
            _r = originalColor.R;
            _g = originalColor.G;
            _b = originalColor.B;
        }

        /// <summary>
        /// Gets or sets the r.
        /// </summary>
        /// <value>
        /// The r.
        /// </value>
        public byte R
        {
            get => _r;
            set
            {
                if (_r == value) return;

                _r = value;
                OnColorUpdated();
            }
        }

        /// <summary>
        /// Gets or sets the g.
        /// </summary>
        /// <value>
        /// The g.
        /// </value>
        public byte G
        {
            get => _g;
            set
            {
                if (_g == value) return;

                _g = value;
                OnColorUpdated();
            }
        }

        /// <summary>
        /// Gets or sets the b.
        /// </summary>
        /// <value>
        /// The b.
        /// </value>
        public byte B
        {
            get => _b;
            set
            {
                if (_b == value) return;

                _b = value;
                OnColorUpdated();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible == value) return;

                _isVisible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the hexadecimal value.
        /// </summary>
        /// <value>
        /// The hexadecimal value.
        /// </value>
        public string HexValue => $"#{R:X2}{G:X2}{B:X2}";

        /// <summary>
        /// Gets the display color.
        /// </summary>
        /// <value>
        /// The display color.
        /// </value>
        public Color DisplayColor => Color.FromArgb(SourceColor.A, R, G, B);

        /// <summary>
        /// Gets the color of the current drawing.
        /// </summary>
        /// <value>
        /// The color of the current drawing.
        /// </value>
        public SystemDrawingColor CurrentDrawingColor => SystemDrawingColor.FromArgb(SourceColor.A, R, G, B);

        /// <summary>
        /// Called when [color updated].
        /// </summary>
        private void OnColorUpdated()
        {
            OnPropertyChanged(nameof(R));
            OnPropertyChanged(nameof(G));
            OnPropertyChanged(nameof(B));
            OnPropertyChanged(nameof(HexValue));
            OnPropertyChanged(nameof(DisplayColor));
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
