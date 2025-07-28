/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ExtendedSystemObjects.Helper
 * FILE:        EntryGeneric.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Runtime.InteropServices;

namespace ExtendedSystemObjects.Helper
{
    /// <summary>
    /// Generic entry structure for a key-value pair.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public struct EntryGeneric<TValue> where TValue : unmanaged
    {
        /// <summary>
        /// The key
        /// </summary>
        public int Key;

        /// <summary>
        /// The value
        /// </summary>
        public TValue Value;

        /// <summary>
        /// The used
        /// </summary>
        public byte Used;
    }
}
