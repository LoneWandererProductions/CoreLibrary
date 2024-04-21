/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/ICustomImageFormat.cs
 * PURPOSE:     Interface of our custom Image Format
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

using System.Drawing;

namespace Imaging
{
    /// <summary>
    ///     Interface for Custom Image Format
    /// </summary>
    public interface ICustomImageFormat
    {
        /// <summary>
        ///     Load a cif file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Image as pure Cif format</returns>
        Cif LoadCif(string path);

        /// <summary>
        ///     Generates Image from Cif.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A bitmap Image</returns>
        Bitmap GetImageFromCif(string path);

        /// <summary>
        ///     Generate Cif from Bitmap
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>Custom Image Format of the Image</returns>
        Cif GenerateCifFromBitmap(Bitmap image);

        /// <summary>
        ///     Saves Bitmap to cif file.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="path">The path.</param>
        void SaveBitmapToCifFile(Bitmap image, string path);

        /// <summary>
        ///     Compressed cif file.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="path">The path.</param>
        void GenerateCompressedCifFromBitmap(Bitmap image, string path);
    }
}
