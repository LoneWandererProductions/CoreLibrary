/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SqliteHelper
 * FILE:        SqliteHelper/CompareOperator.cs
 * PURPOSE:     Compare Operators
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace SqliteHelper
{
    /// <summary>
    ///     Compare Operators
    /// </summary>
    public enum CompareOperator
    {
        /// <summary>
        ///     None = 0.
        /// </summary>
        None = 0,

        /// <summary>
        ///     The Equal Operator = 1.
        /// </summary>
        Equal = 1,

        /// <summary>
        ///     The Not equal Operator= 2.
        /// </summary>
        NotEqual = 2,

        /// <summary>
        ///     The Like Operator = 3.
        /// </summary>
        Like = 3,

        /// <summary>
        ///     The Not like Operator = 4.
        /// </summary>
        NotLike = 4
    }
}
