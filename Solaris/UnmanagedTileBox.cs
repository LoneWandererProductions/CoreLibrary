using RenderEngine;

namespace Solaris
{
    /// <summary>
    /// Lightweight container for unmanaged tile sorting.
    /// </summary>
    public readonly struct UnmanagedTileBox
    {
        public int X { get; init; }
        public int Y { get; init; }
        public int Layer { get; init; }
        public UnmanagedImageBuffer Buffer { get; init; }
    }
}
