/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     SQLiteGui
 * FILE:        SQLiteGui/DataOverview.xaml.cs
 * PURPOSE:     SqlLite graphical Front-end, User-control for the Display Part
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Windows;
using System.Windows.Controls;

// ReSharper disable MemberCanBeInternal, no not possible for User-control

namespace SQLiteGui
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     The data overview class.
    /// </summary>
    public sealed partial class DataOverview
    {
        /// <summary>
        ///     Element added,deleted,updated
        /// </summary>
        internal EventHandler RefreshTable;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:SQLiteGui.DataOverview" /> class.
        /// </summary>
        public DataOverview()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Set the data of the Overview Table
        /// </summary>
        internal void SetData()
        {
            TableData.ItemsSource = TableViewModel.Item;
        }

        /// <summary>
        ///     Selection of Overview Table was changed
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void TableData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Register.SelectionChanged(false, null);

            var tbi = TableData.SelectedItem as dynamic;
            if (tbi == null)
            {
                return;
            }

            Register.SelectionChanged(true, tbi);
        }

        /// <summary>
        ///     Update the Overview Table
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void RowUpdate_Click(object sender, RoutedEventArgs e)
        {
            var check = SqLiteGuiProcessing.UpdateItem();

            if (check)
            {
                RefreshTable?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Delete Element and refresh Overview Table
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void RowDelete_Click(object sender, RoutedEventArgs e)
        {
            var check = SqLiteGuiProcessing.DeleteItem();

            if (check)
            {
                RefreshTable?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Add Element and refresh Overview Table
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Type</param>
        private void RowAdd_Click(object sender, RoutedEventArgs e)
        {
            var check = SqLiteGuiProcessing.AddItem();

            if (check)
            {
                RefreshTable?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
