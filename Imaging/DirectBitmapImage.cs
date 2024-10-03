/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/DirectBitmap.cs
 * PURPOSE:     Custom BitmapImage Class, speeds up Set Pixel
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Imaging
{
    /// <summary>
    ///     Similar to DirectBitmap generate a pixel Image, should be slightly faster
    /// </summary>
    public class DirectBitmapImage
    {
        /// <summary>
        ///     The bitmap
        /// </summary>
        private readonly WriteableBitmap _bitmap;

        /// <summary>
        ///     The height
        /// </summary>
        private readonly int _height;

        /// <summary>
        ///     The width
        /// </summary>
        private readonly int _width;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectBitmapImage" /> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public DirectBitmapImage(int width, int height)
        {
            _width = width;
            _height = height;

            // Initialize the WriteableBitmap
            _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Imaging.DirectBitmapImage" /> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="pixels">The pixels.</param>
        public DirectBitmapImage(int width, int height, IEnumerable<PixelData> pixels) : this(width, height)
        {
            SetPixels(pixels);
        }

        /// <summary>
        ///     Sets the pixels.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        public void SetPixels(IEnumerable<PixelData> pixels)
        {
            _bitmap.Lock(); // Lock the bitmap for writing

            unsafe
            {
                // Get a pointer to the back buffer
                var dataPointer = (byte*)_bitmap.BackBuffer.ToPointer();

                foreach (var pixel in pixels)
                {
                    // Calculate the index in the back buffer
                    var pixelIndex = ((pixel.Y * _width) + pixel.X) * 4; // 4 bytes per pixel (BGRA)

                    // Set the pixel data
                    dataPointer[pixelIndex + 0] = pixel.B; // Blue
                    dataPointer[pixelIndex + 1] = pixel.G; // Green
                    dataPointer[pixelIndex + 2] = pixel.R; // Red
                    dataPointer[pixelIndex + 3] = pixel.A; // Alpha
                }
            }

            // Mark the area of the bitmap that was changed
            _bitmap.AddDirtyRect(new Int32Rect(0, 0, _width, _height));
            _bitmap.Unlock(); // Unlock the bitmap after writing
        }

        /// <summary>
        ///     Gets the bitmap image.
        /// </summary>
        /// <returns>A Converted BitmapImage</returns>
        public BitmapImage GetBitmapImage()
        {
            // Convert WriteableBitmap to BitmapImage
            var bitmapImage = new BitmapImage();

            using var memoryStream = new MemoryStream();
            // Encode WriteableBitmap as a PNG to the memory stream
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(_bitmap));
            encoder.Save(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);

            // Load the BitmapImage from the memory stream
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Freeze the BitmapImage to make it immutable and thread-safe

            return bitmapImage;
        }
    }
}
