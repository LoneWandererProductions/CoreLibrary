/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CoreViewer
 * FILE:        CoreViewer/AnalyzerViewModel.cs
 * PURPOSE:     ViewModel for Analyzer Viewer, handles loading and running analyzers, filtering diagnostics, and folder selection
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using CoreBuilder;
using CoreBuilder.Interface;
using CommonDialogs;
using ViewModel;
using CoreBuilder.Rules;

namespace CoreViewer
{
    /// <summary>
    /// ViewModel for the Analyzer Viewer.
    /// Handles loading analyzers, running them, filtering results, and selecting folders.
    /// </summary>
    public sealed class AnalyzerViewModel : ViewModelBase
    {
        /// <summary>
        /// The filter text
        /// </summary>
        private string _filterText = string.Empty;

        /// <summary>
        /// The target directory
        /// </summary>
        private string _targetDirectory;

        /// <summary>
        /// The selected analyzer
        /// </summary>
        private ICodeAnalyzer? _selectedAnalyzer;

        /// <summary>
        /// The analyzers
        /// </summary>
        private IReadOnlyList<ICodeAnalyzer> _analyzers;

        /// <summary>
        /// The current diagnostics
        /// </summary>
        private readonly List<Diagnostic> _currentDiagnostics = new();

        /// <summary>
        /// The fixable analyzers
        /// </summary>
        private static readonly HashSet<string> FixableAnalyzers = new() { nameof(LicenseHeaderAnalyzer), "..." };

        /// <summary>
        /// Initializes a new instance of <see cref="AnalyzerViewModel"/> and sets up commands.
        /// </summary>
        public AnalyzerViewModel()
        {
            LoadAnalyzers();

            // Use AsyncDelegateCommand instead of RelayCommand
            RunAnalyzerCommand = new AsyncDelegateCommand<object>(_ => RunAnalyzerAsync());
            SelectFolderCommand = new AsyncDelegateCommand<object>(_ => SelectFolderAsync());
        }

        /// <summary>
        /// Read-only list of all available analyzers.
        /// </summary>
        public IReadOnlyList<ICodeAnalyzer> Analyzers => _analyzers;

        /// <summary>
        /// The analyzer currently selected by the user.
        /// </summary>
        public ICodeAnalyzer? SelectedAnalyzer
        {
            get => _selectedAnalyzer;
            set => SetProperty(ref _selectedAnalyzer, value);
        }

        /// <summary>
        /// Diagnostics to be displayed in the UI, filtered as needed.
        /// </summary>
        public ObservableCollection<DiagnosticItemViewModel> DiagnosticsView { get; } = new();

