/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Gifs
 * FILE:        ImageGifHandler.cs
 * PURPOSE:     Some processing stuff for Gif Images, not perfect, the files are slightly bigger though.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://stackoverflow.com/questions/18719302/net-creating-a-looping-gif-using-gifbitmapencoder
 *              https://debugandrelease.blogspot.com/2018/12/creating-gifs-in-c.html
 *              http://www.matthewflickinger.com/lab/whatsinagif/bits_and_bytes.asp
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ExtendedSystemObjects;
using Imaging.Helpers;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Imaging.Gifs
{
    /// <summary>
    ///     Central Entry class for all things related to gifs
    /// </summary>
    public static class ImageGifHandler
    {
        /// <summary>
        /// Deletes the object.
        /// </summary>
        /// <param name="hObject">The h object.</param>
        /// <returns>Status of cleanup</returns>
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        internal static extern bool DeleteObject(nint hObject);

        /// <summary>
        ///     Gets the image information.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>gif Infos</returns>
        public static ImageGifInfo? GetImageInfo(string path)
        {
            ImageGifInfo? info = null;

            try
            {
                info = ImageGifMetadataExtractor.ExtractGifMetadata(path);
            }
            catch (FileNotFoundException ex)
            {
                Trace.WriteLine(ex.Message);
                //TODo fill up
            }
            catch (InvalidDataException ex)
            {
                Trace.WriteLine(ex.Message);
                //TODo fill up
            }

            return info;
        }

        /// <summary>
        ///     Splits the GIF.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>List of Images from gif</returns>
        internal static async Task<List<Bitmap>> SplitGifAsync(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                throw new IOException($"File not found: {path}");

            return await Task.Run(() =>
            {
                var frames = new List<Bitmap>();

                // Load GIF without locking the file
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var image = Image.FromStream(fs);

                var frameCount = image.GetFrameCount(FrameDimension.Time);

                for (var i = 0; i < frameCount; i++)
                {
                    image.SelectActiveFrame(FrameDimension.Time, i);

                    // clone the frame immediately ON THE SAME THREAD
                    frames.Add(new Bitmap(image));
                }

                return frames;
            });
        }


        /// <summary>
        ///     Loads the GIF.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>List of Images from gif as ImageSource</returns>
        internal static async Task<List<ImageSource>> LoadGif(string path)
        {
            // Await the result from the async SplitGifAsync method
            var bitmapList = await SplitGifAsync(path);

            // Convert each Bitmap to ImageSource and return the list
            return bitmapList.Select(image => image.ToBitmapImage()).Cast<ImageSource>().ToList();
        }

        /// <summary>
        /// Creates the gif.
        /// The gif is slightly bigger for now
        /// Sources:
        /// https://stackoverflow.com/questions/18719302/net-creating-a-looping-gif-using-gifbitmapencoder
        /// </summary>
        /// <param name="path">The path to the folder.</param>
        /// <param name="target">The target path.</param>
        /// <param name="delayMs">Optional delay between frames in milliseconds.</param>
        internal static void CreateGif(string path, string target, int delayMs = 100)
        {
            //get all allowed files from target folder
            var lst = FileHelper.GetFilesByExtensionFullPath(path, ImagingResources.Appendix);

            lst = lst.SortNaturally();

            //collect and convert all images
            var btm = lst.ConvertAll(ImageStream.GetOriginalBitmap);

            if (btm.IsNullOrEmpty())
            {
                return;
            }

            GifCreator(btm, target, delayMs);
        }

        /// <summary>
        /// Creates the GIF.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="target">The target.</param>
        /// <param name="delayMs">Optional delay between frames in milliseconds.</param>
        internal static void CreateGif(List<string> path, string target, int delayMs = 100)
        {
            //collect and convert all images
            var btm = path.ConvertAll(ImageStream.GetOriginalBitmap);

            if (btm.IsNullOrEmpty())
            {
                return;
            }

            GifCreator(btm, target, delayMs);
        }

        /// <summary>
        ///     Creates the GIF with variable delays per frame using Tuples.
        /// </summary>
        /// <param name="frames">Collection of Bitmap and specific DelayMs tuples.</param>
        /// <param name="target">The target.</param>
        internal static void CreateGif(IEnumerable<(Bitmap Image, int DelayMs)> frames, string target)
        {
            if (frames == null) return;
            GifCreator(frames, target);
        }

        /// <summary>
        ///     Creates the GIF.
        /// </summary>
        /// <param name="frames">The frames.</param>
        /// <param name="target">The target.</param>
        internal static void CreateGif(IEnumerable<FrameInfo>? frames, string target)
        {
            if (frames == null)
            {
                return;
            }

            GifCreator(frames, target);
        }

        /// <summary>
        /// Create the gif with a fixed delay.
        /// </summary>
        /// <param name="btm">A list of Bitmaps.</param>
        /// <param name="target">The target.</param>
        /// <param name="delayMs">The delay ms.</param>
        private static void GifCreator(IEnumerable<Bitmap> btm, string target, int delayMs)
        {
            var gEnc = new GifBitmapEncoder();
            ushort gifDelay = (ushort)(delayMs / 10); // Convert to hundredths of a second

            foreach (var bmpImage in btm)
            {
                nint hBitmap = bmpImage.GetHbitmap();
                try
                {
                    var src = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        nint.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                    var metadata = new BitmapMetadata("gif");
                    metadata.SetQuery("/grctlext/Delay", gifDelay);

                    gEnc.Frames.Add(BitmapFrame.Create(src, null, metadata, null));
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }

            SaveWithLoopingExtension(gEnc, target);
        }

        /// <summary>
        /// Create the gif with variable per-frame delay using Tuples.
        /// </summary>
        /// <param name="frames">The frames.</param>
        /// <param name="target">The target.</param>
        private static void GifCreator(IEnumerable<(Bitmap Image, int DelayMs)> frames, string target)
        {
            var gEnc = new GifBitmapEncoder();

            foreach (var frame in frames)
            {
                nint hBitmap = frame.Image.GetHbitmap();
                try
                {
                    var src = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        nint.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                    var metadata = new BitmapMetadata("gif");
                    metadata.SetQuery("/grctlext/Delay", (ushort)(frame.DelayMs / 10));

                    gEnc.Frames.Add(BitmapFrame.Create(src, null, metadata, null));
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }

            SaveWithLoopingExtension(gEnc, target);
        }

        /// <summary>
        /// Creates the GIF from FrameInfo.
        /// </summary>
        /// <param name="frames">The frames.</param>
        /// <param name="target">The target.</param>
        private static void GifCreator(IEnumerable<FrameInfo>? frames, string target)
        {
            if (frames == null) return;

            var gEnc = new GifBitmapEncoder();

            foreach (var frameInfo in frames)
            {
                var img = frameInfo.Image;

                var bmpData = img.LockBits(
                    new Rectangle(0, 0, img.Width, img.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb);

                try
                {
                    var bitmapSource = BitmapSource.Create(
                        img.Width,
                        img.Height,
                        img.HorizontalResolution,
                        img.VerticalResolution,
                        PixelFormats.Bgra32,
                        null,
                        bmpData.Scan0,
                        img.Height * bmpData.Stride,
                        bmpData.Stride);

                    var metadata = new BitmapMetadata(ImagingResources.GifMetadata);
                    metadata.SetQuery(ImagingResources.GifMetadataQueryDelay, (ushort)(frameInfo.DelayTime * 100));

                    gEnc.Frames.Add(BitmapFrame.Create(bitmapSource, null, metadata, null));
                }
                finally
                {
                    img.UnlockBits(bmpData);
                }
            }

            using var fs = new FileStream(target, FileMode.Create, FileAccess.Write);
            gEnc.Save(fs);
        }

        /// <summary>
        /// Helper method to inject the Netscape looping extension.
        /// </summary>
        /// <param name="gEnc">The g enc.</param>
        /// <param name="target">The target.</param>
        private static void SaveWithLoopingExtension(GifBitmapEncoder gEnc, string target)
        {
            using var ms = new MemoryStream();
            gEnc.Save(ms);
            var fileBytes = ms.ToArray();

            // This is the NETSCAPE2.0 Application Extension.
            var applicationExtension =
                new byte[] { 33, 255, 11, 78, 69, 84, 83, 67, 65, 80, 69, 50, 46, 48, 3, 1, 0, 0, 0 };

            var newBytes = new List<byte>(fileBytes.Length + applicationExtension.Length);
            newBytes.AddRange(fileBytes.Take(13));
            newBytes.AddRange(applicationExtension);
            newBytes.AddRange(fileBytes.Skip(13));

            File.WriteAllBytes(target, newBytes.ToArray());
        }
    }
}
