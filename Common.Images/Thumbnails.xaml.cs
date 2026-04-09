/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        Thumbnails.xaml.cs
 * PURPOSE:     Custom Thumbnail Control
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Common.Images
{
    /// <summary>
    ///     Basic Image Thumbnails
    /// </summary>
    /// <seealso cref="UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <inheritdoc cref="Window" />
    public sealed partial class Thumbnails : IDisposable
    {
        /// <summary>
        ///     The selection change delegate.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="itemId">The itemId.</param>
        public delegate void DelegateImage(object sender, ImageEventArgs itemId);

        /// <summary>
        ///     The selection change delegate.
        /// </summary>
        public delegate void DelegateLoadFinished();

        /// <summary>
        ///     The Thumb Height (in lines)
        /// </summary>
        public static readonly DependencyProperty DependencyThumbHeight = DependencyProperty.Register(
            nameof(DependencyThumbHeight),
            typeof(int),
            typeof(Thumbnails));

        /// <summary>
        ///     The Thumb Length (in lines)
        /// </summary>
        public static readonly DependencyProperty DependencyThumbWidth = DependencyProperty.Register(
            nameof(DependencyThumbWidth),
            typeof(int),
            typeof(Thumbnails));

        /// <summary>
        ///     The Thumb Cell Size
        /// </summary>
        public static readonly DependencyProperty DependencyThumbCellSize = DependencyProperty.Register(
            nameof(DependencyThumbCellSize),
            typeof(int),
            typeof(Thumbnails));

        /// <summary>
        ///     The dependency thumb grid
        /// </summary>
        public static readonly DependencyProperty DependencyThumbGrid = DependencyProperty.Register(
            nameof(DependencyThumbGrid),
            typeof(bool),
            typeof(Thumbnails), null);

        /// <summary>
        ///     The selection box
        /// </summary>
        public static readonly DependencyProperty SelectionBox = DependencyProperty.Register(nameof(SelectionBox),
            typeof(bool),
            typeof(Thumbnails), null);

        /// <summary>
        ///     The is selected
        /// </summary>
        public static readonly DependencyProperty IsSelected = DependencyProperty.Register(nameof(IsSelected),
            typeof(bool),
            typeof(Thumbnails), null);

        /// <summary>
        ///     The items source property
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource),
            typeof(Dictionary<int, string>),
            typeof(Thumbnails), new PropertyMetadata(OnItemsSourcePropertyChanged));

        /// <summary>
        ///     The image clicked command property
        /// </summary>
        public static readonly DependencyProperty ImageClickedCommandProperty = DependencyProperty.Register(
            nameof(ImageClickedCommand), typeof(ICommand), typeof(Thumbnails), new PropertyMetadata(null));

        /// <summary>
        ///     The image loaded command dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageLoadCommandProperty = DependencyProperty.Register(
            nameof(ImageLoadedCommand),
            typeof(ICommand),
            typeof(Thumbnails),
            new PropertyMetadata(null));

        /// <summary>
        /// The sender tag property
        /// </summary>
        public static readonly DependencyProperty SenderTagProperty = DependencyProperty.Register(
            nameof(SenderTag), typeof(string), typeof(Thumbnails), new PropertyMetadata(string.Empty));

        /// <summary>
        /// The selection property
        /// </summary>
        public static readonly DependencyProperty SelectionProperty = DependencyProperty.Register(
            nameof(Selection), typeof(ConcurrentDictionary<int, bool>), typeof(Thumbnails), new FrameworkPropertyMetadata(null));

        /// <summary>
        ///     The refresh
        /// </summary>
        private static bool _refresh = true;

        /// <summary>
        ///     The cancellation token source
        /// </summary>
        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        /// The loading CTS
        /// </summary>
        private CancellationTokenSource? _loadingCts;

        /// <summary>
        /// The loading task
        /// </summary>
        private Task? _loadingTask;

        /// <summary>
        ///     The current selected border
        /// </summary>
        private Border? _currentSelectedBorder;

        /// <summary>
        ///     The disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        ///     The original height
        /// </summary>
        private int _originalHeight;

        /// <summary>
        ///     The original width
        /// </summary>
        private int _originalWidth;

        /// <summary>
        ///     The selection
        /// </summary>
        private int _selection;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="Thumbnails" /> class.
        /// </summary>
        public Thumbnails()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the height.
        /// </summary>
        /// <value>
        ///     The height.
        /// </value>
        public int ThumbHeight
        {
            get => (int)GetValue(DependencyThumbHeight);
            set => SetValue(DependencyThumbHeight, value);
        }

        /// <summary>
        ///     Gets or sets the length.
        /// </summary>
        /// <value>
        ///     The length.
        /// </value>
        public int ThumbWidth
        {
            get => (int)GetValue(DependencyThumbWidth);
            set => SetValue(DependencyThumbWidth, value);
        }

        /// <summary>
        ///     Gets or sets the size of the cell.
        /// </summary>
        /// <value>
        ///     The size of the cell.
        /// </value>
        public int ThumbCellSize
        {
            get => (int)GetValue(DependencyThumbCellSize);
            set => SetValue(DependencyThumbCellSize, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [thumb grid] is shown.
        /// </summary>
        /// <value>
        ///     <c>true</c> if [thumb grid]; otherwise, <c>false</c>.
        /// </value>
        public bool ThumbGrid

        {
            get => (bool)GetValue(DependencyThumbGrid);
            set => SetValue(DependencyThumbGrid, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [select box].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [select box]; otherwise, <c>false</c>.
        /// </value>
        public bool SelectBox

        {
            get => (bool)GetValue(SelectionBox);
            set => SetValue(SelectionBox, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this instance is CheckBox selected.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is CheckBox selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsCheckBoxSelected

        {
            get => (bool)GetValue(IsSelected);
            set => SetValue(IsSelected, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [thumb grid].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [thumb grid]; otherwise, <c>false</c>.
        /// </value>
        public Dictionary<int, string>? ItemsSource
        {
            get => (Dictionary<int, string>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        ///     Gets or sets the image clicked command.
        /// </summary>
        /// <value>
        ///     The image clicked command.
        /// </value>
        public ICommand ImageClickedCommand
        {
            get => (ICommand)GetValue(ImageClickedCommandProperty);
            set => SetValue(ImageClickedCommandProperty, value);
        }

        /// <summary>
        ///     Gets or sets the image loaded command.
        /// </summary>
        /// <value>
        ///     The image loaded command.
        /// </value>
        public ICommand ImageLoadedCommand
        {
            get => (ICommand)GetValue(ImageLoadCommandProperty);
            set => SetValue(ImageLoadCommandProperty, value);
        }

        /// <summary>
        /// Gets or sets the sender tag.
        /// </summary>
        /// <value>
        /// The sender tag.
        /// </value>
        public string SenderTag
        {
            get => (string)GetValue(SenderTagProperty);
            set => SetValue(SenderTagProperty, value);
        }

        /// <summary>
        ///     The Name of the Image Control
        /// </summary>
        /// <value>
        ///     The Id of the Key
        /// </value>
        private ConcurrentDictionary<string, int>? Keys { get; set; }

        /// <summary>
        ///     Gets or sets the image Dictionary.
        /// </summary>
        /// <value>
        ///     The image Dictionary.
        /// </value>
        private ConcurrentDictionary<string, Image>? ImageDct { get; set; }

        /// <summary>
        ///     Gets or sets the CheckBox.
        /// </summary>
        /// <value>
        ///     The CheckBox.
        /// </value>
        private ConcurrentDictionary<int, CheckBox>? ChkBox { get; set; }

        /// <summary>
        ///     The border
        /// </summary>
        private ConcurrentDictionary<int, Border>? Border { get; set; }

        /// <summary>
        /// Gets or sets the selection.
        /// </summary>
        /// <value>
        /// The selection.
        /// </value>
        public ConcurrentDictionary<int, bool> Selection
        {
            get => (ConcurrentDictionary<int, bool>)GetValue(SelectionProperty);
            set => SetValue(SelectionProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is selection valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is selection valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelectionValid => Selection is { Count: > 0 };

        /// <inheritdoc />
        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     An Image was clicked <see cref="DelegateImage" />.
        /// </summary>
        public event EventHandler<ImageEventArgs>? ImageClicked;

        /// <summary>
        ///     Occurs when [image loaded].
        /// </summary>
        public event DelegateLoadFinished? ImageLoaded;

        /// <summary>
        ///     Called when [items source property changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as Thumbnails;
            if (e.NewValue == e.OldValue)
            {
                return;
            }

            if (!_refresh)
            {
                return;
            }

            control?.OnItemsSourceChanged();
        }

        /// <summary>
        ///     Handles the blanks.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void RemoveSingleItem(int id)
        {
            _refresh = false;

            if (!ItemsSource.ContainsKey(id))
            {
                return;
            }

            var keyName = string.Concat(ComCtlResources.ImageAdd, id);

            // Remove Image and unsubscribe events
            if (ImageDct!.TryRemove(keyName, out var image))
            {
                image.MouseDown -= ImageClick_MouseDown;
                image.MouseRightButtonDown -= ImageClick_MouseRightButtonDown;
                image.Source = null;
                Thb.Children.Remove(image);
            }

            // Remove Border
            if (Border!.TryRemove(id, out var border))
            {
                Thb.Children.Remove(border);
            }

            // Remove CheckBox
            if (SelectBox && ChkBox!.TryRemove(id, out var checkbox))
            {
                checkbox.Checked -= CheckBox_Checked;
                checkbox.Unchecked -= CheckBox_Unchecked;
                Thb.Children.Remove(checkbox);
            }

            _ = ItemsSource.Remove(id);

            _refresh = true;
        }

        /// <summary>
        ///     Called when [items source changed].
        /// </summary>
        private async Task OnItemsSourceChanged()
        {
            // 1. Signal cancellation to the PREVIOUS run
            _loadingCts?.Cancel();

            // 2. WAIT for the previous run to finish or acknowledge cancellation
            // This prevents "Double-Loading" and collection collisions
            if (_loadingTask != null)
            {
                try { await _loadingTask; }
                catch (OperationCanceledException) { /* Swallow expected cancel */ }
            }

            _loadingCts = new CancellationTokenSource();
            var token = _loadingCts.Token;

            // 3. Clean up UI and collections safely
            Thb.Children.Clear();
            Keys?.Clear();
            ImageDct?.Clear();
            ChkBox?.Clear();
            Border?.Clear();
            Selection?.Clear();

            // 4. Start the new task and track it
            _loadingTask = LoadImages(token);
            await _loadingTask;
        }

        /// <summary>
        ///     Handles the Loaded event of the UserControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Capture original width/height immediately
                _originalWidth = ThumbWidth;
                _originalHeight = ThumbHeight;

                // Start loading images asynchronously
                if (ItemsSource != null)
                {
                    _ = LoadItemsAsync();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in Loaded: {ex}");
            }
        }

        /// <summary>
        /// Fire-and-forget wrapper to call your async method
        /// Loads the items asynchronous.
        /// </summary>
        private async Task LoadItemsAsync()
        {
            await OnItemsSourceChanged();
        }

        /// <summary>
        /// Loads the images.
        /// </summary>
        private async Task LoadImages(CancellationToken externalToken)
        {
            if (ItemsSource?.Any() != true) return;

            // 1. Manage the internal CTS
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();

            // 2. Link the external token (from the caller) with our internal token
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, _cancellationTokenSource.Token);
            var token = linkedCts.Token;

            var timer = Stopwatch.StartNew();

            try
            {
                token.ThrowIfCancellationRequested();

                // Copy items source
                var pics = new Dictionary<int, string>(ItemsSource);

                // Initialize dictionaries
                Keys = new ConcurrentDictionary<string, int>();
                ImageDct = new ConcurrentDictionary<string, Image>();
                Border = new ConcurrentDictionary<int, Border>();

                //needed because of Binding
                if (Selection == null)
                {
                    Selection = new ConcurrentDictionary<int, bool>();
                }
                else
                {
                    Selection.Clear();
                }
                if (SelectBox) ChkBox = new ConcurrentDictionary<int, CheckBox>();

                // Capture UI values
                var cellSize = await Dispatcher.InvokeAsync(() => ThumbCellSize);
                var thumbWidth = await Dispatcher.InvokeAsync(() => ThumbWidth);
                var thumbHeight = await Dispatcher.InvokeAsync(() => ThumbHeight);
                var thumbGrid = await Dispatcher.InvokeAsync(() => ThumbGrid);

                // --- Dimension logic ---
                if (cellSize <= 0) cellSize = 100;

                if (thumbHeight <= 0) thumbHeight = 1;
                if (thumbWidth <= 0) thumbWidth = 1;

                var imageCount = pics.Count;

                // First handle single-row or single-column cases
                if (thumbHeight == 1)
                {
                    // horizontal layout → width = number of images
                    thumbWidth = imageCount;
                }
                else if (thumbWidth == 1)
                {
                    // vertical layout → height = number of images
                    thumbHeight = imageCount;
                }
                else if (thumbHeight * thumbWidth < imageCount)
                {
                    // general case → spread images evenly
                    thumbWidth = (imageCount + thumbHeight - 1) / thumbHeight;
                }

                // Update properties after final calculation
                ThumbWidth = thumbWidth;
                ThumbHeight = thumbHeight;
                ThumbCellSize = cellSize;

                // --- Setup grid ---
                var exGrid = ExtendedGrid.ExtendGrid(thumbWidth, thumbHeight, thumbGrid);
                Thb.Children.Clear();
                Thb.Children.Add(exGrid);

                // --- Load images with limited concurrency ---
                var semaphore = new SemaphoreSlim(4);
                var tasks = pics.Select(async kv =>
                {
                    await semaphore.WaitAsync();
                    if (token.IsCancellationRequested)
                    {
                        semaphore.Release();
                        return;
                    }

                    try
                    {
                        await LoadSingleImage(kv.Key, kv.Value, exGrid, token, cellSize, thumbWidth);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }).ToArray();

                await Task.WhenAll(tasks);

                ImageLoaded?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // silently ignore cancellations
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error in LoadImages: {ex.Message}");
            }
            finally
            {
                timer.Stop();
                Trace.WriteLine($"{ComCtlResources.DebugTimer}{timer.Elapsed}");
            }
        }

        /// <summary>
        /// Loads the image asynchronous.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="exGrid">The ex grid.</param>
        /// <param name="token">The token.</param>
        /// <param name="cellSize">Size of the cell.</param>
        /// <param name="thumbWidth">Width of the thumb.</param>
        /// <returns>
        /// Load all images async
        /// </returns>
        private async Task LoadSingleImage(int key, string filePath, Panel exGrid, CancellationToken token, int cellSize, int thumbWidth)
        {
            if (token.IsCancellationRequested) return;

            // 1. Heavy lifting (IO) happens on the background thread
            var bitmap = await GetBitmapImageFileStreamAsync(filePath, cellSize, cellSize);
            if (bitmap == null || token.IsCancellationRequested) return;

            // 2. ALL UI element creation and property setting happens on the UI Thread
            await Dispatcher.InvokeAsync(() =>
            {
                if (token.IsCancellationRequested) return;

                var imageName = $"{ComCtlResources.ImageAdd}{key}";

                // 1. Create the Image and its Border
                var images = new Image
                {
                    Height = cellSize,
                    Width = cellSize,
                    Name = imageName,
                    Source = bitmap,
                    ToolTip = filePath
                };

                var border = new Border
                {
                    Child = images,
                    BorderThickness = new Thickness(0),
                    BorderBrush = Brushes.Transparent,
                    Margin = new Thickness(1),
                    Name = imageName
                };

                // 2. Create a cell container to hold both Image and CheckBox
                var cellContainer = new Grid();

                // The order of adding to Children determines Z-order (last = top)
                cellContainer.Children.Add(border);

                if (SelectBox)
                {
                    var checkbox = new CheckBox
                    {
                        Height = 23,
                        Width = 23,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        IsChecked = IsCheckBoxSelected,
                        Name = imageName,
                        // Ensure the checkbox background doesn't block the image
                        Margin = new Thickness(5)
                    };

                    if (IsCheckBoxSelected) Selection.TryAdd(key, true);

                    checkbox.Checked += CheckBox_Checked;
                    checkbox.Unchecked += CheckBox_Unchecked;
                    ChkBox.TryAdd(key, checkbox);

                    // Add checkbox to the cellContainer, NOT exGrid directly
                    cellContainer.Children.Add(checkbox);

                    images.MouseRightButtonDown += ImageClick_MouseRightButtonDown;
                }

                // 3. Add to collections
                Keys.TryAdd(imageName, key);
                ImageDct.TryAdd(imageName, images);
                Border.TryAdd(key, border);

                // 4. Position the container in the main exGrid
                Grid.SetRow(cellContainer, key / thumbWidth);
                Grid.SetColumn(cellContainer, key % thumbWidth);
                exGrid.Children.Add(cellContainer);

                images.MouseDown += ImageClick_MouseDown;

            }, DispatcherPriority.Normal, token);
        }

        /// <summary>
        ///     Gets the bitmap image file stream asynchronous.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The loaded and resized Image</returns>
        private static async Task<BitmapImage?> GetBitmapImageFileStreamAsync(string filePath, int width, int height)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return null;

            return await Task.Run(() =>
            {
                try
                {
                    // 1. Die Datei wird GEÖFFNET, GELESEN und SOFORT wieder GESCHLOSSEN.
                    byte[] imageBytes = File.ReadAllBytes(filePath);

                    var bitmapImage = new BitmapImage();

                    // MemoryStream ist hier nur der "Überbringer" der Bytes im RAM
                    using (var ms = new MemoryStream(imageBytes))
                    {
                        bitmapImage.BeginInit();

                        // WICHTIG: OnLoad kopiert die Bits in den Grafikspeicher/RAM.
                        // Danach ist der MemoryStream (und die Datei) egal.
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;

                        bitmapImage.DecodePixelWidth = width;
                        bitmapImage.StreamSource = ms;
                        bitmapImage.EndInit();

                        // WICHTIG: Freeze macht das Objekt Thread-Safe und schließt 
                        // alle internen Verbindungen zur Datenquelle.
                        bitmapImage.Freeze();
                    }

                    return bitmapImage;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Fehler beim Laden: {ex.Message}");
                    return null;
                }
            });
        }

        /// <summary>
        ///     Just some Method to Delegate click
        /// </summary>
        /// <param name="sender">Image</param>
        /// <param name="e">Name of Image</param>
        private void ImageClick_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Get the image that was clicked
            if (sender is not Image clickedImage)
            {
                return;
            }

            if (!Keys.TryGetValue(clickedImage.Name, out var id))
            {
                return;
            }

            // Create new click object
            var args = new ImageEventArgs { Id = id, SenderTag = SenderTag };
            OnImageThumbClicked(args); // Trigger the event with the selected image ID

            // Get the parent border (since we wrapped the image in a Border)
            if (clickedImage.Parent is not Border clickedBorder)
            {
                return;
            }

            // Update the selected border (reuse the UpdateSelectedBorder method)
            UpdateSelectedBorder(clickedBorder);
        }

        /// <summary>
        ///     Next Border of this instance.
        /// </summary>
        public void Next()
        {
            var currentIndex = _currentSelectedBorder == null ? -1 : GetCurrentIndex(_currentSelectedBorder.Name);
            var newIndex = (currentIndex + 1) % Border.Count; // Loop to the start if at the end
            SelectImageAtIndex(newIndex);

            CenterOnItem(newIndex);
        }

        /// <summary>
        ///     Previous Border of this instance.
        /// </summary>
        public void Previous()
        {
            var currentIndex = _currentSelectedBorder == null ? -1 : GetCurrentIndex(_currentSelectedBorder.Name);
            var newIndex = (currentIndex - 1 + Border.Count) % Border.Count; // Loop to the end if at the start
            SelectImageAtIndex(newIndex);

            CenterOnItem(newIndex);
        }

        /// <summary>
        ///     Centers the ScrollViewer on a specific item by its ID.
        /// </summary>
        /// <param name="id">The ID of the item to center on.</param>
        public void CenterOnItem(int id)
        {
            if (MainScrollViewer == null || Border == null)
            {
                return;
            }

            // Check if the item with the specified ID exists
            if (Border.TryGetValue(id, out var targetElement) && targetElement != null)
            {
                // Get the position of the target element relative to the ScrollViewer
                var itemTransform = targetElement.TransformToAncestor(MainScrollViewer);
                var itemPosition = itemTransform.Transform(new Point(0, 0));

                // Calculate the offsets needed to center the item
                var centerOffsetX = itemPosition.X - MainScrollViewer.ViewportWidth / 2 +
                                    targetElement.RenderSize.Width / 2;
                var centerOffsetY = itemPosition.Y - MainScrollViewer.ViewportHeight / 2 +
                                    targetElement.RenderSize.Height / 2;

                // Set the ScrollViewer's offset to center the item
                MainScrollViewer.ScrollToHorizontalOffset(centerOffsetX);
                MainScrollViewer.ScrollToVerticalOffset(centerOffsetY);
            }
        }

        /// <summary>
        ///     Handles the MouseRightButtonDown event of the ImageClick control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void ImageClick_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //get the button that was clicked
            if (sender is not Image clickedButton)
            {
                return;
            }

            if (!Keys.TryGetValue(clickedButton.Name, out var value))
            {
                return;
            }

            _selection = value;

            var cm = new ContextMenu();

            var menuItem = new MenuItem { Header = ComCtlResources.ContextDeselect };
            menuItem.Click += Deselect_Click;
            _ = cm.Items.Add(menuItem);

            menuItem = new MenuItem { Header = ComCtlResources.ContextDeselectAll };
            menuItem.Click += DeselectAll_Click;
            _ = cm.Items.Add(menuItem);

            cm.IsOpen = true;
        }

        /// <summary>
        ///     Handles the Click event of the Deselect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void Deselect_Click(object sender, RoutedEventArgs e)
        {
            var check = ChkBox[_selection];
            check.IsChecked = check.IsChecked != true;
        }

        /// <summary>
        ///     Handles the Click event of the DeselectAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            if (Selection.Count == 0)
            {
                return;
            }

            // Take a snapshot to avoid modifying while enumerating
            var selectedIds = Selection.Keys.ToList();

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                foreach (var id in selectedIds)
                {
                    if (ChkBox.TryGetValue(id, out var checkBox))
                    {
                        checkBox.IsChecked = false;
                    }
                }
            });
        }

        /// <summary>
        ///     Handles the Checked event of the CheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //get the button that was clicked
            if (sender is not CheckBox clickedCheckBox)
            {
                return;
            }

            if (!Keys.TryGetValue(clickedCheckBox.Name, out var id))
            {
                return;
            }

            Selection.TryAdd(id, true);
        }

        /// <summary>
        ///     Handles the Unchecked event of the CheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //get the button that was clicked
            if (sender is not CheckBox clickedCheckBox)
            {
                return;
            }

            if (!Keys.TryGetValue(clickedCheckBox.Name, out var id))
            {
                return;
            }

            _ = Selection.TryRemove(id, out _);
        }

        /// <summary>
        ///     Just some Method to Delegate click
        ///     Notifies Subscriber
        /// </summary>
        /// <param name="args">Custom Events</param>
        private void OnImageThumbClicked(ImageEventArgs args)
        {
            ImageClickedCommand?.Execute(args);

            ImageClicked?.Invoke(this, args);
        }

        /// <summary>
        ///     Gets the index of the current.
        /// </summary>
        /// <param name="name">The key.</param>
        /// <returns>Index of Border</returns>
        private int GetCurrentIndex(string name)
        {
            // Find the index of the selected border
            return Border
                .Where(pair => pair.Value.Name == name)
                .Select(pair => pair.Key)
                .FirstOrDefault();
        }

        /// <summary>
        ///     Selects the index of the image at.
        /// </summary>
        /// <param name="index">The index.</param>
        private void SelectImageAtIndex(int index)
        {
            if (index < 0 || index >= Border.Count || !Border.TryGetValue(index, out var border))
            {
                return;
            }

            // Now, update the current selected border with the found Border
            UpdateSelectedBorder(border);
        }

        /// <summary>
        ///     Updates the selected border.
        /// </summary>
        /// <param name="newSelectedBorder">The new selected border.</param>
        private void UpdateSelectedBorder(Border? newSelectedBorder)
        {
            // Remove the "selected" style from the previously selected border
            if (_currentSelectedBorder != null)
            {
                _currentSelectedBorder.BorderBrush = Brushes.Transparent; // Reset previous border
                _currentSelectedBorder.BorderThickness = new Thickness(0); // Reset thickness
            }

            // Set the new border as selected
            newSelectedBorder.BorderBrush = Brushes.Blue; // Set a color for the border
            newSelectedBorder.BorderThickness = new Thickness(2); // Set thickness to highlight

            // Update the current selected border reference
            _currentSelectedBorder = newSelectedBorder;
        }

        /// <summary>
        ///     Disposes the specified disposing.
        /// </summary>
        /// <param name="disposing">if set to <c>true</c> [disposing].</param>
        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Unsubscribe Image events
                foreach (var image in ImageDct?.Values ?? Enumerable.Empty<Image>())
                {
                    image.MouseDown -= ImageClick_MouseDown;
                    image.MouseRightButtonDown -= ImageClick_MouseRightButtonDown;
                    image.Source = null;
                }

                // Unsubscribe CheckBox events
                foreach (var checkBox in ChkBox?.Values ?? Enumerable.Empty<CheckBox>())
                {
                    checkBox.Checked -= CheckBox_Checked;
                    checkBox.Unchecked -= CheckBox_Unchecked;
                }

                // Clear children from the UI
                Thb.Children.Clear();

                // Cancel any ongoing image loading
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();

                _disposed = true;
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _loadingCts?.Cancel();
                _loadingCts?.Dispose();

                ImageDct?.Clear();
                Border?.Clear();
            }

            _disposed = true;
        }

        /// <summary>
        ///     Finalizes this instance.
        /// </summary>
        ~Thumbnails()
        {
            Dispose(false);
        }
    }
}