/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/SQLiteGuiProcessing.cs
 * PURPOSE:     Basic View Model
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using SqliteHelper;
using DataSet = SqliteHelper.DataSet;

namespace SQLiteGui;

/// <summary>
///     Basic Database Activities
/// </summary>
internal static class SqLiteGuiProcessing
{
    /// <summary>
    ///     Database Handler
    /// </summary>
    private static ISqliteDatabase _db;

    /// <summary>
    ///     Where Clause for the Statements
    /// </summary>
    private static Binary _paramsClause;

    /// <summary>
    ///     Create a Database
    /// </summary>
    /// <param name="location">Local location of the Database</param>
    /// <param name="dbName">Name of Database</param>
    internal static void CreateDatabase(string location, string dbName)
    {
        try
        {
            if (string.IsNullOrEmpty(location))
            {
                Register.Info.AppendInfo(SqLiteGuiResource.ErrorNoValidPath);
                return;
            }

            if (string.IsNullOrEmpty(dbName))
            {
                Register.Info.AppendInfo(SqLiteGuiResource.ErrorNoValidDbName);
                return;
            }

            var check = _db.CreateDatabase(location, dbName, true);
            Register.Info.AppendInfo(check ? SqLiteGuiResource.InfoCreateDb : SqLiteGuiResource.ErrorCreateDb);
        }
        catch (Exception ex)
        {
            Register.Info.AppendInfo($"Error creating database: {ex.Message}");
        }
    }

    /// <summary>
    ///     Load a Database
    /// </summary>
    /// <param name="path">Target Path to Database</param>
    internal static void OpenDatabase(string path)
    {
        //Check if Path is Correct
        if (string.IsNullOrEmpty(path))
        {
            Register.Info.AppendInfo(SqLiteGuiResource.ErrorNoValidPath);
            return;
        }

        //Load Database
        var location = Directory.GetParent(path)?.ToString();
        var dbName = Path.GetFileName(path);

        _db = new SqliteDatabase(location, dbName);

        if (Register.Info != null)
        {
            Register.Info.AppendInfo(_db.GetDatabaseInfos());
        }
    }

    /// <summary>
    ///     Gets the table details.
    /// </summary>
    /// <returns>List of tables</returns>
    internal static IEnumerable<TableDetails> GetTableDetails()
    {
        if (_db == null)
        {
            if (Register.Info != null)
            {
                Register.Info.AppendInfo("No Database Connection provided.");
            }

            return null;
        }

        //Get Tables
        var lst = _db.GetTables();

        //No Tables Catch and Process
        if (lst != null)
        {
            return lst.ConvertAll(tab => new TableDetails { TableAlias = tab });
        }

        //Print Information
        Register.Info.AppendInfo(SqLiteGuiResource.ErrorNoTables);
        return null;
    }

    /// <summary>
    ///     Delete Selected Item from table
    /// </summary>
    /// <returns>Success Status</returns>
    internal static bool DeleteItem()
    {
        if (_db == null)
        {
            Register.Info.AppendInfo("Database not initialized.");
            return false;
        }

        if (!GetWhereClause())
        {
            return false;
        }

        var count = _db.DeleteRows(Register.TableAlias, _paramsClause.Where, _paramsClause.Value);
        Register.Info.AppendInfo(SqLiteGuiResource.InfoRows + count);

        return count != -1;
    }

    /// <summary>
    ///     Select a Table
    /// </summary>
    /// <param name="tableAlias">Name of the Table</param>
    internal static DataView SelectTable(string tableAlias)
    {
        if (tableAlias.Length == 0)
        {
            Register.Info.AppendInfo(SqLiteGuiResource.ErrorNoValidTableName);
        }

        var data = _db.Primary_Key_list(tableAlias);
        var uniqueIndex = string.Empty;

        if (data.Count != 0)
        {
            uniqueIndex = data[0];

            Register.Info.AppendInfo(SqLiteGuiResource.InfoPrimaryKey + uniqueIndex);
        }
        else
        {
            Register.Info.AppendInfo(SqLiteGuiResource.WarningNoPrimaryKey);
        }

        //Load Data into our Table Details
        var db = _db.SimpleSelect(tableAlias);

        //get some basic Infos about our Database
        var tableInfo = _db.Pragma_TableInfo(tableAlias);

        Register.Info.AppendInfo(SqLiteGuiResource.Header);

        foreach (var item in tableInfo.DColumns)
        {
            Register.Info.AppendInfo(string.Concat(
                item.Key,
                SqLiteGuiResource.Separator,
                item.Value.DataType,
                SqLiteGuiResource.Separator,
                item.Value.Unique,
                SqLiteGuiResource.Separator,
                item.Value.PrimaryKey));
        }

        //Set Register with infos about current Table Selection
        Register.SelectedTable(tableAlias, uniqueIndex);


        if (db == null)
        {
            Register.Info.AppendInfo("Table was empty.");
            return null;
        }

        return db.Raw;
    }

