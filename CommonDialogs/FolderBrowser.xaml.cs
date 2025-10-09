/* 
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonDialogs
 * FILE:        CommonDialogs/FolderBrowser.xaml.cs
 * PURPOSE:     Basic Folder Browser dialog based on WPF, using my own FolderControl.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.ComponentModel;
using System.Windows;

namespace CommonDialogs;

/// <inheritdoc cref="Window" />
/// <summary>
///     Simple Folder Browser dialog.
/// </summary>
[ToolboxItem(false)]
public sealed partial class FolderBrowser
{
    private readonly FolderViewModel _viewModel;

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
    public FolderBrowser(string startFolder)
    {
        InitializeComponent();

        // Set up the ViewModel
        _viewModel = new FolderViewModel();
        VFolder.DataContext = _viewModel;
        VFolder.Initiate(startFolder);
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
            ? _viewModel.Paths // take currently navigated folder
            : null;

        Close();
    }
}
