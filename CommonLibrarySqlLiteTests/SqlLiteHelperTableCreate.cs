/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonLibrarySqlLiteTests
 * FILE:        CommonLibrarySqlLiteTests/SqlLiteHelperTableCreate.cs
 * PURPOSE:     Create tests
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteHelper;

namespace CommonLibrarySqlLiteTests;

/// <summary>
///     The sql lite helper table create unit test class.
/// </summary>
[TestClass]
public sealed class SqlLiteHelperTableCreate
{
    /// <summary>
    ///     The db name one (const). Value: "DbTableCreateOne.db".
    /// </summary>
    private const string DbNameOne = "DbTableCreateOne.db";

    /// <summary>
    ///     The db name two (const). Value: "DbTableCreateTwo.db".
    /// </summary>
    private const string DbNameTwo = "DbTableCreateTwo.db";

    /// <summary>
    ///     The db name three (const). Value: "DbTableCreateThree.db".
    /// </summary>
    private const string DbNameThree = "DbTableCreateThree.db";

    /// <summary>
    ///     The db name four (const). Value: "DbTableCreateFour.db".
    /// </summary>
    private const string DbNameFour = "DbTableCreateFour.db";

    /// <summary>
    ///     The db name five (const). Value: "DbTableCreateFive.db".
    /// </summary>
    private const string DbNameFive = "DbTableCreateFive.db";

    /// <summary>
    ///     The db source (const). Value: "Source".
    /// </summary>
    private const string DbSource = "Source";

    /// <summary>
    ///     The db target (const). Value: "Target".
    /// </summary>
    private const string DbTarget = "Target";

    /// <summary>
    ///     The First header (const). Value: "First".
    /// </summary>
    private const string FrstHeader = "First";

    /// <summary>
    ///     The Second header (const). Value: "Second".
    /// </summary>
    private const string ScdHeader = "Second";

    /// <summary>
    ///     The Third header (const). Value: "Third".
    /// </summary>
    private const string ThrdHeader = "Third";

    /// <summary>
    ///     The Fourth header (const). Value: "Fourth".
    /// </summary>
    private const string FrthHeader = "Fourth";

    /// <summary>
    ///     The Fifth header (const). Value: "Fifth".
    /// </summary>
    private const string FfthHeader = "Fifth";

    /// <summary>
    ///     The Table name first (const). Value: "Tbl_First".
    /// </summary>
    private const string TblNameFirst = "Tbl_First";

    /// <summary>
    ///     The Table name second (const). Value: "Tbl_Second".
    /// </summary>
    private const string TblNameSecond = "Tbl_Second";

    /// <summary>
    ///     The Table name third (const). Value: "Tbl_Third".
    /// </summary>
    private const string TblNameThird = "Tbl_Third";

    /// <summary>
    ///     The target (readonly). Value: new SqlLiteDatabase().
    /// </summary>
    private readonly SqliteDatabase _target = new();

