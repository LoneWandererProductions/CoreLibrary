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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Imaging
{
    /// <inheritdoc />
    /// <summary>
    ///     Similar to DirectBitmap, generate a pixel image, should be slightly faster
    /// </summary>
    public sealed class DirectBitmapImage : IDisposable
    {
        /// <summary>
        ///     The bitmap
        /// </summary>
        private readonly WriteableBitmap _bitmap;

        /// <summary>
        ///     GCHandle to manage the memory of the bits array
        /// </summary>
        private readonly GCHandle _bitsHandle;

        /// <summary>
        ///     The height
        /// </summary>
        private readonly int _height;

        /// <summary>
        ///     The width
        /// </summary>
        private readonly int _width;

        /// <summary>
        ///     Indicates if the instance has been disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectBitmapImage" /> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public DirectBitmapImage(int width, int height)
        {
            _width = width;
            _height = height;

            // Initialize the Bits array and pin it for direct access
            Bits = new uint[width * height];
            _bitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);

            // Initialize WriteableBitmap
            _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
        }

        /// <summary>
        ///     Gets the bitmap Image.
        /// </summary>
        /// <value>
        ///     The bitmap Image.
        /// </value>
        public BitmapImage BitmapImage => ConvertImage();

        /// <summary>
        ///     Gets the bits.
        /// </summary>
        public uint[] Bits { get; }


        /// <inheritdoc />
        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Sets the pixels from an enumerable source of pixel data.
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

                    // Set the pixel data in the back buffer
                    dataPointer[pixelIndex + 0] = pixel.B; // Blue
                    dataPointer[pixelIndex + 1] = pixel.G; // Green
                    dataPointer[pixelIndex + 2] = pixel.R; // Red
                    dataPointer[pixelIndex + 3] = pixel.A; // Alpha

                    // Store the pixel data as ARGB in the Bits array
                    Bits[(pixel.Y * _width) + pixel.X] = Bits[(pixel.Y * _width) + pixel.X] =
                        (uint)((pixel.A << 24) | (pixel.R << 16) | (pixel.G << 8) | pixel.B);
                    ; // This will be fine as long as A, R, G, B are 0-255
                }
            }

            // Mark the area of the bitmap that was changed
            _bitmap.AddDirtyRect(new Int32Rect(0, 0, _width, _height));
            _bitmap.Unlock(); // Unlock the bitmap after writing
        }

        /// <summary>
        ///     Converts the image.
        /// </summary>
        /// <returns>The BitmapImage from our WriteableBitmap.</returns>
        private BitmapImage ConvertImage()
        {
            // Create a new WriteableBitmap
            var bitmap = new WriteableBitmap(_width, _height, 96, 96, PixelFormats.Bgra32, null);

            // Create a byte array to hold the byte representation of the Bits
            var byteArray = new byte[Bits.Length * sizeof(uint)];

            // Fill the byte array with data from the Bits array
            Buffer.BlockCopy(Bits, 0, byteArray, 0, byteArray.Length);

            // Copy the byte array to the WriteableBitmap's back buffer
            bitmap.Lock();
            Marshal.Copy(byteArray, 0, bitmap.BackBuffer, byteArray.Length);
            bitmap.AddDirtyRect(new Int32Rect(0, 0, _width, _height));
            bitmap.Unlock();

            // Create a new BitmapImage from the WriteableBitmap
            var bitmapImage = new BitmapImage();
            using var memoryStream = new MemoryStream();
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

            return bitmapImage;
        }

        /// <summary>
        ///     Applies the color matrix.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        public void ApplyColorMatrix(float[][] matrix)
        {
            // Initialize transformedColors to hold the transformed pixel colors
            var transformedColors = new (int x, int y, Color color)[Bits.Length];

            // Loop through the Bits array and apply the color matrix
            for (var i = 0; i < Bits.Length; i++)
            {
                var color = Bits[i];

                // Extract ARGB values from the color
                var converter = new ColorHsv((int)color);
                Debug.WriteLine(
                    $"Original color for pixel {i}: ARGB({converter.A}, {converter.R}, {converter.G}, {converter.B})");

                // Create a vector for the color (ARGB)
                var colorVector = new float[4] { converter.R, converter.G, converter.B, converter.A };

                // Initialize a new result array for transformed colors
                var result = new float[4];

                // Perform the matrix multiplication
                result[0] = (matrix[0][0] * colorVector[0]) + (matrix[0][1] * colorVector[1]) +
                            (matrix[0][2] * colorVector[2]) + (matrix[0][3] * colorVector[3]);
                result[1] = (matrix[1][0] * colorVector[0]) + (matrix[1][1] * colorVector[1]) +
                            (matrix[1][2] * colorVector[2]) + (matrix[1][3] * colorVector[3]);
                result[2] = (matrix[2][0] * colorVector[0]) + (matrix[2][1] * colorVector[1]) +
                            (matrix[2][2] * colorVector[2]) + (matrix[2][3] * colorVector[3]);
                result[3] = (matrix[3][0] * colorVector[0]) + (matrix[3][1] * colorVector[1]) +
                            (matrix[3][2] * colorVector[2]) + (matrix[3][3] * colorVector[3]);

                // Log transformed color before adding bias
                Debug.WriteLine(
                    $"Transformed color for pixel {i} (before bias): R={result[0]}, G={result[1]}, B={result[2]}, A={result[3]}");

                // Add the bias from the last row of the matrix
                result[0] += matrix[4][0]; // Bias for R
                result[1] += matrix[4][1]; // Bias for G
                result[2] += matrix[4][2]; // Bias for B
                result[3] += matrix[4][3]; // Bias for A

                // Log color after bias addition
                Debug.WriteLine($"After adding bias: R={result[0]}, G={result[1]}, B={result[2]}, A={result[3]}");

                // Clamp the values to [0, 255]
                result[0] = Math.Clamp(result[0], 0, 255);
                result[1] = Math.Clamp(result[1], 0, 255);
                result[2] = Math.Clamp(result[2], 0, 255);
                result[3] = Math.Clamp(result[3], 0, 255);

                // Log clamped values
                Debug.WriteLine($"Clamped values: R={result[0]}, G={result[1]}, B={result[2]}, A={result[3]}");

                // Create new color
                var newColor = new Color
                {
                    A = (byte)result[3], R = (byte)result[0], G = (byte)result[1], B = (byte)result[2]
                };

                // Calculate the x and y positions
                var x = i % _width;
                var y = i / _width;

                // Store the transformed pixel color
                transformedColors[i] = (x, y, newColor);
                Debug.WriteLine(
                    $"Transformed pixel position ({x}, {y}) with new color: ARGB({newColor.A}, {newColor.R}, {newColor.G}, {newColor.B})");
            }

            // Use SetPixelsSimd to apply the transformed pixel colors
            SetPixelsSimd(transformedColors.Where(p => p.color.A != 0)); // Filter out default colors
            Debug.WriteLine("Applied transformed colors to pixels.");
        }

        /// <summary>
        ///     Sets the pixels simd.
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
                        Bits[indices[j]] = (uint)colors[j];
                    }
                }
            }
        }

        /// <summary>
        ///     Updates the bitmap from the Bits array.
        /// </summary>
        public void UpdateBitmapFromBits()
        {
            _bitmap.Lock();

            // Create a byte array to hold the byte representation of the Bits
            var byteArray = new byte[Bits.Length * sizeof(uint)];

            // Fill the byte array with data from the Bits array
            Buffer.BlockCopy(Bits, 0, byteArray, 0, byteArray.Length);

            // Copy the byte array to the WriteableBitmap's back buffer
            Marshal.Copy(byteArray, 0, _bitmap.BackBuffer, byteArray.Length);
            _bitmap.AddDirtyRect(new Int32Rect(0, 0, _width, _height));
            _bitmap.Unlock();
        }

        /// <summary>
        ///     Releases unmanaged and managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
        ///     unmanaged resources.
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
        ///     Finalizes the instance.
        /// </summary>
        ~DirectBitmapImage()
        {
            Dispose(false);
        }
    }
}
