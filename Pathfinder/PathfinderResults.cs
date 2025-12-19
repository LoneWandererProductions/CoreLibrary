/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Pathfinder
 * FILE:        PathfinderResults.cs
 * PURPOSE:     Abstraction of the Pathfinder result set. We can calculate more stuff and provide more informations without changing the core algorithm.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

namespace Pathfinder;

/// <summary>
///     Represents the result of a pathfinder operation.
/// </summary>
public sealed class PathfinderResults
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PathfinderResults" /> class.
    /// </summary>
    public PathfinderResults()
    {
        LimitedRangePath = new List<Node>();
    }

    /// <summary>
    ///     Gets or sets the full path from the start to the goal.
    /// </summary>
    public List<Node> FullPath { get; set; }

    /// <summary>
    ///     Gets or sets the path considering the range limit.
    /// </summary>
    public List<Node> LimitedRangePath { get; set; }
}
