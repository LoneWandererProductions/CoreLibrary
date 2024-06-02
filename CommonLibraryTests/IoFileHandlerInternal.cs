/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     CommonLibraryTests
* FILE:        CommonLibraryTests/IoFileHandlerInternal.cs
* PURPOSE:     Tests for IoFileHandler, internal stuff
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     Basic File Operations
    /// </summary>
    [TestClass]
    public sealed class IoFileHandlerInternal
    {
        /// <summary>
        ///     Cleans up extension list null input throws argument null exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CleanUpExtensionListNullInputThrowsArgumentNullException()
        {
            // Act
            FileHandlerProcessing.CleanUpExtensionList(null);
        }

        /// <summary>
        ///     Cleans up extension list valid input removes dots.
        /// </summary>
        [TestMethod]
        public void CleanUpExtensionListValidInputRemovesDots()
        {
            // Arrange
            var input = new List<string> { ".txt", ".csv", ".docx" };
            var expected = new List<string> { "txt", "csv", "docx" };

            // Act
            var result = FileHandlerProcessing.CleanUpExtensionList(input);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        /// <summary>
        ///     Gets the sub folder invalid element path throws argument exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSubFolderInvalidElementPathThrowsArgumentException()
        {
            // Arrange
            string element = null;
            var root = @"C:\root\folder";
            var target = @"D:\target";

            // Act
            FileHandlerProcessing.GetSubFolder(element, root, target);
        }

        /// <summary>
        ///     Gets the sub folder invalid root path throws argument exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSubFolderInvalidRootPathThrowsArgumentException()
        {
            // Arrange
            var element = @"C:\root\folder\subfolder\file.txt";
            string root = null;
            var target = @"D:\target";

            // Act
            FileHandlerProcessing.GetSubFolder(element, root, target);
        }

        /// <summary>
        ///     Gets the sub folder valid paths returns relative path.
        /// </summary>
        [TestMethod]
        public void GetSubFolderValidPathsReturnsRelativePath()
        {
            // Arrange
            var element = @"C:\root\folder\subfolder\";
            var root = @"C:\root\folder";
            var target = @"D:\target";

            var expected = @"D:\target\subfolder\";

            // Act
            var result = FileHandlerProcessing.GetSubFolder(element, root, target);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        ///     Gets the files by extension empty path throws file handler exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileHandlerException))]
        public void GetFilesByExtensionEmptyPathThrowsFileHandlerException()
        {
            // Act
            FileHandlerProcessing.GetFilesByExtension(string.Empty, ".txt", false);
        }

        /// <summary>
        ///     Gets the files by extension invalid path returns null.
        /// </summary>
        [TestMethod]
        public void GetFilesByExtensionInvalidPathReturnsNull()
        {
            // Act
            var result = FileHandlerProcessing.GetFilesByExtension(@"C:\invalidpath", ".txt", false);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        ///     Gets the files by extension valid path returns files.
        /// </summary>
        [TestMethod]
        public void GetFilesByExtensionValidPathReturnsFiles()
        {
            // Arrange
            var path = Path.Combine(Path.GetTempPath(), "testfolder");
            Directory.CreateDirectory(path);
            File.Create(Path.Combine(path, "file1.txt")).Dispose();
            File.Create(Path.Combine(path, "file2.txt")).Dispose();

            var appendix = ".txt";
            var subdirectories = false;

            try
            {
                // Act
                var result = FileHandlerProcessing.GetFilesByExtension(path, appendix, subdirectories);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Count);
                Assert.IsTrue(result.All(file => file.EndsWith(".txt")));
            }
            finally
            {
                // Cleanup
                Directory.Delete(path, true);
            }
        }
    }
}
