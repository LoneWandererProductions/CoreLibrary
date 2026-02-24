/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGuiTests
 * FILE:        WpfVectorRendererTests.cs
 * PURPOSE:     Basic tests for the WpfVectorRenderer, especially to catch the "Double Offset" bug where the line's internal coordinates are incorrectly offset by the StartCoordinates.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */


using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using NUnit.Framework;
using System.Threading;
using System.Linq;
using LightVector;
using LightVector.Enums;

namespace CommonLibraryGuiTests
{
    /// <summary>
    /// Simple tests for the WpfVectorRenderer, especially to catch the "Double Offset" bug where the line's internal coordinates are incorrectly offset by the StartCoordinates.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class WpfVectorRendererTests
    {
        /// <summary>
        /// Renders to container verify normalization.
        /// </summary>
        [Test]
        public void RenderToContainer_VerifyNormalization()
        {
            // Arrange
            var startPoint = new Point(50, 50);
            var line = new LineObject { Direction = new System.Numerics.Vector2(10, 10), Thickness = 2 };
            var saveObj = new SaveObject { Graphic = line, Type = GraphicTypes.Line, StartCoordinates = startPoint };

            var renderer = new WpfVectorRenderer(new List<SaveObject> { saveObj });

            // Act
            var canvas = (Canvas)renderer.RenderToContainer();
            var renderedLine = canvas.Children.OfType<Line>().First();

            // Assert
            // Because of normalization, the line is shifted. 
            // It should now be at 'padding' (Thickness / 2)
            Assert.That(Canvas.GetLeft(renderedLine), Is.EqualTo(1.0d), "Line should be shifted to the edge of the canvas.");
            Assert.That(renderedLine.X1, Is.EqualTo(0), "Internal offset must remain 0.");
        }

        /// <summary>
        /// Renders to image verify dimensions match bounds.
        /// </summary>
        [Test]
        public void RenderToImage_VerifyDimensionsMatchBounds()
        {
            // Arrange
            var circle = new CircleObject { Radius = 100, Thickness = 10 };
            var saveObj = new SaveObject
            {
                Graphic = circle,
                Type = GraphicTypes.Circle,
                StartCoordinates = new Point(0, 0),
                Layer = 0
            };

            var renderer = new WpfVectorRenderer(new List<SaveObject> { saveObj });

            // Act
            var image = renderer.RenderToImage();

            // Assert
            // Circle radius 100 + thickness 10 = total diameter 210.
            // Our GetGraphicBounds should account for this.
            Assert.That(image.Width, Is.GreaterThanOrEqualTo(200), "Image must be wide enough for the diameter.");
        }
    }
}
