/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/ImageStream.cs
 * PURPOSE:     Does all the leg work for the Image operations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using ExtendedSystemObjects;
using Mathematics;

namespace Imaging
{
    /// <summary>
    ///     Loads a BitMapImage out of a specific path
    ///     Can Combine two Images and returns a new one
    /// </summary>
    public static class ImageStream
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
            ValidateFilePath(path);
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
        /// <exception cref="IOException">Error while we try to access the File</exception>
        public static BitmapImage GetBitmapImageFileStream(string path, int width = 0, int height = 0)
        {
            ValidateFilePath(path);

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
        ///     Get the bitmap file.
        ///     Will  leak like crazy. Only use it if we load Icons or something.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///     The Image as <see cref="Bitmap" />.
        /// </returns>
        /// <exception cref="IOException">File not Found</exception>
        internal static Bitmap GetBitmapFile(string path)
        {
            ValidateFilePath(path);

            return new Bitmap(path, true);
        }

        /// <summary>
        ///     Gets the original bitmap file.
        ///     Takes longer but produces a higher quality
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        ///     The Image in Original size
        /// </returns>
        /// <exception cref="IOException">
        /// </exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        internal static Bitmap GetOriginalBitmap(string path)
        {
            ValidateFilePath(path);

            try
            {
                using var flStream = new FileStream(path, FileMode.Open);
                // Original picture information
                var original = new Bitmap(flStream);

                var bmp = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppPArgb);

                using var graph = Graphics.FromImage(bmp);
                graph.Clear(Color.Transparent);
                graph.CompositingMode = CompositingMode.SourceCopy;
                graph.CompositingQuality = CompositingQuality.HighQuality;
                graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graph.SmoothingMode = SmoothingMode.HighQuality;
                graph.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graph.DrawImage(original, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0,
                    original.Width,
                    original.Height, GraphicsUnit.Pixel);

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
        ///     Resizes an image
        /// </summary>
        /// <param name="image">The image to resize</param>
        /// <param name="width">The new width in pixels</param>
        /// <param name="height">The new height in pixels</param>
        /// <returns>
        ///     A resized version of the original image as <see cref="Bitmap" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">if Image is null</exception>
        /// <exception cref="ArgumentException">
        /// </exception>
        /// <exception cref="InsufficientMemoryException"></exception>
        internal static Bitmap BitmapScaling(Bitmap image, int width, int height)
        {
            ImageHelper.ValidateImage(nameof(BitmapScaling), image);

            var btm = new Bitmap(width, height);
            //fix Resolution
            btm.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            try
            {
                using var graph = Graphics.FromImage(btm);
                graph.CompositingMode = CompositingMode.SourceCopy;
                graph.CompositingQuality = CompositingQuality.HighQuality;
                graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graph.SmoothingMode = SmoothingMode.HighQuality;
                graph.PixelOffsetMode = PixelOffsetMode.HighQuality;

                graph.DrawImage(image, 0, 0, width, height);
                return new Bitmap(btm);
            }

            catch (InsufficientMemoryException ex)
            {
                Trace.WriteLine(ex);
                throw new InsufficientMemoryException(ex.Message);
            }
            catch (OutOfMemoryException ex)
            {
                Trace.WriteLine(ex);
                throw;
            }
            catch (Exception ex)
            {
                ImageHelper.HandleException(ex);
                // Optionally, rethrow or handle further as needed
                throw; // This will preserve the original stack trace and exception details
            }
        }

        /// <summary>
        ///     Bitmaps the scaling.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="scaling">The scaling.</param>
        /// <returns>
        ///     A resized version of the original image as <see cref="Bitmap" />.
        /// </returns>
        /// <exception cref="ArgumentNullException">if Image is null</exception>
        internal static Bitmap BitmapScaling(Bitmap image, float scaling)
        {
            ImageHelper.ValidateImage(nameof(BitmapScaling), image);

            var width = (int)(image.Width * scaling);
            var height = (int)(image.Height * scaling);

            //needed because of: A Graphics object cannot be created from an image that has an indexed pixel format
            var btm = new Bitmap(width, height);
            //fix Resolution
            btm.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graph = Graphics.FromImage(btm))
            {
                graph.CompositingMode = CompositingMode.SourceCopy;
                graph.CompositingQuality = CompositingQuality.HighQuality;
                graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graph.SmoothingMode = SmoothingMode.HighQuality;
                graph.PixelOffsetMode = PixelOffsetMode.HighQuality;

                var rect = new Rectangle(0, 0, width, height);

                using var wrapMode = new ImageAttributes();
                wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                graph.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
            }

            return new Bitmap(btm);
        }

        /// <summary>
        ///     Combines the bitmaps overlays them and merges them into one Image
        ///     Can and will throw Exceptions as part of nameof(ToBitmapImage)
        ///     The size will be trimmed to the size of the first Image
        /// </summary>
        /// <param name="files">The files.</param>
        /// <returns>
        ///     a new Bitmap with all combined Images
        /// </returns>
        /// <exception cref="ArgumentNullException">if file is null or Empty</exception>
        [return: MaybeNull]
        internal static Bitmap CombineBitmap(List<string> files)
        {
            if (files.IsNullOrEmpty())
            {
                var innerException =
                    new ArgumentNullException(string.Concat(nameof(CombineBitmap), ImagingResources.Spacing,
                        nameof(files)));
                throw new ArgumentNullException(ImagingResources.ErrorWrongParameters, innerException);
            }

            //read all images into memory
            var images = new List<Bitmap>();

            foreach (var image in files)
            {
                //create a Bitmap from the file and add it to the list
                if (!File.Exists(image))
                {
                    Trace.WriteLine(string.Concat(ImagingResources.ErrorMissingFile, image));
                    continue;
                }

                var bitmap = new Bitmap(image);

                images.Add(bitmap);
            }

            if (images.IsNullOrEmpty())
            {
                Trace.WriteLine(ImagingResources.ErrorMissingFile);
                return null;
            }

            //get the correct size of the Final Image
            var bmp = images[0];

            //create a bitmap to hold the combined image
            var btm = new Bitmap(bmp.Width, bmp.Height);

            //get a graphics object from the image so we can draw on it
            using (var graph = Graphics.FromImage(btm))
            {
                //go through each image and draw it on the final image
                foreach (var image in images)
                {
                    graph.DrawImage(image,
                        new Rectangle(0, 0, image.Width, image.Height));
                }
            }

            foreach (var image in images)
            {
                image.Dispose();
            }

            //before return please Convert
            return btm;
        }

        /// <summary>
        ///     Combines the bitmaps.
        /// </summary>
        /// <param name="image">The original image.</param>
        /// <param name="overlay">The overlay image.</param>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <returns>Combined Image</returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static Bitmap CombineBitmap(Bitmap image, Bitmap overlay, int x, int y)
        {
            ImageHelper.ValidateImage(nameof(CombineBitmap), image);

            if (overlay == null)
            {
                var innerException = new ArgumentNullException(string.Concat(nameof(CombineBitmap),
                    ImagingResources.Spacing, nameof(overlay)));
                throw new ArgumentNullException(ImagingResources.ErrorWrongParameters, innerException);
            }

            //get a graphics object from the image so we can draw on it
            using var graph = Graphics.FromImage(image);

            graph.DrawImage(overlay,
                new Rectangle(x, y, overlay.Width, overlay.Height));

            return image;
        }

        /// <summary>
        ///     Cuts a piece out of a bitmap.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <returns>The cut Image</returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static Bitmap CutBitmap(Bitmap image, int x, int y, int height, int width)
        {
            ImageHelper.ValidateImage(nameof(CutBitmap), image);

            var btm = new Bitmap(width, height);

            using var graph = Graphics.FromImage(btm);
            graph.CompositingMode = CompositingMode.SourceCopy;
            graph.CompositingQuality = CompositingQuality.HighQuality;
            graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graph.SmoothingMode = SmoothingMode.HighQuality;
            graph.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graph.Clear(Color.White);
            var rect = new Rectangle(x, y, width, height);

            graph.DrawImage(image, 0, 0, rect, GraphicsUnit.Pixel);

            return btm;
        }

        /// <summary>
        ///     Cuts a bitmap.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <returns>List of cut Images</returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static List<Bitmap> CutBitmaps(Bitmap image, int x, int y, int height, int width)
        {
            ImageHelper.ValidateImage(nameof(CutBitmaps), image);

            //read all images into memory
            var images = new List<Bitmap>(x * y);

            for (var j = 0; j < y; j++)
            for (var i = 0; i < x; i++)
            {
                var img = CutBitmap(image, i * width, j * height, height, width);
                images.Add(img);
            }

            return images;
        }

        /// <summary>
        ///     Erases the rectangle from an Image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="height">The height.</param>
        /// <param name="width">The width.</param>
        /// <returns>Original Image with the erased area</returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static Bitmap EraseRectangle(Bitmap image, int x, int y, int height, int width)
        {
            ImageHelper.ValidateImage(nameof(EraseRectangle), image);

            using var graph = Graphics.FromImage(image);

            graph.CompositingMode = CompositingMode.SourceCopy;

            using var br = new SolidBrush(Color.FromArgb(0, 255, 255, 255));

            graph.FillRectangle(br, new Rectangle(x, y, width, height));

            return image;
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
            if (image == null)
            {
                var innerException = new ArgumentNullException(string.Concat(nameof(BitmapImageToBitmap),
                    ImagingResources.Spacing, nameof(image)));
                throw new ArgumentNullException(ImagingResources.ErrorWrongParameters, innerException);
            }

            using var outStream = new MemoryStream();
            var enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(image));
            enc.Save(outStream);
            var bitmap = new Bitmap(outStream);

            return new Bitmap(bitmap);
        }

