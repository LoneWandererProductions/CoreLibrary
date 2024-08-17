using System.Drawing;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Imaging;

public class ARGBImage
{
    private int[] _argbValues;
    private int _width;
    private int _height;

    public ARGBImage(int width, int height)
    {
        _width = width;
        _height = height;
        _argbValues = new int[width * height];
    }

    public int Width => _width;
    public int Height => _height;

    private int GetIndex(int x, int y)
    {
        return y * _width + x;
    }

    public int GetPixel(int x, int y)
    {
        return _argbValues[GetIndex(x, y)];
    }

    public void SetPixel(int x, int y, int argb)
    {
        _argbValues[GetIndex(x, y)] = argb;
    }

    public void SetPixelsSimd(IEnumerable<(int x, int y, int argb)> pixels)
    {
        var pixelArray = pixels.ToArray();
        var count = pixelArray.Length;
        var vectorCount = Vector<int>.Count;

        for (int i = 0; i < count; i += vectorCount)
        {
            var argbValues = new int[vectorCount];
            var indices = new int[vectorCount];

            for (int j = 0; j < vectorCount && i + j < count; j++)
            {
                var (x, y, argb) = pixelArray[i + j];
                indices[j] = GetIndex(x, y);
                argbValues[j] = argb;
            }

            var indexVector = new Vector<int>(indices);
            var argbVector = new Vector<int>(argbValues);

            for (int j = 0; j < vectorCount && i + j < count; j++)
            {
                _argbValues[indexVector[j]] = argbVector[j];
            }
        }
    }

    public Bitmap ToBitmap()
    {
        Bitmap bitmap = new Bitmap(_width, _height);
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                bitmap.SetPixel(x, y, Color.FromArgb(GetPixel(x, y)));
            }
        }
        return bitmap;
    }

    public void SaveToFile(string filePath)
    {
        using (var bitmap = ToBitmap())
        {
            bitmap.Save(filePath, ImageFormat.Png);
        }
    }
}
