using System.Collections.ObjectModel;
using System.Windows;
using CoreBuilder;
using CoreBuilder.Interface;

namespace CoreViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ICodeAnalyzer> _analyzers;
        private List<Diagnostic> _currentDiagnostics = new();

        private readonly ObservableCollection<Diagnostic> _diagnosticsView = new();


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadAnalyzers();
        }

        private void LoadAnalyzers()
        {
            // Instantiate all analyzers here
            _analyzers =
            [
                new DoubleNewlineAnalyzer(),
                new LicenseHeaderAnalyzer(),
                new UnusedLocalVariableAnalyzer(),
                new UnusedParameterAnalyzer(),
                new UnusedPrivateFieldAnalyzer(),
                new HotPathAnalyzer(),
                new AllocationAnalyzer(),
                new DisposableAnalyzer(),
                new EventHandlerAnalyzer(),
            ];

            AnalyzerSelector.ItemsSource = _analyzers;
        }

        private void RunAnalyzer_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<ICodeAnalyzer> analyzersToRun = AnalyzerSelector.SelectedItem is ICodeAnalyzer selected
                ? new[] { selected }
                : _analyzers;

            _currentDiagnostics.Clear();

            var files = System.IO.Directory.GetFiles(@"C:\Repos\Source\ServiceCraneScale", "*.cs", System.IO.SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var content = System.IO.File.ReadAllText(file);
                foreach (var analyzer in analyzersToRun)
                {
                    _currentDiagnostics.AddRange(analyzer.Analyze(file, content));
                }
            }

            ApplyFilter();
        }

        private void FilterBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyFilter();
        }
        private void ApplyFilter()
        {
            var filterText = FilterBox?.Text?.ToLower() ?? "";

            _diagnosticsView.Clear();

            foreach (var d in string.IsNullOrWhiteSpace(filterText)
                ? _currentDiagnostics
                : _currentDiagnostics.Where(d =>
                    d.FilePath.ToLower().Contains(filterText) ||
                    d.Message.ToLower().Contains(filterText) ||
                    d.Name.ToLower().Contains(filterText)))
            {
                _diagnosticsView.Add(d);
            }
        }

    }
}
