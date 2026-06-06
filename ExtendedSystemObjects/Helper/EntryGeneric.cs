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
    ///     Generic entry structure for a key-value pair.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public struct EntryGeneric<TValue> where TValue : unmanaged
    {
        /// <summary>
        /// The used
        /// checked first in every probe
        /// 3 bytes padding (compiler), then:
        /// </summary>
        public byte used;

        /// <summary>
        /// The key
        /// checked second
        /// </summary>
        public int key;

        /// <summary>
        /// The value only read on hit
        /// </summary>
        public TValue value;
    }
}
