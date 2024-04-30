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
        And = 0,
        Or = 1
    }

    /// <summary>
    ///     Enum for compare Operators
    /// </summary>
    public enum CompareOperator
    {
        Like = 0,
        Notlike = 1
    }
}
