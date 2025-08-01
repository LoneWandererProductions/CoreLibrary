﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngineTests
 * FILE:        LayeredImageContainerTests.cs
 * PURPOSE:     Your file purpose here
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RenderEngine;

namespace RenderEngineTests;

[TestClass]
public class LayeredImageContainerTests
{
    /// <summary>
    ///     Composites the merges layers with alpha.
    /// </summary>
    [TestMethod]
    public void CompositeMergesLayersWithAlpha()
    {
        const int width = 1;
        const int height = 1;

        var container = new LayeredImageContainer(width, height);

        var bottom = new UnmanagedImageBuffer(width, height);
        bottom.SetPixel(0, 0, 255, 0, 0, 255); // fully opaque blue
        container.AddLayer(bottom);

        var top = new UnmanagedImageBuffer(width, height);
        top.SetPixel(0, 0, 128, 255, 0, 0); // 50% transparent red
        container.AddLayer(top);

        var result = container.Composite();
        var pixel = result.BufferSpan.Slice(0, 4).ToArray();

        // Simple blend between blue and red (50% red alpha)
        // R: 50% of 255 = 127.5 => 127 or 128
        // G: 0
        // B: 50% of 255 = 127.5 => 127 or 128
        Assert.IsTrue(pixel[0] >= 127 && pixel[0] <= 128); // B
        Assert.AreEqual(0, pixel[1]); // G
        Assert.IsTrue(pixel[2] >= 127 && pixel[2] <= 128); // R
        Assert.AreEqual(255, pixel[3]); // A (fully opaque after blend)
    }

    /// <summary>
    ///     Adds the layer throws if size mismatch.
    /// </summary>
    [TestMethod]
    public void AddLayerThrowsIfSizeMismatch()
    {
        using var container = new LayeredImageContainer(2, 2);
        using var badLayer = new UnmanagedImageBuffer(1, 1);

        Assert.ThrowsException<ArgumentException>(() => container.AddLayer(badLayer));
    }

    /// <summary>
    ///     Adds the empty layer adds transparent layer.
    /// </summary>
    [TestMethod]
    public void AddEmptyLayerAddsTransparentLayer()
    {
        using var container = new LayeredImageContainer(2, 2);

        var layer = container.AddEmptyLayer();
        var span = layer.BufferSpan;

        foreach (var t in span)
        {
            Assert.AreEqual(0, t);
        }
    }

    /// <summary>
    ///     Composites the throws if no layers.
    /// </summary>
    [TestMethod]
    public void CompositeThrowsIfNoLayers()
    {
        using var container = new LayeredImageContainer(2, 2);
        Assert.ThrowsException<InvalidOperationException>(() => container.Composite());
    }
}
