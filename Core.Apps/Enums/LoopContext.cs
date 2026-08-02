/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Apps
 * FILE:        LoopContext.cs
 * PURPOSE:     That holds the types of loops.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Core.Apps.Enums
{
    /// <summary>
    /// The loop types.
    /// </summary>
    public enum LoopContext
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,

        /// <summary>
        /// The constant bounded
        /// </summary>
        ConstantBounded = 1,

        /// <summary>
        /// The variable bounded
        /// </summary>
        VariableBounded = 2,

        /// <summary>
        /// The nested
        /// </summary>
        Nested = 3
    }
}
