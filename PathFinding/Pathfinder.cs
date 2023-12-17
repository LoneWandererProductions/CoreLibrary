/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     PathFinding
 * FILE:        PathFinding/IPathfinder.cs
 * PURPOSE:     Path-finding class Interface for external Access
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System.Collections.Generic;

namespace PathFinding
{
    /// <inheritdoc />
    /// <summary>
    ///     The pathfinder class.
    /// </summary>
    public sealed class Pathfinder : IPathfinder
    {
        /// <summary>
        ///     A map consists of Nodes
        ///     A node Represents a Cell
        ///     NW,N,NE,
        ///     W,X,E
        ///     SW,S,SE
        ///     Initiate the Pathfinder
        /// </summary>
        /// <param name="borders">Map as Point divided in Cells</param>
        public Pathfinder(int[,] borders)
        {
            var length = (borders.GetUpperBound(0) + 1) / 3;
            var height = (borders.GetUpperBound(1) + 1) / 3;
            //start Data
            PathFinding.SetData(borders, height, length);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Return fastest way, returns a the Numbers of the Node
        /// </summary>
        /// <param name="start">Start Point</param>
        /// <param name="end">End Point</param>
        /// <param name="diagonal">Can we go diagonal?</param>
        /// <returns>Returns a the Numbers of the Node as a List</returns>
        public List<int> GetPath(int start, int end, bool diagonal)
        {
            return PathFinding.GetPath(start, end, diagonal);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Checks if a node is blocked
        /// </summary>
        /// <param name="targetCoordinateId">Target Node Id</param>
        /// <returns>Blocked or not</returns>
        public bool IsTileBlocked(int targetCoordinateId)
        {
            return PathFinding.IsTileBlocked(targetCoordinateId);
        }
    }
}
