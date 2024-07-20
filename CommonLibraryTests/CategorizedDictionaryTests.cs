/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CategorizedDictionaryTests.cs
 * PURPOSE:     Test for our Dictionary with Category
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

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
        /// Sets the category should update category if key exists.
        /// </summary>
        [TestMethod]
        public void SetCategoryShouldUpdateCategoryIfKeyExists()
        {
            var dict = new CategorizedDictionary<int, string>();
            dict.Add("Category1", 1, "Value1");

            var updated = dict.SetCategory(1, "NewCategory");

            Assert.IsTrue(updated);
            var result = dict.GetCategoryAndValue(1);
            Assert.IsNotNull(result);
            Assert.AreEqual("NewCategory", result?.Category);
        }

        /// <summary>
        /// Sets the category should return false if key does not exist.
        /// </summary>
        [TestMethod]
        public void SetCategoryShouldReturnFalseIfKeyDoesNotExist()
        {
            var dict = new CategorizedDictionary<int, string>();

            var updated = dict.SetCategory(1, "NewCategory");

            Assert.IsFalse(updated);
        }

        /// <summary>
        /// Converts to string should return correct string representation.
        /// </summary>
        [TestMethod]
        public void ToStringShouldReturnCorrectStringRepresentation()
        {
            var dict = new CategorizedDictionary<int, string>();
            dict.Add("Category1", 1, "Value1");
            dict.Add("Category2", 2, "Value2");

            var result = dict.ToString();
            const string expected = "Key: 1, Category: Category1, Value: Value1\r\nKey: 2, Category: Category2, Value: Value2";

            Assert.AreEqual(expected, result);
        }
    }
}
