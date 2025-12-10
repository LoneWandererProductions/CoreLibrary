/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/ImageStreamMedia.cs
 * PURPOSE:     Does all the leg work for the Image operations, in this case the newer Media.Imaging
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCE:      https://lodev.org/cgtutor/floodfill.html
 */

using Imaging.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Imaging;

/// <summary>
/// Handle the more newer WPF Libraries.
/// </summary>
public static class ImageStreamMedia
{
    /// <summary>
    ///     Loads File one Time.
    ///     Can only be used to load an Image Once.
    ///     Use this for huge amounts of images that will be resized, else we will break memory limits.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <param name="width">Target width (optional).</param>
    /// <param name="height">Target height (optional).</param>
    /// <returns>A <see cref="BitmapImage"/>.</returns>
    /// <exception cref="IOException">Could not find the file.</exception>
    /// <exception cref="UriFormatException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="NotSupportedException">Unsupported file type.</exception>
    public static BitmapImage GetBitmapImage(string path, int width = 0, int height = 0)
    {
        ImageHelper.ValidateFilePath(path);

        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CreateOptions = BitmapCreateOptions.DelayCreation;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(path);

            if (width > 0 && height > 0)
            {
                bmp.DecodePixelWidth = width;
                bmp.DecodePixelHeight = height;
            }

            bmp.EndInit();
            bmp.Freeze(); // Must freeze only AFTER EndInit()

            return bmp;
        }
        catch (Exception ex)
        {
            ImageHelper.HandleException(ex);
            throw;
        }
    }

    /// <summary>
    ///     Loads file through a stream.
    ///     Takes longer but can be changed at runtime.
    /// </summary>
    /// <param name="path">Path to file.</param>
    /// <param name="width">Target width (optional).</param>
    /// <param name="height">Target height (optional).</param>
    /// <returns>A <see cref="BitmapImage"/> or null if the format is invalid.</returns>
    public static BitmapImage? GetBitmapImageFileStream(string path, int width = 0, int height = 0)
    {
        ImageHelper.ValidateFilePath(path);

        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CreateOptions = BitmapCreateOptions.DelayCreation;
            bmp.CacheOption = BitmapCacheOption.OnLoad;

            using (var flStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                bmp.StreamSource = flStream;

                if (width > 0 && height > 0)
                {
                    bmp.DecodePixelWidth = width;
                    bmp.DecodePixelHeight = height;
                }

                bmp.EndInit();
                bmp.Freeze();
            }

            return bmp;
        }
        catch (FileFormatException ex)
        {
            Trace.Write(ex.ToString());
            return null;
        }
        catch (Exception ex)
        {
            ImageHelper.HandleException(ex);
            throw;
        }
    }

    /// <summary>
    /// Converts a <see cref="BitmapImage"/> to a System.Drawing <see cref="Bitmap"/>.
    /// </summary>
    /// <param name="image">Source image.</param>
    /// <returns>A <see cref="Bitmap"/>.</returns>
    /// <exception cref="ArgumentNullException">If image is null.</exception>
    internal static Bitmap BitmapImageToBitmap(BitmapImage image)
    {
        ImageHelper.ValidateImage(nameof(BitmapImageToBitmap), image);

        using var outStream = new MemoryStream();
        var enc = new BmpBitmapEncoder();
        enc.Frames.Add(BitmapFrame.Create(image));
        enc.Save(outStream);

        outStream.Position = 0; // REQUIRED!
        return new Bitmap(outStream);
    }

    /// <summary>
    /// Converts a System.Drawing <see cref="Bitmap"/> to a WPF <see cref="BitmapImage"/>.
    /// </summary>
    /// <param name="image">The source bitmap.</param>
    /// <param name="lossless">If true, encode PNG losslessly.</param>
    /// <returns>A <see cref="BitmapImage"/>.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static BitmapImage BitmapToBitmapImage(Bitmap image, bool lossless = false)
    {
        if (!lossless)
            return FastConvert(image);

        ImageHelper.ValidateImage(nameof(BitmapToBitmapImage), image);

        Bitmap processed = image;
        Bitmap? temp = null;

        if (image.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
        {
            temp = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(temp);
            g.DrawImage(image, 0, 0);
            processed = temp;
        }

        try
        {
            using var ms = new MemoryStream();
            processed.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = ms;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        finally
        {
            temp?.Dispose();
        }
    }

    /// <summary>
    /// Fast conversion using WriteableBitmap.
    /// </summary>
    /// <param name="bmp">The BMP.</param>
    /// <returns>BitmapImage from Bitmap</returns>
    private static BitmapImage FastConvert(Bitmap bmp)
    {
        if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
        {
            var converted = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(converted);
            g.DrawImage(bmp, 0, 0);
            bmp = converted;
        }

        var rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
        var bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        try
        {
            var wb = new WriteableBitmap(
                bmp.Width,
                bmp.Height,
                96, 96,
                PixelFormats.Bgra32,
                null);

            wb.WritePixels(
                new Int32Rect(0, 0, bmp.Width, bmp.Height),
                bmpData.Scan0,
                Math.Abs(bmpData.Stride) * bmp.Height,
                Math.Abs(bmpData.Stride));

            wb.Freeze();

            return WriteableBitmapToBitmapImage(wb);
        }
        finally
        {
            bmp.UnlockBits(bmpData);
        }
    }

    /// <summary>
    /// Converts a <see cref="WriteableBitmap" /> to a <see cref="BitmapImage" />.
    /// </summary>
    /// <param name="wb">The wb.</param>
    /// <returns>BitmapImage from WriteableBitmap</returns>
    private static BitmapImage WriteableBitmapToBitmapImage(WriteableBitmap wb)
    {
        using var ms = new MemoryStream();
        var enc = new BmpBitmapEncoder();
        enc.Frames.Add(BitmapFrame.Create(wb));
        enc.Save(ms);
        ms.Position = 0;

        var img = new BitmapImage();
        img.BeginInit();
        img.CacheOption = BitmapCacheOption.OnLoad;
        img.StreamSource = ms;
        img.EndInit();
        img.Freeze();
        return img;
    }
}
