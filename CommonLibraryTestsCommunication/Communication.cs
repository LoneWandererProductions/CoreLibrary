/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTestsCommunication
 * FILE:        Communication.cs
 * PURPOSE:     Test for our communication
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.IO;
using Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTestsCommunication
{
    [TestClass]
    public class Communication
    {
        private readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), nameof(Communication));

        [TestMethod]
        public void Communications()
        {
            var communication = new Com();
            communication.SaveFile(_path, "https://www.google.de/");
            var path = Path.Combine(Directory.GetCurrentDirectory(), nameof(Communication), "www.google.de");

            Assert.IsTrue(File.Exists(path), "File not found");

            File.Delete(path);
        }
    }
}
