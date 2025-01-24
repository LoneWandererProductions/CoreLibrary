/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        RenderEngine/TkRender.cs
 * PURPOSE:     Render Control with OpenTK, aka OpenGL
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */


// ReSharper disable UnusedType.Global

using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;

namespace RenderEngine
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    /// Generic Display for our Graphics
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class TkRender
    {
        private GLControl _glControl;

        private int _shaderProgram;
        private int _vao;
        private int _vbo;

        public TkRender()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            Initialize();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Create a WindowsFormsHost
            var windowsFormsHost = new WindowsFormsHost();

            // Create and configure the GLControl
            _glControl = new GLControl { Dock = DockStyle.Fill };

            // Attach GLControl to WindowsFormsHost
            windowsFormsHost.Child = _glControl;

            // Add WindowsFormsHost to the WPF layout
            MainGrid.Children.Add(windowsFormsHost);

            // Attach OpenGL event handlers
            _glControl.Load += GlControl_Load;
            _glControl.Paint += GlControl_Paint;
            _glControl.Resize += GlControl_Resize;

            // Force initial paint
            _glControl.Invalidate();
        }

        public void Initialize()
        {
            // Compile shaders
            const string vertexShaderSource = @"
                #version 450 core
                layout(location = 0) in vec2 aPosition;
                layout(location = 1) in vec3 aColor;

                out vec3 vColor;

                void main()
                {
                    gl_Position = vec4(aPosition, 0.0, 1.0);
                    vColor = aColor;
                }
            ";

            const string fragmentShaderSource = @"
                #version 450 core
                in vec3 vColor;
                out vec4 FragColor;

                void main()
                {
                    FragColor = vec4(vColor, 1.0);
                }
            ";

            var vertexShader = CompileShader(ShaderType.VertexShader, vertexShaderSource);
            var fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentShaderSource);

            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vertexShader);
            GL.AttachShader(_shaderProgram, fragmentShader);
            GL.LinkProgram(_shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // Create VAO and VBO
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }

        private float[] GenerateVertexData<T>(T[] data, int screenWidth, int screenHeight, Func<T, float[]> getVertexAttributes)
        {
            var vertexData = new float[data.Length * 6 * 5]; // 6 vertices * 5 attributes (x, y, r, g, b)

            for (var i = 0; i < data.Length; i++)
            {
                var attributes = getVertexAttributes(data[i]);
                var xLeft = (i / (float)screenWidth * 2.0f) - 1.0f;
                var xRight = ((i + 1) / (float)screenWidth * 2.0f) - 1.0f;

                var columnHeight = attributes[0]; // In case of columns, this would be height, for example
                var yTop = columnHeight - 1.0f;
                var yBottom = -1.0f;

                var offset = i * 30; // 6 vertices * 5 attributes (x, y, r, g, b)

                // Using helper to set vertices
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

        public void RenderColumns(ColumnData[] columns, int screenWidth, int screenHeight)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Generate vertex data using the helper method
            var vertexData = GenerateVertexData(columns, screenWidth, screenHeight, column =>
            {
                return new float[] { column.Height / screenHeight, column.Color.X, column.Color.Y, column.Color.Z };
            });

            // Upload vertex data to GPU
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.DynamicDraw);

            // Render
            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, columns.Length * 6);
        }

        public void RenderPixels(PixelData[] pixels, int screenWidth, int screenHeight)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Generate vertex data using the helper method
            var vertexData = GenerateVertexData(pixels, screenWidth, screenHeight, pixel =>
            {
                var pixelWidth = 2.0f / screenWidth;
                var pixelHeight = 2.0f / screenHeight;
                return new float[] { pixelWidth, pixel.Color.X, pixel.Color.Y, pixel.Color.Z };
            });

            // Upload vertex data to GPU
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.DynamicDraw);

            // Render
            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, pixels.Length * 6);
        }

        private int CompileShader(ShaderType type, string source)
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

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_shaderProgram != 0) GL.DeleteProgram(_shaderProgram);
            if (_vao != 0) GL.DeleteVertexArray(_vao);
            if (_vbo != 0) GL.DeleteBuffer(_vbo);
            _glControl?.Dispose();
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            GL.ClearColor(0.1f, 0.2f, 0.3f, 1.0f);
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _glControl.SwapBuffers();
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            GL.Viewport(0, 0, _glControl.Width, _glControl.Height);
        }
    }
}
