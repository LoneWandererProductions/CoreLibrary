/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrary.Tests
 * FILE:        InterOps.cs
 * PURPOSE:     We try to test basic Interop functions
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using InterOp;

namespace CommonLibrary.Tests
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
