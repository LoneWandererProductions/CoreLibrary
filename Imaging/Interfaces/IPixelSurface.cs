using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Imaging.Interfaces
{
    public interface IPixelSurface : IDisposable
    {
        int Width { get; }
        int Height { get; }

        Pixel32[] Bits { get; }

        void SetPixel(int x, int y, System.Drawing.Color color);

        void SetPixelsSimd(IEnumerable<(int x, int y, System.Drawing.Color color)> pixels);

        void BlendInt(uint[] src);

        Pixel32 GetPixel32(int x, int y);
    }

}
