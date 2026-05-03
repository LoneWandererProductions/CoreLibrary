/* 
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Dialogs
 * FILE:        FolderItemViewModel.cs
 * PURPOSE:     ViewModel representing a single folder or file in a TreeView
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ViewModel;

namespace Common.Dialogs
{
    /// <inheritdoc cref="ViewModelBase" />
    /// <summary>
    /// Represents a single folder or file in a TreeView. Supports lazy loading of children and selection tracking.
    /// </summary>
    public sealed class FolderItemViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets the full path of this folder or file.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets or sets the display name of the folder or file.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Gets the child folders and files. Populated on demand when expanded.
        /// </summary>
        public ObservableCollection<FolderItemViewModel> Children { get; } = new();

        /// <summary>
        /// The is expanded
        /// </summary>
        private bool _isExpanded;

        /// <summary>
        /// True if this folder has child folders. Determined at construction.
        /// </summary>
        public bool HasChildren { get; private set; }

        /// <summary>
        /// The is selected
        /// </summary>
        private bool _isSelected;

        /// <summary>
        /// True if this folder or file is selected in the TreeView.
        /// Setting this property raises <see cref="PropertyChanged"/>.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;

                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));

                if (_isSelected)
                    _parentVm.SelectedFolder = this; // ← direct, safe
            }
        }

        /// <summary>
        /// Raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// The parent vm
        /// </summary>
        private readonly FolderViewModel _parentVm;

        /// <summary>
        /// Initializes a new instance of <see cref="FolderItemViewModel" /> for a given path.
        /// </summary>
        /// <param name="path">Full path of the folder or file.</param>
        /// <param name="parentVm">The parent vm.</param>
        public FolderItemViewModel(string path, FolderViewModel parentVm)
        {
            Path = path;
            Header = System.IO.Path.GetFileName(path);
            _parentVm = parentVm;
            HasChildren = SafeHasChildren(path);
        }

        /// <summary>
        /// True if this folder is expanded in the TreeView.
        /// Expanding triggers lazy loading of children.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value) return;

                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));

                if (_isExpanded && HasChildren && Children.Count == 0)
                {
                    _ = LoadChildrenAsync(); // Load actual children when expanded
                }
            }
        }

        /// <summary>
        /// Safes the has children.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// If folder has child
        /// </returns>
        private static bool SafeHasChildren(string path)
        {
            try
            {
                // PREVENT the IOException by checking if it's actually a directory first.
                // If it's a file (or doesn't exist/access denied), Directory.Exists cleanly returns false.
                if (!Directory.Exists(path))
                {
                    return false;
                }

                using var enumerator = Directory.EnumerateFileSystemEntries(path).GetEnumerator();
                return enumerator.MoveNext();
            }
            catch (UnauthorizedAccessException)
            {
                // Handle permission errors silently
                return false;
            }
            catch (IOException)
            {
                // Handle locked system files, device errors, or edge-case invalid paths silently
                return false;
            }
            catch
            {
                // Fallback for PathTooLongException, DirectoryNotFoundException, etc.
                return false;
            }
        }

        /// <summary>
        /// Loads child directories and files asynchronously and adds them to <see cref="Children" />.
        /// </summary>
        private async Task LoadChildrenAsync()
        {
            try
            {
                var dirs = Array.Empty<string>();
                var files = Array.Empty<string>();

                await Task.Run(() =>
                {
                    if (Directory.Exists(Path))
                    {
                        dirs = Directory.GetDirectories(Path);
                        files = Directory.GetFiles(Path);
                    }
                });

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var dir in dirs)
                        Children.Add(new FolderItemViewModel(dir, _parentVm));

                    if (_parentVm.ShowFiles)
                    {
                        foreach (var file in files)
                            Children.Add(new FolderItemViewModel(file, _parentVm)
                            {
                                Header = System.IO.Path.GetFileName(file)
                            });
                    }
                });
            }
            catch
            {
                // ignore
            }
        }
    }
}
