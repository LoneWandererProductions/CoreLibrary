/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/ImageResources.cs
 * PURPOSE:     String Resources
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace ImageCompare
{
    /// <summary>
    ///     Basic Strings and Numbers
    /// </summary>
    internal static class ImageResources
    {
        /// <summary>
        ///     The Duplicate Size (const). Value: "16"
        /// </summary>
        internal const int DuplicateSize = 16;

        /// <summary>
        ///     The Similar Size (const). Value: "16"
        /// </summary>
        internal const int SimilarSize = 16;

        /// <summary>
        ///     The Color Threshold (const). Value: "3"
        /// </summary>
        internal const int ColorThreshold = 3;

        /// <summary>
        ///     The Max Pixel Count (const). Value: "256"
        /// </summary>
        internal const int MaxPixel = 256;

        /// <summary>
        ///     The Max Color Count (const). Value: "256"
        /// </summary>
        internal const int MaxColor = 256;

        /// <summary>
        ///     The Image Name (const). Value: "Name: "
        /// </summary>
        internal const string ImageName = ", Name: ";

        /// <summary>
        ///     The Image Size (const). Value: " , Size: "
        /// </summary>
        internal const string ImageSize = " , Size: ";

        /// <summary>
        ///     The Image Height (const). Value: "Bytes, Height: "
        /// </summary>
        internal const string ImageHeight = "Bytes, Height: ";

        /// <summary>
        ///     The Image Width (const). Value: " , Width: "
        /// </summary>
        internal const string ImageWidth = " , Width: ";

        /// <summary>
        ///     The Image Path (const). Value: " Path: "
        /// </summary>
        internal const string ImagePath = " Path: ";

        /// <summary>
        ///     The Error Image was empty (const). Value: "Error image was empty: "
        /// </summary>
        internal const string ErrorImageEmpty = "Error image was empty: ";

        /// <summary>
        ///     The Error File not found (const). Value: "Error file not found: "
        /// </summary>
        internal const string ErrorFileNotFound = "Error file not found: ";
    }
}
