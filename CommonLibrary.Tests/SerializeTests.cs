/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrary.Tests
 * FILE:        SerializeTests.cs
 * PURPOSE:     Test for Serializer
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using Core.MemoryLog;
using FileHandler;
using Serializer;

namespace CommonLibrary.Tests
{
    /// <summary>
    /// Some serializer Tests
    /// </summary>
    [TestClass]
    public sealed class SerializeTests
    {
        /// <summary>
        /// Helper to generate a completely isolated, unique file path per test execution
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <returns> Returns a unique test path.</returns>
        private string GetUniqueTestPath(string prefix)
        {
            string fileName = $"{prefix}_{Guid.NewGuid():N}.xml";
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        /// <summary>
        /// Loads the object from XML.
        /// </summary>
        [TestMethod]
        public void LoadObjectFromXml()
        {
            var obj = new XmlItem();
            var path = GetUniqueTestPath("object");

            try
            {
                Serialize.SaveObjectToXml(obj, path);
                var test = DeSerialize.LoadObjectFromXml<XmlItem>(path);

                Assert.IsTrue(test != null, "Test failed no changes");
            }
            finally
            {
                _ = FileHandleDelete.DeleteFile(path);
            }
        }

        /// <summary>
        /// Loads the LST object from XML.
        /// </summary>
        [TestMethod]
        public void LoadLstObjectFromXml()
        {
            var lst = new List<XmlItem> { ResourcesGeneral.DataItemOne, ResourcesGeneral.DataItemTwo };
            var path = GetUniqueTestPath("list");

            try
            {
                Serialize.SaveLstObjectToXml(lst, path);
                var rslt = DeSerialize.LoadListFromXml<XmlItem>(path);

                Assert.AreEqual(2, rslt.Count, "Test failed no changes");
            }
            finally
            {
                _ = FileHandleDelete.DeleteFile(path);
            }
        }

        /// <summary>
        /// Tests the serialize dictionary null dictionary throws exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSerializeDictionaryNullDictionaryThrowsException()
        {
            var path = GetUniqueTestPath("null_dict");
            Serialize.SaveDctObjectToXml((Dictionary<string, string>)null, path);
        }

        /// <summary>
        /// Tests the serialize dictionary valid dictionary creates XML file.
        /// </summary>
        [TestMethod]
        public void TestSerializeDictionaryValidDictionaryCreatesXmlFile()
        {
            var dictionary = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };
            var path = GetUniqueTestPath("valid_dict");

            try
            {
                Serialize.SaveDctObjectToXml(dictionary, path);

                Assert.IsTrue(File.Exists(path));
                var fileContent = File.ReadAllText(path);
                Assert.IsTrue(fileContent.Contains("<Item>"));
                Assert.IsTrue(fileContent.Contains("key1"));
                Assert.IsTrue(fileContent.Contains("value1"));
            }
            finally
            {
                _ = FileHandleDelete.DeleteFile(path);
            }
        }

        /// <summary>
        /// Tests the load dictionary from XML valid path returns dictionary.
        /// </summary>
        [TestMethod]
        public void TestLoadDictionaryFromXmlValidPathReturnsDictionary()
        {
            var dictionary = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } };
            var path = GetUniqueTestPath("load_dict");

            try
            {
                Serialize.SaveDctObjectToXml(dictionary, path);
                var result = DeSerialize.LoadDictionaryFromXml<string, string>(path);

                Assert.IsNotNull(result);
                Assert.AreEqual(dictionary.Count, result.Count);
                Assert.AreEqual(dictionary["key1"], result["key1"]);
                Assert.AreEqual(dictionary["key2"], result["key2"]);
            }
            finally
            {
                _ = FileHandleDelete.DeleteFile(path);
            }
        }

        /// <summary>
        /// Tests the load dictionary from XML invalid path throws exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestLoadDictionaryFromXmlInvalidPathThrowsException()
        {
            var path = GetUniqueTestPath("nonexistent"); // Guaranteed not to exist yet

            var result = DeSerialize.LoadDictionaryFromXml<string, string>(path);

            Assert.IsNull(result, "Result was not null");
            var log = InMemoryLogger.Instance.GetLatestLogs("Serializer", 1);
            Assert.IsTrue(log.First().Message.Contains("Error"), "Log was empty.");
        }
    }
}
