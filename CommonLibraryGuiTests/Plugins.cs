/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryGuiTests
 * FILE:        CommonLibraryGuiTests/Plugins.cs
 * PURPOSE:     Tests for Plugin Loader
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.IO;
using System.Linq;
using System.Threading;
using FileHandler;
using NUnit.Framework;
using PluginLoader;

namespace CommonLibraryGuiTests;

public sealed class Plugins
{
    /// <summary>
    ///     Test the  Loads Plugin.
    ///     with my custom SqlLite Frontend
    /// </summary>
    [Test]
    [Apartment(ApartmentState.STA)]
    public void LoadPluginSqliIte()
    {
        var root = DirectoryInformation.GetParentDirectory(3);
        var target = Path.Combine(root, @"SqlLiteGui\bin\Debug\net9.0-windows");
        var check = PluginLoad.LoadAll(target);

        Assert.AreEqual(1, PluginLoad.PluginContainer.Count, "done");
        Assert.IsTrue(check, "done");

        var command = PluginLoad.PluginContainer.FirstOrDefault();

        if (command == null)
        {
            Assert.Fail("Value was null");
            return;
        }

        var code = command.Execute();

        Assert.AreEqual(0, code, "done");

        code = command.Close();

        Assert.AreEqual(0, code, "done");
    }
}
