namespace Mathematics
{
    /// <summary>
    /// Basic Controller for the World and the camera
    /// </summary>
    public sealed class Transform
    {
        /// <summary>
        /// Gets or sets the angle of the camera.
        /// </summary>
        /// <value>
        /// The angle in rad.
        /// </value>
        public double Angle { get; set; } = 0;

        /// <summary>
        /// Gets or sets the camera Vector.
        /// </summary>
        /// <value>
        /// The camera.
        /// </value>
        public Vector3D Camera { get; set; } = Vector3D.ZeroVector;

        /// <summary>
        /// Gets or sets up for the Camera.
        /// </summary>
        /// <value>
        /// Up.
        /// </value>
        public Vector3D Up { get; set; } = new Vector3D(0, 1, 0);

        /// <summary>
        /// Gets or sets the target Camera.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public Vector3D Target { get; set; } = new Vector3D(0,0,1);

        /// <summary>
        /// Gets or sets the look Vector direction.
        /// </summary>
        /// <value>
        /// The v look dir.
        /// </value>
        public Vector3D VLookDir { get; set; } = Vector3D.ZeroVector;

        /// <summary>
        /// Gets or sets the translation.
        /// </summary>
        /// <value>
        /// The translation.
        /// </value>
        public Vector3D Translation { get; set; } = Vector3D.UnitVector;

        /// <summary>
        /// Gets or sets the rotation.
        /// </summary>
        /// <value>
        /// The rotation.
        /// </value>
        public Vector3D Rotation { get; set; } = Vector3D.UnitVector;

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>
        /// The scale.
        /// </value>
        public Vector3D Scale { get; set; } = Vector3D.UnitVector;

        public void MoveWorld(Vector3D translation)
        {
            Translation += translation;
        }

        public void RotateWorld(Vector3D rotation)
        {
            Rotation += rotation;
        }

        public void RotateCameraLeft(double angle)
        {
            Angle += angle;
        }

        public void RotateCameraRight(double angle)
        {
            Angle -= angle;
        }

        public void UpCamera(double y)
        {
            Camera.Y += y;
        }

        public void DownCamera(double y)
        {
            Camera.Y -= y;
        }

        public void LeftCamera(double x)
        {
            Camera.X -= x;
        }

        public void RightCamera(double x)
        {
            Camera.X -= x;
        }

        public void LeftRotateCamera(double value)
        {
            var vForward = VLookDir * value;
            Camera += vForward;
        }

        public void RightRotateCamera(double value)
        {
            var vForward = VLookDir * value;
            Camera -= vForward;
        }
    }
}
