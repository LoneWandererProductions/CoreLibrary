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

        public void RenderColumns(ColumnData[] columns, int screenWidth, int screenHeight)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Generate vertex data
            var vertexData = new float[columns.Length * 5 * 6];
            for (var i = 0; i < columns.Length; i++)
            {
                var column = columns[i];

                var columnHeight = column.Height / screenHeight;
                var xLeft = (i / (float)screenWidth * 2.0f) - 1.0f;
                var xRight = ((i + 1) / (float)screenWidth * 2.0f) - 1.0f;

                var yTop = columnHeight - 1.0f;
                var yBottom = -1.0f;

                var offset = i * 30; // 6 vertices * 5 attributes (x, y, r, g, b)

                // Vertex 1
                vertexData[offset + 0] = xLeft;
                vertexData[offset + 1] = yTop;
                vertexData[offset + 2] = column.Color.X;
                vertexData[offset + 3] = column.Color.Y;
                vertexData[offset + 4] = column.Color.Z;

                // Vertex 2
                vertexData[offset + 5] = xRight;
                vertexData[offset + 6] = yTop;
                vertexData[offset + 7] = column.Color.X;
                vertexData[offset + 8] = column.Color.Y;
                vertexData[offset + 9] = column.Color.Z;

                // Vertex 3
                vertexData[offset + 10] = xRight;
                vertexData[offset + 11] = yBottom;
                vertexData[offset + 12] = column.Color.X;
                vertexData[offset + 13] = column.Color.Y;
                vertexData[offset + 14] = column.Color.Z;

                // Vertex 4
                vertexData[offset + 15] = xLeft;
                vertexData[offset + 16] = yTop;
                vertexData[offset + 17] = column.Color.X;
                vertexData[offset + 18] = column.Color.Y;
                vertexData[offset + 19] = column.Color.Z;

                // Vertex 5
                vertexData[offset + 20] = xRight;
                vertexData[offset + 21] = yBottom;
                vertexData[offset + 22] = column.Color.X;
                vertexData[offset + 23] = column.Color.Y;
                vertexData[offset + 24] = column.Color.Z;

                // Vertex 6
                vertexData[offset + 25] = xLeft;
                vertexData[offset + 26] = yBottom;
                vertexData[offset + 27] = column.Color.X;
                vertexData[offset + 28] = column.Color.Y;
                vertexData[offset + 29] = column.Color.Z;
            }

            // Upload vertex data to GPU
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData,
                BufferUsageHint.DynamicDraw);

            // Render
            GL.UseProgram(_shaderProgram);
            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, columns.Length * 6);
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
            var aspectRatio = (float)_glControl.Width / _glControl.Height;
            GL.Viewport(0, 0, _glControl.Width, _glControl.Height);
        }
    }
}
