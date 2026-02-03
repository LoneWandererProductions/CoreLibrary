/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Weaver.Messages
 * FILE:        EnumTypes.cs
 * PURPOSE:     Enum of supported data types.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Weaver.Messages
{
    /// <summary>
    /// Type of Datatypes we support.
    /// </summary>
    public enum EnumTypes
    {
        /// <summary>
        /// The string type.
        /// </summary>
        Wstring = 0,

        /// <summary>
        /// The int type.
        /// </summary>
        Wint = 1,

        /// <summary>
        /// The wdouble
        /// </summary>
        Wdouble = 2,

        /// <summary>
        /// The bool type.
        /// </summary>
        Wbool = 3,

        // Compound types
        Wlist = 10, // a list of VmValues
        Wobject = 11, // a dictionary of string -> VmValue
        Wpointer = 12, // reference to another key
    }
}