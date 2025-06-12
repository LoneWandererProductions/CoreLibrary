/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreBackgroundWorkerTests
 * FILE:        CoreBackgroundWorkerTests/CoreBackgroundWorkerTests.cs
 * PURPOSE:     Test for our Background Worker
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreInject;
using CoreMemoryLog;
using CoreWorker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreBuilderTests
{
    /// <summary>
    ///     Part of my Dependency Injection Journey
    /// </summary>
    [TestClass]
    public class CoreBackgroundWorkerTests
    {
        /// <summary>
        ///     The injector
        /// </summary>
        private CoreInjector _injector;

        /// <summary>
        ///     Setup: Initialize CoreInjector container for dependency injection
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            _injector = new CoreInjector();

            // Register ILogger first
            _injector.RegisterInstance<ILogger>(new InMemoryLogger());

            // Register TestBackgroundWorker with constructor injection
            _injector.RegisterSingleton<ICoreBackgroundWorker>(injector =>
            {
                var logger = injector.Resolve<ILogger>();
                return new TestBackgroundWorker(logger); // Inject the logger
            });
        }

        /// <summary>
        ///     Cores the background worker should start and stop correctly.
        /// </summary>
        [TestMethod]
        public async Task CoreBackgroundWorkerShouldStartAndStopCorrectly()
        {
            // Arrange: Resolve the worker and logger from the DI container
            var worker = _injector.Resolve<ICoreBackgroundWorker>();
            var logger = _injector.Resolve<ILogger>() as InMemoryLogger;

            // Act: Start the worker
            worker.Start();
            await Task.Delay(3000); // Wait longer to allow logging to occur

            // Assert: Check that the worker logged the "running" message
            var log = logger.GetLog();
            Assert.IsTrue(log.Any(l => l.Message.Contains("Test worker is running")),
                "Worker did not log 'running' message.");
            Assert.IsTrue(log.Any(l => l.Message.Contains("Test worker has started")),
                "Worker did not log 'start' message.");

            // Act: Stop the worker
            worker.Stop();

            log = logger.GetLog();
            // Assert: Check that the worker logged the "stopped" message
            Assert.IsTrue(log.Any(l => l.Message.Contains("Worker was canceled")),
                "Worker did not log 'stopped' message.");
        }

        /// <summary>
        ///     Cores the background worker should handle cancellation.
        /// </summary>
        [TestMethod]
        public async Task CoreBackgroundWorkerShouldHandleCancellation()
        {
            // Arrange: Resolve the worker and logger from the DI container
            var worker = _injector.Resolve<ICoreBackgroundWorker>();
            var logger = _injector.Resolve<ILogger>() as InMemoryLogger;

            // Act: Start the worker and cancel it shortly after
            worker.Start();
            await Task.Delay(1000); // Give the worker time to start
            worker.Stop(); // Stop the worker before it finishes

            // Assert: Check cancellation logging
            var log = logger.GetLog();
            Assert.IsTrue(log.Any(l => l.Message.Contains("Worker was canceled")),
                "Worker did not log cancellation.");
        }

        /// <summary>
        ///     Cleanup: Dispose of the container if needed
        ///     Tears down.
        /// </summary>
        [TestCleanup]
        public void TearDown()
        {
            // There's no explicit Dispose method in CoreInjector,
            // but you can clean up scopes if required.
            _injector.EndScope();
        }

        /// <summary>
        ///     Concrete implementation of CoreBackgroundWorker for testing.
        /// </summary>
        /// <summary>
        ///     Concrete implementation of CoreBackgroundWorker for testing.
        /// </summary>
        private class TestBackgroundWorker : ICoreBackgroundWorker
        {
            /// <summary>
            ///     The logger
            /// </summary>
            private readonly ILogger _logger;

            /// <summary>
            ///     The cancellation token source
            /// </summary>
            private CancellationTokenSource _cancellationTokenSource;

            /// <summary>
            ///     Initializes a new instance of the <see cref="TestBackgroundWorker" /> class.
            /// </summary>
            public TestBackgroundWorker()
            {
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="TestBackgroundWorker" /> class.
            /// </summary>
            /// <param name="logger">The logger.</param>
            /// <exception cref="System.ArgumentNullException">logger</exception>
            public TestBackgroundWorker(ILogger logger)
            {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            // Optional: Override Start if you want to add more behavior when starting
            public void Start()
            {
                _logger.LogInformation("Test worker has started.");

                _cancellationTokenSource = new CancellationTokenSource();
                // Start the worker task
                Task.Run(() => ExecuteAsync(_cancellationTokenSource.Token));
            }

            public void Stop()
            {
                _logger.LogInformation("Worker was canceled.");
                // Cancel the operation if it's running
                _cancellationTokenSource?.Cancel();
            }

            /// <summary>
            ///     Defines the worker task that must be implemented in a derived class.
            /// </summary>
            /// <param name="cancellationToken">The cancellation token.</param>
            public async Task ExecuteAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("Test worker is running..."); // Log the message
                await Task.Delay(500, cancellationToken); // Simulate work with a short delay
            }
        }
    }
}
