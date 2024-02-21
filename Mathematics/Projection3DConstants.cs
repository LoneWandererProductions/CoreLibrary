/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/Projection3DConstants.cs
 * PURPOSE:     Holds the basic 3D Matrices
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://learn.microsoft.com/en-us/windows/win32/direct3d9/transforms
 *              https://www.brainvoyager.com/bv/doc/UsersGuide/CoordsAndTransforms/SpatialTransformationMatrices.html
 *              https://github.com/OneLoneCoder/Javidx9/blob/master/ConsoleGameEngine/BiggerProjects/Engine3D/OneLoneCoder_olcEngine3D_Part3.cpp
 */

using System;

namespace Mathematics
{
    /// <summary>
    ///     Some Matrices that can be used outside of 3D Projection
    /// </summary>
    internal static class Projection3DConstants
    {
        /// <summary>
        ///     Convert Degree to radial
        /// </summary>
        private const double Rad = Math.PI / 180.0d;

        /// <summary>
        ///     Camera Rotation Matrix.
        /// </summary>
        /// <param name="angleD">The angle d.</param>
        /// <returns>Camera Rotation Matrix</returns>
        internal static BaseMatrix RotateCamera(double angleD)
        {
            //convert to Rad
            var angle = angleD * Rad;

            double[,] rotation =
            {
                { Math.Cos(angle), 0, Math.Sin(angle), 0 }, { 0, 1, 0, 0 },
                { -Math.Sin(angle), 0, Math.Cos(angle), 0 }, { 0, 0, 0, 1 }
            };

            return new BaseMatrix { Matrix = rotation };
        }

        /// <summary>
        ///     Projections the to 3d matrix.
        /// </summary>
        /// <returns>Projection Matrix</returns>
        internal static BaseMatrix ProjectionTo3DMatrix()
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
        ///     Converts Coordinates based on the Camera.
        ///     https://ksimek.github.io/2012/08/22/extrinsic/
        ///     https://www.youtube.com/watch?v=HXSuNxpCzdM
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <returns>
        ///     matrix for Transforming the Coordinate
        /// </returns>
        internal static BaseMatrix PointAt(Transform transform)
        {
            var newForward = (transform.Target - transform.Camera).Normalize();
            //transform.Forward = (transform.Target - transform.Position).Normalize();

            var a = newForward * (transform.Up * newForward);
            //var a = transform.Forward * (transform.Up * transform.Forward);
            var newUp = (transform.Up - a).Normalize();
            //transform.Up = (transform.Up - a).Normalize();
            var newRight = newUp.CrossProduct(newForward);
            //transform.Right  = transform.Up.CrossProduct(transform.Forward);

            //return new BaseMatrix(4, 4)
            //{
            //    [0, 0] = transform.Right.X,
            //    [0, 1] = transform.Right..Y,
            //    [0, 2] = transform.Right..Z,
            //    [0, 3] = 0.0d,
            //    [1, 0] = transform.Up.X,
            //    [1, 1] = transform.Up.Y,
            //    [1, 2] = transform.Up.Z,
            //    [1, 3] = 0.0d,
            //    [2, 0] = transform.Forward.X,
            //    [2, 1] = transform.Forward.Y,
            //    [2, 2] = transform.Forward.Z,
            //    [2, 3] = 0.0d,
            //    [3, 0] = transform.Position.X,
            //    [3, 1] = transform.Position.Y,
            //    [3, 2] = transform.Position.Z,
            //    [3, 3] = transform.Position.W
            //};

            return new BaseMatrix(4, 4)
            {
                [0, 0] = newRight.X,
                [0, 1] = newRight.Y,
                [0, 2] = newRight.Z,
                [0, 3] = 0.0d,
                [1, 0] = newUp.X,
                [1, 1] = newUp.Y,
                [1, 2] = newUp.Z,
                [1, 3] = 0.0d,
                [2, 0] = newForward.X,
                [2, 1] = newForward.Y,
                [2, 2] = newForward.Z,
                [2, 3] = 0.0d,
                [3, 0] = transform.Position.X,
                [3, 1] = transform.Position.Y,
                [3, 2] = transform.Position.Z,
                [3, 3] = transform.Position.W
            };
        }

