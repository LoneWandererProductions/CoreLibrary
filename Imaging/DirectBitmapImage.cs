/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/DirectBitmap.cs
 * PURPOSE:     Custom BitmapImage Class, speeds up Set Pixel
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Imaging
{
    /// <summary>
    ///     Similar to DirectBitmap, generate a pixel image, should be slightly faster
    /// </summary>
    public class DirectBitmapImage : IDisposable
    {
        /// <summary>
        /// Gets the bitmap Image.
        /// </summary>
        /// <value>
        /// The bitmap Image.
        /// </value>
        public BitmapImage BitmapImage => ConvertImage();

        /// <summary>
        /// The bitmap
        /// </summary>
        private readonly WriteableBitmap _bitmap;

        /// <summary>
        /// The height
        /// </summary>
        private readonly int _height;

        /// <summary>
        /// The width
        /// </summary>
        private readonly int _width;

        /// <summary>
        /// Gets the bits.
        /// </summary>
        public int[] Bits { get; }

        /// <summary>
        /// GCHandle to manage the memory of the bits array
        /// </summary>
        private readonly GCHandle _bitsHandle;

        /// <summary>
        /// Indicates if the instance has been disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectBitmapImage" /> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public DirectBitmapImage(int width, int height)
        {
            _width = width;
            _height = height;

            // Initialize the Bits array and pin it for direct access
            Bits = new int[width * height];
            _bitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);

            // Initialize WriteableBitmap
            _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        }

        /// <summary>
        /// Sets the pixels from an enumerable source of pixel data.
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
                    // Validate pixel bounds
                    if (pixel.X < 0 || pixel.X >= _width || pixel.Y < 0 || pixel.Y >= _height)
                    {
                        continue; // Skip invalid pixels
                    }

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
        /// Converts the image.
        /// </summary>
        /// <returns>The BitmapImage from our WriteableBitmap.</returns>
        private BitmapImage ConvertImage()
        {
            // Create a new WriteableBitmap
            var bitmap = new WriteableBitmap(_width, _height, 96, 96, PixelFormats.Bgra32, null);

            // Copy the bits directly into the WriteableBitmap's back buffer
            bitmap.Lock();
            Marshal.Copy(Bits, 0, bitmap.BackBuffer, Bits.Length);
            bitmap.AddDirtyRect(new Int32Rect(0, 0, _width, _height));
            bitmap.Unlock();

            // Create a new BitmapImage from the WriteableBitmap
            var bitmapImage = new BitmapImage();
            using (var memoryStream = new MemoryStream())
            {
                // Encode the WriteableBitmap as a PNG and write it to a MemoryStream
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(memoryStream);

                // Reset the stream's position to the beginning
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Load the BitmapImage from the MemoryStream
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Make it immutable and thread-safe
            }

            return bitmapImage;
        }

        /// <summary>
        /// Applies the color matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        public void ApplyColorMatrix(float[][] matrix)
        {
            var transformedColors = new (int x, int y, Color color)[Bits.Length];

            // Loop through the Bits array and apply the color matrix
            for (var i = 0; i < Bits.Length; i++)
            {
                var color = Bits[i];

                // Extract ARGB values from the color
                var converter = new ColorHsv(color);

                // Apply the color matrix with precomputed values
                float newR = 0, newG = 0, newB = 0, newA = 0;

                for (int j = 0; j < 4; j++)
                {
                    newR += converter.R * matrix[0][j];
                    newG += converter.G * matrix[1][j];
                    newB += converter.B * matrix[2][j];
                    newA += converter.A * matrix[3][j];
                }

                // Add the bias
                newR += matrix[4][0];
                newG += matrix[4][1];
                newB += matrix[4][2];
                newA += matrix[4][3];

                // Clamp the values and create new color
                var newColor = new Color()
                {
                    A = (byte)ImageHelper.Clamp(newA),
                    R = (byte)ImageHelper.Clamp(newR),
                    G = (byte)ImageHelper.Clamp(newG),
                    B = (byte)ImageHelper.Clamp(newB)
                };

                // Calculate the x and y positions
                var x = i % _width;
                var y = i / _width;

                // Store the transformed pixel color
                transformedColors[i] = (x, y, newColor);
            }

            // Use SetPixelsSimd to apply the transformed pixel colors
            SetPixelsSimd(transformedColors.Where(p => p.color.A != 0)); // Filter out default colors
        }

        /// <summary>
        /// Sets the pixels simd.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <exception cref="InvalidOperationException">ImagingResources.ErrorInvalidOperation</exception>
        public void SetPixelsSimd(IEnumerable<(int x, int y, Color color)> pixels)
        {
            var pixelArray = pixels.ToArray();
            var vectorCount = Vector<int>.Count;

            // Ensure Bits array is properly initialized
            if (Bits == null || Bits.Length < _width * _height)
            {
                throw new InvalidOperationException(ImagingResources.ErrorInvalidOperation);
            }

            for (var i = 0; i < pixelArray.Length; i += vectorCount)
            {
                var indices = new int[vectorCount];
                var colors = new int[vectorCount];

                // Load data into vectors
                for (var j = 0; j < vectorCount; j++)
                {
                    if (i + j < pixelArray.Length)
                    {
                        var (x, y, color) = pixelArray[i + j];
                        indices[j] = x + (y * _width);
                        colors[j] = (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
                    }
                    else
                    {
                        // Handle cases where the remaining elements are less than vectorCount
                        indices[j] = 0;
                        colors[j] = 0; // Use a default color or handle it as needed
                    }
                }

                // Write data to Bits array
                for (var j = 0; j < vectorCount; j++)
                {
                    if (i + j < pixelArray.Length)
                    {
                        Bits[indices[j]] = colors[j];
                    }
                }
            }
        }

        /// <summary>
        /// Updates the bitmap from the Bits array.
        /// </summary>
        public void UpdateBitmapFromBits()
        {
            _bitmap.Lock();
            Marshal.Copy(Bits, 0, _bitmap.BackBuffer, Bits.Length);
            _bitmap.AddDirtyRect(new Int32Rect(0, 0, _width, _height));
            _bitmap.Unlock();
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        /// unmanaged resources.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Free the GCHandle if allocated
                if (_bitsHandle.IsAllocated)
                {
                    _bitsHandle.Free();
                }
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizes the instance.
        /// </summary>
        ~DirectBitmapImage()
        {
            Dispose(false);
        }
    }
}
