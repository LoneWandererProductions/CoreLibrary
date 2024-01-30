namespace Mathematics
{
    public class Transform
    {
        public Vector3D Position { get; set; } = Vector3D.UnitVector;
        public Vector3D Rotation { get; set; } = Vector3D.UnitVector; // Degrees
        public Vector3D Scale { get; set; } = Vector3D.UnitVector;

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