    /// <summary>
    ///     Show all logs from current Database
    /// </summary>
    internal static void ShowLogs()
    {
        if (string.IsNullOrEmpty(Register.ActiveDb))
        {
            Register.Info.AppendInfo(SqLiteGuiResource.ErrorNoValidDatabaseName);
            return;
        }

        var logs = new LogsWindow(SQLiteGuiStringResource.LogsWindowTitle, _db.ListErrors, _db.LogFile);
        _ = logs.ShowDialog();
    }

    /// <summary>
    ///     Update selected Items
    /// </summary>
    /// <returns>Success Status</returns>
    internal static bool UpdateItem()
    {
        if (!CheckTableAlias())
        {
            return false;
        }

        if (!GetWhereClause())
        {
            return false;
        }

        var data = _db.SimpleSelect(Register.TableAlias, _paramsClause.Where,
            CompareOperator.Equal, _paramsClause.Value);

        if (data == null)
        {
            Register.Info.AppendInfo(SqLiteGuiResource.ErrorEmptySelect);
            return false;
        }

        var pragma = _db.Pragma_TableInfo(Register.TableAlias);

        var item = GenerateUpdateItem(data, pragma);

        var inputWin = new InputUpdateWindow(item);
        _ = inputWin.ShowDialog();

        if (InputUpdateWindow.TableRow == null)
        {
            return false;
        }

        var count = _db.UpdateTable(Register.TableAlias, CompareOperator.Equal, _paramsClause.Where,
            _paramsClause.Value, InputUpdateWindow.TableRow.Row);

        return count != -1;
    }

    /// <summary>
    ///     Add an Item
    /// </summary>
    /// <returns>Success Status</returns>
    internal static bool AddItem()
    {
        if (!CheckTableAlias())
        {
            return false;
        }

        var pragma = _db.Pragma_TableInfo(Register.TableAlias);

        var item = GenerateUpdateItem(pragma);

        var input = new InputUpdateWindow(item);
        _ = input.ShowDialog();

        return InputUpdateWindow.TableRow != null &&
               _db.InsertSingleRow(Register.TableAlias, InputUpdateWindow.TableRow, true);
    }

    /// <summary>
    ///     Truncates the table.
    /// </summary>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>Success Status</returns>
    internal static bool TruncateTable(string tableAlias)
    {
        Register.TableAlias = tableAlias;

        if (Register.TableAlias.Length != 0)
        {
            return _db.TruncateTable(tableAlias);
        }

        Register.Info.AppendInfo(SqLiteGuiResource.ErrorEmptySelect);
        return false;
    }


    /// <summary>
    ///     Drops the table.
    /// </summary>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>Success Status</returns>
    internal static bool DropTable(string tableAlias)
    {
        Register.TableAlias = tableAlias;

        if (Register.TableAlias.Length != 0)
        {
            return _db.DropTable(tableAlias);
        }

        Register.Info.AppendInfo(SqLiteGuiResource.ErrorEmptySelect);
        return false;
    }

    /// <summary>
    ///     Copies the table.
    /// </summary>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>Success Status</returns>
    internal static bool CopyTable(string tableAlias)
    {
        Register.TableAlias = tableAlias;

        var inputWin = new InputBinaryWindow
        (
            SQLiteGuiStringResource.InputWindowTitleCopy,
            SQLiteGuiStringResource.InputLblDescriptionCopy,
            SQLiteGuiStringResource.InputLblFirstSourceCopy,
            SQLiteGuiStringResource.InputLblSecondTargetCopy,
            Register.TableAlias
        );

        _ = inputWin.ShowDialog();

        return InputBinaryWindow.ParamsClause != null &&
               _db.CopyTable(InputBinaryWindow.ParamsClause.Where, InputBinaryWindow.ParamsClause.Value);
    }

