/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        FogState.cs
 * PURPOSE:     Flags that defines the visibility of the tile..
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Solaris
{
    /// <summary>
    /// Enum that declares the State of the Tile.
    /// </summary>
    public enum FogState : byte
    {
        /// <summary>
        /// Completely undiscovered (Terra Incognita: black, nothing is drawn).
        /// </summary>
        Unexplored = 0,

        /// <summary>
        /// Explored fog of war: Terrain is darkened; enemy units are invisible.
        /// </summary>
        Explored = 1,

        /// <summary>
        /// Active field of view: Everything is fully visible.
        /// </summary>
        Visible = 2
    }
}
