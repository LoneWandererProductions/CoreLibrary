/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibraryTests
 * FILE:        CommonLibraryTests/IoFileSearch.cs
 * PURPOSE:     Tests for IoFileHandler and mostly the search part
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.IO;
using FileHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     Basic File Operations
    /// </summary>
    [TestClass]
    public sealed class IoFileSearch
    {

        /// <summary>
        ///     The path (readonly). Value: Path.Combine(Directory.GetCurrentDirectory(), ResourcesGeneral.CampaignsFolder).
        /// </summary>
        private readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), nameof(IoFileHandler));

        /// <summary>
        ///     The _pathOperations (readonly). Value: "IO".
        /// </summary>
        private const string PathOperations = "IO";

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            Directory.CreateDirectory(_path);
            File.WriteAllText(Path.Combine(_path, "test1.txt"), "Test file 1");
            File.WriteAllText(Path.Combine(_path, "test2.log"), "Test file 2");
            File.WriteAllText(Path.Combine(_path, "test3.txt"), "Test file 3");
            Directory.CreateDirectory(Path.Combine(_path, "subdir"));
            File.WriteAllText(Path.Combine(_path, "subdir", "test4.txt"), "Test file 4");
        }

        /// <summary>
        ///     Simple Check for getting files Contains in a Folder
        /// </summary>
        [TestMethod]
        public void GetFilesByExtensionWithExtension()
        {
            var isDone = FileHandleDelete.DeleteCompleteFolder(_path);
            Assert.IsTrue(isDone, "Could not cleanup");

            var check = false;
            var file = Path.Combine(_path, Path.ChangeExtension(PathOperations, ResourcesGeneral.TstExt)!);
            HelperMethods.CreateFile(file);
            var list =
                FileHandleSearch.GetFileByExtensionWithExtension(_path, ResourcesGeneral.TstExt, false);

            if (list == null)
            {
                Assert.Fail("Null Reference");
            }

            if (list.Count == 1)
            {
                check = true;
            }

            Assert.IsTrue(check, "Did not delete Folder:");

            Assert.AreEqual(list[0], PathOperations + ResourcesGeneral.TstExt, "Correct File");

            check = FileHandleDelete.DeleteFile(file);

            Assert.IsTrue(check, "Did not delete File");
        }

        /// <summary>
        ///     Simple Check for getting files Contains in a Folder
        /// </summary>
        [TestMethod]
        public void GetFilesByExtensionWithoutExtension()
        {
            var isDone = FileHandleDelete.DeleteCompleteFolder(_path);
            Assert.IsTrue(isDone, "Could not cleanup");

            var file = Path.Combine(_path, Path.ChangeExtension(PathOperations, ResourcesGeneral.TstExt)!);

            //Create File
            HelperMethods.CreateFile(file);
            //some extras, now we have 7 files to search though
            HelperMethods.CreateFiles(_path, ResourcesGeneral.FileExtList);

            //basic search test
            var list =
                FileHandleSearch.GetFileByExtensionWithoutExtension(_path, ResourcesGeneral.TstExt, false);

            if (list == null)
            {
                Assert.Fail("Null Reference");
            }

            var check = list.Count == 1;

            Assert.IsTrue(check, "Correct Number of files");

            Assert.AreEqual(list[0], PathOperations, "Not the correct File");

            //search Test with contains string
            var ext = new List<string> { ResourcesGeneral.TstExt };

            list =
                FileHandleSearch.GetFilesWithSubString(_path, ext, false, "IO", false);

            if (list == null)
            {
                Assert.Fail("Null Reference");
            }

            check = list.Count == 1;

            Assert.IsTrue(check, "Correct Number of files");

            list =
                FileHandleSearch.GetFilesWithSubString(_path, ext, false, "IO", true);

            if (list == null)
            {
                Assert.Fail("Null Reference");
            }

            check = list.Count == 0;

            Assert.IsTrue(check, "Correct Number of files");

            list =
                FileHandleSearch.GetFilesWithSubString(_path, ResourcesGeneral.FileExtList, false, "1", true);

            if (list == null)
            {
                Assert.Fail("Null Reference");
            }

            check = list.Count == 5;

            if (list == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.IsTrue(check, "Correct Number of files");

            check = FileHandleDelete.DeleteFile(file);

            Assert.IsTrue(check, "Did not delete File");
        }

        /// <summary>
        ///     Simple Check for getting files Contains in a Folder
        /// </summary>
        [TestMethod]
        public void GetAllSubfolders()
        {
            var check = false;
            var list = FileHandleSearch.GetAllSubfolders(Directory.GetCurrentDirectory());

            if (list == null)
            {
                Assert.Fail("Null Reference");
            }

            if (list.Count > 0)
            {
                check = true;
            }

            Assert.IsTrue(check, "Did not get all Folders");

            var path = DirectoryInformation.GetParentDirectory(1);
            Assert.IsTrue(path.EndsWith("\\CoreLibrary\\CommonLibraryTests\\bin", StringComparison.Ordinal),
                $"Wrong Directory Name: {path}");

            path = DirectoryInformation.GetParentDirectory(2);

            Assert.IsTrue(path.EndsWith("\\CoreLibrary\\CommonLibraryTests", StringComparison.Ordinal),
                $"Wrong Directory Name: {path}");
        }

        /// <summary>
        ///     Gets the file information.
        /// </summary>
        [TestMethod]
        public void GetFileInformation()
        {
            var file = Path.Combine(_path, Path.ChangeExtension(PathOperations, ResourcesGeneral.TstExt)!);
            HelperMethods.CreateFile(file);

            var info = FileHandleSearch.GetFileDetails(file);

            Assert.IsNotNull(info, "no results");

            Assert.AreEqual(100, info.Size, "Correct size");
            Assert.AreEqual(".txt", info.Extension, "Correct Extension");
            Assert.AreEqual("IO.txt", info.FileName, "Correct FileName");
            //the rest is null because it is a fresh created file no need to check it.

            _ = FileHandleDelete.DeleteFile(file);
            var check = FileHandleSearch.FileExists(file);
            Assert.IsFalse(check, "File not deleted");
        }


        /// <summary>
        ///     Simple Check for getting files Contained in a Folder
        ///     Check Lock Status
        ///     Set Retries
        /// </summary>
        [TestMethod]
        public void GetFilesByExtensionFullPath()
        {
            var isDone = FileHandleDelete.DeleteCompleteFolder(_path);
            Assert.IsTrue(isDone, "Could not cleanup");

            //Set Amount of Repeats
            FileHandlerRegister.Tries = 2;

            var file = Path.Combine(_path, Path.ChangeExtension(PathOperations, ResourcesGeneral.TstExt)!);
            HelperMethods.CreateFile(file);

            var list =
                FileHandleSearch.GetFilesByExtensionFullPath(_path, ResourcesGeneral.TstExt, false);

            if (list == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.AreEqual(1, list.Count, "Got the Files Folder by single Extension");

            var ext = new List<string> { ResourcesGeneral.TstExt };
            list = FileHandleSearch.GetFilesByExtensionFullPath(_path, ext, false);

            if (list == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.AreEqual(1, list.Count, "Did not get the Files Folder by multiple Extension");

            Assert.AreEqual(list[0], Path.Combine(_path, PathOperations + ResourcesGeneral.TstExt),
                "Not the correct File");

            var lst = FileHandleSearch.GetAllFiles(_path, false);

            Assert.IsNotNull(lst, "Result set was empty");

            Assert.IsFalse(FileHandleDelete.IsFileLocked(file), "File was not Locked");

            Assert.IsTrue(FileHandleDelete.DeleteFile(file), "Deleted File");

            _ = FileHandleDelete.DeleteFile(file);
            var check = FileHandleSearch.FileExists(file);
            Assert.IsFalse(check, "File not deleted");
        }

        /// <summary>
        /// Files the exists should return true when file exists.
        /// </summary>
        [TestMethod]
        public void FileExistsShouldReturnTrueWhenFileExists()
        {
            var path = Path.Combine(_path, "test1.txt");
            var result = FileHandleSearch.FileExists(path);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Files the exists should return false when file does not exist.
        /// </summary>
        [TestMethod]
        public void FileExistsShouldReturnFalseWhenFileDoesNotExist()
        {
            var path = Path.Combine(_path, "nonexistent.txt");
            var result = FileHandleSearch.FileExists(path);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Gets the files by extension full path should return files with given extension.
        /// </summary>
        [TestMethod]
        public void GetFilesByExtensionFullPathShouldReturnFilesWithGivenExtension()
        {
            var result = FileHandleSearch.GetFilesByExtensionFullPath(_path, ".txt", false);
            Assert.AreEqual(2, result.Count);
        }

        /// <summary>
        /// Gets the files by extension full path should return files from subdirectories.
        /// </summary>
        [TestMethod]
        public void GetFilesByExtensionFullPathShouldReturnFilesFromSubdirectories()
        {
            var result = FileHandleSearch.GetFilesByExtensionFullPath(_path, ".txt", true);
            Assert.AreEqual(3, result.Count);
        }

        /// <summary>
        /// Gets the file details should return file details.
        /// </summary>
        [TestMethod]
        public void GetFileDetailsShouldReturnFileDetails()
        {
            var path = Path.Combine(_path, "test1.txt");
            var result = FileHandleSearch.GetFileDetails(path);
            Assert.IsNotNull(result);
            Assert.AreEqual("test1.txt", result.FileName);
        }

        /// <summary>
        /// Gets all subfolders should return subfolders.
        /// </summary>
        [TestMethod]
        public void GetAllSubfoldersShouldReturnSubfolders()
        {
            var result = FileHandleSearch.GetAllSubfolders(_path);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("subdir", result[0]);
        }

        /// <summary>
        /// Checks if folder contains element should return true if folder contains files.
        /// </summary>
        [TestMethod]
        public void CheckIfFolderContainsElementShouldReturnTrueIfFolderContainsFiles()
        {
            var result = FileHandleSearch.CheckIfFolderContainsElement(_path);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Checks if folder contains element should return false if folder is empty.
        /// </summary>
        [TestMethod]
        public void CheckIfFolderContainsElementShouldReturnFalseIfFolderIsEmpty()
        {
            var emptyDir = Path.Combine(_path, "emptyDir");
            Directory.CreateDirectory(emptyDir);
            var result = FileHandleSearch.CheckIfFolderContainsElement(emptyDir);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Gets the files with sub string should return files containing sub string.
        /// </summary>
        [TestMethod]
        public void GetFilesWithSubStringShouldReturnFilesContainingSubString()
        {
            var result = FileHandleSearch.GetFilesWithSubString(_path, new List<string> { ".txt" }, true, "3", false);
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].EndsWith("test3.txt"));
        }

        /// <summary>
        /// Gets the files with sub string should return files not containing sub string when invert is true.
        /// </summary>
        [TestMethod]
        public void GetFilesWithSubStringShouldReturnFilesNotContainingSubString_WhenInvertIsTrue()
        {
            var result = FileHandleSearch.GetFilesWithSubString(_path, new List<string> { ".txt" }, true, "3", true);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.TrueForAll(file => !file.EndsWith("test3.txt")));
        }


        /// <summary>
        /// Teardowns this instance.
        /// </summary>
        [TestCleanup]
        public void Teardown()
        {
            Directory.Delete(_path, true);
        }
    }
}