        /// <summary>
        ///     Converts to bitmap image.
        ///     https://stackoverflow.com/questions/5199205/how-do-i-rotate-image-then-move-to-the-top-left-0-0-without-cutting-off-the-imag/5200280#5200280
        /// </summary>
        /// <param name="image">The bitmap.</param>
        /// The Image as
        /// <see cref="BitmapImage" />
        /// .
        /// <exception cref="ArgumentNullException">if Image is null</exception>
        internal static BitmapImage BitmapToBitmapImage(Bitmap image)
        {
            ImageHelper.ValidateImage(nameof(BitmapToBitmapImage), image);

            using var memory = new MemoryStream();
            image.Save(memory, ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }

        /// <summary>
        ///     Rotates the image.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="degree">The angle in degrees</param>
        /// <returns>
        ///     Rotated Image
        /// </returns>
        /// <exception cref="ArgumentNullException">if Image is null</exception>
        /// <exception cref="OverflowException">Degrees have a certain allowed radius</exception>
        internal static Bitmap RotateImage(Bitmap image, int degree)
        {
            ImageHelper.ValidateImage(nameof(RotateImage), image);

            //no need to do anything
            if (degree is 360 or 0)
            {
                return image;
            }

            if (degree is > 360 or < -360)
            {
                var innerException = new OverflowException(nameof(degree));
                throw new OverflowException(string.Concat(ImagingResources.ErrorWrongParameters, degree),
                    innerException);
            }

            //Calculate the size of the new Bitmap because if we rotate the Image it will be bigger
            var wOver = image.Width / 2.0f;
            var hOver = image.Height / 2.0f;

            // Get the coordinates of the corners, taking the origin to be the centre of the bitmap.
            PointF[] corners = { new(-wOver, -hOver), new(+wOver, -hOver), new(+wOver, +hOver), new(-wOver, +hOver) };

            for (var i = 0; i < 4; i++)
            {
                var point = corners[i];
                corners[i] =
                    new PointF(
                        (float)((point.X * ExtendedMath.CalcCos(degree)) - (point.Y * ExtendedMath.CalcSin(degree))),
                        (float)((point.X * ExtendedMath.CalcSin(degree)) + (point.Y * ExtendedMath.CalcCos(degree))));
            }

            // Find the min and max x and y coordinates.
            var minX = corners[0].X;
            var maxX = minX;
            var minY = corners[0].Y;
            var maxY = minY;

            for (var i = 1; i < 4; i++)
            {
                var p = corners[i];
                minX = Math.Min(minX, p.X);
                maxX = Math.Max(maxX, p.X);
                minY = Math.Min(minY, p.Y);
                maxY = Math.Max(maxY, p.Y);
            }

            // Get the size of the new bitmap.
            var newSize = new SizeF(maxX - minX, maxY - minY);
            // create it.
            var btm = new Bitmap((int)Math.Ceiling(newSize.Width), (int)Math.Ceiling(newSize.Height));
            //fix Resolution
            btm.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            //  draw the old bitmap on it.
            using var graph = Graphics.FromImage(btm);
            graph.TranslateTransform(newSize.Width / 2.0f, newSize.Height / 2.0f);
            graph.RotateTransform(degree);
            graph.TranslateTransform(-image.Width / 2.0f, -image.Height / 2.0f);
            graph.DrawImage(image, 0, 0);

            return btm;
        }

        /// <summary>
        ///     Crops the image.
        ///     Non Edges are defined as transparent
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>Coped Image</returns>
        /// <exception cref="ArgumentNullException"></exception>
        internal static Bitmap CropImage(Bitmap image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            var bounds = ImageHelper.GetNonTransparentBounds(image);

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                // Return an empty image or handle this case as needed
                return new Bitmap(1, 1);
            }

            var croppedBitmap = new Bitmap(bounds.Width, bounds.Height);
            using var graphics = Graphics.FromImage(croppedBitmap);
            graphics.DrawImage(image, 0, 0, bounds, GraphicsUnit.Pixel);

            return croppedBitmap;
        }

