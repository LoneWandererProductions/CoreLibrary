/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/SQLiteGuiProcessing.cs
 * PURPOSE:     Basic View Model
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using SQLiteHelper;

namespace SQLiteGui
{
    /// <summary>
    ///     Basic Database Activities
    /// </summary>
    internal sealed class SqLiteGuiProcessing
    {
        /// <summary>
        ///     Used for Information generation
        /// </summary>
        private static DbInfo _dbInfo;

        /// <summary>
        ///     Database Handler
        /// </summary>
        private static SqliteDatabase _db;

        /// <summary>
        ///     Where Clause for the Statements
        /// </summary>
        private static Binary _paramsClause;

        /// <summary>
        ///     Just needed to activate our Messenger
        /// </summary>
        /// <param name="dbInfo">Info Panel</param>
        internal SqLiteGuiProcessing(DbInfo dbInfo)
        {
            _dbInfo = dbInfo;
            if (_db != null)
            {
                _db.SendMessage += SendMessage;
            }
        }

        /// <summary>
        ///     Create a Database
        /// </summary>
        /// <param name="location">Local location of the Database</param>
        /// <param name="dbName">Name of Database</param>
        internal static void CreateDatabase(string location, string dbName)
        {
            if (string.IsNullOrEmpty(location))
            {
                _dbInfo.SetData(SqLiteGuiResource.ErrorNoValidPath);
                return;
            }

            if (string.IsNullOrEmpty(dbName))
            {
                _dbInfo.SetData(SqLiteGuiResource.ErrorNoValidDbName);
                return;
            }

            var check = _db.CreateDatabase(location, dbName, true);
            _dbInfo.SetData(check ? SqLiteGuiResource.InfoCreateDb : SqLiteGuiResource.ErrorCreateDb);
        }

        /// <summary>
        ///     Load a Database
        /// </summary>
        /// <param name="path">Target Path to Database</param>
        internal static void OpenDatabase(string path)
        {
            //Clean Detail View
            if (TableViewModel.Item != null)
            {
                TableViewModel.Item = null;
            }

            //Check if Path is Correct
            if (string.IsNullOrEmpty(path))
            {
                _dbInfo.SetData(SqLiteGuiResource.ErrorNoValidPath);
                return;
            }

            //Load Database
            var location = Directory.GetParent(path)?.ToString();
            var dbName = Path.GetFileName(path);

            _db = new SqliteDatabase(location, dbName);

            //Get Tables
            var lst = _db.GetTables();

            _dbInfo.SetData(_db.GetDatabaseInfos());

            //No Tables Catch and Process
            if (lst == null)
            {
                //clean Display needed for reload
                TablesViewModel.Item = null;
                //Print Information
                _dbInfo.SetData(SqLiteGuiResource.ErrorNoTables);
                return;
            }

            //Else Display the Table
            var table = lst.ConvertAll(tab => new TableDetails { TableAlias = tab });

            TablesViewModel.Item = table;
        }

        /// <summary>
        ///     Delete Selected Item from table
        /// </summary>
        /// <returns>Success Status</returns>
        internal static bool DeleteItem()
        {
            if (!GetWhereClause())
            {
                return false;
            }

            //Remove selected Item
            var count = _db.DeleteRows(Register.TableAlias, _paramsClause.Where, _paramsClause.Value);

            _dbInfo.SetData(SqLiteGuiResource.InfoRows + count);

            return count != -1;
        }

        /// <summary>
        ///     Select a Table
        /// </summary>
        /// <param name="tablealias">Name of the Table</param>
        internal static void SelectTable(string tablealias)
        {
            if (tablealias.Length == 0)
            {
                _dbInfo.SetData(SqLiteGuiResource.ErrorNoValidTableName);
            }

            var data = _db.Primary_Key_list(tablealias);
            var uniqueIndex = string.Empty;

            if (data.Count != 0)
            {
                uniqueIndex = data[0];

                _dbInfo.SetData(SqLiteGuiResource.InfoPrimaryKey + uniqueIndex);
            }
            else
            {
                _dbInfo.SetData(SqLiteGuiResource.WarningNoPrimaryKey);
            }

            //Load Data into our Table Details
            var db = _db.SimpleSelect(tablealias);
            TableViewModel.Item = db?.Raw;

            //get some basic Infos about our Database
            var tableInfo = _db.Pragma_TableInfo(tablealias);

            _dbInfo.SetData(SqLiteGuiResource.Header);

            foreach (var item in tableInfo.DColumns)
            {
                _dbInfo.SetData(string.Concat(
                    item.Key,
                    SqLiteGuiResource.Separator,
                    item.Value.DataType,
                    SqLiteGuiResource.Separator,
                    item.Value.Unique,
                    SqLiteGuiResource.Separator,
                    item.Value.PrimaryKey));
            }

            //Set Register with infos about current Table Selection
            Register.SelectedTable(tablealias, uniqueIndex);
        }

