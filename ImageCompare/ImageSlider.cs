/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/ImageSlider.cs
 * PURPOSE:     Checks if an Image is contained in another image. Stuff like a cut out.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Drawing;
using Imaging;
using Mathematics;

namespace ImageCompare;

/// <summary>
///     Image Slider to check if Image is part of a smaller one
/// </summary>
internal static class ImageSlider
{
    /// <summary>
    ///     Determines whether [is part of] [the specified big image path].
    /// </summary>
    /// <param name="bigImagePath">The big image path.</param>
    /// <param name="smallImagePath">The small image path.</param>
    /// <param name="startCoordinates">The start coordinates.</param>
    /// <param name="threshold">The color difference threshold.</param>
    /// <returns>
    ///     <c>true</c> if [is part of] [the specified big image path]; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsPartOf(string bigImagePath, string smallImagePath, out Coordinate2D startCoordinates,
        int threshold = 0)
    {
        using var bigImage = new Bitmap(bigImagePath);
        using var smallImage = new Bitmap(smallImagePath);
        return IsPartOf(bigImage, smallImage, out startCoordinates, threshold);
    }

    /// <summary>
    ///     Determines whether the small image is part of the big image.
    /// </summary>
    /// <param name="bigImage">The big image.</param>
    /// <param name="smallImage">The small image.</param>
    /// <param name="startCoordinates">The starting coordinates where the small image overlays the big image.</param>
    /// <param name="threshold">The color difference threshold.</param>
    /// <returns>
    ///     <c>true</c> if the small image is part of the big image; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsPartOf(Bitmap bigImage, Bitmap smallImage, out Coordinate2D startCoordinates,
        int threshold = 0)
    {
        var bigHeight = bigImage.Height;
        var bigWidth = bigImage.Width;
        var smallHeight = smallImage.Height;
        var smallWidth = smallImage.Width;

        using var dbmBig = new DirectBitmap(bigImage);
        using var dbmSmall = new DirectBitmap(smallImage);

        var smallImageBottomEdge = new DirectBitmap(smallWidth, 1);

        for (var i = 0; i <= bigHeight - smallHeight; i++)
            for (var j = 0; j <= bigWidth - smallWidth; j++)
            {
                // Update the bottom edge for the current position in the big image
                for (var x = 0; x < smallWidth; x++)
                {
                    smallImageBottomEdge.SetPixel(x, 0, dbmBig.GetPixel(j + x, i + smallHeight - 1));
                }

                if (CheckEdges(dbmBig, dbmSmall, i, j, smallImageBottomEdge, threshold) &&
                    CheckFull(dbmBig, dbmSmall, i, j, threshold))
                {
                    smallImageBottomEdge.Dispose();
                    startCoordinates = new Coordinate2D(j, i);
                    return true;
                }
            }

        smallImageBottomEdge.Dispose();
        startCoordinates = Coordinate2D.NullPoint;
        return false;
    }

    /// <summary>
    ///     Checks the edges of the small image within the big image.
    /// </summary>
    /// <param name="bigImage">The big image.</param>
    /// <param name="smallImage">The small image.</param>
    /// <param name="startRow">The start row.</param>
    /// <param name="startCol">The start col.</param>
    /// <param name="smallImageBottomEdge">The small image bottom edge.</param>
    /// <param name="threshold">The color difference threshold.</param>
    /// <returns>Basic check of the edges</returns>
    private static bool CheckEdges(DirectBitmap bigImage, DirectBitmap smallImage, int startRow, int startCol,
        DirectBitmap smallImageBottomEdge, int threshold)
    {
        var smallHeight = smallImage.Height;
        var smallWidth = smallImage.Width;

        // Check top edge
        for (var x = 0; x < smallWidth; x++)
        {
            if (!IsColorMatch(bigImage.GetPixel(startCol + x, startRow), smallImage.GetPixel(x, 0), threshold))
            {
                return false;
            }
        }

        // Check bottom edge
        for (var x = 0; x < smallWidth; x++)
        {
            if (!IsColorMatch(bigImage.GetPixel(startCol + x, startRow + smallHeight - 1),
                    smallImageBottomEdge.GetPixel(x, 0), threshold))
            {
                return false;
            }
        }

        // Check left edge
        for (var y = 0; y < smallHeight; y++)
        {
            if (!IsColorMatch(bigImage.GetPixel(startCol, startRow + y), smallImage.GetPixel(0, y), threshold))
            {
                return false;
            }
        }

        // Check right edge
        for (var y = 0; y < smallHeight; y++)
        {
            if (!IsColorMatch(bigImage.GetPixel(startCol + smallWidth - 1, startRow + y),
                    smallImage.GetPixel(smallWidth - 1, y), threshold))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Fully checks if the images match in the defined area.
    /// </summary>
    /// <param name="bigImage">The big image.</param>
    /// <param name="smallImage">The small image.</param>
    /// <param name="startRow">The start row.</param>
    /// <param name="startCol">The start col.</param>
    /// <param name="threshold">The color difference threshold.</param>
    /// <returns>
    ///     True if images are equal in the defined area, otherwise false.
    /// </returns>
    private static bool CheckFull(DirectBitmap bigImage, DirectBitmap smallImage, int startRow, int startCol,
        int threshold)
    {
        var smallHeight = smallImage.Height;
        var smallWidth = smallImage.Width;

        for (var y = 0; y < smallHeight; y++)
            for (var x = 0; x < smallWidth; x++)
            {
                if (!IsColorMatch(bigImage.GetPixel(startCol + x, startRow + y), smallImage.GetPixel(x, y),
                        threshold))
                {
                    return false;
                }
            }

        return true;
    }

    /// <summary>
    ///     Checks if two colors match within a given threshold.
    /// </summary>
    /// <param name="color1">The first color.</param>
    /// <param name="color2">The second color.</param>
    /// <param name="threshold">The color difference threshold.</param>
    /// <returns>
    ///     <c>true</c> if the colors match within the threshold; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsColorMatch(Color color1, Color color2, int threshold)
    {
        return Math.Abs(color1.R - color2.R) <= threshold &&
               Math.Abs(color1.G - color2.G) <= threshold &&
               Math.Abs(color1.B - color2.B) <= threshold;
    }
}
