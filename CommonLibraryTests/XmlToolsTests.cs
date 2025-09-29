/*
 * COPYRIGHT:   See COPYING in the top-level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/XmlToolsTests.cs
 * PURPOSE:     Test for XML tools
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serializer;

namespace CommonLibraryTests;

/// <summary>
///     Some XML tests
/// </summary>
[TestClass]
public class XmlToolsTests
{
    /// <summary>
    ///     The test XML
    /// </summary>
    private const string TestXml = "test.xml";

    /// <summary>
    ///     The test XML path
    /// </summary>
    private static string _testXmlPath;

    /// <summary>
    ///     Creates the test XML file.
    /// </summary>
    /// <param name="relativePath">The relative path.</param>
    private static void CreateTestXmlFile(string relativePath)
    {
        var directory = AppDomain.CurrentDomain.BaseDirectory; // Gets the directory where the test is running
        _testXmlPath = Path.Combine(directory, relativePath);

        var xmlDoc = new XmlDocument();
        var declaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(declaration);

        var rootElement = xmlDoc.CreateElement("root");
        xmlDoc.AppendChild(rootElement);

        var itemElement = xmlDoc.CreateElement("Item");
        itemElement.SetAttribute("Number", "1");
        itemElement.SetAttribute("Other", "100.5");

        // Add GenericText as an attribute (for attribute-based tests)
        itemElement.SetAttribute("GenericText", "Test Item");

        // Add GenericText as an element (for element-based tests)
        var genericTextElement = xmlDoc.CreateElement("GenericText");
        genericTextElement.InnerText = "Test Item";
        itemElement.AppendChild(genericTextElement);

        rootElement.AppendChild(itemElement);
        xmlDoc.Save(_testXmlPath);
    }

    /// <summary>
    ///     Sets up.
    /// </summary>
    [TestInitialize]
    public void SetUp()
    {
        // Create a sample XML file before running tests
        if (File.Exists(_testXmlPath))
        {
            File.Delete(_testXmlPath);
        }

        CreateTestXmlFile(TestXml);
    }

    [TestCleanup]
    public void CleanUp()
    {
        // Clean up test file after tests
        if (File.Exists(_testXmlPath))
        {
            File.Delete(_testXmlPath);
        }
    }

    /// <summary>
    ///     Gets the first attribute from XML valid property returns correct value.
    /// </summary>
    [TestMethod]
    public void GetFirstAttributeFromXmlValidPropertyReturnsCorrectValue()
    {
        // Act
        var result = XmlTools.GetFirstAttributeFromXml(_testXmlPath, "GenericText");

        // Assert
        Assert.AreEqual("Test Item", result);
    }

    /// <summary>
    ///     Gets the first attribute from XML property not found returns null.
    /// </summary>
    [TestMethod]
    public void GetFirstAttributeFromXmlPropertyNotFoundReturnsNull()
    {
        // Act
        var result = XmlTools.GetFirstAttributeFromXml(_testXmlPath, "NonExistentProperty");

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Gets the attributes from XML valid property returns correct values.
    /// </summary>
    [TestMethod]
    public void GetAttributesFromXmlValidPropertyReturnsCorrectValues()
    {
        // Act
        var result = XmlTools.GetAttributesFromXml(_testXmlPath, "GenericText");

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Test Item", result[0]);
    }

    /// <summary>
    ///     Gets the attributes from XML property not found returns null.
    /// </summary>
    [TestMethod]
    public void GetAttributesFromXmlPropertyNotFoundReturnsNull()
    {
        // Act
        var result = XmlTools.GetAttributesFromXml(_testXmlPath, "NonExistentProperty");

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Loads the XML invalid file returns null.
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
    ///     Gets the first attribute from XML file does not exist returns null.
    /// </summary>
    [TestMethod]
    public void GetFirstAttributeFromXmlFileDoesNotExistReturnsNull()
    {
        // Act
        var result = XmlTools.GetFirstAttributeFromXml("nonexistent.xml", "GenericText");

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Gets the attributes from XML file does not exist returns null.
    /// </summary>
    [TestMethod]
    public void GetAttributesFromXmlFileDoesNotExistReturnsNull()
    {
        // Act
        var result = XmlTools.GetAttributesFromXml("nonexistent.xml", "GenericText");

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Gets the elements from XML valid element returns correct values.
    /// </summary>
    [TestMethod]
    public void GetElementsFromXmlValidElementReturnsCorrectValues()
    {
        // Act
        var result = XmlTools.GetElementsFromXml(_testXmlPath, "GenericText");

        // Assert
        Assert.IsNotNull(result, "Result should not be null.");
        Assert.AreEqual(1, result.Count, "There should be exactly one GenericText element.");
        Assert.AreEqual("Test Item", result[0], "Element value should match expected.");
    }

    /// <summary>
    ///     Gets the elements from XML file does not exist returns null.
    /// </summary>
    [TestMethod]
    public void GetElementsFromXmlFileDoesNotExistReturnsNull()
    {
        // Act
        var result = XmlTools.GetElementsFromXml("nonexistent.xml", "GenericText");

        // Assert
        Assert.IsNull(result);
    }
}
