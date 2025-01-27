using System;

namespace RenderEngine
{

    public class WorldCamera
    {
        /// <summary>
        ///     X position on the map.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        ///     Y position on the map.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        ///     Z position: 
        ///     Z = −CellSize / 2: Bottom of the cell.
        ///     Z = +CellSize / 2: Top of the cell.
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        ///     Offset of the horizon position (looking up-down).
        /// </summary>
        public int Horizon { get; set; }

        /// <summary>
        ///     Distance of the camera looking forward.
        /// </summary>
        public float ZFar { get; init; }

        /// <summary>
        ///     Camera angle (radians, clockwise).
        /// </summary>
        public float Angle { get; set; }

        /// <summary>
        ///     Camera pitch (vertical angle, radians, looking up-down).
        /// </summary>
        public float Pitch { get; set; }

        /// <summary>
        ///     Field of view in degrees.
        /// </summary>
        public float FieldOfView { get; set; } = 90f;

        /// <summary>
        ///     Cell size (dimensions of each unit cube in the map).
        /// </summary>
        public int CellSize { get; init; } = 1;

        /// <summary>
        ///     Calculates the visible bounds in the 3D world based on camera settings.
        /// </summary>
        public (float left, float right, float top, float bottom) GetViewFrustum()
        {
            // Compute half-width/height of the frustum at ZFar
            float halfHeight = (float)(Math.Tan(FieldOfView * Math.PI / 360) * ZFar);
            float halfWidth = halfHeight;

            return (
                left: X - halfWidth,
                right: X + halfWidth,
                top: Y + halfHeight,
                bottom: Y - halfHeight
            );
        }

        /// <summary>
        ///     Moves the camera in the given direction.
        /// </summary>
        public void Move(float deltaX, float deltaY, float deltaZ)
        {
            X += (int)deltaX;
            Y += (int)deltaY;
            Z += (int)deltaZ;
        }

        /// <summary>
        ///     Rotates the camera by the specified angle (in radians).
        /// </summary>
        public void Rotate(float deltaAngle)
        {
            Angle += deltaAngle;
            Angle %= (float)(2 * Math.PI); // Keep angle within 0 to 2π
        }

        /// <summary>
        ///     Adjusts the pitch of the camera.
        /// </summary>
        public void AdjustPitch(float deltaPitch)
        {
            Pitch += deltaPitch;
            Pitch = Math.Clamp(Pitch, -MathF.PI / 2, MathF.PI / 2); // Clamp between -90° and +90°
        }

        /// <summary>
        ///     Simulates rendering based on the camera's field of view and position.
        /// </summary>
        public void Render()
        {
            var frustum = GetViewFrustum();
            Console.WriteLine($"Rendering frustum:");
            Console.WriteLine($"Left: {frustum.left}, Right: {frustum.right}, Top: {frustum.top}, Bottom: {frustum.bottom}");
            Console.WriteLine($"Camera Position: ({X}, {Y}, {Z})");
            Console.WriteLine($"Angle: {Angle} radians, Pitch: {Pitch} radians");
        }
    }

}
