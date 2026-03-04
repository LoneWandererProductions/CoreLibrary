/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        FileObserverTests.cs
 * PURPOSE:     Some tests for FileObserver. Not exhaustive, but a good start to validate the core functionality and catch regressions.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */


using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileHandler;

namespace CommonLibraryTests
{
    [TestClass]
    public class FileObserverTests
    {
        /// <summary>
        /// The temporary directory
        /// </summary>
        private string _tempDirectory = string.Empty;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Runs before EVERY test to create a pristine, isolated folder
            _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDirectory);
        }

        /// <summary>
        /// Cleanups this instance.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            // Runs after EVERY test to wipe the folder and leave no trace
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }

        /// <summary>
        /// Constructors the when directory does not exist throws directory not found exception.
        /// </summary>
        [TestMethod]
        public void Constructor_WhenDirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            var badPath = Path.Combine(_tempDirectory, "NonExistentFolder");

            // Act & Assert
            Assert.ThrowsException<DirectoryNotFoundException>(() => new FileObserver(badPath));
        }

        /// <summary>
        /// Creates the event when file is created fires successfully.
        /// </summary>
        [TestMethod]
        public async Task CreatedEvent_WhenFileIsCreated_FiresSuccessfully()
        {
            // Arrange
            using var observer = new FileObserver(_tempDirectory);
            var eventFired = false;

            observer.Created += args =>
            {
                eventFired = true;
                return Task.CompletedTask;
            };

            // Act
            observer.Start();

            // Create a file to trigger the OS event
            var testFile = Path.Combine(_tempDirectory, "test.txt");
            await File.WriteAllTextAsync(testFile, "Hello World");

            // Give the OS and the observer a moment to process the event
            await Task.Delay(100);

            // Assert
            Assert.IsTrue(eventFired, "The Created event did not fire.");
        }

        /// <summary>
        /// Changes the event when spammed debounces and fires only once.
        /// </summary>
        [TestMethod]
        public async Task ChangedEvent_WhenSpammed_DebouncesAndFiresOnlyOnce()
        {
            // Arrange
            using var observer = new FileObserver(_tempDirectory);
            var testFile = Path.Combine(_tempDirectory, "spam.txt");

            // Create the initial file before we start watching so it doesn't trigger 'Created'
            await File.WriteAllTextAsync(testFile, "Initial Setup");

            var eventCount = 0;
            observer.Changed += args =>
            {
                Interlocked.Increment(ref eventCount); // Thread-safe counter
                return Task.CompletedTask;
            };

            observer.Start();

            // Act: Spam the file with 5 rapid saves
            for (var i = 0; i < 5; i++)
            {
                await using var fs = new FileStream(testFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                await using var writer = new StreamWriter(fs);
                await writer.WriteAsync($"Spam {i}");
            }

            // Wait just slightly longer than your 200ms debounce limit
            await Task.Delay(300);

            // Assert
            // Without your debounce logic, this would be 5 (or more, due to OS quirks).
            // With your logic, it should be exactly 1.
            Assert.AreEqual(1, eventCount);
        }

        /// <summary>
        /// Runs the until cancelled asynchronous when token is cancelled exits gracefully.
        /// </summary>
        [TestMethod]
        public async Task RunUntilCancelledAsync_WhenTokenIsCancelled_ExitsGracefully()
        {
            // Arrange
            using var observer = new FileObserver(_tempDirectory);
            using var cts = new CancellationTokenSource();

            // Act
            var runTask = observer.RunUntilCancelledAsync(cts.Token);

            // Cancel it almost immediately
            cts.Cancel();

            // Await the task. If it throws an unhandled exception, the test fails. 
            await runTask;

            // Assert
            Assert.IsTrue(runTask.IsCompletedSuccessfully);
        }
    }
}
