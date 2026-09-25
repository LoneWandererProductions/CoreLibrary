/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        CifChannelEditor.xaml.cs
 * PURPOSE:     A Color Channel Editor based on my Cif Image format
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Common.Dialogs;
using Imaging.Cifs;
using Image = System.Windows.Controls.Image;
using SystemDrawingColor = System.Drawing.Color;

namespace Common.Images
{
    /// <summary>
    /// Cif Channel Editor Control.
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class CifChannelEditor
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
        /// The is syncing multiselect
        /// </summary>
        private bool _isSyncingMultiselect;

        /// <summary>
        /// The is updating sliders
        /// </summary>
        private bool _isUpdatingSliders;

        /// <summary>
        /// The is bulk updating
        /// </summary>
        private bool _isBulkUpdating;

        /// <summary>
        /// Gets the palette items.
        /// </summary>
        /// <value>
        /// The palette items.
        /// </value>
        public ObservableCollection<CifColorItem> PaletteItems { get; } = new();

        /// <summary>
        /// The current selection
        /// </summary>
        private List<CifColorItem> _currentSelection = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CifChannelEditor"/> class.
        /// </summary>
        public CifChannelEditor()
        {
            InitializeComponent();
            Name = "Root";
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
        /// The is isolation enabled property
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
        /// Gets or sets a value indicating whether this instance is isolation enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is isolation enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsIsolationEnabled
        {
            get => (bool)GetValue(IsIsolationEnabledProperty);
            set => SetValue(IsIsolationEnabledProperty, value);
        }

        /// <summary>
        /// Gets or sets the selected palette item.
        /// </summary>
        /// <value>
        /// The selected palette item.
        /// </value>
        public CifColorItem? SelectedPaletteItem
        {
            get => (CifColorItem?)GetValue(SelectedPaletteItemProperty);
            set => SetValue(SelectedPaletteItemProperty, value);
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
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void OnChannelOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CifChannelEditor { CifSource: { } } editor)
            {
                editor.RequestRender();
            }
        }

