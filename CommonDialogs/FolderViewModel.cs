﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ViewModel;

namespace CommonDialogs
{
    /// <summary>
    /// ViewModel for the <see cref="FolderControl"/> UserControl.
    /// Handles folder navigation, file/folder loading, and command bindings for UI interaction.
    /// Implements async loading to keep the UI responsive.
    /// </summary>
    public sealed class FolderViewModel : ViewModelBase
    {
        /// <summary>
        /// Collection of folders and files displayed in the TreeView.
        /// Each item is a <see cref="FolderItemViewModel"/>.
        /// </summary>
        public ObservableCollection<FolderItemViewModel> FolderItems { get; } = new();

        private FolderItemViewModel? _selectedFolder;

        /// <summary>
        /// Gets or sets the selected folder.
        /// </summary>
        /// <value>
        /// The selected folder.
        /// </value>
        public FolderItemViewModel? SelectedFolder
        {
            get => _selectedFolder;
            set => SetProperty(ref _selectedFolder, value); // SetProperty from your ViewModelBase
        }

        private string? _paths;

        /// <summary>
        /// Currently selected folder path.
        /// Updates whenever navigation occurs or user selects a folder.
        /// </summary>
        public string? Paths
        {
            get => _paths;
            set => SetProperty(ref _paths, value); // ViewModelBase provides INotifyPropertyChanged
        }

        private string _lookUp = string.Empty;

        /// <summary>
        /// User input for navigating to a specific folder.
        /// Bound to the LookUp TextBox in the UI.
        /// </summary>
        public string LookUp
        {
            get => _lookUp;
            set => SetProperty(ref _lookUp, value);
        }

        private bool _showFiles;

        /// <summary>
        /// Determines whether files should be displayed in addition to folders.
        /// Bound to a ShowFiles toggle in the UI if needed.
        /// </summary>
        public bool ShowFiles
        {
            get => _showFiles;
            set => SetProperty(ref _showFiles, value);
        }

        #region Commands

        public RelayCommand UpCommand { get; }
        public RelayCommand GoCommand { get; }
        public RelayCommand ExplorerCommand { get; }
        public RelayCommand DesktopCommand { get; }
        public RelayCommand RootCommand { get; }
        public RelayCommand DocsCommand { get; }
        public RelayCommand PersonalCommand { get; }
        public RelayCommand PicturesCommand { get; }
        public RelayCommand CreateFolderCommand { get; }

        #endregion

        /// <summary>
        /// Default constructor.
        /// Initializes all commands and binds them to appropriate actions.
        /// </summary>
        public FolderViewModel()
        {
            UpCommand = new RelayCommand(async () =>
            {
                var parent = Directory.GetParent(Paths ?? string.Empty)?.FullName;
                if (!string.IsNullOrEmpty(parent))
                    await LoadRootAsync(parent);
            });

            GoCommand = new RelayCommand(async () =>
            {
                if (!string.IsNullOrEmpty(LookUp) && Directory.Exists(LookUp))
                    await LoadRootAsync(LookUp);

                LookUp = string.Empty;
            });

            ExplorerCommand = new RelayCommand(() =>
            {
                if (!string.IsNullOrEmpty(Paths) && Directory.Exists(Paths))
                    _ = Process.Start(new ProcessStartInfo { FileName = Paths, UseShellExecute = true });
            });

            DesktopCommand = new RelayCommand(() =>
                _ = LoadRootAsync(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)));

            RootCommand = new RelayCommand(() => _ = LoadRootAsync(@"C:\"));

            DocsCommand = new RelayCommand(() =>
                _ = LoadRootAsync(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));

            PersonalCommand = new RelayCommand(() =>
                _ = LoadRootAsync(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));

            PicturesCommand = new RelayCommand(() =>
                _ = LoadRootAsync(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)));

            CreateFolderCommand = new RelayCommand(async () =>
            {
                if (string.IsNullOrEmpty(Paths)) return;

                var newDirPath = Path.Combine(Paths, "New Folder");
                var dirName = newDirPath;
                int i = 1;

                while (Directory.Exists(dirName))
                    dirName = $"{newDirPath} ({i++})";

                Directory.CreateDirectory(dirName);

                // Reload current folder to show the new folder
                await LoadRootAsync(Paths);
            });
        }

        /// <summary>
        /// Initializes the ViewModel with a starting folder.
        /// Called by the view when the control is first displayed.
        /// </summary>
        /// <param name="startFolder">Starting folder path.</param>
        public void Initiate(string startFolder)
        {
            if (!string.IsNullOrEmpty(startFolder) && Directory.Exists(startFolder))
            {
                _ = LoadRootAsync(startFolder); // Fire-and-forget async initialization
            }
            else
            {
                _ = LoadRootAsync(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            }
        }

        /// <summary>
        /// Loads folders and files from a given path asynchronously and populates the <see cref="FolderItems"/> collection.
        /// Uses the Dispatcher to update UI-bound collections on the UI thread.
        /// </summary>
        /// <param name="path">Target folder path.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LoadRootAsync(string path)
        {
            Paths = path;

            string[] directories =
                Directory.Exists(path) ? Directory.GetDirectories(path) : Directory.GetLogicalDrives();
            string[] files = ShowFiles && Directory.Exists(path) ? Directory.GetFiles(path) : Array.Empty<string>();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                FolderItems.Clear();

                // Add directories first
                foreach (var dir in directories)
                    FolderItems.Add(new FolderItemViewModel(dir));

                // Add files if enabled
                if (ShowFiles)
                {
                    foreach (var file in files)
                        FolderItems.Add(new FolderItemViewModel(file) { Header = Path.GetFileName(file) ?? file });
                }
            });
        }
    }
}
