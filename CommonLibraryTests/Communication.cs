/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        Communication.cs
 * PURPOSE:     Test for our communication
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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
        ///     Communications instance.
        /// </summary>
        [TestMethod]
        public async Task CommunicationsAsync()
        {
            var communication = new NetCom();
            _ = await communication.SaveFile(_path, "https://www.google.de/");
            var path = Path.Combine(Directory.GetCurrentDirectory(), nameof(Communication), "www.google.de");

            Assert.IsTrue(File.Exists(path), "File not found");

            File.Delete(path);
        }

        [TestMethod]
        public async Task Untested()
        {
            //var str = await HttpClientManager.CallSoapServiceAsync();

            //Trace.WriteLine(str);
        }
    }
}
