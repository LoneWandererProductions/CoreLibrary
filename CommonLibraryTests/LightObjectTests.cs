/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     CommonLibraryTests
* FILE:        CommonLibraryTests/LightObjectTests.cs
* PURPOSE:     Tests for my Light Vector Library
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

using System;
using System.Collections.Generic;
using System.Numerics;
using LightVector;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     Test various stuff from my vector objects
    /// </summary>
    [TestClass]
    public class LightObjectTests
    {
        /// <summary>
        ///     Applies the scale transformation should scale vectors correctly.
        /// </summary>
        [TestMethod]
        public void ApplyScaleTransformationShouldScaleVectorsCorrectly()
        {
            // Arrange
            var curve = new BezierCurve { Vectors = new List<Vector2> { new(1, 2), new(3, 4) } };

            var scaleTransform = new ScaleTransform(2, 3);

            // Act
            curve.ApplyTransformation(scaleTransform);

            // Assert
            Assert.AreEqual(new Vector2(2, 6), curve.Vectors[0]);
            Assert.AreEqual(new Vector2(6, 12), curve.Vectors[1]);
        }

        /// <summary>
        ///     Applies the rotate transformation should rotate vectors correctly.
        /// </summary>
        [TestMethod]
        public void ApplyRotateTransformationShouldRotateVectorsCorrectly()
        {
            // Arrange
            var curve = new BezierCurve
            {
                Vectors = new List<Vector2>
                {
                    new(1, 0) // Starting vector (1, 0)
                }
            };

            var rotateTransform = new RotateTransform(90); // Rotate by 90 degrees

            // Act
            curve.ApplyTransformation(rotateTransform);

            // Define a small tolerance for floating-point comparison
            const float tolerance = 1e-6f;

            // Assert
            Assert.IsTrue(Math.Abs(curve.Vectors[0].X - 0) < tolerance, "X component not as expected");
            Assert.IsTrue(Math.Abs(curve.Vectors[0].Y - 1) < tolerance, "Y component not as expected");
        }


        /// <summary>
        ///     Applies the translate transformation should translate vectors correctly.
        /// </summary>
        [TestMethod]
        public void ApplyTranslateTransformationShouldTranslateVectorsCorrectly()
        {
            // Arrange
            var curve = new BezierCurve { Vectors = new List<Vector2> { new(1, 2), new(3, 4) } };

            var translateTransform = new TranslateTransform(5, -3);

            // Act
            curve.ApplyTransformation(translateTransform);

            // Assert
            Assert.AreEqual(new Vector2(6, -1), curve.Vectors[0]);
            Assert.AreEqual(new Vector2(8, 1), curve.Vectors[1]);
        }

        /// <summary>
        ///     Applies the multiple transformations should apply all transformations correctly.
        /// </summary>
        [TestMethod]
        public void ApplyMultipleTransformationsShouldApplyAllTransformationsCorrectly()
        {
            // Arrange
            var curve = new BezierCurve { Vectors = new List<Vector2> { new(1, 2), new(3, 4) } };

            var scaleTransform = new ScaleTransform(2, 3); // First scale
            var rotateTransform = new RotateTransform(90); // Then rotate 90 degrees
            var translateTransform = new TranslateTransform(5, -3); // Finally, translate

            // Act
            curve.ApplyTransformation(scaleTransform);
            curve.ApplyTransformation(rotateTransform);
            curve.ApplyTransformation(translateTransform);

            // Assert
            Assert.AreEqual(new Vector2(-1, -1), curve.Vectors[0]);
            Assert.AreEqual(new Vector2(-7, 3), curve.Vectors[1]);
        }


        /// <summary>
        ///     Applies the empty transformation should not change vectors.
        /// </summary>
        [TestMethod]
        public void ApplyEmptyTransformationShouldNotChangeVectors()
        {
            // Arrange
            var curve = new BezierCurve { Vectors = new List<Vector2> { new(1, 2), new(3, 4) } };

            var emptyTransform = new Transform(); // Assuming you have an empty transformation class

            // Act
            curve.ApplyTransformation(emptyTransform);

            // Assert
            Assert.AreEqual(new Vector2(1, 2), curve.Vectors[0]);
            Assert.AreEqual(new Vector2(3, 4), curve.Vectors[1]);
        }

        /// <summary>
        ///     Applies the multiple scales should correctly apply each scale.
        /// </summary>
        [TestMethod]
        public void ApplyMultipleScalesShouldCorrectlyApplyEachScale()
        {
            // Arrange
            var curve = new BezierCurve
            {
                Vectors = new List<Vector2>
                {
                    new(3, 3) // Initial vector
                }
            };

            var scale1 = new ScaleTransform(2, 2); // Scale by 2
            var scale2 = new ScaleTransform(3, 3); // Scale by 3

            // Act
            curve.ApplyTransformation(scale1);
            curve.ApplyTransformation(scale2); // The final vector should be scaled by 2 * 3 = 6

            // Assert
            // After scaling by 2 and 3, the expected result should be (3 * 2 * 3, 3 * 2 * 3) = (18, 18)
            Assert.AreEqual(18f, curve.Vectors[0].X, "X component not as expected after scaling");
            Assert.AreEqual(18f, curve.Vectors[0].Y, "Y component not as expected after scaling");
        }

        /// <summary>
        ///     Polygons the object should initialize with vertices.
        /// </summary>
        [TestMethod]
        public void PolygonObjectShouldInitializeWithVertices()
        {
            var polygon = new PolygonObject
            {
                Vertices = new List<Vector2> { new(0, 0), new(1, 0), new(1, 1), new(0, 1) }
            };

            Assert.AreEqual(4, polygon.Vertices.Count);
            Assert.AreEqual(new Vector2(0, 0), polygon.Vertices[0]);
        }

        /// <summary>
        ///     Circles the object should initialize correctly.
        /// </summary>
        [TestMethod]
        public void CircleObjectShouldInitializeCorrectly()
        {
            var circle = new CircleObject { Center = new Vector2(5, 5), Radius = 3.0f };

            Assert.AreEqual(new Vector2(5, 5), circle.Center);
            Assert.AreEqual(3.0f, circle.Radius);
        }

        /// <summary>
        ///     Ovals the object should initialize correctly.
        /// </summary>
        [TestMethod]
        public void OvalObjectShouldInitializeCorrectly()
        {
            var oval = new OvalObject { Center = new Vector2(2, 2), RadiusX = 5.0f, RadiusY = 3.0f };

            Assert.AreEqual(new Vector2(2, 2), oval.Center);
            Assert.AreEqual(5.0f, oval.RadiusX);
            Assert.AreEqual(3.0f, oval.RadiusY);
        }

        /// <summary>
        ///     Applies the transformation should scale polygon correctly.
        /// </summary>
        [TestMethod]
        public void ApplyTransformationShouldScalePolygonCorrectly()
        {
            var polygon = new PolygonObject
            {
                Vertices = new List<Vector2> { new(1, 1), new(2, 1), new(2, 2), new(1, 2) }
            };

            var scaleTransform = new ScaleTransform(2, 2);
            polygon.ApplyTransformation(scaleTransform);

            Assert.AreEqual(new Vector2(2, 2), polygon.Vertices[0]);
            Assert.AreEqual(new Vector2(4, 2), polygon.Vertices[1]);
            Assert.AreEqual(new Vector2(4, 4), polygon.Vertices[2]);
            Assert.AreEqual(new Vector2(2, 4), polygon.Vertices[3]);
        }

        /// <summary>
        ///     Applies the transformation should translate circle correctly.
        /// </summary>
        [TestMethod]
        public void ApplyTransformationShouldTranslateCircleCorrectly()
        {
            var circle = new CircleObject { Center = new Vector2(5, 5), Radius = 3.0f };

            var translateTransform = new TranslateTransform(2, -1);
            circle.ApplyTransformation(translateTransform);

            Assert.AreEqual(new Vector2(7, 4), circle.Center);
        }

        /// <summary>
        ///     Applies the transformation should rotate oval correctly.
        /// </summary>
        [TestMethod]
        public void ApplyTransformationShouldRotateOvalCorrectly()
        {
            var oval = new OvalObject { Center = new Vector2(0, 0), RadiusX = 5, RadiusY = 3 };

            var rotateTransform = new RotateTransform(90);
            oval.ApplyTransformation(rotateTransform);

            Assert.AreEqual(0, oval.Center.X);
            Assert.AreEqual(0, oval.Center.Y);
        }

        /// <summary>
        ///     Scales the transform changes direction correctly.
        /// </summary>
        [TestMethod]
        public void ScaleTransformChangesDirectionCorrectly()
        {
            // Arrange
            var line = new LineObject { Direction = new Vector2(2, 3) };
            var scaleTransform = new ScaleTransform(2, 0.5f);

            // Act
            line.ApplyTransformation(scaleTransform);

            // Assert
            Assert.AreEqual(new Vector2(4, 1.5f), line.Direction);
        }

        /// <summary>
        ///     Rotates the transform90 degrees rotates direction correctly.
        /// </summary>
        [TestMethod]
        public void RotateTransform90DegreesRotatesDirectionCorrectly()
        {
            // Arrange
            var line = new LineObject { Direction = new Vector2(1, 0) };
            var rotateTransform = new RotateTransform(90);

            // Act
            line.ApplyTransformation(rotateTransform);

            // Assert
            var expected = new Vector2(0, 1);
            Assert.IsTrue(AreVectorsEqual(expected, line.Direction));
        }

        /// <summary>
        ///     Rotates the transform180 degrees rotates direction correctly.
        /// </summary>
        [TestMethod]
        public void RotateTransform180DegreesRotatesDirectionCorrectly()
        {
            // Arrange
            var line = new LineObject { Direction = new Vector2(1, 0) };
            var rotateTransform = new RotateTransform(180);

            // Act
            line.ApplyTransformation(rotateTransform);

            // Assert
            var expected = new Vector2(-1, 0);
            Assert.IsTrue(AreVectorsEqual(expected, line.Direction));
        }

        /// <summary>
        ///     Rotates the transform45 degrees rotates direction correctly.
        /// </summary>
        [TestMethod]
        public void RotateTransform45DegreesRotatesDirectionCorrectly()
        {
            // Arrange
            var line = new LineObject { Direction = new Vector2(1, 0) };
            var rotateTransform = new RotateTransform(45);

            // Act
            line.ApplyTransformation(rotateTransform);

            // Assert
            var expected = new Vector2((float)Math.Cos(Math.PI / 4), (float)Math.Sin(Math.PI / 4));
            Assert.IsTrue(AreVectorsEqual(expected, line.Direction));
        }

        /// <summary>
        ///     Ares the vectors equal.
        /// </summary>
        /// <param name="v1">The v1.</param>
        /// <param name="v2">The v2.</param>
        /// <param name="tolerance">The tolerance.</param>
        /// <returns></returns>
        private static bool AreVectorsEqual(Vector2 v1, Vector2 v2, float tolerance = 0.0001f)
        {
            return Math.Abs(v1.X - v2.X) < tolerance && Math.Abs(v1.Y - v2.Y) < tolerance;
        }
    }
}
