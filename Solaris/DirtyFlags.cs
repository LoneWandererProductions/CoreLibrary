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
        None = 0,
        TileMap = 1 << 0,   // Layer 1: Base terrain / static tiles
        Grid = 1 << 1,      // Layer 2: Polaris grid overlay
        Overlays = 1 << 2,  // Layer 3: Selection, glyphs, dynamic cursors
        FullRedraw = TileMap | Grid | Overlays
    }
}
