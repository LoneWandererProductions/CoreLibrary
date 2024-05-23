using System;

namespace PlayGround
{
    internal class PathNode
    {
        public int X { get; }
        public int Y { get; }
        public int G { get; set; } // Cost from start to current node
        public int H { get; set; } // Estimated cost from current node to end node
        public int F => G + H; // Total cost
        public PathNode Parent { get; set; }

        public PathNode(int x, int y)
        {
            X = x;
            Y = y;
            G = int.MaxValue;
            H = 0;
            Parent = null;
        }

        public override bool Equals(object obj)
        {
            return obj is PathNode node && X == node.X && Y == node.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}
