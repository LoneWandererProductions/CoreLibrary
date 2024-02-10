/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     PathFinding
 * FILE:        PathFinding/MovePoint.cs
 * PURPOSE:     Path-finding class with the A* Algorithm, Custom Coordinate Object
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Source:      https://www.codeproject.com/Articles/1221034/PathFinding-Algorithms-in-Csharp
 */

// ReSharper disable NonReadonlyMemberInGetHashCode

using System;

namespace PathFinding
{
    /// <summary>
    ///     The move point class.
    /// </summary>
    internal sealed class MovePoint
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MovePoint" /> class.
        /// </summary>
        internal MovePoint()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MovePoint" /> class.
        /// </summary>
        /// <param name="xRow">Row as X</param>
        /// <param name="yColumn"> Column as Y</param>
        internal MovePoint(int xRow, int yColumn)
        {
            XRow = xRow;
            YColumn = yColumn;
        }

        /// <summary>
        ///     Row as X
        /// </summary>
        internal int XRow { get; set; }

        /// <summary>
        ///     Column as Y
        /// </summary>
        internal int YColumn { get; set; }

        /// <summary>
        ///     Compares If a Coordinate is equal to other Coordinate
        /// </summary>
        /// <param name="other">other Coordinate</param>
        /// <returns>If MovePoint is equal</returns>
        internal bool Equals(MovePoint other)
        {
            return XRow == other?.XRow && YColumn == other.YColumn;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Concat(XRow, PathResources.Separator, YColumn);
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(XRow, YColumn);
        }

        /// <summary>
        ///     Provides the equal Command
        /// </summary>
        /// <param name="obj">Other Object</param>
        /// <returns>If Object is equal</returns>
        public override bool Equals(object obj)
        {
            return obj is MovePoint other && Equals(other);
        }
    }
}
