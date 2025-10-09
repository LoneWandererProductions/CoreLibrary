using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using CoreBuilder;
using CoreBuilder.Interface;
using CommonDialogs;
using ViewModel;

namespace CoreViewer;

/// <summary>
/// ViewModel for the Analyzer Viewer.
/// Handles loading analyzers, running them, filtering, and selecting folders.
/// </summary>
public sealed class AnalyzerViewModel : ViewModelBase
{
    private string _filterText = string.Empty;
    private string _targetDirectory = @"C:\Repos\Source\ServiceCraneScale";
    private ICodeAnalyzer? _selectedAnalyzer;

    private readonly List<ICodeAnalyzer> _analyzers = new();
    private readonly List<Diagnostic> _currentDiagnostics = new();

    public AnalyzerViewModel()
    {
        LoadAnalyzers();
        RunAnalyzerCommand = new RelayCommand(RunAnalyzer);
        SelectFolderCommand = new RelayCommand(SelectFolder);
    }

    /// <summary>
    /// Available analyzers.
    /// </summary>
    public IReadOnlyList<ICodeAnalyzer> Analyzers => _analyzers;

    /// <summary>
    /// The analyzer selected by the user.
    /// </summary>
    public ICodeAnalyzer? SelectedAnalyzer
    {
        get => _selectedAnalyzer;
        set => SetProperty(ref _selectedAnalyzer, value);
    }

    /// <summary>
    /// Diagnostics displayed in the UI.
    /// </summary>
    public ObservableCollection<Diagnostic> DiagnosticsView { get; } = new();

    /// <summary>
    /// Text filter for diagnostics.
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
    /// The directory where source files are analyzed.
    /// </summary>
    public string TargetDirectory
    {
        get => _targetDirectory;
        set => SetProperty(ref _targetDirectory, value);
    }

    public ICommand RunAnalyzerCommand { get; }
    public ICommand SelectFolderCommand { get; }

    private void LoadAnalyzers()
    {
        _analyzers.AddRange(new ICodeAnalyzer[]
        {
            new DoubleNewlineAnalyzer(),
            new LicenseHeaderAnalyzer(),
            new UnusedLocalVariableAnalyzer(),
            new UnusedParameterAnalyzer(),
            new UnusedPrivateFieldAnalyzer(),
            new HotPathAnalyzer(),
            new AllocationAnalyzer(),
            new DisposableAnalyzer(),
            new EventHandlerAnalyzer(),
        });
    }

    private void RunAnalyzer()
    {
        if (!Directory.Exists(TargetDirectory))
            return;

        IEnumerable<ICodeAnalyzer> analyzersToRun = SelectedAnalyzer is not null
            ? new[] { SelectedAnalyzer }
            : _analyzers;

        _currentDiagnostics.Clear();

        var files = Directory.GetFiles(TargetDirectory, "*.cs", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            foreach (var analyzer in analyzersToRun)
            {
                _currentDiagnostics.AddRange(analyzer.Analyze(file, content));
            }
        }

        ApplyFilter();
    }

    private void SelectFolder()
    {
        var selected = DialogHandler.ShowFolder(TargetDirectory);
        if (!string.IsNullOrWhiteSpace(selected))
        {
            TargetDirectory = selected;
        }
    }

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
            DiagnosticsView.Add(d);
        }
    }
}
