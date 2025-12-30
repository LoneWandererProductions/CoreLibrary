/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls.Images
 * FILE:        PickResults.cs
 * PURPOSE:     Results of the Color Wheel Click
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */


/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls.Images
 * FILE:        PickResults.cs
 * PURPOSE:     Results of the Color Wheel Click
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace CommonControls.Images
{
    /// <summary>
    ///     The Result for the Click in the Color Wheel
    /// </summary>
    internal readonly struct PickResults
    {
        /// <summary>
        ///     Gets or sets the area.
        /// </summary>
        /// <value>
        ///     The area.
        /// </value>
        internal ColorPick.Area Area { get; init; }

        /// <summary>
        ///     Gets or sets the hue.
        /// </summary>
        /// <value>
        ///     The hue.
        /// </value>
        internal double Hue { get; init; }

        /// <summary>
        ///     Gets or sets the sat.
        /// </summary>
        /// <value>
        ///     The sat.
        /// </value>
        internal double Sat { get; init; }

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        /// <value>
        ///     The value.
        /// </value>
        internal double Val { get; init; }
    }
}
