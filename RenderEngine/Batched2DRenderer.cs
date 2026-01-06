/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine;
 * FILE:        Batched2DRenderer.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;

namespace RenderEngine;

public sealed class Batched2DRenderer : IDisposable
{
    private readonly int _width;
    private readonly int _height;

    private int _vboSolid;
    private int _vaoSolid;
    private readonly List<Vertex> _solidVertices = new();

    private int _vboTex;
    private int _vaoTex;
    private readonly List<Vertex> _texVertices = new();

    private int _solidShader;
    private int _textureShader;

    private bool _initialized;

    private struct Vertex
    {
        public float X, Y;
        public float R, G, B, A;
        public float U, V;
        public float TexIndex;
    }

    public Batched2DRenderer(int width, int height)
    {
        _width = width;
        _height = height;

        Test_DrawFullscreenQuad(width, height);
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;

        // --- Solid VAO/VBO ---
        _vaoSolid = GL.GenVertexArray();
        _vboSolid = GL.GenBuffer();
        GL.BindVertexArray(_vaoSolid);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);
        GL.BufferData(BufferTarget.ArrayBuffer, 1024 * 1024, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        var strideSolid = 6 * sizeof(float);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, strideSolid, 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, strideSolid, 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.BindVertexArray(0);

        // --- Textured VAO/VBO ---
        _vaoTex = GL.GenVertexArray();
        _vboTex = GL.GenBuffer();
        GL.BindVertexArray(_vaoTex);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);
        GL.BufferData(BufferTarget.ArrayBuffer, 1024 * 1024, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        var strideTex = 9 * sizeof(float);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, strideTex, 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, strideTex, 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, strideTex, 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, strideTex, 8 * sizeof(float));
        GL.EnableVertexAttribArray(3);
        GL.BindVertexArray(0);

        // --- Shaders ---
        _solidShader = CompileShader(SolidVertex(), SolidFragment());
        _textureShader = CompileShader(TextureVertex(), TextureFragment());

