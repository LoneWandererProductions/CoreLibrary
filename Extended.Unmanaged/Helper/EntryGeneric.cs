/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Extended.Unmanaged.Helper
 * FILE:        EntryGeneric.cs
 * PURPOSE:     Helper Struct for UnmanagedMap.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System.Runtime.InteropServices;

namespace Extended.Unmanaged.Helper
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
