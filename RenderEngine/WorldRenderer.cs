using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace RenderEngine
{
    public class WorldRenderer
    {
        public int CellSize { get; set; }
        public int WorldLength { get; set; }
        public int WorldWidth { get; set; }
        public int WorldHeight { get; set; }

        // Simulates the world as a 3D array of colors (can be replaced with a more complex system)
        private readonly Color[,,] _worldData;

        public WorldRenderer(int length, int width, int height, int cellSize)
        {
            WorldLength = length;
            WorldWidth = width;
            WorldHeight = height;
            CellSize = cellSize;

            // Initialize the world data with random colors for demonstration
            _worldData = new Color[WorldLength, WorldWidth, WorldHeight];
            var random = new Random();
            for (int x = 0; x < WorldLength; x++)
            {
                for (int y = 0; y < WorldWidth; y++)
                {
                    for (int z = 0; z < WorldHeight; z++)
                    {
                        _worldData[x, y, z] = Color.FromArgb(
                            random.Next(256), random.Next(256), random.Next(256));
                    }
                }
            }
        }

        public Bitmap Render(WorldCamera camera, int resolutionWidth, int resolutionHeight)
        {
            var image = new Bitmap(resolutionWidth, resolutionHeight);

            // Precompute frustum parameters
            float halfFOV = camera.FieldOfView * (float)Math.PI / 360; // FOV/2 in radians
            float aspectRatio = (float)resolutionWidth / resolutionHeight;
            float tanHalfFOV = (float)Math.Tan(halfFOV);

            // Loop through each pixel in the image
            for (int px = 0; px < resolutionWidth; px++)
            {
                for (int py = 0; py < resolutionHeight; py++)
                {
                    // Normalize pixel coordinates to [-1, 1] range
                    float nx = (2f * px / resolutionWidth - 1f) * aspectRatio;
                    float ny = 1f - 2f * py / resolutionHeight;

                    // Calculate ray direction in world space
                    float dx = nx * tanHalfFOV;
                    float dy = ny * tanHalfFOV;
                    float dz = 1f; // Assume forward direction

                    // Apply camera rotation
                    (dx, dy, dz) = RotateRay(dx, dy, dz, (int)camera.Angle, (int)camera.Pitch);

                    // Cast the ray and get the color
                    Color color = CastRay(camera.X, camera.Y, camera.Z, dx, dy, dz);

                    // Set the pixel color
                    image.SetPixel(px, py, color);
                }
            }

            return image;
        }

        private Color CastRay(float startX, float startY, float startZ, float dx, float dy, float dz)
        {
            var t = 0f;
            while (t < 1000) // Arbitrary large number to prevent infinite loops
            {
                // Calculate current position along the ray
                float x = startX + t * dx;
                float y = startY + t * dy;
                float z = startZ + t * dz;

                // Convert to world grid coordinates
                int cellX = (int)Math.Floor(x / CellSize);
                int cellY = (int)Math.Floor(y / CellSize);
                int cellZ = (int)Math.Floor(z / CellSize);

                // Check if the ray is outside the world bounds
                if (cellX < 0 || cellX >= WorldLength ||
                    cellY < 0 || cellY >= WorldWidth ||
                    cellZ < 0 || cellZ >= WorldHeight)
                {
                    return Color.Black; // Background color
                }

                // Return the color of the intersected cell
                return _worldData[cellX, cellY, cellZ];
            }

            return Color.Black; // Default to black if no intersection
        }

        private (float dx, float dy, float dz) RotateRay(float dx, float dy, float dz, float angle, float pitch)
        {
            // Convert angles to radians
            float angleRad = angle * (float)Math.PI / 180;
            float pitchRad = pitch * (float)Math.PI / 180;

            // Apply rotation around Y-axis (horizontal angle)
            float cosA = (float)Math.Cos(angleRad);
            float sinA = (float)Math.Sin(angleRad);
            float newDx = dx * cosA - dz * sinA;
            float newDz = dx * sinA + dz * cosA;

            // Apply rotation around X-axis (vertical pitch)
            float cosP = (float)Math.Cos(pitchRad);
            float sinP = (float)Math.Sin(pitchRad);
            float newDy = dy * cosP - newDz * sinP;
            float finalDz = dy * sinP + newDz * cosP;

            return (newDx, newDy, finalDz);
        }
        //Huge TODO

        private Color SampleOutdoorSkybox(float nx, float ny, WorldCamera camera)
        {
            // Map normalized coordinates to spherical direction
            float dx = nx;
            float dy = ny;
            float dz = 1f;

            // Rotate direction based on camera angle
            (dx, dy, dz) = RotateRay(dx, dy, dz, camera.Angle, camera.Pitch);

            // Map to skybox texture (spherical or cubemap)
            float u = 0.5f + (float)(Math.Atan2(dz, dx) / (2 * Math.PI));
            float v = 0.5f - (float)(Math.Asin(dy) / Math.PI);

            // Sample the skybox texture
            return Color.Black;  //OutdoorSkyboxTexture.Sample(u, v);
        }

        private Color SampleIndoorSkybox(float nx, float ny, WorldCamera camera, int roomWidth, int roomHeight, int roomLength)
        {
            // Normalize pixel to room dimensions
            float dx = nx * roomWidth;
            float dy = ny * roomHeight;

            // Calculate texture coordinates based on camera position and room size
            float u = (camera.X + dx) / roomWidth;
            float v = (camera.Y + dy) / roomHeight;

            // Choose wall, floor, or ceiling based on camera angle and position
            if (camera.Z < roomHeight / 2)
                return Color.Black; //CeilingTexture.Sample(u, v);
            else if (camera.Z > -roomHeight / 2)
                return Color.Black; //FloorTexture.Sample(u, v);
            else
                return Color.Black; //WallTexture.Sample(u, v);
        }

    }

}
