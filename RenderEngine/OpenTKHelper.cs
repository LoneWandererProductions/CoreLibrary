/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        RenderEngine/TKHelper.cs
 * PURPOSE:     Basic Helper utilities for OpenTK/OpenGL
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using OpenTK.Graphics.OpenGL4;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace RenderEngine
{
    /// <summary>
    /// Exposes OpenTK/OpenGL helper methods in a neat static class.
    /// Provides utilities for checking OpenGL compatibility, shader compilation,
    /// texture creation and loading, and cube map handling.
    /// </summary>
    public static class OpenTkHelper
    {
        private static Version? _glVersion;

        /// <summary>
        /// Compiles a GLSL shader from source code.
        /// </summary>
        /// <param name="type">Shader type (vertex, fragment, etc.).</param>
        /// <param name="source">GLSL shader source code.</param>
        /// <returns>OpenGL shader ID.</returns>
        /// <exception cref="Exception">Thrown if compilation fails.</exception>
        internal static int CompileShader(ShaderType type, string source)
        {
            var shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var status);
            if (status == (int)All.True)
                return shader;

            var infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"Error compiling shader of type {type}: {infoLog}");
        }

        /// <summary>
        /// Loads and links a vertex + fragment shader program from files.
        /// </summary>
        /// <param name="vertexPath">Path to vertex shader.</param>
        /// <param name="fragmentPath">Path to fragment shader.</param>
        /// <returns>OpenGL shader program ID.</returns>
        /// <exception cref="Exception">Thrown if compilation or linking fails.</exception>
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
        /// Generates mipmaps for better scaling.
        /// </summary>
        /// <param name="image">Image buffer with raw pixel data.</param>
        /// <param name="opaqueFastPath">If true, assumes opaque RGB only for faster upload.</param>
        /// <returns>OpenGL texture ID.</returns>
        public static int CreateTexture(UnmanagedImageBuffer image, bool opaqueFastPath = false)
        {
            var texId = GL.GenTexture();
            try
            {
                GL.BindTexture(TextureTarget.Texture2D, texId);

                var internalFormat = opaqueFastPath ? PixelInternalFormat.Rgb : PixelInternalFormat.Rgba;
                var format = opaqueFastPath ? PixelFormat.Rgb : PixelFormat.Bgra;

                GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat,
                    image.Width, image.Height, 0, format, PixelType.UnsignedByte, image.Buffer);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                    (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                    (int)TextureWrapMode.Repeat);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                return texId;
            }
            catch
            {
                // Cleanup if something fails
                if (texId != 0 && GL.IsTexture(texId))
                    GL.DeleteTexture(texId);

                throw; // rethrow original exception
            }
            finally
            {
                GL.BindTexture(TextureTarget.Texture2D, 0); // always unbind
            }
        }

        /// <summary>
        /// Loads a 2D texture from a file path into OpenGL.
        /// Generates mipmaps for better scaling.
        /// </summary>
        /// <param name="filePath">Path to texture image.</param>
        /// <returns>OpenGL texture ID.</returns>
        /// <exception cref="FileNotFoundException">Thrown if file missing.</exception>
        internal static int LoadTextureFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Trace.WriteLine($"File not found: {filePath}");
                throw new FileNotFoundException($"Texture file not found: {filePath}");
            }

            using var bitmap = new Bitmap(filePath);

            // Fix for OpenGL's bottom-left origin requirement
            bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            var texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texId);

            try
            {
                var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                try
                {
                    // Zero-allocation upload directly from the unmanaged memory pointer
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width,
                        bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                }
                finally
                {
                    bitmap.UnlockBits(bmpData);
                }

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                    (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                    (int)TextureWrapMode.Repeat);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                return texId;
            }
            catch
            {
                // Cleanup if something goes wrong during upload
                if (texId != 0 && GL.IsTexture(texId))
                    GL.DeleteTexture(texId);
                throw;
            }
            finally
            {
                GL.BindTexture(TextureTarget.Texture2D, 0); // Always unbind
            }
        }

        /// <summary>
        /// Loads a cubemap texture from six image files.
        /// </summary>
        /// <param name="filePaths">Array of six image file paths in cubemap order.</param>
        /// <returns>OpenGL cubemap texture ID.</returns>
        /// <exception cref="ArgumentException">Thrown if array length is not 6.</exception>
        internal static int LoadCubeMap(string[] filePaths)
        {
            if (filePaths.Length != 6)
                throw new ArgumentException("Cube map must have exactly 6 textures.", nameof(filePaths));

            var textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, textureId);

            try
            {
                for (var i = 0; i < 6; i++)
                {
                    if (!File.Exists(filePaths[i]))
                        throw new FileNotFoundException($"Cubemap texture not found: {filePaths[i]}");

                    using var bitmap = new Bitmap(filePaths[i]);

                    var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    var bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    try
                    {
                        // Zero-allocation upload
                        GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelInternalFormat.Rgba,
                            bitmap.Width, bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                    }
                    finally
                    {
                        bitmap.UnlockBits(bmpData);
                    }
                }

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
                    (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter,
                    (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS,
                    (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT,
                    (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR,
                    (int)TextureWrapMode.ClampToEdge);

                GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);

                return textureId;
            }
            catch
            {
                if (textureId != 0 && GL.IsTexture(textureId))
                    GL.DeleteTexture(textureId);
                throw;
            }
            finally
            {
                GL.BindTexture(TextureTarget.TextureCubeMap, 0); // Always unbind
            }
        }

        /// <summary>
        /// Checks if the system supports at least the given OpenGL version.
        /// Caches the result to avoid repeated GLContext queries.
        /// </summary>
        internal static bool IsOpenGlCompatible(int requiredMajor = 4, int requiredMinor = 5)
        {
            if (_glVersion == null)
            {
                try
                {
                    // Use GetInteger instead of GetString to avoid GPU vendor string formatting issues
                    var major = GL.GetInteger(GetPName.MajorVersion);
                    var minor = GL.GetInteger(GetPName.MinorVersion);
                    _glVersion = new Version(major, minor);
                }
                catch
                {
                    _glVersion = new Version(0, 0);
                }
            }

            return _glVersion >= new Version(requiredMajor, requiredMinor);
        }
    }
}
