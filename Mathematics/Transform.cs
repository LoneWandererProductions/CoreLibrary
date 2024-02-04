namespace Mathematics
{
    public class Transform
    {
        public double Yaw
        {
            get;
            set;
        }

        public double Pitch { get; set; }

        public Vector3D Position { get; set; } = Vector3D.UnitVector;
        public Vector3D Rotation { get; set; } = Vector3D.UnitVector; // Degrees
        public Vector3D Scale { get; set; } = Vector3D.UnitVector;
        public Vector3D Right { get; set; }
        public Vector3D Up { get; set; }
        public Vector3D Forward { get; set; }

        public void Move(Vector3D translation)
        {
            Position += translation;
        }

        public void Rotate(Vector3D rotation)
        {
            Rotation += rotation;
        }
    }
}
