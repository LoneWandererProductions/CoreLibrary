/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGuiTests
 * FILE:        CommonLibraryGuiTests/Debugger.cs
 * PURPOSE:     Tests the Debugger, mostly config file
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Diagnostics;
using System.Threading;
using Debugger;
using NUnit.Framework;

namespace CommonLibraryGuiTests
{
    public sealed class Debugger
    {
        /// <summary>
        ///     Test the Config saving and loading in the Debugger
        /// </summary>
        [Test]
        [Apartment(ApartmentState.STA)]
        public void LoadConfig()
        {
            var log = new DebugLog();
            log.Start();
            Assert.IsTrue(DebugRegister.IsRunning, "Status not correct");
            Assert.IsFalse(DebugRegister.IsDumpActive, "Dump failed");

            DebugRegister.IsDumpActive = true;
            log.StopDebugging();
            Assert.IsFalse(DebugRegister.IsRunning, "Status not correct");
            Assert.IsTrue(DebugRegister.IsDumpActive, "Dump set");

            var path = DebugRegister.DebugPath;

            DebugRegister.ReadConfigFile();

            Trace.WriteLine(path);

            Assert.IsTrue(DebugRegister.IsDumpActive, "Dump loaded");
        }
    }
}
