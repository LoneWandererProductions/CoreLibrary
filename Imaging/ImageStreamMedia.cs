/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/ImageStreamMedia.cs
 * PURPOSE:     Does all the leg work for the Image operations, in this case the newer Media.Imaging
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCE:      https://lodev.org/cgtutor/floodfill.html
 */

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Imaging.Helpers;

namespace Imaging
{
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
                // bmp.CreateOptions = BitmapCreateOptions.DelayCreation; is bad it does not load all data immediately.
                bmp.CacheOption = BitmapCacheOption.OnLoad;

                using var flStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                bmp.StreamSource = flStream;

                if (width > 0 && height > 0)
                {
                    bmp.DecodePixelWidth = width;
                    bmp.DecodePixelHeight = height;
                }

                bmp.EndInit();
                bmp.Freeze();

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

            using var ms = new MemoryStream();
            var enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(image));
            enc.Save(ms);

            ms.Position = 0;

            using var tmp = new Bitmap(ms);
            return new Bitmap(tmp); // deep copy, no stream dependency
        }

        /// <summary>
        /// Converts a System.Drawing <see cref="Bitmap"/> to a WPF <see cref="BitmapImage"/>.
        /// </summary>
        /// <param name="bitmap">The source bitmap.</param>
        /// <returns>A <see cref="BitmapImage"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            ImageHelper.ValidateImage(nameof(BitmapToBitmapImage), bitmap);

            var width = bitmap.Width;
            var height = bitmap.Height;

            // 1. Memory Copy to WriteableBitmap (Your efficient logic)
            var wbmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            var rect = new System.Drawing.Rectangle(0, 0, width, height);
            var bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            wbmp.Lock();
            unsafe
            {
                Buffer.MemoryCopy((void*)bmpData.Scan0, (void*)wbmp.BackBuffer, width * height * 4, width * height * 4);
            }
            wbmp.AddDirtyRect(new Int32Rect(0, 0, width, height));
            wbmp.Unlock();
            bitmap.UnlockBits(bmpData);

            // 2. The Bridge to BitmapImage
            var bitmapImage = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                // BMP Encoder is leaner and faster than PNG for internal memory swaps
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbmp));
                encoder.Save(stream);
                stream.Position = 0;

                bitmapImage.BeginInit();
                bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            bitmapImage.Freeze();
            return bitmapImage;
        }
    }
}
