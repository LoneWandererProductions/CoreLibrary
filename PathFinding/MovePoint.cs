/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     PathFinding
 * FILE:        PathFinding/MovePoint.cs
 * PURPOSE:     Path-finding class with the A* Algorithm, Custom Coordinate Object
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * Source:      https://www.codeproject.com/Articles/1221034/PathFinding-Algorithms-in-Csharp
 */

// ReSharper disable NonReadonlyMemberInGetHashCode

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
        /// <param name="xrow">Row as X</param>
        /// <param name="ycolumn"> Column as Y</param>
        internal MovePoint(int xrow, int ycolumn)
        {
            Xrow = xrow;
            Ycolumn = ycolumn;
        }

        /// <summary>
        ///     Row as X
        /// </summary>
        internal int Xrow { get; set; }

        /// <summary>
        ///     Column as Y
        /// </summary>
        internal int Ycolumn { get; set; }

        /// <summary>
        ///     Compares If a Coordinate is equal to other Coordinate
        /// </summary>
        /// <param name="other">other Coordinate</param>
        /// <returns>If MovePoint is equal</returns>
        internal bool Equals(MovePoint other)
        {
            return Xrow == other?.Xrow && Ycolumn == other.Ycolumn;
        }

        /// <summary>
        ///     Generate Hash Code just for the Three Attributes, we don't need More
        /// </summary>
        /// <returns>Hash Value</returns>
        public override int GetHashCode()
        {
            return Xrow ^ Ycolumn;
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