    /// <summary>
    ///     Test for creating Table
    ///     Create
    ///     Delete
    /// </summary>
    [TestMethod]
    public void CreateTableMultipleRows()
    {
        _target.SendMessage += SharedHelperClass.DebugPrints;

        //cleanup
        SharedHelperClass.CleanUp(ResourcesSqlLite.Root + Path.DirectorySeparatorChar + DbNameOne);

        //Check if file was created
        _target.CreateDatabase(ResourcesSqlLite.Root, DbNameOne, true);

        var elementone = new TableColumns
        {
            DataType = SqLiteDataTypes.Text, PrimaryKey = false, Unique = false, NotNull = false
        };

        var elementtwo = new TableColumns
        {
            DataType = SqLiteDataTypes.Integer, PrimaryKey = true, Unique = true, NotNull = false
        };

        var elementthree = new TableColumns
        {
            DataType = SqLiteDataTypes.Text, PrimaryKey = false, Unique = true, NotNull = true
        };

        var elementfour = new TableColumns
        {
            DataType = SqLiteDataTypes.DateTime, PrimaryKey = false, Unique = false, NotNull = false
        };

        var elementfive = new TableColumns
        {
            DataType = SqLiteDataTypes.Real, PrimaryKey = false, Unique = false, NotNull = false
        };

        var columns = new DictionaryTableColumns();

        columns.DColumns.Add(FrstHeader, elementone);
        columns.DColumns.Add(ScdHeader, elementtwo);
        columns.DColumns.Add(ThrdHeader, elementthree);
        columns.DColumns.Add(FrthHeader, elementfour);
        columns.DColumns.Add(FfthHeader, elementfive);

        var check = _target.CreateTable(TblNameFirst, columns);
        Assert.IsTrue(check, "Test failed create: " + _target.LastErrors);

        var lst = _target.GetTables();

        Assert.AreEqual(1, lst.Count,
            "Test failed right Number of Tables, Numbers: " + lst.Count + " Errors: " + _target.LastErrors);

        Assert.AreEqual(TblNameFirst, lst[0], "Test failed right Name of the Table, Name: " + lst[0]);

        var str = _target.GetDatabaseInfos();

        Assert.AreNotEqual(str, string.Empty, "Test failed Get Table Status: " + str + " " + _target.LastErrors);
        Trace.WriteLine(str);
    }

    /// <summary>
    ///     Test for creating Table
    ///     Create
    ///     Delete
    /// </summary>
    [TestMethod]
    public void CreateTableTwoRows()
    {
        _target.SendMessage += SharedHelperClass.DebugPrints;
        //cleanup
        SharedHelperClass.CleanUp(ResourcesSqlLite.Root + Path.DirectorySeparatorChar + DbNameTwo);

        //Check if file was created
        _target.CreateDatabase(ResourcesSqlLite.Root, DbNameTwo, true);

        var elementone = new TableColumns
        {
            DataType = SqLiteDataTypes.Text, PrimaryKey = false, Unique = false, NotNull = false
        };

        var elementtwo = new TableColumns
        {
            DataType = SqLiteDataTypes.Integer, PrimaryKey = true, Unique = true, NotNull = false
        };

        var columns = new DictionaryTableColumns();

        columns.DColumns.Add(FrstHeader, elementone);
        columns.DColumns.Add(ScdHeader, elementtwo);

        var check = _target.CreateTable(TblNameFirst, columns);
        Assert.IsTrue(check, "Test failed create: " + _target.LastErrors);
    }

    /// <summary>
    ///     Test for creating Table
    ///     Create
    ///     Delete
    /// </summary>
    [TestMethod]
    public void CreateTableOneRows()
    {
        _target.SendMessage += SharedHelperClass.DebugPrints;
        //cleanup
        SharedHelperClass.CleanUp(Path.Combine(ResourcesSqlLite.Root, DbNameThree));

        //Check if file was created
        _target.CreateDatabase(ResourcesSqlLite.Root, DbNameThree, true);

        var elementone = new TableColumns
        {
            DataType = SqLiteDataTypes.Text, PrimaryKey = false, Unique = false, NotNull = false
        };

        var columns = new DictionaryTableColumns();

        columns.DColumns.Add(FrstHeader, elementone);

        var check = _target.CreateTable(TblNameFirst, columns);
        Assert.IsTrue(check, "Test failed create: " + _target.LastErrors);

        //Clean Up
        _target.DeleteDatabase(_target.Location, DbNameThree);
    }

    /// <summary>
    ///     Test if we can create a Database
    ///     And delete it afterwards
    /// </summary>
    [TestMethod]
    public void ValidColumnNames()
    {
        _target.SendMessage += SharedHelperClass.DebugPrints;

        //cleanup
        SharedHelperClass.CleanUp(ResourcesSqlLite.DbValidColumnNames);

        //Check if file was created
        _target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbValidColumnNames, true);
        Assert.IsTrue(File.Exists(ResourcesSqlLite.DbValidColumnNames),
            "Test failed Create: " + _target.LastErrors);

