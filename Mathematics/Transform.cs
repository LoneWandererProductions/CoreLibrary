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
    ///     Basic Controller for the World and the camera
    /// </summary>
    public sealed class Transform
    {
        /// <summary>
        ///     Gets or sets the angle of the camera.
        /// </summary>
        /// <value>
        ///     The angle in rad.
        /// </value>
        public double Angle { get; set; }

        /// <summary>
        ///     Gets or sets the camera Vector.
        /// </summary>
        /// <value>
        ///     The camera.
        /// </value>
        public Vector3D Camera { get; set; }

        /// <summary>
        ///     Gets or sets the camera Vector.
        /// </summary>
        /// <value>
        ///     The camera.
        /// </value>
        public Vector3D Position { get; set; }

        /// <summary>
        ///     Gets or sets up for the Camera.
        /// </summary>
        /// <value>
        ///     Up.
        /// </value>
        public Vector3D Up { get; set; }

        /// <summary>
        ///     Gets or sets the target Camera.
        /// </summary>
        /// <value>
        ///     The target.
        /// </value>
        public Vector3D Target { get; set; }

        /// <summary>
        ///     Gets or sets the look Vector direction.
        /// </summary>
        /// <value>
        ///     The v look dir.
        /// </value>
        public Vector3D VLookDir { get; set; }

        /// <summary>
        ///     Gets or sets the translation.
        /// </summary>
        /// <value>
        ///     The translation.
        /// </value>
        public Vector3D Translation { get; set; }

        /// <summary>
        ///     Gets or sets the rotation.
        /// </summary>
        /// <value>
        ///     The rotation.
        /// </value>
        public Vector3D Rotation { get; set; }

        /// <summary>
        ///     Gets or sets the scale.
        /// </summary>
        /// <value>
        ///     The scale.
        /// </value>
        public Vector3D Scale { get; set; }

        /// <summary>
        /// Gets or sets the right.
        /// </summary>
        /// <value>
        /// The right.
        /// </value>
        public Vector3D Right { get; set; } = new();

        /// <summary>
        /// Gets or sets the forward.
        /// </summary>
        /// <value>
        /// The forward.
        /// </value>
        public Vector3D Forward { get; set; } = new();

        /// <summary>
        /// Gets or sets the pitch.
        /// </summary>
        /// <value>
        /// The pitch.
        /// </value>
        public double Pitch { get; set; }

        /// <summary>
        /// Gets or sets the yaw.
        /// </summary>
        /// <value>
        /// The yaw.
        /// </value>
        public double Yaw { get; set; }

        public bool CameraType { get; set; } = true;

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <returns>Instance of Transform</returns>
        public static Transform GetInstance()
        {
            return new Transform
            {
                Up = new Vector3D(0, 1, 0),
                Target = new Vector3D(0, 0, 1),
                VLookDir = new Vector3D(),
                Angle = 0,
                Camera = new Vector3D(),
                Position = new Vector3D(),
                Translation = new Vector3D(),
                Rotation = new Vector3D(),
                Scale = Vector3D.UnitVector
            };
        }

        /// <summary>
        ///     Gets the instance.
        /// </summary>
        /// <param name="translation">The translation.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="rotation">The rotation.</param>
        /// <returns>Instance of Transform</returns>
        public static Transform GetInstance(Vector3D translation, Vector3D scale, Vector3D rotation)
        {
            return new Transform
            {
                Up = new Vector3D(0, 1, 0),
                Target = new Vector3D(0, 0, 1),
                VLookDir = new Vector3D(),
                Angle = 0,
                Camera = new Vector3D(),
                Position = new Vector3D(),
                Translation = translation,
                Rotation = rotation,
                Scale = scale
            };
        }

        /// <summary>
        ///     Ups the camera.
        /// </summary>
        /// <param name="y">The y.</param>
        public void UpCamera(double y)
        {
            //orbit
            if (CameraType) Position += Up * 0.05d;
            //pointAt
            else Position.Y += 0.05d;
        }

        /// <summary>
        ///     Downs the camera.
        /// </summary>
        /// <param name="y">The y.</param>
        public void DownCamera(double y)
        {
            //orbit
            if (CameraType) Position -= Up * 0.05d;
            //pointAt
            else Position.Y -= 0.05d;
        }

        /// <summary>
        ///     Lefts the camera.
        /// </summary>
        /// <param name="x">The x.</param>
        public void LeftCamera(double x)
        {
            //orbit
            if (CameraType) Position -= Right * 0.05d;
            //pointAt
            Position.X += 0.05d;
        }

        /// <summary>
        ///     Rights the camera.
        /// </summary>
        /// <param name="x">The x.</param>
        public void RightCamera(double x)
        {
            //orbit
            if (CameraType) Position += Right * 0.05d;
            //pointAt
            else Position.X -= 0.05d;
        }

        /// <summary>
        ///     Lefts the rotate camera.
        /// </summary>
        /// <param name="value">The value.</param>
        public void LeftRotateCamera(double value)
        {
            //orbit
            if (CameraType) Yaw += 2.0d;
            //pointAt
            else Yaw -= 2.0d;
        }

        /// <summary>
        ///     Rights the rotate camera.
        /// </summary>
        /// <param name="value">The value.</param>
        public void RightRotateCamera(double value)
        {
            //orbit
            if (CameraType) Yaw += 2.0d;
            //pointAt
            else Yaw += 2.0d;
        }

        /// <summary>
        /// Moves the forward.
        /// </summary>
        /// <param name="z">The z.</param>
        public void MoveForward(double z)
        {
            //orbit
            if (CameraType) Position += Forward * 0.05d;
            //pointAt
            else Position += Forward * 0.05d;
        }

        public void MoveBack(double z)
        {
            //orbit
            if (CameraType) Position -= Forward * 0.05d;
            //pointAt
            else Position.Z -= z;
        }
    }
}
