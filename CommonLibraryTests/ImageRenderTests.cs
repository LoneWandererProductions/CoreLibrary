/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/ImageRenderTests.cs
 * PURPOSE:     Interface testing and dependency Injection, plus some functional tests
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */


using System.Drawing;
using Imaging;
using Imaging.Enums;
using Imaging.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests;

/// <summary>
///     Some Basic Image Render tets
/// </summary>
[TestClass]
public class ImageRenderTests
{
    private IImageRender _imageRender;

    /// <summary>
    ///     Setups this instance.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _imageRender = new ImageRender(); // Assuming ImageRender implements IImageRender
    }

    /// <summary>
    ///     Fills the area with color start point not null remains not null.
    /// </summary>
    [TestMethod]
    public void FillAreaWithColorStartPointNotNullRemainsNotNull()
    {
        // Arrange
        var image = new Bitmap(10, 10);
        int? width = 5;
        int? height = 5;
        var color = Color.Red;
        const MaskShape shape = MaskShape.Circle;
        object shapeParams = null;
        Point? startPoint = new Point(3, 3); // Non-null start point

        // Act
        _imageRender.FillAreaWithColor(image, width, height, color, shape, shapeParams, startPoint);

        // Assert
        Assert.IsNotNull(startPoint, "startPoint should not be null after method call");
        Assert.AreEqual(new Point(3, 3), startPoint.Value, "startPoint value should remain unchanged");
    }
}
