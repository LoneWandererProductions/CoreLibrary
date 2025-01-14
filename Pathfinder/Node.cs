/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Pathfinder
 * FILE:        Pathfinder/Node.cs
 * PURPOSE:     Node Object for Pathfinder Algorithm
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;

namespace Pathfinder
{
    /// <inheritdoc />
    /// <summary>
    ///     Represents a node in the A* pathfinder algorithm.
    /// </summary>
    /// <seealso cref="T:System.IComparable`1" />
    public sealed class Node : IComparable<Node>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Node" /> class.
        /// </summary>
        /// <param name="x">The X coordinate of the node.</param>
        /// <param name="y">The Y coordinate of the node.</param>
        /// <param name="g">The G cost, which is the cost from the start node to this node.</param>
        /// <param name="h">The H cost, which is the heuristic estimated cost from this node to the goal.</param>
        /// <param name="movementCost">The expected cost.</param>
        /// <param name="parent">The parent node, which is used to reconstruct the path once the goal is reached.</param>
        internal Node(int x, int y, int g, int h, int movementCost, Node parent = null)
        {
            X = x;
            Y = y;
            G = g;
            H = h;
            MovementCost = movementCost;
            Parent = parent;
        }

        /// <summary>
        ///     Gets the X coordinate of the node.
        /// </summary>
        /// <value>
        ///     The X coordinate.
        /// </value>
        public int X { get; }

        /// <summary>
        ///     Gets the Y coordinate of the node.
        /// </summary>
        /// <value>
        ///     The Y coordinate.
        /// </value>
        public int Y { get; }

        /// <summary>
        ///     Gets or sets the G cost, which is the cost from the start node to this node.
        /// </summary>
        /// <value>
        ///     The G cost.
        /// </value>
        internal int G { get; set; }

        /// <summary>
        ///     Gets or sets the H cost, which is the heuristic estimated cost from this node to the goal.
        /// </summary>
        /// <value>
        ///     The H cost.
        /// </value>
        internal int H { get; set; }

        /// <summary>
        ///     Gets or sets the parent node, which is used to reconstruct the path once the goal is reached.
        /// </summary>
        /// <value>
        ///     The parent node.
        /// </value>
        internal Node Parent { get; set; }

        /// <summary>
        ///     Gets the movement cost.
        /// </summary>
        /// <value>
        ///     The movement cost.
        /// </value>
        private int MovementCost { get; }

        /// <summary>
        ///     Gets the F cost, which is the sum of G and H. This is used to determine the priority of the node in the open list.
        /// </summary>
        /// <value>
        ///     The F cost.
        /// </value>
        private int F => G + H;

        /// <inheritdoc />
        /// <summary>
        ///     Compares the current instance with another <see cref="T:Pathfinder.Node" /> object and returns an integer that
        ///     indicates whether
        ///     the current instance
        ///     precedes, follows, or occurs in the same position in the sort order as the other object based on the F cost.
        /// </summary>
        /// <param name="other">The node to compare with this instance.</param>
        /// <returns>
        ///     A value that indicates the relative order of the nodes being compared. The return value has these meanings:
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Value</term><description>Meaning</description>
        ///         </listheader>
        ///         <item>
        ///             <term>Less than zero</term>
        ///             <description>This instance precedes <paramref name="other" /> in the sort order.</description>
        ///         </item>
        ///         <item>
        ///             <term>Zero</term>
        ///             <description>This instance occurs in the same position in the sort order as <paramref name="other" />.</description>
        ///         </item>
        ///         <item>
        ///             <term>Greater than zero</term>
        ///             <description>This instance follows <paramref name="other" /> in the sort order.</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public int CompareTo(Node other)
        {
            return F.CompareTo(other.F);
        }
    }
}
