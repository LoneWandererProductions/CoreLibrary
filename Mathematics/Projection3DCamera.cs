/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/Projection3DCamera.cs
 * PURPOSE:     Some basics for 3D displays. mostly the camera
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

using System;

namespace Mathematics
{
    /// <summary>
    ///     3D Projection
    /// </summary>
    public static class Projection3DCamera
    {
        /// <summary>
        ///     Converts to 3d.
        ///     TODO Test
        ///     We need norm vectors for the camera if we use a plane
        ///     than we need a translation matrix to increase the values and perhaps position of the point, might as well use the
        ///     directX components in the future
        ///     TODO Alt:
        ///     https://stackoverflow.com/questions/8633034/basic-render-3d-perspective-projection-onto-2d-screen-with-camera-without-openg
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns>Transformed Coordinates</returns>
        public static Vector3D ProjectionTo3D(Vector3D start)
        {
            double[,] matrix = { { start.X, start.Y, start.Z, 1 } };

            var m1 = new BaseMatrix(matrix);
            var m2 = ProjectionTo3DMatrix();

            var result = m1 * m2;
            var x = result[0, 0];
            var y = result[0, 1];
            var z = result[0, 2];
            var w = result[0, 3];

            var check = Math.Round(w, 2);

            if (check == 0.0f)
            {
                return new Vector3D(x, y, z);
            }

            x /= w;
            y /= w;
            z /= w;
            return new Vector3D(x, y, z);
        }

        /// <summary>
        ///     Projections the to3 d matrix.
        /// </summary>
        /// <returns>Projection Matrix</returns>
        public static BaseMatrix ProjectionTo3DMatrix()
        {
            double[,] translation =
            {
                { Projection3DRegister.A * Projection3DRegister.F, 0, 0, 0 }, { 0, Projection3DRegister.F, 0, 0 },
                { 0, 0, Projection3DRegister.Q, 1 },
                { 0, 0, -Projection3DRegister.ZNear * Projection3DRegister.Q, 0 }
            };

            //now lacks /w, has to be done at the end!
            return new BaseMatrix(translation);
        }

        /// <summary>
        ///     [ModelViewProjectionMatrix] = [View To Projection]x[World To View]x[Model to World]
        ///     Creates a 3D world transformation
        ///     Todo test
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <param name="camera">The camera.</param>
        /// <param name="angle">The angle.</param>
        /// <param name="angleX">The angle x.</param>
        /// <param name="angleZ">The angle z.</param>
        /// <returns>World Transformation</returns>
        public static BaseMatrix ModelViewProjectionMatrix(Vector3D vector, Vector3D camera, int angle, int angleX,
            int angleZ)
        {
            // Set up "World Transform"
            //var matRotZ = Projection3D.RotateZ(angle);
            //var matRotX = Projection3D.RotateX(angle);
            //var matTrans = Projection3D.Translate(0.0f, 0.0f, 5.0f);

            //identity Matrix
            var worldMatrix = MatrixUtility.MatrixIdentity(4);

            // Form ModelViewProjectionMatrix
            // View To Projection, Camera
            // Model to World, Transform by rotation
            // Model to World, Transform by translation
            var m1 = ViewMatrix(camera, angle);

            worldMatrix = worldMatrix * m1 *
                          Projection3D.RotateX(vector, angleX) *
                          Projection3D.RotateZ(vector, angleZ) *
                          Projection3D.RotateZ(vector, angleZ);

            return worldMatrix;
        }

        /// <summary>
        ///     Views the matrix.
        /// </summary>
        /// <param name="camera">The camera.</param>
        /// <param name="angle">The angle.</param>
        /// <returns>The View Matrix, aka the Camera</returns>
        public static BaseMatrix ViewMatrix(Vector3D camera, double angle)
        {
            var up = new Vector3D(0, 1, 0);

            //rotate into new Position
            var cameraRotation = CameraRotation(angle);

            //var lookDir = cameraRotation * target, (matrix * Vector)
            //TODO check, a huge mess, compare with other results
            var mTarget = new BaseMatrix(1, 4) { [0, 0] = 0, [0, 1] = 0, [0, 2] = 1, [0, 3] = 1 };

            mTarget = cameraRotation * mTarget;
            var lookDir = Projection3D.GetVector(mTarget);
            var vTarget = camera + lookDir;

            var matCamera = PointAt(camera, vTarget, up);

            // Make view matrix from camera
            return matCamera.Inverse();
        }


        /// <summary>
        ///     Converts Coordinates based on the Camera.
        ///     https://ksimek.github.io/2012/08/22/extrinsic/
        ///     https://github.com/OneLoneCoder/Javidx9/blob/master/ConsoleGameEngine/BiggerProjects/Engine3D/OneLoneCoder_olcEngine3D_Part3.cpp
        ///     https://www.youtube.com/watch?v=HXSuNxpCzdM
        /// </summary>
        /// <param name="camera">Current Position.</param>
        /// <param name="target">Directional Vector, Point at.</param>
        /// <param name="up">Directional Vector, Z Axis.</param>
        /// <returns>matrix for Transforming the Coordinate</returns>
        public static BaseMatrix PointAt(Vector3D camera, Vector3D target, Vector3D up)
        {
            var newForward = target - camera;
            var a = newForward.Multiply(up * newForward);
            var newUp = up - a;
            var newRight = newUp.CrossProduct(newForward);

            return new BaseMatrix
            {
                Matrix =
                {
                    [0, 0] = newRight.X,
                    [0, 1] = newRight.Y,
                    [0, 2] = newRight.Z,
                    [0, 3] = 0.0f,
                    [1, 0] = newUp.X,
                    [1, 1] = newUp.Y,
                    [1, 2] = newUp.Z,
                    [1, 3] = 0.0f,
                    [2, 0] = newForward.X,
                    [2, 1] = newForward.Y,
                    [2, 2] = newForward.Z,
                    [2, 3] = 0.0f,
                    [3, 0] = camera.X,
                    [3, 1] = camera.Y,
                    [3, 2] = camera.Z,
                    [3, 3] = 1.0f
                }
            };
        }


        /// <summary>
        ///     Cameras the rotation.
        ///     https://github.com/OneLoneCoder/Javidx9/blob/master/ConsoleGameEngine/BiggerProjects/Engine3D/OneLoneCoder_olcEngine3D_Part4.cpp
        /// </summary>
        /// <param name="angle">The angle.</param>
        /// <returns>The Rotation Matrix for our Camera</returns>
        private static BaseMatrix CameraRotation(double angle)
        {
            double[,] rotation =
            {
                { Math.Cos(angle), 0, Math.Sin(angle), 0 }, { 0, 1, 0, 0 },
                { 0, -Math.Sin(angle), 0, Math.Cos(angle) }, { 0, 0, 0, 1 }
            };

            return new BaseMatrix(rotation);
        }
    }
}
