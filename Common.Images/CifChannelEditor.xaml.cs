/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        CifChannelEditor.xaml.cs
 * PURPOSE:     A Color Channel Editor based on my Cif Image format
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedType.Global
// ReSharper disable MemberCanBePrivate.Global

using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Common.Dialogs;
using Imaging.Cifs;
using System;
using SystemDrawingColor = System.Drawing.Color;

namespace Common.Images
{
    /// <summary>
    /// Custom Control for Color Channel manipulation.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class CifChannelEditor : UserControl
    {
        /// <summary>
        /// The writeable bitmap
        /// </summary>
        private WriteableBitmap? _writeableBitmap;

        /// <summary>
        /// The render token source
        /// </summary>
        private CancellationTokenSource? _renderTokenSource;

        /// <summary>
        /// The cif filter
        /// </summary>
        private const string CifFilter = "CIF Files (*.cif)|*.cif";

        /// <summary>
        /// The custom format
        /// </summary>
        private readonly CustomImageFormat _customFormat = new();

        /// <summary>
        /// The collection of unique colors found in the loaded CIF image.
        /// </summary>
        public ObservableCollection<CifColorItem> PaletteItems { get; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CifChannelEditor"/> class.
        /// </summary>
        public CifChannelEditor()
        {
            InitializeComponent();
            Name = "Root"; // Required for XAML ElementName bindings
        }

        #region Dependency Properties

        /// <summary>
        /// The cif source property
        /// </summary>
        public static readonly DependencyProperty CifSourceProperty = DependencyProperty.Register(
            nameof(CifSource), typeof(Cif), typeof(CifChannelEditor),
            new PropertyMetadata(null, OnCifSourceChanged));

        /// <summary>
        /// The red offset property
        /// </summary>
        public static readonly DependencyProperty RedOffsetProperty = DependencyProperty.Register(
            nameof(RedOffset), typeof(int), typeof(CifChannelEditor),
            new PropertyMetadata(0, OnChannelOffsetChanged));

        /// <summary>
        /// The green offset property
        /// </summary>
        public static readonly DependencyProperty GreenOffsetProperty = DependencyProperty.Register(
            nameof(GreenOffset), typeof(int), typeof(CifChannelEditor),
            new PropertyMetadata(0, OnChannelOffsetChanged));

        /// <summary>
        /// The blue offset property
        /// </summary>
        public static readonly DependencyProperty BlueOffsetProperty = DependencyProperty.Register(
            nameof(BlueOffset), typeof(int), typeof(CifChannelEditor),
            new PropertyMetadata(0, OnChannelOffsetChanged));

        /// <summary>
        /// The isolation enabled property
        /// </summary>
        public static readonly DependencyProperty IsIsolationEnabledProperty = DependencyProperty.Register(
            nameof(IsIsolationEnabled), typeof(bool), typeof(CifChannelEditor),
            new PropertyMetadata(false, OnRenderStateChanged));

        /// <summary>
        /// The selected palette item property
        /// </summary>
        public static readonly DependencyProperty SelectedPaletteItemProperty = DependencyProperty.Register(
            nameof(SelectedPaletteItem), typeof(CifColorItem), typeof(CifChannelEditor),
            new PropertyMetadata(null, OnRenderStateChanged));

        /// <summary>
        /// Gets or sets the cif source.
        /// </summary>
        /// <value>
        /// The cif source.
        /// </value>
        public Cif? CifSource
        {
            get => (Cif?)GetValue(CifSourceProperty);
            set => SetValue(CifSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the red offset.
        /// </summary>
        /// <value>
        /// The red offset.
        /// </value>
        public int RedOffset
        {
            get => (int)GetValue(RedOffsetProperty);
            set => SetValue(RedOffsetProperty, value);
        }

        /// <summary>
        /// Gets or sets the green offset.
        /// </summary>
        /// <value>
        /// The green offset.
        /// </value>
        public int GreenOffset
        {
            get => (int)GetValue(GreenOffsetProperty);
            set => SetValue(GreenOffsetProperty, value);
        }

        /// <summary>
        /// Gets or sets the blue offset.
        /// </summary>
        /// <value>
        /// The blue offset.
        /// </value>
        public int BlueOffset
        {
            get => (int)GetValue(BlueOffsetProperty);
            set => SetValue(BlueOffsetProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether color isolation is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if isolation is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsIsolationEnabled
        {
            get => (bool)GetValue(IsIsolationEnabledProperty);
            set => SetValue(IsIsolationEnabledProperty, value);
        }

        /// <summary>
        /// Gets or sets the currently selected color item for isolation mapping.
        /// </summary>
        /// <value>
        /// The selected palette item.
        /// </value>
        public CifColorItem? SelectedPaletteItem
        {
            get => (CifColorItem?)GetValue(SelectedPaletteItemProperty);
            set => SetValue(SelectedPaletteItemProperty, value);
        }


        /// <summary>
        /// Resets the palette and channel offsets. Can be called from XAML button clicks.
        /// </summary>
        public void ResetPalette()
        {
            RedOffset = 0;
            GreenOffset = 0;
            BlueOffset = 0;
            IsIsolationEnabled = false;
            SelectedPaletteItem = null;
        }

        #endregion

        /// <summary>
        /// Called when [cif source changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnCifSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CifChannelEditor editor && e.NewValue is Cif newCif)
            {
                editor.PopulatePalette(newCif);
                editor.InitializeViewport(newCif);
            }
        }

        /// <summary>
        /// Called when [channel offset changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnChannelOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CifChannelEditor editor && editor.CifSource != null)
            {
                editor.RequestRender();
            }
        }

        /// <summary>
        /// Called when [render state changed] (Isolation or Palette Selection).
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnRenderStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CifChannelEditor editor && editor.CifSource != null)
            {
                editor.RequestRender();
            }
        }

        /// <summary>
        /// Populates the color palette collection based on the loaded CIF keys.
        /// </summary>
        /// <param name="cif">The cif.</param>
        private void PopulatePalette(Cif cif)
        {
            PaletteItems.Clear();
            foreach (var colorKey in cif.CifImage.Keys)
            {
                PaletteItems.Add(new CifColorItem(colorKey));
            }
        }

        /// <summary>
        /// Initializes the viewport.
        /// </summary>
        /// <param name="cif">The cif.</param>
        private void InitializeViewport(Cif cif)
        {
            // Bgra32 is highly optimized for WPF rendering
            _writeableBitmap = new WriteableBitmap(cif.Width, cif.Height, 96, 96, PixelFormats.Bgra32, null);

            // Note: Update this to match your actual XAML Image control Name
            // If your Image is named Viewport, this works perfectly.
            if (FindName("Viewport") is Image viewport)
            {
                viewport.Source = _writeableBitmap;
            }

            RequestRender();
        }

        /// <summary>
        /// Requests the render.
        /// </summary>
        private async void RequestRender()
        {
            if (CifSource == null || _writeableBitmap == null) return;

            // Cancel any pending render operations to prevent UI thread thrashing
            _renderTokenSource?.Cancel();
            _renderTokenSource = new CancellationTokenSource();
            var token = _renderTokenSource.Token;

            var rOffset = RedOffset;
            var gOffset = GreenOffset;
            var bOffset = BlueOffset;
            var cif = CifSource;
            var isIsolationEnabled = IsIsolationEnabled;
            var selectedColor = SelectedPaletteItem?.SourceColor;

            try
            {
                // Offload dictionary iteration and array mapping to a background thread
                var pixelData =
                    await Task.Run(
                        () => GeneratePixelData(cif, rOffset, gOffset, bOffset, isIsolationEnabled, selectedColor,
                            token), token);

                if (token.IsCancellationRequested || pixelData == null) return;

                // Push the calculated array directly to the back buffer
                var rect = new Int32Rect(0, 0, cif.Width, cif.Height);
                var stride = cif.Width * 4; // 4 bytes per pixel (Bgra32)
                _writeableBitmap.WritePixels(rect, pixelData, stride, 0);
            }
            catch (TaskCanceledException)
            {
                // Expected when slider moves rapidly
            }
        }

        /// <summary>
        /// Generates the pixel data.
        /// </summary>
        /// <param name="cif">The cif.</param>
        /// <param name="rOffset">The r offset.</param>
        /// <param name="gOffset">The g offset.</param>
        /// <param name="bOffset">The b offset.</param>
        /// <param name="isIsolationEnabled">if set to <c>true</c> [is isolation enabled].</param>
        /// <param name="isolatedColor">The active isolated color to map.</param>
        /// <param name="token">The token.</param>
        /// <returns>Set of Pixel Data for image Display.</returns>
        private static int[]? GeneratePixelData(
            Cif cif,
            int rOffset,
            int gOffset,
            int bOffset,
            bool isIsolationEnabled,
            SystemDrawingColor? isolatedColor,
            CancellationToken token)
        {
            var totalPixels = cif.PixelCount;
            var buffer = new int[totalPixels];

            foreach (var kvp in cif.CifImage)
            {
                if (token.IsCancellationRequested) return null;

                var originalColor = kvp.Key;
                int pixelInt;

                if (isIsolationEnabled && isolatedColor.HasValue)
                {
                    if (originalColor.ToArgb() == isolatedColor.Value.ToArgb())
                    {
                        // Render isolated color directly with offset calculations
                        var newR = Math.Clamp(originalColor.R + rOffset, 0, 255);
                        var newG = Math.Clamp(originalColor.G + gOffset, 0, 255);
                        var newB = Math.Clamp(originalColor.B + bOffset, 0, 255);
                        pixelInt = (originalColor.A << 24) | (newR << 16) | (newG << 8) | newB;
                    }
                    else
                    {
                        // Convert non-isolated colors to grayscale preview
                        var gray = (int)(originalColor.R * 0.299 + originalColor.G * 0.587 + originalColor.B * 0.114);
                        pixelInt = (originalColor.A << 24) | (gray << 16) | (gray << 8) | gray;
                    }
                }
                else
                {
                    // Regular channel offset calculation across all palette keys
                    var newR = Math.Clamp(originalColor.R + rOffset, 0, 255);
                    var newG = Math.Clamp(originalColor.G + gOffset, 0, 255);
                    var newB = Math.Clamp(originalColor.B + bOffset, 0, 255);

                    // Construct Bgra32 integer (Little Endian: A R G B)
                    pixelInt = (originalColor.A << 24) | (newR << 16) | (newG << 8) | newB;
                }

                foreach (var id in kvp.Value)
                {
                    buffer[id] = pixelInt;
                }
            }

            return buffer;
        }

        /// <summary>
        /// Called when [isolation mode changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnIsolationModeChanged(object sender, RoutedEventArgs e)
        {
            RequestRender();
        }

        /// <summary>
        /// Handles the Click event of the LoadCif control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void LoadCif_Click(object sender, RoutedEventArgs e)
        {
            var target = DialogHandler.HandleFileOpen(CifFilter);
            if (target == null || string.IsNullOrEmpty(target.FilePath))
            {
                return;
            }

            // Load CIF structure directly via your custom format
            var cif = _customFormat.GetCif(target.FilePath);
            if (cif == null)
            {
                return;
            }

            // Assign directly to the instance property
            CifSource = cif;
        }

        /// <summary>
        /// Handles the Click event of the ExportCif control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ExportCif_Click(object sender, RoutedEventArgs e)
        {
            // Access property directly on the current instance
            var activeCif = CifSource;
            if (activeCif == null)
            {
                return;
            }

            var target = DialogHandler.HandleFileSave(CifFilter);
            if (target == null || string.IsNullOrEmpty(target.FilePath))
            {
                return;
            }

            // Export active Cif back out to disk
            using var bitmap = activeCif.GetImage();
            if (bitmap != null)
            {
                if (activeCif.Compressed)
                {
                    _customFormat.GenerateCifCompressedFromBitmap(bitmap, target.FilePath);
                }
                else
                {
                    _customFormat.GenerateBitmapToCifFile(bitmap, target.FilePath);
                }
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the PaletteListBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void PaletteListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsolateCheckBox.IsChecked == true)
            {
                RequestRender();
            }
        }

        /// <summary>
        /// Handles the Click event of the ResetPalette control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ResetPalette_Click(object sender, RoutedEventArgs e)
        {
            RedOffset = 0;
            GreenOffset = 0;
            BlueOffset = 0;
            IsolateCheckBox.IsChecked = false;
            PaletteListBox.UnselectAll();
            RequestRender();
        }
    }
}