        /// <summary>
        ///     Saves the bitmap.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="path">The path where we want to save it.</param>
        /// <param name="format">The Image format.</param>
        /// <exception cref="ArgumentNullException">Wrong parameters</exception>
        /// <exception cref="IOException">File already exists</exception>
        /// <exception cref="ExternalException">Errors with the Path</exception>
        internal static void SaveBitmap(Bitmap image, string path, ImageFormat format)
        {
            ImageHelper.ValidateImage(nameof(SaveBitmap), image);

            if (format == null)
            {
                var innerException =
                    new ArgumentNullException(string.Concat(nameof(SaveBitmap), ImagingResources.Spacing,
                        nameof(format)));
                throw new ArgumentNullException(ImagingResources.ErrorWrongParameters, innerException);
            }

            if (string.IsNullOrEmpty(path))
            {
                var innerException =
                    new ArgumentNullException(string.Concat(nameof(SaveBitmap), ImagingResources.Spacing,
                        nameof(path)));
                throw new ArgumentNullException(ImagingResources.ErrorWrongParameters, innerException);
            }

            if (File.Exists(path))
            {
                var count = 1;

                var fileNameOnly = Path.GetFileNameWithoutExtension(path);
                var extension = Path.GetExtension(path);
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    return;
                }

                var newPath = path;

                while (File.Exists(newPath))
                {
                    var tempFileName = $"{fileNameOnly}({count++})";
                    newPath = Path.Combine(directory!, tempFileName + extension);
                }

                SaveBitmap(image, newPath, format);
            }

