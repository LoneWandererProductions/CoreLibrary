using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace RenderEngine
{
    public static class Tester
    {
        public static WriteableBitmap ProcessWithSkiaSharp(string imagePath)
        {
            // Load the image using SkiaSharp
            using SKBitmap bitmap = SKBitmap.Decode(imagePath);
            // Access the raw pixel data as a byte array (ARGB format)
            byte[] pixelData = bitmap.Bytes;

            // Loop through the pixels and apply the grayscale conversion
            for (int i = 0; i < pixelData.Length; i += 4) // Skip 4 bytes per pixel (ARGB)
            {
                byte blue = pixelData[i];
                byte green = pixelData[i + 1];
                byte red = pixelData[i + 2];

                // Calculate grayscale value (luminosity formula)
                byte grayValue = (byte)(red * 0.3 + green * 0.59 + blue * 0.11);

                // Set new grayscale value for all channels
                pixelData[i] = grayValue; // Blue channel
                pixelData[i + 1] = grayValue; // Green channel
                pixelData[i + 2] = grayValue; // Red channel
                // Alpha channel remains unchanged
            }

            // Create a WriteableBitmap from the processed pixel data
            WriteableBitmap writeableBitmap = new WriteableBitmap(bitmap.Width, bitmap.Height, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);

            // Write the pixel data to the WriteableBitmap
            writeableBitmap.WritePixels(new System.Windows.Int32Rect(0, 0, bitmap.Width, bitmap.Height), pixelData, bitmap.Width * 4, 0);

            return writeableBitmap;
        }

        public static WriteableBitmap Render()
        {
            // Create a bitmap
            SKBitmap bitmap = new SKBitmap(800, 600);

            // Create a canvas to draw on the bitmap
            SKCanvas canvas = new SKCanvas(bitmap);

            // Draw something or manipulate the bitmap
            canvas.Clear(SKColors.White);

            // Convert to .NET Bitmap
            return bitmap.ToWriteableBitmap();
        }
    }
}
