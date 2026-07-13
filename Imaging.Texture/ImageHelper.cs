/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Texture
 * FILE:        ImageHelper.cs
 * PURPOSE:     Here I try to minimize the footprint of my class and pool all shared methods
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MissingSpace

using System.Runtime.CompilerServices;

namespace Imaging.Texture
{
    /// <summary>
    /// Helper methods for image processing.
    /// </summary>
    internal static class ImageHelper
    {
        /// <summary>
        ///     Interpolates the specified a.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <param name="t">The t.</param>
        /// <returns>Interpolation</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double Interpolate(double a, double b, double t)
        {
            var ft = t * Math.PI;
            var f = (1 - Math.Cos(ft)) * 0.5;
            return a * (1 - f) + b * f;
        }
    }
}
