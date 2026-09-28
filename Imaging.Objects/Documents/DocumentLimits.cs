/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Objects.Documents;
 * FILE:        DocumentLimits.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Objects.Documents;

internal static class DocumentLimits
{
    /// <summary>
    ///     UnmanagedImageBuffer computes its size in 32-bit ints, so a pixel buffer must stay below 2 GiB.
    /// </summary>
    internal static void EnsureValidSize(int width, int height)
    {
        if (width <= 0 || height <= 0 || (long)width * height * UnmanagedImageBuffer.BytesPerPixel > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(width),
                string.Format(ImageResource.ErrorInvalidDocumentSize, width, height));
        }
    }
}