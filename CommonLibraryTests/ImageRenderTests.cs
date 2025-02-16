﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging
 * FILE:        Imaging/ImageRenderTests.cs
 * PURPOSE:     Interface testing and dependency Injection, plus some functional tests
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */


using System.Drawing;
using Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    [TestClass]
    public class ImageRenderTests
    {
        private IImageRender _imageRender;

        [TestInitialize]
        public void Setup()
        {
            _imageRender = new ImageRender(); // Assuming ImageRender implements IImageRender
        }

        [TestMethod]
        public void FillAreaWithColor_StartPointNotNull_RemainsNotNull()
        {
            // Arrange
            var image = new Bitmap(10, 10);
            int? width = 5;
            int? height = 5;
            Color color = Color.Red;
            const MaskShape shape = MaskShape.Circle;
            object shapeParams = null;
            Point? startPoint = new Point(3, 3); // Non-null start point

            // Act
            var result = _imageRender.FillAreaWithColor(image, width, height, color, shape, shapeParams, startPoint);

            // Assert
            Assert.IsNotNull(startPoint, "startPoint should not be null after method call");
            Assert.AreEqual(new Point(3, 3), startPoint.Value, "startPoint value should remain unchanged");
        }
    }
}
