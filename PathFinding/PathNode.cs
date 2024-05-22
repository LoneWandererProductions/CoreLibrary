/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     PathFinding
 * FILE:        PathFinding/PathNode.cs
 * PURPOSE:     Helper Object for the Path-finding Algorithm A*
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;

namespace PathFinding
{
    /// <inheritdoc />
    /// <summary>
    ///     The Path finding node class.
    /// </summary>
    public sealed class PathNode : IComparable<PathNode>
    {
        /// <summary>
        ///     The amount needed to move from the starting node to the given other
        /// </summary>
        internal int Cost { get; set; }

        /// <summary>
        ///     The estimated cost to move from that given node to the end point - Called Heuristic
        /// </summary>
        internal int Heuristic { get; set; }

        /// <summary>
        ///     Is it a wall yes or no
        /// </summary>
        internal bool IsWall { get; init; }

        /// <summary>
        ///     Parent Node
        /// </summary>
        internal PathNode Parent { get; set; }

        /// <summary>
        ///     Position on the X axis
        /// </summary>
        internal int XNodeRow { get; init; }

        /// <summary>
        ///     Position on the Y axis
        /// </summary>
        internal int YNodeColumn { get; init; }

        /// <summary>
        ///     Cost + Heuristic, expected Cost
        /// </summary>
        private int F => Cost + Heuristic;
        public int TotalCost => Cost + Heuristic;

        public int StepCost { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Implementation of  IComparable (which requires a method called "CompareTo" to work)
        ///     Inherits from the interface IComparable, allows the nodes to be sorted using .sort()
        ///     specifies how to sort the nodes
        /// </summary>
        /// <param name="other">PathNode </param>
        /// <returns> Sorted Nodes</returns>
        public int CompareTo(PathNode other)
        {
            return TotalCost.CompareTo(other.TotalCost);
        }
    }
}
