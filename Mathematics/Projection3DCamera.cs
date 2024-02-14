using System;
using System.Diagnostics;

namespace Mathematics
{
    /// <summary>
    ///     3D Projection
    /// </summary>
    public static class Projection3DCamera
    {
        private const double Rad = Math.PI / 180.0;

        //TODO
        // vector = vector * WorldMatrix
        // vector = matView * vector;
        // final = matProj * vector;

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
            var projection = ProjectionTo3DMatrix();

            var result = m1 * projection;
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
        ///     Orthographic projection to3 d.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <returns>Transformed Coordinates</returns>
        public static Vector3D OrthographicProjectionTo3D(Vector3D start)
        {
            double[,] matrix = { { start.X, start.Y, start.Z, 1 } };

            var m1 = new BaseMatrix(matrix);
            var projection = OrthographicProjectionTo3DMatrix();

            var result = m1 * projection;
            var x = result[0, 0];
            var y = result[0, 1];
            var z = result[0, 2];

            return new Vector3D(x, y, z);
        }

        /// <returns>
        ///     World Transformation
        ///     ModelViewProjection mvp = Projection * View * Model
        ///     Use LEFT-Handed rotation matrices (as seen in DirectX)
        ///     https://docs.microsoft.com/en-us/windows/win32/direct3d9/transforms#rotate
        ///     https://www.opengl-tutorial.org/beginners-tutorials/tutorial-3-matrices/#cumulating-transformations
        /// </returns>
        /// <summary>
        ///     Gets the model matrix.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <returns>The Model Matrix</returns>
        public static BaseMatrix ModelMatrix(Transform transform)
        {
            var rotationX = Projection3DConstants.RotateX(transform.Rotation.X);
            var rotationY = Projection3DConstants.RotateY(transform.Rotation.Y);
            var rotationZ = Projection3DConstants.RotateZ(transform.Rotation.Z);

            // XYZ rotation = (((Z × Y) × X) × Vector3) or (Z×Y×X)×V
            var rotation = rotationZ * rotationY * rotationX;

            Trace.WriteLine(rotation.ToString());

            var translation = Projection3DConstants.Translate(transform.Position);

            var scaling = Projection3DConstants.Scale(transform.Scale);

            // Model Matrix = T × R × S (right to left order)
            return scaling * rotation * translation;
        }

        /// <summary>
        ///     Projections the to3 d matrix.
        /// </summary>
        /// <returns>Projection Matrix</returns>
        private static BaseMatrix ProjectionTo3DMatrix()
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
        ///     Orthographic projection to3 d matrix.
        /// </summary>
        /// <returns>Projection Matrix</returns>
        private static BaseMatrix OrthographicProjectionTo3DMatrix()
        {
            double[,] translation =
            {
                { Projection3DRegister.A, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 1 }
            };
            return new BaseMatrix(translation);
        }

        public static BaseMatrix ViewCamera(int angle, Vector3D vCamera, Vector3D up)
        {
            //vec3d vTarget = { 1,2,3 };//{ 0,0,1 };
            var vTarget = new Vector3D(1, 2, 3);
            var vLookDir =
                vTarget; // vTarget.CrossProduct(Vector3D.UnitVector); // Matrix_MultiplyVector(angle, vTarget);
            //NYI todo
            var rotationCamera =  Projection3DConstants.RotateCamera(angle);
            //1,2,3,1
            vTarget = vCamera + vLookDir;
            //1,2,3,1

            var matCamera = PointAt(vCamera, vTarget, up);

            var matView = matCamera.Inverse();
            return matView;
        }

        /// <summary>
        ///     Converts Coordinates based on the Camera.
        ///     https://ksimek.github.io/2012/08/22/extrinsic/
        ///     https://github.com/OneLoneCoder/Javidx9/blob/master/ConsoleGameEngine/BiggerProjects/Engine3D/OneLoneCoder_olcEngine3D_Part3.cpp
        ///     https://www.youtube.com/watch?v=HXSuNxpCzdM
        /// </summary>
        /// <param name="position">Current Position, also known as vCamera.</param>
        /// <param name="target">Directional Vector, Point at.</param>
        /// <param name="up">Directional Vector, Z Axis.</param>
        /// <returns>matrix for Transforming the Coordinate</returns>
        private static BaseMatrix PointAt(Vector3D position, Vector3D target, Vector3D up)
        {
            var newForward = (target - position).Normalize();
            var a = newForward * (up * newForward);
            var newUp = (up - a).Normalize();
            var newRight = newUp.CrossProduct(newForward);

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
                [3, 0] = position.X,
                [3, 1] = position.Y,
                [3, 2] = position.Z,
                [3, 3] = position.W
            };
        }
    }
}
