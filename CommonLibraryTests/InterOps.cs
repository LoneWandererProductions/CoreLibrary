/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/InterOps.cs
 * PURPOSE:     We try to test basic Interop functions
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using InterOp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     InterOps testing
    /// </summary>
    [TestClass]
    public class InterOps
    {
        /// <summary>
        ///     Registry test.
        /// </summary>
        [TestMethod]
        public void Registry()
        {
            var registry = new RegistryHandler();

            var results = registry.GetRegistryObjects(@"SOFTWARE\Classes\CLSID");

            Assert.AreNotEqual(0, results.Count, "Nothing was found");
        }
    }
}
