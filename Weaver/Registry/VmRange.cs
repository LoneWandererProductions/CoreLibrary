/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Registry
 * FILE:        VmRange.cs
 * PURPOSE:     Struct that contains the helpers for VM memory ranges, used in objects and arrays.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.Registry
{
    /// <summary>
    /// Struct that contains the helpers for VM memory ranges, used in objects and arrays.
    /// </summary>
    public readonly struct VmRange
    {
        /// <summary>
        /// Gets the start.
        /// </summary>
        /// <value>
        /// The start.
        /// </value>
        public int Start { get; }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get; }

        /// <summary>
        /// Gets the end.
        /// </summary>
        /// <value>
        /// The end.
        /// </value>
        public int End => Start + Length - 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="VmRange"/> struct.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="length">The length.</param>
        public VmRange(int start, int length)
        {
            Start = start;
            Length = length;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is scalar.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is scalar; otherwise, <c>false</c>.
        /// </value>
        public bool IsScalar => Length == 1;

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty => Length <= 0;
    }
}