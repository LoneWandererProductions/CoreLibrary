/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/ImageRegister.cs
 * PURPOSE:     Register for Image Operations, and some helpful extensions
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://docs.rainmeter.net/tips/colormatrix-guide/
 *              https://archive.ph/hzR2W
 *              https://www.codeproject.com/Articles/3772/ColorMatrix-Basics-Simple-Image-Color-Adjustment
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedMember.Global

using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace Imaging
{
    /// <summary>
    ///     The image register class.
    /// </summary>
    public static class ImageRegister
    {
        /// <summary>
        /// The sharpen filter
        /// </summary>
        internal static readonly double[,] sharpenFilter = new double[,]
        {
            { 0, -1,  0 }, { -1,  5, -1 }, { 0, -1,  0 }
        };

        /// <summary>
        /// The gaussian blur
        /// </summary>
        internal static readonly double[,] gaussianBlur = new double[,]
        {
            { 1, 2, 1 }, { 2, 4, 2 }, { 1, 2, 1 }
        };

        /// <summary>
        /// The emboss filter
        /// </summary>
        internal static readonly double[,] embossFilter = new double[,]
        {
            { -2, -1,  0 },
            { -1,  1,  1 },
            {  0,  1,  2 }
        };

        /// <summary>
        /// The box blur
        /// </summary>
        internal static readonly double[,] boxBlur = new double[,]
        {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 }
        };

        /// <summary>
        /// The laplacian filter
        /// </summary>
        internal static readonly double[,] laplacianFilter = new double[,]
        {
            {  0, -1,  0 },
            { -1,  4, -1 },
            {  0, -1,  0 }
        };

        /// <summary>
        /// The edge enhance
        /// </summary>
        internal static readonly double[,] edgeEnhance = new double[,]
        {
            {  0,  0,  0 },
            { -1,  1,  0 },
            {  0,  0,  0 }
        };

        /// <summary>
        /// The motion blur
        /// </summary>
        internal static readonly double[,] motionBlur = new double[,]
        {
            { 1, 0, 0, 0, 0 },
            { 0, 1, 0, 0, 0 },
            { 0, 0, 1, 0, 0 },
            { 0, 0, 0, 1, 0 },
            { 0, 0, 0, 0, 1 }
        };

        /// <summary>
        /// The unsharp mask
        /// </summary>
        internal static readonly double[,] unsharpMask = new double[,]
        {
            { -1, -1, -1 },
            { -1,  9, -1 },
            { -1, -1, -1 }
        };


        /// <summary>
        ///     the color matrix needed to GrayScale an image
        ///     Source:
        ///     https://archive.ph/hzR2W
        /// </summary>
        internal static readonly ColorMatrix GrayScale = new(new[]
        {
            new[] { .3f, .3f, .3f, 0, 0 }, new[] { .59f, .59f, .59f, 0, 0 }, new[] { .11f, .11f, .11f, 0, 0 },
            new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 0, 0, 0, 1 }
        });

        /// <summary>
        ///     the color matrix needed to invert an image
        ///     Source:
        ///     https://archive.ph/hzR2W
        /// </summary>
        internal static readonly ColorMatrix Invert = new(new[]
        {
            new float[] { -1, 0, 0, 0, 0 }, new float[] { 0, -1, 0, 0, 0 }, new float[] { 0, 0, -1, 0, 0 },
            new float[] { 0, 0, 0, 1, 0 }, new float[] { 1, 1, 1, 0, 1 }
        });

        /// <summary>
        ///     the color matrix needed to Sepia an image
        ///     Source:
        ///     https://archive.ph/hzR2W
        /// </summary>
        internal static readonly ColorMatrix Sepia = new(new[]
        {
            new[] { .393f, .349f, .272f, 0, 0 }, new[] { .769f, .686f, .534f, 0, 0 },
            new[] { 0.189f, 0.168f, 0.131f, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 0, 0, 0, 1 }
        });

        /// <summary>
        ///     the color matrix needed to Color Swap an image to Polaroid
        ///     Source:
        ///     https://docs.rainmeter.net/tips/colormatrix-guide/
        /// </summary>
        internal static readonly ColorMatrix Polaroid = new(new[]
        {
            new[] { 1.438f, -0.062f, -0.062f, 0, 0 }, new[] { -0.122f, 1.378f, -0.122f, 0, 0 },
            new[] { 0.016f, -0.016f, 1.483f, 0, 0 }, new float[] { 0, 0, 0, 1, 0 },
            new[] { 0.03f, 0.05f, -0.02f, 0, 1 }
        });

        /// <summary>
        ///     the color matrix needed to Color Swap an image to BlackAndWhite
        ///     Source:
        ///     https://docs.rainmeter.net/tips/colormatrix-guide/
        /// </summary>
        internal static readonly ColorMatrix BlackAndWhite = new(new[]
        {
            new[] { 1.5f, 1.5f, 1.5f, 0, 0 }, new[] { 1.5f, 1.5f, 1.5f, 0, 0 }, new[] { 1.5f, 1.5f, 1.5f, 0, 0 },
            new float[] { 0, 0, 0, 1, 0 }, new float[] { -1, -1, -1, 0, 1 }
        });

        /// <summary>
        ///     Gets or sets the count of retries.
        /// </summary>
        /// <value>
        ///     The count.
        /// </value>
        internal static int Count { get; set; }
    }

    /// <summary>
    ///     Image Extension Methods
    /// </summary>
    public static class ImageExtension
    {
        /// <summary>
        ///     Extension Method
        ///     Converts Bitmap to BitmapImage.
        /// </summary>
        /// <param name="bmp">The BMP.</param>
        /// <returns>
        ///     A BitmapImage
        /// </returns>
        public static BitmapImage ToBitmapImage(this Bitmap bmp)
        {
            return ImageStream.BitmapToBitmapImage(bmp);
        }

        /// <summary>
        ///     Extension Method
        ///     Converts Bitmap to BitmapImage.
        /// </summary>
        /// <param name="bmp">The BMP.</param>
        /// <returns>
        ///     A BitmapImage
        /// </returns>
        public static Bitmap ToBitmap(this BitmapImage bmp)
        {
            return ImageStream.BitmapImageToBitmap(bmp);
        }

        /// <summary>
        ///     Extension Method
        ///     Converts Image to BitmapImage.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>
        ///     A BitmapImage
        /// </returns>
        public static BitmapImage ToBitmapImage(this Image image)
        {
            var bitmap = new Bitmap(image);
            return bitmap.ToBitmapImage();
        }
    }

    /// <summary>
    ///     Enum of possible Filter
    /// </summary>
    public enum ImageFilter
    {
        /// <summary>
        ///     No Filter
        /// </summary>
        None = 0,

        /// <summary>
        ///     The gray scale filter
        /// </summary>
        GrayScale = 1,

        /// <summary>
        ///     The invert filter
        /// </summary>
        Invert = 2,

        /// <summary>
        ///     The sepia filter
        /// </summary>
        Sepia = 3,

        /// <summary>
        ///     The black and white filter
        /// </summary>
        BlackAndWhite = 4,

        /// <summary>
        ///     The polaroid filter
        /// </summary>
        Polaroid = 5,

        /// <summary>
        ///     The contour
        /// </summary>
        Contour = 6,

        // New convolution-based filters

        /// <summary>
        /// The sharpen
        /// </summary>
        Sharpen = 7,

        /// <summary>
        /// The gaussian blur
        /// </summary>
        GaussianBlur = 8,

        /// <summary>
        /// The emboss
        /// </summary>
        Emboss = 9,

        /// <summary>
        /// The box blur
        /// </summary>
        BoxBlur = 10,

        /// <summary>
        /// The laplacian
        /// </summary>
        Laplacian = 11,

        /// <summary>
        /// The edge enhance
        /// </summary>
        EdgeEnhance = 12,

        /// <summary>
        /// The motion blur
        /// </summary>
        MotionBlur = 13,

        /// <summary>
        /// The unsharp mask
        /// </summary>
        UnsharpMask = 14
    }
}
