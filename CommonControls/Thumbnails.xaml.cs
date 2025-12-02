/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/Thumbnails.xaml.cs
 * PURPOSE:     Custom Thumbnail Control
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable EventNeverSubscribedTo.Global
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
using Mathematics;

namespace CommonControls;

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
        typeof(Thumbnails), new PropertyMetadata(100));

    /// <summary>
    ///     The Thumb Length (in lines)
    /// </summary>
    public static readonly DependencyProperty DependencyThumbWidth = DependencyProperty.Register(
        nameof(DependencyThumbWidth),
        typeof(int),
        typeof(Thumbnails), new PropertyMetadata(100));

    /// <summary>
    ///     The Thumb Cell Size
    /// </summary>
    public static readonly DependencyProperty DependencyThumbCellSize = DependencyProperty.Register(
        nameof(DependencyThumbCellSize),
        typeof(int),
        typeof(Thumbnails), new PropertyMetadata(100));

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
    ///     The refresh
    /// </summary>
    private static bool _refresh = true;

    /// <summary>
    ///     The cancellation token source
    /// </summary>
    private CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    ///     The current selected border
    /// </summary>
    private Border _currentSelectedBorder;

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
    public Dictionary<int, string> ItemsSource
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
    ///     The Name of the Image Control
    /// </summary>
    /// <value>
    ///     The Id of the Key
    /// </value>
    private ConcurrentDictionary<string, int> Keys { get; set; }

    /// <summary>
    ///     Gets or sets the image Dictionary.
    /// </summary>
    /// <value>
    ///     The image Dictionary.
    /// </value>
    private ConcurrentDictionary<string, Image> ImageDct { get; set; }

    /// <summary>
    ///     Gets or sets the CheckBox.
    /// </summary>
    /// <value>
    ///     The CheckBox.
    /// </value>
    private ConcurrentDictionary<int, CheckBox> ChkBox { get; set; }

    /// <summary>
    ///     The border
    /// </summary>
    private ConcurrentDictionary<int, Border> Border { get; set; }

    /// <summary>
    ///     Gets or sets the selection.
    /// </summary>
    /// <value>
    ///     The selection.
    /// </value>
    public ConcurrentDictionary<int, bool> Selection = new();

    /// <summary>
    /// Gets a value indicating whether this instance is selection valid.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is selection valid; otherwise, <c>false</c>.
    /// </value>
    public bool IsSelectionValid => Selection != null && Selection.Count > 0;

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
    public event EventHandler<ImageEventArgs> ImageClicked;

    /// <summary>
    ///     Occurs when [image loaded].
    /// </summary>
    public event DelegateLoadFinished ImageLoaded;

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
        if (ImageDct.TryRemove(keyName, out var image))
        {
            image.MouseDown -= ImageClick_MouseDown;
            image.MouseRightButtonDown -= ImageClick_MouseRightButtonDown;
            image.Source = null;
            Thb.Children.Remove(image);
        }

        // Remove Border
        if (Border.TryRemove(id, out var border))
        {
            Thb.Children.Remove(border);
        }

        // Remove CheckBox
        if (SelectBox && ChkBox.TryRemove(id, out var checkbox))
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
        // Cancel any ongoing loads
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();

        // Unsubscribe events and clear dictionaries
        foreach (var img in ImageDct?.Values ?? Enumerable.Empty<Image>())
        {
            img.MouseDown -= ImageClick_MouseDown;
            img.MouseRightButtonDown -= ImageClick_MouseRightButtonDown;
            img.Source = null;
        }

        foreach (var cb in ChkBox?.Values ?? Enumerable.Empty<CheckBox>())
        {
            cb.Checked -= CheckBox_Checked;
            cb.Unchecked -= CheckBox_Unchecked;
        }

        Thb.Children.Clear();
        Keys?.Clear();
        ImageDct?.Clear();
        ChkBox?.Clear();
        Border?.Clear();
        Selection?.Clear();

        ThumbWidth = _originalWidth;
        ThumbHeight = _originalHeight;

        await LoadImages();

        //All Images Loaded
        ImageLoadedCommand?.Execute(this);
        ImageLoaded?.Invoke();
    }

    /// <summary>
    ///     Handles the Loaded event of the UserControl control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _originalWidth = ThumbWidth;
            _originalHeight = ThumbHeight;

            await LoadImages(); // safe to await here
        }
        catch (Exception ex)
        {
            // Log or handle exceptions gracefully
            Trace.WriteLine($"Error loading images: {ex}");
        }
    }

    /// <summary>
    ///     Loads the images.
    /// </summary>
    private async Task LoadImages()
    {
        if (ItemsSource?.Any() != true) return;

        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;



        var timer = new Stopwatch();
        timer.Start();

        // Initiate all values
        ExtendedGrid.CellSize = ThumbCellSize;
        var pics = new Dictionary<int, string>(ItemsSource);
        Keys = new ConcurrentDictionary<string, int>();
        ImageDct = new ConcurrentDictionary<string, Image>();
        Border = new ConcurrentDictionary<int, Border>();
        Selection = new ConcurrentDictionary<int, bool>();
        if (SelectBox) ChkBox = new ConcurrentDictionary<int, CheckBox>();

        int cellSize = await Application.Current.Dispatcher.InvokeAsync(() => ThumbCellSize);
        int thumbWidth = await Application.Current.Dispatcher.InvokeAsync(() => ThumbWidth);
        int thumbHeight = await Application.Current.Dispatcher.InvokeAsync(() => ThumbHeight);

        // Handle special cases
        if (cellSize == 0)
        {
            cellSize = 100;
        }

        if (thumbHeight == 0 && thumbWidth == 0)
        {
            thumbHeight = 1;
        }

        if (ThumbHeight * thumbWidth < pics.Count)
        {
            if (thumbWidth == 1)
            {
                thumbHeight = pics.Count;
            }

            if (thumbHeight == 1)
            {
                thumbWidth = pics.Count;
            }

            if (thumbHeight != 1 && thumbWidth != 1 && pics.Count > 1)
            {
                var fraction = new Fraction(pics.Count, thumbHeight);
                thumbWidth = (int)Math.Ceiling(fraction.Decimal);
            }
        }

        // Setup the grid layout
        var exGrid = ExtendedGrid.ExtendGrid(ThumbWidth, ThumbHeight, ThumbGrid);

        _ = Thb.Children.Add(exGrid);

        var semaphore = new SemaphoreSlim(4); // limit concurrent image loads
        var tasks = pics.Select(async kv =>
        {
            await semaphore.WaitAsync(token);
            try
            {
                await LoadSingleImage(kv.Key, kv.Value, exGrid, token);
            }
            finally
            {
                semaphore.Release();
            }
        }).ToArray();

        await Task.WhenAll(tasks);

        // Wait for all remaining tasks
        await Task.WhenAll(tasks);

        timer.Stop();
        Trace.WriteLine(string.Concat(ComCtlResources.DebugTimer, timer.Elapsed));

        // Notify that loading is finished
        ImageLoaded?.Invoke();
    }

    /// <summary>
    ///     Loads the image asynchronous.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="filePath">The file path.</param>
    /// <param name="exGrid">The ex grid.</param>
    /// <returns>Load all images async</returns>
    private async Task LoadSingleImage(int key, string filePath, Panel exGrid, CancellationToken token)
    {
        if (token.IsCancellationRequested) return;

        // Capture UI-thread values safely
        int cellSize = await Application.Current.Dispatcher.InvokeAsync(() => ThumbCellSize);
        bool isCheckBoxSelected = await Application.Current.Dispatcher.InvokeAsync(() => IsCheckBoxSelected);

        // Load the bitmap off the UI thread
        BitmapImage bitmap = await GetBitmapImageFileStreamAsync(filePath, cellSize, cellSize);
        if (bitmap == null) return;

        // Prepare UI elements
        var images = new Image
        {
            Height = cellSize,
            Width = cellSize,
            Name = $"{ComCtlResources.ImageAdd}{key}"
        };

        var border = new Border
        {
            Child = images,
            BorderThickness = new Thickness(0),
            BorderBrush = Brushes.Transparent,
            Margin = new Thickness(1),
            Name = images.Name
        };

        CheckBox checkbox = null;
        if (SelectBox)
        {
            checkbox = new CheckBox
            {
                Height = 23,
                Width = 23,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                IsChecked = isCheckBoxSelected
            };

            if (isCheckBoxSelected)
            {
                Selection.TryAdd(key, true);
            }

            checkbox.Checked += CheckBox_Checked;
            checkbox.Unchecked += CheckBox_Unchecked;

            ChkBox.TryAdd(key, checkbox);
        }

        // All UI updates in one Dispatcher batch
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            Keys.TryAdd(images.Name, key);
            ImageDct.TryAdd(images.Name, images);
            Border.TryAdd(key, border);

            // Grid placement
            Grid.SetRow(border, key / ThumbWidth);
            Grid.SetColumn(border, key % ThumbWidth);
            _ = exGrid.Children.Add(border);

            if (SelectBox && checkbox != null)
            {
                Grid.SetRow(checkbox, key / ThumbWidth);
                Grid.SetColumn(checkbox, key % ThumbWidth);
                _ = exGrid.Children.Add(checkbox);

                // Attach right-click event for context menu
                images.MouseRightButtonDown += ImageClick_MouseRightButtonDown;
            }

            // Assign image source and tooltip
            images.Source = bitmap;
            images.ToolTip = filePath;

            // Attach left-click event
            images.MouseDown += ImageClick_MouseDown;
        });
    }

    /// <summary>
    ///     Gets the bitmap image file stream asynchronous.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <returns>The loaded and resized Image</returns>
    private static async Task<BitmapImage> GetBitmapImageFileStreamAsync(string filePath, int width, int height)
    {
        return string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)
            ? null
            : await Task.Run(() =>
            {
                BitmapImage bitmapImage = null;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    FileStream stream = null;
                    try
                    {
                        bitmapImage = new BitmapImage();
                        stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.DecodePixelWidth = width;
                        bitmapImage.DecodePixelHeight = height;
                        bitmapImage.StreamSource = stream;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"{ComCtlResources.ErrorCouldNotLoadImage} {ex.Message}");
                    }
                    finally
                    {
                        stream?.Dispose();
                    }
                });

                return bitmapImage;
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
        var args = new ImageEventArgs { Id = id };
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
            var centerOffsetX = itemPosition.X - (MainScrollViewer.ViewportWidth / 2) +
                                (targetElement.RenderSize.Width / 2);
            var centerOffsetY = itemPosition.Y - (MainScrollViewer.ViewportHeight / 2) +
                                (targetElement.RenderSize.Height / 2);

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

        _ = Selection.TryAdd(id, true);
    }

    /// <summary>
    ///     Just some Method to Delegate click
    ///     Notifies Subscriber
    /// </summary>
    /// <param name="args">Custom Events</param>
    private void OnImageThumbClicked(ImageEventArgs args)
    {
        ImageClickedCommand.Execute(args);

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
    private void UpdateSelectedBorder(Border newSelectedBorder)
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