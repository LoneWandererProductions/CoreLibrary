﻿using System.Drawing;

namespace Solaris
{
    /// <summary>
    /// Contains the Image Tile
    /// </summary>
    internal sealed class Box
    {
        internal Bitmap Image { get; init; }

        internal int X { get; init; }

        internal int Y { get; init; }

        internal int Layer { get; init; }
    }
}
