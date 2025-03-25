/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonDialogs
 * FILE:        CommonDialogs/FolderView.xaml.cs
 * PURPOSE:     FolderView Control, can be used independent of the FolderBrowser
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal, do not make internal because it is used from the outside scope
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CommonDialogs
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///     Generic folder view
    /// </summary>
    /// <seealso cref="T:System.Windows.Controls.UserControl" />
    /// <seealso cref="T:System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="T:System.Windows.Controls.UserControl" />
    /// <seealso cref="T:System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="T:System.Windows.Markup.IComponentConnector" />
    public sealed partial class FolderControl : INotifyPropertyChanged
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
        /// <value>
        ///     The root.
        /// </value>
        public static string? Root { get; private set; }

        /// <summary>
        ///     Gets or sets the paths.
        /// </summary>
        /// <value>
        ///     The paths.
        /// </value>
        public string? Paths
        {
            get => Root;
            set
            {
                if (value == Root)
                {
                    return;
                }

                Root = value;
                OnPropertyChanged(nameof(Paths));
            }
        }

        /// <summary>
        ///     Gets or sets the look up.
        /// </summary>
        /// <value>
        ///     The look up.
        /// </value>
        public string LookUp
        {
            get => _lookUp;
            set
            {
                if (value == _lookUp)
                {
                    return;
                }

                _lookUp = value;
                OnPropertyChanged(nameof(LookUp));
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///     Called when [show files changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void OnShowFilesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FolderControl folderControl)
            {
                folderControl.SetItems(folderControl.Paths);
            }
        }

        /// <inheritdoc cref="PropertyChangedEventArgs" />
        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Main function to initiate the control with a specific path
        ///     Initiates the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        public void Initiate(string path)
        {
            SetItems(path);
        }

        /// <summary>
        ///     Sets the items asynchronous.
        ///     Function to set items (folders and files) asynchronously
        /// </summary>
        /// <param name="path">The path.</param>
        private async void SetItems(string path)
        {
            var directories = Array.Empty<string>();
            var files = Array.Empty<string>();

            // Use asynchronous loading to prevent UI freezing
            await Task.Run(() =>
            {
                if (Directory.Exists(path))
                {
                    directories = Directory.GetDirectories(path);
                    if (ShowFiles)
                    {
                        files = Directory.GetFiles(path);
                    }
                }
                else
                {
                    directories = Directory.GetLogicalDrives();
                }
            });

            // Clear TreeView before repopulating
            FoldersItem.Items.Clear();

            // Add directories asynchronously
            foreach (var item in directories.Select(CreateTreeViewItem))
            {
                _ = FoldersItem.Items.Add(item);
            }

            // Optionally add files asynchronously
            if (ShowFiles)
            {
                foreach (var fileItem in files.Select(file => new TreeViewItem { Header = Path.GetFileName(file), Tag = file }))
                {
                    _ = FoldersItem.Items.Add(fileItem);
                }
            }

            // Update Paths property with the current path
            Paths = path;
        }


        /// <summary>
        ///     Creates the TreeView item.
        ///     Function to create a TreeViewItem for a directory
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>TreeViewItem</returns>
        private TreeViewItem CreateTreeViewItem(string path)
        {
            var item = new TreeViewItem
            {
                Header = Path.GetFileName(path), Tag = path, FontWeight = FontWeights.Normal
            };

            // Placeholder for lazy loading of subdirectories
            _ = item.Items.Add(null);
            item.Expanded += FolderExpanded;
            return item;
        }

        /// <summary>
        ///     Handles the Expanded event of the Folder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private async void FolderExpanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;
            if (item.Items.Count != 1 || item.Items[0] != null)
            {
                return; // Already expanded
            }

            item.Items.Clear();
            var path = item.Tag.ToString();

            try
            {
                var subDirs = await Task.Run(() => Directory.GetDirectories(path));

                foreach (var subItem in subDirs.Select(CreateTreeViewItem))
                {
                    item.Items.Add(subItem);
                }

                if (ShowFiles)
                {
                    var files = await Task.Run(() => Directory.GetFiles(path));

                    foreach (var fileItem in files.Select(file => new TreeViewItem { Header = Path.GetFileName(file), Tag = file }))
                    {
                        item.Items.Add(fileItem);
                    }
                }
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
        ///     Event handler for when a folder is selected
        ///     Handles the SelectedItemChanged event of the FoldersItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedPropertyChangedEventArgs{Object}" /> instance containing the event data.</param>
        private void FoldersItemSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem {Tag: string selectedPath})
            {
                Paths = selectedPath;
            }
        }

        /// <summary>
        ///     Button up click.
        ///     Navigate up one level in the directory tree
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnUpClick(object sender, RoutedEventArgs e)
        {
            var path = Directory.GetParent(Paths);
            if (path != null)
            {
                SetItems(path.ToString());
            }
        }

        /// <summary>
        ///     Handles the Click event of the BtnGo control.
        ///     Go to the directory specified in the LookUp text box
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnGoClick(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(LookUp))
            {
                return;
            }

            SetItems(LookUp);
            LookUp = string.Empty;
        }

        /// <summary>
        ///     Navigate to the Desktop directory
        ///     Handles the Click event of the BtnDesktop control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnDesktopClick(object sender, RoutedEventArgs e)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            SetItems(path);
        }

        /// <summary>
        ///     Navigate to the root directory (C:\)
        ///     Button root click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnRootClick(object sender, RoutedEventArgs e)
        {
            SetItems(@"C:\");
        }

        /// <summary>
        ///     Navigate to the Documents directory
        ///     Button docs click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnDocsClick(object sender, RoutedEventArgs e)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            SetItems(path);
        }

        /// <summary>
        ///     Navigate to the user's profile directory
        ///     Handles the Click event of the Personal Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnPersonalClick(object sender, RoutedEventArgs e)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            SetItems(path);
        }

        /// <summary>
        ///     Navigate to the Pictures directory
        ///     Handles the Click event of the Picture Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnPictureClick(object sender, RoutedEventArgs e)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            SetItems(path);
        }


        /// <summary>
        ///     Create a new folder in the current directory
        ///     Handles the Click event of the Folder Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnFolderClick(object sender, RoutedEventArgs e)
        {
            var newDirPath = Path.Combine(Paths, "New Folder");
            var dirName = newDirPath;
            var i = 1;

            // Ensure the new folder name is unique
            while (Directory.Exists(dirName))
            {
                dirName = $"{newDirPath} ({i++})";
            }

            // Create the new directory and refresh the TreeView
            _ = Directory.CreateDirectory(dirName);
            SetItems(Paths);
        }

        /// <summary>
        ///     Open the current directory in Windows Explorer
        ///     Handles the Click event of the Explorer Button control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnExplorerClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Paths) && Directory.Exists(Paths))
            {
                _ = Process.Start(new ProcessStartInfo { FileName = Paths, UseShellExecute = true });
            }
        }
    }
}
