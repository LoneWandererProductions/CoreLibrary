using System;

namespace PlayGround
{
    internal readonly struct MovePoint
    {
        public int X { get; }
        public int Y { get; }

        public MovePoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is MovePoint point && X == point.X && Y == point.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}
