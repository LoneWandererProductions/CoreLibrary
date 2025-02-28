namespace LightVector
{
    public class Transform
    {
        // Base class for all transformations (e.g., scaling, rotating, translating)
    }

    public sealed class ScaleTransform : Transform
    {
        public float ScaleX { get; }
        public float ScaleY { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScaleTransform"/> class.
        /// </summary>
        /// <param name="scaleX">The scale x.</param>
        /// <param name="scaleY">The scale y.</param>
        public ScaleTransform(float scaleX, float scaleY)
        {
            ScaleX = scaleX;
            ScaleY = scaleY;
        }
    }

    public sealed class RotateTransform : Transform
    {
        public double Angle { get; }

        public RotateTransform(double angle)
        {
            Angle = angle;
        }
    }

    public sealed class TranslateTransform : Transform
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
