/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/Transform.cs
 * PURPOSE:     Controller Class for all 3D Projections and Camera
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace Mathematics
{
    /// <summary>
    /// Basic Controller for the World and the camera
    /// </summary>
    public sealed class Transform
    {
        /// <summary>
        /// Gets or sets the angle of the camera.
        /// </summary>
        /// <value>
        /// The angle in rad.
        /// </value>
        public double Angle { get; set; } = 0;

        /// <summary>
        /// Gets or sets the camera Vector.
        /// </summary>
        /// <value>
        /// The camera.
        /// </value>
        public Vector3D Camera { get; set; } = Vector3D.ZeroVector;

        /// <summary>
        /// Gets or sets up for the Camera.
        /// </summary>
        /// <value>
        /// Up.
        /// </value>
        public Vector3D Up { get; set; } = new Vector3D(0, 1, 0);

        /// <summary>
        /// Gets or sets the target Camera.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public Vector3D Target { get; set; } = new Vector3D(0, 0, 1);

        /// <summary>
        /// Gets or sets the look Vector direction.
        /// </summary>
        /// <value>
        /// The v look dir.
        /// </value>
        public Vector3D VLookDir { get; set; } = Vector3D.ZeroVector;

        /// <summary>
        /// Gets or sets the translation.
        /// </summary>
        /// <value>
        /// The translation.
        /// </value>
        public Vector3D Translation { get; set; } = Vector3D.UnitVector;

        /// <summary>
        /// Gets or sets the rotation.
        /// </summary>
        /// <value>
        /// The rotation.
        /// </value>
        public Vector3D Rotation { get; set; } = Vector3D.UnitVector;

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>
        /// The scale.
        /// </value>
        public Vector3D Scale { get; set; } = Vector3D.UnitVector;

        /// <summary>
        /// Moves the world.
        /// </summary>
        /// <param name="translation">The translation.</param>
        public void MoveWorld(Vector3D translation)
        {
            Translation += translation;
        }

        /// <summary>
        /// Rotates the world.
        /// </summary>
        /// <param name="rotation">The rotation.</param>
        public void RotateWorld(Vector3D rotation)
        {
            Rotation += rotation;
        }

        /// <summary>
        /// Rotates the camera left.
        /// </summary>
        /// <param name="angle">The angle.</param>
        public void RotateCameraLeft(double angle)
        {
            Angle += angle;
        }

        /// <summary>
        /// Rotates the camera right.
        /// </summary>
        /// <param name="angle">The angle.</param>
        public void RotateCameraRight(double angle)
        {
            Angle -= angle;
        }

        /// <summary>
        /// Ups the camera.
        /// </summary>
        /// <param name="y">The y.</param>
        public void UpCamera(double y)
        {
            Camera.Y += y;
        }

        /// <summary>
        /// Downs the camera.
        /// </summary>
        /// <param name="y">The y.</param>
        public void DownCamera(double y)
        {
            Camera.Y -= y;
        }

        /// <summary>
        /// Lefts the camera.
        /// </summary>
        /// <param name="x">The x.</param>
        public void LeftCamera(double x)
        {
            Camera.X -= x;
        }

        /// <summary>
        /// Rights the camera.
        /// </summary>
        /// <param name="x">The x.</param>
        public void RightCamera(double x)
        {
            Camera.X -= x;
        }

        /// <summary>
        /// Lefts the rotate camera.
        /// </summary>
        /// <param name="value">The value.</param>
        public void LeftRotateCamera(double value)
        {
            var vForward = VLookDir * value;
            Camera += vForward;
        }

        /// <summary>
        /// Rights the rotate camera.
        /// </summary>
        /// <param name="value">The value.</param>
        public void RightRotateCamera(double value)
        {
            var vForward = VLookDir * value;
            Camera -= vForward;
        }
    }
}
