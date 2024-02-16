namespace Mathematics
{
    public sealed class Transform
    {
        public double Angle { get; set; } = 0;
        public Vector3D Camera { get; set; } = Vector3D.ZeroVector;

        public Vector3D Translation { get; set; } = Vector3D.UnitVector;
        public Vector3D Rotation { get; set; } = Vector3D.UnitVector; // Degrees
        public Vector3D Scale { get; set; } = Vector3D.UnitVector;

        public void Move(Vector3D translation)
        {
            Translation += translation;
        }

        public void Rotate(Vector3D rotation)
        {
            Rotation += rotation;
        }
    }
}
