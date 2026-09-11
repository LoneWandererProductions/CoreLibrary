/* 
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Dialogs
 * FILE:        FolderBrowser.xaml.cs
 * PURPOSE:     Basic Folder Browser dialog based on WPF, using my own FolderControl.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.ComponentModel;
using System.Windows;

namespace Common.Dialogs
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     Simple Folder Browser dialog.
    /// </summary>
    [ToolboxItem(false)]
    public sealed partial class FolderBrowser
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the FolderBrowser dialog.
        /// </summary>
        public FolderBrowser() : this(string.Empty)
        {
        }

        /// <summary>
        ///     Initializes the dialog with a specified starting folder.
        /// </summary>
        /// <param name="startFolder">The target folder to start in.</param>
        public FolderBrowser(string? startFolder)
        {
            InitializeComponent();

            // Use FolderControl's own ViewModel as the single source of truth instead of creating
            // a second, competing FolderViewModel here and reassigning VFolder.DataContext to it.
            // That used to leave two separate instances alive: the XAML bindings inside
            // FolderControl.xaml ended up wired to whichever instance DataContext pointed at,
            // while FolderControl's code-behind (Enter-to-navigate, keyboard shortcuts, etc.)
            // reached for the OTHER, disconnected instance via its own ViewModel property - so
            // pressing Enter after typing a path silently did nothing.
            VFolder.Initiate(startFolder ?? string.Empty);
        }

        /// <summary>
        ///     The selected path after closing the dialog.
        /// </summary>
        internal string? Root { get; private set; }

        /// <summary>
        ///     Handles the OK button click event to confirm folder selection.
        /// </summary>
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            HandleButtonClick(true);
        }

        /// <summary>
        ///     Handles the Cancel button click event to reset the folder and close.
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            HandleButtonClick(false);
        }

        /// <summary>
        ///     Handles both OK and Cancel clicks to avoid duplicate code.
        /// </summary>
        /// <param name="isOkClicked">True if OK was clicked, false for Cancel.</param>
        private void HandleButtonClick(bool isOkClicked)
        {
            Root = isOkClicked
                ? VFolder.ViewModel.Paths // take currently navigated folder
                : null;

            Close();
        }
    }
}