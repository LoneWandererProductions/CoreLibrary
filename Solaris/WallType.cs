/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        WallType.cs
 * PURPOSE:     Defines the different types of walls for the isometric map.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Solaris
{
    /// <summary>
    /// Different types of walls for the isometric map.
    /// </summary>
    public enum WallType
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,

        /// <summary>
        /// The intact, Full height (for background NW / NE walls)
        /// </summary>
        Intact = 1,

        /// <summary>
        /// The cutaway, 35% height (for foreground SW / SE walls to look inside)
        /// </summary>
        Cutaway = 2,

        /// <summary>
        /// The ruined, Jagged, broken silhouette with crumbled ridge
        /// </summary>
        Ruined = 3,

        /// <summary>
        /// The rubble, Floor-level debris pile
        /// </summary>
        Rubble = 4
    }
}
