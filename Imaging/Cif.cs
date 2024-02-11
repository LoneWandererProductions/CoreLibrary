/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/Cif.cs
 * PURPOSE:     Custom Image Format object, that contains all attributes and basic information
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using Mathematics;

namespace Imaging
{
    /// <summary>
    ///     Image in Cif format, with various further Tools
    /// </summary>
    public sealed class Cif
    {
        /// <summary>
        ///     The cif image
        /// </summary>
        public Dictionary<Color, List<int>> cifImage = new();

        /// <summary>
        ///     Gets a value indicating whether this <see cref="Cif" /> is compressed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if compressed; otherwise, <c>false</c>.
        /// </value>
        public bool Compressed { get; init; }

        /// <summary>
        ///     Gets the height.
        /// </summary>
        /// <value>
        ///     The height.
        /// </value>
        public int Height { get; init; }

        /// <summary>
        ///     Gets the width.
        /// </summary>
        /// <value>
        ///     The width.
        /// </value>
        public int Width { get; init; }

        /// <summary>
        ///     Gets the check sum.
        /// </summary>
        /// <value>
        ///     The check sum.
        /// </value>
        public int CheckSum => Height * Width;

        /// <summary>
        ///     Gets the number of colors.
        /// </summary>
        /// <value>
        ///     The number of colors.
        /// </value>
        public int NumberOfColors { get; init; }

        /// <summary>
        ///     Changes the color.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="color">The color.</param>
        /// <returns>Success Status</returns>
        public bool ChangeColor(int x, int y, Color color)
        {
            var coordinate = new Coordinate2D(x, y, Width);
            var id = coordinate.Id;

            if (id > CheckSum)
            {
                return false;
            }

            foreach (var (key, value) in cifImage)
            {
                if (!value.Contains(id))
                {
                    continue;
                }

                if (key == color)
                {
                    return false;
                }

                cifImage[key].Remove(id);

                if (cifImage.ContainsKey(color))
                {
                    cifImage[color].Add(id);
                }
                else
                {
                    var cache = new List<int> { id };
                    cifImage.Add(color, cache);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Changes the color.
        /// </summary>
        /// <param name="oldColor">The old color.</param>
        /// <param name="newColor">The new color.</param>
        /// <returns>Success Status</returns>
        public bool ChangeColor(Color oldColor, Color newColor)
        {
            if (!cifImage.ContainsKey(oldColor))
            {
                return false;
            }

            var cache = cifImage[oldColor];
            cifImage.Remove(oldColor);

            if (cifImage.ContainsKey(newColor))
            {
                cifImage[newColor].AddRange(cache);
            }
            else
            {
                cifImage.Add(newColor, cache);
            }

            return true;
        }

        /// <summary>
        ///     Gets the image.
        /// </summary>
        /// <returns>Cif Converted to Image</returns>
        [return: MaybeNull]
        public Image GetImage()
        {
            if (cifImage == null)
            {
                return null;
            }

            var image = new Bitmap(Height, Width);
            var dbm = DirectBitmap.GetInstance(image);

            foreach (var (key, value) in cifImage)
            foreach (var coordinate in value.Select(id => Coordinate2D.GetInstance(id, Width)))
            {
                dbm.SetPixel(coordinate.X, coordinate.Y, key);
            }

            return null;
        }

        /// <summary>
        ///     Converts to string.
        /// </summary>
        /// <returns>
        ///     A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var info = string.Empty;

            foreach (var (color, value) in cifImage)
            {
                info = string.Concat(info, ImagingResources.Color, color, ImagingResources.Spacing);

                for (var i = 0; i < value.Count - 1; i++)
                {
                    info = string.Concat(info, value[i], ImagingResources.Indexer);
                }

                info = string.Concat(info, value[value.Count], Environment.NewLine);
            }

            return info;
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Height, Width, NumberOfColors);
        }
    }
}
