using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenderEngine
{
    internal class Skybox
    {
        public string[,] NorthWall { get; set; }
        public string[,] SouthWall { get; set; }
        public string[,] EastWall { get; set; }
        public string[,] WestWall { get; set; }
        public string[,] TopLayer { get; set; }
        public string[,] BottomLayer { get; set; }

        public Skybox(int rows, int cols)
        {
            NorthWall = new string[rows, cols];
            SouthWall = new string[rows, cols];
            EastWall = new string[rows, cols];
            WestWall = new string[rows, cols];
            TopLayer = new string[rows, cols];
            BottomLayer = new string[rows, cols];
        }
    }
}
