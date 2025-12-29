/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     CommonLibraryTests
* FILE:        CommonLibraryTests/LightVectorSaveTests.cs
* PURPOSE:     Tests for IoFileHandler and mostly the save part
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
    ///     Tests for our Light Vector Library
    /// </summary>
    [TestClass]
    public class LightVectorSaveTests
    {
        /// <summary>
        ///     Serializes the save container generates valid XML.
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
                    new()
                    {
                        Id = 1,
                        Layer = 0,
                        StartCoordinates = new Point(5, 5),
                        Graphic = new LineObject { Direction = new Vector2(10, 10) },
                        Type = GraphicTypes.Line
                    },
                    new()
                    {
                        Id = 2,
                        Layer = 1,
                        StartCoordinates = new Point(15, 20),
                        Graphic = new BezierCurve
                        {
                            Vectors = new List<Vector2> { new(10, 10), new(20, 40), new(50, 50) },
                            Tension = 0.5
                        },
                        Type = GraphicTypes.BezierCurve
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
            Assert.IsTrue(result.Contains("<Element"),
                "Serialized output should contain the correct root XML element <Element>.");
            Assert.IsTrue(result.Contains("<Id>1</Id>"),
                "Serialized output should contain the correct Id for the first object.");
            Assert.IsTrue(result.Contains("<Layer>0</Layer>"),
                "Serialized output should contain the correct Layer for the first object.");
            Assert.IsTrue(result.Contains("<StartCoordinates"), "Serialized output should contain StartCoordinates.");
            Assert.IsTrue(result.Contains("<X>5</X>"), "Serialized output should contain the correct X coordinate.");
            Assert.IsTrue(result.Contains("<Y>5</Y>"), "Serialized output should contain the correct Y coordinate.");

            // Debugging output
            Console.WriteLine(result);
        }

        /// <summary>
        ///     Saves the object should serialize and deserialize correctly.
        /// </summary>
        [TestMethod]
        public void SaveObjectShouldSerializeAndDeserializeCorrectly()
        {
            var saveObject = new SaveObject
            {
                Id = 1,
                Layer = 2,
                StartCoordinates = new Point(10, 10),
                Graphic = new CircleObject { Center = new Vector2(5, 5), Radius = 3.0f },
                Type = GraphicTypes.Circle
            };

            var serializer = new XmlSerializer(typeof(SaveObject));
            string xml;

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, saveObject);
                xml = writer.ToString();
            }

            SaveObject deserializedObject;
            using (var reader = new StringReader(xml))
            {
                deserializedObject = (SaveObject)serializer.Deserialize(reader);
            }

            if (deserializedObject == null)
            {
                return;
            }

            Assert.AreEqual(1, deserializedObject.Id);
            Assert.AreEqual(GraphicTypes.Circle, deserializedObject.Type);
            Assert.AreEqual(3.0f, ((CircleObject)deserializedObject.Graphic).Radius);
        }
    }
}
