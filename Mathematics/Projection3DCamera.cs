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
            double[,] matrix = {{start.X, start.Y, start.Z, 1}};

            var m1 = new BaseMatrix(matrix);
            var projection = Projection3DConstants.ProjectionTo3DMatrix();

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
            double[,] matrix = {{start.X, start.Y, start.Z, 1}};

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

            // XYZ rotation = (((Z × Y) × X) × Vector3D) or (Z×Y×X)×V
            var rotation = rotationZ * rotationY * rotationX;

            var translation = Projection3DConstants.Translate(transform.Translation);

            var scaling = Projection3DConstants.Scale(transform.Scale);

            // Model Matrix = T × R × S (right to left order)
            return scaling * rotation * translation;
        }

        /// <summary>
        ///     Orthographic projection to3 d matrix.
        /// </summary>
        /// <returns>Projection Matrix</returns>
        private static BaseMatrix OrthographicProjectionTo3DMatrix()
        {
            double[,] translation = {{Projection3DRegister.A, 0, 0, 0}, {0, 1, 0, 0}, {0, 0, 0, 0}, {0, 0, 0, 1}};
            return new BaseMatrix(translation);
        }

        /// <summary>
        ///     Generates the view from the Camera onto the world.
        /// </summary>
        /// <param name="transform">The transform object.</param>
        /// <returns>
        ///     transform
        ///     View on the Object from the Camera perspective
        /// </returns>
        internal static BaseMatrix ViewCamera(Transform transform)
        {
            var matCameraRot = Projection3DConstants.RotateCamera(transform.Angle);

            transform.VLookDir = transform.Target * matCameraRot;

            transform.Target = transform.Camera + transform.VLookDir;

            var matCamera = Projection3DConstants.PointAt(transform);

            return matCamera.Inverse();
        }

        internal static BaseMatrix OrbitCamera(Transform transform)
        {
            // LEFT-Handed Coordinate System

            // Rotation in X = Positive when 'looking down'
            // Rotation in Y = Positive when 'looking right'
            // Rotation in Z = Positive when 'tilting left'

            var toRad = (float)(Math.PI / 180.0f);

            var cosPitch = (float)Math.Cos(transform.Pitch * toRad);
            var sinPitch = (float)Math.Sin(transform.Pitch * toRad);

            var cosYaw = (float)Math.Cos(transform.Yaw * toRad);
            var sinYaw = (float)Math.Sin(transform.Yaw * toRad);

            transform.Right = new Vector3D(cosYaw, 0, -sinYaw);

            transform.Up = new Vector3D(sinYaw * sinPitch, cosPitch, cosYaw * sinPitch);

            transform.Forward = new Vector3D(sinYaw * cosPitch, -sinPitch, cosPitch * cosYaw);

            // The inverse camera's translation
            var transl = new Vector3D(-(transform.Right * transform.Position),
                -(transform.Up * transform.Position),
                -(transform.Forward * transform.Position));

            // Join rotation and translation in a single matrix
            // instead of calculating their multiplication
            double[,] viewMatrix =
            {
                {transform.Right.X, transform.Up.X, transform.Forward.X, 0}, {transform.Right.Y, transform.Up.Y, transform.Forward.Y, 0},
                {transform.Right.Z, transform.Up.Z, transform.Forward.Z, 0}, 
                {transl.X, transl.Y, transl.Z, 1}
            };

            return new BaseMatrix {Matrix = viewMatrix};
        }
    }
}
