/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        DirtyFlags.cs
 * PURPOSE:     Flags needed for redwaring operations, marks cells as dirty for redraw.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;

namespace Solaris
{
    /// <summary>
    /// Categorizes layer and region invalidations for targeted repainting.
    /// </summary>
    [Flags]
    public enum DirtyFlags
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,

        /// <summary>
        /// Layer 1: Base terrain / static tiles
        /// </summary>
        TileMap = 1 << 0,

        /// <summary>
        /// Layer 2: Polaris grid overlay
        /// </summary>
        Grid = 1 << 1,

        /// <summary>
        /// Layer 3: Selection, glyphs, dynamic cursors
        /// </summary>
        Overlays = 1 << 2,

        /// <summary>
        /// The viewport
        /// </summary>
        Viewport = 1 << 3,

        /// <summary>
        /// The full redraw
        /// </summary>
        FullRedraw = TileMap | Grid | Overlays | Viewport
    }
}
