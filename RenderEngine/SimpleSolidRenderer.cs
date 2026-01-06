/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        SimpleSolidRenderer.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace RenderEngine
{
    /// <summary>
    /// Minimal solid color quad renderer.
    /// Draws directly in screen-space coordinates.
    /// </summary>
    public sealed class SimpleSolidRenderer : IDisposable
    {
        private int _shaderProgram;
        private int _vao;
        private int _vbo;
        private int _uColor;
        private int _uResolution;

        private int _width;
        private int _height;

        private readonly string _vertexSource = @"
#version 410 core
layout (location = 0) in vec2 aPosition;

uniform vec2 uResolution;

void main()
{
    vec2 pos = aPosition / uResolution * 2.0 - 1.0;
    pos.y = -pos.y; // flip Y for top-left origin
    gl_Position = vec4(pos, 0.0, 1.0);
}
";

        private readonly string _fragmentSource = @"
#version 410 core
out vec4 FragColor;
uniform vec4 uColor;

void main()
{
    FragColor = uColor;
}
";

        public void Initialize(int width, int height)
        {
            _width = width;
            _height = height;

            _shaderProgram = CreateShader(_vertexSource, _fragmentSource);
            _uColor = GL.GetUniformLocation(_shaderProgram, "uColor");
            _uResolution = GL.GetUniformLocation(_shaderProgram, "uResolution");

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);

            GL.UseProgram(_shaderProgram);
            GL.Uniform2(_uResolution, new Vector2(width, height));
            GL.UseProgram(0);
        }

        public void DrawSolidQuad(Point p0, Point p1, Point p2, Point p3, Color fill)
        {
            if (_shaderProgram == 0)
                return;

            float[] vertices = { p0.X, p0.Y, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y };

            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices,
                BufferUsageHint.StreamDraw);

            GL.Uniform4(_uColor,
                fill.R / 255f,
                fill.G / 255f,
                fill.B / 255f,
                fill.A / 255f);

            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        private static int CreateShader(string vertSrc, string fragSrc)
        {
            var vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, vertSrc);
            GL.CompileShader(vs);
            CheckShader(vs, "VERTEX");

            var fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, fragSrc);
            GL.CompileShader(fs);
            CheckShader(fs, "FRAGMENT");

            var prog = GL.CreateProgram();
            GL.AttachShader(prog, vs);
            GL.AttachShader(prog, fs);
            GL.LinkProgram(prog);
            CheckProgram(prog);

            GL.DeleteShader(vs);
            GL.DeleteShader(fs);

            return prog;
        }

        private static void CheckShader(int shader, string name)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var status);
            if (status == 0)
                throw new Exception($"[{name}] compile error:\n{GL.GetShaderInfoLog(shader)}");
        }

        private static void CheckProgram(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var status);
            if (status == 0)
                throw new Exception($"Program link error:\n{GL.GetProgramInfoLog(program)}");
        }

        public void Dispose()
        {
            if (_vbo != 0) GL.DeleteBuffer(_vbo);
            if (_vao != 0) GL.DeleteVertexArray(_vao);
            if (_shaderProgram != 0) GL.DeleteProgram(_shaderProgram);
        }
    }
}
