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
/// Handle the more newer wpf Libraries
/// </summary>
public static class ImageStreamMedia
{
    /// <summary>
    ///     Loads File one Time
    ///     Can only used to load an Image Once
    ///     Use this for huge amounts of image that will be resized, else we will break memory limits
    /// </summary>
    /// <param name="path">path to the File</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <returns>
    ///     An Image as <see cref="BitmapImage" />.
    /// </returns>
    /// <exception cref="IOException">Could not find the File</exception>
    /// <exception cref="UriFormatException"></exception>
    /// <exception cref="InvalidOperationException">Could not get correct access to the Object</exception>
    /// <exception cref="NotSupportedException">File Type provided was not supported</exception>
    public static BitmapImage GetBitmapImage(string path, int width = 0, int height = 0)
    {
        ImageHelper.ValidateFilePath(path);
        try
        {
            var bmp = new BitmapImage { CreateOptions = BitmapCreateOptions.DelayCreation };
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(path);
            if (width > 0 && height > 0)
            {
                bmp.DecodePixelWidth = width;
                bmp.DecodePixelHeight = height;
            }

            bmp.EndInit();
            return bmp;
        }
        catch (Exception ex)
        {
            ImageHelper.HandleException(ex);
            // Optionally, rethrow or handle further as needed
            throw; // This will preserve the original stack trace and exception details
        }
    }

    /// <summary>
    ///     Loads File in a Stream
    ///     takes longer but can be changed on Runtime
    /// </summary>
    /// <param name="path">path to the File</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <returns>
    ///     An Image as <see cref="BitmapImage" />.
    /// </returns>
    /// <exception cref="IOException">
    /// </exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="IOException">Error while we try to access the File</exception>
    /// <exception cref="ArgumentException">No Correct Argument were provided</exception>
    /// <exception cref="NotSupportedException">File Type provided was not supported</exception>
    /// <exception cref="InvalidOperationException">Could not get correct access to the Object</exception>
    public static BitmapImage GetBitmapImageFileStream(string path, int width = 0, int height = 0)
    {
        ImageHelper.ValidateFilePath(path);

        var bmp = new BitmapImage { CreateOptions = BitmapCreateOptions.DelayCreation };

        try
        {
            using var flStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;

            if (width > 0 && height > 0)
            {
                bmp.DecodePixelWidth = width;
                bmp.DecodePixelHeight = height;
            }

            bmp.StreamSource = flStream;
            bmp.EndInit();

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
            // Optionally, rethrow or handle further as needed
            throw; // This will preserve the original stack trace and exception details
        }
    }

    /// <summary>
    ///     Bitmaps the image2 bitmap.
    /// </summary>
    /// <param name="image">The bitmap image.</param>
    /// <returns>
    ///     The Image as <see cref="Bitmap" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">if Image is null</exception>
    internal static Bitmap BitmapImageToBitmap(BitmapImage image)
    {
        ImageHelper.ValidateImage(nameof(BitmapImageToBitmap), image);

        using var outStream = new MemoryStream();
        var enc = new BmpBitmapEncoder();
        enc.Frames.Add(BitmapFrame.Create(image));
        enc.Save(outStream);
        var bitmap = new Bitmap(outStream);

        return new Bitmap(bitmap);
    }

    /// <summary>
    /// Converts to bitmap image.
    /// https://stackoverflow.com/questions/5199205/how-do-i-rotate-image-then-move-to-the-top-left-0-0-without-cutting-off-the-imag/5200280#5200280
    /// </summary>
    /// <param name="image">The bitmap.</param>
    /// <param name="lossless">if set to <c>true</c> [lossless].</param>
    /// <returns>BitmpaImage from Bitmap</returns>
    /// <exception cref="ArgumentNullException">if Image is null</exception>
    /// The Image as
    /// <see cref="BitmapImage" />
    /// .
    internal static BitmapImage BitmapToBitmapImage(Bitmap image, bool lossless = false)
    {
        if (!lossless)
            return FastConvert(image);

        ImageHelper.ValidateImage(nameof(BitmapToBitmapImage), image);

        Bitmap processed = image;
        Bitmap? tempImage = null;

        // Normalize to 32bppArgb if needed
        if (image.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
        {
            tempImage = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (var g = Graphics.FromImage(tempImage))
                g.DrawImage(image, 0, 0);

            processed = tempImage;
        }

        try
        {
            using var ms = new MemoryStream();
            processed.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = ms;
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.EndInit();
            bmp.Freeze();

            return bmp;
        }
        finally
        {
            tempImage?.Dispose();
        }
    }

    /// <summary>
    /// Fasts the convert.
    /// </summary>
    /// <param name="bmp">The BMP.</param>
    /// <returns>BitmapImage from Bitmap.</returns>
    private static BitmapImage FastConvert(Bitmap bmp)
    {
        // Ensure 32bpp ARGB
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
            // WPF erwartet Int32Rect und PixelFormats.Bgra32
            var wbRect = new Int32Rect(0, 0, bmp.Width, bmp.Height);

            var wb = new WriteableBitmap(
                bmp.Width,
                bmp.Height,
                96, 96,
                PixelFormats.Bgra32,
                null);

            // Verwende die IntPtr-Überladung: WritePixels(Int32Rect, IntPtr buffer, int bufferSize, int stride)
            wb.WritePixels(wbRect, bmpData.Scan0, Math.Abs(bmpData.Stride) * bmp.Height, Math.Abs(bmpData.Stride));
            wb.Freeze();

            return WriteableBitmapToBitmapImage(wb);
        }
        finally
        {
            bmp.UnlockBits(bmpData);
        }
    }

    /// <summary>
    /// Writeables the bitmap to bitmap image.
    /// </summary>
    /// <param name="wb">The wb.</param>
    /// <returns>BitmapImage from Bitmap.</returns>
    private static BitmapImage WriteableBitmapToBitmapImage(WriteableBitmap wb)
    {
        var encoder = new JpegBitmapEncoder(); // MUCH faster than PNG
        encoder.QualityLevel = 100;            // visually lossless

        using var ms = new MemoryStream();
        encoder.Frames.Add(BitmapFrame.Create(wb));
        encoder.Save(ms);
        ms.Position = 0;

        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.StreamSource = ms;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }
}