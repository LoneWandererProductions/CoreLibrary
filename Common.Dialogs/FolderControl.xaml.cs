/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Dialogs
 * FILE:        FolderControl.cs
 * PURPOSE:     FolderView Control, drop-in replacement for FolderBrowser, improved and thread-safe
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Common.Dialogs
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    /// Interaction logic for FolderControl.xaml
    /// </summary>
    public sealed partial class FolderControl
    {
        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        public FolderViewModel ViewModel { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderControl"/> class.
        /// </summary>
        public FolderControl()
        {
            InitializeComponent();

            ViewModel = new FolderViewModel();
            DataContext = ViewModel;
        }

        /// <summary>
        /// Expose a convenient method to initialize the control with a starting folder.
        /// </summary>
        /// <param name="startFolder">The start folder.</param>
        internal void Initiate(string startFolder)
        {
            ViewModel.Initiate(startFolder);
        }

        /// <summary>
        /// Handles the MouseDown event of the PathDisplay control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void PathDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Hide the display, show the box
            PathDisplay.Visibility = Visibility.Collapsed;
            PathEntry.Visibility = Visibility.Visible;

            // Focus the box and select all text for easy editing
            PathEntry.Focus();
            PathEntry.SelectAll();
        }

        /// <summary>
        /// Handles the LostFocus event of the PathEntry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void PathEntry_LostFocus(object sender, RoutedEventArgs e)
        {
            // Switch back when clicking away
            PathEntry.Visibility = Visibility.Collapsed;
            PathDisplay.Visibility = Visibility.Visible;
        }
    }
}
