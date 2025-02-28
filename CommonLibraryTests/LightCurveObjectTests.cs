/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     CommonLibraryTests
* FILE:        CommonLibraryTests/LightCurveObjectTests.cs
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
    [TestClass]
    public class LightCurveObjectTests
    {
        /// <summary>
        /// Applies the scale transformation should scale vectors correctly.
        /// </summary>
        [TestMethod]
        public void ApplyScaleTransformationShouldScaleVectorsCorrectly()
        {
            // Arrange
            var curve = new CurveObject
            {
                Vectors = new List<Vector2>
                {
                    new Vector2(1, 2),
                    new Vector2(3, 4)
                }
            };

            var scaleTransform = new ScaleTransform(2, 3);

            // Act
            curve.ApplyTransformation(scaleTransform);

            // Assert
            Assert.AreEqual(new Vector2(2, 6), curve.Vectors[0]);
            Assert.AreEqual(new Vector2(6, 12), curve.Vectors[1]);
        }

        /// <summary>
        /// Applies the rotate transformation should rotate vectors correctly.
        /// </summary>
        [TestMethod]
        public void ApplyRotateTransformationShouldRotateVectorsCorrectly()
        {
            // Arrange
            var curve = new CurveObject
            {
                Vectors = new List<Vector2>
                {
                    new Vector2(1, 0)  // Starting vector (1, 0)
                }
            };

            var rotateTransform = new RotateTransform(90); // Rotate by 90 degrees

            // Act
            curve.ApplyTransformation(rotateTransform);

            // Define a small tolerance for floating-point comparison
            float tolerance = 1e-6f;

            // Assert
            Assert.IsTrue(Math.Abs(curve.Vectors[0].X - 0) < tolerance, "X component not as expected");
            Assert.IsTrue(Math.Abs(curve.Vectors[0].Y - 1) < tolerance, "Y component not as expected");
        }


        /// <summary>
        /// Applies the translate transformation should translate vectors correctly.
        /// </summary>
        [TestMethod]
        public void ApplyTranslateTransformationShouldTranslateVectorsCorrectly()
        {
            // Arrange
            var curve = new CurveObject
            {
                Vectors = new List<Vector2>
                {
                    new Vector2(1, 2),
                    new Vector2(3, 4)
                }
            };

            var translateTransform = new TranslateTransform(5, -3);

            // Act
            curve.ApplyTransformation(translateTransform);

            // Assert
            Assert.AreEqual(new Vector2(6, -1), curve.Vectors[0]);
            Assert.AreEqual(new Vector2(8, 1), curve.Vectors[1]);
        }

        /// <summary>
        /// Applies the multiple transformations should apply all transformations correctly.
        /// </summary>
        [TestMethod]
        public void ApplyMultipleTransformationsShouldApplyAllTransformationsCorrectly()
        {
            // Arrange
            var curve = new CurveObject
            {
                Vectors = new List<Vector2>
                {
                    new Vector2(1, 2),
                    new Vector2(3, 4)
                }
            };

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
        /// Applies the empty transformation should not change vectors.
        /// </summary>
        [TestMethod]
        public void ApplyEmptyTransformationShouldNotChangeVectors()
        {
            // Arrange
            var curve = new CurveObject
            {
                Vectors = new List<Vector2>
                {
                    new Vector2(1, 2),
                    new Vector2(3, 4)
                }
            };

            var emptyTransform = new Transform(); // Assuming you have an empty transformation class

            // Act
            curve.ApplyTransformation(emptyTransform);

            // Assert
            Assert.AreEqual(new Vector2(1, 2), curve.Vectors[0]);
            Assert.AreEqual(new Vector2(3, 4), curve.Vectors[1]);
        }

        /// <summary>
        /// Applies the multiple scales should correctly apply each scale.
        /// </summary>
        [TestMethod]
        public void ApplyMultipleScalesShouldCorrectlyApplyEachScale()
        {
            // Arrange
            var curve = new CurveObject
            {
                Vectors = new List<Vector2>
                {
                    new Vector2(3, 3)  // Initial vector
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

    }
}
