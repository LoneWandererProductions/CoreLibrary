/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        ImagingFacade.cs
 * PURPOSE:     Official public entry point for the Imaging engine
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Imaging.Enums;

namespace Imaging
{
    /// <summary>
    ///     Unified façade for the Imaging engine.
    ///     This class provides a stable public API for common image processing tasks,
    ///     including loading, saving, conversion, resizing, filtering, pixel manipulation,
    ///     GIF handling, blending, and texture generation.
    ///     All internal classes like <see cref="ImageRender"/> or <see cref="TextureGenerator"/>
    ///     are hidden behind this façade.
    /// </summary>
    public static class ImagingFacade
    {
        #region Register

        /// <summary>
        /// Gets the singleton instance of the <see cref="ImageRegister"/>.
        /// Allows querying and modifying filter and texture settings globally.
        /// </summary>
        public static ImageRegister Register => ImageRegister.Instance;

        /// <summary>
        /// Retrieves the configuration for a specific filter type.
        /// </summary>
        /// <param name="filter">The filter type.</param>
        /// <returns>The current <see cref="FiltersConfig"/> for the filter.</returns>
        public static FiltersConfig GetFilterSettings(FiltersType filter) => Register.GetSettings(filter);

        /// <summary>
        /// Updates the configuration for a specific filter type.
        /// </summary>
        /// <param name="filter">The filter type.</param>
        /// <param name="config">The new filter configuration.</param>
        public static void SetFilterSettings(FiltersType filter, FiltersConfig config) =>
            Register.SetSettings(filter, config);

        /// <summary>
        /// Retrieves all available filters.
        /// </summary>
        /// <returns>An enumerable of available <see cref="FiltersType"/>.</returns>
        public static IEnumerable<FiltersType> GetAvailableFilters() => Register.GetAvailableFilters();

        /// <summary>
        /// Gets the property names used by a specific filter type.
        /// </summary>
        /// <param name="filter">The filter type.</param>
        /// <returns>A set of property names relevant to the filter.</returns>
        public static HashSet<string> GetFilterProperties(FiltersType filter) => Register.GetUsedProperties(filter);

        /// <summary>
        /// Retrieves the configuration for a specific texture type.
        /// </summary>
        /// <param name="texture">The texture type.</param>
        /// <returns>The current <see cref="TextureConfiguration"/> for the texture.</returns>
        public static TextureConfiguration GetTextureSettings(TextureType texture) => Register.GetSettings(texture);

        /// <summary>
        /// Updates the configuration for a specific texture type.
        /// </summary>
        /// <param name="texture">The texture type.</param>
        /// <param name="config">The new texture configuration.</param>
        public static void SetTextureSettings(TextureType texture, TextureConfiguration config) =>
            Register.SetSettings(texture, config);

        /// <summary>
        /// Gets the property names used by a specific texture type.
        /// </summary>
        /// <param name="texture">The texture type.</param>
        /// <returns>A set of property names relevant to the texture.</returns>
        public static HashSet<string> GetTextureProperties(TextureType texture) => Register.GetUsedProperties(texture);

        /// <summary>
        /// Loads filter and texture settings from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string containing settings.</param>
        public static void LoadSettingsFromJson(string json) => Register.LoadSettingsFromJson(json);

        /// <summary>
        /// Retrieves the current filter and texture settings as a JSON string.
        /// </summary>
        /// <returns>A JSON representation of the current settings.</returns>
        public static string GetSettingsAsJson() => Register.GetSettingsAsJson();

        /// <summary>
        /// Adds an error to the global Imaging error log.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        public static void LogError(Exception ex) => Register.SetError(ex);

        /// <summary>
        /// Gets the last error message recorded by the Imaging engine.
        /// </summary>
        public static string? LastError => Register.LastError;

        #endregion

        #region Load / Save

        /// <summary>
        ///     Loads a bitmap from the specified file path.
        /// </summary>
        /// <param name="path">The file path to the image.</param>
        /// <returns>A <see cref="Bitmap"/> representing the loaded image.</returns>
        public static Bitmap Load(string path)
            => new ImageRender().GetOriginalBitmap(path);

        /// <summary>
        ///     Saves a bitmap to the specified path in the given format.
        /// </summary>
        /// <param name="bitmap">The bitmap to save.</param>
        /// <param name="path">The target file path.</param>
        /// <param name="format">The <see cref="ImageFormat"/> to save as.</param>
        public static void Save(Bitmap bitmap, string path, ImageFormat format)
            => new ImageRender().SaveBitmap(bitmap, path, format);

        #endregion

        #region Conversion

