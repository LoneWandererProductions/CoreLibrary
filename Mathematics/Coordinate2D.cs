/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/Coordinate2D.cs
 * PURPOSE:     A more clever way to handle some 2D coordinate Stuff
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

namespace Mathematics
{
    public class Coordinate2D
    {
        public Coordinate2D(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Coordinate2D(int x, int y, int width)
        {
            X = x;
            Y = y;
            Id = CalculateId(x,y, width);
        }

        public Coordinate2D()
        {
        }

        public static Coordinate2D GetInstance(int id, int width)
        {
            return new Coordinate2D {X = IdToX(id, width), Y = IdToY(id, width)};
        }

        public int Id { get; private set; }

        public int Y { get; set; }

        public int X { get; set; }

        private static int CalculateId(int x, int y, int width)
        {
            return (y * width) + x;
        }

        private static int IdToX(int masterId, int width)
        {
            return masterId % width;
        }

        private static int IdToY(int masterId, int width)
        {
            return masterId / width;
        }
    }
}
