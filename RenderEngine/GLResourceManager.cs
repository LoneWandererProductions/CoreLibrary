﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        GlResourceManager.cs
 * PURPOSE:     Manager that holds all texture information and shaders on runtime.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;

namespace RenderEngine
{
    public sealed class GlResourceManager : IDisposable
    {
        private readonly Dictionary<string, int> _textureCache = new();
        private readonly Dictionary<ShaderTypeApp, int> _programCache = new();

        // --- Textures ---

        /// <summary>
        /// Gets the texture.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>Id of Texture</returns>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        public int GetTexture(string filePath)
        {
            if (_textureCache.TryGetValue(filePath, out var texId))
                return texId;

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            texId = OpenTkHelper.LoadTextureFromFile(filePath);
            _textureCache[filePath] = texId;
            return texId;
        }

        /// <summary>
        /// Creates the master textures.
        /// </summary>
        /// <param name="textures">The textures.</param>
        /// <returns>Dictionary of Input id and Id of texture</returns>
        /// <exception cref="System.ArgumentNullException">Texture {id} is null.</exception>
        public Dictionary<int, int> CreateMasterTextures(Dictionary<int, UnmanagedImageBuffer?> textures)
        {
            var result = new Dictionary<int, int>();

            foreach (var (id, tex) in textures)
            {
                if (tex == null)
                    throw new ArgumentNullException($"Texture {id} is null.");

                // Create GL texture from Bitmap
                var texId = OpenTkHelper.CreateTexture(tex, opaqueFastPath: false);

                // Store in result dictionary
                result[id] = texId;
            }

            return result;
        }

        /// <summary>
        /// Gets the texture.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="opaqueFastPath">if set to <c>true</c> [opaque fast path].</param>
        /// <returns>Id of Texture</returns>
        public int GetTexture(UnmanagedImageBuffer buffer, bool opaqueFastPath = false)
        {
            return OpenTkHelper.CreateTexture(buffer, opaqueFastPath);
        }

        // --- Shader Programs ---

        /// <summary>
        /// Gets the shader program.
        /// </summary>
        /// <param name="appType">Type of the application.</param>
        /// <returns>Id of Shader</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">appType</exception>
        public int GetShaderProgram(ShaderTypeApp appType)
        {
            if (_programCache.TryGetValue(appType, out var programId))
                return programId;

            string vertexSrc, fragmentSrc;
            switch (appType)
            {
                case ShaderTypeApp.SolidColor:
                    vertexSrc = ShaderResource.SolidColorVertexShader;
                    fragmentSrc = ShaderResource.SolidColorFragmentShader;
                    break;
                case ShaderTypeApp.TexturedQuad:
                    vertexSrc = ShaderResource.TextureMappingVertexShader;
                    fragmentSrc = ShaderResource.TextureMappingFragmentShader;
                    break;
                case ShaderTypeApp.VertexColor:
                    vertexSrc = ShaderResource.VertexColorVertexShader;
                    fragmentSrc = ShaderResource.VertexColorFragmentShader;
                    break;
                case ShaderTypeApp.Wireframe:
                    vertexSrc = ShaderResource.WireframeVertexShader;
                    fragmentSrc = ShaderResource.WireframeFragmentShader;
                    break;
                case ShaderTypeApp.TextureArrayTilemap:
                    vertexSrc = ShaderResource.TextureArrayTilemapVertexShader;
                    fragmentSrc = ShaderResource.TextureArrayTilemapFragmentShader;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(appType));
            }

            programId = CompileAndLinkShader(vertexSrc, fragmentSrc);
            _programCache[appType] = programId;
            return programId;
        }

        /// <summary>
        /// Uses the shader.
        /// </summary>
        /// <param name="appType">Type of the application.</param>
        public void UseShader(ShaderTypeApp appType)
        {
            GL.UseProgram(GetShaderProgram(appType));
        }

        // --- Compile & link ---

        /// <summary>
        /// Compiles the and link shader.
        /// </summary>
        /// <param name="vertexSource">The vertex source.</param>
        /// <param name="fragmentSource">The fragment source.</param>
        /// <returns>Id of Shader</returns>
        /// <exception cref="System.Exception">Shader program link failed: " + info</exception>
        private static int CompileAndLinkShader(string vertexSource, string fragmentSource)
        {
            var vertex = CompileSingleShader(ShaderType.VertexShader, vertexSource);
            var fragment = CompileSingleShader(ShaderType.FragmentShader, fragmentSource);

            var program = GL.CreateProgram();
            GL.AttachShader(program, vertex);
            GL.AttachShader(program, fragment);
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var status);
            if (status != (int)All.True)
            {
                var info = GL.GetProgramInfoLog(program);
                GL.DeleteProgram(program);
                throw new Exception("Shader program link failed: " + info);
            }

            // shaders can be detached and deleted after linking
            GL.DetachShader(program, vertex);
            GL.DetachShader(program, fragment);
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);

            return program;
        }

        /// <summary>
        /// Compiles the single shader.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="source">The source.</param>
        /// <returns>Id of Shader</returns>
        /// <exception cref="System.Exception">Error compiling {type}: {info}</exception>
        private static int CompileSingleShader(ShaderType type, string source)
        {
            var shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var status);

            if (status == (int)All.True) return shader;

            var info = GL.GetShaderInfoLog(shader);
            GL.DeleteShader(shader);
            throw new Exception($"Error compiling {type}: {info}");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            foreach (var texId in _textureCache.Values)
                GL.DeleteTexture(texId);

            foreach (var program in _programCache.Values)
                GL.DeleteProgram(program);

            _textureCache.Clear();
            _programCache.Clear();
        }
    }
}
