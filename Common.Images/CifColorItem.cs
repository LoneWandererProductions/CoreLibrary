/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        CifColorItem.cs
 * PURPOSE:     Helper Object needed for Databinding in WPF.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

using SystemDrawingColor = System.Drawing.Color;
using MediaColor = System.Windows.Media.Color;

namespace Common.Images
{
    /// <summary>
    /// Wrapper for presenting CIF dictionary color keys inside WPF controls.
    /// </summary>
    public class CifColorItem
    {
        /// <summary>
        /// Gets the source System.Drawing.Color.
        /// </summary>
        public SystemDrawingColor SourceColor { get; }

        /// <summary>
        /// Gets the display Media.Color for XAML binding.
        /// </summary>
        public MediaColor DisplayColor { get; }

        /// <summary>
        /// Gets the hex value representation.
        /// </summary>
        public string HexValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CifColorItem"/> class.
        /// </summary>
        /// <param name="color">The source color.</param>
        public CifColorItem(SystemDrawingColor color)
        {
            SourceColor = color;
            DisplayColor = MediaColor.FromArgb(color.A, color.R, color.G, color.B);
            HexValue = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}