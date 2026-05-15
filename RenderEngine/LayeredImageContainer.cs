/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngine
 * FILE:        LayeredImageContainer.cs
 * PURPOSE:     Layered Image Container to overlay Images in a quick way.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;

namespace RenderEngine;

/// <inheritdoc />
/// <summary>
///     Provides a container for multiple image layers stored as unmanaged buffers,
///     allowing fast compositing and alpha blending of layered images.
/// </summary>
public sealed class LayeredImageContainer : IDisposable
{
    /// <summary>
    ///     The height
    /// </summary>
    private readonly int _height;

    /// <summary>
    ///     The layers
    /// </summary>
    private readonly List<UnmanagedImageBuffer> _layers = new();

    /// <summary>
    ///     The width
    /// </summary>
    private readonly int _width;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LayeredImageContainer" /> class
    ///     with the specified width and height.
    /// </summary>
    /// <param name="width">The width of the container and all layers.</param>
    /// <param name="height">The height of the container and all layers.</param>
    public LayeredImageContainer(int width, int height)
    {
        _width = width;
        _height = height;
    }

    /// <summary>
    ///     Gets the layer count.
    /// </summary>
    /// <value>
    ///     The layer count.
    /// </value>
    public int LayerCount => _layers.Count;

    /// <inheritdoc />
    /// <summary>
    ///     Releases all resources used by the <see cref="T:RenderEngine.LayeredImageContainer" />,
    ///     including all contained <see cref="T:RenderEngine.UnmanagedImageBuffer" /> layers.
    /// </summary>
    public void Dispose()
    {
        foreach (var layer in _layers)
        {
            layer.Dispose();
        }

        _layers.Clear();
    }

    /// <summary>
    ///     Adds an existing unmanaged image buffer as a layer.
    /// </summary>
    /// <param name="layer">The <see cref="UnmanagedImageBuffer" /> to add as a layer.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown if the layer's dimensions do not match the container's size.
    /// </exception>
    public void AddLayer(UnmanagedImageBuffer layer)
    {
        if (layer.Width != _width || layer.Height != _height)
        {
            throw new ArgumentException(RenderResource.ErrorLayerSize);
        }

        _layers.Add(layer);
    }

    /// <summary>
    ///     Adds a new empty (fully transparent) layer to the container.
    /// </summary>
    /// <returns>
    ///     The newly created <see cref="UnmanagedImageBuffer" /> representing the blank layer.
    /// </returns>
    public UnmanagedImageBuffer AddEmptyLayer()
    {
        var newLayer = new UnmanagedImageBuffer(_width, _height);
        newLayer.Clear(System.Drawing.Color.FromArgb(0, 0, 0, 0)); // transparent clear
        _layers.Add(newLayer);
        return newLayer;
    }

    /// <summary>
    ///     Composites all layers in the container using alpha blending,
    ///     producing a single combined <see cref="UnmanagedImageBuffer" />.
    /// </summary>
    /// <returns>
    ///     A new <see cref="UnmanagedImageBuffer" /> representing the composited image.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if no layers exist to composite.</exception>
    public UnmanagedImageBuffer Composite()
    {
        if (_layers.Count == 0)
        {
            throw new InvalidOperationException(RenderResource.ErrorNoLayers);
        }

        var result = new UnmanagedImageBuffer(_width, _height);
        result.Clear(System.Drawing.Color.FromArgb(0, 0, 0, 0)); // start transparent

        var targetSpan = result.BufferSpan;

        foreach (var layer in _layers)
        {
            AlphaBlend(targetSpan, layer.BufferSpan);
        }

        return result;
    }

    /// <summary>
    ///     Composites the layers.
    /// </summary>
    /// <param name="layerIndices">The layer indices.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">layerIndices</exception>
    public UnmanagedImageBuffer CompositeLayers(IEnumerable<int> layerIndices)
    {
        var result = new UnmanagedImageBuffer(_width, _height);
        result.Clear(System.Drawing.Color.FromArgb(0, 0, 0, 0)); // start transparent

        var targetSpan = result.BufferSpan;
        foreach (var index in layerIndices)
        {
            if (index < 0 || index >= _layers.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(layerIndices),
                    string.Format(RenderResource.ErrorInvalidLayerIndex, index));
            }

            var layerSpan = _layers[index].BufferSpan;
            AlphaBlend(targetSpan, layerSpan);
        }

        return result;
    }

    /// <summary>
    ///     Inserts the layer.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="layer">The layer.</param>
    /// <exception cref="ArgumentException"></exception>
    public void InsertLayer(int index, UnmanagedImageBuffer layer)
    {
        if (layer.Width != _width || layer.Height != _height)
        {
            throw new ArgumentException(RenderResource.ErrorLayerSizeMismatch);
        }

        _layers.Insert(index, layer);
    }

    /// <summary>
    ///     Removes the layer.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <exception cref="ArgumentOutOfRangeException">index</exception>
    public void RemoveLayer(int index)
    {
        if (index < 0 || index >= _layers.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index),
                string.Format(RenderResource.ErrorInvalidLayerIndex, index));
        }

        _layers.RemoveAt(index);
    }

    /// <summary>
    /// Performs alpha blending using highly optimized integer math and pointer arithmetic.
    /// Implements Porter-Duff "Source Over" without floating-point overhead.
    /// </summary>
    /// <param name="baseSpan">The base span.</param>
    /// <param name="overlaySpan">The overlay span.</param>
    private static unsafe void AlphaBlend(Span<byte> baseSpan, Span<byte> overlaySpan)
    {
        var length = baseSpan.Length;

        // Pin the spans in memory so we can use raw pointers for maximum speed
        fixed (byte* pBase = baseSpan)
        fixed (byte* pOverlay = overlaySpan)
        {
            for (var i = 0; i < length; i += 4)
            {
                int srcA = pOverlay[i + 3];

                // 1. Fast Path: Fully Transparent Overlay
                if (srcA == 0) continue;

                // 2. Fast Path: Fully Opaque Overlay (Just overwrite the base pixel)
                if (srcA == 255)
                {
                    // Cast to an integer pointer to copy all 4 bytes (BGRA) in a single CPU tick
                    *(int*)(pBase + i) = *(int*)(pOverlay + i);
                    continue;
                }

                // 3. Integer Math Porter-Duff Compositing
                int dstA = pBase[i + 3];
                var invSrcA = 255 - srcA;

                // Calculate the output alpha scaled by 255
                var outA = (srcA * 255) + (dstA * invSrcA);
                if (outA == 0) continue;

                // Calculate color channels (Numerator / Denominator)
                // Maximum value of numerator is ~33 million, which fits perfectly inside a standard 32-bit int
                pBase[i] = (byte)(((pOverlay[i] * srcA * 255) + (pBase[i] * dstA * invSrcA)) / outA); // Blue
                pBase[i + 1] =
                    (byte)(((pOverlay[i + 1] * srcA * 255) + (pBase[i + 1] * dstA * invSrcA)) / outA); // Green
                pBase[i + 2] = (byte)(((pOverlay[i + 2] * srcA * 255) + (pBase[i + 2] * dstA * invSrcA)) / outA); // Red

                pBase[i + 3] = (byte)(outA / 255); // Alpha
            }
        }
    }
}
