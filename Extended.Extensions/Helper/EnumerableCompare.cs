/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Extended.Extensions.Helper
 * FILE:        Helper/EnumerableCompare.cs
 * PURPOSE:     Compare operator, for now mostly Enumerable
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Extended.Extensions.Helper
{
    /// <summary>
    ///     Compare conditions for lists
    /// </summary>
    public enum EnumerableCompare
    {
        /// <summary>
        ///     Ignore order and count
        /// </summary>
        IgnoreOrderCount = 0,

        /// <summary>
        ///     Ignore order, but not count
        /// </summary>
        IgnoreOrder = 1,

        /// <summary>
        ///     List must be identical in order and count
        /// </summary>
        AllEqual = 2
    }
}
