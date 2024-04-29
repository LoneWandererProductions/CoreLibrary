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
using System.Windows;
using System.Windows.Controls;

namespace CommonDialogs
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     Folder View Control
    ///     TODO add more Images, optional Files
    /// </summary>
    public sealed partial class FolderControl : INotifyPropertyChanged
    {
        /// <summary>
        ///     The look up
        /// </summary>
        private string _lookUp;

        /// <inheritdoc />
        /// <summary>
        ///     startup the control
        /// </summary>
        public FolderControl()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets the root.
        /// </summary>
        /// <value>
        ///     The root.
        /// </value>
        public static string Root { get; private set; }

        /// <summary>
        ///     Gets or sets the path.
        /// </summary>
        /// <value>
        ///     The path.
        /// </value>
        public string Paths
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
        ///     Triggers if an Attribute gets changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Initiate Control
        /// </summary>
        /// <param name="path">target Folder</param>
        public void Initiate(string path)
        {
            SetItems(path);
        }

        /// <summary>
        ///     Set the items.
        /// </summary>
        private void SetItems(string path)
        {
            string[] directory;

            if (!Directory.Exists(path))
            {
                directory = Directory.GetLogicalDrives();
                if (directory.Length == 0)
                {
                    return;
                }

                path = directory[0];
            }
            else
            {
                directory = Directory.GetDirectories(path);
            }

            //Clear
            FoldersItem.Items.Clear();

            //Just get all HD
            foreach (var item in directory.Select(s => new TreeViewItem
                     {
                         Header = s, Tag = s, FontWeight = FontWeights.Normal
                     }))
            {
                //add Method and dummy
                _ = item.Items.Add(null);
                item.Expanded += Folder_Expanded;
                _ = FoldersItem.Items.Add(item);
            }

            Paths = path;
        }

        /// <summary>
        ///     We clicked on expand Button
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private static void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            var item = (TreeViewItem)sender;
            if (item.Items.Count != 1 || item.Items[0] != null)
            {
                return;
            }

            item.Items.Clear();

            try
            {
                foreach (var subItem in Directory.GetDirectories(item.Tag.ToString()!).Select(path => new TreeViewItem
                         {
                             Header =
                                 path[(path.LastIndexOf(ComCtlResources.Path, StringComparison.Ordinal) + 1)..],
                             Tag = path,
                             FontWeight = FontWeights.Normal
                         }))
                {
                    //add Method and dummy
                    _ = subItem.Items.Add(null);
                    subItem.Expanded += Folder_Expanded;
                    _ = item.Items.Add(subItem);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (ArgumentException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (IOException ex)
            {
                Trace.WriteLine(ex);
            }
            catch (InvalidOperationException ex)
            {
                Trace.WriteLine(ex);
            }
        }

        /// <summary>
        ///     We select an item
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void FoldersItem_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var tree = sender as TreeView;

            var selection = (TreeViewItem)tree?.SelectedItem;

            if (selection != null)
            {
                Paths = selection.Tag.ToString();
            }
        }

        /// <summary>
        ///     The btn up click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            var path = Directory.GetParent(Paths);

            if (path != null)
            {
                SetItems(path.ToString());
            }
        }

        /// <summary>
        ///     The btn go click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(LookUp))
            {
                return;
            }

            SetItems(LookUp);
            LookUp = string.Empty;
        }

        /// <summary>
        ///     The Button desktop click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnDesktop_Click(object sender, RoutedEventArgs e)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            SetItems(path);
        }

        /// <summary>
        ///     The Button root click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnRoot_Click(object sender, RoutedEventArgs e)
        {
            var dir = Directory.GetLogicalDrives();
            if (dir.Length == 0)
            {
                return;
            }

            var path = dir[0];

            SetItems(path);
        }

        /// <summary>
        ///     The Button docs click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnDocs_Click(object sender, RoutedEventArgs e)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);

            SetItems(path);
        }

        /// <summary>
        ///     The Button personal click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnPersonal_Click(object sender, RoutedEventArgs e)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            SetItems(path);
        }

        /// <summary>
        ///     Handles the Click event of the Button Picture control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnPicture_Click(object sender, RoutedEventArgs e)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            SetItems(path);
        }

        /// <summary>
        ///     Handles the Click event of the BtnExplorer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnExplorer_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(Paths))
            {
                return;
            }

            _ = Process.Start(ComCtlResources.Explorer, Paths);
        }

        /// <summary>
        ///     Handles the Click event of the Button Folder control.
        ///     Create a new Folder here
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void BtnFolder_Click(object sender, RoutedEventArgs e)
        {
            var input = new InputBox(ComCtlResources.HeaderDirectoryName, ComCtlResources.TextNameFolder);
            _ = input.ShowDialog();

            if (string.IsNullOrEmpty(input.InputText))
            {
                return;
            }

            var path = Path.Combine(Paths, input.InputText);
            _ = Directory.CreateDirectory(path);
        }
    }
}
