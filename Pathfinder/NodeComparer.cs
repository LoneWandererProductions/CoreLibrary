/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Pathfinder
 * FILE:        NodeComparer.cs
 * PURPOSE:     Nodes Comparer for the open list
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;

namespace Pathfinder
{
    /// <inheritdoc />
    /// <summary>
    ///     Compares two nodes for ordering in the open list.
    /// </summary>
    /// <seealso cref="T:System.Collections.Generic.IComparer`1" />
    public sealed class NodeComparer : IComparer<Node>
    {
        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.
        /// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description><paramref name="x" /> is less than <paramref name="y" />.</description></item><item><term> Zero</term><description><paramref name="x" /> equals <paramref name="y" />.</description></item><item><term> Greater than zero</term><description><paramref name="x" /> is greater than <paramref name="y" />.</description></item></list>
        /// </returns>
        public int Compare(Node? x, Node? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            // 1. Total Cost is always the king of A*
            var result = x.F.CompareTo(y.F);

            if (result == 0)
            {
                // 2. Revert to your logic: Compare G. 
                // This ensures we respect the diagonal and movement costs 
                // when the heuristic guess is identical.
                result = x.G.CompareTo(y.G);

                if (result == 0)
                {
                    // 3. Final unique identifier for the SortedSet
                    result = x.X.CompareTo(y.X);
                    if (result == 0)
                    {
                        result = x.Y.CompareTo(y.Y);
                    }
                }
            }

            return result;
        }
    }
}
