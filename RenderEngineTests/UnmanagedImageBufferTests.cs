/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     RenderEngineTests
 * FILE:        UnmanagedImageBufferTests.cs
 * PURPOSE:     Tests for UnmanagedImageBuffer.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RenderEngine;

namespace RenderEngineTests;

/// <summary>
/// Some basic tests for my Unmanaged Buffer Image Format
/// </summary>
[TestClass]
public class UnmanagedImageBufferTests
{
    /// <summary>
    /// Creates the size of the buffer initializes correct.
    /// </summary>
    [TestMethod]
    public void CreateBufferInitializesCorrectSize()
    {
        using var buffer = new UnmanagedImageBuffer(10, 10);

        Assert.AreEqual(10, buffer.Width);
        Assert.AreEqual(10, buffer.Height);
        Assert.AreEqual(400, buffer.BufferSpan.Length); // 10 * 10 * 4
    }

    /// <summary>
    /// Sets the pixel sets correct values.
    /// </summary>
    [TestMethod]
    public void SetPixelSetsCorrectValues()
    {
        using var buffer = new UnmanagedImageBuffer(2, 2);
        buffer.SetPixel(1, 1, 100, 150, 200); // R,G,B,A

        const int offset = (1 + (1 * 2)) * 4;
        var span = buffer.BufferSpan;

        Assert.AreEqual(200, span[offset]); // B
        Assert.AreEqual(150, span[offset + 1]); // G
        Assert.AreEqual(100, span[offset + 2]); // R
        Assert.AreEqual(255, span[offset + 3]); // A
    }

    /// <summary>
    /// Clears the color of the fills buffer with.
    /// </summary>
    [TestMethod]
    public void ClearFillsBufferWithColor()
    {
        using var buffer = new UnmanagedImageBuffer(2, 2);

        ImagePrimitives.Clear(buffer, 128, 10, 20, 30); // A,R,G,B

        var span = buffer.BufferSpan;
        for (var i = 0; i < span.Length; i += 4)
        {
            Assert.AreEqual(30, span[i]); // B
            Assert.AreEqual(20, span[i + 1]); // G
            Assert.AreEqual(10, span[i + 2]); // R
            Assert.AreEqual(128, span[i + 3]); // A
        }
    }

    /// <summary>
    /// Applies the changes updates only targeted pixels.
    /// </summary>
    [TestMethod]
    public void ApplyChangesUpdatesOnlyTargetedPixels()
    {
        using var buffer = new UnmanagedImageBuffer(2, 2);
        ImagePrimitives.Clear(buffer, 0, 0, 0, 0);

        var changes = new (int x, int y, uint bgra)[]
        {
            (0, 0, 0xFF112233), // A=0xFF, R=0x11, G=0x22, B=0x33
            (1, 1, 0x80123456) // A=0x80, R=0x12, G=0x34, B=0x56
        };

        buffer.ApplyChanges(changes);

        var span = buffer.BufferSpan;

        // Check pixel (0,0)
        Assert.AreEqual(0x33, span[0]); // B
        Assert.AreEqual(0x22, span[1]); // G
        Assert.AreEqual(0x11, span[2]); // R
        Assert.AreEqual(0xFF, span[3]); // A

        // Check pixel (1,1)
        const int offset = (1 + (1 * 2)) * 4;
        Assert.AreEqual(0x56, span[offset]);
        Assert.AreEqual(0x34, span[offset + 1]);
        Assert.AreEqual(0x12, span[offset + 2]);
        Assert.AreEqual(0x80, span[offset + 3]);
    }

    /// <summary>
    /// Replaces the buffer replaces all pixels.
    /// </summary>
    [TestMethod]
    public void ReplaceBufferReplacesAllPixels()
    {
        using var buffer = new UnmanagedImageBuffer(2, 2);

        var newData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

        buffer.ReplaceBuffer(newData);

        var span = buffer.BufferSpan;

        for (var i = 0; i < newData.Length; i++)
        {
            Assert.AreEqual(newData[i], span[i]);
        }
    }

    /// <summary>
    /// Gets the pixel span returns correct span.
    /// </summary>
    [TestMethod]
    public void GetPixelSpanReturnsCorrectSpan()
    {
        using var buffer = new UnmanagedImageBuffer(4, 1);
        ImagePrimitives.Clear(buffer, 255, 1, 2, 3);

        var span = buffer.GetPixelSpan(1, 0, 2);
        Assert.AreEqual(8, span.Length);
    }
}
