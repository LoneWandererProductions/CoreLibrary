/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/Projection3D.cs
 * PURPOSE:     3D Matrix transformations
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCES:     https://learn.microsoft.com/en-us/windows/win32/direct3d9/transforms
 *              https://www.brainvoyager.com/bv/doc/UsersGuide/CoordsAndTransforms/SpatialTransformationMatrices.html
 */

// ReSharper disable MemberCanBeInternal

using System;

namespace Mathematics
{
    /// <summary>
    ///     Helper Class that handles some 3D Transformations
    /// </summary>
    public static class Projection3D
    {
        private const double Rad = Math.PI / 180.0;

        /// <summary>
        ///     Rotates the x axis.
        ///     Source: https://learn.microsoft.com/en-us/windows/win32/direct3d9/transforms
        ///     https://en.wikipedia.org/wiki/Rotation_matrix
        ///     Use DirectX
        /// </summary>
        /// <param name="vector">The start vector.</param>
        /// <param name="angleD">The Angle degree.</param>
        /// <returns>Rotated Vector</returns>
        public static BaseMatrix RotateX(Vector3D vector, int angleD)
        {
            double[,] matrix = { { vector.X, vector.Y, vector.Z, 1 } };

            var m1 = new BaseMatrix { Matrix = matrix };

            var m2 = Projection3DConstants.RotateX(angleD);

            return m1 * m2;
        }

        /// <summary>
        ///     Rotates the y axis.
        ///     Source: https://learn.microsoft.com/en-us/windows/win32/direct3d9/transforms
        ///     https://en.wikipedia.org/wiki/Rotation_matrix
        ///     Use DirectX
        /// </summary>
        /// <param name="vector">The start vector.</param>
        /// <param name="angleD">The Angle degree.</param>
        /// <returns>Rotated Vector</returns>
        public static BaseMatrix RotateY(Vector3D vector, int angleD)
        {
            double[,] matrix = { { vector.X, vector.Y, vector.Z, 1 } };

            var m1 = new BaseMatrix { Matrix = matrix };

            var m2 = Projection3DConstants.RotateY(angleD);

            return m1 * m2;
        }

        /// <summary>
        ///     Rotates the z axis.
        ///     Source: https://learn.microsoft.com/en-us/windows/win32/direct3d9/transforms
        ///     https://en.wikipedia.org/wiki/Rotation_matrix
        ///     https://en.wikipedia.org/wiki/Rotation_formalisms_in_three_dimensions
        ///     Use DirectX
        /// </summary>
        /// <param name="vector">The start vector.</param>
        /// <param name="angleD">The Angle degree.</param>
        /// <returns>Rotated Vector</returns>
        public static BaseMatrix RotateZ(Vector3D vector, int angleD)
        {
            double[,] matrix = { { vector.X, vector.Y, vector.Z, 1 } };

            var m1 = new BaseMatrix { Matrix = matrix };

            var m2 = Projection3DConstants.RotateZ(angleD);

            return m1 * m2;
        }

        /// <summary>
        ///     Scales the specified start.
        ///     Source: https://learn.microsoft.com/en-us/windows/win32/direct3d9/transforms
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="value">The value.</param>
        /// <returns>Scale Matrix</returns>
        public static BaseMatrix Scale(Vector3D start, int value)
        {
            double[,] matrix = { { start.X, start.Y, start.Z, 1 } };

            var m1 = new BaseMatrix(matrix);

            var m2 = Projection3DConstants.Scale(value);

            return m1 * m2;
        }

        /// <summary>
        ///     Scales the specified start.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="one">The x value.</param>
        /// <param name="two">The y value.</param>
        /// <param name="three">The z value.</param>
        /// <returns>Translation Matrix</returns>
        public static BaseMatrix Scale(Vector3D start, double one, double two, double three)
        {
            double[,] matrix = { { start.X, start.Y, start.Z, 1 } };

            var m1 = new BaseMatrix(matrix);

            var m2 = Projection3DConstants.Scale(one, two, three);

            return m1 * m2;
        }

        /// <summary>
        ///     Translates the specified start.
        ///     Source: https://learn.microsoft.com/en-us/windows/win32/direct3d9/transforms
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>Transplanted Vector</returns>
        public static BaseMatrix Translate(Vector3D start, Vector3D end)
        {
            double[,] matrix = { { end.X, end.Y, end.Z, 1 } };

            var m1 = new BaseMatrix { Matrix = matrix };

            var m2 = Projection3DConstants.Translate(start);

            return m1 * m2;
        }

        /// <summary>
        ///     Gets the vector.
        /// </summary>
        /// <param name="matrix">The matrix.</param>
        /// <returns>Vector from Matrix</returns>
        public static Vector3D GetVector(BaseMatrix matrix)
        {
            return new Vector3D(matrix[0, 0], matrix[0, 1], matrix[0, 2]);
        }
    }
}
