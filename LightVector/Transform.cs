namespace LightVector
{
    public abstract class Transform
    {
        // Base class for all transformations (e.g., scaling, rotating, translating)
    }

    public class ScaleTransform : Transform
    {
        public float ScaleX { get; }
        public float ScaleY { get; }

        public ScaleTransform(float scaleX, float scaleY)
        {
            ScaleX = scaleX;
            ScaleY = scaleY;
        }
    }

    public class RotateTransform : Transform
    {
        public double Angle { get; }

        public RotateTransform(double angle)
        {
            Angle = angle;
        }
    }

    public class TranslateTransform : Transform
    {
        public double X { get; }
        public double Y { get; }

        public TranslateTransform(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

}
