/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Debugger
 * FILE:        Debugger/ErCode.cs
 * PURPOSE:     Error Codes
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace Debugger
{
    /// <summary>
    ///     Define all possible Error Types
    /// </summary>
    public enum ErCode
    {
        /// <summary>
        ///     Error Message = 0.
        /// </summary>
        Error = 0,

        /// <summary>
        ///     Warning Message= 1.
        /// </summary>
        Warning = 1,

        /// <summary>
        ///     Information Message = 2.
        /// </summary>
        Information = 2,

        /// <summary>
        ///     External Message = 3.
        /// </summary>
        External = 3,
        
        /// <summary>
        ///     Verbose Message = 4. 
        ///     Highly detailed debug information, used for troubleshooting when no debugger is available.
        /// </summary>
        Verbose = 4
    }
}
