/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/ImageGifHandler.cs
 * PURPOSE:     Some processing stuff for Gif Images
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace Imaging
{
    public static class ImageGifHandler
    {
        /// <summary>
        ///     Gets the image information.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>gif Infos</returns>
        public static ImageGifInfo GetImageInfo(string path)
        {
            var info = new ImageGifInfo();

            using var image = Image.FromFile(path);
            info.Height = image.Height;
            info.Width = image.Width;
            info.Name = Path.GetFileName(path);
            info.Size = image.Size;

            if (!image.RawFormat.Equals(ImageFormat.Gif))
            {
                return null;
            }

            if (!ImageAnimator.CanAnimate(image))
            {
                return info;
            }

            var frameDimension = new FrameDimension(image.FrameDimensionsList[0]);

            var frameCount = image.GetFrameCount(frameDimension);

            info.AnimationLength = frameCount / 10 * frameCount;

            info.IsAnimated = true;

            info.IsLooped = BitConverter.ToInt16(image.GetPropertyItem(20737)?.Value!, 0) != 1;

            info.Frames = frameCount;

            return info;
        }

        /// <summary>
        ///     Splits the GIF.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>List of Images from gif</returns>
        internal static List<Bitmap> SplitGif(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                var innerException = path != null
                    ? new IOException(string.Concat(nameof(path), ImagingResources.Spacing, path))
                    : new IOException(nameof(path));
                throw new IOException(ImagingResources.ErrorMissingFile, innerException);
            }

            var lst = new List<Bitmap>();

            try
            {
                using var image = Image.FromFile(path);

                var numberOfFrames = image.GetFrameCount(FrameDimension.Time);

                for (var i = 0; i < numberOfFrames; i++)
                {
                    image.SelectActiveFrame(FrameDimension.Time, i);
                    var bmp = new Bitmap(image);
                    lst.Add(bmp);
                }
            }
            //try to handle potential Memory problem, a bit of a hack
            catch (OutOfMemoryException ex)
            {
                var currentProcess = Process.GetCurrentProcess();
                var memorySize = currentProcess.PrivateMemorySize64;

                Trace.WriteLine(string.Concat(ex, ImagingResources.Separator, ImagingResources.ErrorMemory,
                    memorySize));
                lst.Clear();
                //enforce clean up and hope for the best
                GC.Collect();

                ImageRegister.Count++;

                if (ImageRegister.Count < 3)
                {
                    SplitGif(path);
                }
                else
                {
                    ImageRegister.Count = 0;
                    throw;
                }
            }

            return lst;
        }

        /// <summary>
        ///     Loads the GIF.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>List of Images from gif as ImageSource</returns>
        internal static List<ImageSource> LoadGif(string path)
        {
            var list = SplitGif(path);

            return list.Select(image => image.ToBitmapImage()).Cast<ImageSource>().ToList();
        }
    }

    /// <summary>
    ///     The infos about the gif
    /// </summary>
    public sealed class ImageGifInfo
    {
        /// <summary>
        ///     Gets the height.
        /// </summary>
        /// <value>
        ///     The height.
        /// </value>
        public int Height { get; internal set; }

        /// <summary>
        ///     Gets the width.
        /// </summary>
        /// <value>
        ///     The width.
        /// </value>
        public int Width { get; internal set; }

        /// <summary>
        ///     Gets the length of the animation.
        /// </summary>
        /// <value>
        ///     The length of the animation.
        /// </value>
        public int AnimationLength { get; internal set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is animated.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is animated; otherwise, <c>false</c>.
        /// </value>
        internal bool IsAnimated { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is looped.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is looped; otherwise, <c>false</c>.
        /// </value>
        internal bool IsLooped { get; set; }

        /// <summary>
        ///     Gets the frames.
        /// </summary>
        /// <value>
        ///     The frames.
        /// </value>
        public int Frames { get; internal set; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name { get; internal set; }

        /// <summary>
        ///     Gets the size.
        /// </summary>
        /// <value>
        ///     The size.
        /// </value>
        public Size Size { get; internal set; }
    }
}
