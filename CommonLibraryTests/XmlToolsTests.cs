/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/XmlToolsTests.cs
 * PURPOSE:     Test for XML tools
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serializer;
using System.IO;
using System.Xml;

namespace CommonLibraryTests
{
    [TestClass]
    public class XmlToolsTests
    {
        /// <summary>
        /// The test XML path
        /// </summary>
        private const string TestXmlPath = "test.xml";

        /// <summary>
        /// Creates the test XML file.
        /// </summary>
        /// <param name="path">The path.</param>
        private static void CreateTestXmlFile(string path)
        {
            var xmlDoc = new XmlDocument();
            var declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlDoc.AppendChild(declaration);

            var rootElement = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(rootElement);

            var itemElement = xmlDoc.CreateElement("Item");
            itemElement.SetAttribute("Number", "1");
            itemElement.SetAttribute("GenericText", "Test Item");
            itemElement.SetAttribute("Other", "100.5");
            rootElement.AppendChild(itemElement);

            xmlDoc.Save(path);
        }

        [TestInitialize]
        public void SetUp()
        {
            // Create a sample XML file before running tests
            if (File.Exists(TestXmlPath))
            {
                File.Delete(TestXmlPath);
            }

            CreateTestXmlFile(TestXmlPath);
        }

        [TestCleanup]
        public void CleanUp()
        {
            // Clean up test file after tests
            if (File.Exists(TestXmlPath))
            {
                File.Delete(TestXmlPath);
            }
        }

        [TestMethod]
        public void GetFirstAttributeFromXml_ValidProperty_ReturnsCorrectValue()
        {
            // Act
            var result = XmlTools.GetFirstAttributeFromXml(TestXmlPath, "GenericText");

            // Assert
            Assert.AreEqual("Test Item", result);
        }

        [TestMethod]
        public void GetFirstAttributeFromXml_PropertyNotFound_ReturnsNull()
        {
            // Act
            var result = XmlTools.GetFirstAttributeFromXml(TestXmlPath, "NonExistentProperty");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetAttributesFromXml_ValidProperty_ReturnsCorrectValues()
        {
            // Act
            var result = XmlTools.GetAttributesFromXml(TestXmlPath, "GenericText");

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Test Item", result[0]);
        }

        [TestMethod]
        public void GetAttributesFromXml_PropertyNotFound_ReturnsNull()
        {
            // Act
            var result = XmlTools.GetAttributesFromXml(TestXmlPath, "NonExistentProperty");

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Loads the XML invalid file returns null.
        /// </summary>
        [TestMethod]
        public void LoadXmlInvalidFileReturnsNull()
        {
            // Act
            var result = XmlTools.LoadXml("invalid.xml");

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Gets the first attribute from XML file does not exist returns null.
        /// </summary>
        [TestMethod]
        public void GetFirstAttributeFromXmlFileDoesNotExistReturnsNull()
        {
            // Act
            var result = XmlTools.GetFirstAttributeFromXml("nonexistent.xml", "GenericText");

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetAttributesFromXml_FileDoesNotExist_ReturnsNull()
        {
            // Act
            var result = XmlTools.GetAttributesFromXml("nonexistent.xml", "GenericText");

            // Assert
            Assert.IsNull(result);
        }
    }
}