            try
            {
                image.Save(path, format);
            }
            catch (ExternalException ex)
            {
                Trace.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        ///     Converts White to Transparent.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="threshold">The threshold when the color is still white.</param>
        /// <returns>The Transparent Image</returns>
        /// <exception cref="ArgumentNullException">Wrong parameters</exception>
        internal static Bitmap ConvertWhiteToTransparent(Bitmap image, int threshold)
        {
            ImageHelper.ValidateImage(nameof(ConvertWhiteToTransparent), image);

            //use our new Format
            var dbm = DirectBitmap.GetInstance(image);

            //255,255,255 is White
            var replacementColor = Color.FromArgb(255, 255, 255);

            for (var x = 0; x < dbm.Width; x++)
            for (var y = 0; y < dbm.Height; y++)
            {
                var color = dbm.GetPixel(x, y);

                //not in the area? continue, 255 is White
                if (255 - color.R >= threshold || 255 - color.G >= threshold || 255 - color.B >= threshold)
                {
                    continue;
                }

                //replace Value under the threshold with pure White
                dbm.SetPixel(x, y, replacementColor);
            }

            //get the Bitmap
            var btm = new Bitmap(dbm.Bitmap);
            //make Transparent
            btm.MakeTransparent(replacementColor);
            //cleanup
            dbm.Dispose();
            return btm;
        }

        /// <summary>
        ///     Gets the pixel.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="point">The point.</param>
        /// <returns>
        ///     The Color at the point
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Image was null</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Point was out of bound.</exception>
        internal static Color GetPixel(Bitmap image, Point point)
        {
            ImageHelper.ValidateImage(nameof(GetPixel), image);

            if (point.X < 0 || point.X >= image.Width || point.Y < 0 || point.Y >= image.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(point), ImagingResources.ErrorOutOfBounds);
            }

            //use our new Format
            var dbm = DirectBitmap.GetInstance(image);
            return dbm.GetPixel(point.X, point.Y);
        }

