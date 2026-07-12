/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        CoordinatePacker.cs
 * PURPOSE:     A more clever way to handle some 3D coordinate Stuff
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace Mathematics
{
    using System.Runtime.CompilerServices;

    /// <summary>
    /// A coordinate packer for 3D coordinates, allowing packing and unpacking of x, y, z coordinates into a single ulong value.
    /// </summary>
    public static class CoordinatePacker
    {
        /// <summary>
        /// Packs the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        /// <returns>The packed 3D coordinate as a ulong.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Pack(uint x, uint y, uint z)
        {
            return (x & 0x1FFFFF) |
                   (((ulong)y & 0x1FFFFF) << 21) |
                   (((ulong)z & 0x1FFFFF) << 42);
        }

        /// <summary>
        /// Unpacks the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unpack(ulong key, out uint x, out uint y, out uint z)
        {
            x = (uint)(key & 0x1FFFFF);
            y = (uint)((key >> 21) & 0x1FFFFF);
            z = (uint)((key >> 42) & 0x1FFFFF);
        }
    }
}
