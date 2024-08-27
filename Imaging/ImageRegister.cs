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

using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;

namespace Imaging
{
    /// <summary>
    ///     The image register class.
    /// </summary>
    public static class ImageRegister
    {
        /// <summary>
        ///     The settings for our Filter
        /// </summary>
        private static readonly ConcurrentDictionary<ImageFilters, ImageFilterConfig> FilterSettings = new();

        /// <summary>
        ///     The texture setting
        /// </summary>
        private static readonly ConcurrentDictionary<TextureType, TextureConfig> TextureSetting = new();

        /// <summary>
        ///     The sharpen filter
        /// </summary>
        internal static readonly double[,] SharpenFilter = { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };

        /// <summary>
        ///     The gaussian blur
        /// </summary>
        internal static readonly double[,] GaussianBlur = { { 1, 2, 1 }, { 2, 4, 2 }, { 1, 2, 1 } };

        /// <summary>
        ///     The emboss filter
        /// </summary>
        internal static readonly double[,] EmbossFilter = { { -2, -1, 0 }, { -1, 1, 1 }, { 0, 1, 2 } };

        /// <summary>
        ///     The box blur
        /// </summary>
        internal static readonly double[,] BoxBlur = { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };

        /// <summary>
        ///     The laplacian filter
        /// </summary>
        internal static readonly double[,] LaplacianFilter = { { 0, -1, 0 }, { -1, 4, -1 }, { 0, -1, 0 } };

        /// <summary>
        ///     The edge enhance
        /// </summary>
        internal static readonly double[,] EdgeEnhance = { { 0, 0, 0 }, { -1, 1, 0 }, { 0, 0, 0 } };

        /// <summary>
        ///     The unsharp mask
        /// </summary>
        internal static readonly double[,] UnsharpMask = { { -1, -1, -1 }, { -1, 9, -1 }, { -1, -1, -1 } };

        /// <summary>
        ///     The motion blur
        /// </summary>
        internal static readonly double[,] MotionBlur =
        {
            { 1, 0, 0, 0, 0 }, { 0, 1, 0, 0, 0 }, { 0, 0, 1, 0, 0 }, { 0, 0, 0, 1, 0 }, { 0, 0, 0, 0, 1 }
        };

        /// <summary>
        ///     the color matrix needed to GrayScale an image
        ///     Source:
        ///     https://archive.ph/hzR2W
        ///     ColorMatrix:
        ///     | m11 m12 m13 m14 m15 |
        ///     | m21 m22 m23 m24 m25 |
        ///     | m31 m32 m33 m34 m35 |
        ///     | m41 m42 m43 m44 m45 |
        ///     | m51 m52 m53 m54 m55 |
        ///     translates to:
        ///     NewR = (m11 * R + m12 * G + m13 * B + m14 * A + m15)
        ///     NewG = (m21* R + m22* G + m23* B + m24* A + m25)
        ///     NewB = (m31* R + m32* G + m33* B + m34* A + m35)
        ///     NewA = (m41* R + m42* G + m43* B + m44* A + m45)
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
        ///     The brightness Filter
        ///     Adjusts the brightness of the image by scaling the color values.
        /// </summary>
        internal static readonly ColorMatrix Brightness = new(new[]
        {
            new[] { 1.2f, 0, 0, 0, 0 }, new[] { 0, 1.2f, 0, 0, 0 }, new[] { 0, 0, 1.2f, 0, 0 },
            new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 0, 0, 0, 1 }
        });

