/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonDialogs
 * FILE:        CommonDialogs/FolderControl.cs
 * PURPOSE:     FolderView Control, drop-in replacement for FolderBrowser, improved and thread-safe
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CommonDialogs;

/// <summary>
/// Generic folder view
/// </summary>
/// <seealso cref="System.Windows.Controls.UserControl" />
/// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
/// <seealso cref="System.Windows.Markup.IComponentConnector" />
/// <inheritdoc cref="UserControl" />
/// <seealso cref="T:System.Windows.Controls.UserControl" />
/// <seealso cref="T:System.ComponentModel.INotifyPropertyChanged" />
public sealed partial class FolderControl : UserControl, INotifyPropertyChanged
{
    /// <summary>
    ///     The dependency property for showing files
    /// </summary>
    public static readonly DependencyProperty ShowFilesProperty =
        DependencyProperty.Register(nameof(ShowFiles), typeof(bool), typeof(FolderControl),
            new PropertyMetadata(false, OnShowFilesChanged));

    /// <summary>
    ///     The look up
    /// </summary>
    private string _lookUp;

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="T:CommonDialogs.FolderControl" /> class.
    /// </summary>
    public FolderControl()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Gets or sets the ShowFiles dependency property.
    /// </summary>
    public bool ShowFiles
    {
        get => (bool)GetValue(ShowFilesProperty);
        set => SetValue(ShowFilesProperty, value);
    }

    /// <summary>
    ///     Gets the root.
    /// </summary>
    public static string? Root { get; private set; }

    /// <summary>
    ///     Gets or sets the paths.
    /// </summary>
    public string? Paths
    {
        get => Root;
        set
        {
            if (value == Root) return;
            Root = value;
            OnPropertyChanged(nameof(Paths));
        }
    }

    /// <summary>
    ///     Gets or sets the look up.
    /// </summary>
    public string LookUp
    {
        get => _lookUp;
        set
        {
            if (value == _lookUp) return;
            _lookUp = value;
            OnPropertyChanged(nameof(LookUp));
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Called when [property changed].
    /// </summary>
    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    ///     Called when [show files changed].
    /// </summary>
    private static void OnShowFilesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FolderControl folderControl)
        {
            _ = folderControl.SetItems(folderControl.Paths);
        }
    }

    /// <summary>
    ///     Main function to initiate the control with a specific path
    /// </summary>
    public void Initiate(string path) => _ = SetItems(path);

    /// <summary>
    ///     Sets the items asynchronously.
    ///     Function to set items (folders and files) asynchronously
    /// </summary>
    private async Task SetItems(string path)
    {
        if (string.IsNullOrEmpty(path)) path = string.Empty;

        string[] directories = Array.Empty<string>();
        string[] files = Array.Empty<string>();

        // Fetch directories and files off the UI thread
        await Task.Run(() =>
        {
            if (Directory.Exists(path))
            {
                directories = Directory.GetDirectories(path);
                if (ShowFiles) files = Directory.GetFiles(path);
            }
            else
            {
                directories = Directory.GetLogicalDrives();
            }
        });

        // Update UI safely on the main thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            FoldersItem.Items.Clear();

            foreach (var dir in directories.Select(CreateTreeViewItem))
                FoldersItem.Items.Add(dir);

            if (ShowFiles)
            {
                foreach (var file in files)
                    FoldersItem.Items.Add(new TreeViewItem { Header = Path.GetFileName(file), Tag = file });
            }

            Paths = path;
        });
    }

    /// <summary>
    ///     Creates the TreeView item.
    /// </summary>
    private TreeViewItem CreateTreeViewItem(string path)
    {
        var item = new TreeViewItem
        {
            Header = Path.GetFileName(path),
            Tag = path,
            FontWeight = FontWeights.Normal
        };

        // Placeholder for lazy loading
        item.Items.Add(null);
        item.Expanded += FolderExpanded;
        return item;
    }

    /// <summary>
    ///     Handles the Expanded event of the Folder control.
    /// </summary>
    private async void FolderExpanded(object sender, RoutedEventArgs e)
    {
        if (sender is not TreeViewItem item) return;
        if (item.Items.Count != 1 || item.Items[0] != null) return;

        item.Items.Clear();
        var path = item.Tag?.ToString();
        if (string.IsNullOrEmpty(path)) return;

        string[] subDirs = Array.Empty<string>();
        string[] files = Array.Empty<string>();

        try
        {
            await Task.Run(() =>
            {
                subDirs = Directory.GetDirectories(path);
                if (ShowFiles) files = Directory.GetFiles(path);
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var sub in subDirs.Select(CreateTreeViewItem)) item.Items.Add(sub);

                if (ShowFiles)
                {
                    foreach (var file in files)
                        item.Items.Add(new TreeViewItem { Header = Path.GetFileName(file), Tag = file });
                }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            Trace.WriteLine($"Access denied to {path}: {ex.Message}");
        }
        catch (IOException ex)
        {
            Trace.WriteLine($"Error accessing {path}: {ex.Message}");
        }
    }

    /// <summary>
    ///     Handles selection changed of the TreeView
    /// </summary>
    private void FoldersItemSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is TreeViewItem { Tag: string selectedPath })
            Paths = selectedPath;
    }

    /// <summary>
    ///     Navigate up one level
    /// </summary>
    private void BtnUpClick(object sender, RoutedEventArgs e)
    {
        var path = Directory.GetParent(Paths)?.FullName;
        if (!string.IsNullOrEmpty(path))
            _ = SetItems(path);
    }

    /// <summary>
    ///     Go to the path in LookUp
    /// </summary>
    private void BtnGoClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(LookUp) && Directory.Exists(LookUp))
            _ = SetItems(LookUp);
        LookUp = string.Empty;
    }

    /// <summary>
    ///     Open current directory in Explorer
    /// </summary>
    private void BtnExplorerClick(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(Paths) && Directory.Exists(Paths))
            _ = Process.Start(new ProcessStartInfo { FileName = Paths, UseShellExecute = true });
    }

    /// <summary>
    /// BTNs the desktop click.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    /// <returns></returns>
    private void BtnDesktopClick(object sender, RoutedEventArgs e)
        => _ = SetItems(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

    /// <summary>
    /// BTNs the root click.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    /// <returns></returns>
    private void BtnRootClick(object sender, RoutedEventArgs e) => _ = SetItems(@"C:\");

    /// <summary>
    /// BTNs the docs click.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    /// <returns></returns>
    private void BtnDocsClick(object sender, RoutedEventArgs e)
        => _ = SetItems(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

    /// <summary>
    /// BTNs the personal click.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    /// <returns></returns>
    private void BtnPersonalClick(object sender, RoutedEventArgs e)
        => _ = SetItems(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

    /// <summary>
    /// BTNs the picture click.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
    /// <returns></returns>
    private void BtnPictureClick(object sender, RoutedEventArgs e)
        => _ = SetItems(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));

    /// <summary>
    ///     Create a new folder in the current directory
    /// </summary>
    private void BtnFolderClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Paths)) return;

        var newDirPath = Path.Combine(Paths, "New Folder");
        var dirName = newDirPath;
        int i = 1;

        while (Directory.Exists(dirName))
            dirName = $"{newDirPath} ({i++})";

        Directory.CreateDirectory(dirName);
        _ = SetItems(Paths);
    }
}
