/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/Serialize.cs
 * PURPOSE:     Test for Serializer
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using DataFormatter;
using FileHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serializer;

namespace CommonLibraryTests
{
    /// <summary>
    ///     Serializer unit test class.
    /// </summary>
    [TestClass]
    public sealed class Serialize
    {
        /// <summary>
        ///     Check if our basic functions do Work out
        /// </summary>
        [TestMethod]
        public void LoadObjectFromXml()
        {
            var obj = new XmlItem();

            var paths = Path.Combine(Directory.GetCurrentDirectory(), "test.xml");

            Serializer.Serialize.SaveObjectToXml(obj, paths);

            var test = DeSerialize.LoadObjectFromXml<XmlItem>(paths);

            Assert.IsTrue(test != null, "Test failed no changes");

            FileHandleDelete.DeleteFile(paths);
        }

        /// <summary>
        ///     Check if our basic functions do Work out
        /// </summary>
        [TestMethod]
        public void LoadLstObjectFromXml()
        {
            var lst = new List<XmlItem> { ResourcesGeneral.DataItemOne, ResourcesGeneral.DataItemTwo };

            var paths = Path.Combine(Directory.GetCurrentDirectory(), "testList.xml");

            Serializer.Serialize.SaveLstObjectToXml(lst, paths);

            var rslt = DeSerialize.LoadListFromXml<XmlItem>(paths);

            Assert.AreEqual(2, rslt.Count, "Test failed no changes");

            FileHandleDelete.DeleteFile(paths);
        }

        /// <summary>
        ///     Check if our XmlTools works
        /// </summary>
        [TestMethod]
        public void XmlToolTests()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), nameof(Serialize),
                "testProperty.xml");

            Serializer.Serialize.SaveObjectToXml(ResourcesGeneral.DataItemOne, path);

            var cache = XmlTools.GetFirstAttributeFromXml(path, "Number");

            //Display
            Assert.AreEqual("1", cache, "Correct Display");

            FileHandleDelete.DeleteFile(path);
        }

        /// <summary>
        ///     Tests the serialize dictionary null dictionary throws exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSerializeDictionaryNullDictionaryThrowsException()
        {
            // Arrange
            var path = "test.xml";

            // Act
            Serializer.Serialize.SaveDctObjectToXml((Dictionary<string, string>)null, path);
        }

        /// <summary>
        ///     Tests the serialize dictionary empty dictionary throws exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSerializeDictionaryEmptyDictionaryThrowsException()
        {
            // Arrange
            var dictionary = new Dictionary<string, string>();
            const string path = "test.xml";

            // Act
            Serializer.Serialize.SaveDctObjectToXml(dictionary, path);
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
            Serializer.Serialize.SaveDctObjectToXml(dictionary, path);

            // Assert
            Assert.IsTrue(File.Exists(path));
            var fileContent = File.ReadAllText(path);
            Assert.IsTrue(fileContent.Contains("<Item>"));
            Assert.IsTrue(fileContent.Contains("key1"));
            Assert.IsTrue(fileContent.Contains("value1"));

            FileHandleDelete.DeleteFile(path);
        }

        /// <summary>
        ///     Tests the serialize dictionary invalid path throws exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileHandlerException))]
        public void Test_SerializeDictionaryInvalidPathThrowsException()
        {
            // Arrange
            var dictionary = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };
            var path = "invalid\0path.xml";

            // Act
            Serializer.Serialize.SaveDctObjectToXml(dictionary, path);
        }

        /// <summary>
        ///     Tests the load dictionary from XML valid path returns dictionary.
        /// </summary>
        [TestMethod]
        public void Test_LoadDictionaryFromXmlValidPathReturnsDictionary()
        {
            // Arrange
            var dictionary = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };
            var path = Path.Combine(Directory.GetCurrentDirectory(), "test.xml");
            Serializer.Serialize.SaveDctObjectToXml(dictionary, path);

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
            var path = "nonexistent.xml";

            // Act
            DeSerialize.LoadDictionaryFromXml<string, string>(path);
        }

        /// <summary>
        ///     Tests the load dictionary from XML empty file throws exception.
        /// </summary>
        [TestMethod]
        public void TestLoadDictionaryFromXmlEmptyFileThrowsException()
        {
            // Arrange
            var path = "empty.xml";
            File.WriteAllText(path, string.Empty);

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => DeSerialize.LoadDictionaryFromXml<string, string>(path));
        }
    }
}
