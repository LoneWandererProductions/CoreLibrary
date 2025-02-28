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
        public void Serialize_SaveContainer_GeneratesValidXml()
        {
            // Arrange
            var container = new SaveContainer
            {
                Width = 500,
                Objects = new List<SaveObject>
                {
                    new SaveObject
                    {
                        Graphic = new LineObject
                        {
                            StartPoint = new Vector2(10, 10),
                            EndPoint = new Vector2(50, 50)
                        },
                        Type = VectorObjects.Line
                    },
                    new SaveObject
                    {
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
                        Type = VectorObjects.Curve
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

            // Debugging output
            Console.WriteLine(result);
        }

    }
}
