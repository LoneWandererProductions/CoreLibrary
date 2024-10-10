/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/AddTableWindow.xaml.cs
 * PURPOSE:     Add new Tables
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.ObjectModel;
using System.Windows;
using SqliteHelper;

namespace SQLiteGui
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     Add our new Tables here
    /// </summary>
    internal sealed partial class AddTableWindow
    {
        /// <summary>
        ///     The Button add (readonly).
        /// </summary>
        private readonly string _btnAdd;

        /// <summary>
        ///     The Button cancel (readonly).
        /// </summary>
        private readonly string _btnCancel;

        /// <summary>
        ///     The Button delete (readonly).
        /// </summary>
        private readonly string _btnDelete;

        /// <summary>
        ///     The Button execute (readonly).
        /// </summary>
        private readonly string _btnExecute;

        /// <summary>
        ///     The table name label (readonly).
        /// </summary>
        private readonly string _tableNameLbl;

        /// <summary>
        ///     The title (readonly).
        /// </summary>
        private readonly string _title;

        /// <inheritdoc />
        /// <summary>
        ///     Standard Constructor not used only by the IDE <see cref="T:SQLiteGui.AddTableWindow" /> class.
        /// </summary>
        internal AddTableWindow()
        {
            TableElements = new ObservableCollection<TableColumnsExtended>();
            InitializeComponent();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Add Table Window
        /// </summary>
        /// <param name="title">Title of Window</param>
        /// <param name="tableNameLbl">Name of the Table</param>
        /// <param name="btnExecute">Execute Button</param>
        /// <param name="btnCancel">Cancel Button</param>
        /// <param name="btnAdd">Add Button</param>
        /// <param name="btnDelete">Add Delete Button</param>
        internal AddTableWindow(string title, string tableNameLbl, string btnExecute, string btnCancel, string btnAdd,
            string btnDelete)
        {
            TableElements = new ObservableCollection<TableColumnsExtended>();
            InitializeComponent();

            _title = title;
            _tableNameLbl = tableNameLbl;
            _btnExecute = btnExecute;
            _btnCancel = btnCancel;
            _btnAdd = btnAdd;
            _btnDelete = btnDelete;
            TableInfos = null;
        }

        /// <summary>
        ///     Our Table overview
        /// </summary>
        public ObservableCollection<TableColumnsExtended> TableElements { get; }

        /// <summary>
        ///     Gets the table info.
        /// </summary>
        internal static TableObject TableInfos { get; private set; }

        /// <summary>
        ///     Get the basics in Place
        /// </summary>
        /// <param name="sender">Control Sender</param>
        /// <param name="e">Type of Event</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = _title;
            TableNameLbl.Content = _tableNameLbl;
            BtnExecute.Content = _btnExecute;
            BtnCancel.Content = _btnCancel;
            BtnAdd.Content = _btnAdd;
            BtnDelete.Content = _btnDelete;
        }

        /// <summary>
        ///     Just close
        /// </summary>
        /// <param name="sender">Control Sender</param>
        /// <param name="e">Type of Event</param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Convert to the needed Format
        /// </summary>
        /// <param name="sender">Control Sender</param>
        /// <param name="e">Type of Event</param>
        private void BtnExecute_Click(object sender, RoutedEventArgs e)
        {
            if (TxtBoxTableName.Text.Length == 0)
            {
                CreateErrorBox(SqLiteGuiResource.ErrorNoValidTableName);
                return;
            }

            var tbl = new TableObject { Header = TxtBoxTableName.Text };
            var dct = new DictionaryTableColumns();

            foreach (var table in TableElements)
            {
                var columns = new TableColumns
                {
                    DataType = table.DataType,
                    NotNull = table.NotNull,
                    PrimaryKey = table.PrimaryKey,
                    Unique = table.Unique
                };

                if (dct.DColumns.ContainsKey(table.Header))
                {
                    CreateErrorBox(SqLiteGuiResource.ErrorDuplicateTableName);
                    return;
                }

                dct.DColumns.Add(table.Header, columns);
            }

            tbl.Columns = dct;
            TableInfos = tbl;

            //Finally Close
            Close();
        }

        /// <summary>
        ///     Display a Message box
        /// </summary>
        /// <param name="error">Error Message</param>
        private static void CreateErrorBox(string error)
        {
            _ = MessageBox.Show(error, SQLiteGuiStringResource.MsgBxError, MessageBoxButton.OK);
        }

        /// <summary>
        ///     Add another Column
        /// </summary>
        /// <param name="sender">Control Sender</param>
        /// <param name="e">Type of Event</param>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var tbl = new TableColumnsExtended();
            TableElements.Add(tbl);
        }

        /// <summary>
        ///     Delete Element
        /// </summary>
        /// <param name="sender">Control Sender</param>
        /// <param name="e">Type of Event</param>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridTbl.SelectedItem is not TableColumnsExtended selectedItem)
            {
                return;
            }

            _ = TableElements.Remove(selectedItem);
        }
    }

    /// <summary>
    ///     Helper Object to feed into Sqlite Helper
    /// </summary>
    internal sealed class TableObject
    {
        /// <summary>
        ///     Gets or sets the header.
        /// </summary>
        public string Header { get; init; }

        /// <summary>
        ///     Gets or sets the columns.
        /// </summary>
        public DictionaryTableColumns Columns { get; set; }
    }
}
