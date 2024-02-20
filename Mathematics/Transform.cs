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

        public Vector3D Right { get; set; } = new();

        public Vector3D Forward { get; set; } = new();

        public double Pitch { get; set; }

        public double Yaw { get; set; }

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
        ///     Moves the world.
        /// </summary>
        /// <param name="translation">The translation.</param>
        public void MoveWorld(Vector3D translation)
        {
            Translation += translation;
        }

        /// <summary>
        ///     Rotates the world.
        /// </summary>
        /// <param name="rotation">The rotation.</param>
        public void RotateWorld(Vector3D rotation)
        {
            Rotation += rotation;
        }

        /// <summary>
        ///     Ups the camera.
        /// </summary>
        /// <param name="y">The y.</param>
        public void UpCamera(double y)
        {
            //Position.Y += y;
            Position += Up * 0.05f;
        }

        /// <summary>
        ///     Downs the camera.
        /// </summary>
        /// <param name="y">The y.</param>
        public void DownCamera(double y)
        {
            //Position.Y -= y;
            Position -= Up * 0.05f;
        }

        /// <summary>
        ///     Lefts the camera.
        /// </summary>
        /// <param name="x">The x.</param>
        public void LeftCamera(double x)
        {
            //Position.X += x;
            Position -= Right * 0.05f;
        }

        /// <summary>
        ///     Rights the camera.
        /// </summary>
        /// <param name="x">The x.</param>
        public void RightCamera(double x)
        {
            //Position.X -= x;
            Position += Right * 0.05f;
        }

        /// <summary>
        ///     Lefts the rotate camera.
        /// </summary>
        /// <param name="value">The value.</param>
        public void LeftRotateCamera(double value)
        {
            //var vForward = VLookDir * value;
            //Position += vForward;
            Yaw -= 2.0f;
        }

        /// <summary>
        ///     Rights the rotate camera.
        /// </summary>
        /// <param name="value">The value.</param>
        public void RightRotateCamera(double value)
        {
            //var vForward = VLookDir * value;
            //Position -= vForward;
            Yaw += 2.0f;
        }

        public void MoveForward(double z)
        {
            //Position.Z += z;
            Position += Forward * 0.05f;
        }

        public void MoveBack(double z)
        {
            //Position.Z -= z;
            //Position -= Forward * 0.05f;
            Position -= Forward * 0.05f;
        }
    }
}
