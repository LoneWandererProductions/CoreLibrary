/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     CommonLibraryTests
* FILE:        CommonLibraryTests/IoFileSearch.cs
* PURPOSE:     Tests for IoFileHandler and mostly the search part
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Xml.Serialization;
using LightVector;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///    Tests for our Light Vector Library
    /// </summary>
    [TestClass]
    public class LightVectorTests
    {
        /// <summary>
        /// Serializes the save container generates valid XML.
        /// </summary>
        [TestMethod]
        public void SerializeSaveContainerGeneratesValidXml()
        {
            // Arrange
            var container = new SaveContainer
            {
                Width = 500,
                Objects = new List<SaveObject>
        {
            new SaveObject
            {
                Id = 1,
                Layer = 0,
                StartCoordinates = new Point(5, 5),
                Graphic = new LineObject
                {
                    Direction = new Vector2(10, 10),
                },
                Type = GraphicTypes.Line
            },
            new SaveObject
            {
                Id = 2,
                Layer = 1,
                StartCoordinates = new Point(15, 20),
                Graphic = new CurveObject
                {
                    Vectors = new List<Vector2>
                    {
                        new(10, 10),
                        new(20, 40),
                        new(50, 50)
                    },
                    Tension = 0.5
                },
                Type = GraphicTypes.Curve
            }
        }
            };

            var serializer = new XmlSerializer(typeof(SaveContainer));

            // Act
            string result;
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, container);
                result = writer.ToString();
            }

            // Assert
            Assert.IsNotNull(result, "Serialized XML should not be null.");
            Assert.IsTrue(result.Contains("<Element"), "Serialized output should contain the correct root XML element <Element>.");
            Assert.IsTrue(result.Contains("<Id>1</Id>"), "Serialized output should contain the correct Id for the first object.");
            Assert.IsTrue(result.Contains("<Layer>0</Layer>"), "Serialized output should contain the correct Layer for the first object.");
            Assert.IsTrue(result.Contains("<StartCoordinates"), "Serialized output should contain StartCoordinates.");
            Assert.IsTrue(result.Contains("<X>5</X>"), "Serialized output should contain the correct X coordinate.");
            Assert.IsTrue(result.Contains("<Y>5</Y>"), "Serialized output should contain the correct Y coordinate.");

            // Debugging output
            Console.WriteLine(result);
        }


        /// <summary>
        /// Scales the transform changes direction correctly.
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
        /// Rotates the transform90 degrees rotates direction correctly.
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
        /// Rotates the transform180 degrees rotates direction correctly.
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
        /// Rotates the transform45 degrees rotates direction correctly.
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
        /// Ares the vectors equal.
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
