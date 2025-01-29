using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace RenderEngine
{
    public class StackedPlanesModel
    {
        private int _vao, _vbo, _ebo;
        private int _texture;
        private int _gridSizeX, _gridSizeY, _heightLevels;
        private float _cellSize;

        public StackedPlanesModel(int gridSizeX, int gridSizeY, int heightLevels, float cellSize, string texturePath)
        {
            _gridSizeX = gridSizeX;
            _gridSizeY = gridSizeY;
            _heightLevels = heightLevels;
            _cellSize = cellSize;
            LoadTexture(texturePath);
            GenerateModel();
        }

        private void GenerateModel()
        {
            int numPlanes = _gridSizeX * _gridSizeY * _heightLevels;
            float[] vertices = new float[numPlanes * 4 * 5]; // 4 vertices per plane, 5 attributes (x,y,z,u,v)
            uint[] indices = new uint[numPlanes * 6]; // 6 indices per plane (2 triangles)

            int vIndex = 0, iIndex = 0, vertexOffset = 0;

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    for (int z = 0; z < _heightLevels; z++)
                    {
                        float xPos = x * _cellSize;
                        float yPos = y * _cellSize;
                        float zPos = z * _cellSize;

                        // Define 4 vertices for a single 2D plane
                        Vector3[] planeVertices = {
                        new Vector3(xPos, yPos, zPos),                        // Bottom-left
                        new Vector3(xPos + _cellSize, yPos, zPos),            // Bottom-right
                        new Vector3(xPos + _cellSize, yPos + _cellSize, zPos), // Top-right
                        new Vector3(xPos, yPos + _cellSize, zPos)             // Top-left
                    };

                        // UV Coordinates (entire texture mapped)
                        float[] uv = { 0, 0, 1, 0, 1, 1, 0, 1 };

                        for (int v = 0; v < 4; v++)
                        {
                            vertices[vIndex++] = planeVertices[v].X;
                            vertices[vIndex++] = planeVertices[v].Y;
                            vertices[vIndex++] = planeVertices[v].Z;
                            vertices[vIndex++] = uv[v * 2];
                            vertices[vIndex++] = uv[v * 2 + 1];
                        }

                        // Indices for two triangles per plane
                        indices[iIndex++] = (uint)(vertexOffset);
                        indices[iIndex++] = (uint)(vertexOffset + 1);
                        indices[iIndex++] = (uint)(vertexOffset + 2);
                        indices[iIndex++] = (uint)(vertexOffset + 2);
                        indices[iIndex++] = (uint)(vertexOffset + 3);
                        indices[iIndex++] = (uint)(vertexOffset);

                        vertexOffset += 4;
                    }
                }
            }

            // Generate OpenGL buffers
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // Position Attribute
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Texture Coordinate Attribute
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
        }

        private void LoadTexture(string path)
        {
            _texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _texture);

            // Set texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // Load image
            using (var image = new System.Drawing.Bitmap(path))
            {
                var data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                    PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                image.UnlockBits(data);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public void Render()
        {
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _gridSizeX * _gridSizeY * _heightLevels * 6, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
    }

}
