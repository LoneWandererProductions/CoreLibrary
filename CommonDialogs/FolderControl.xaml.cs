/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonDialogs
 * FILE:        CommonDialogs/FolderControl.cs
 * PURPOSE:     FolderView Control, drop-in replacement for FolderBrowser, improved and thread-safe
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Windows.Controls;

namespace CommonDialogs;

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
    /// <param name="startFolder"></param>
    internal void Initiate(string startFolder)
    {
        ViewModel.Initiate(startFolder);
    }
}
