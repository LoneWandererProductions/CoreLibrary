/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Pixel32.cs
 * PURPOSE:     Custom Pixel Container for DirectBitmapImage and DirectBitmap to share the same underlying data structure.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Runtime.InteropServices;

namespace Imaging
{
    /// <summary>
    /// Shared data structure for DirectBitmapImage and DirectBitmap to share the same underlying data structure.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Pixel32
    {
        /// <summary>
        /// The Blue component of the pixel. 0 is no blue, 255 is full blue.
        /// </summary>
        public readonly byte B;

        /// <summary>
        /// The Green component of the pixel. 0 is no green, 255 is full green.
        /// </summary>
        public readonly byte G;

        /// <summary>
        /// The Red component of the pixel. 0 is no red, 255 is full red.
        /// </summary>
        public readonly byte R;

        /// <summary>
        /// Transparency component. 0 is fully transparent, 255 is fully opaque.
        /// </summary>
        public readonly byte A;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pixel32"/> struct.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="a">a.</param>
        public Pixel32(byte r, byte g, byte b, byte a = 255)
        {
            B = b;
            G = g;
            R = r;
            A = a;
        }

        /// <summary>
        /// Gets the packed.
        /// </summary>
        /// <value>
        /// The packed.
        /// </value>
        public readonly uint Packed
            => (uint)(A << 24 | R << 16 | G << 8 | B);

        /// <summary>
        /// Froms the packed.
        /// </summary>
        /// <param name="packed">The packed.</param>
        /// <returns></returns>
        public static Pixel32 FromPacked(uint packed)
            => new Pixel32(
                (byte)(packed >> 16),
                (byte)(packed >> 8),
                (byte)packed,
                (byte)(packed >> 24)
            );

        /// <summary>
        /// Ares the equal.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns></returns>
        public static bool AreEqual(Pixel32 a, Pixel32 b)
            => a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(Pixel32 other) => AreEqual(this, other);
    }
}
