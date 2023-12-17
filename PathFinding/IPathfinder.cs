/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     PathFinding
 * FILE:        PathFinding/IPathfinder.cs
 * PURPOSE:     Path-finding class Interface for external Access
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

using System.Collections.Generic;

namespace PathFinding
{
    /// <summary>
    ///     The IPathfinder interface.
    /// </summary>
    internal interface IPathfinder
    {
        /// <summary>
        ///     Get the path.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="diagonal">The diagonal.</param>
        /// <returns>The Movement Path<see cref="T:List{int}" />.</returns>
        List<int> GetPath(int start, int end, bool diagonal);

        /// <summary>
        ///     The is tile blocked.
        /// </summary>
        /// <param name="targetCoordinateId">The targetCoordinateId.</param>
        /// <returns>The Block Status<see cref="bool" />.</returns>
        bool IsTileBlocked(int targetCoordinateId);
    }
}
