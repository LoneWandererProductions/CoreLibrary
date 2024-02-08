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
            ExDictionary<int, int> dict = new ExDictionary<int, int>();
            dict.Add(-1, -1);
            dict.Add(1, -1);
            dict.Add(9, -1);
            dict.ContainsKey(9);
            dict.GetValue(9);
            dict[10] = 10;

            foreach (var item in dict)
            {
                Trace.WriteLine(item.Key);
                Trace.WriteLine(item.Value);
            }
            Trace.WriteLine(dict[10]);
        }
    }
}