        /// <summary>
        /// Called when [render state changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void OnRenderStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CifChannelEditor { CifSource: { } } editor)
            {
                editor.RequestRender();
            }
        }

        /// <summary>
        /// Populates the palette.
        /// </summary>
        /// <param name="cif">The cif.</param>
        private void PopulatePalette(Cif? cif)
        {
            foreach (var item in PaletteItems)
            {
                item.PropertyChanged -= ColorItem_PropertyChanged;
            }

            PaletteItems.Clear();

            var index = 0;

            if (cif?.CifImage == null) return;

            foreach (var kvp in cif.CifImage)
            {
                var count = kvp.Value.Count();
                var newItem = new CifColorItem(kvp.Key, index++, count);
                newItem.PropertyChanged += ColorItem_PropertyChanged;
                PaletteItems.Add(newItem);
            }
        }

        /// <summary>
        /// Reacts to changes in individual CifColorItems.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void ColorItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isBulkUpdating) return;

            if (e.PropertyName is nameof(CifColorItem.R) or nameof(CifColorItem.G) or nameof(CifColorItem.B))
            {
                RequestRender();
                CheckForColorMerge(sender as CifColorItem);
            }
        }

        /// <summary>
        /// Checks for color merge.
        /// </summary>
        /// <param name="changedItem">The changed item.</param>
        private void CheckForColorMerge(CifColorItem? changedItem)
        {
            if (changedItem == null || CifSource == null) return;

            // Check if the newly changed color exactly matches another existing visible color
            var match = PaletteItems.FirstOrDefault(p =>
                p.IsVisible &&
                p != changedItem &&
                p.R == changedItem.R &&
                p.G == changedItem.G &&
                p.B == changedItem.B);

            if (match is { })
            {
                // Note: To implement a full merge, you would need a method in your Cif class
                // that remaps the pixel indices from changedItem.SourceColor to match.SourceColor.
                // activeCif.RemapPixelIndex(changedItem.SourceColor, match.SourceColor);

                changedItem.IsVisible = false;
                SelectedPaletteItem = match; // Shift selection to the merged target
            }
        }

        /// <summary>
        /// Initializes the viewport.
        /// </summary>
        /// <param name="cif">The cif.</param>
        private void InitializeViewport(Cif cif)
        {
            _writeableBitmap = new WriteableBitmap(cif.Width, cif.Height, 96, 96, PixelFormats.Bgra32, null);

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

            _renderTokenSource?.Cancel();
            _renderTokenSource = new CancellationTokenSource();
            var token = _renderTokenSource.Token;

            var rOffset = RedOffset;
            var gOffset = GreenOffset;
            var bOffset = BlueOffset;
            var cif = CifSource;
            var isIsolationEnabled = IsIsolationEnabled;
            var isolatedColorArgb = PaletteListBox.SelectedItems
                .Cast<CifColorItem>()
                .Select(i => i.SourceColor.ToArgb())
                .ToHashSet();

            // Snapshot the current UI palette colors for thread safety
            var activePalette = PaletteItems.ToDictionary(p => p.SourceColor, p => p.CurrentDrawingColor);

            try
            {
                var pixelData = await Task.Run(() => GeneratePixelData(
                        cif, rOffset, gOffset, bOffset, isIsolationEnabled, isolatedColorArgb, activePalette, token),
                    token);

                if (token.IsCancellationRequested || pixelData == null) return;

                var rect = new Int32Rect(0, 0, cif.Width, cif.Height);
                var stride = cif.Width * 4;
                _writeableBitmap.WritePixels(rect, pixelData, stride, 0);
            }
            catch (TaskCanceledException)
            {
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
        /// <param name="isolatedColorArgb">The isolated color ARGB.</param>
        /// <param name="activePalette">The active palette.</param>
        /// <param name="token">The token.</param>
        /// <returns>
        /// Array of Pixel data.
        /// </returns>
        private static int[]? GeneratePixelData(
            Cif cif,
            int rOffset,
            int gOffset,
            int bOffset,
            bool isIsolationEnabled,
            HashSet<int> isolatedColorArgb,
            IReadOnlyDictionary<SystemDrawingColor, SystemDrawingColor> activePalette,
            CancellationToken token)
        {
            if (cif?.CifImage == null) return null;

            var totalPixels = cif.PixelCount;
            var buffer = new int[totalPixels];

            foreach (var (originalColor, value) in cif.CifImage)
            {
                if (token.IsCancellationRequested) return null;
                var baseColor = activePalette.TryGetValue(originalColor, out var uiColor) ? uiColor : originalColor;
                int pixelInt;

                // Updated isolation check to support multiple selected colors
                if (isIsolationEnabled && isolatedColorArgb.Count > 0)
                {
                    if (isolatedColorArgb.Contains(originalColor.ToArgb()))
                    {
                        var newR = Math.Clamp(baseColor.R + rOffset, 0, 255);
                        var newG = Math.Clamp(baseColor.G + gOffset, 0, 255);
                        var newB = Math.Clamp(baseColor.B + bOffset, 0, 255);
                        pixelInt = (baseColor.A << 24) | (newR << 16) | (newG << 8) | newB;
                    }
                    else
                    {
                        var gray = (int)(baseColor.R * 0.299 + baseColor.G * 0.587 + baseColor.B * 0.114);
                        pixelInt = (baseColor.A << 24) | (gray << 16) | (gray << 8) | gray;
                    }
                }
                else
                {
                    var newR = Math.Clamp(baseColor.R + rOffset, 0, 255);
                    var newG = Math.Clamp(baseColor.G + gOffset, 0, 255);
                    var newB = Math.Clamp(baseColor.B + bOffset, 0, 255);
                    pixelInt = (baseColor.A << 24) | (newR << 16) | (newG << 8) | newB;
                }

                foreach (var id in value)
                {
                    buffer[id] = pixelInt;
                }
            }

            return buffer;
        }

        /// <summary>
        /// Handles the Click event of the LoadCif control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void LoadCif_Click(object sender, RoutedEventArgs e)
        {
            var target = DialogHandler.HandleFileOpen(CifFilter);
            if (string.IsNullOrEmpty(target?.FilePath)) return;

            var cif = _customFormat.GetCif(target.FilePath);
            if (cif == null) return;

            CifSource = cif;
        }

        /// <summary>
        /// Handles the Click event of the ExportCif control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ExportCif_Click(object sender, RoutedEventArgs e)
        {
            if (CifSource == null) return;

            var target = DialogHandler.HandleFileSave(CifFilter);
            if (string.IsNullOrEmpty(target?.FilePath)) return;

            using var bitmap = CifSource.GetImage();
            if (bitmap is { })
            {
                if (CifSource.Compressed)
                    _customFormat.GenerateCifCompressedFromBitmap(bitmap, target.FilePath);
                else
                    _customFormat.GenerateBitmapToCifFile(bitmap, target.FilePath);
            }
        }

        /// <summary>
        /// Handles the Click event of the ExportEditedCif control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ExportEditedCif_Click(object sender, RoutedEventArgs e)
        {
            // Exports the modified visual state.
            // In a production environment, you might update the CifSource dictionary directly,
            // but saving the active WriteableBitmap handles the combined offsets + palette edits.
            if (CifSource == null || _writeableBitmap == null) return;

            var target = DialogHandler.HandleFileSave(CifFilter);
            if (string.IsNullOrEmpty(target?.FilePath)) return;

            var dir = Path.GetDirectoryName(target.FilePath) ?? string.Empty;
            var file = Path.GetFileNameWithoutExtension(target.FilePath);
            var newPath = Path.Combine(dir, $"{file}_edited.cif");

            // Convert WriteableBitmap back to standard Bitmap for custom format pipeline
            using var outStream = new MemoryStream();
            BitmapEncoder enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(_writeableBitmap));
            enc.Save(outStream);
            using var bitmap = new Bitmap(outStream);

            if (CifSource.Compressed)
                _customFormat.GenerateCifCompressedFromBitmap(bitmap, newPath);
            else
                _customFormat.GenerateBitmapToCifFile(bitmap, newPath);
        }

        /// <summary>
        /// Handles the SelectionChanged event of the PaletteListBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void PaletteListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentSelection = PaletteListBox.SelectedItems.Cast<CifColorItem>().ToList();

            // Bei neuer Auswahl die Slider auf die Werte der primär gewählten Farbe setzen
            if (SelectedPaletteItem != null)
            {
                _isUpdatingSliders = true;
                EditRSlider.Value = SelectedPaletteItem.R;
                EditGSlider.Value = SelectedPaletteItem.G;
                EditBSlider.Value = SelectedPaletteItem.B;
                _isUpdatingSliders = false;

                UpdatePreviewColor();
            }

            if (IsIsolationEnabled)
            {
                RequestRender();
            }
        }

        /// <summary>
        /// Snaps a global channel offset slider back to 0 on double-click - a quick, discoverable
        /// way to undo a single channel without having to hit the full "Reset Image" button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OffsetSlider_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Slider slider)
            {
                slider.Value = 0;
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

            // Reset all local color edits
            foreach (var item in PaletteItems)
            {
                item.R = item.SourceColor.R;
                item.G = item.SourceColor.G;
                item.B = item.SourceColor.B;
                item.IsVisible = true;
            }

            RequestRender();
        }

        /// <summary>
        /// Handles the Click event of the ApplyColorEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void ApplyColorEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSelection.Count == 0) return;

            var r = (byte)EditRSlider.Value;
            var g = (byte)EditGSlider.Value;
            var b = (byte)EditBSlider.Value;

            _isBulkUpdating = true;

            foreach (var item in _currentSelection)
            {
                item.R = r;
                item.G = g;
                item.B = b;
            }

            _isBulkUpdating = false;

            RequestRender();

            foreach (var item in _currentSelection.ToList())
            {
                CheckForColorMerge(item);
            }
        }

        /// <summary>
        /// Handles the ValueChanged event of the EditSlider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedPropertyChangedEventArgs{double}"/> instance containing the event data.</param>
        /// <returns></returns>
        private void EditSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isUpdatingSliders) return;
            UpdatePreviewColor();
        }

        /// <summary>
        /// Updates the color of the preview.
        /// </summary>
        /// <returns></returns>
        private void UpdatePreviewColor()
        {
            if (PreviewBrush == null) return;
            var r = (byte)EditRSlider.Value;
            var g = (byte)EditGSlider.Value;
            var b = (byte)EditBSlider.Value;
            PreviewBrush.Color = System.Windows.Media.Color.FromRgb(r, g, b);
        }
    }
}