        /// <summary>
        ///     Converts a <see cref="Bitmap"/> to a WPF <see cref="BitmapImage"/>.
        /// </summary>
        /// <param name="bitmap">The source bitmap.</param>
        /// <returns>The converted <see cref="BitmapImage"/>.</returns>
        public static BitmapImage ToBitmapImage(Bitmap bitmap)
            => new ImageRender().BitmapToBitmapImage(bitmap);

        /// <summary>
        ///     Converts a WPF <see cref="BitmapImage"/> to a <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="bitmapImage">The source bitmap image.</param>
        /// <returns>The converted <see cref="Bitmap"/>.</returns>
        public static Bitmap ToBitmap(BitmapImage bitmapImage)
            => new ImageRender().BitmapImageToBitmap(bitmapImage);

        #endregion

        #region Resize / Crop

        /// <summary>
        ///     Resizes the specified bitmap to the target width and height.
        /// </summary>
        /// <param name="image">The source bitmap.</param>
        /// <param name="width">The target width.</param>
        /// <param name="height">The target height.</param>
        /// <returns>A resized <see cref="Bitmap"/>.</returns>
        public static Bitmap Resize(Bitmap image, int width, int height)
            => new ImageRender().BitmapScaling(image, width, height);

        /// <summary>
        ///     Resizes the specified bitmap by a scaling factor.
        /// </summary>
        /// <param name="image">The source bitmap.</param>
        /// <param name="scaling">Scaling factor (1.0 = original size).</param>
        /// <returns>A resized <see cref="Bitmap"/>.</returns>
        public static Bitmap Resize(Bitmap image, float scaling)
            => new ImageRender().BitmapScaling(image, scaling);

        /// <summary>
        ///     Crops the bitmap to the specified rectangle.
        /// </summary>
        /// <param name="image">The source bitmap.</param>
        /// <param name="x">The X-coordinate of the crop origin.</param>
        /// <param name="y">The Y-coordinate of the crop origin.</param>
        /// <param name="width">The width of the cropped area.</param>
        /// <param name="height">The height of the cropped area.</param>
        /// <returns>The cropped <see cref="Bitmap"/>.</returns>
        public static Bitmap Crop(Bitmap image, int x, int y, int width, int height)
            => new ImageRender().CutBitmap(image, x, y, height, width);

        #endregion

        #region Filters

        /// <summary>
        ///     Applies a global filter to the bitmap.
        /// </summary>
        /// <param name="image">The source bitmap.</param>
        /// <param name="filter">The filter type to apply.</param>
        /// <returns>The filtered <see cref="Bitmap"/>.</returns>
        public static Bitmap? ApplyFilter(Bitmap image, FiltersType filter)
            => new ImageRender().FilterImage(image, filter);

        /// <summary>
        ///     Applies a filter to a specified area of the bitmap with optional shape masking.
        /// </summary>
        /// <param name="image">The source bitmap.</param>
        /// <param name="filter">The filter type.</param>
        /// <param name="shape">The mask shape.</param>
        /// <param name="shapeParams">Optional parameters for the mask shape.</param>
        /// <param name="startPoint">Optional start point for the filter.</param>
        /// <returns>The filtered <see cref="Bitmap"/>.</returns>
        public static Bitmap ApplyFilterArea(Bitmap image, FiltersType filter, MaskShape shape, object shapeParams,
            Point? startPoint = null)
            => new ImageRender().FilterImageArea(image, null, null, filter, shape, shapeParams, startPoint);

        #endregion

        #region Drawing / Pixels

        /// <summary>
        ///     Gets the color of a pixel at the specified point.
        /// </summary>
        /// <param name="image">The bitmap.</param>
        /// <param name="p">The pixel location.</param>
        /// <returns>The <see cref="Color"/> of the pixel.</returns>
        public static Color GetPixel(Bitmap image, Point p)
            => new ImageRender().GetPixel(image, p);

        /// <summary>
        ///     Sets the color of a pixel at the specified point.
        /// </summary>
        /// <param name="image">The bitmap.</param>
        /// <param name="p">The pixel location.</param>
        /// <param name="color">The color to set.</param>
        public static void SetPixel(Bitmap image, Point p, Color color)
            => new ImageRender().SetPixel(image, p, color);

        /// <summary>
        ///     Performs a flood-fill operation starting from the specified pixel.
        /// </summary>
        /// <param name="image">The bitmap.</param>
        /// <param name="x">X-coordinate of start point.</param>
        /// <param name="y">Y-coordinate of start point.</param>
        /// <param name="newColor">The fill color.</param>
        public static void FloodFill(Bitmap image, int x, int y, Color newColor)
            => new ImageRender().FloodFillScanLineStack(image, x, y, newColor);

