/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonFilter
 * FILE:        CommonFilter/FilterEnums.cs
 * PURPOSE:     Enums for all operators
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

namespace CommonFilter
{
    /// <summary>
    ///     Enum for logic Operators
    /// </summary>
    public enum LogicOperator
    {
        /// <summary>
        ///     The and
        /// </summary>
        And = 0,

        /// <summary>
        ///     The or
        /// </summary>
        Or = 1,

        /// <summary>
        ///     The and not
        /// </summary>
        AndNot = 2,

        /// <summary>
        ///     The or not
        /// </summary>
        OrNot = 3
    }

    /// <summary>
    ///     Enum for compare Operators
    /// </summary>
    public enum CompareOperator
    {
        /// <summary>
        ///     The like
        /// </summary>
        Like = 0,

        /// <summary>
        ///     The not like
        /// </summary>
        NotLike = 1,

        /// <summary>
        ///     The equal
        /// </summary>
        Equal = 2,

        /// <summary>
        ///     The not equal
        /// </summary>
        NotEqual = 3
    }
}
