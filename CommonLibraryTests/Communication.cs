/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        Communication.cs
 * PURPOSE:     Test for our communication
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.IO;
using Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    [TestClass]
    public class Communication
    {
        /// <summary>
        ///     The path
        /// </summary>
        private readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), nameof(Communication));

        /// <summary>
        ///     Communicationses this instance.
        /// </summary>
        [TestMethod]
        public void Communications()
        {
            var communication = new Com();
            _ = communication.SaveFile(_path, "https://www.google.de/");
            var path = Path.Combine(Directory.GetCurrentDirectory(), nameof(Communication), "www.google.de");

            Assert.IsTrue(File.Exists(path), "File not found");

            File.Delete(path);
        }
    }
}
