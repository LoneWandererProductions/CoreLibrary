/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        RenderEngine/TKHelper.cs
 * PURPOSE:     Basic Helper stuff for our engine
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace RenderEngine;

/// <summary>
/// Exposes OpenTK / OpenGL helper methods in a neat static class.
/// Provides utilities for checking OpenGL compatibility, shader compilation,
/// texture creation and loading, and cube map handling.
/// </summary>
public static class OpenTkHelper
{
    /// <summary>
    /// Checks if the system supports at least the given OpenGL version.
    /// </summary>
    /// <param name="requiredMajor">The required major version (default: 4).</param>
    /// <param name="requiredMinor">The required minor version (default: 5).</param>
    /// <returns><c>true</c> if the current system is compatible, otherwise <c>false</c>.</returns>
    internal static bool IsOpenGlCompatible(int requiredMajor = 4, int requiredMinor = 5)
    {
        var isCompatible = false;

        Application.Current.Dispatcher.Invoke(() =>
        {
            try
            {
                using var tempContext = new GLControl();
                tempContext.MakeCurrent();

                var versionString = GL.GetString(StringName.Version);
                var renderer = GL.GetString(StringName.Renderer);
                var vendor = GL.GetString(StringName.Vendor);

                Trace.WriteLine($"OpenGL Renderer: {renderer}");
                Trace.WriteLine($"OpenGL Vendor: {vendor}");
                Trace.WriteLine($"OpenGL Version: {versionString}");

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

    /// <summary>
    /// Compiles a GLSL shader from source code.
    /// </summary>
    /// <param name="type">The type of shader (vertex, fragment, etc.).</param>
    /// <param name="source">The GLSL shader source code.</param>
    /// <returns>The OpenGL shader ID.</returns>
    /// <exception cref="Exception">Thrown if compilation fails with error log.</exception>
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

    /// <summary>
    /// Loads and links a vertex + fragment shader program from files.
    /// </summary>
    /// <param name="vertexPath">Path to the vertex shader file.</param>
    /// <param name="fragmentPath">Path to the fragment shader file.</param>
    /// <returns>The OpenGL program ID.</returns>
    /// <exception cref="Exception">Thrown if linking fails with error log.</exception>
    public static int LoadShader(string vertexPath, string fragmentPath)
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

    /// <summary>
    /// Creates a 2D OpenGL texture from an <see cref="UnmanagedImageBuffer"/>.
    /// </summary>
    /// <param name="image">The image buffer containing raw pixel data.</param>
    /// <param name="opaqueFastPath">If true, assumes opaque (RGB only) for performance.</param>
    /// <returns>OpenGL texture ID.</returns>
    public static int CreateTexture(UnmanagedImageBuffer image, bool opaqueFastPath = false)
    {
        var texId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, texId);

        var internalFormat = opaqueFastPath ? PixelInternalFormat.Rgb : PixelInternalFormat.Rgba;
        var format = opaqueFastPath ? PixelFormat.Rgb : PixelFormat.Bgra;

        GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, image.Width, image.Height,
            0, format, PixelType.UnsignedByte, image.Buffer);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        return texId;
    }

    /// <summary>
    /// Loads a cubemap texture from 6 image files.
    /// </summary>
    /// <param name="filePaths">Array of 6 image file paths in cubemap order.</param>
    /// <returns>OpenGL cubemap texture ID.</returns>
    /// <exception cref="ArgumentException">Thrown if the array does not contain exactly 6 paths.</exception>
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

            using var bitmap = new Bitmap(filePaths[i]);
            var pixels = GetBitmapBytes(bitmap);
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba,
                bitmap.Width, bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
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

    /// <summary>
    /// Loads a texture from a file path into a 2D OpenGL texture.
    /// </summary>
    /// <param name="filePath">The file path of the texture image.</param>
    /// <returns>OpenGL texture ID or -1 if file not found.</returns>
    internal static int LoadTextureFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Trace.WriteLine($"File not found: {filePath}");
            return -1;
        }

        var textureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureId);

        using (var bitmap = new Bitmap(filePath))
        {
            var pixels = GetBitmapBytes(bitmap);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width,
                bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);
        }

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        return textureId;
    }

    /// <summary>
    /// Converts a <see cref="Bitmap"/> into a tightly packed byte array (BGRA).
    /// </summary>
    /// <param name="bitmap">The bitmap to process.</param>
    /// <returns>Byte array containing image data in BGRA order.</returns>
    private static byte[] GetBitmapBytes(Bitmap bitmap)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var rect = new Rectangle(0, 0, width, height);
        var bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        try
        {
            var stride = bmpData.Stride;
            const int bytesPerPixel = 4;
            var rawData = new byte[stride * height];
            Marshal.Copy(bmpData.Scan0, rawData, 0, rawData.Length);

            var pixels = new byte[width * height * bytesPerPixel];

            for (var y = 0; y < height; y++)
            {
                var srcRow = y * stride;
                var dstRow = y * width * bytesPerPixel;

                for (var x = 0; x < width; x++)
                {
                    var srcIndex = srcRow + (x * bytesPerPixel);
                    var dstIndex = dstRow + (x * bytesPerPixel);

                    pixels[dstIndex + 0] = rawData[srcIndex + 0];
                    pixels[dstIndex + 1] = rawData[srcIndex + 1];
                    pixels[dstIndex + 2] = rawData[srcIndex + 2];
                    pixels[dstIndex + 3] = rawData[srcIndex + 3];
                }
            }

            return pixels;
        }
        finally
        {
            bitmap.UnlockBits(bmpData);
        }
    }
}
