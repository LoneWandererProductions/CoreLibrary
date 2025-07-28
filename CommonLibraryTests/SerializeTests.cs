/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/SerializeTests.cs
 * PURPOSE:     Test for Serializer
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreMemoryLog;
using FileHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serializer;

namespace CommonLibraryTests;

/// <summary>
///     Serializer unit test class.
/// </summary>
[TestClass]
public sealed class SerializeTests
{
    /// <summary>
    ///     Check if our basic functions do Work out
    /// </summary>
    [TestMethod]
    public void LoadObjectFromXml()
    {
        var obj = new XmlItem();

        var paths = Path.Combine(Directory.GetCurrentDirectory(), "test.xml");

        Serialize.SaveObjectToXml(obj, paths);

        var test = DeSerialize.LoadObjectFromXml<XmlItem>(paths);

        Assert.IsTrue(test != null, "Test failed no changes");

        _ = FileHandleDelete.DeleteFile(paths);
    }

    /// <summary>
    ///     Check if our basic functions do Work out
    /// </summary>
    [TestMethod]
    public void LoadLstObjectFromXml()
    {
        var lst = new List<XmlItem> { ResourcesGeneral.DataItemOne, ResourcesGeneral.DataItemTwo };

        var paths = Path.Combine(Directory.GetCurrentDirectory(), "testList.xml");

        Serialize.SaveLstObjectToXml(lst, paths);

        var rslt = DeSerialize.LoadListFromXml<XmlItem>(paths);

        Assert.AreEqual(2, rslt.Count, "Test failed no changes");

        _ = FileHandleDelete.DeleteFile(paths);
    }

    /// <summary>
    ///     Tests the serialize dictionary null dictionary throws exception.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestSerializeDictionaryNullDictionaryThrowsException()
    {
        // Arrange
        const string path = "test.xml";

        // Act
        Serialize.SaveDctObjectToXml((Dictionary<string, string>)null, path);
    }

    /// <summary>
    ///     Tests the serialize dictionary valid dictionary creates XML file.
    /// </summary>
    [TestMethod]
    public void TestSerializeDictionaryValidDictionaryCreatesXmlFile()
    {
        // Arrange
        var dictionary = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };

        var path = Path.Combine(Directory.GetCurrentDirectory(), "test.xml");

        // Act
        Serialize.SaveDctObjectToXml(dictionary, path);

        // Assert
        Assert.IsTrue(File.Exists(path));
        var fileContent = File.ReadAllText(path);
        Assert.IsTrue(fileContent.Contains("<Item>"));
        Assert.IsTrue(fileContent.Contains("key1"));
        Assert.IsTrue(fileContent.Contains("value1"));

        _ = FileHandleDelete.DeleteFile(path);
    }

    /// <summary>
    ///     Tests the load dictionary from XML valid path returns dictionary.
    /// </summary>
    [TestMethod]
    public void TestLoadDictionaryFromXmlValidPathReturnsDictionary()
    {
        // Arrange
        var dictionary = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };
        var path = Path.Combine(Directory.GetCurrentDirectory(), "test.xml");
        Serialize.SaveDctObjectToXml(dictionary, path);

        // Act
        var result = DeSerialize.LoadDictionaryFromXml<string, string>(path);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(dictionary.Count, result.Count);
        Assert.AreEqual(dictionary["key1"], result["key1"]);
        Assert.AreEqual(dictionary["key2"], result["key2"]);
    }

    /// <summary>
    ///     Tests the load dictionary from XML invalid path throws exception.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TestLoadDictionaryFromXmlInvalidPathThrowsException()
    {
        // Arrange
        const string path = "nonexistent.xml";

        // Act
        var result = DeSerialize.LoadDictionaryFromXml<string, string>(path);

        //Assert
        Assert.IsNull(result, "Result was not null");

        var log = InMemoryLogger.Instance.GetLatestLogs("Serializer", 1);

        Assert.IsTrue(log.First().Message.Contains("Error"), "Log was empty.");
    }
}
