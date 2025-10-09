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
using System.Threading.Tasks;
using FileHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommonLibraryTests;

/// <summary>
///     Basic File Operations
/// </summary>
[TestClass]
public sealed class IoFileSearch
{
    /// <summary>
    ///     The _pathOperations (readonly). Value: "IO".
    /// </summary>
    private const string PathOperations = "IO";

    /// <summary>
    ///     The path (readonly). Value: Path.Combine(Directory.GetCurrentDirectory(), ResourcesGeneral.CampaignsFolder).
    /// </summary>
    private readonly string _path = Path.Combine(Directory.GetCurrentDirectory(), nameof(IoFileHandler));

    /// <summary>
    ///     Simple Check for getting files Contains in a Folder
    /// </summary>
    [TestMethod]
    public async Task GetFilesByExtensionWithExtensionAsync()
    {
        var isDone = FileHandleDelete.DeleteCompleteFolder(_path);
        Assert.IsTrue(isDone, "Could not cleanup");

        var check = false;
        var file = Path.Combine(_path, Path.ChangeExtension(PathOperations, ResourcesGeneral.TstExt));
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

        check = await FileHandleDelete.DeleteFile(file);

        Assert.IsTrue(check, "Did not delete File");
    }

    /// <summary>
    ///     Simple Check for getting files Contains in a Folder
    /// </summary>
    [TestMethod]
    public async Task GetFilesByExtensionWithoutExtensionAsync()
    {
        var isDone = FileHandleDelete.DeleteCompleteFolder(_path);
        Assert.IsTrue(isDone, "Could not cleanup");

        var file = Path.Combine(_path, Path.ChangeExtension(PathOperations, ResourcesGeneral.TstExt));

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

        check = await FileHandleDelete.DeleteFile(file);

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
    public async Task GetFileInformationAsync()
    {
        var file = Path.Combine(_path, Path.ChangeExtension(PathOperations, ResourcesGeneral.TstExt));
        HelperMethods.CreateFile(file);

        var info = FileHandleSearch.GetFileDetails(file);

        Assert.IsNotNull(info, "no results");

        Assert.AreEqual(100, info.Size, "Correct size");
        Assert.AreEqual(".txt", info.Extension, "Correct Extension");
        Assert.AreEqual("IO.txt", info.FileName, "Correct FileName");
        //the rest is null because it is a fresh created file no need to check it.

        _ = await FileHandleDelete.DeleteFile(file);
        var check = File.Exists(file);
        Assert.IsFalse(check, "File not deleted");
    }


    /// <summary>
    ///     Simple Check for getting files Contained in a Folder
    ///     Check Lock Status
    ///     Set Retries
    /// </summary>
    [TestMethod]
    public async Task GetFilesByExtensionFullPathAsync()
    {
        var isDone = FileHandleDelete.DeleteCompleteFolder(_path);
        Assert.IsTrue(isDone, "Could not cleanup");

        //Set Amount of Repeats
        FileHandlerRegister.Tries = 2;

        var file = Path.Combine(_path, Path.ChangeExtension(PathOperations, ResourcesGeneral.TstExt));
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

        var check = await FileHandleDelete.DeleteFile(file);

        Assert.IsTrue(check, "Deleted File");

        _ = FileHandleDelete.DeleteFile(file);
        check = File.Exists(file);
        Assert.IsFalse(check, "File not deleted");
    }
}