        /// <summary>
        ///     The contrast Filter
        ///     Adjusts the contrast of the image by scaling the differences between pixel values.
        /// </summary>
        internal static readonly ColorMatrix Contrast = new(new[]
        {
            new[] { 1.5f, 0, 0, 0, -0.2f }, new[] { 0, 1.5f, 0, 0, -0.2f }, new[] { 0, 0, 1.5f, 0, -0.2f },
            new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 0, 0, 0, 1 }
        });

        /// <summary>
        ///     The hue shift Filter
        ///     Shifts the hue of the image, effectively rotating the color wheel.
        /// </summary>
        internal static readonly ColorMatrix HueShift = new(new[]
        {
            new[] { 0.213f, 0.715f, 0.072f, 0, 0 }, new[] { 0.213f, 0.715f, 0.072f, 0, 0 },
            new[] { 0.213f, 0.715f, 0.072f, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 0, 0, 0, 1 }
        });

        /// <summary>
        ///     The color balance Filter
        ///     Adjusts the balance of colors to emphasize or de-emphasize specific color channels.
        /// </summary>
        internal static readonly ColorMatrix ColorBalance = new(new[]
        {
            new[] { 1f, 0.2f, -0.2f, 0, 0 }, new[] { -0.2f, 1f, 0.2f, 0, 0 }, new[] { 0.2f, -0.2f, 1f, 0, 0 },
            new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 0, 0, 0, 1 }
        });

        /// <summary>
        ///     The vintage Filter
        ///     Applies a vintage effect by modifying the color matrix to mimic old photo tones.
        /// </summary>
        internal static readonly ColorMatrix Vintage = new(new[]
        {
            new[] { 0.393f, 0.349f, 0.272f, 0, 0 }, new[] { 0.769f, 0.686f, 0.534f, 0, 0 },
            new[] { 0.189f, 0.168f, 0.131f, 0, 0 }, new float[] { 0, 0, 0, 1, 0 }, new float[] { 0, 0, 0, 0, 1 }
        });

        /// <summary>
        ///     Initializes the <see cref="ImageRegister" /> class.
        /// </summary>
        static ImageRegister()
        {
            // Initialize default Filter settings
            FilterSettings[ImageFilters.GaussianBlur] = new ImageFilterConfig { Factor = 1.0 / 16.0, Bias = 0.0 };
            FilterSettings[ImageFilters.BoxBlur] = new ImageFilterConfig { Factor = 1.0 / 9.0, Bias = 0.0 };
            FilterSettings[ImageFilters.MotionBlur] = new ImageFilterConfig { Factor = 1.0 / 5.0, Bias = 0.0 };
            FilterSettings[ImageFilters.Sharpen] =
                new ImageFilterConfig { Factor = 1.0, Bias = 0.0 }; // Assuming default values
            FilterSettings[ImageFilters.Emboss] =
                new ImageFilterConfig { Factor = 1.0, Bias = 0.0 }; // Assuming default values
            FilterSettings[ImageFilters.Laplacian] =
                new ImageFilterConfig { Factor = 1.0, Bias = 0.0 }; // Assuming default values
            FilterSettings[ImageFilters.EdgeEnhance] =
                new ImageFilterConfig { Factor = 1.0, Bias = 0.0 }; // Assuming default values
            FilterSettings[ImageFilters.UnsharpMask] =
                new ImageFilterConfig { Factor = 1.0, Bias = 0.0 }; // Assuming default values
            FilterSettings[ImageFilters.AnisotropicKuwahara] = new ImageFilterConfig { BaseWindowSize = 5 };
            FilterSettings[ImageFilters.SupersamplingAntialiasing] = new ImageFilterConfig { Scale = 1 };
            FilterSettings[ImageFilters.PostProcessingAntialiasing] = new ImageFilterConfig { Sigma = 1.0 };
            // Add more default settings as needed

            // Initialize default Texture settings
            TextureSetting[TextureType.Noise] =
                new TextureConfig { MinValue = 0, MaxValue = 255, Alpha = 255, TurbulenceSize = 64 };
            TextureSetting[TextureType.Clouds] =
                new TextureConfig { MinValue = 0, MaxValue = 255, Alpha = 255, TurbulenceSize = 64 };
            TextureSetting[TextureType.Marble] =
                new TextureConfig { Alpha = 255, BaseColor = Color.FromArgb(30, 10, 0) };
            TextureSetting[TextureType.Wood] =
                new TextureConfig { Alpha = 255, BaseColor = Color.FromArgb(80, 30, 30) };
            TextureSetting[TextureType.Wave] = new TextureConfig { Alpha = 255 };
            // Add more default settings as needed
        }

        /// <summary>
        ///     Gets or sets the count of retries.
        /// </summary>
        /// <value>
        ///     The count.
        /// </value>
        internal static int Count { get; set; }

        /// <summary>
        ///     Gets the settings.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>Return the current config</returns>
        internal static ImageFilterConfig GetSettings(ImageFilters filter)
        {
            return FilterSettings.TryGetValue(filter, out var config) ? config : new ImageFilterConfig();
        }

        /// <summary>
        ///     Sets the settings.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="config">The configuration.</param>
        public static void SetSettings(ImageFilters filter, ImageFilterConfig config)
        {
            FilterSettings[filter] = config;
        }

        /// <summary>
        ///     Gets the settings.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static TextureConfig GetSettings(TextureType filter)
        {
            return TextureSetting.TryGetValue(filter, out var config) ? config : new TextureConfig();
        }

        /// <summary>
        ///     Sets the settings.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="config">The configuration.</param>
        public static void SetSettings(TextureType filter, TextureConfig config)
        {
            TextureSetting[filter] = config;
        }
    }
}
