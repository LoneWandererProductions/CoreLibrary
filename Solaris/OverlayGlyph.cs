/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        OverlayGlyph.cs
 * PURPOSE:     Handle the overlay glyphs, which are drawn on top of the tile and can be used to display additional information like health, damage, or other status effects.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace Solaris
{
    namespace Solaris
    {
        public sealed class OverlayGlyph
        {
            /// <summary>
            /// Gets or sets the symbol.
            /// E.g., "⚔", "🪙", "💧", or "4"
            /// </summary>
            /// <value>
            /// The symbol.
            /// </value>
            public string Symbol { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the color.
            /// </summary>
            /// <value>
            /// The color.
            /// </value>
            public System.Drawing.Color Color { get; set; } = System.Drawing.Color.Gold;

            /// <summary>
            /// Gets or sets the name of the font.
            /// Handles Unicode vectors out of the box
            /// </summary>
            /// <value>
            /// The name of the font.
            /// </value>
            public string FontName { get; set; } = "Segoe UI Symbol";

            /// <summary>
            /// Gets or sets the size of the font.
            /// Scaled proportionally to match your TileSize
            /// </summary>
            /// <value>
            /// The size of the font.
            /// </value>
            public float FontSize { get; set; } = 14f;

            /// <summary>
            /// Gets or sets a value indicating whether this instance is bold.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance is bold; otherwise, <c>false</c>.
            /// </value>
            public bool IsBold { get; set; } = true;
        }
    }
}
