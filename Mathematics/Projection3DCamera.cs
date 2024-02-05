using System;

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
        ///     https://www.opengl-tutorial.org/beginners-tutorials/tutorial-3-matrices/#cumulating-transformations
        ///     ModelViewProjection mvp = Projection * View * Model
        ///     Use LEFT-Handed rotation matrices (as seen in DirectX)
        ///     https://docs.microsoft.com/en-us/windows/win32/direct3d9/transforms#rotate
        /// </returns>
        /// <summary>
        /// Gets the model matrix.
        /// </summary>
        /// <param name="transform">The transform.</param>
        /// <returns>The Model Matrix</returns>
        public static BaseMatrix ModelMatrix(Transform transform)
        {
            var rotationX = Projection3DConstants.RotateX(transform.Rotation.X);
            var rotationY = Projection3DConstants.RotateX(transform.Rotation.Y);
            var rotationZ = Projection3DConstants.RotateX(transform.Rotation.Z);

            // XYZ rotation = (((Z × Y) × X) × Vector3) or (Z×Y×X)×V
            var rotation = rotationZ * rotationY;
            rotation *= rotationX;

            var translation = Projection3DConstants.Translate(transform.Position);

            var scaling = Projection3DConstants.Scale(transform.Scale);

            // Model Matrix = T × R × S (right to left order)
            return (scaling * rotation) * translation;
        }

        /// <summary>
        ///     View matrix.
        ///     Uses the PointAt matrix
        ///     https://github.com/OneLoneCoder/Javidx9/blob/master/ConsoleGameEngine/BiggerProjects/Engine3D/OneLoneCoder_olcEngine3D_Part3.cpp
        ///     https://ksimek.github.io/2012/08/22/extrinsic/
        ///     https://github.com/OneLoneCoder/Javidx9/blob/master/ConsoleGameEngine/BiggerProjects/Engine3D/OneLoneCoder_olcEngine3D_Part3.cpp
        ///     https://www.opengl-tutorial.org/beginners-tutorials/tutorial-3-matrices/#the-model-view-and-projection-matrices
        ///     https://www.youtube.com/watch?v=HXSuNxpCzdM
        /// </summary>
        /// <returns>The View Matrix, aka the Camera</returns>
        public static BaseMatrix ViewMatrix(Transform transform)
        {
            var cosPitch = Math.Cos(transform.Pitch * Rad);
            var sinPitch = Math.Sin(transform.Pitch * Rad);

            var cosYaw = Math.Cos(transform.Yaw * Rad);
            var sinYaw = Math.Sin(transform.Yaw * Rad);

            transform.Right = new Vector3D(cosYaw, 0, -sinYaw);

            transform.Up = new Vector3D(sinYaw * sinPitch, cosPitch, cosYaw * sinPitch);

            transform.Forward = new Vector3D(sinYaw * cosPitch, -sinPitch, cosPitch * cosYaw);

            // The inverse camera's translation
            var transl = new Vector3D(-(transform.Right * transform.Position),
                -(transform.Up * transform.Position),
                -(transform.Forward * transform.Position));

            // Join rotation and translation in a single matrix
            // instead of calculating their multiplication
            var viewMatrix = new[,]
            {
                {transform.Right.X, transform.Up.X, transform.Forward.X, 0},
                {transform.Right.Y, transform.Up.Y, transform.Forward.Y, 0},
                {transform.Right.Z, transform.Up.Z, transform.Forward.Z, 0},
                {transl.X, transl.Y, transl.Z, 1}
            };

            return new BaseMatrix { Matrix = viewMatrix };
        }

        /// <summary>
        ///     Projections the to3 d matrix.
        /// </summary>
        /// <returns>Projection Matrix</returns>
        private static BaseMatrix ProjectionTo3DMatrix()
        {
            double[,] translation =
            {
                {Projection3DRegister.A * Projection3DRegister.F, 0, 0, 0}, {0, Projection3DRegister.F, 0, 0},
                {0, 0, Projection3DRegister.Q, 1}, {0, 0, -Projection3DRegister.ZNear * Projection3DRegister.Q, 0}
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
            double[,] translation = { { Projection3DRegister.A, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 1 } };
            return new BaseMatrix(translation);
        }

        public static BaseMatrix ViewCamera(int angle, Vector3D position, Vector3D target, Vector3D up)
        {
            var matCamera = PointAt(position, target, up);

            return matCamera.Inverse();
        }

        /// <summary>
        ///     Converts Coordinates based on the Camera.
        ///     https://ksimek.github.io/2012/08/22/extrinsic/
        ///     https://github.com/OneLoneCoder/Javidx9/blob/master/ConsoleGameEngine/BiggerProjects/Engine3D/OneLoneCoder_olcEngine3D_Part3.cpp
        ///     https://www.youtube.com/watch?v=HXSuNxpCzdM
        /// </summary>
        /// <param name="position">Current Position.</param>
        /// <param name="target">Directional Vector, Point at.</param>
        /// <param name="up">Directional Vector, Z Axis.</param>
        /// <returns>matrix for Transforming the Coordinate</returns>
        public static BaseMatrix PointAt(Vector3D position, Vector3D target, Vector3D up)
        {
            var newForward = target - position;
            var a = newForward *(up * newForward);
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
                    [3, 0] = position.X,
                    [3, 1] = position.Y,
                    [3, 2] = position.Z,
                    [3, 3] = 1.0f
                }
            };
        }
    }
}