        //add Column with specific Name
        var element = new TableColumns
        {
            DataType = SqLiteDataTypes.Text, PrimaryKey = false, Unique = false, NotNull = false
        };

        var columns = new DictionaryTableColumns();

        columns.DColumns.Add("Test_Header_one", element);

        var check = _target.CreateTable(TblNameFirst, columns);
        Assert.IsTrue(check, "Test failed create: " + _target.LastErrors);

        //Test the

        var rslt = _target.Pragma_TableInfo(TblNameFirst);

        Assert.AreEqual("Test_Header_one", rslt.DColumns.First().Key, "Name was not created correct.");

        _target.DeleteDatabase(_target.Location, ResourcesSqlLite.DbValidColumnNames);
    }

    /// <summary>
    ///     Test for creating Table
    ///     Create
    ///     Delete
    /// </summary>
    [TestMethod]
    public void CreateTableTwoUniqueRows()
    {
        _target.SendMessage += SharedHelperClass.DebugPrints;
        //cleanup
        SharedHelperClass.CleanUp(ResourcesSqlLite.Root + Path.DirectorySeparatorChar + DbNameFour);

        //Check if file was created
        _target.CreateDatabase(ResourcesSqlLite.Root, DbNameFour, true);

        var elementone = new TableColumns
        {
            DataType = SqLiteDataTypes.Text, PrimaryKey = true, Unique = false, NotNull = false
        };

        var columns = new DictionaryTableColumns();

        columns.DColumns.Add(FrstHeader, elementone);
        columns.DColumns.Add(ScdHeader, elementone);

        var check = _target.CreateTable(TblNameFirst, columns);
        Assert.IsFalse(check, $"Test failed Unique Key caught: {_target.LastErrors}");
    }

    /// <summary>
    ///     Create table and get Table Infos
    /// </summary>
    [TestMethod]
    public void DeleteTable()
    {
        _target.SendMessage += SharedHelperClass.DebugPrints;
        //cleanup
        SharedHelperClass.CleanUp(ResourcesSqlLite.Root + Path.DirectorySeparatorChar + DbNameFive);

        //Check if file was created
        _target.CreateDatabase(ResourcesSqlLite.Root, DbNameFive, true);

        var elementOne = new TableColumns
        {
            DataType = SqLiteDataTypes.Text, PrimaryKey = false, Unique = false, NotNull = false
        };

        var elementTwo = new TableColumns
        {
            DataType = SqLiteDataTypes.Integer, PrimaryKey = true, Unique = true, NotNull = false
        };

        var elementThree = new TableColumns
        {
            DataType = SqLiteDataTypes.Text, PrimaryKey = false, Unique = true, NotNull = true
        };

        var elementFour = new TableColumns
        {
            DataType = SqLiteDataTypes.DateTime, PrimaryKey = false, Unique = false, NotNull = false
        };

        var elementFive = new TableColumns
        {
            DataType = SqLiteDataTypes.Real, PrimaryKey = false, Unique = false, NotNull = false
        };

        var columns = new DictionaryTableColumns();

        columns.DColumns.Add(FrstHeader, elementOne);
        columns.DColumns.Add(ScdHeader, elementTwo);
        columns.DColumns.Add(ThrdHeader, elementThree);
        columns.DColumns.Add(FrthHeader, elementFour);
        columns.DColumns.Add(FfthHeader, elementFive);

        _target.CreateTable(TblNameFirst, columns);

        columns = new DictionaryTableColumns();

        columns.DColumns.Add(FrstHeader, elementOne);
        columns.DColumns.Add(ScdHeader, elementTwo);

        _target.CreateTable(TblNameSecond, columns);

        columns = new DictionaryTableColumns();

        columns.DColumns.Add(FrstHeader, elementOne);

        _target.CreateTable(TblNameThird, columns);

        var check = _target.DropTable(TblNameFirst);
        Assert.IsTrue(check, "Test failed delete First Table: " + _target.LastErrors);
        check = _target.DropTable(TblNameSecond);
        Assert.IsTrue(check, "Test failed delete Second Table: " + _target.LastErrors);
        check = _target.DropTable(TblNameThird);
        Assert.IsTrue(check, "Test failed delete Third Table: " + _target.LastErrors);
    }

    /// <summary>
    ///     Create table , test Pragma and make a copy of the table
    /// </summary>
    [TestMethod]
    public void CopyTable()
    {
        _target.SendMessage += SharedHelperClass.DebugPrints;

        //cleanup
        SharedHelperClass.CleanUp(ResourcesSqlLite.PathCopyTable);

        //Check if file was created
        _target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbCopyTable, true);
        Assert.IsTrue(File.Exists(ResourcesSqlLite.PathCopyTable),
            "Test failed Create " + _target.LastErrors);

        var header = SharedHelperClass.CreateTableHeadersMultiple();
        //create the Table
        var check = _target.CreateTable(DbSource, header);
        Assert.IsTrue(check, "Test failed Create Table" + _target.LastErrors);

        var cache = _target.Pragma_TableInfo(DbSource);

        check = SharedHelperClass.CheckPragmaTableInfo(cache, header);
        Assert.IsTrue(check, "Test failed Compare Results" + _target.LastErrors);

        check = _target.CopyTable(DbSource, DbTarget);

        Assert.IsTrue(check, "Test failed Copy Table" + _target.LastErrors);

        cache = _target.Pragma_TableInfo(DbTarget);

        check = SharedHelperClass.CheckPragmaTableInfo(cache, header);
        Assert.IsTrue(check, "Test failed Compare Results" + _target.LastErrors);
    }

    /// <summary>
    ///     Create table , test Pragma and make a copy of the table
    /// </summary>
    [TestMethod]
    public void CopyTableAdvanced()
    {
        _target.SendMessage += SharedHelperClass.DebugPrints;

        //cleanup
        SharedHelperClass.CleanUp(ResourcesSqlLite.PathCopyTableAdvanced);

        //Check if file was created
        _target.CreateDatabase(ResourcesSqlLite.Root, ResourcesSqlLite.DbCopyTableAdvanced, true);
        Assert.IsTrue(File.Exists(ResourcesSqlLite.PathCopyTableAdvanced),
            "Test failed Create " + _target.LastErrors);

        var header = SharedHelperClass.CreateTableHeadersMultiple();
        //create the Table
        var check = _target.CreateTable(DbSource, header);
        Assert.IsTrue(check, "Test failed Create Table" + _target.LastErrors);

        var cache = _target.Pragma_TableInfo(DbSource);

        check = SharedHelperClass.CheckPragmaTableInfo(cache, header);
        Assert.IsTrue(check, "Test failed Compare Results" + _target.LastErrors);

        //now with more Content
        var table = SharedHelperClass.CreateContent();

        check = _target.InsertMultipleRow(DbSource, table, false);
        Assert.IsTrue(check, "Test not passed Insert into Table: " + _target.LastErrors);

        check = _target.CopyTable(DbSource, DbTarget);

        Assert.IsTrue(check, "Test failed Copy Table" + _target.LastErrors);

        cache = _target.Pragma_TableInfo(DbTarget);

        check = SharedHelperClass.CheckPragmaTableInfo(cache, header);
        Assert.IsTrue(check, "Test failed Compare Results" + _target.LastErrors);

        check = _target.DropTable(DbTarget);
        //Use the advanced feature
        Assert.IsTrue(check, "Delete failed" + _target.LastErrors);

        //Select simple
        var set = _target.SimpleSelect(DbSource);
        var result = new List<TableSet>(set.Row);
        result.RemoveAt(set.Row.Count - 1);

        Assert.AreEqual(8, set.Height, "Test failed Copy Table, wrong amount old: " + set.Height);

        check = _target.CopyTable(DbSource, DbTarget, result);

        Assert.IsTrue(check, "Test failed Copy Table" + _target.LastErrors);

        set = _target.SimpleSelect(DbTarget);
        Assert.AreEqual(7, set.Height, "Test failed Copy Table, wrong amount new: " + set.Height);
    }
}
