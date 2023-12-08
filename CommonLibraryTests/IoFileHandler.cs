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
        ///     The path (readonly). Value: Path.Combine(Directory.GetCurrentDirectory(), ResourcesGeneral.CampaignsFolder).
        /// </summary>
        private readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), nameof(IoFileHandler));

        /// <summary>
        ///     The _pathOperations (readonly). Value: "IO".
        /// </summary>
        private readonly string _pathOperations = "IO";

        /// <summary>
        ///     The _pathOperations Two (readonly). Value: "IO2".
        /// </summary>
        private readonly string _pathOperationsTwo = "IO2";

        /// <summary>
        ///     The core (readonly). Value: Path.Combine(Directory.GetCurrentDirectory(),  "test").
        /// </summary>
        private readonly string _renamePath = Path.Combine(Directory.GetCurrentDirectory(), PathRename);

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

            var file = Path.Combine(_path, Path.ChangeExtension(_pathOperations, ResourcesGeneral.TstExt)!);
            HelperMethods.CreateFile(file);

            var list =
                FileHandleSearch.GetFilesByExtensionFullPath(_path, ResourcesGeneral.TstExt, false);

            Assert.AreEqual(1, list.Count, "Got the Files Folder by single Extension");

            var ext = new List<string> { ResourcesGeneral.TstExt };
            list = FileHandleSearch.GetFilesByExtensionFullPath(_path, ext, false);

            Assert.AreEqual(1, list.Count, "Did not get the Files Folder by multiple Extension");

            Assert.AreEqual(list[0], Path.Combine(_path, _pathOperations + ResourcesGeneral.TstExt),
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
        ///     Simple Check for getting files Contains in a Folder
        /// </summary>
        [TestMethod]
        public void GetFilesByExtensionWithoutExtension()
        {
            var isDone = FileHandleDelete.DeleteCompleteFolder(_path);
            Assert.IsTrue(isDone, "Could not cleanup");

            var file = Path.Combine(_path, Path.ChangeExtension(_pathOperations, ResourcesGeneral.TstExt)!);

            //Create File
            HelperMethods.CreateFile(file);
            //some extras, now we have 7 files to search though
            HelperMethods.CreateFiles(_path, ResourcesGeneral.FileExtList);

            //basic search test
            var list =
                FileHandleSearch.GetFileByExtensionWithoutExtension(_path, ResourcesGeneral.TstExt, false);

            var check = list.Count == 1;

            Assert.IsTrue(check, "Correct Number of files");

            Assert.AreEqual(list[0], _pathOperations, "Not the correct File");

            //search Test with contains string
            var ext = new List<string> { ResourcesGeneral.TstExt };

            list =
                FileHandleSearch.GetFilesWithSubString(_path, ext, false, "IO", false);

            check = list.Count == 1;

            Assert.IsTrue(check, "Correct Number of files");

            list =
                FileHandleSearch.GetFilesWithSubString(_path, ext, false, "IO", true);

            check = list.Count == 0;

            Assert.IsTrue(check, "Correct Number of files");

            list =
                FileHandleSearch.GetFilesWithSubString(_path, ResourcesGeneral.FileExtList, false, "1", true);

            check = list.Count == 5;

            Assert.IsTrue(check, "Correct Number of files");

            check = FileHandleDelete.DeleteFile(file);

            Assert.IsTrue(check, "Did not delete File");
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
            var file = Path.Combine(_path, Path.ChangeExtension(_pathOperations, ResourcesGeneral.TstExt)!);
            HelperMethods.CreateFile(file);

            var list =
                FileHandleSearch.GetFileByExtensionWithExtension(_path, ResourcesGeneral.TstExt, false);
            if (list.Count == 1)
            {
                check = true;
            }

            Assert.IsTrue(check, "Did not delete Folder:");

            Assert.AreEqual(list[0], _pathOperations + ResourcesGeneral.TstExt, "Correct File");

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

            if (list.Count > 0)
            {
                check = true;
            }

            Assert.IsTrue(check, "Did not get all Folders");

            var path = DirectoryInformation.GetParentDirectory(1);

            Assert.IsTrue(path.EndsWith("\\Projects\\CoreLibrary\\CommonLibraryTests\\bin", StringComparison.Ordinal), "Wrong Directory Name");

            path = DirectoryInformation.GetParentDirectory(2);

            Assert.IsTrue(path.EndsWith("Projects\\CoreLibrary\\CommonLibraryTests", StringComparison.Ordinal), "Wrong Directory Name");
        }

        /// <summary>
        ///     Gets the file information.
        /// </summary>
        [TestMethod]
        public void GetFileInformation()
        {
            var file = Path.Combine(_path, Path.ChangeExtension(_pathOperations, ResourcesGeneral.TstExt)!);
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
        ///     Gets the file information.
        /// </summary>
        [TestMethod]
        public void GetNewFileName()
        {
            FileHandleDelete.DeleteAllContents(_path, true);
            var fileOne = Path.Combine(_path, nameof(GetNewFileName),
                Path.ChangeExtension(_pathOperations, ResourcesGeneral.TstExt)!);
            HelperMethods.CreateFile(fileOne);

            var info = FileUtility.GetNewFileName(fileOne);
            HelperMethods.CreateFile(info);

            Assert.IsTrue(info.EndsWith("Projects\\CoreLibrary\\CommonLibraryTests\\bin\\Debug\\net5.0-windows\\IoFileHandler\\GetNewFileName\\IO(0).txt", StringComparison.Ordinal), "Expected File Name");

            info = FileUtility.GetNewFileName(fileOne);

            Assert.IsTrue(info.EndsWith("Projects\\CoreLibrary\\CommonLibraryTests\\bin\\Debug\\net5.0-windows\\IoFileHandler\\GetNewFileName\\IO(1).txt", StringComparison.Ordinal), "Expected File Name");

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
                _pathOperations);

            Assert.IsTrue(check,
                "Path not Created: ");

            var expectedPath = Path.Combine(_path,
                nameof(GenerateFolder),
                _pathOperations);

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

            Assert.IsTrue(isDone, string.Concat("Folder cleaned: ", error));
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

            var file = Path.Combine(path, _pathOperations + ResourcesGeneral.TstExt);
            Debug.WriteLine(file);
            HelperMethods.CreateFile(file);

            isDone = FileHandleSearch.CheckIfFolderContainsElement(path);
            Assert.IsTrue(isDone, "File does not exist");

            //search for Files
            var subPathOne = Path.Combine(path, @"subOne\");
            var subPathTwo = Path.Combine(path, @"subTwo\");

            FileHandleCreate.CreateFolder(subPathOne);
            FileHandleCreate.CreateFolder(subPathTwo);
            var lst = FileHandleSearch.GetAllSubfolders(path);
            Assert.AreEqual(2, lst.Count, "Not the right amount of Sub folders");

            //delete File
            FileHandleDelete.DeleteFile(file);
            Assert.IsFalse(FileHandleSearch.CheckIfFolderContainsElement(path), "File was not deleted");

            //create File
            file = Path.Combine(subPathOne, _pathOperations + ResourcesGeneral.TstExt);
            Debug.WriteLine(file);
            HelperMethods.CreateFile(file);

            //copy Files
            FileHandleCopy.CopyFiles(subPathOne, subPathTwo, true);
            Debug.WriteLine(subPathTwo);

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

            var file = Path.Combine(_renamePath, _pathOperations, ResourcesGeneral.TstExt);
            var target = Path.Combine(_renamePath, "new.txt");
            Debug.WriteLine(file);
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
        public void RAppendageOfFile()
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
            Assert.AreEqual(2, lst.Count, "Not the right amount of Sub folders");

            //create File
            var file = Path.Combine(subPathOne, _pathOperations + ResourcesGeneral.TstExt);
            Debug.WriteLine(file);
            HelperMethods.CreateFile(file);

            file = Path.Combine(subPathTwo, _pathOperations + ResourcesGeneral.TstExt);
            Debug.WriteLine(file);
            HelperMethods.CreateFile(file);

            var firstTime = File.GetLastWriteTime(file);

            isDone = FileHandleCopy.CopyFilesReplaceIfNewer(subPathOne, subPathTwo);

            var secondTime = File.GetLastWriteTime(file);

            Assert.IsTrue(isDone, "File were not moved");
            Assert.AreEqual(firstTime, secondTime, "File was not the same");
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
            Assert.AreEqual(2, lst.Count, "Not the right amount of Sub folders");

            //create File
            var file = Path.Combine(subPathTwo, _pathOperations + ResourcesGeneral.TstExt);
            Debug.WriteLine(file);
            HelperMethods.CreateFile(file);

            file = Path.Combine(subPathTwoExtended, _pathOperations + ResourcesGeneral.TstExt);
            HelperMethods.CreateFile(file);
            lst = FileHandleSearch.GetFilesByExtensionFullPath(subPathTwo, ".*", true);

            isDone = FileHandleCopy.CopyFiles(lst, subPathOne, true);

            Assert.IsTrue(isDone, "File were not moved");
            lst = FileHandleSearch.GetFilesByExtensionFullPath(subPathOne, ".*", true);
            Assert.AreEqual(2, lst.Count, "Not enough Files were moved");
            lst = FileHandleSearch.GetFilesByExtensionFullPath(subPathTwo, ".*", true);
            Assert.AreEqual(2, lst.Count, "Files are still there");
            var result = FileHandleCopy.CopyFiles(subPathOne, subPathTwo);

            Assert.AreEqual(2, result.Count, "Enough files were accounted for");
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
            Assert.AreEqual(2, lst.Count, "Not the right amount of Sub folders");

            //create File
            var file = Path.Combine(subPathTwo, _pathOperations + ResourcesGeneral.TstExt);
            Debug.WriteLine(file);
            HelperMethods.CreateFile(file);

            file = Path.Combine(subPathTwoExtended, _pathOperations + ResourcesGeneral.TstExt);
            HelperMethods.CreateFile(file);
            lst = FileHandleSearch.GetFilesByExtensionFullPath(subPathTwo, ".*", true);

            isDone = FileHandleCut.CutFiles(lst, subPathOne, true);

            Assert.IsTrue(isDone, "File were not moved");
            lst = FileHandleSearch.GetFilesByExtensionFullPath(subPathOne, ".*", true);
            Assert.AreEqual(2, lst.Count, "Not enough Files were moved");
            lst = FileHandleSearch.GetFilesByExtensionFullPath(subPathTwo, ".*", true);
            Assert.AreEqual(0, lst.Count, "Files were not deleted");
            var result = FileHandleCopy.CopyFiles(subPathOne, subPathTwo);

            Assert.AreEqual(2, result.Count, "Enough files were accounted for");
        }

        [TestMethod]
        public void Compression()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Compress");
            FileHandleDelete.DeleteCompleteFolder(path);

            //list of the files
            var lst = new List<string>();

            //generate to Files
            var file = Path.Combine(path, _pathOperations + ResourcesGeneral.TstExt);
            lst.Add(file);
            HelperMethods.CreateFile(file);
            file = Path.Combine(path, _pathOperationsTwo + ResourcesGeneral.TstExt);
            lst.Add(file);
            HelperMethods.CreateFile(file);

            file = Path.Combine(path, "compressed.zip");
            var check = FileHandleCompress.SaveZip(file, lst, true);
            Assert.IsTrue(check, "Could not compress");

            var files = FileHandleSearch.GetAllFiles(path, false);

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
        ///     Check if our Exceptions do actual work
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileHandlerException), "Invalid Input. String was Empty")]
        public void CopyException()
        {
            FileHandleCopy.CopyFiles(string.Empty, null);
        }
    }
}
