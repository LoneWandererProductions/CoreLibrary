/* 
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Dialogs
 * FILE:        FolderViewModel.cs
 * PURPOSE:     ViewModel for FolderControl UserControl, handles folder navigation and loading
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ViewModel;

namespace Common.Dialogs
{
    /// <inheritdoc cref="ViewModelBase" />
    /// <summary>
    /// ViewModel for the <see cref="T:Common.Dialogs.FolderControl" /> UserControl.
    /// Handles folder navigation, file/folder loading, and command bindings for UI interaction.
    /// Implements async loading to keep the UI responsive.
    /// </summary>
    public sealed class FolderViewModel : ViewModelBase
    {
        /// <summary>
        /// Back-navigation history (most recently visited folder on top). Populated by
        /// <see cref="LoadRootAsync" /> whenever navigation moves to a genuinely different folder,
        /// so Back/Forward behave the same standard way as Explorer or a browser: going Up counts
        /// as a navigation too, and Back simply undoes it.
        /// </summary>
        private readonly Stack<string> _backStack = new();

        /// <summary>
        /// Forward-navigation history, populated only by <see cref="BackCommand" /> and cleared
        /// whenever a fresh (non-history) navigation happens.
        /// </summary>
        private readonly Stack<string> _forwardStack = new();

        /// <summary>
        /// Set while <see cref="BackCommand" />/<see cref="ForwardCommand" /> are driving a
        /// navigation, so <see cref="LoadRootAsync" /> knows not to push onto the history stacks
        /// for a move that came from history in the first place.
        /// </summary>
        private bool _isNavigatingHistory;

        /// <summary>
        /// The start folder
        /// </summary>
        private string? _startFolder;

        /// <summary>
        /// Collection of folders and files displayed in the TreeView.
        /// Each item is a <see cref="FolderItemViewModel"/>.
        /// </summary>
        public ObservableCollection<FolderItemViewModel> FolderItems { get; } = new();

        /// <summary>
        /// Gets or sets the start folder.
        /// </summary>
        /// <value>
        /// The start folder.
        /// </value>
        public string? StartFolder
        {
            get => _startFolder;
            set
            {
                if (_startFolder == value) return;

                _startFolder = value;
                OnPropertyChanged(nameof(StartFolder));

                if (!string.IsNullOrEmpty(value) && Directory.Exists(value))
                    _ = LoadRootAsync(value);
            }
        }

        /// <summary>
        /// The selected folder
        /// </summary>
        private FolderItemViewModel? _selectedFolder;

        /// <summary>
        /// The paths
        /// </summary>
        private string? _paths;

        /// <summary>
        /// The look up
        /// </summary>
        private string _lookUp = string.Empty;

        /// <summary>
        /// The show files
        /// </summary>
        private bool _showFiles;

        /// <summary>
        /// Gets or sets the selected folder.
        /// </summary>
        /// <value>
        /// The selected folder.
        /// </value>
        public FolderItemViewModel? SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (SetProperty(ref _selectedFolder, value))
                {
                    if (value != null && Directory.Exists(value.Path))
                    {
                        Paths = value.Path;
                    }
                }
            }
        }

        /// <summary>
        /// Currently selected folder path.
        /// Updates whenever navigation occurs or user selects a folder.
        /// </summary>
        public string Paths
        {
            get => _paths;
            set
            {
                if (_paths == value) return;

                _paths = value;
                OnPropertyChanged(nameof(Paths));

                LookUp = value;
            }
        }

        /// <summary>
        /// User input for navigating to a specific folder.
        /// Bound to the LookUp TextBox in the UI.
        /// </summary>
        public string LookUp
        {
            get => _lookUp;
            set
            {
                if (_lookUp == value) return;

                _lookUp = value;
                OnPropertyChanged(nameof(LookUp));
            }
        }

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

        /// <summary>
        /// Gets up command.
        /// </summary>
        /// <value>
        /// Up command.
        /// </value>
        public RelayCommand UpCommand { get; }

        /// <summary>
        /// Gets the command that navigates back to the previously visited folder.
        /// </summary>
        /// <value>
        /// The back command.
        /// </value>
        public RelayCommand BackCommand { get; }

        /// <summary>
        /// Gets the command that re-navigates forward after a Back, mirroring standard
        /// browser/Explorer navigation semantics.
        /// </summary>
        /// <value>
        /// The forward command.
        /// </value>
        public RelayCommand ForwardCommand { get; }

        /// <summary>
        /// Gets the go command.
        /// </summary>
        /// <value>
        /// The go command.
        /// </value>
        public RelayCommand GoCommand { get; }

        /// <summary>
        /// Gets the explorer command.
        /// </summary>
        /// <value>
        /// The explorer command.
        /// </value>
        public RelayCommand ExplorerCommand { get; }

        /// <summary>
        /// Gets the desktop command.
        /// </summary>
        /// <value>
        /// The desktop command.
        /// </value>
        public RelayCommand DesktopCommand { get; }

        /// <summary>
        /// Gets the root command.
        /// </summary>
        /// <value>
        /// The root command.
        /// </value>
        public RelayCommand RootCommand { get; }

        /// <summary>
        /// Gets the docs command.
        /// </summary>
        /// <value>
        /// The docs command.
        /// </value>
        public RelayCommand DocsCommand { get; }

        /// <summary>
        /// Gets the personal command.
        /// </summary>
        /// <value>
        /// The personal command.
        /// </value>
        public RelayCommand PersonalCommand { get; }

        /// <summary>
        /// Gets the pictures command.
        /// </summary>
        /// <value>
        /// The pictures command.
        /// </value>
        public RelayCommand PicturesCommand { get; }

        /// <summary>
        /// Gets the create folder command.
        /// </summary>
        /// <value>
        /// The create folder command.
        /// </value>
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
                var parent = SafeGetParent(Paths);

                if (!string.IsNullOrEmpty(parent))
                    await LoadRootAsync(parent);
            }, () => !string.IsNullOrEmpty(SafeGetParent(Paths)));

            BackCommand = new RelayCommand(async () =>
            {
                if (_backStack.Count == 0) return;

                var target = _backStack.Pop();
                if (!string.IsNullOrEmpty(Paths)) _forwardStack.Push(Paths);

                _isNavigatingHistory = true;
                await LoadRootAsync(target);
                _isNavigatingHistory = false;
            }, () => _backStack.Count > 0);

            ForwardCommand = new RelayCommand(async () =>
            {
                if (_forwardStack.Count == 0) return;

                var target = _forwardStack.Pop();
                if (!string.IsNullOrEmpty(Paths)) _backStack.Push(Paths);

                _isNavigatingHistory = true;
                await LoadRootAsync(target);
                _isNavigatingHistory = false;
            }, () => _forwardStack.Count > 0);

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

            RootCommand = new RelayCommand(() => _ = LoadRootAsync(ComDlgResources.Root));

            DocsCommand = new RelayCommand(() =>
                _ = LoadRootAsync(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));

            PersonalCommand = new RelayCommand(() =>
                _ = LoadRootAsync(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));

            PicturesCommand = new RelayCommand(() =>
                _ = LoadRootAsync(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)));

            CreateFolderCommand = new RelayCommand(async () =>
            {
                if (string.IsNullOrEmpty(Paths)) return;

                var newDirPath = Path.Combine(Paths, ComDlgResources.NewFolder);
                var dirName = newDirPath;
                var i = 1;

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
            // Record history for a genuine navigation (not one already driven by Back/Forward,
            // and not a no-op re-navigation to the folder we're already on).
            if (!_isNavigatingHistory && !string.IsNullOrEmpty(Paths) &&
                !string.Equals(Paths, path, StringComparison.OrdinalIgnoreCase))
            {
                _backStack.Push(Paths);
                _forwardStack.Clear();
            }

            Paths = path;

            var directories =
                Directory.Exists(path) ? Directory.GetDirectories(path) : Directory.GetLogicalDrives();
            var files = ShowFiles && Directory.Exists(path) ? Directory.GetFiles(path) : Array.Empty<string>();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                FolderItems.Clear();

                // Add directories first
                foreach (var dir in directories)
                    FolderItems.Add(new FolderItemViewModel(dir, this));

                // Add files if enabled
                if (ShowFiles)
                {
                    foreach (var file in files)
                        FolderItems.Add(
                            new FolderItemViewModel(file, this) { Header = Path.GetFileName(file) });
                }

                // Buttons don't automatically know the back/forward/up stacks just changed -
                // nudge WPF to re-check CanExecute now rather than waiting for the next
                // incidental UI event to trigger CommandManager's automatic requery.
                UpCommand.RaiseCanExecuteChanged();
                BackCommand.RaiseCanExecuteChanged();
                ForwardCommand.RaiseCanExecuteChanged();
            });
        }

        /// <summary>
        /// Safes the get parent.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>Safe parent Path.</returns>
        private string? SafeGetParent(string? path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return null;

                // Normalize path (removes trailing slashes)
                var full = Path.GetFullPath(path);

                // Detect root (C:\, D:\, etc.)
                var root = Path.GetPathRoot(full);
                if (root != null &&
                    root.Equals(full, StringComparison.OrdinalIgnoreCase))
                {
                    return null; // No parent above root
                }

                // Normal parent lookup
                return Directory.GetParent(full)?.FullName;
            }
            catch
            {
                return null; // On ANY error, return null
            }
        }
    }
}