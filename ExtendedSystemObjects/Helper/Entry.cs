namespace ExtendedSystemObjects
{
    using System.Runtime.InteropServices;

    public unsafe partial struct UnmanagedIntMap
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Entry
        {
            public int Key;
            public int Value;
            public byte Used;
        }
    }
}
