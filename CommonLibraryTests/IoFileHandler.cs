/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     CommonLibraryTests
* FILE:        CommonLibraryTests/IoFileHandler.cs
* PURPOSE:     Tests for IoFileHandler
* PROGRAMER:   Peter Geinitz (Wayfarer)
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ExtendedSystemObjects;
using FileHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests
{
    /// <summary>
    ///     Basic File Operations
    /// </summary>
    [TestClass]
    public sealed class IoFileHandler
    {
        /// <summary>
        ///     The _path Rename (readonly). Value: "IO".
        /// </summary>
        private const string PathRename = "Rename";

        /// <summary>
        ///     The _pathOperations (readonly). Value: "IO".
        /// </summary>
        private const string PathOperations = "IO";

        /// <summary>
        ///     The _pathOperations Two (readonly). Value: "IO2".
        /// </summary>
        private const string PathOperationsTwo = "IO2";

        /// <summary>
        ///     The test source dir
        /// </summary>
        private const string TestSourceDir = "TestSource";

        /// <summary>
        ///     The test target dir
        /// </summary>
        private const string TestTargetDir = "TestTarget";

        /// <summary>
        ///     The path (readonly). Value: Path.Combine(Directory.GetCurrentDirectory(), ResourcesGeneral.CampaignsFolder).
        /// </summary>
        private readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), nameof(IoFileHandler));

        /// <summary>
        ///     The core (readonly). Value: Path.Combine(Directory.GetCurrentDirectory(),  "test").
        /// </summary>
        private readonly string _renamePath = Path.Combine(Directory.GetCurrentDirectory(), PathRename);

        /// <summary>
        ///     Setups this instance.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            Cleanup(); // Ensure a clean state before each test

            Directory.CreateDirectory(TestSourceDir);
            Directory.CreateDirectory(TestTargetDir);
        }

        /// <summary>
        ///     Basic test for the File Function
        /// </summary>
        [TestMethod]
        public void GetPathWithoutExtension()
        {
            Assert.AreEqual(@"C:\Users\Admin\Documents\example",
                PathInformation.GetPathWithoutExtension(@"C:\Users\Admin\Documents\example.exe"),
                "Test passed path and file without extension");
        }

        /// <summary>
        ///     Gets the file information.
        /// </summary>
        [TestMethod]
        public void GetNewFileName()
        {
            FileHandleDelete.DeleteAllContents(_path, true);
            var fileOne = Path.Combine(_path, nameof(GetNewFileName),
                Path.ChangeExtension(PathOperations, ResourcesGeneral.TstExt)!);
            HelperMethods.CreateFile(fileOne);

            var info = FileUtility.GetNewFileName(fileOne);
            HelperMethods.CreateFile(info);

            if (info == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.IsTrue(
                info.EndsWith(
                    "\\CoreLibrary\\CommonLibraryTests\\bin\\Debug\\net5.0-windows\\IoFileHandler\\GetNewFileName\\IO(0).txt",
                    StringComparison.Ordinal), "Expected File Name");

            info = FileUtility.GetNewFileName(fileOne);

            if (info == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.IsTrue(
                info.EndsWith(
                    "\\CoreLibrary\\CommonLibraryTests\\bin\\Debug\\net5.0-windows\\IoFileHandler\\GetNewFileName\\IO(1).txt",
                    StringComparison.Ordinal), "Expected File Name");

            _ = FileHandleDelete.DeleteFile(fileOne);
            var check = FileHandleSearch.FileExists(fileOne);
            Assert.IsFalse(check, "File not deleted");

            _ = FileHandleDelete.DeleteFile(info);
            check = FileHandleSearch.FileExists(info);
            Assert.IsFalse(check, "File not deleted");
        }

        /// <summary>
        ///     Simple Check if Folders are in the right place and created correctly
        /// </summary>
        [TestMethod]
        public void GenerateFolder()
        {
            var check = FileHandleCreate.CreateFolder(Path.Combine(_path, nameof(GenerateFolder)),
                PathOperations);

            Assert.IsTrue(check,
                "Path not Created: ");

            var expectedPath = Path.Combine(_path,
                nameof(GenerateFolder),
                PathOperations);

            Assert.IsTrue(Directory.Exists(expectedPath),
                "Path not equal: ");

            check = FileHandleDelete.DeleteFolder(expectedPath);

            Assert.IsTrue(check, "Folder no deleted: " + expectedPath);
        }

        /// <summary>
        ///     Simple Check if Folders are in the right place and created correctly
        /// </summary>
        [TestMethod]
        public void DeleteFolderContentsByExtension()
        {
            //create dummy files
            HelperMethods.CreateFiles(_path, ResourcesGeneral.FileExtList);

            FileHandlerRegister.ClearLog();
            var isDone = FileHandleDelete.DeleteFolderContentsByExtension(_path, ResourcesGeneral.FileExtList, false);

            var error = string.Empty;
            if (!FileHandlerRegister.ErrorLog.IsNullOrEmpty())
            {
                error = FileHandlerRegister.ErrorLog.Last();
            }

            Assert.IsTrue(isDone, $"Folder cleaned: {error}");
        }

        /// <summary>
        ///     Complete package still some bugs, we don't seem to get exclusive access to the files
        /// </summary>
        [TestMethod]
        public void CompleteFileHandler()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Copy");
            var isDone = FileHandleDelete.DeleteCompleteFolder(path);
            Assert.IsTrue(isDone, "Could not cleanup");

            FileHandleCreate.CreateFolder(path);
            Assert.IsTrue(Directory.Exists(path), "Folder not cleaned");

            var file = Path.Combine(path, PathOperations + ResourcesGeneral.TstExt);
            Trace.WriteLine(file);
            HelperMethods.CreateFile(file);

            isDone = FileHandleSearch.CheckIfFolderContainsElement(path);
            Assert.IsTrue(isDone, "File does not exist");

            //search for Files
            var subPathOne = Path.Combine(path, @"subOne\");
            var subPathTwo = Path.Combine(path, @"subTwo\");

            FileHandleCreate.CreateFolder(subPathOne);
            FileHandleCreate.CreateFolder(subPathTwo);
            var lst = FileHandleSearch.GetAllSubfolders(path);

            if (lst == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.AreEqual(2, lst.Count, "Not the right amount of Sub folders");

            //delete File
            FileHandleDelete.DeleteFile(file);
            Assert.IsFalse(FileHandleSearch.CheckIfFolderContainsElement(path), "File was not deleted");

            //create File
            file = Path.Combine(subPathOne, PathOperations + ResourcesGeneral.TstExt);
            Trace.WriteLine(file);
            HelperMethods.CreateFile(file);

            //copy Files
            FileHandleCopy.CopyFiles(subPathOne, subPathTwo, true);
            Trace.WriteLine(subPathTwo);

            Assert.IsTrue(FileHandleSearch.CheckIfFolderContainsElement(subPathTwo), "File was not moved");

            Assert.IsTrue(FileHandleDelete.DeleteAllContents(path, true), "Full delete seems to not work");

            Assert.IsFalse(FileHandleSearch.CheckIfFolderContainsElement(subPathTwo), "Files were not deleted");

            Assert.IsFalse(FileHandleSearch.CheckIfFolderContainsElement(subPathOne), "Files were not deleted");
        }

        /// <summary>
        ///     Test the rename feature
        /// </summary>
        [TestMethod]
        public void RenameFile()
        {
            var isDone = FileHandleDelete.DeleteCompleteFolder(_renamePath);
            Assert.IsTrue(isDone, "Could not cleanup");

            FileHandleCreate.CreateFolder(_renamePath);
            Assert.IsTrue(Directory.Exists(_renamePath), "Folder not cleaned");

            var file = Path.Combine(_renamePath, PathOperations, ResourcesGeneral.TstExt);
            var target = Path.Combine(_renamePath, "new.txt");
            Trace.WriteLine(file);
            HelperMethods.CreateFile(file);

            //rename a single file

            isDone = FileHandleRename.RenameFile(file, target);
            Assert.IsTrue(isDone, "Wrong return value for File Renaming");
            Assert.IsTrue(File.Exists(target), "File not renamed");

            //check directory move
            target = Path.Combine(Directory.GetCurrentDirectory(), "test_new");
            isDone = FileHandleRename.RenameDirectory(_renamePath, target);
            Assert.IsTrue(isDone, "Wrong return value");
            Assert.IsTrue(Directory.Exists(target), "Folder not cleaned");
            isDone = FileHandleDelete.DeleteCompleteFolder(target);
            Assert.IsTrue(isDone, "Could not cleanup");
        }

        /// <summary>
        ///     Test the Rename feature with appendages
        /// </summary>
        [TestMethod]
        public void AppendageOfFile()
        {
            var isDone = FileHandleDelete.DeleteCompleteFolder(_renamePath);
            Assert.IsTrue(isDone, "Could not cleanup");

            FileHandleCreate.CreateFolder(_renamePath);
            Assert.IsTrue(Directory.Exists(_renamePath), "Folder not cleaned");

            if ("test".StartsWith("_", StringComparison.OrdinalIgnoreCase))
            {
                Assert.Fail();
            }

            //generate a bunch of files
            var fileOne = Path.Combine(_renamePath, "fist");
            HelperMethods.CreateFile(fileOne);
            var fileSecond = Path.Combine(_renamePath, "_second__");
            HelperMethods.CreateFile(fileSecond);
            var fileThird = Path.Combine(_renamePath, "-third");
            HelperMethods.CreateFile(fileThird);
            var fileFourth = Path.Combine(_renamePath, "_fourth");
            HelperMethods.CreateFile(fileFourth);
            var fileFifth = Path.Combine(_renamePath, "_234234234_fifth");
            HelperMethods.CreateFile(fileFifth);

            //Rename, remove "_"
            var count = FileNameConverter.RemoveAppendage("_", _renamePath, false);
            Assert.AreEqual(3, count, "Not enough files renamed");

            var check = FileHandleSearch.FileExists(Path.Combine(_renamePath, "second__"));
            Assert.IsTrue(check, "File was not correct");

            var file = "_second__".RemoveAppendage("_");
            Assert.AreEqual("second__", file, "File not correctly renamed");

            //Rename, add "_"
            count = FileNameConverter.AddAppendage("_", _renamePath, false);
            Assert.AreEqual(5, count, "Not enough files renamed");

            check = FileHandleSearch.FileExists(Path.Combine(_renamePath, "_second__"));
            Assert.IsTrue(check, "File was not correct");

            file = "_second__".AddAppendage("_");
            //if already added don't do anything
            Assert.AreEqual("_second__", file, "File not correctly renamed");

            //Rename, remove string "_"
            count = FileNameConverter.ReplacePart("_", string.Empty, _renamePath, false);
            Assert.AreEqual(5, count, "Not enough files renamed");

            check = FileHandleSearch.FileExists(Path.Combine(_renamePath, "second"));
            Assert.IsTrue(check, "File was not correct");

            file = "_second__".ReplacePart("_", string.Empty);
            Assert.AreEqual("second", file, "File not correctly renamed");

            //Rename, reorder Numbers
            count = FileNameConverter.ReOrderNumbers(_renamePath, false);
            Assert.AreEqual(5, count, "Not enough files renamed");

            check = FileHandleSearch.FileExists(Path.Combine(_renamePath, "fifth_234234234"));
            Assert.IsTrue(check, "File was not correct");

            file = "_234234234_fifth".ReOrderNumbers();
            Assert.AreEqual("__fifth_234234234", file, "File not correctly renamed");
        }

        /// <summary>
        ///     Copy the if newer.
        /// </summary>
        [TestMethod]
        public void CopyList()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "CopyList");
            var isDone = FileHandleDelete.DeleteCompleteFolder(path);
            Assert.IsTrue(isDone, "Could not cleanup");

            var subPathOne = Path.Combine(path, @"subOne\");
            var subPathTwo = Path.Combine(path, @"subTwo\");
            var subPathTwoExtended = Path.Combine(path, @"subTwo\test\");
            FileHandleCreate.CreateFolder(subPathOne);
            FileHandleCreate.CreateFolder(subPathTwo);
            FileHandleCreate.CreateFolder(subPathTwoExtended);
            var lst = FileHandleSearch.GetAllSubfolders(path);

            if (lst == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.AreEqual(2, lst.Count, "Not the right amount of Sub folders");

            //create File
            var file = Path.Combine(subPathTwo, PathOperations + ResourcesGeneral.TstExt);
            Trace.WriteLine(file);
            HelperMethods.CreateFile(file);

            file = Path.Combine(subPathTwoExtended, PathOperations + ResourcesGeneral.TstExt);
            HelperMethods.CreateFile(file);
            lst = FileHandleSearch.GetFilesByExtensionFullPath(subPathTwo, ".*", true);

            isDone = FileHandleCopy.CopyFiles(lst, subPathOne, true);

            Assert.IsTrue(isDone, "File were not moved");
            lst = FileHandleSearch.GetFilesByExtensionFullPath(subPathOne, ".*", true);
            if (lst == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.AreEqual(2, lst.Count, "Not enough Files were moved");
            lst = FileHandleSearch.GetFilesByExtensionFullPath(subPathTwo, ".*", true);
            if (lst == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.AreEqual(2, lst.Count, "Files are still there");
            var result = FileHandleCopy.CopyFiles(subPathOne, subPathTwo);
            if (result == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.AreEqual(2, result.Count, "Enough files were accounted for");
        }

        /// <summary>
        ///     Copies the files valid source and target copies files.
        /// </summary>
        [TestMethod]
        public void CopyFilesValidSourceAndTargetCopiesFiles()
        {
            var sourceFilePath = Path.Combine(TestSourceDir, "test.txt");
            File.WriteAllText(sourceFilePath, "Test Content");

            var result = FileHandleCopy.CopyFiles(TestSourceDir, TestTargetDir, true);

            Assert.IsTrue(result);
            var targetFilePath = Path.Combine(TestTargetDir, "test.txt");
            Assert.IsTrue(File.Exists(targetFilePath));
            Assert.AreEqual("Test Content", File.ReadAllText(targetFilePath));
        }

        /// <summary>
        ///     Copies the files source directory not found returns false.
        /// </summary>
        [TestMethod]
        public void CopyFilesSourceDirectoryNotFoundReturnsFalse()
        {
            var result = FileHandleCopy.CopyFiles("NonExistentSource", TestTargetDir, true);

            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Copies the files source and target equal throws file handler exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileHandlerException))]
        public void CopyFilesSourceAndTargetEqualThrowsFileHandlerException()
        {
            FileHandleCopy.CopyFiles(TestSourceDir, TestSourceDir, true);
        }

        /// <summary>
        ///     Check if our Exceptions do actual work
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileHandlerException), "Invalid Input. String was Empty")]
        public void CopyException()
        {
            FileHandleCopy.CopyFiles(string.Empty, null);
        }

        /// <summary>
        ///     Copy the if newer.
        /// </summary>
        [TestMethod]
        public void CopyIfNewer()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "CopyNewer");
            var isDone = FileHandleDelete.DeleteCompleteFolder(path);
            Assert.IsTrue(isDone, "Could not cleanup");

            var subPathOne = Path.Combine(path, @"subOne\");
            var subPathTwo = Path.Combine(path, @"subTwo\");
            FileHandleCreate.CreateFolder(subPathOne);
            FileHandleCreate.CreateFolder(subPathTwo);
            var lst = FileHandleSearch.GetAllSubfolders(path);

            if (lst == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.AreEqual(2, lst.Count, "Not the right amount of Sub folders");

            //create File
            var file = Path.Combine(subPathOne, PathOperations + ResourcesGeneral.TstExt);
            Trace.WriteLine(file);
            HelperMethods.CreateFile(file);

            file = Path.Combine(subPathTwo, PathOperations + ResourcesGeneral.TstExt);
            Trace.WriteLine(file);
            HelperMethods.CreateFile(file);

            var firstTime = File.GetLastWriteTime(file);

            isDone = FileHandleCopy.CopyFilesReplaceIfNewer(subPathOne, subPathTwo);

            var secondTime = File.GetLastWriteTime(file);

            Assert.IsTrue(isDone, "File were not moved");
            Assert.AreEqual(firstTime, secondTime, "File was not the same");
        }

        /// <summary>
        ///     Copies the files with file list copies files successfully.
        /// </summary>
        [TestMethod]
        public void CopyFilesWithFileListCopiesFilesSuccessfully()
        {
            var sourceFilePath = Path.Combine(TestSourceDir, "test.txt");
            File.WriteAllText(sourceFilePath, "Test Content");
            var fileList = new List<string> { sourceFilePath };

            var result = FileHandleCopy.CopyFiles(fileList, TestTargetDir, true);

            Assert.IsTrue(result);
            var targetFilePath = Path.Combine(TestTargetDir, "test.txt");
            Assert.IsTrue(File.Exists(targetFilePath));
            Assert.AreEqual("Test Content", File.ReadAllText(targetFilePath));
        }

        /// <summary>
        ///     Copies the files replace if newer copies only newer files.
        /// </summary>
        [TestMethod]
        public void CopyFilesReplaceIfNewerCopiesOnlyNewerFiles()
        {
            var sourceFilePath = Path.Combine(TestSourceDir, "test.txt");
            File.WriteAllText(sourceFilePath, "New Content");
            var targetFilePath = Path.Combine(TestTargetDir, "test.txt");
            File.WriteAllText(targetFilePath, "Old Content");
            File.SetLastWriteTime(targetFilePath, DateTime.Now.AddDays(-1));

            var result = FileHandleCopy.CopyFilesReplaceIfNewer(TestSourceDir, TestTargetDir);

            Assert.IsTrue(result);
            Assert.AreEqual("New Content", File.ReadAllText(targetFilePath));
        }

        /// <summary>
        ///     Copies the files with differing source and target returns files not copied.
        /// </summary>
        [TestMethod]
        public void CopyFilesWithDifferingSourceAndTargetReturnsFilesNotCopied()
        {
            var subPathOne = Path.Combine(TestSourceDir, "subOne");
            var subPathTwo = Path.Combine(TestTargetDir, "subTwo");
            Directory.CreateDirectory(subPathOne);
            Directory.CreateDirectory(subPathTwo);

            var file1 = Path.Combine(subPathOne, PathOperations + ".txt");
            var file2 = Path.Combine(subPathTwo, PathOperationsTwo + ".txt");
            File.WriteAllText(file1, "Content One");
            File.WriteAllText(file2, "Content Two");

            var fileList = new List<string> { file1, file2 };
            var notCopied = FileHandleCopy.CopyFiles(fileList, subPathOne, false);

            Assert.IsFalse(notCopied);
        }

        /// <summary>
        ///     Copies the files copy list copies files correctly.
        /// </summary>
        [TestMethod]
        public void CopyFilesCopyListCopiesFilesCorrectly()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "CopyList");
            var subPathOne = Path.Combine(path, "subOne");
            var subPathTwo = Path.Combine(path, "subTwo");
            var subPathTwoExtended = Path.Combine(path, "subTwo", "test");

            Directory.CreateDirectory(subPathOne);
            Directory.CreateDirectory(subPathTwo);
            Directory.CreateDirectory(subPathTwoExtended);

            var file1 = Path.Combine(subPathTwo, PathOperations + ".txt");
            var file2 = Path.Combine(subPathTwoExtended, PathOperations + ".txt");
            File.WriteAllText(file1, "Content One");
            File.WriteAllText(file2, "Content Two");

            var filesToCopy = new List<string> { file1, file2 };
            var result = FileHandleCopy.CopyFiles(filesToCopy, subPathOne, true);

            Assert.IsTrue(result);

            var copiedFiles = Directory.GetFiles(subPathOne, "*.*", SearchOption.AllDirectories);
            Assert.AreEqual(2, copiedFiles.Length);
        }

        /// <summary>
        ///     Copy the if newer.
        /// </summary>
        [TestMethod]
        public void CutList()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "CutList");
            var isDone = FileHandleDelete.DeleteCompleteFolder(path);
            Assert.IsTrue(isDone, "Could not cleanup");

            var subPathOne = Path.Combine(path, @"subOne\");
            var subPathTwo = Path.Combine(path, @"subTwo\");
            var subPathTwoExtended = Path.Combine(path, @"subTwo\test\");
            FileHandleCreate.CreateFolder(subPathOne);
            FileHandleCreate.CreateFolder(subPathTwo);
            FileHandleCreate.CreateFolder(subPathTwoExtended);
            var lst = FileHandleSearch.GetAllSubfolders(path);
            if (lst == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.AreEqual(2, lst.Count, "Not the right amount of Sub folders");

            //create File
            var file = Path.Combine(subPathTwo, PathOperations + ResourcesGeneral.TstExt);
            Trace.WriteLine(file);
            HelperMethods.CreateFile(file);

            file = Path.Combine(subPathTwoExtended, PathOperations + ResourcesGeneral.TstExt);
            HelperMethods.CreateFile(file);
            lst = FileHandleSearch.GetFilesByExtensionFullPath(subPathTwo, ".*", true);

            isDone = FileHandleCut.CutFiles(lst, subPathOne, true);

            Assert.IsTrue(isDone, "File were not moved");
            lst = FileHandleSearch.GetFilesByExtensionFullPath(subPathOne, ".*", true);
            if (lst == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.AreEqual(2, lst.Count, "Not enough Files were moved");
            lst = FileHandleSearch.GetFilesByExtensionFullPath(subPathTwo, ".*", true);
            if (lst == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.AreEqual(0, lst.Count, "Files were not deleted");

            var result = FileHandleCopy.CopyFiles(subPathOne, subPathTwo);
            if (result == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.AreEqual(2, result.Count, "Enough files were accounted for");
        }

        /// <summary>
        ///     Cuts the files valid source and target cuts files.
        /// </summary>
        [TestMethod]
        public void CutFilesValidSourceAndTargetCutsFiles()
        {
            // Arrange
            var sourceFilePath = Path.Combine(TestSourceDir, "test.txt");
            File.WriteAllText(sourceFilePath, "Test Content");

            // Act
            var result = FileHandleCut.CutFiles(TestSourceDir, TestTargetDir, true);

            // Assert
            Assert.IsTrue(result);
            var targetFilePath = Path.Combine(TestTargetDir, "test.txt");
            Assert.IsTrue(File.Exists(targetFilePath));
            Assert.AreEqual("Test Content", File.ReadAllText(targetFilePath));
            Assert.IsFalse(File.Exists(sourceFilePath));
        }

        /// <summary>
        ///     Cuts the files source and target equal throws file handler exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileHandlerException))]
        public void CutFilesSourceAndTargetEqualThrowsFileHandlerException()
        {
            FileHandleCut.CutFiles(TestSourceDir, TestSourceDir, true);
        }

        /// <summary>
        ///     Cuts the files non existent source returns false.
        /// </summary>
        [TestMethod]
        public void CutFilesNonExistentSourceReturnsFalse()
        {
            var result = FileHandleCut.CutFiles("NonExistentSource", TestTargetDir, true);
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Cuts the files with file list cuts files successfully.
        /// </summary>
        [TestMethod]
        public void CutFilesWithFileListCutsFilesSuccessfully()
        {
            // Arrange
            var sourceFilePath = Path.Combine(TestSourceDir, "test.txt");
            File.WriteAllText(sourceFilePath, "Test Content");
            var fileList = new List<string> { sourceFilePath };

            // Act
            var result = FileHandleCut.CutFiles(fileList, TestTargetDir, true);

            // Assert
            Assert.IsTrue(result);
            var targetFilePath = Path.Combine(TestTargetDir, "test.txt");
            Assert.IsTrue(File.Exists(targetFilePath));
            Assert.AreEqual("Test Content", File.ReadAllText(targetFilePath));
            Assert.IsFalse(File.Exists(sourceFilePath));
        }

        /// <summary>
        ///     Cuts the files file list null or empty throws file handler exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileHandlerException))]
        public void CutFilesFileListNullOrEmptyThrowsFileHandlerException()
        {
            FileHandleCut.CutFiles((List<string>)null, TestTargetDir, true);
        }

        /// <summary>
        ///     Cuts the files file list non existent file returns false.
        /// </summary>
        [TestMethod]
        public void CutFilesFileListNonExistentFileReturnsFalse()
        {
            var fileList = new List<string> { "NonExistentFile.txt" };
            var result = FileHandleCut.CutFiles(fileList, TestTargetDir, true);
            Assert.IsFalse(result);
        }

        /// <summary>
        ///     Cuts the files with overwrite cuts files successfully.
        /// </summary>
        [TestMethod]
        public void CutFilesWithOverwriteCutsFilesSuccessfully()
        {
            // Arrange
            var sourceFilePath = Path.Combine(TestSourceDir, "test.txt");
            var targetFilePath = Path.Combine(TestTargetDir, "test.txt");
            File.WriteAllText(sourceFilePath, "New Content");
            File.WriteAllText(targetFilePath, "Old Content");

            // Act
            var result = FileHandleCut.CutFiles(TestSourceDir, TestTargetDir, true);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(targetFilePath));
            Assert.AreEqual("New Content", File.ReadAllText(targetFilePath));
            Assert.IsFalse(File.Exists(sourceFilePath));
        }

        /// <summary>
        ///     Cuts the files without overwrite existing files not overwritten.
        /// </summary>
        [TestMethod]
        public void CutFilesWithoutOverwriteExistingFilesNotOverwritten()
        {
            // Arrange
            var sourceFilePath = Path.Combine(TestSourceDir, "test.txt");
            var targetFilePath = Path.Combine(TestTargetDir, "test.txt");
            File.WriteAllText(sourceFilePath, "New Content");
            File.WriteAllText(targetFilePath, "Old Content");

            // Act
            var result = FileHandleCut.CutFiles(TestSourceDir, TestTargetDir, false);

            // Assert, show false, since we have not moved all files
            Assert.IsFalse(result);
            Assert.IsTrue(File.Exists(targetFilePath));
            Assert.AreEqual("Old Content", File.ReadAllText(targetFilePath));
            Assert.IsTrue(File.Exists(sourceFilePath)); // Source file should not be moved
        }

        /// <summary>
        ///     Cuts the files handles subdirectories.
        /// </summary>
        [TestMethod]
        public void CutFilesHandlesSubdirectories()
        {
            // Arrange
            var subDir = Path.Combine(TestSourceDir, "SubDir");
            Directory.CreateDirectory(subDir);
            var sourceFilePath = Path.Combine(subDir, "test.txt");
            File.WriteAllText(sourceFilePath, "Test Content");

            // Act
            var result = FileHandleCut.CutFiles(TestSourceDir, TestTargetDir, true);

            // Assert
            Assert.IsTrue(result);
            var targetFilePath = Path.Combine(TestTargetDir, "SubDir", "test.txt");
            Assert.IsTrue(File.Exists(targetFilePath));
            Assert.AreEqual("Test Content", File.ReadAllText(targetFilePath));
            Assert.IsFalse(File.Exists(sourceFilePath));
        }

        /// <summary>
        ///     Compressions this instance.
        /// </summary>
        [TestMethod]
        public void Compression()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Compress");
            FileHandleDelete.DeleteCompleteFolder(path);

            //list of the files
            var lst = new List<string>();

            //generate to Files
            var file = Path.Combine(path, PathOperations + ResourcesGeneral.TstExt);
            lst.Add(file);
            HelperMethods.CreateFile(file);
            file = Path.Combine(path, PathOperationsTwo + ResourcesGeneral.TstExt);
            lst.Add(file);
            HelperMethods.CreateFile(file);

            file = Path.Combine(path, "compressed.zip");
            var check = FileHandleCompress.SaveZip(file, lst, true);
            Assert.IsTrue(check, "Could not compress");

            var files = FileHandleSearch.GetAllFiles(path, false);

            if (files == null)
            {
                Assert.Fail("Null Reference");
            }

            Assert.AreEqual(1, files.Count, "Compressed File created and or files were not deleted");

            check = FileHandleCompress.OpenZip(file, path, true);
            Assert.IsTrue(check, "Could not decompress");
        }

        /// <summary>
        ///     Sorts the files.
        ///     Other examples:
        ///     https://www.codeproject.com/Articles/22517/Natural-Sort-Comparer
        /// </summary>
        [TestMethod]
        public void SortFiles()
        {
            var lst = new List<string>
            {
                @"C:\1aaaaaaa.txt",
                @"C:\1.txt",
                @"C:\1000001.txt",
                @"C:\10.txt",
                @"C:\a_1.txt"
            };
            lst = lst.PathSort();

            Assert.AreEqual(@"C:\1.txt", lst[0], "Order was correct");
            Assert.AreEqual(@"C:\1aaaaaaa.txt", lst[1], "Order was correct");
            Assert.AreEqual(@"C:\10.txt", lst[2], "Order was correct");
            Assert.AreEqual(@"C:\1000001.txt", lst[3], "Order was correct");
            Assert.AreEqual(@"C:\a_1.txt", lst[4], "Order was correct");

            lst = new List<string>
            {
                "abc92",
                "2",
                "z24",
                "z2",
                "z15",
                "z1",
                "b1",
                "b6",
                "z 21",
                "z22",
                "1",
                "5",
                "3",
                "b2",
                "abc1",
                "abc9",
                "abc9z4",
                "z3",
                "b3",
                "z20",
                "a5",
                "z11",
                "b5",
                "b4"
            };

            lst = lst.PathSort();

            Assert.AreEqual("1", lst[0], "Order was correct");
            Assert.AreEqual("2", lst[1], "Order was correct");
            Assert.AreEqual("3", lst[2], "Order was correct");
            Assert.AreEqual("5", lst[3], "Order was correct");
            Assert.AreEqual("a5", lst[4], "Order was correct");
            Assert.AreEqual("abc1", lst[5], "Order was correct");
            Assert.AreEqual("abc9", lst[6], "Order was correct");
            Assert.AreEqual("abc9z4", lst[7], "Order was correct");
            Assert.AreEqual("abc92", lst[8], "Order was correct");
            Assert.AreEqual("b1", lst[9], "Order was correct");
            Assert.AreEqual("b2", lst[10], "Order was correct");
            Assert.AreEqual("b3", lst[11], "Order was correct");
            Assert.AreEqual("b4", lst[12], "Order was correct");
            Assert.AreEqual("b5", lst[13], "Order was correct");
            Assert.AreEqual("b6", lst[14], "Order was correct");
            Assert.AreEqual("z1", lst[15], "Order was correct");
            Assert.AreEqual("z2", lst[16], "Order was correct");
            Assert.AreEqual("z3", lst[17], "Order was correct");
            Assert.AreEqual("z11", lst[18], "Order was correct");
            Assert.AreEqual("z15", lst[19], "Order was correct");
            Assert.AreEqual("z20", lst[20], "Order was correct");
            Assert.AreEqual("z 21", lst[21], "Order was correct");
            Assert.AreEqual("z22", lst[22], "Order was correct");
            Assert.AreEqual("z24", lst[23], "Order was correct");
        }

        /// <summary>
        ///     Check if our Exceptions do actual work
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileHandlerException), "Invalid Input. String was Empty")]
        public void FileHandlerException()
        {
            FileHandleCreate.CreateFolder(null);
        }

        /// <summary>
        ///     Check if our Exceptions do actual work
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileHandlerException), "Invalid Input. String was Empty")]
        public void FileHandlerExceptionEmpty()
        {
            FileHandleCreate.CreateFolder(string.Empty);
        }

        /// <summary>
        ///     Cleanups this instance.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(TestSourceDir))
            {
                Directory.Delete(TestSourceDir, true);
            }

            if (Directory.Exists(TestTargetDir))
            {
                Directory.Delete(TestTargetDir, true);
            }
        }
    }
}
