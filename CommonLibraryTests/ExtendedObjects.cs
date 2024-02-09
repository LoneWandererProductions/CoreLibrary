using System.Diagnostics;
using ExtendedSystemObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    [TestClass]
    public class ExtendedObjects
    {
        [TestMethod]
        public void ExtendedDictionary()
        {
            ExDictionary<int, int> dict = new ExDictionary<int, int> {{-1, -1}, {1, -1}, {9, -1}};
            var check =dict.ContainsKey(9);
            if(check)
            {
                var value = dict.GetValue(9);
                Assert.AreEqual(-1, value, "Wrong Value");
            }

            dict[10] = 10;

            //Assert.AreEqual(4, dict.Count, "Wrong Count");

            foreach (var (key, value) in dict)
            {
                Trace.Write(key);
                Trace.Write(", ");
                Trace.WriteLine(value);
            }
            Trace.WriteLine(dict[10]);
        }
    }
}