        /// <summary>
        ///     Rotates x.
        /// </summary>
        /// <param name="angleD">The angle d.</param>
        /// <returns>Rotation Matrix X</returns>
        public static BaseMatrix RotateX(double angleD)
        {
            //convert to Rad
            var angle = angleD * Rad;

            double[,] rotation =
            {
                { 1, 0, 0, 0 }, { 0, Math.Cos(angle), Math.Sin(angle), 0 },
                { 0, -Math.Sin(angle), Math.Cos(angle), 0 }, { 0, 0, 0, 1 }
            };

            return new BaseMatrix { Matrix = rotation };
        }

        /// <summary>
        ///     Rotates y.
        /// </summary>
        /// <param name="angleD">The angle d.</param>
        /// <returns>Rotation Matrix Y</returns>
        public static BaseMatrix RotateY(double angleD)
        {
            //convert to Rad
            var angle = angleD * Rad;

            double[,] rotation =
            {
                { Math.Cos(angle), 0, -Math.Sin(angle), 0 }, { 0, 1, 0, 0 },
                { Math.Sin(angle), 0, Math.Cos(angle), 0 }, { 0, 0, 0, 1 }
            };

            return new BaseMatrix { Matrix = rotation };
        }

        /// <summary>
        ///     Rotates z.
        /// </summary>
        /// <param name="angleD">The angle d.</param>
        /// <returns>Rotation Matrix Z</returns>
        public static BaseMatrix RotateZ(double angleD)
        {
            //convert to Rad
            var angle = angleD * Rad;

            double[,] rotation =
            {
                { Math.Cos(angle), Math.Sin(angle), 0, 0 }, { -Math.Sin(angle), Math.Cos(angle), 0, 0 },
                { 0, 0, 1, 0 }, { 0, 0, 0, 1 }
            };

            return new BaseMatrix { Matrix = rotation };
        }

        /// <summary>
        ///     Scale Matrix.
        /// </summary>
        /// <param name="value">The scale value.</param>
        /// <returns>Scale Matrix.</returns>
        public static BaseMatrix Scale(int value)
        {
            double[,] scale = { { value, 0, 0, 0 }, { 0, value, 0, 0 }, { 0, 0, value, 0 }, { 0, 0, 0, 1 } };

            return new BaseMatrix { Matrix = scale };
        }

        /// <summary>
        ///     Scales the specified vector.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <returns>Scale Matrix.</returns>
        public static BaseMatrix Scale(Vector3D vector)
        {
            double[,] scale = { { vector.X, 0, 0, 0 }, { 0, vector.Y, 0, 0 }, { 0, 0, vector.Z, 0 }, { 0, 0, 0, 1 } };

            return new BaseMatrix { Matrix = scale };
        }

        /// <summary>
        ///     Scale Matrix.
        /// </summary>
        /// <param name="one">The x value.</param>
        /// <param name="two">The y value.</param>
        /// <param name="three">The z value.</param>
        /// <returns>Scale Matrix.</returns>
        public static BaseMatrix Scale(double one, double two, double three)
        {
            double[,] scale = { { one, 0, 0, 0 }, { 0, two, 0, 0 }, { 0, 0, three, 0 }, { 0, 0, 0, 1 } };

            return new BaseMatrix { Matrix = scale };
        }

        /// <summary>
        ///     Translates the specified vector.
        /// </summary>
        /// <param name="vector">The vector.</param>
        /// <returns>Translation Matrix</returns>
        public static BaseMatrix Translate(Vector3D vector)
        {
            double[,] translate =
            {
                { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { vector.X, vector.Y, vector.Z, 1 }
            };

            return new BaseMatrix { Matrix = translate };
        }
    }
}
