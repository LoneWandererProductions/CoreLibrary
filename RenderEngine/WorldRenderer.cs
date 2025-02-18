using System;
using System.Drawing;

namespace RenderEngine
{
    public class WorldRenderer
    {
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
            for (var x = 0; x < WorldLength; x++)
            {
                for (var y = 0; y < WorldWidth; y++)
                {
                    for (var z = 0; z < WorldHeight; z++)
                    {
                        _worldData[x, y, z] = Color.FromArgb(
                            random.Next(256), random.Next(256), random.Next(256));
                    }
                }
            }
        }

        public int CellSize { get; set; }
        public int WorldLength { get; set; }
        public int WorldWidth { get; set; }
        public int WorldHeight { get; set; }

        public Bitmap Render(WorldCamera camera, int resolutionWidth, int resolutionHeight)
        {
            var image = new Bitmap(resolutionWidth, resolutionHeight);

            // Precompute frustum parameters
            var halfFov = camera.FieldOfView * (float)Math.PI / 360; // FOV/2 in radians
            var aspectRatio = (float)resolutionWidth / resolutionHeight;
            var tanHalfFov = (float)Math.Tan(halfFov);

            // Loop through each pixel in the image
            for (var px = 0; px < resolutionWidth; px++)
            {
                for (var py = 0; py < resolutionHeight; py++)
                {
                    // Normalize pixel coordinates to [-1, 1] range
                    var nx = ((2f * px / resolutionWidth) - 1f) * aspectRatio;
                    var ny = 1f - (2f * py / resolutionHeight);

                    // Calculate ray direction in world space
                    var dx = nx * tanHalfFov;
                    var dy = ny * tanHalfFov;
                    var dz = 1f; // Assume forward direction

                    // Apply camera rotation
                    (dx, dy, dz) = RotateRay(dx, dy, dz, (int)camera.Angle, (int)camera.Pitch);

                    // Cast the ray and get the color
                    var color = CastRay(camera.X, camera.Y, camera.Z, dx, dy, dz);

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
                var x = startX + (t * dx);
                var y = startY + (t * dy);
                var z = startZ + (t * dz);

                // Convert to world grid coordinates
                var cellX = (int)Math.Floor(x / CellSize);
                var cellY = (int)Math.Floor(y / CellSize);
                var cellZ = (int)Math.Floor(z / CellSize);

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
            var angleRad = angle * (float)Math.PI / 180;
            var pitchRad = pitch * (float)Math.PI / 180;

            // Apply rotation around Y-axis (horizontal angle)
            var cosA = (float)Math.Cos(angleRad);
            var sinA = (float)Math.Sin(angleRad);
            var newDx = (dx * cosA) - (dz * sinA);
            var newDz = (dx * sinA) + (dz * cosA);

            // Apply rotation around X-axis (vertical pitch)
            var cosP = (float)Math.Cos(pitchRad);
            var sinP = (float)Math.Sin(pitchRad);
            var newDy = (dy * cosP) - (newDz * sinP);
            var finalDz = (dy * sinP) + (newDz * cosP);

            return (newDx, newDy, finalDz);
        }
        //Huge TODO

        private Color SampleOutdoorSkybox(float nx, float ny, WorldCamera camera)
        {
            // Map normalized coordinates to spherical direction
            var dx = nx;
            var dy = ny;
            var dz = 1f;

            // Rotate direction based on camera angle
            (dx, dy, dz) = RotateRay(dx, dy, dz, camera.Angle, camera.Pitch);

            // Map to skybox texture (spherical or cubemap)
            var u = 0.5f + (float)(Math.Atan2(dz, dx) / (2 * Math.PI));
            var v = 0.5f - (float)(Math.Asin(dy) / Math.PI);

            // Sample the skybox texture
            return Color.Black; //OutdoorSkyboxTexture.Sample(u, v);
        }

        private Color SampleIndoorSkybox(float nx, float ny, WorldCamera camera, int roomWidth, int roomHeight,
            int roomLength)
        {
            // Normalize pixel to room dimensions
            var dx = nx * roomWidth;
            var dy = ny * roomHeight;

            // Calculate texture coordinates based on camera position and room size
            var u = (camera.X + dx) / roomWidth;
            var v = (camera.Y + dy) / roomHeight;

            // Choose wall, floor, or ceiling based on camera angle and position
            if (camera.Z < roomHeight / 2)
            {
                return Color.Black; //CeilingTexture.Sample(u, v);
            }

            if (camera.Z > -roomHeight / 2)
            {
                return Color.Black; //FloorTexture.Sample(u, v);
            }

            return Color.Black; //WallTexture.Sample(u, v);
        }

        //TOOD other approach
        public (float uMin, float uMax, float vMin, float vMax) GetSkyboxProjection(WorldCamera camera)
        {
            // Step 1: Calculate the boundaries of the frustum
            var halfHeight = (float)(Math.Tan(camera.FieldOfView * Math.PI / 360) * camera.ZFar);
            var halfWidth = halfHeight;

            // Frustum boundaries (camera position is at the origin, frustum at ZFar distance)
            var frustumLeft = camera.X - halfWidth;
            var frustumRight = camera.X + halfWidth;
            var frustumTop = camera.Y + halfHeight;
            var frustumBottom = camera.Y - halfHeight;

            // Step 2: Map these boundaries to the skybox plane (assuming the skybox is a 2D texture or cubemap face)
            // Here we assume the skybox is projected onto a unit square (0, 1) in both u and v coordinates.

            // Convert the frustum to normalized coordinates for the skybox (u, v)
            var uMin = (frustumLeft + halfWidth) / (2 * halfWidth); // Normalize to [0, 1]
            var uMax = (frustumRight + halfWidth) / (2 * halfWidth); // Normalize to [0, 1]
            var vMin = (frustumBottom + halfHeight) / (2 * halfHeight); // Normalize to [0, 1]
            var vMax = (frustumTop + halfHeight) / (2 * halfHeight); // Normalize to [0, 1]

            return (uMin, uMax, vMin, vMax);
        }
    }
}
