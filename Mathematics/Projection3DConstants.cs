/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/Projection3DConstants.cs
 * PURPOSE:     Holds the basic 3D Matrices
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://learn.microsoft.com/en-us/windows/win32/direct3d9/transforms
 *              https://www.brainvoyager.com/bv/doc/UsersGuide/CoordsAndTransforms/SpatialTransformationMatrices.html
 */

using System;

namespace Mathematics
{
    internal static class Projection3DConstants
    {
        /// <summary>
        ///     Convert Degree to radial
        /// </summary>
        private const double Rad = Math.PI / 180.0;

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
        /// Scales the specified vector.
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

        /// <summary>
        /// Gets the model matrix.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <returns>The Model Matrix</returns>
        public static BaseMatrix GetModelMatrix(Transform transform)
        {
            // Use LEFT-Handed rotation matrices (as seen in DirectX)
            // https://docs.microsoft.com/en-us/windows/win32/direct3d9/transforms#rotate

            BaseMatrix rotationX = RotateX(transform.Rotation.X);
            BaseMatrix rotationY = RotateX(transform.Rotation.Y);
            BaseMatrix rotationZ = RotateX(transform.Rotation.Z);

            // XYZ rotation = (((Z × Y) × X) × Vector3) or (Z×Y×X)×V
            var rotation = (rotationZ * rotationY);
            rotation *= rotationX;

            BaseMatrix translation = Translate(transform.Position);

            BaseMatrix scaling = Scale(transform.Scale);

            // Model Matrix = T × R × S (right to left order)
            return ((scaling * rotation) * translation);
        }
    }
}
