/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonFilter
 * FILE:        CommonFilter/FilterEnums.cs
 * PURPOSE:     Enums for all operators
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace CommonFilter
{
    /// <summary>
    ///     Enum for logic Operators
    /// </summary>
    public enum LogicOperator
    {
        and = 0,
        or = 1
    }

    /// <summary>
    ///     Enum for compare Operators
    /// </summary>
    public enum CompareOperator
    {
        like = 0,
        Notlike = 1
    }
}
