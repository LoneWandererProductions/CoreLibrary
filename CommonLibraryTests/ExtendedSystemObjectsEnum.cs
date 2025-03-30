/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/ExtendedSystemObjectsEnum.cs
 * PURPOSE:     Tests my dynamic Enum
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExtendedSystemObjects;

namespace CommonLibraryTests
{
    /// <summary>
    /// Test Class for our enum Implementation
    /// </summary>
    /// <seealso cref="ExtendedSystemObjects.DynamicEnum&lt;CommonLibraryTests.MyEnum&gt;" />
    /// <seealso cref="MyEnum" />
    public sealed class MyEnum : DynamicEnum<MyEnum>
    {
        public static readonly MyEnum OptionA = new("OptionA", 1);

        /// <summary>
        /// The option b
        /// </summary>
        public static readonly MyEnum OptionB = new("OptionB", 2);

        /// <summary>
        /// Initializes a new instance of the <see cref="MyEnum"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public MyEnum(string name, int value) : base(name, value) { }
    }


    [TestClass]
    public class ExtendedSystemObjectsEnum
    {
        [TestMethod]
        public void TestPredefinedOptions()
        { 
            var optionA = MyEnum.OptionA;
            var optionB = MyEnum.OptionB;

            Assert.AreEqual("OptionA", optionA.Name);
            Assert.AreEqual(1, optionA.Value);
            Assert.AreEqual("OptionB", optionB.Name);
            Assert.AreEqual(2, optionB.Value);
        }

        [TestMethod]
        public void TestAddNewEntry()
        {
            var customOption = MyEnum.Add("CustomOption", 3);
            Assert.AreEqual("CustomOption", customOption.Name);
            Assert.AreEqual(3, customOption.Value);

            // Verify it exists in the collection
            Assert.IsTrue(MyEnum.GetAll().Contains(customOption));
        }

        [TestMethod]
        public void TestRemoveEntry()
        {
            var customOption = MyEnum.Add("TempOption", 99);
            Assert.IsTrue(MyEnum.GetAll().Contains(customOption));

            MyEnum.Remove("TempOption");

            // Ensure it was removed
            Assert.IsFalse(MyEnum.GetAll().Contains(customOption));
        }

        [TestMethod]
        public void TestSwitchStatement()
        {
            var myEnum = MyEnum.OptionA;
            string result = myEnum.Name switch
            {
                "OptionA" => "Matched OptionA",
                "OptionB" => "Matched OptionB",
                _ => "Unknown Option"
            };

            Assert.AreEqual("Matched OptionA", result);
        }

        [TestMethod]
        public void TestTryGetExisting()
        {
            bool exists = MyEnum.TryGet("OptionA", out var result);
            Assert.IsTrue(exists);
            Assert.IsNotNull(result);
            Assert.AreEqual("OptionA", result!.Name);
        }

        [TestMethod]
        public void TestTryGetNonExisting()
        {
            bool exists = MyEnum.TryGet("NonExisting", out var result);
            Assert.IsFalse(exists);
            Assert.IsNull(result);
        }
    }
}
