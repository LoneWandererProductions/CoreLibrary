/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonFilter
 * FILE:        CommonFilter/IFilter.cs
 * PURPOSE:     Interface for Filter
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;

namespace CommonFilter
{
    /// <summary>
    ///     Interface Filter
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        ///     Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        ///     Checks the filter.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        bool CheckFilter(string input);

        /// <summary>
        ///     Occurs when [filter changed].
        /// </summary>
        event EventHandler FilterChanged;
    }
}