        #endregion

        #region Blending

        /// <summary>
        ///     Combines two bitmaps by overlaying the second bitmap onto the first at the specified position.
        /// </summary>
        /// <param name="baseImage">The base bitmap.</param>
        /// <param name="overlay">The overlay bitmap.</param>
        /// <param name="x">X-coordinate for overlay placement.</param>
        /// <param name="y">Y-coordinate for overlay placement.</param>
        /// <returns>The combined <see cref="Bitmap"/>.</returns>
        public static Bitmap? Combine(Bitmap baseImage, Bitmap overlay, int x, int y)
            => new ImageRender().CombineBitmap(baseImage, overlay, x, y);

        /// <summary>
        ///     Blends two images by averaging pixel values.
        /// </summary>
        /// <param name="a">The first bitmap.</param>
        /// <param name="b">The second bitmap.</param>
        /// <returns>The blended <see cref="Bitmap"/>.</returns>
        public static Bitmap Blend(Bitmap a, Bitmap b)
            => new ImageRender().AverageImages(a, b);

        #endregion

        #region Adjustments

        /// <summary>
        ///     Adjusts the brightness of the bitmap.
        /// </summary>
        /// <param name="image">The bitmap.</param>
        /// <param name="factor">Brightness factor.</param>
        /// <returns>The adjusted <see cref="Bitmap"/>.</returns>
        public static Bitmap AdjustBrightness(Bitmap image, double factor)
            => new ImageRender().AdjustBrightness(image, factor);

        /// <summary>
        ///     Adjusts the hue of the bitmap.
        /// </summary>
        /// <param name="image">The bitmap.</param>
        /// <param name="shift">Hue shift value.</param>
        /// <returns>The adjusted <see cref="Bitmap"/>.</returns>
        public static Bitmap AdjustHue(Bitmap image, double shift)
            => new ImageRender().AdjustHue(image, shift);

        /// <summary>
        ///     Adjusts the saturation of the bitmap.
        /// </summary>
        /// <param name="image">The bitmap.</param>
        /// <param name="factor">Saturation factor.</param>
        /// <returns>The adjusted <see cref="Bitmap"/>.</returns>
        public static Bitmap AdjustSaturation(Bitmap image, double factor)
            => new ImageRender().AdjustSaturation(image, factor);

        /// <summary>
        ///     Applies gamma correction to the bitmap.
        /// </summary>
        /// <param name="image">The bitmap.</param>
        /// <param name="gamma">Gamma value.</param>
        /// <returns>The gamma-corrected <see cref="Bitmap"/>.</returns>
        public static Bitmap ApplyGamma(Bitmap image, double gamma)
            => new ImageRender().ApplyGammaCorrection(image, gamma);

        #endregion

        #region GIF

        /// <summary>
        ///     Loads a GIF asynchronously and returns all frames as <see cref="Bitmap"/> objects.
        /// </summary>
        /// <param name="path">Path to the GIF file.</param>
        /// <returns>A task that represents the asynchronous operation, containing a read-only list of frames.</returns>
        public static async Task<IReadOnlyList<Bitmap>> LoadGifAsync(string path)
        {
            var sources = await new ImageRender().LoadGifAsync(path);

            var frames = new List<Bitmap>(sources.Count);
            frames.AddRange(sources.Select(src => ((BitmapImage)src).ToBitmap()));

            return frames;
        }

        /// <summary>
        ///     Creates a GIF from a sequence of frames.
        /// </summary>
        /// <param name="frames">The frames to include in the GIF.</param>
        /// <param name="target">The output file path.</param>
        public static void CreateGif(IEnumerable<FrameInfo> frames, string target)
            => new ImageRender().CreateGif(frames, target);

        #endregion

        #region Textures

        /// <summary>
        ///     Generates a procedural texture using the specified type, mask shape, and parameters.
        /// </summary>
        /// <param name="width">Width of the texture.</param>
        /// <param name="height">Height of the texture.</param>
        /// <param name="type">Type of texture (<see cref="TextureType"/>).</param>
        /// <param name="shape">Mask shape (<see cref="MaskShape"/>).</param>
        /// <param name="shapeParams">Optional parameters for the shape.</param>
        /// <param name="startPoint">Optional starting point.</param>
        /// <returns>The generated texture as a <see cref="Bitmap"/>.</returns>
        public static Bitmap? GenerateTexture(int width, int height, TextureType type, MaskShape shape,
            object shapeParams, Point? startPoint = null)
            => new TextureGenerator().GenerateTexture(width, height, type, shape, startPoint, shapeParams);

        #endregion
    }
}
