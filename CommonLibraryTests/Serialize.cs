/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/Serialize.cs
 * PURPOSE:     Test for Serializer
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

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
            var lst = new List<XmlItem> {ResourcesGeneral.DataItemOne, ResourcesGeneral.DataItemTwo};

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
        ///     Check if the whole cvs stuff works.
        /// </summary>
        [TestMethod]
        public void Cvs()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), nameof(Serialize),
                "test.cvs");
            var lst = new List<List<string>>();

            for (var i = 0; i < 10; i++)
            {
                var line = new List<string>();

                for (var j = 0; j < 10; j++)
                {
                    line.Add(j.ToString());
                }

                lst.Add(line);
            }

            CsvHandler.WriteCsv(path, lst);

            Assert.IsTrue(File.Exists(path), "File exists");

            lst = CsvHandler.ReadCsv(path, ',');

            Assert.AreEqual("0", lst[1][0], "Right Element");

            Assert.AreEqual("9", lst[2][9], "Right Element");

            FileHandleDelete.DeleteFile(path);
        }
    }
}