        _initialized = true;
    }

    // Minimal test — renders a fullscreen red quad using its own VAO/VBO/shader (no batching)
    void Test_DrawFullscreenQuad(int width, int height)
    {
        // create simple shader program (compile once; re-create each test is fine)
        var vs = @"
        #version 410 core
        layout(location = 0) in vec2 aPos;
        void main() {
            // positions are already in clip space
            gl_Position = vec4(aPos, 0.0, 1.0);
        }";
        var fs = @"
        #version 410 core
        out vec4 FragColor;
        void main() { FragColor = vec4(1.0, 0.0, 0.0, 1.0); }"; // solid red

        var v = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(v, vs);
        GL.CompileShader(v);
        GL.GetShader(v, ShaderParameter.CompileStatus, out var okv);
        if (okv == 0) throw new Exception("VS: " + GL.GetShaderInfoLog(v));

        var f = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(f, fs);
        GL.CompileShader(f);
        GL.GetShader(f, ShaderParameter.CompileStatus, out var okf);
        if (okf == 0) throw new Exception("FS: " + GL.GetShaderInfoLog(f));

        var prog = GL.CreateProgram();
        GL.AttachShader(prog, v);
        GL.AttachShader(prog, f);
        GL.LinkProgram(prog);
        GL.GetProgram(prog, GetProgramParameterName.LinkStatus, out var linkOk);
        if (linkOk == 0) throw new Exception("Link: " + GL.GetProgramInfoLog(prog));

        GL.DeleteShader(v);
        GL.DeleteShader(f);

        // Fullscreen quad in clip space (already -1..1)
        var verts = new float[]
        {
            -1f, 1f, // top-left
            1f, 1f, // top-right
            1f, -1f, // bottom-right

            1f, -1f, -1f, -1f, -1f, 1f
        };

        var vao = GL.GenVertexArray();
        var vbo = GL.GenBuffer();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // Render
        GL.Viewport(0, 0, width, height);
        GL.Disable(EnableCap.DepthTest);
        GL.ClearColor(Color.CornflowerBlue);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.UseProgram(prog);
        GL.BindVertexArray(vao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        // Read any GL error
        var err = GL.GetError();

        // cleanup
        GL.BindVertexArray(0);
        GL.DeleteBuffer(vbo);
        GL.DeleteVertexArray(vao);
        GL.DeleteProgram(prog);

        // If err != NoError, capture it and log
        if (err != ErrorCode.NoError) throw new Exception("GL error: " + err.ToString());
    }


    #region Shaders

    private string SolidVertex() => $@"
        #version 410 core
        layout(location=0) in vec2 aPos;
        layout(location=1) in vec4 aColor;
        out vec4 vColor;
        void main() {{
            vec2 pos = aPos / vec2({_width},{_height}) * 2.0 - 1.0;
            pos.y = -pos.y;
            gl_Position = vec4(pos,0,1);
            vColor = aColor;
        }}";

    private string SolidFragment() => @"
        #version 410 core
        in vec4 vColor;
        out vec4 FragColor;
        void main() { FragColor = vColor; }";

    private string TextureVertex() => $@"
        #version 410 core
        layout(location=0) in vec2 aPos;
        layout(location=1) in vec4 aColor;
        layout(location=2) in vec2 aTex;
        layout(location=3) in float aTexIndex;
        out vec2 vTex;
        out vec4 vColor;
        out float vTexIndex;
        void main() {{
            vec2 pos = aPos / vec2({_width},{_height}) * 2.0 - 1.0;
            pos.y = -pos.y;
            gl_Position = vec4(pos,0,1);
            vTex = aTex;
            vColor = aColor;
            vTexIndex = aTexIndex;
        }}";

    private string TextureFragment() => @"
        #version 410 core
        in vec2 vTex;
        in vec4 vColor;
        in float vTexIndex;
        uniform sampler2D uTexture;
        out vec4 FragColor;
        void main() {
            vec4 texColor = texture(uTexture, vTex);
            FragColor = texColor * vColor;
        }";

    #endregion

    private int CompileShader(string vs, string fs)
    {
        var v = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(v, vs);
        GL.CompileShader(v);
        GL.GetShader(v, ShaderParameter.CompileStatus, out var success);
        if (success == 0) throw new Exception(GL.GetShaderInfoLog(v));

        var f = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(f, fs);
        GL.CompileShader(f);
        GL.GetShader(f, ShaderParameter.CompileStatus, out success);
        if (success == 0) throw new Exception(GL.GetShaderInfoLog(f));

        var program = GL.CreateProgram();
        GL.AttachShader(program, v);
        GL.AttachShader(program, f);
        GL.LinkProgram(program);
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out success);
        if (success == 0) throw new Exception(GL.GetProgramInfoLog(program));

        GL.DeleteShader(v);
        GL.DeleteShader(f);
        return program;
    }

    private void AddSolidVertex(float x, float y, float r, float g, float b, float a)
        => _solidVertices.Add(new Vertex
        {
            X = x,
            Y = y,
            R = r,
            G = g,
            B = b,
            A = a
        });

    private void AddTexVertex(float x, float y, float r, float g, float b, float a, float u, float v, float texId)
        => _texVertices.Add(new Vertex
        {
            X = x,
            Y = y,
            R = r,
            G = g,
            B = b,
            A = a,
            U = u,
            V = v,
            TexIndex = texId
        });

    #region Public Draw API

    public void DrawSolidQuad(Point p0, Point p1, Point p2, Point p3, Color color)
    {
        EnsureInitialized();
        var r = color.R / 255f;
        var g = color.G / 255f;
        var b = color.B / 255f;
        var a = color.A / 255f;

        AddSolidVertex(p0.X, p0.Y, r, g, b, a);
        AddSolidVertex(p1.X, p1.Y, r, g, b, a);
        AddSolidVertex(p2.X, p2.Y, r, g, b, a);

        AddSolidVertex(p2.X, p2.Y, r, g, b, a);
        AddSolidVertex(p3.X, p3.Y, r, g, b, a);
        AddSolidVertex(p0.X, p0.Y, r, g, b, a);
    }

    public void DrawTexturedQuad(Point p0, Point p1, Point p2, Point p3, int texId, float u0 = 0, float v0 = 0,
        float u1 = 1, float v1 = 1)
    {
        EnsureInitialized();
        AddTexVertex(p0.X, p0.Y, 1, 1, 1, 1, u0, v0, texId);
        AddTexVertex(p1.X, p1.Y, 1, 1, 1, 1, u1, v0, texId);
        AddTexVertex(p2.X, p2.Y, 1, 1, 1, 1, u1, v1, texId);

        AddTexVertex(p2.X, p2.Y, 1, 1, 1, 1, u1, v1, texId);
        AddTexVertex(p3.X, p3.Y, 1, 1, 1, 1, u0, v1, texId);
        AddTexVertex(p0.X, p0.Y, 1, 1, 1, 1, u0, v0, texId);
    }

    #endregion

    public void Flush()
    {
        EnsureInitialized();

        // --- Solid ---
        if (_solidVertices.Count > 0)
        {
            GL.BindVertexArray(_vaoSolid);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboSolid);

            var raw = new float[_solidVertices.Count * 6];
            for (var i = 0; i < _solidVertices.Count; i++)
            {
                var idx = i * 6;
                var v = _solidVertices[i];
                raw[idx + 0] = v.X;
                raw[idx + 1] = v.Y;
                raw[idx + 2] = v.R;
                raw[idx + 3] = v.G;
                raw[idx + 4] = v.B;
                raw[idx + 5] = v.A;
            }

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, raw.Length * sizeof(float), raw);

            GL.UseProgram(_solidShader);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _solidVertices.Count);
            _solidVertices.Clear();
        }

        // --- Textured ---
        if (_texVertices.Count > 0)
        {
            GL.BindVertexArray(_vaoTex);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboTex);

            var raw = new float[_texVertices.Count * 9];
            for (var i = 0; i < _texVertices.Count; i++)
            {
                var idx = i * 9;
                var v = _texVertices[i];
                raw[idx + 0] = v.X;
                raw[idx + 1] = v.Y;
                raw[idx + 2] = v.R;
                raw[idx + 3] = v.G;
                raw[idx + 4] = v.B;
                raw[idx + 5] = v.A;
                raw[idx + 6] = v.U;
                raw[idx + 7] = v.V;
                raw[idx + 8] = v.TexIndex;
            }

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, raw.Length * sizeof(float), raw);

            GL.UseProgram(_textureShader);
            // Bind first texture only (for simplicity)
            var texId = (int)_texVertices[0].TexIndex;
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texId);
            var loc = GL.GetUniformLocation(_textureShader, "uTexture");
            GL.Uniform1(loc, 0);

            GL.DrawArrays(PrimitiveType.Triangles, 0, _texVertices.Count);
            _texVertices.Clear();
        }

        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        if (!_initialized) return;

        GL.DeleteVertexArray(_vaoSolid);
        GL.DeleteBuffer(_vboSolid);
        GL.DeleteVertexArray(_vaoTex);
        GL.DeleteBuffer(_vboTex);
        GL.DeleteProgram(_solidShader);
        GL.DeleteProgram(_textureShader);
        _initialized = false;
    }
}
