/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        DocumentLimits.cs
 * PURPOSE:     Check if size is valid for our Image7Document Format.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents
{
    /// <summary>
    /// The Limits of our Image Format.
    /// </summary>
    internal static class DocumentLimits
    {
        /// <summary>
        /// UnmanagedImageBuffer computes its size in 32-bit ints, so a pixel buffer must stay below 2 GiB.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">width</exception>
        internal static void EnsureValidSize(int width, int height)
        {
            if (width <= 0 || height <= 0 || (long)width * height * UnmanagedImageBuffer.BytesPerPixel > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(width),
                    string.Format(ImageResource.ErrorInvalidDocumentSize, width, height));
            }
        }
    }
}
