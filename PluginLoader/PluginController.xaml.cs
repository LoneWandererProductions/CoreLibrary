/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     PluginLoader
 * FILE:        PluginLoader/PluginController.xaml.cs
 * PURPOSE:     Plugin Control, that displays all plugins
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace PluginLoader
{
    /// <inheritdoc cref="INotifyPropertyChanged" />
    /// <summary>
    ///     Plugin Manager
    /// </summary>
    public sealed partial class PluginController : INotifyPropertyChanged
    {
        /// <summary>
        ///     The plugin path
        /// </summary>
        public static readonly DependencyProperty TargetPathProperty = DependencyProperty.Register(nameof(TargetPath),
            typeof(string),
            typeof(PluginController), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ExtensionProperty = DependencyProperty.Register(nameof(Extension),
            typeof(string),
            typeof(PluginController), new PropertyMetadata(default(string)));

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:PluginLoader.PluginController" /> class.
        /// </summary>
        public PluginController()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the plugin path.
        ///     The path must be equal to the Current Directory or a sub Directory.
        /// </summary>
        /// <value>
        ///     The plugin path.
        /// </value>
        public string TargetPath
        {
            get => (string)GetValue(TargetPathProperty);
            set => SetValue(TargetPathProperty, value);
        }

        /// <summary>
        ///     Gets or sets the extension.
        /// </summary>
        /// <value>
        ///     The extension.
        /// </value>
        public string Extension
        {
            get => (string)GetValue(ExtensionProperty);
            set => SetValue(ExtensionProperty, value);
        }

        /// <summary>
        ///     Gets or sets the observable plugin.
        /// </summary>
        /// <value>
        ///     The observable plugin.
        /// </value>
        public ObservableCollection<PluginItem> ObservablePlugin { get; set; } = new();

        /// <inheritdoc />
        /// <summary>
        ///     Tells the Components something was changed
        ///     Needed since we have to trigger it user defined
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     The notify property changed.
        /// </summary>
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Handles the MouseDoubleClick event of the DataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataGrid.SelectedItem is PluginItem item)
            {
                var exe = item.Command.Execute();
                Trace.WriteLine(exe);
            }
        }

        /// <summary>
        ///     Handles the Loaded event of the PluginController control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void PluginController_Loaded(object sender, RoutedEventArgs e)
        {
            var directory = Directory.GetCurrentDirectory();
            var path = Path.Combine(directory, TargetPath);

            if (!Directory.Exists(path))
            {
                Trace.WriteLine(PluginLoaderResources.ErrorPath);
                return;
            }

            bool check;

            if (!string.IsNullOrEmpty(Extension))
            {
                check = PluginLoad.LoadAll(path, Extension);
            }
            else
            {
                check = PluginLoad.LoadAll(path);
            }

            if (!check || PluginLoad.PluginContainer == null || PluginLoad.PluginContainer.Count == 0)
            {
                Trace.WriteLine(PluginLoaderResources.InformationPlugin);
                return;
            }

            var lst = new ObservableCollection<PluginItem>();

            foreach (var plugin in PluginLoad.PluginContainer)
            {
                lst.Add(new PluginItem
                {
                    Command = plugin,
                    Name = plugin.Name,
                    Version = plugin.Version,
                    Type = plugin.Type,
                    Description = plugin.Description
                });
            }

            ObservablePlugin = lst;
            NotifyPropertyChanged(nameof(ObservablePlugin));
        }
    }
}
