/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/PluginLoadTests.cs
 * PURPOSE:     Bare bones tests for the plugin system
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Plugin;
using PluginLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CommonLibraryTests
{
    /// <summary>
    /// Some more basic tests for the plugin system, right now nothing serious
    /// </summary>
    [TestClass]
    public class PluginLoadTests
    {
        /// <summary>
        /// Creates the commands invalid assembly throws argument exception.
        /// </summary>
        [TestMethod]
        public void CreateCommandsInvalidAssemblyThrowsArgumentException()
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly();
            var invalidAssembly = typeof(string).Assembly;

            // Act & Assert
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                InvokePrivateStaticMethod<IEnumerable<IPlugin>>(typeof(PluginLoad), "CreateCommands", invalidAssembly).ToList();
            });
        }

        /// <summary>
        /// Gets the files by extension full path invalid path returns null.
        /// </summary>
        [TestMethod]
        public void GetFilesByExtensionFullPathInvalidPathReturnsNull()
        {
            // Arrange
            var invalidPath = "invalid_path";
            var extension = ".dll";

            // Act
            var result = InvokePrivateStaticMethod<IEnumerable<string>>(typeof(PluginLoad), "GetFilesByExtensionFullPath", invalidPath, extension);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Loads all no plugins found returns false.
        /// </summary>
        [TestMethod]
        public void LoadAllNoPluginsFoundReturnsFalse()
        {
            // Arrange
            var pluginPaths = InvokePrivateStaticMethod<IEnumerable<string>>(typeof(PluginLoad), "GetFilesByExtensionFullPath", "invalid_path", ".dll");

            // Assert
            Assert.IsNull(pluginPaths);
        }

        /// <summary>
        /// Sets the environment variables valid store returns true.
        /// </summary>
        [TestMethod]
        public void SetEnvironmentVariablesValidStoreReturnsTrue()
        {
            // Arrange
            var store = new Dictionary<int, object>
        {
            { 1, "Value1" },
            { 2, "Value2" }
        };

            // Act
            var result = PluginLoad.SetEnvironmentVariables(store);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(store, DataRegister.Store);
        }

        /// <summary>
        /// Sets the environment variables null store returns false.
        /// </summary>
        [TestMethod]
        public void SetEnvironmentVariablesNullStoreReturnsFalse()
        {
            // Act
            var result = PluginLoad.SetEnvironmentVariables(null);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Invokes the private static method.
        /// </summary>
        /// <typeparam name="T">Generic Type Paramter</typeparam>
        /// <param name="type">The type.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Method call to private method</returns>
        /// <exception cref="ArgumentException">Method '{methodName}' not found. - methodName</exception>
        private static T InvokePrivateStaticMethod<T>(Type type, string methodName, params object[] parameters)
        {
            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
                throw new ArgumentException($"Method '{methodName}' not found.", nameof(methodName));

            return (T)method.Invoke(null, parameters);
        }
    }

    /// <summary>
    /// Plugin for tests
    /// </summary>
    /// <seealso cref="Plugin.IPlugin" />
    public class MockPlugin : IPlugin
    {
        /// <summary>
        /// Gets the name.
        /// This field must be equal to the file name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name => "MockPlugin";
        
        /// <summary>
        /// Gets the type.
        /// This field is optional.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type => "MockType";
        
        /// <summary>
        /// Gets the description.
        /// This field is optional.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description => "Mock plugin for testing";

        /// <summary>
        /// Gets the version.
        /// This field is optional.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public Version Version => new Version(1, 0, 0);

        /// <summary>
        /// Gets the possible commands for the Plugin.
        /// This field is optional.
        /// </summary>
        /// <value>
        /// The commands that the main module can call from the plugin.
        /// </value>
        public List<Command> Commands => new List<Command>();

        /// <summary>
        /// Executes this instance.
        /// Absolute necessary.
        /// </summary>
        /// <returns>
        /// Status Code
        /// </returns>
        public int Execute() => 0;

        /// <summary>
        /// Executes the command.
        /// Returns the result as object.
        /// If we allow plugins, we must know what the plugin returns beforehand.
        /// Based on the architecture say an image Viewer. The base module that handles most images is a plugin and always
        /// returns a BitMapImage.
        /// Every new plugin for Image viewing must nur return the same.
        /// So if we add a plugin for another Image type, we define the plugin as Image Codec for example.
        /// The main module now always expects a BitMapImage as return value.
        /// This method is optional.
        /// </summary>
        /// <param name="id">The identifier of the command.</param>
        /// <returns>
        /// Result object
        /// </returns>
        public object ExecuteCommand(int id) => null;

        /// <summary>
        /// Returns the type of the plugin. Defined by the coder.
        /// As already mentioned in ExecuteCommand, we need to know what we can expect as return value from this Plugin.
        /// With this the main module can judge what to expect from the plugin.
        /// This method is optional.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// int as Id, can be used by the dev to define or get the type of Plugin this is
        /// </returns>
        public int GetPluginType(int id) => 0;

        /// <summary>
        /// Gets the basic information of the plugin human readable.
        /// This method is optional.
        /// </summary>
        /// <returns>
        /// Info about the plugin
        /// </returns>
        public string GetInfo() => "Mock plugin info";

        /// <summary>
        /// Closes this instance.
        /// This method is optional.
        /// </summary>
        /// <returns>
        /// Status Code
        /// </returns>
        public int Close() => 0;
    }

    /// <summary>
    /// Plugin for tests, async
    /// </summary>
    /// <seealso cref="Plugin.IAsyncPlugin" />
    public class MockAsyncPlugin : IAsyncPlugin
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name => "MockAsyncPlugin";
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type => "MockType";

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description => "Mock async plugin for testing";

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public Version Version => new Version(1, 0, 0);

        /// <summary>
        /// Gets the commands.
        /// </summary>
        /// <value>
        /// The commands.
        /// </value>
        public List<Command> Commands => new List<Command>();

        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <returns>
        /// Status Code asnc
        /// </returns>
        public Task<int> ExecuteAsync() => Task.FromResult(0);

        /// <summary>
        /// Executes the command asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// Result object, async.
        /// </returns>
        public Task<object> ExecuteCommandAsync(int id) => Task.FromResult<object>(null);

        /// <summary>
        /// Gets the plugin type asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// Status Code asnc
        /// </returns>
        public Task<int> GetPluginTypeAsync(int id) => Task.FromResult(0);

        /// <summary>
        /// Closes asynchronous.
        /// </summary>
        /// <returns>
        /// Status Code
        /// </returns>
        public Task<int> CloseAsync() => Task.FromResult(0);

        /// <summary>
        /// Gets the information.
        /// </summary>
        /// <returns>
        /// Info about the plugin
        /// </returns>
        public string GetInfo() => "Mock async plugin info";
    }

}
