/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/InputUpdateWindow.xaml.cs
 * PURPOSE:     Update Window
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SQLiteHelper;

namespace SQLiteGui
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     The input update window class.
    /// </summary>
    internal sealed partial class InputUpdateWindow
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:SQLiteGui.InputUpdateWindow" /> class.
        /// </summary>
        internal InputUpdateWindow()
        {
            InitializeComponent();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:SQLiteGui.InputUpdateWindow" /> class.
        /// </summary>
        /// <param name="items">Table Data</param>
        internal InputUpdateWindow(IEnumerable<UpdateItem> items)
        {
            InitializeComponent();
            SetData(items);
        }

        /// <summary>
        ///     Gets the table row.
        /// </summary>
        internal static TableSet TableRow { get; private set; }

        /// <summary>
        ///     Set the data.
        /// </summary>
        /// <param name="items">The items.</param>
        private void SetData(IEnumerable<UpdateItem> items)
        {
            DtGrdUpdate.ItemsSource = items;
        }

        /// <summary>
        ///     Close the mess and set values if possible
        /// </summary>
        /// <param name="sender">Control Sender</param>
        /// <param name="e">Type of Event</param>
        private void BtnOkay_Click(object sender, RoutedEventArgs e)
        {
            TableRow = DtGrdUpdate.ItemsSource == null ? null : ConvertItems();

            Close();
        }

        /// <summary>
        ///     Close the mess and set values to null
        /// </summary>
        /// <param name="sender">Control Sender</param>
        /// <param name="e">Type of Event</param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            TableRow = null;
            Close();
        }

        /// <summary>
        ///     Convert the Update Item into an usable Object for the Update
        /// </summary>
        /// <returns>Converted Item as TableSet</returns>
        private TableSet ConvertItems()
        {
            var tbl = new TableSet();
            if (DtGrdUpdate.ItemsSource is not IEnumerable<UpdateItem> data)
            {
                return tbl;
            }

            tbl.Row.AddRange(from item in data select item.Value);
            return tbl;
        }
    }
}
