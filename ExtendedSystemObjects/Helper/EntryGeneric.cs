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
    [StructLayout(LayoutKind.Sequential)]
    public struct EntryGeneric<TValue> where TValue : unmanaged
    {
        public int Key;
        public TValue Value;
        public byte Used;
    }
}
