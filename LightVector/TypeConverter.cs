using System.Windows;
using Mathematics;

namespace LightVector
{
    internal static class TypeConverter
    {
        internal static Point ToPoint(this Vector3D vector)
        {
            return new Point {X = vector.X, Y = vector.Y};
        }

        internal static Point ToPointR(this Vector3D vector)
        {
            return new Point { X = vector.RoundedX, Y = vector.RoundedY };
        }

        internal static Point ToPoint(this Vector2D vector)
        {
            return new Point { X = vector.X, Y = vector.Y };
        }

        internal static Point ToPointR(this Vector2D vector)
        {
            return new Point { X = vector.RoundedX, Y = vector.RoundedY };
        }

        internal static Vector2D ToPVector2D(this Point point)
        {
            return new Vector2D { X = point.X, Y = point.Y };
        }

        internal static System.Drawing.Point ToPoint(this Point point)
        {
            return new System.Drawing.Point { X = (int)point.X, Y = (int)point.Y };
        }

        internal static Point ToPoint(this System.Drawing.Point point)
        {
            return new Point { X = point.X, Y = point.Y };
        }
    }
}