        /// <summary>
        /// Filter text to narrow down displayed diagnostics.
        /// Setting this property automatically updates the filtered view.
        /// </summary>
        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value))
                    ApplyFilter();
            }
        }

        /// <summary>
        /// The directory to analyze. Updating this does not automatically run analyzers.
        /// </summary>
        public string TargetDirectory
        {
            get => _targetDirectory;
            set => SetProperty(ref _targetDirectory, value);
        }

        /// <summary>
        /// Command to run analyzers on the current target directory.
        /// </summary>
        public ICommand RunAnalyzerCommand { get; }

        /// <summary>
        /// Command to select a new target folder.
        /// Automatically updates <see cref="TargetDirectory"/> and runs analyzers.
        /// </summary>
        public ICommand SelectFolderCommand { get; }

        /// <summary>
        /// Loads all available analyzers into the <see cref="_analyzers"/> list.
        /// </summary>
        private void LoadAnalyzers()
        {
            _analyzers = AnalyzerFactory.GetAllAnalyzers();
        }

        /// <summary>
        /// Runs analyzers on all C# files in the target directory.
        /// If <see cref="SelectedAnalyzer"/> is set, only that analyzer is run.
        /// Results are stored in <see cref="DiagnosticsView"/> and filtered according to <see cref="FilterText"/>.
        /// </summary>
        private async Task RunAnalyzerAsync()
        {
            if (!Directory.Exists(TargetDirectory))
                return;

            IEnumerable<ICodeAnalyzer> analyzersToRun = SelectedAnalyzer is not null
                ? new[] { SelectedAnalyzer }
                : _analyzers;

            _currentDiagnostics.Clear();

            // Run file enumeration and analysis on a background thread
            var files = await Task.Run(() => SafeEnumerateFiles(TargetDirectory, CoreResources.ResourceCsExtension));

            foreach (var file in files)
            {
                // Read file and analyze in background thread
                var content = await Task.Run(() => File.ReadAllText(file));

                foreach (var analyzer in analyzersToRun)
                {
                    var diagnostics = await Task.Run(() => analyzer.Analyze(file, content));
                    _currentDiagnostics.AddRange(diagnostics);
                }
            }

            ApplyFilter(); // This can stay on UI thread
        }


        /// <summary>
        /// Safes the enumerate files.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns>Allowed Folders.</returns>
        private static IEnumerable<string> SafeEnumerateFiles(string root, string pattern)
        {
            var stack = new Stack<string>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                var files = Array.Empty<string>();
                try
                {
                    files = Directory.GetFiles(current, pattern);
                }
                catch (Exception exe)
                {
                    Trace.WriteLine($"Error accessing files in {current}: {exe.Message}");
                }

                foreach (var file in files)
                    yield return file;

                var dirs = Array.Empty<string>();
                try
                {
                    dirs = Directory.GetDirectories(current);
                }
                catch (Exception exe)
                {
                    Trace.WriteLine($"Error accessing directories in {current}: {exe.Message}");
                }

                foreach (var dir in dirs)
                    stack.Push(dir);
            }
        }

        /// <summary>
        /// Opens a folder selection dialog and updates <see cref="TargetDirectory"/>.
        /// Automatically runs analyzers on the selected folder.
        /// </summary>
        private async Task SelectFolderAsync()
        {
            var selected = DialogHandler.ShowFolder(TargetDirectory);
            if (!string.IsNullOrWhiteSpace(selected))
            {
                TargetDirectory = selected;
                await RunAnalyzerAsync();
            }
        }

        /// <summary>
        /// Applies the current <see cref="FilterText"/> to the <see cref="_currentDiagnostics"/>
        /// and updates <see cref="DiagnosticsView"/> accordingly.
        /// </summary>
        private void ApplyFilter()
        {
            DiagnosticsView.Clear();

            var filtered = string.IsNullOrWhiteSpace(FilterText)
                ? _currentDiagnostics
                : _currentDiagnostics.Where(d =>
                    d.FilePath.ToLower().Contains(FilterText.ToLower()) ||
                    d.Message.ToLower().Contains(FilterText.ToLower()) ||
                    d.Name.ToLower().Contains(FilterText.ToLower()));

            foreach (var d in filtered)
            {
                DiagnosticsView.Add(new DiagnosticItemViewModel(d,
                    openFile: _ => HandleOpen(d.Name),
                    ignore: _ => HandleIgnore(d.Name),
                    fix: FixableAnalyzers.Contains(d.Name) ? _ => HandleFix(d.Name) : null));
            }
        }

        /// <summary>
        /// Handles the ignore.
        /// </summary>
        /// <param name="name">The name.</param>
        private void HandleIgnore(string name)
        {
            // Minimal implementation: just remove all diagnostics with this name from the view
            var toRemove = DiagnosticsView.Where(d => d.Diagnostic.Name == name).ToList();
            foreach (var d in toRemove)
            {
                DiagnosticsView.Remove(d);
            }
        }

        /// <summary>
        /// Handles the open.
        /// </summary>
        /// <param name="name">The name.</param>
        private void HandleOpen(string name)
        {
            // Minimal implementation: just show a message box with the file paths
            var files = DiagnosticsView
                .Where(d => d.Diagnostic.Name == name)
                .Select(d => d.Diagnostic.FilePath)
                .Distinct();

            var message = string.Join(Environment.NewLine, files);
            System.Windows.MessageBox.Show(message, $"Open files for {name}");
        }

        /// <summary>
        /// Handles the fix.
        /// </summary>
        /// <param name="name">The name.</param>
        private void HandleFix(string name)
        {
            // Minimal placeholder
            System.Windows.MessageBox.Show($"Fix logic for {name} not implemented yet.", "Fix");
        }
    }
}