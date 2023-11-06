/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/ColorPickerRegister.cs
 * PURPOSE:     Central Register
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using Imaging;

namespace CommonControls
{
    internal static class ColorPickerRegister
    {
        /// <summary>
        ///     The intern size
        /// </summary>
        internal const int InternSize = 189;

        /// <summary>
        ///     The size
        /// </summary>
        internal const int Size = 500;

        /// <summary>
        ///     Gets a value indicating whether [color changed].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [color changed]; otherwise, <c>false</c>.
        /// </value>
        internal static bool ColorChanged { get; set; }

        /// <summary>
        ///     Gets or sets the colors.
        /// </summary>
        /// <value>
        ///     The colors.
        /// </value>
        internal static ColorHsv Colors { get; set; }
    }
}
