/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Core.Viewer
 * FILE:        CoreViewer/MainWindow.xaml.cs
 * PURPOSE:     Entry for the WPF application.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Core.Viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow
    {
        /// <summary>
        /// The last header clicked
        /// </summary>
        private GridViewColumnHeader? _lastHeaderClicked = null;

        /// <summary>
        /// The last direction
        /// </summary>
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Grids the view column header clicked handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;

            // Ignore clicks on the padding space next to the columns or columns without a Tag
            if (headerClicked == null ||
                headerClicked.Role == GridViewColumnHeaderRole.Padding ||
                headerClicked.Column.Header == null)
            {
                return;
            }

            var sortBy = headerClicked.Column.Header.ToString();

            ListSortDirection direction;
            if (headerClicked != _lastHeaderClicked)
            {
                direction = ListSortDirection.Ascending;
            }
            else
            {
                // Toggle direction if clicking the same column again
                direction = _lastDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }

            Sort(sortBy, direction);

            _lastHeaderClicked = headerClicked;
            _lastDirection = direction;
        }

        /// <summary>
        /// Sorts the specified sort by.
        /// </summary>
        /// <param name="sortBy">The sort by.</param>
        /// <param name="direction">The direction.</param>
        private void Sort(string sortBy, ListSortDirection direction)
        {
            // Get the view that WPF automatically created for the ItemsSource
            var dataView = CollectionViewSource.GetDefaultView(DiagnosticsListView.ItemsSource);

            if (dataView == null) return;

            dataView.SortDescriptions.Clear();
            var sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }
    }
}
