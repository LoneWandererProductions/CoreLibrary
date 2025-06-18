/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonExtendedObjectsTests
 * FILE:        CommonExtendedObjectsTests/ExtendedSystemObjects.cs
 * PURPOSE:     Tests for ExtendedSystemObjects
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedAutoPropertyAccessor.Local just here once for test purposes

#pragma warning disable 649

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ExtendedSystemObjects;
using ExtendedSystemObjects.Interfaces;
using Mathematics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonExtendedObjectsTests
{
    /// <summary>
    ///     CommonLibraryTests Extended system objects unit test class.
    /// </summary>
    [TestClass]
    public sealed class ExtendedSystemObjects
    {
        /// <summary>
        ///     The Dictionary one (readonly). Value: new Dictionary&lt;int, string&gt;().
        /// </summary>
        private readonly Dictionary<int, string> _dictOne = new();

        /// <summary>
        ///     The Dictionary three (readonly).
        ///     Value: new Dictionary&lt;int, string&gt; { {1, "test"}, {2, "new"}, {3, "new"}, {4, "hell"} }.
        /// </summary>
        private readonly Dictionary<int, string> _dictThree =
            new() { { 1, "test" }, { 2, "new" }, { 3, "new" }, { 4, "hell" } };

        /// <summary>
        ///     The list one (readonly). Value: new List&lt;string&gt;().
        /// </summary>
        private readonly List<string> _listOne = new();

        /// <summary>
        ///     The list two (readonly). Value: new List&lt;string&gt; {"test"}.
        /// </summary>
        private readonly List<string> _listTwo = new() { "test" };

        /// <summary>
        ///     Here we Test our List
        /// </summary>
        [TestMethod]
        public void ExtendedListIsNullOrEmpty()
        {
            List<string> listCache = null;

            Assert.IsTrue(_listOne.IsNullOrEmpty(), "Test passed check Extension of List");

            // ReSharper disable once ExpressionIsAlwaysNull, I know but that is the test
            Assert.IsTrue(listCache.IsNullOrEmpty(), "Test passed check Extension of List");

            Assert.IsFalse(_listTwo.IsNullOrEmpty(), "Test passed check Extension of List");
        }

        /// <summary>
        ///     Here we Test our List with Add First
        /// </summary>
        [TestMethod]
        public void ExtendedListAddFirst()
        {
            var lst = new List<int> { 0, 1, 3 };

            lst.AddFirst(7);

            Assert.AreEqual(7, lst[0], "Test passed added Element at the first position " + lst[0]);
        }

        /// <summary>
        ///     Here we Test our List AddDistinct and AddIsDistinct
        /// </summary>
        [TestMethod]
        public void ExtendedListAddDistinct()
        {
            var lst = new List<int> { 2, 3 };
            var check = lst.AddDistinct(3);

            Assert.IsFalse(check, "Wrong bool");

            Assert.AreEqual(2, lst.Count, "Value added");

            check = lst.AddDistinct(4);
            Assert.IsTrue(check, "Wrong bool");

            Assert.IsTrue(check, "Element added");
            check = lst.AddDistinct(2);
            Assert.IsFalse(check, "Element not added");
        }

        /// <summary>
        ///     Here we Test our Dictionary Basic Clone tests no guaranty
        /// </summary>
        [TestMethod]
        public void CloneList()
        {
            var list = new List<int> { 2, 3, 1 };

            var clone = list.Clone();

            Assert.AreEqual(clone.Count, list.Count, "Test passed CloneList, first round:  " + list.Count);

            _ = list.Remove(1);

            Assert.AreNotEqual(clone.Count, list.Count, "Test passed CloneList, first round:  " + list.Count);

            var lst = new List<TstObj>();
            var obj = new TstObj { First = 1, Second = 2 };

            lst.Add(obj);
            lst.Add(obj);

            var secondClone = lst.Clone();

            Assert.AreEqual(lst.Count, secondClone.Count,
                $"Test passed CloneList, second round:  {list.Count}");

            secondClone.RemoveAt(0);

            Assert.AreNotEqual(lst.Count, secondClone.Count,
                $"Test passed CloneList, second round:  {list.Count}");
        }

        /// <summary>
        ///     Chunks of a list.
        /// </summary>
        [TestMethod]
        public void ChunkList()
        {
            var lst = new List<int>
            {
                1,
                2,
                3,
                4,
                5
            };
            var result = lst.ChunkBy(2);

            Assert.AreEqual(3, result.Count, $"Test passed ChunkList:  {result.Count}");
            var cache = result[2];

            Assert.AreEqual(5, cache[0], $"Test passed ChunkList, second round:  {cache[0]}");
        }

        /// <summary>
        ///     Here we Test our Dictionary AddDistinct
        /// </summary>
        [TestMethod]
        public void ExtendedDictionaryAddDistinct()
        {
            var lst = new Dictionary<XmlItem, int>
            {
                { ResourcesGeneral.DataItemOne, 1 }, { ResourcesGeneral.DataItemThree, 2 }
            };

            Trace.WriteLine("Passed the basic add");

            lst.AddDistinct(ResourcesGeneral.DataItemOne, 3);

            Assert.AreEqual(2, lst.Count, $"Tested not as equal: {lst.Count}");

            lst.AddDistinct(ResourcesGeneral.DataItemTwo, 4);

            Assert.AreEqual(3, lst.Count, $"Tested not as equal: {lst.Count}");
        }

        /// <summary>
        ///     Here we Test our List AddReplace
        /// </summary>
        [TestMethod]
        public void ExtendedListChunk()
        {
            var baseLst = new List<int> { 1, 2, 3 };
            var removeList = new List<int> { 1, 2 };

            baseLst.AddRange(removeList);
            var result = baseLst.ChunkBy(3);

            Assert.AreEqual(2, result.Count, "Not enough Chunks.");

            Assert.AreEqual(2, result[1][1], "Wrong element.");
        }

        /// <summary>
        ///     Here we Test our List
        /// </summary>
        [TestMethod]
        public void ExtendedListCompare()
        {
            var one = new List<int> { 1, 2, 3 };
            var two = new List<int> { 1, 3, 2 };
            var three = new List<int> { 1, 3, 2, 4 };
            var four = new List<int> { 1, 2, 3, 3 };

            var check = one.Equal(two);
            Assert.IsTrue(check,
                $"Base Compare Test failed: {nameof(one)} , {nameof(two)} : {check}");
            check = one.Equal(four);
            Assert.IsTrue(check, $"Base Test failed: {nameof(one)} , {nameof(four)} : {check}");

            check = one.Equal(two, EnumerableCompare.IgnoreOrderCount);
            Assert.IsTrue(check,
                $"IgnoreOrderCount Compare Test failed: {nameof(one)} , {nameof(two)} : {check}");
            check = one.Equal(four, EnumerableCompare.IgnoreOrderCount);
            Assert.IsTrue(check,
                string.Concat("IgnoreOrderCount Compare Test failed: ", nameof(one), " , ", nameof(four), " : ",
                    check));

            check = one.Equal(two, EnumerableCompare.IgnoreOrder);
            Assert.IsTrue(check,
                $"IgnoreOrder Compare Test failed: {nameof(one)} , {nameof(two)} : {check}");
            check = one.Equal(four, EnumerableCompare.IgnoreOrder);
            Assert.IsFalse(check,
                $"IgnoreOrder Compare Test failed: {nameof(one)} , {nameof(four)} : {check}");

            check = one.Equal(two, EnumerableCompare.AllEqual);
            Assert.IsFalse(check,
                $"AllEqual Compare Test failed: {nameof(one)} , {nameof(two)} : {check}");
            check = one.Equal(one, EnumerableCompare.AllEqual);
            Assert.IsTrue(check,
                $"AllEqual Compare Test failed: {nameof(one)} , {nameof(one)} : {check}");
            check = one.Equal(three, EnumerableCompare.AllEqual);
            Assert.IsFalse(check,
                $"AllEqual Compare Test failed: {nameof(one)} , {nameof(three)} : {check}");
            check = one.Equal(four, EnumerableCompare.AllEqual);
            Assert.IsFalse(check,
                $"AllEqual Compare Test failed: {nameof(one)} , {nameof(four)} : {check}");
        }

        /// <summary>
        ///     Here we test our IsNullOrEmpty
        /// </summary>
        [TestMethod]
        public void ExtendedDictionaryIsNullOrEmpty()
        {
            Assert.IsTrue(_dictOne.IsNullOrEmpty(), "Test passed check Extension of Dictionary");

            Dictionary<int, string> dictCache = null;

            // ReSharper disable once ExpressionIsAlwaysNull, I know but that is the test
            Assert.IsTrue(dictCache.IsNullOrEmpty(), "Test passed check Extension of Dictionary");

            Assert.IsFalse(_dictThree.IsNullOrEmpty(), "Test passed check Extension of Dictionary");
        }

        /// <summary>
        ///     Here we Test our Dictionary FindFirstKeyByValue
        /// </summary>
        [TestMethod]
        public void ExtendedDictionaryFindFirstKeyByValue()
        {
            var cache = _dictThree.GetFirstKeyByValue("new");

            Assert.AreEqual(2, cache, "Test passed check Extension of Dictionary:  " + cache);
        }

        /// <summary>
        ///     Here we Test our Dictionary FindKeysByValue
        /// </summary>
        [TestMethod]
        public void ExtendedDictionaryFindKeysByValue()
        {
            var cache = _dictThree.GetKeysByValue("new");

            Assert.AreEqual(2, cache.Count, "Test passed check Extension of Dictionary:  " + cache);

            Assert.AreEqual(2, cache[0], "Test passed check Extension of Dictionary:  " + cache[0]);

            Assert.AreEqual(3, cache[1], "Test passed check Extension of Dictionary:  " + cache[1]);
        }

        /// <summary>
        ///     Here we Test our Dictionary FindKeysByValue
        /// </summary>
        [TestMethod]
        public void ExtendedDictionaryGetDictionaryByValues()
        {
            var lst = new List<int> { 2, 4 };

            var cache = _dictThree.GetDictionaryByValues(lst);

            Assert.AreEqual(2, cache.Count, "Test passed check Extension of Dictionary:  " + cache);

            Assert.AreEqual("new", cache[2], "Test passed check Extension of Dictionary:  " + cache[2]);

            Assert.AreEqual("hell", cache[4], "Test passed check Extension of Dictionary:  " + cache[4]);
        }

        /// <summary>
        ///     Here we Test our Dictionary Basic Clone tests no guaranty
        /// </summary>
        [TestMethod]
        public void CloneDictionary()
        {
            var dct = new Dictionary<int, int>();

            dct.AddDistinctKeyValue(2, 2);
            dct.AddDistinctKeyValue(3, 3);
            dct.AddDistinctKeyValue(1, 1);

            var clone = dct.Clone();

            Assert.AreEqual(clone.Count, dct.Count, "Test passed CloneDictionary, first round:  " + dct.Count);

            _ = dct.Remove(1);

            Assert.AreNotEqual(clone.Count, dct.Count, "Test passed CloneDictionary, first round:  " + dct.Count);

            var dctAdv = new Dictionary<int, List<int>>();

            var lst = new List<int> { 1, 3 };

            dctAdv.Add(2, lst);
            dctAdv.Add(3, lst);
            dctAdv.Add(1, lst);

            Assert.AreEqual(clone.Count, dctAdv.Count,
                "Test passed CloneDictionary Collection, seconds round:  " + dctAdv.Count);

            _ = dctAdv.Remove(1);

            Assert.AreNotEqual(clone.Count, dctAdv.Count,
                "Test passed CloneDictionary Collection, second round:  " + dctAdv.Count);

            var dctAdvObj = new Dictionary<int, List<TstObj>>();

            var lstObj = new List<TstObj>();
            var obj = new TstObj { First = 1, Second = 2 };

            lstObj.Add(obj);
            lstObj.Add(obj);

            dctAdvObj.Add(2, lstObj);
            dctAdvObj.Add(3, lstObj);
            dctAdvObj.Add(1, lstObj);

            Assert.AreEqual(clone.Count, dctAdvObj.Count,
                "Test passed CloneDictionary Collection, third round:  " + dctAdvObj.Count);

            _ = dctAdvObj.Remove(1);

            Assert.AreNotEqual(clone.Count, dctAdvObj.Count,
                "Test passed CloneDictionary Collection, third round:  " + dctAdvObj.Count);
        }

        /// <summary>
        ///     Here we Test our Dictionary FindKeysByValue
        /// </summary>
        [TestMethod]
        public void DistinctDictionary()
        {
            var dct = new Dictionary<int, int>();

            dct.AddDistinctKeyValue(2, 2);
            dct.AddDistinctKeyValue(3, 3);
            dct.AddDistinctKeyValue(1, 1);

            Assert.AreEqual(3, dct.Count, "Test passed DistinctDictionary:  " + dct.Count);

            dct = dct.Sort();

            Assert.AreEqual(1, dct.First().Value, "Test passed Sort:  " + dct.First().Value);

            _ = dct.Remove(1);

            Assert.AreEqual(2, dct.First().Value, "Test passed Sort:  " + dct.First().Value);

            Assert.IsTrue(dct.IsValueDistinct(), "Test passed Values are distinct");
        }

        /// <summary>
        ///     The contains keys.
        /// </summary>
        [TestMethod]
        public void ContainsKeys()
        {
            var dct = new Dictionary<int, int>();

            dct.AddDistinctKeyValue(2, 2);
            dct.AddDistinctKeyValue(3, 3);
            dct.AddDistinctKeyValue(1, 1);

            var lst = new List<int> { 1, 2, 3 };

            Assert.IsTrue(dct.ContainsKeys(lst), "Test passed ContainsKeys, contained True");

            lst.Add(4);

            Assert.IsFalse(dct.ContainsKeys(lst), "Test passed ContainsKeys, contained False");
        }

        /// <summary>
        ///     Here we Test our Dictionary FindKeysByValue
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException), "")]
        public void ExtendedDictionaryException()
        {
            Dictionary<int, int> dct = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            dct.AddDistinct(1, 1);
        }

        /// <summary>
        ///     Here if we only truly add Distinct Values
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "")]
        public void DistinctDictionaryException()
        {
            var dct = new Dictionary<int, int>();

            dct.AddDistinctKeyValue(2, 2);
            dct.AddDistinctKeyValue(1, 2);
        }

        /// <summary>
        ///     Dictionaries the add list.
        /// </summary>
        [TestMethod]
        public void DictionaryAddList()
        {
            var dct = new Dictionary<int, List<int>> { { 1, 1 }, { 1, 1 } };

            Assert.AreEqual(2, dct[1].Count, "Correct count");

            dct.Add(0, 0);

            Assert.IsTrue(dct.ContainsKey(0), "key not added");

            dct.Add(2, 0);

            Assert.IsTrue(dct.ContainsKey(2), "key not added");

            dct.Add(2, 1);

            Assert.AreEqual(2, dct[2].Count, "Correct count");

            var dctTwo = new Dictionary<int, SortedSet<int>> { { 1, 1 }, { 1, 1 } };

            Assert.AreEqual(2, dct[1].Count, "Correct count");

            dctTwo.Add(0, 0);

            Assert.IsTrue(dct.ContainsKey(0), "key not added");

            dctTwo.Add(2, 0);

            Assert.AreEqual(2, dct[2].Count, "Correct count");

            Assert.IsTrue(dct.ContainsKey(2), "key not added");
        }

        /// <summary>
        ///     Dictionaries the add list.
        /// </summary>
        [TestMethod]
        public void DictionaryAddDisctinct()
        {
            var dct = new Dictionary<int, List<int>> { { 1, 1 }, { 1, 1 } };

            Assert.AreEqual(2, dct[1].Count, "Correct count");

            _ = dct.AddDistinct(0, 0);

            Assert.IsTrue(dct.ContainsKey(0), "key not added");

            _ = dct.AddDistinct(0, 1);

            Assert.AreEqual(2, dct[1].Count, "Correct count");

            _ = dct.AddDistinct(0, 1);

            Assert.AreEqual(2, dct[1].Count, "Correct count");
        }

        /// <summary>
        ///     Swap Test for Dictionaries
        /// </summary>
        [TestMethod]
        public void DictionarySwap()
        {
            //add some Test Data
            var loot = new Dictionary<int, Item> { { 0, new Item() }, { 10, new Item() } };

            //add some Test Data
            //first
            loot[0].Id = 1;
            loot[0].MaxStack = 5;
            loot[0].Amount = 5;

            //second
            loot[10].Id = 2;
            loot[10].MaxStack = 8;
            loot[10].Amount = 5;

            loot.Swap(0, 10);

            Assert.AreEqual(2, loot[0].Id, "Check passed");
            Assert.AreEqual(5, loot[0].Amount, "Check passed");

            Assert.AreEqual(1, loot[10].Id, "Check passed");
            Assert.AreEqual(5, loot[10].Amount, "Check passed");

            loot.Swap(10, 11);

            Assert.AreEqual(1, loot[11].Id, "Check passed");
            Assert.AreEqual(5, loot[11].Amount, "Check passed");

            Assert.IsFalse(loot.ContainsKey(10), "Check passed");
        }

        /// <summary>
        ///     remove top stack
        /// </summary>
        [TestMethod]
        public void DictionaryReduce()
        {
            var dct = new Dictionary<int, string>(_dictThree);

            _ = dct.Reduce();
            Assert.AreEqual(4, _dictThree.Count, "Check passed");
            Assert.AreEqual(3, dct.Count, "Check passed");

            for (var i = 0; i < 3; i++)
            {
                _ = dct.Reduce();
            }

            Assert.AreEqual(0, dct.Count, "Check passed");
        }

        /// <summary>
        ///     Test Interval Check
        /// </summary>
        [TestMethod]
        public void Interval()
        {
            const int i = 50;
            const int one = 49;
            const int two = 46;
            const int interval = 3;

            Assert.IsTrue(i.Interval(one, interval), "Check passed");
            Assert.IsFalse(i.Interval(two, interval), "Check passed");
        }

        /// <summary>
        ///     Test some MultiArray stuff.
        /// </summary>
        [TestMethod]
        public void MultiArray()
        {
            int[,] matrix = { { 1, 2, 3, 4 }, { 1, 2, 3, 4 }, { 5, 2, 3, 1 } };

            matrix.SwapColumn(0, 2);
            Assert.AreEqual(matrix[2, 3], 4, "4");
            Assert.AreEqual(matrix[0, 3], 1, "4");

            Assert.AreEqual(matrix[2, 0], 1, "4");
            Assert.AreEqual(matrix[0, 0], 5, "4");

            matrix = new[,] { { 1, 2, 3, 4 }, { 1, 2, 3, 4 }, { 5, 2, 3, 1 } };

            matrix.SwapRow(0, 1);
            Assert.AreEqual(matrix[0, 0], 2, "4");
            Assert.AreEqual(matrix[0, 1], 1, "4");

            Assert.AreEqual(matrix[2, 0], 2, "4");
            Assert.AreEqual(matrix[2, 1], 5, "4");

            var str = matrix.ToText();
            Assert.IsFalse(string.IsNullOrEmpty(str), "Null string");
            Assert.AreEqual(str.Length, 45, "Count");

            int[,] normal = { { 1, 2 }, { 1, 2 } };
            var copy = normal.Duplicate();
            normal[0, 0] = 0;
            Assert.AreEqual(copy[0, 0], 1, "00");
            Assert.AreEqual(copy[0, 1], 2, "01");

            //see Matrix Multiplication, the speed is sadly slower, but it converts arrays to span in a fast way and the results are the same

            var x = new double[,] { { 2, 1, 1 }, { 1, 2, 1 }, { 1, 1, 2 } };
            var m1 = new BaseMatrix { Matrix = x };

            var y = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
            var m2 = new BaseMatrix { Matrix = y };

            var compare1 = HelperMethods.MatrixTestOne(m1, m2);

            Assert.AreEqual(compare1[0, 0], 2, "00");
            Assert.AreEqual(compare1[1, 0], 1, "10");
            Assert.AreEqual(compare1[2, 0], 1, "20");
            Assert.AreEqual(compare1[0, 1], 1, "01");
            Assert.AreEqual(compare1[1, 1], 2, "11");
            Assert.AreEqual(compare1[2, 1], 1, "21");
            Assert.AreEqual(compare1[0, 2], 1, "02");
            Assert.AreEqual(compare1[1, 2], 1, "12");
            Assert.AreEqual(compare1[2, 2], 2, "22");
        }

        /// <summary>
        ///     Converts the jagged double array to rectangular array.
        /// </summary>
        [TestMethod]
        public void ConvertJaggedDoubleArrayToRectangularArray()
        {
            // Arrange
            double[][] jaggedArray = { new[] { 1.1, 1.2, 1.3 }, new[] { 2.1, 2.2, 2.3 }, new[] { 3.1, 3.2, 3.3 } };

            double[,] expected = { { 1.1, 1.2, 1.3 }, { 2.1, 2.2, 2.3 }, { 3.1, 3.2, 3.3 } };

            // Act
            var result = jaggedArray.Convert();

            // Assert
            Assert.AreEqual(expected.GetLength(0), result.GetLength(0), "Row count mismatch.");
            Assert.AreEqual(expected.GetLength(1), result.GetLength(1), "Column count mismatch.");
            for (var i = 0; i < expected.GetLength(0); i++)
            {
                for (var j = 0; j < expected.GetLength(1); j++)
                {
                    Assert.AreEqual(expected[i, j], result[i, j], $"Value mismatch at [{i}, {j}].");
                }
            }
        }

        [TestMethod]
        public void IdInterface()
        {
            var dct = new Dictionary<int, Item> { { 0, new Item() }, { 10, new Item() } };

            //add some Test Data
            //first
            dct[0].Id = 1;
            dct[0].MaxStack = 5;
            dct[0].Amount = 5;

            //second
            dct[10].Id = 2;
            dct[10].MaxStack = 8;
            dct[10].Amount = 5;

            //To List
            var lst = dct.ToListId();
            Assert.AreEqual(lst[0].Id, 0, "Check passed");
            Assert.AreEqual(lst[1].Id, 10, "Check passed");

            lst[0].Id = 5;
            lst[1].Id = 6;

            var dic = lst.ToDictionaryId<Item, int>();
            Assert.AreEqual(dic.Count, 2, "Check passed");
            Assert.IsTrue(dic.ContainsKey(5), "Key does not exist");
            Assert.IsTrue(dic.ContainsKey(6), "Key does not exist");
        }

        /// <summary>
        ///     The Test obj class.
        /// </summary>
        private sealed class TstObj
        {
            /// <summary>
            ///     Gets or sets the first.
            /// </summary>
            internal int First { get; init; }

            /// <summary>
            ///     Gets or sets the second.
            /// </summary>
            internal int Second { get; init; }
        }

        /// <summary>
        ///     Basic Object for Testing
        /// </summary>
        private sealed class Item : IIdHandling<int>
        {
            /// <summary>Gets or sets the amount.</summary>
            /// <value>The amount.</value>
            public int Amount { get; set; }

            /// <summary>Gets or sets the maximum stack.</summary>
            /// <value>The maximum stack.</value>
            public int MaxStack { get; set; } = 1;

            /// <summary>Gets or sets the identifier.</summary>
            /// <value>The identifier.</value>
            public int Id { get; set; } = -1;
        }
    }
}
