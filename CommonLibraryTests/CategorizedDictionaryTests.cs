/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CategorizedDictionaryTests.cs
 * PURPOSE:     Test for our Dictionary with Category
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    [TestClass]
    public class CategorizedDictionaryTests
    {
        /// <summary>
        ///     The dictionary
        /// </summary>
        private CategorizedDictionary<string, string> _dict;

        /// <summary>
        ///     Sets up.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            _dict = new CategorizedDictionary<string, string>();
        }

        /// <summary>
        ///     Adds the and retrieve value by key.
        /// </summary>
        [TestMethod]
        public void AddAndRetrieveValueByKey()
        {
            _dict.Add("Category1", "Key1", "Value1");

            var result = _dict.Get("Key1");

            Assert.AreEqual("Value1", result);
        }

        /// <summary>
        ///     Adds the and retrieve value by key in null category.
        /// </summary>
        [TestMethod]
        public void AddAndRetrieveValueByKeyInNullCategory()
        {
            _dict.Add(null, "Key3", "Value3");

            var result = _dict.Get("Key3");

            Assert.AreEqual("Value3", result);
        }

        /// <summary>
        ///     Gets the category and value by key.
        /// </summary>
        [TestMethod]
        public void GetCategoryAndValueByKey()
        {
            _dict.Add("Category1", "Key1", "Value1");

            var result = _dict.GetCategoryAndValue("Key1");

            Assert.IsNotNull(result);
            Assert.AreEqual("Category1", result.Value.Category);
            Assert.AreEqual("Value1", result.Value.Value);
        }

        /// <summary>
        ///     Gets the category and value by non existing key.
        /// </summary>
        [TestMethod]
        public void GetCategoryAndValueByNonExistingKey()
        {
            var result = _dict.GetCategoryAndValue("NonExistingKey");

            Assert.IsNull(result);
        }

        /// <summary>
        ///     Gets the values by category.
        /// </summary>
        [TestMethod]
        public void GetValuesByCategory()
        {
            _dict.Add("Category1", "Key1", "Value1");
            _dict.Add("Category1", "Key2", "Value2");
            _dict.Add("Category2", "Key3", "Value3");

            var result = _dict.GetCategory("Category1");

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Value1", result["Key1"]);
            Assert.AreEqual("Value2", result["Key2"]);
        }

        /// <summary>
        ///     Gets the values by null category.
        /// </summary>
        [TestMethod]
        public void GetValuesByNullCategory()
        {
            _dict.Add(null, "Key3", "Value3");
            _dict.Add("Category1", "Key1", "Value1");

            var result = _dict.GetCategory(null);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Value3", result["Key3"]);
        }

        /// <summary>
        ///     Gets the categories.
        /// </summary>
        [TestMethod]
        public void GetCategories()
        {
            _dict.Add("Category1", "Key1", "Value1");
            _dict.Add("Category2", "Key2", "Value2");
            _dict.Add(null, "Key3", "Value3");

            var result = _dict.GetCategories();

            Assert.AreEqual(3, result.Count());
            CollectionAssert.Contains(result.ToList(), "Category1");
            CollectionAssert.Contains(result.ToList(), "Category2");
            CollectionAssert.Contains(result.ToList(), null);
        }

        /// <summary>
        ///     Gets the non existing key.
        /// </summary>
        [TestMethod]
        public void GetNonExistingKey()
        {
            var result = _dict.Get("NonExistingKey");

            Assert.IsNull(result);
        }

        /// <summary>
        ///     Sets the category should update category if key exists.
        /// </summary>
        [TestMethod]
        public void SetCategoryShouldUpdateCategoryIfKeyExists()
        {
            var dict = new CategorizedDictionary<int, string> { { "Category1", 1, "Value1" } };

            var updated = dict.SetCategory(1, "NewCategory");

            Assert.IsTrue(updated);
            var result = dict.GetCategoryAndValue(1);
            Assert.IsNotNull(result);
            Assert.AreEqual("NewCategory", result.Value.Category);
        }

        /// <summary>
        ///     Sets the category should return false if key does not exist.
        /// </summary>
        [TestMethod]
        public void SetCategoryShouldReturnFalseIfKeyDoesNotExist()
        {
            var dict = new CategorizedDictionary<int, string>();

            var updated = dict.SetCategory(1, "NewCategory");

            Assert.IsFalse(updated);
        }

        /// <summary>
        ///     Tries the get category should return true and category when key exists.
        /// </summary>
        [TestMethod]
        public void TryGetCategoryShouldReturnTrueAndCategoryWhenKeyExists()
        {
            // Arrange
            var dict = new CategorizedDictionary<int, string>
            {
                { "COMMAND", 1, "Print(hello World)" }, { "IF", 2, "if(condition)" }
            };

            // Act
            var result = dict.TryGetCategory(1, out var category);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("COMMAND", category);
        }

        /// <summary>
        ///     Tries the get category should return false and null when key does not exist.
        /// </summary>
        [TestMethod]
        public void TryGetCategoryShouldReturnFalseAndNullWhenKeyDoesNotExist()
        {
            // Arrange
            var dict = new CategorizedDictionary<int, string> { { "COMMAND", 1, "Print(hello World)" } };

            // Act
            var result = dict.TryGetCategory(2, out var category);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(category);
        }

        /// <summary>
        ///     Tries the get value should return true and value when key exists.
        /// </summary>
        [TestMethod]
        public void TryGetValueShouldReturnTrueAndValueWhenKeyExists()
        {
            // Arrange
            var dict = new CategorizedDictionary<int, string>
            {
                { "COMMAND", 1, "Print(hello World)" }, { "IF", 2, "if(condition)" }
            };

            // Act
            var result = dict.TryGetValue(1, out var value);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("Print(hello World)", value);
        }

        /// <summary>
        ///     Tries the get value should return false and default value when key does not exist.
        /// </summary>
        [TestMethod]
        public void TryGetValueShouldReturnFalseAndDefaultValueWhenKeyDoesNotExist()
        {
            // Arrange
            var dict = new CategorizedDictionary<int, string> { { "COMMAND", 1, "Print(hello World)" } };

            // Act
            var result = dict.TryGetValue(2, out var value);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(value);
        }

        /// <summary>
        /// Enumerators the returns correct key value pairs.
        /// </summary>
        [TestMethod]
        public void EnumeratorReturnsCorrectKeyValuePairs()
        {
            // Arrange
            var dict = new CategorizedDictionary<string, int>
            {
                { "Category1", "Key1", 10 }, { "Category2", "Key2", 20 }, { "Category1", "Key3", 30 }
            };

            // Act
            var keyValuePairs = dict.ToKeyValueList();

            // Assert
            var expected = new List<KeyValuePair<string, int>> { new("Key1", 10), new("Key2", 20), new("Key3", 30) };

            CollectionAssert.AreEqual(expected, keyValuePairs);
        }

        /// <summary>
        /// Enumerators the is enumerated in insertion order.
        /// </summary>
        [TestMethod]
        public void EnumeratorIsEnumeratedInInsertionOrder()
        {
            // Arrange
            var dict = new CategorizedDictionary<string, int>
            {
                { "CategoryA", "Key1", 100 }, { "CategoryB", "Key2", 200 }, { "CategoryA", "Key3", 300 }
            };

            // Act
            var keyValuePairs = dict.ToKeyValueList();

            // Assert
            var expected = new List<KeyValuePair<string, int>> { new("Key1", 100), new("Key2", 200), new("Key3", 300) };

            CollectionAssert.AreEqual(expected, keyValuePairs);
        }

        /// <summary>
        /// Enumerators the returns empty for empty dictionary.
        /// </summary>
        [TestMethod]
        public void EnumeratorReturnsEmptyForEmptyDictionary()
        {
            // Arrange
            var dict = new CategorizedDictionary<string, int>();

            // Act
            var keyValuePairs = dict.ToKeyValueList();

            // Assert
            Assert.AreEqual(0, keyValuePairs.Count);
        }

        /// <summary>
        /// Enumerators the returns correct key value pairs after modification.
        /// </summary>
        [TestMethod]
        public void EnumeratorReturnsCorrectKeyValuePairsAfterModification()
        {
            // Arrange
            var dict = new CategorizedDictionary<string, int>
            {
                { "Category1", "Key1", 10 }, { "Category2", "Key2", 20 }, { "Category3", "Key3", 30 }
            };

            // Act
            // Modify dictionary
            var keyValuePairs = dict.ToKeyValueList();

            // Assert
            var expected = new List<KeyValuePair<string, int>> { new("Key1", 10), new("Key2", 20), new("Key3", 30) };

            CollectionAssert.AreEqual(expected, keyValuePairs);
        }
    }
}
