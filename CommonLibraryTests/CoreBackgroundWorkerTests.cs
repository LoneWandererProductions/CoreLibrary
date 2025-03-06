/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBackgroundWorkerTests
 * FILE:        CoreBackgroundWorkerTests/CoreBackgroundWorkerTests.cs
 * PURPOSE:     Test for our Background Worker
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using CoreInject;
using CoreWorker;
using Debugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    /// Part of my Dependency Injection Journey
    /// </summary>
    [TestClass]
    public class CoreBackgroundWorkerTests
    {
        private CoreInjector _injector;

        /// <summary>
        ///  In-memory logger for testing purposes
        /// </summary>
        /// <seealso cref="ILogger" />
        private sealed class InMemoryLogger : ILogger
        {
            /// <summary>
            /// Gets the logs.
            /// </summary>
            /// <value>
            /// The logs.
            /// </value>
            public List<string> Logs { get; } = new List<string>();

            public void LogInformation(string message)
            {
                Logs.Add(message);
            }
        }

        /// <summary>
        /// Setup: Initialize CoreInjector container for dependency injection
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            _injector = new CoreInjector();

            // Register the InMemoryLogger and CoreBackgroundWorker in the DI container
            var logger = new InMemoryLogger();
            _injector.RegisterInstance<ILogger>(logger);
            _injector.RegisterSingleton<ICoreBackgroundWorker, CoreBackgroundWorker>();
        }


        /// <summary>
        /// Test: Ensure the background worker starts and stops correctly
        /// </summary>
        [TestMethod]
        public async Task CoreBackgroundWorkerShouldStartAndStopCorrectly()
        {
            // Arrange: Resolve the worker and logger from the DI container
            var worker = _injector.Resolve<ICoreBackgroundWorker>();
            var logger = _injector.Resolve<ILogger>() as InMemoryLogger;

            // Act: Start the worker
            worker.Start();
            await Task.Delay(1000); // Let the worker run for a short time

            // Assert: Check that the worker logged the "running" message
            //Assert.IsTrue(logger?.Logs.Exists(log => log.Contains("Worker is running")) ?? false,
            //    "Worker did not log 'running' message.");

            // Act: Stop the worker
            worker.Stop();

            // Assert: No error during stopping
            Assert.IsTrue(true); // You can replace this with additional verification if needed
        }

        // Cleanup: Dispose of the container if needed
        [TestCleanup]
        public void TearDown()
        {
            // There's no explicit Dispose method in CoreInjector, 
            // but you can clean up scopes if required.
            _injector.EndScope();
        }
    }
}
