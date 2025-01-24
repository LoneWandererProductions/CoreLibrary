/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        RenderEngine/TKHelper.cs
 * PURPOSE:     Basic Helper stuff for our engine
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace RenderEngine
{
    internal static class TkHelper
    {
        internal static float[] GenerateVertexData<T>(T[] data, int screenWidth, int screenHeight, Func<T, float[]> getVertexAttributes)
        {
            var vertexData = new float[data.Length * 6 * 5]; // 6 vertices * 5 attributes (x, y, r, g, b)

            for (var i = 0; i < data.Length; i++)
            {
                var attributes = getVertexAttributes(data[i]);
                var xLeft = (i / (float)screenWidth * 2.0f) - 1.0f;
                var xRight = ((i + 1) / (float)screenWidth * 2.0f) - 1.0f;

                var columnHeight = attributes[0]; // In case of columns, this would be height, for example
                var yTop = (columnHeight / (float)screenHeight) * 2.0f - 1.0f;  // Use columnHeight to set the top Y position
                var yBottom = -1.0f; // Bottom of the screen is -1.0f

                var offset = i * 30; // 6 vertices * 5 attributes (x, y, r, g, b)

                // Set up vertices for the column or pixel
                vertexData[offset + 0] = xLeft;
                vertexData[offset + 1] = yTop;
                vertexData[offset + 2] = attributes[1]; // r
                vertexData[offset + 3] = attributes[2]; // g
                vertexData[offset + 4] = attributes[3]; // b

                vertexData[offset + 5] = xRight;
                vertexData[offset + 6] = yTop;
                vertexData[offset + 7] = attributes[1];
                vertexData[offset + 8] = attributes[2];
                vertexData[offset + 9] = attributes[3];

                vertexData[offset + 10] = xRight;
                vertexData[offset + 11] = yBottom;
                vertexData[offset + 12] = attributes[1];
                vertexData[offset + 13] = attributes[2];
                vertexData[offset + 14] = attributes[3];

                vertexData[offset + 15] = xLeft;
                vertexData[offset + 16] = yTop;
                vertexData[offset + 17] = attributes[1];
                vertexData[offset + 18] = attributes[2];
                vertexData[offset + 19] = attributes[3];

                vertexData[offset + 20] = xRight;
                vertexData[offset + 21] = yBottom;
                vertexData[offset + 22] = attributes[1];
                vertexData[offset + 23] = attributes[2];
                vertexData[offset + 24] = attributes[3];

                vertexData[offset + 25] = xLeft;
                vertexData[offset + 26] = yBottom;
                vertexData[offset + 27] = attributes[1];
                vertexData[offset + 28] = attributes[2];
                vertexData[offset + 29] = attributes[3];
            }

            return vertexData;
        }

        public static byte[] LoadTexture(string filePath, out int width, out int height)
        {
            // Load the image using System.Drawing.Bitmap
            using var bitmap = new Bitmap(filePath);

            // Ensure the image format is consistent with OpenGL requirements
            width = bitmap.Width;
            height = bitmap.Height;

            // Lock the bitmap data
            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Copy the image data into a byte array
            int dataSize = Math.Abs(bitmapData.Stride) * bitmapData.Height;
            byte[] pixelData = new byte[dataSize];
            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, pixelData, 0, dataSize);

            // Unlock the bitmap data
            bitmap.UnlockBits(bitmapData);

            return pixelData;
        }

        public static int LoadTexture(string filePath)
        {
            // Load the image file
            var textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            using (var bitmap = new Bitmap(filePath))
            {
                // Convert the bitmap to a byte array (RGBA)
                var pixels = new byte[bitmap.Width * bitmap.Height * 4]; // 4 for RGBA channels
                var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var data = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
                bitmap.UnlockBits(data);

                // Convert the byte array to IntPtr (needed for GL.TexImage2D)
                IntPtr pointer = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);

                // Load the image into OpenGL texture
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pointer);
            }

            // Set texture parameters (filtering, wrapping)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return textureId;
        }
    }
}