        /// <summary>
        ///     Gets the pixel.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="point">The point.</param>
        /// <param name="radius">The radius.</param>
        /// <returns>
        ///     The Color at the Point
        /// </returns>
        /// <exception cref="System.ArgumentNullException">image was null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     radius or point is out of bounds.
        /// </exception>
        internal static Color GetPixel(Bitmap image, Point point, int radius)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image), ImagingResources.ErrorWrongParameters);
            }

            if (radius < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), ImagingResources.ErrorRadius);
            }

            if (point.X < 0 || point.X >= image.Width || point.Y < 0 || point.Y >= image.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(point), ImagingResources.ErrorOutOfBounds);
            }

            var points = ImageHelper.GetCirclePoints(point, radius, image.Height, image.Width);

            if (points.Count == 0)
            {
                return GetPixel(image, point);
            }

            int redSum = 0, greenSum = 0, blueSum = 0;

            foreach (var color in points.Select(pointSingle => GetPixel(image, pointSingle)))
            {
                redSum += color.R;
                greenSum += color.G;
                blueSum += color.B;
            }

            return Color.FromArgb(redSum / points.Count, greenSum / points.Count, blueSum / points.Count);
        }

        /// <summary>
        ///     Sets the pixel.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="point">The point.</param>
        /// <param name="color">The color.</param>
        /// <returns>
        ///     The changed image as Bitmap
        /// </returns>
        /// <exception cref="ArgumentNullException">nameof(image)</exception>
        internal static Bitmap SetPixel(Bitmap image, Point point, Color color)
        {
            if (image == null)
            {
                var innerException =
                    new ArgumentNullException(string.Concat(nameof(SetPixel), ImagingResources.Spacing, nameof(image)));
                throw new ArgumentNullException(ImagingResources.ErrorWrongParameters, innerException);
            }

            //use our new Format
            var dbm = DirectBitmap.GetInstance(image);
            dbm.SetPixel(point.X, point.Y, color);

            return dbm.Bitmap;
        }

        /// <summary>
        ///     Sets the pixel.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="point">The point.</param>
        /// <param name="color">The color.</param>
        /// <param name="radius">The radius.</param>
        /// <returns>The Changed Image</returns>
        /// <exception cref="ArgumentNullException">nameof(image)</exception>
        internal static Bitmap SetPixel(Bitmap image, Point point, Color color, int radius)
        {
            if (image == null)
            {
                var innerException =
                    new ArgumentNullException(string.Concat(nameof(SetPixel), ImagingResources.Spacing, nameof(image)));
                throw new ArgumentNullException(ImagingResources.ErrorWrongParameters, innerException);
            }

            var points = ImageHelper.GetCirclePoints(point, radius, image.Height, image.Width);

            return points.Aggregate(image, (current, pointSingle) => SetPixel(current, pointSingle, color));
        }

        /// <summary>
        ///     Validates the file path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <exception cref="System.IO.IOException"></exception>
        private static void ValidateFilePath(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                var innerException = path != null
                    ? new IOException(string.Concat(nameof(path), ImagingResources.Spacing, path))
                    : new IOException(nameof(path));
                throw new IOException(ImagingResources.ErrorMissingFile, innerException);
            }
        }
    }
}
