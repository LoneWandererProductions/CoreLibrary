using System;
using System.Collections.Generic;
using System.Numerics;

namespace RenderEngine
{
    // Layered image container:
    public class LayeredImageContainer : IDisposable
    {
        private readonly List<UnmanagedImageBuffer> _layers = new();
        private readonly int _width;
        private readonly int _height;

        public LayeredImageContainer(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public void AddLayer(UnmanagedImageBuffer layer)
        {
            if (layer.Width != _width || layer.Height != _height)
                throw new ArgumentException("Layer size does not match container size.");

            _layers.Add(layer);
        }

        // New method to add a blank layer (all transparent)
        public UnmanagedImageBuffer AddEmptyLayer()
        {
            var newLayer = new UnmanagedImageBuffer(_width, _height);
            newLayer.Clear(0, 0, 0, 0); // transparent clear
            _layers.Add(newLayer);
            return newLayer;
        }

        public UnmanagedImageBuffer Composite()
        {
            if (_layers.Count == 0)
                throw new InvalidOperationException("No layers to composite.");

            var result = new UnmanagedImageBuffer(_width, _height);
            result.Clear(0, 0, 0, 0); // start transparent

            var targetSpan = result.BufferSpan;

            foreach (var layer in _layers)
            {
                AlphaBlend(targetSpan, layer.BufferSpan);
            }

            return result;
        }

        private static void AlphaBlend(Span<byte> baseSpan, Span<byte> overlaySpan)
        {
            int length = baseSpan.Length;
            int vectorSize = Vector<byte>.Count;
            int bytesPerPixel = 4;

            // Process per pixel (4 bytes at a time)
            for (int i = 0; i <= length - vectorSize; i += vectorSize)
            {
                Vector<byte> baseVec = new(baseSpan.Slice(i, vectorSize));
                Vector<byte> overVec = new(overlaySpan.Slice(i, vectorSize));

                Span<byte> blendedBytes = stackalloc byte[vectorSize];

                for (int j = 0; j < vectorSize; j += 4)
                {
                    byte ob = overVec[j];
                    byte og = overVec[j + 1];
                    byte orr = overVec[j + 2];
                    byte oa = overVec[j + 3];

                    float alpha = oa / 255f;

                    if (oa == 0)
                    {
                        blendedBytes[j] = baseVec[j];
                        blendedBytes[j + 1] = baseVec[j + 1];
                        blendedBytes[j + 2] = baseVec[j + 2];
                        blendedBytes[j + 3] = baseVec[j + 3];
                        continue;
                    }

                    blendedBytes[j] = (byte)(ob * alpha + baseVec[j] * (1 - alpha));
                    blendedBytes[j + 1] = (byte)(og * alpha + baseVec[j + 1] * (1 - alpha));
                    blendedBytes[j + 2] = (byte)(orr * alpha + baseVec[j + 2] * (1 - alpha));
                    blendedBytes[j + 3] = 255; // full opacity
                }

                blendedBytes.CopyTo(baseSpan.Slice(i, vectorSize));
            }

            // Handle remaining pixels
            for (int i = (length / vectorSize) * vectorSize; i < length; i += bytesPerPixel)
            {
                byte ob = overlaySpan[i];
                byte og = overlaySpan[i + 1];
                byte orr = overlaySpan[i + 2];
                byte oa = overlaySpan[i + 3];

                float alpha = oa / 255f;

                if (oa == 0) continue;

                baseSpan[i] = (byte)(ob * alpha + baseSpan[i] * (1 - alpha));
                baseSpan[i + 1] = (byte)(og * alpha + baseSpan[i + 1] * (1 - alpha));
                baseSpan[i + 2] = (byte)(orr * alpha + baseSpan[i + 2] * (1 - alpha));
                baseSpan[i + 3] = 255;
            }
        }

        public void Dispose()
        {
            foreach (var layer in _layers)
            {
                layer.Dispose();
            }
            _layers.Clear();
        }
    }

}