    /// <summary>
    ///     Renames the table.
    /// </summary>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>Success Status</returns>
    internal static bool RenameTable(string tableAlias)
    {
        Register.TableAlias = tableAlias;

        var inputWin = new InputBinaryWindow
        (
            SQLiteGuiStringResource.InputWindowTitleRename,
            SQLiteGuiStringResource.InputLblDescriptionRename,
            SQLiteGuiStringResource.InputLblFirstSourceRename,
            SQLiteGuiStringResource.InputLblSecondTargetRename,
            tableAlias
        );

        _ = inputWin.ShowDialog();

        return InputBinaryWindow.ParamsClause != null &&
               _db.RenameTable(InputBinaryWindow.ParamsClause.Where, InputBinaryWindow.ParamsClause.Value);
    }

    /// <summary>
    ///     Add a Table
    /// </summary>
    /// <returns>Success Status</returns>
    internal static bool AddTable()
    {
        if (!CheckTableAlias())
        {
            return false;
        }

        var addTable = new AddTableWindow
        (
            SQLiteGuiStringResource.AddTableWindowNameTitel,
            SQLiteGuiStringResource.AddTableWindowNameLbl,
            SQLiteGuiStringResource.BtnExecute,
            SQLiteGuiStringResource.BtnCancel,
            SQLiteGuiStringResource.BtnAdd,
            SQLiteGuiStringResource.BtnDelete
        );
        _ = addTable.ShowDialog();

        return AddTableWindow.TableInfos != null &&
               _db.CreateTable(AddTableWindow.TableInfos.Header, AddTableWindow.TableInfos.Columns);
    }

    /// <summary>
    ///     Closes the database.
    /// </summary>
    internal static void CloseDatabase()
    {
        _db = null;
    }

    /// <summary>
    ///     Generate an Item we want to Update
    /// </summary>
    /// <param name="tableSet">The table set.</param>
    /// <param name="pragma">Table Info</param>
    /// <returns>
    ///     Item we will Replace
    /// </returns>
    private static IEnumerable<UpdateItem> GenerateUpdateItem(DataSet tableSet, DictionaryTableColumns pragma)
    {
        var lst = GenerateUpdateItem(pragma);

        var slice = tableSet.Columns(SqLiteGuiResource.StartColumn);

        for (var i = 0; i < lst.Count; i++)
        {
            lst[i].Value = slice[i];
        }

        return lst;
    }

    /// <summary>
    ///     Generate a Single Item to add
    /// </summary>
    /// <param name="pragma">Table Info</param>
    /// <returns>Item we will Add</returns>
    private static List<UpdateItem> GenerateUpdateItem(DictionaryTableColumns pragma)
    {
        return pragma.DColumns.Select(
                item =>
                    new UpdateItem
                    {
                        DataType = item.Value.DataType, HeaderName = item.Key, Value = SqLiteGuiResource.ParamDummy
                    })
            .ToList();
    }

    /// <summary>
    ///     Get the where clause
    /// </summary>
    /// <returns>if something went wrong</returns>
    private static bool GetWhereClause()
    {
        if (!Register.IsDetailActive)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(Register.PrimaryKey))
        {
            _paramsClause = new Binary
            {
                Where = Register.PrimaryKey, Value = Register.PrimaryKeyItem(Register.PrimaryKey)
            };
            return true;
        }

        Register.Info.AppendInfo(SqLiteGuiResource.WarningNoPrimaryKey);

        var inputWin = new InputBinaryWindow
        (
            SQLiteGuiStringResource.InputWindowTitleWhere,
            SQLiteGuiStringResource.InputLblDescriptionWhereClause,
            SQLiteGuiStringResource.InputLblFirstWhere,
            SQLiteGuiStringResource.InputLblSecondValueWhere
        );

        _ = inputWin.ShowDialog();

        if (InputBinaryWindow.ParamsClause == null)
        {
            return false;
        }

        _paramsClause = InputBinaryWindow.ParamsClause;
        return true;
    }

    /// <summary>
    ///     Checks the table alias.
    /// </summary>
    /// <returns>Success Status.</returns>
    private static bool CheckTableAlias()
    {
        if (!string.IsNullOrEmpty(Register.TableAlias))
        {
            return true;
        }

        Register.Info.AppendInfo(SqLiteGuiResource.ErrorEmptySelect);
        return false;
    }

    /// <summary>
    ///     Loads the CSV.
    /// </summary>
    /// <param name="csvData">The CSV data.</param>
    /// <returns>Success Status.</returns>
    internal static bool LoadCsv(List<List<string>> csvData)
    {
        return _db.LoadCsv(Register.TableAlias, csvData, true);
    }

    /// <summary>
    ///     Exports the CVS.
    /// </summary>
    /// <returns>The table Data in our csv format</returns>
    internal static List<List<string>> ExportCvs()
    {
        return _db.ExportCvs(Register.TableAlias, true);
    }
}
