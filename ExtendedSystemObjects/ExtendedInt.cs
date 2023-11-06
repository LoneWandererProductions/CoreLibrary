/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects
 * FILE:        ExtendedSystemObjects/ExtendedInt.cs
 * PURPOSE:     Some Extensions for int
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global

namespace ExtendedSystemObjects
{
    /// <summary>
    ///     Some extensions for int
    /// </summary>
    public static class ExtendedInt
    {
        /// <summary>
        ///     Intervals the specified value.
        /// </summary>
        /// <param name="i">The int we want to check.</param>
        /// <param name="value">The value we are comparing.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>If value is in the interval</returns>
        public static bool Interval(this int i, int value, int interval)
        {
            return i - interval <= value && value <= i + interval;
        }
    }
}
