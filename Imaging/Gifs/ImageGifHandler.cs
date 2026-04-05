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

using ExtendedSystemObjects;
using Imaging.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Imaging.Gifs
{
    /// <summary>
    ///     Central Entry class for all things related to gifs
    /// </summary>
    public static class ImageGifHandler
    {
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
        /// <summary>
        ///     Splits the GIF into individual frames while filling transparency holes with Gray
        ///     and accumulating frame data to prevent incomplete images.
        /// </summary>
        /// <param name="path">The path to the GIF file.</param>
        /// <returns>List of composed Bitmaps.</returns>
        internal static async Task<List<Bitmap>> SplitGifAsync(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                throw new IOException($"File not found: {path}");

            return await Task.Run(() =>
            {
                var frames = new List<Bitmap>();

                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var gifImage = Image.FromStream(fs);

                var frameCount = gifImage.GetFrameCount(FrameDimension.Time);
                var width = gifImage.Width;
                var height = gifImage.Height;

                // This is our canvas that survives the loop
                var masterCanvas = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using var g = Graphics.FromImage(masterCanvas);

                // Start the whole GIF on a Gray floor
                g.Clear(System.Drawing.Color.Gray);

                // GIF Property ID for Frame Delay and Disposal
                // 0x5100 is the PropertyTagFrameDelay in GDI+
                var disposalProperty = gifImage.GetPropertyItem(0x5100);

                for (var i = 0; i < frameCount; i++)
                {
                    gifImage.SelectActiveFrame(FrameDimension.Time, i);

                    // Get the disposal method for THIS specific frame
                    // The disposal method is usually the 4th byte of the property data
                    var disposalMethod = disposalProperty.Value[i * 4 + 3];

                    // If Disposal Method is 2 (Restore to Background), 
                    // we have to clear the canvas back to Gray before drawing this frame
                    if (disposalMethod == 2)
                    {
                        g.Clear(System.Drawing.Color.Gray);
                    }

                    // Draw the current frame patch
                    g.DrawImage(gifImage, new Rectangle(0, 0, width, height));

                    // Snapshot the result
                    frames.Add(new Bitmap(masterCanvas));

                    // If the disposal method was 3 (Restore to Previous), 
                    // technically we should undo the last draw, but Method 2 is the 
                    // one that usually causes the "white hole" issue.
                }

                return frames;
            });
        }

        /// <summary>
        /// Loads the GIF using WPF's native decoder.
        /// This perfectly preserves transparency and is much faster.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// List of Images from gif as ImageSource
        /// </returns>
        /// <exception cref="System.IO.IOException">File not found: {path}</exception>
        internal static async Task<List<ImageSource>> LoadGif(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                throw new IOException($"File not found: {path}");

            return await Task.Run(() =>
            {
                var result = new List<ImageSource>();

                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var decoder = new GifBitmapDecoder(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);

                int width = decoder.Frames[0].PixelWidth;
                int height = decoder.Frames[0].PixelHeight;

                var composed = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);

                // Clear initial canvas
                ImageGifHelper.ClearBitmap(composed);

                WriteableBitmap? previousFrameBackup = null;

                int prevLeft = 0, prevTop = 0, prevWidth = 0, prevHeight = 0;
                int prevDisposal = 0;

                foreach (var frame in decoder.Frames)
                {
                    var metadata = frame.Metadata as BitmapMetadata;

                    int left = ImageGifHelper.GetShort(metadata, "/imgdesc/Left");
                    int top = ImageGifHelper.GetShort(metadata, "/imgdesc/Top");
                    int disposal = ImageGifHelper.GetShort(metadata, "/grctlext/Disposal");

                    int frameWidth = frame.PixelWidth;
                    int frameHeight = frame.PixelHeight;

                    // 🔁 1. Apply previous disposal
                    switch (prevDisposal)
                    {
                        case 2: // Restore to background
                            ImageGifHelper.ClearRegion(composed, prevLeft, prevTop, prevWidth, prevHeight);
                            break;

                        case 3: // Restore to previous
                            if (previousFrameBackup != null)
                            {
                                ImageGifHelper.CopyBitmap(previousFrameBackup, composed);
                            }
                            break;
                    }

                    // 💾 2. Backup if current frame requires it
                    if (disposal == 3)
                    {
                        previousFrameBackup = composed.Clone();
                        previousFrameBackup.Freeze();
                    }
                    else
                    {
                        previousFrameBackup = null;
                    }

                    // 🧩 3. Blend frame into canvas
                    ImageGifHelper.BlendFrame(composed, frame, left, top);

                    // 📸 4. Snapshot
                    //var snapshot = composed.Clone();
                    //snapshot.Freeze();

                    var snapshot = new WriteableBitmap(composed);
                    snapshot.Freeze();

                    result.Add(snapshot);

                    // store for next loop
                    prevDisposal = disposal;
                    prevLeft = left;
                    prevTop = top;
                    prevWidth = frameWidth;
                    prevHeight = frameHeight;
                }

                return result;
            });
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
            var gifDelay = (ushort)(delayMs / 10);

            foreach (var bmpImage in btm)
            {
                var src = ConvertToBitmapSource(bmpImage);
                var metadata = new BitmapMetadata("gif");

                metadata.SetQuery("/grctlext/Delay", gifDelay);
                // Optional but recommended: Set Disposal method to "Restore to Background" (2)
                // This prevents transparent frames from stacking on top of each other.
                metadata.SetQuery("/grctlext/Disposal", 2);

                gEnc.Frames.Add(BitmapFrame.Create(src, null, metadata, null));
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
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

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
        /// <summary>
        /// Helper method to inject the Netscape looping extension safely.
        /// </summary>
        private static void SaveWithLoopingExtension(GifBitmapEncoder gEnc, string target)
        {
            using var ms = new MemoryStream();
            gEnc.Save(ms);
            var fileBytes = ms.ToArray();

            // NETSCAPE2.0 Application Extension (loops forever)
            var applicationExtension = new byte[] { 33, 255, 11, 78, 69, 84, 83, 67, 65, 80, 69, 50, 46, 48, 3, 1, 0, 0, 0 };

            // GIF Header (6) + Logical Screen Descriptor (7) = 13 bytes
            int insertionIndex = 13;

            // Byte 10 contains the packed fields. 
            // Bit 7 (0x80) indicates if a Global Color Table (GCT) is present.
            bool hasGlobalColorTable = (fileBytes[10] & 0x80) != 0;

            if (hasGlobalColorTable)
            {
                // The size of the GCT is determined by the lower 3 bits of byte 10.
                // Size = 3 * 2^(value + 1)
                int gctSizeValue = fileBytes[10] & 0x07;
                int gctSize = 3 * (1 << (gctSizeValue + 1));

                // Push the insertion index past the Global Color Table
                insertionIndex += gctSize;
            }

            var newBytes = new List<byte>(fileBytes.Length + applicationExtension.Length);

            // Safely insert the extension after the Header, LSD, and GCT
            newBytes.AddRange(fileBytes.Take(insertionIndex));
            newBytes.AddRange(applicationExtension);
            newBytes.AddRange(fileBytes.Skip(insertionIndex));

            File.WriteAllBytes(target, newBytes.ToArray());
        }

        /// <summary>
        /// Helper method to safely convert System.Drawing.Bitmap to WPF BitmapSource
        /// while perfectly preserving transparency.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns>Image as BitmapSource.</returns>
        private static BitmapSource ConvertToBitmapSource(Bitmap bitmap)
        {
            using var ms = new MemoryStream();
            // Saving as PNG preserves the Alpha channel
            bitmap.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Freezing for WPF cross-thread usage

            return bitmapImage;
        }
    }
}
