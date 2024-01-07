/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/FolderBrowser.xaml.cs
 * PURPOSE:     Old FolderBrowser restored
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System.ComponentModel;
using System.Windows;

//TODO add basic Folder Infos

namespace CommonControls
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     Simple Folder Browser
    ///     https://docs.microsoft.com/de-de/dotnet/api/system.drawing.design.toolboxitem?view=net-5.0
    /// </summary>
    [ToolboxItem(false)]
    public sealed partial class FolderBrowser
    {
        /// <inheritdoc />
        /// <summary>
        ///     Initiate Dialog
        /// </summary>
        internal FolderBrowser()
        {
            InitializeComponent();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initiate Dialog
        /// </summary>
        /// <param name="startFolder">target Folder</param>
        public FolderBrowser(string startFolder)
        {
            InitializeComponent();
            VFolder.Initiate(startFolder);
        }

        /// <summary>
        ///     Selected Path
        /// </summary>
        internal string Root { get; private set; }

        /// <summary>
        ///     Just close
        /// </summary>
        /// <param name="sender">Object</param>
        /// <param name="e">Event type</param>
        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Root = FolderControl.Root;
            Close();
        }

        /// <summary>
        ///     Close and reset selected folder
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The routed event arguments.</param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Root = string.Empty;
            Close();
        }
    }
}