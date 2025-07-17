/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        RenderEngine/TKHelper.cs
 * PURPOSE:     Basic Helper stuff for our engine
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;

namespace RenderEngine
{
    internal static class OpenTkHelper
    {
        internal static bool IsOpenGlCompatible(int requiredMajor = 4, int requiredMinor = 5)
        {
            var isCompatible = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    using var tempContext = new GLControl();
                    tempContext.MakeCurrent(); // Set OpenGL-Context

                    var versionString = GL.GetString(StringName.Version);
                    var renderer = GL.GetString(StringName.Renderer);
                    var vendor = GL.GetString(StringName.Vendor);

                    Console.WriteLine($"OpenGL Renderer: {renderer}");
                    Console.WriteLine($"OpenGL Vendor: {vendor}");
                    Console.WriteLine($"OpenGL Version: {versionString}");

                    var versionParts = versionString.Split('.');
                    if (versionParts.Length < 2)
                    {
                        return;
                    }

                    if (int.TryParse(versionParts[0], out var major) &&
                        int.TryParse(versionParts[1].Split(' ')[0], out var minor))
                    {
                        isCompatible = major > requiredMajor || (major == requiredMajor && minor >= requiredMinor);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"OpenGL initialization failed: {ex.Message}");
                }
            });

            return isCompatible;
        }

        internal static float[] GenerateVertexData<T>(T[] data, int screenWidth, int screenHeight,
            Func<T, float[]> getVertexAttributes)
        {
            var vertexData = new float[data.Length * 6 * 5]; // 6 vertices * 5 attributes (x, y, r, g, b)

            for (var i = 0; i < data.Length; i++)
            {
                var attributes = getVertexAttributes(data[i]);
                var xLeft = (i / (float)screenWidth * 2.0f) - 1.0f;
                var xRight = ((i + 1) / (float)screenWidth * 2.0f) - 1.0f;

                var columnHeight = attributes[0]; // In case of columns, this would be height, for example
                var yTop = (columnHeight / screenHeight * 2.0f) - 1.0f;
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

        internal static int CompileShader(ShaderType type, string source)
        {
            var shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var status);
            if (status == (int)All.True)
            {
                return shader;
            }

            var infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"Error compiling shader of type {type}: {infoLog}");
        }

        internal static int LoadShader(string vertexPath, string fragmentPath)
        {
            var vertexSource = File.ReadAllText(vertexPath);
            var fragmentSource = File.ReadAllText(fragmentPath);

            var vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);
            var fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);

            var shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);

            GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out var status);
            if (status == (int)All.False)
            {
                var infoLog = GL.GetProgramInfoLog(shaderProgram);
                throw new Exception($"Error linking shader program: {infoLog}");
            }

            GL.DetachShader(shaderProgram, vertexShader);
            GL.DetachShader(shaderProgram, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return shaderProgram;
        }

        private static byte[] LoadTexturePixels(string filePath, out int width, out int height)
        {
            using var bitmap = new Bitmap(filePath);
            width = bitmap.Width;
            height = bitmap.Height;

            var rect = new Rectangle(0, 0, width, height);
            var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var length = bitmapData.Stride * height;
            var pixels = new byte[length];
            Marshal.Copy(bitmapData.Scan0, pixels, 0, length);

            bitmap.UnlockBits(bitmapData);

            return pixels;
        }

        internal static int LoadTexture(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Trace.WriteLine($"File not found: {filePath}");
                return -1;
            }

            var textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            int width, height;
            var pixels = LoadTexturePixels(filePath, out width, out height);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0,
                          OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                            (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                            (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                            (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                            (int)TextureWrapMode.Repeat);

            return textureId;
        }

        internal static int LoadCubeMap(string[] filePaths)
        {
            if (filePaths.Length != 6)
            {
                throw new ArgumentException("Cube map must have exactly 6 textures.", nameof(filePaths));
            }

            var textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, textureId);

            for (var i = 0; i < 6; i++)
            {
                if (!File.Exists(filePaths[i]))
                {
                    Trace.WriteLine($"Cube map texture not found: {filePaths[i]}");
                    continue;
                }

                int width, height;
                var pixels = LoadTexturePixels(filePaths[i], out width, out height);

                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba,
                    width, height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR,
                (int)TextureWrapMode.ClampToEdge);

            return textureId;
        }
    }
}
