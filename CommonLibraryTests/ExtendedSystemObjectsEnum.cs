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

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CommonLibraryTests.MyEnum" /> class.
        /// Must be public!
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public MyEnum(string name, int value) : base(name, value) { }
    }


    /// <summary>
    /// Test our enums
    /// </summary>
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

        /// <summary>
        /// Tests the add new entry.
        /// </summary>
        [TestMethod]
        public void TestAddNewEntry()
        {
            var customOption = MyEnum.Add("CustomOption", 3);
            Assert.AreEqual("CustomOption", customOption.Name);
            Assert.AreEqual(3, customOption.Value);

            // Verify it exists in the collection
            Assert.IsTrue(MyEnum.GetAll().Contains(customOption));
        }

        /// <summary>
        /// Tests the remove entry.
        /// </summary>
        [TestMethod]
        public void TestRemoveEntry()
        {
            var customOption = MyEnum.Add("TempOption", 99);
            Assert.IsTrue(MyEnum.GetAll().Contains(customOption));

            MyEnum.Remove("TempOption");

            // Ensure it was removed
            Assert.IsFalse(MyEnum.GetAll().Contains(customOption));
        }

        /// <summary>
        /// Tests the switch statement.
        /// </summary>
        [TestMethod]
        public void TestSwitchStatement()
        {
            // Example DynamicEnum initialization (you should define your own classes inheriting DynamicEnum)
            var myEnum = MyEnum.OptionA; // Assuming MyDynamicEnum is derived from DynamicEnum<T>

            string result = myEnum.Value switch
            {
                1 => "Matched OptionA", // Assuming OptionA has a Value of 1
                2 => "Matched OptionB", // Assuming OptionB has a Value of 2
                _ => "Unknown Option"
            };

            Assert.AreEqual("Matched OptionA", result);
        }


        /// <summary>
        /// Tests the try get existing.
        /// </summary>
        [TestMethod]
        public void TestTryGetExisting()
        {
            bool exists = MyEnum.TryGet("OptionA", out var result);
            Assert.IsTrue(exists);
            Assert.IsNotNull(result);
            Assert.AreEqual("OptionA", result!.Name);
        }

        /// <summary>
        /// Tests the try get non existing.
        /// </summary>
        [TestMethod]
        public void TestTryGetNonExisting()
        {
            bool exists = MyEnum.TryGet("NonExisting", out var result);
            Assert.IsFalse(exists);
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests the pattern matching.
        /// </summary>
        [TestMethod]
        public void TestPatternMatching()
        {
            var optionA = MyEnum.OptionA;
            var optionB = MyEnum.OptionB;

            // Check if pattern matching works for OptionA
            var resultA = GetOptionDescription(optionA);
            Assert.AreEqual("This is Option A with value 1.", resultA);

            // Check if pattern matching works for OptionB
            var resultB = GetOptionDescription(optionB);
            Assert.AreEqual("This is Option B with value 2.", resultB);
        }

        // The method that uses pattern matching
        /// <summary>
        /// Gets the option description.
        /// </summary>
        /// <param name="option">The option.</param>
        /// <returns>Test our enum</returns>
        private static string GetOptionDescription(DynamicEnum<MyEnum> option)
        {
            return option switch
            {
                var o when o == MyEnum.OptionA => "This is Option A with value 1.",
                var o when o == MyEnum.OptionB => "This is Option B with value 2.",
                _ => "Unknown Option"
            };
        }
    }
}
