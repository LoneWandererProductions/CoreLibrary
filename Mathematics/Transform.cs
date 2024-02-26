﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/Transform.cs
 * PURPOSE:     Controller Class for all 3D Projections and Camera
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

namespace Mathematics
{
    /// <summary>
    ///     Basic Controller for the World and the camera
    /// </summary>
    public sealed class Transform
    {
        /// <summary>
        ///     Gets or sets the camera Vector.
        ///     Used in the Orbit and LookAt Camera
        /// </summary>
        /// <value>
        ///     The camera.
        /// </value>
        public Vector3D Position { get; set; }

        /// <summary>
        ///     Gets or sets the target Camera.
        ///     Used in the LookAt Camera
        /// </summary>
        /// <value>
        ///     The target.
        /// </value>
        public Vector3D Target { get; private init; } = Vector3D.ZeroVector;

        /// <summary>
        ///     Gets or sets up for the Camera.
        ///     Used in the Orbit and LookAt Camera
        /// </summary>
        /// <value>
        ///     Up.
        /// </value>
        public Vector3D Up { get; set; }

        /// <summary>
        ///     Gets or sets the right.
        ///     Used in the Orbit Camera.
        /// </summary>
        /// <value>
        ///     The right.
        /// </value>
        public Vector3D Right { get; set; } = new();

        /// <summary>
        ///     Gets or sets the forward.
        ///     Used in the Orbit Camera.
        /// </summary>
        /// <value>
        ///     The forward.
        /// </value>
        public Vector3D Forward { get; set; } = new();

        /// <summary>
        ///     Gets or sets the pitch.
        ///     Used in the Orbit Camera.
        /// </summary>
        /// <value>
        ///     The pitch.
        /// </value>
        public double Pitch { get; set; }

        /// <summary>
        ///     Gets or sets the yaw.
        ///     Used in the Orbit Camera.
        /// </summary>
        /// <value>
        ///     The yaw.
        /// </value>
        public double Yaw { get; set; }

        /// <summary>
        ///     Gets or sets the translation of the World.
        /// </summary>
        /// <value>
        ///     The translation.
        /// </value>
        public Vector3D Translation { get; set; }

        /// <summary>
        ///     Gets or sets the rotation of the World.
        /// </summary>
        /// <value>
        ///     The rotation.
        /// </value>
        public Vector3D Rotation { get; set; }

        /// <summary>
        ///     Gets or sets the scale of the World.
        /// </summary>
        /// <value>
        ///     The scale.
        /// </value>
        public Vector3D Scale { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [camera type] is LookAt or Orbit Camera.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Orbit Camera; otherwise, if <c>false</c> LookAt Camera.
        /// </value>
        public Cameras CameraType { get; set; } = Cameras.Orbit;

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
                Position = new Vector3D(),
                Translation = translation,
                Rotation = rotation,
                Scale = scale
            };
        }

        /// <summary>
        ///     Ups the camera
        /// </summary>
        /// <param name="y">The y.</param>
        public void UpCamera(double y = 0.05d)
        {
            switch (CameraType)
            {
                case Cameras.Orbit:
                    Position += Up * y;
                    break;
                case Cameras.PointAt:
                    Position.Y += y;
                    break;
            }
        }

        /// <summary>
        ///     Downs the camera.
        ///     Orbit and PointAt.
        /// </summary>
        /// <param name="y">The y.</param>
        public void DownCamera(double y = 0.05d)
        {
            switch (CameraType)
            {
                case Cameras.Orbit:
                    Position -= Up * y;
                    break;
                case Cameras.PointAt:
                    Position.Y -= y;
                    break;
            }
        }

        /// <summary>
        ///     Lefts the camera.
        /// </summary>
        /// <param name="x">The x.</param>
        public void LeftCamera(double x = 0.05d)
        {
            switch (CameraType)
            {
                case Cameras.Orbit:
                    Position += Right * x;
                    break;
                case Cameras.PointAt:
                    Position.X -= x;
                    break;
            }
        }

        /// <summary>
        ///     Rights the camera.
        ///     Orbit and PointAt.
        /// </summary>
        /// <param name="x">The x.</param>
        public void RightCamera(double x = 0.05d)
        {
            switch (CameraType)
            {
                case Cameras.Orbit:
                    Position -= Right * x;
                    break;
                case Cameras.PointAt:
                    Position.X += x;
                    break;
            }
        }

        /// <summary>
        ///     Lefts the rotate camera.
        /// </summary>
        /// <param name="value">The value.</param>
        public void LeftRotateCamera(double value = 2.0d)
        {
            switch (CameraType)
            {
                case Cameras.Orbit:
                    Yaw -= value;
                    break;
                case Cameras.PointAt:
                    Yaw -= value;
                    break;
            }
        }

        /// <summary>
        ///     Rights the rotate camera.
        /// </summary>
        /// <param name="value">The value.</param>
        public void RightRotateCamera(double value)
        {
            switch (CameraType)
            {
                case Cameras.Orbit:
                    Yaw += value;
                    break;
                case Cameras.PointAt:
                    Yaw += value;
                    break;
            }
        }

        /// <summary>
        ///     Moves the forward.
        /// </summary>
        /// <param name="z">The z.</param>
        public void MoveForward(double z = 0.05d)
        {
            switch (CameraType)
            {
                case Cameras.Orbit:
                    Position += Forward * z;
                    break;
                case Cameras.PointAt:
                    Position += new Vector3D(0, 0, 1) * z;
                    break;
            }
        }

        /// <summary>
        ///     Moves the back.
        /// </summary>
        /// <param name="z">The z.</param>
        public void MoveBack(double z = 0.05d)
        {
            switch (CameraType)
            {
                case Cameras.Orbit:
                    Position -= Forward * z;
                    break;
                case Cameras.PointAt:
                    Position -= new Vector3D(0, 0, 1) * z;
                    break;
            }
        }
    }
}
