namespace LightVector
{
    public class ShearTransform : Transform
    {
        public double ShearX { get; }
        public double ShearY { get; }

        public ShearTransform(double shearX, double shearY)
        {
            ShearX = shearX;
            ShearY = shearY;
        }
    }

}
