/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     ImageCompare
 * FILE:        ImageCompare/ImageData.cs
 * PURPOSE:     Simple Information Container for the Images
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace ImageCompare
{
    /// <summary>
    ///     Container that holds all image Information
    /// </summary>
    public sealed class ImageData
    {
        /// <summary>
        ///     Gets or sets the image path.
        /// </summary>
        /// <value>
        ///     The image path.
        /// </value>
        public string ImagePath { get; init; }

        /// <summary>
        ///     Gets or sets the name of the image.
        /// </summary>
        /// <value>
        ///     The name of the image.
        /// </value>
        public string ImageName { get; init; }

        /// <summary>
        ///     Gets or sets the r.
        /// </summary>
        /// <value>
        ///     The r.
        /// </value>
        public int R { get; init; }

        /// <summary>
        ///     Gets or sets the g.
        /// </summary>
        /// <value>
        ///     The g.
        /// </value>
        public int G { get; init; }

        /// <summary>
        ///     Gets or sets the b.
        /// </summary>
        /// <value>
        ///     The b.
        /// </value>
        public int B { get; init; }

        /// <summary>
        ///     Gets or sets the width.
        /// </summary>
        /// <value>
        ///     The width.
        /// </value>
        public int Width { get; init; }

        /// <summary>
        ///     Gets or sets the height.
        /// </summary>
        /// <value>
        ///     The height.
        /// </value>
        public int Height { get; init; }

        /// <summary>
        ///     Gets or sets the size.
        /// </summary>
        /// <value>
        ///     The size.
        /// </value>
        public long Size { get; init; }

        /// <summary>
        ///     Gets or sets the similarity.
        /// </summary>
        /// <value>
        ///     The similarity.
        /// </value>
        public double Similarity { get; internal set; }

        /// <summary>
        ///     Gets or sets the extension.
        /// </summary>
        /// <value>
        ///     The extension.
        /// </value>
        public string Extension { get; init; }

        /// <summary>
        ///     Gets the details.
        /// </summary>
        /// <returns>Information about the Image</returns>
        public string GetDetails()
        {
            return string.Concat(ImageResources.ImagePath, ImagePath, ImageResources.ImageName,
                ImageName, ImageResources.ImageHeight, Height, ImageResources.ImageWidth, Width,
                ImageResources.ImageSize, Height * Width);
        }

        /// <summary>
        ///     Gets the details.
        /// </summary>
        /// <returns>Information about the Image, without Path Information</returns>
        public string GetDetailsSimple()
        {
            return string.Concat(ImageResources.ImageHeight, Height, ImageResources.ImageWidth, Width,
                ImageResources.ImageSize, Height * Width);
        }
    }
}
