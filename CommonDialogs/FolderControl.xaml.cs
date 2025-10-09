/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonDialogs
 * FILE:        CommonDialogs/FolderControl.cs
 * PURPOSE:     FolderView Control, drop-in replacement for FolderBrowser, improved and thread-safe
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Windows.Controls;

namespace CommonDialogs;

/// <summary>
/// Interaction logic for FolderControl.xaml
/// </summary>
public partial class FolderControl : UserControl
{
    public FolderViewModel ViewModel { get; }


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
    public void Initiate(string startFolder)
    {
        ViewModel.Initiate(startFolder);
    }
}

