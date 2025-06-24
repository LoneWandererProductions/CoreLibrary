using System.Runtime.InteropServices;

namespace ExtendedSystemObjects.Helper
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Entry
    {
        public int Key;
        public int Value;
        public byte Used;
    }
}
