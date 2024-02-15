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

        /// <summary>
        /// Generates the view from the Camera onto the world.
        /// </summary>
        /// <param name="angle">Camera Angle</param>
        /// <param name="vCamera">Position of the Camera</param>
        /// <param name="up">Pitch of the Camera</param>
        /// <returns>View on the Object from the Camera perspective</returns>
        public static BaseMatrix ViewCamera(double angle, Vector3D vCamera, Vector3D up)
        {
            //NYI TODO, activate and change Test results          
            //var vUp = new Vector3D(0,1,0);
		    var vTarget = new Vector3D(0,0,1);
            var matCameraRot = Projection3DConstants.RotateCamera(angle);
            var vLookDir = vTarget * matCameraRot;
            vTarget = vCamera + vLookDir;

            var matCamera = Projection3DConstants.PointAt(vCamera, vTarget, up);

            var matView = matCamera.Inverse();
            return matView;
        }
    }
}
