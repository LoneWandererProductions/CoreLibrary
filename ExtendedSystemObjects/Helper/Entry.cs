using System.Runtime.InteropServices;

namespace ExtendedSystemObjects.Helper
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Entry
    {
        public int Key;
        public int Value;
        public byte Used;
    }
}
