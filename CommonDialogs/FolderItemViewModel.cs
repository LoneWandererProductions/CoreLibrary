/* 
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonDialogs
 * FILE:        FolderItemViewModel.cs
 * PURPOSE:     ViewModel representing a single folder or file in a TreeView
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CommonDialogs
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a single folder or file in a TreeView. Supports lazy loading of children and selection tracking.
    /// </summary>
    public sealed class FolderItemViewModel : INotifyPropertyChanged
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
                    _parentVM.SelectedFolder = this; // ← direct, safe
            }
        }

        /// <summary>
        /// Raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// The parent vm
        /// </summary>
        private readonly FolderViewModel _parentVM;

        /// <summary>
        /// Initializes a new instance of <see cref="FolderItemViewModel" /> for a given path.
        /// </summary>
        /// <param name="path">Full path of the folder or file.</param>
        /// <param name="parentVM">The parent vm.</param>
        public FolderItemViewModel(string path, FolderViewModel parentVM)
        {
            Path = path;
            Header = System.IO.Path.GetFileName(path) ?? path;
            _parentVM = parentVM;
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
        /// <returns>If folder has child</returns>
        private static bool SafeHasChildren(string path)
        {
            try
            {
                return Directory.EnumerateFileSystemEntries(path).Any();
            }
            catch
            {
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
                var dirs = Directory.GetDirectories(Path);
                var files = Directory.GetFiles(Path);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (var dir in dirs)
                        Children.Add(new FolderItemViewModel(dir, _parentVM));

                    foreach (var file in files)
                        Children.Add(
                            new FolderItemViewModel(file, _parentVM) { Header = System.IO.Path.GetFileName(file) });
                });
            }
            catch
            {
                // Access denied or IO exceptions are silently ignored
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for a property.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
