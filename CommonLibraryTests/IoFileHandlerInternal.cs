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
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CleanUpExtensionList_NullInput_ThrowsArgumentNullException()
        {
            // Act
            FileHandlerProcessing.CleanUpExtensionList(null);
        }

        [TestMethod]
        public void CleanUpExtensionList_ValidInput_RemovesDots()
        {
            // Arrange
            var input = new List<string> { ".txt", ".csv", ".docx" };
            var expected = new List<string> { "txt", "csv", "docx" };

            // Act
            var result = FileHandlerProcessing.CleanUpExtensionList(input);

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSubFolder_InvalidElementPath_ThrowsArgumentException()
        {
            // Arrange
            string element = null;
            var root = @"C:\root\folder";
            var target = @"D:\target";

            // Act
            FileHandlerProcessing.GetSubFolder(element, root, target);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetSubFolder_InvalidRootPath_ThrowsArgumentException()
        {
            // Arrange
            var element = @"C:\root\folder\subfolder\file.txt";
            string root = null;
            var target = @"D:\target";

            // Act
            FileHandlerProcessing.GetSubFolder(element, root, target);
        }

        [TestMethod]
        public void GetSubFolder_ValidPaths_ReturnsRelativePath()
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

        [TestMethod]
        [ExpectedException(typeof(FileHandlerException))]
        public void GetFilesByExtension_EmptyPath_ThrowsFileHandlerException()
        {
            // Act
            FileHandlerProcessing.GetFilesByExtension(string.Empty, ".txt", false);
        }

        [TestMethod]
        public void GetFilesByExtension_InvalidPath_ReturnsNull()
        {
            // Act
            var result = FileHandlerProcessing.GetFilesByExtension(@"C:\invalidpath", ".txt", false);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetFilesByExtension_ValidPath_ReturnsFiles()
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
