﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrarySqlLiteTests
 * FILE:        CommonLibrarySqlLiteTests/SqlLiteHelperDbCreate.cs
 * PURPOSE:     Database Create
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteHelper;

namespace CommonLibrarySqlLiteTests;

/// <summary>
///     The sql lite helper db create unit test class.
/// </summary>
[TestClass]
public sealed class SqlLiteHelperDbCreate
{
    /// <summary>
    ///     The table name (const). Value: "bogus".
    /// </summary>
    private const string TableName = "bogus";

    /// <summary>
    ///     The header first (const). Value: "First".
    /// </summary>
    private const string HeaderFirst = "First";

    /// <summary>
    ///     The header second (const). Value: "Second".
    /// </summary>
    private const string HeaderSecond = "Second";

    /// <summary>
    ///     The SqlLite Interface.
    /// </summary>
    private static readonly SqliteDatabase Target = new();

    /// <summary>
    ///     Test if we can create a Database
    ///     And delete it afterwards
    /// </summary>
    [TestMethod]
    public void CreateDatabase()
    {
        Target.SendMessage += SharedHelperClass.DebugPrints;

        //cleanup
        SharedHelperClass.CleanUp(ResourcesSqlLite.PathDbCreate);

        //Check if file was created
        Target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbDatabaseCreate, true);
        Assert.IsTrue(File.Exists(ResourcesSqlLite.PathDbCreate),
            "Test failed Create: " + Target.LastErrors);

        //check if file was deleted
        Target.DeleteDatabase(Target.Location, ResourcesSqlLite.DbDatabaseCreate);
        Assert.IsFalse(File.Exists(ResourcesSqlLite.PathDbCreate),
            "Test failed Delete: " + Target.LastErrors);
    }

    /// <summary>
    ///     Test if we can detach and attach a Database
    ///     Absolutely useless right now
    ///     https://www.tutorialspoint.com/sqlite/sqlite_detach_database.htm
    /// </summary>
    [TestMethod]
    public void DetachDatabase()
    {
        Target.SendMessage += SharedHelperClass.DebugPrints;

        //cleanup
        SharedHelperClass.CleanUp(ResourcesSqlLite.PathDbDetach);
        SharedHelperClass.CleanUp(ResourcesSqlLite.PathDbCreate);

        //Check if file was created
        Target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbDetach, true);
        Assert.IsTrue(File.Exists(ResourcesSqlLite.PathDbDetach),
            "Test failed Create: " + Target.LastErrors);

        //add some content
        var elementone = new TableColumns
        {
            DataType = SqLiteDataTypes.Text, PrimaryKey = false, Unique = false, NotNull = false
        };

        var columns = new DictionaryTableColumns();

        columns.DColumns.Add("First", elementone);

        Target.CreateTable("First", columns);

        //Check if file was created
        Target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbDatabaseCreate, true);
        Assert.IsTrue(File.Exists(ResourcesSqlLite.PathDbCreate),
            "Test passes Create " + Target.LastErrors);

        //add some content
        elementone = new TableColumns
        {
            DataType = SqLiteDataTypes.Text, PrimaryKey = false, Unique = false, NotNull = false
        };

        columns = new DictionaryTableColumns();

        columns.DColumns.Add("Second", elementone);

        Target.CreateTable("Second", columns);

        //Test attach and Detach
        var check = Target.AttachDatabase("DbDetach", "DbDetach");
        Assert.IsTrue(check, "Test failed attach Database: " + Target.LastErrors);
    }

    /// <summary>
    ///     Test Config and Settings
    /// </summary>
    [TestMethod]
    public void ConfigConnection()
    {
        var config = new SqliteDatabase("Path", "Name of Db", 2);

        config.SendMessage += SharedHelperClass.DebugPrints;

        config.MaxLinesError = 10;

        config.MaxLinesLog = 20;

        var cache = config.GetConnectionDetails();

        Assert.AreEqual("Name of Db", cache.DbName,
            "Test failed Wrong Data: " + cache.DbName);

        Assert.IsTrue(cache.Location.Contains(@"Path"),
            "Test failed Wrong Data: " + cache.Location);

        Assert.AreEqual(3, cache.DbVersion,
            "Test failed Wrong Data: " + cache.DbVersion);

        Assert.AreEqual(2, cache.TimeOut,
            "Test failed Wrong Data: " + cache.TimeOut);

        Assert.AreEqual(0, cache.ListErrors.Count,
            "Test failed Wrong Data: " + cache.ListErrors.Count);

        Assert.AreEqual(10, cache.MaxLinesError,
            "Test failed Wrong Data: " + cache.MaxLinesError);

        Assert.AreEqual(20, cache.MaxLinesLog,
            "Test failed Wrong Data: " + cache.MaxLinesLog);
    }

    /// <summary>
    ///     Test if we can create a Database
    ///     And try to trigger some Exceptions
    /// </summary>
    [TestMethod]
    public void BreakDatabase()
    {
        Target.SendMessage += SharedHelperClass.DebugPrints;

        //cleanup
        SharedHelperClass.CleanUp(ResourcesSqlLite.PathDbCrash);

        //Check if file was created
        Target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbCrash, true);
        Assert.IsTrue(File.Exists(ResourcesSqlLite.PathDbCrash),
            "Test failed Create: " + Target.LastErrors);

        //select a bogus DB
        Target.SimpleSelect(TableName);

        /*
         * Target is not to get the right Error Message but to enforce Exceptions
         */
        Assert.IsTrue(Target.LastErrors.Contains("Table does not exist"), "Test did not failed correctly");

        var elementOne = new TableColumns
        {
            DataType = SqLiteDataTypes.Text, PrimaryKey = false, Unique = false, NotNull = false
        };

        var elementTwo = new TableColumns
        {
            DataType = SqLiteDataTypes.Integer, PrimaryKey = true, Unique = true, NotNull = false
        };

        var columns = new DictionaryTableColumns();

        columns.DColumns.Add(HeaderFirst, elementOne);
        columns.DColumns.Add(HeaderSecond, elementTwo);

        Target.CreateTable(TableName, columns);

        Target.InsertMultipleRow(TableName, null, true);

        /*
         * Target is not to get the right Error Message but to enforce Exceptions
         */
        Assert.IsTrue(Target.LastErrors.Contains("Input Value was empty"),
            "Test did not failed correctly");

        var check = Target.InsertMultipleRow("bogus", new List<TableSet>(), true);

        /*
         * Target is not to get the right Error Message but to enforce Exceptions
         */
        Assert.IsFalse(check, "Should have returned early");
    }
}
