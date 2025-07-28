/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Pathfinder
 * FILE:        Pathfinder/IPathfinding.cs
 * PURPOSE:     Interface for the Pathfinder
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMemberInSuper.Global

namespace Pathfinder;

/// <summary>
///     Defines the contract for pathfinder algorithms.
/// </summary>
public interface IPathfinding
{
    /// <summary>
    ///     Finds the path from the start coordinates to the goal coordinates.
    /// </summary>
    /// <param name="startX">The start x-coordinate.</param>
    /// <param name="startY">The start y-coordinate.</param>
    /// <param name="goalX">The goal x-coordinate.</param>
    /// <param name="goalY">The goal y-coordinate.</param>
    /// <param name="rangeLimit">The maximum range limit for the path calculation.</param>
    /// <returns>A <see cref="PathfinderResults" /> object containing the path and other calculations.</returns>
    PathfinderResults FindPath(int startX, int startY, int goalX, int goalY, int rangeLimit);
}