        /// <summary>
        ///     Show all logs from current Database
        /// </summary>
        internal static void ShowLogs()
        {
            if (string.IsNullOrEmpty(Register.ActiveDb))
            {
                _dbInfo.SetData(SqLiteGuiResource.ErrorNoValidDatabaseName);
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
            if (Register.TableAlias.Length == 0)
            {
                _dbInfo.SetData(SqLiteGuiResource.ErrorEmptySelect);
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
                _dbInfo.SetData(SqLiteGuiResource.ErrorEmptySelect);
                return false;
            }

            var pragma = _db.Pragma_TableInfo(Register.TableAlias);

            var item = GenerateUpdateItem(data, pragma);

            var inputwin = new InputUpdateWindow(item);
            _ = inputwin.ShowDialog();

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
            if (Register.TableAlias.Length == 0)
            {
                _dbInfo.SetData(SqLiteGuiResource.ErrorEmptySelect);
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
        ///     Delete Table Content
        /// </summary>
        /// <returns>Success Status</returns>
        internal static bool TruncateTable()
        {
            if (Register.TableAlias.Length != 0)
            {
                return _db.TruncateTable(Register.TableAlias);
            }

            _dbInfo.SetData(SqLiteGuiResource.ErrorEmptySelect);
            return false;
        }

        /// <summary>
        ///     Delete Table
        /// </summary>
        /// <returns>Success Status</returns>
        internal static bool DropTable()
        {
            if (Register.TableAlias.Length != 0)
            {
                return _db.DropTable(Register.TableAlias);
            }

            _dbInfo.SetData(SqLiteGuiResource.ErrorEmptySelect);
            return false;
        }

        /// <summary>
        ///     Copy Table
        /// </summary>
        /// <returns>Success Status</returns>
        internal static bool CopyTable()
        {
            var inputwin = new InputBinaryWindow
            (
                SQLiteGuiStringResource.InputWindowTitleCopy,
                SQLiteGuiStringResource.InputLblDescriptionCopy,
                SQLiteGuiStringResource.InputLblFirstSourceCopy,
                SQLiteGuiStringResource.InputLblSecondTargetCopy,
                Register.TableAlias
            );

            _ = inputwin.ShowDialog();

            return InputBinaryWindow.ParamsClause != null &&
                   _db.CopyTable(InputBinaryWindow.ParamsClause.Where, InputBinaryWindow.ParamsClause.Value);
        }

        /// <summary>
        ///     Rename Table
        /// </summary>
        /// <returns>Success Status</returns>
        internal static bool RenameTable()
        {
            var inputwin = new InputBinaryWindow
            (
                SQLiteGuiStringResource.InputWindowTitleRename,
                SQLiteGuiStringResource.InputLblDescriptionRename,
                SQLiteGuiStringResource.InputLblFirstSourceRename,
                SQLiteGuiStringResource.InputLblSecondTargetRename,
                Register.TableAlias
            );

            _ = inputwin.ShowDialog();

            return InputBinaryWindow.ParamsClause != null &&
                   _db.RenameTable(InputBinaryWindow.ParamsClause.Where, InputBinaryWindow.ParamsClause.Value);
        }

        /// <summary>
        ///     Add a Table
        /// </summary>
        /// <returns>Success Status</returns>
        internal static bool AddTable()
        {
            if (string.IsNullOrEmpty(Register.ActiveDb))
            {
                _dbInfo.SetData(SqLiteGuiResource.ErrorNoValidDatabaseName);
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
        ///     Generate an Item we want to Update
        /// </summary>
        /// <param name="tableSet"></param>
        /// <param name="pragma">Table Info</param>
        /// <returns>Item we will Replace</returns>
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
                            DataType = item.Value.DataType,
                            HeaderName = item.Key,
                            Value = SqLiteGuiResource.ParamDummy
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

            _dbInfo.SetData(SqLiteGuiResource.WarningNoPrimaryKey);

            var inputwin = new InputBinaryWindow
            (
                SQLiteGuiStringResource.InputWindowTitleWhere,
                SQLiteGuiStringResource.InputLblDescriptionWhereClause,
                SQLiteGuiStringResource.InputLblFirstWhere,
                SQLiteGuiStringResource.InputLblSecondValueWhere
            );

            _ = inputwin.ShowDialog();

            if (InputBinaryWindow.ParamsClause == null)
            {
                return false;
            }

            _paramsClause = InputBinaryWindow.ParamsClause;
            return true;
        }

        /// <summary>
        ///     Send our Message
        /// </summary>
        /// <param name="sender">Control Sender</param>
        /// <param name="e">Type of Event</param>
        private static void SendMessage(object sender, string e)
        {
            _dbInfo.SetData(e);
        }
    }
}
