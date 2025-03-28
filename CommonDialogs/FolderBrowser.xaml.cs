/*
 * COPYRIGHT:   See COPYING in the top-level directory
 * PROJECT:     CommonDialogs
 * FILE:        CommonDialogs/FolderBrowser.xaml.cs
 * PURPOSE:     Old FolderBrowser restored
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System;
using System.ComponentModel;
using System.Windows;

// TODO: Add basic Folder Infos

namespace CommonDialogs
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
        internal FolderBrowser() : this(string.Empty) { }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes the dialog with a specified starting folder.
        /// </summary>
        /// <param name="startFolder">The target folder to start in.</param>
        public FolderBrowser(string startFolder)
        {
            try
            {
                InitializeComponent();
                VFolder.Initiate(startFolder);
            }
            catch (Exception ex)
            {
                // Handle potential XAML loading errors
                Console.WriteLine($"Error initializing FolderBrowser: {ex.Message}");
            }
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
            Root = isOkClicked ? FolderControl.Root : null; // Set Root only if OK is clicked
            Close();
        }
    }
}
