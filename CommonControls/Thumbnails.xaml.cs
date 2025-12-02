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
        typeof(Thumbnails), null);

    /// <summary>
    ///     The Thumb Length (in lines)
    /// </summary>
    public static readonly DependencyProperty DependencyThumbWidth = DependencyProperty.Register(
        nameof(DependencyThumbWidth),
        typeof(int),
        typeof(Thumbnails), null);

    /// <summary>
    ///     The Thumb Cell Size
    /// </summary>
    public static readonly DependencyProperty DependencyThumbCellSize = DependencyProperty.Register(
        nameof(DependencyThumbCellSize),
        typeof(int),
        typeof(Thumbnails), null);

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
    private bool Refresh = true;

    /// <summary>
    ///     The cancellation token source
    /// </summary>
    private CancellationTokenSource _cancellationTokenSource;

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
    public IEnumerable<int> Selection => _select.Keys;

    /// <summary>
    /// The select
    /// </summary>
    private ConcurrentDictionary<int, bool> _select = new();

    /// <summary>
    /// Gets a value indicating whether this instance is selection valid.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is selection valid; otherwise, <c>false</c>.
    /// </value>
    public bool IsSelectionValid => _select is { Count: > 0 };

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
        if (sender is not Thumbnails control)
            return;

        if (e.NewValue == e.OldValue)
            return;

        if (!control.Refresh) // use instance field now
            return;

        control.OnItemsSourceChangedAsync();
    }


    /// <summary>
    ///     Handles the blanks.
    /// </summary>
    /// <param name="id">The identifier.</param>
    public void RemoveSingleItem(int id)
    {
        Refresh = false;

        if (!ItemsSource.ContainsKey(id))
            return;

        var imageKey = string.Concat(ComCtlResources.ImageAdd, id);

        if (ImageDct.TryRemove(imageKey, out var image))
        {
            image.MouseDown -= ImageClick_MouseDown;
            image.MouseRightButtonDown -= ImageClick_MouseRightButtonDown;
            image.Source = null;
        }

        if (Border.TryRemove(id, out var border))
        {
            Thb.Children.Remove(border);
        }

        if (ChkBox != null && ChkBox.TryRemove(id, out var checkbox))
        {
            checkbox.Checked -= CheckBox_Checked;
            checkbox.Unchecked -= CheckBox_Unchecked;
            Thb.Children.Remove(checkbox);
        }

        _ = ItemsSource.Remove(id);

        Refresh = true;
    }

    /// <summary>
    ///     Called when [items source changed].
    /// </summary>
    private async Task OnItemsSourceChangedAsync()
    {
        ThumbWidth = _originalWidth;
        ThumbHeight = _originalHeight;

        // Clear existing images from the grid
        Thb.Children.Clear();

         await LoadImages();

        //All Images Loaded
        ImageLoadedCommand.Execute(this);
        ImageLoaded();
    }

    /// <summary>
    ///     Handles the Loaded event of the UserControl control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
    private async Task UserControl_LoadedAsync(object sender, RoutedEventArgs e)
    {
        _originalWidth = ThumbWidth;
        _originalHeight = ThumbHeight;

        await LoadImages();
    }

    /// <summary>
    /// Loads all images asynchronously with limited concurrency and updates the UI.
    /// </summary>
    private async Task LoadImages()
    {
        if (ItemsSource == null || ItemsSource.Count == 0)
            return;

        // Cancel any previous loading
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        var timer = Stopwatch.StartNew();

        // Reset UI
        ThumbWidth = _originalWidth;
        ThumbHeight = _originalHeight;
        Thb.Children.Clear();

        // Copy ItemsSource to avoid collection modification issues
        var pics = new Dictionary<int, string>(ItemsSource);

        // Initialize dictionaries
        Keys = new ConcurrentDictionary<string, int>();
        ImageDct = new ConcurrentDictionary<string, Image>();
        Border = new ConcurrentDictionary<int, Border>();
        _select = new ConcurrentDictionary<int, bool>();

        if (SelectBox)
            ChkBox = new ConcurrentDictionary<int, CheckBox>();

        // Default values
        if (ThumbCellSize <= 0) ThumbCellSize = 100;
        if (ThumbHeight <= 0) ThumbHeight = 1;
        if (ThumbWidth <= 0) ThumbWidth = 1;

        // Adjust grid size to fit all images
        if (ThumbHeight * ThumbWidth < pics.Count)
        {
            if (ThumbWidth == 1) ThumbHeight = pics.Count;
            else if (ThumbHeight == 1) ThumbWidth = pics.Count;
            else
            {
                var fraction = new Fraction(pics.Count, ThumbHeight);
                ThumbWidth = (int)Math.Ceiling(fraction.Decimal);
            }
        }

        // Create extended grid
        var exGrid = ExtendedGrid.ExtendGrid(ThumbWidth, ThumbHeight, ThumbGrid);
        Thb.Children.Add(exGrid);

        // Semaphore limits concurrency
        using var semaphore = new SemaphoreSlim(4); // max 4 concurrent image loads

        var tasks = pics.Select(async kvp =>
        {
            await semaphore.WaitAsync(token).ConfigureAwait(false);
            try
            {
                if (!token.IsCancellationRequested)
                    await LoadImageAsync(kvp.Key, kvp.Value, exGrid, token).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);

        timer.Stop();
        Trace.WriteLine($"{ComCtlResources.DebugTimer} {timer.Elapsed}");

        // Notify that loading is finished on UI thread
        await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            ImageLoadedCommand?.Execute(this);
            ImageLoaded?.Invoke();
        }));
    }

    /// <summary>
    ///     Loads the image asynchronous.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="filePath">The filepath.</param>
    /// <param name="exGrid">The ex grid.</param>
    /// <returns>Load all images async</returns>
    /// <summary>
    /// Loads a single image and adds it to the UI asynchronously.
    /// </summary>
    private async Task LoadImageAsync(int key, string filePath, Panel exGrid, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return;

        BitmapImage? bitmap = null;
        try
        {
            // Load bitmap off the UI thread
            bitmap = await Task.Run(() => LoadBitmap(filePath, ThumbCellSize, ThumbCellSize), token);
        }
        catch (Exception ex) when (ex is IOException or ArgumentException or NotSupportedException)
        {
            Trace.WriteLine($"Failed to load image {filePath}: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            return; // Task was cancelled
        }

        if (bitmap == null)
            return;

        var imageName = $"{ComCtlResources.ImageAdd}{key}";
        var image = new Image
        {
            Height = ThumbCellSize,
            Width = ThumbCellSize,
            Name = imageName,
            Source = bitmap,
            ToolTip = filePath
        };

        var border = new Border
        {
            Child = image,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Margin = new Thickness(1),
            Name = imageName
        };

        // Add click handler
        image.MouseDown += ImageClick_MouseDown;

        // If SelectBox is enabled, prepare checkbox
        CheckBox? checkbox = null;
        if (SelectBox)
        {
            checkbox = new CheckBox
            {
                Height = ComCtlResources.CheckBoxDimension,
                Width = ComCtlResources.CheckBoxDimension,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                IsChecked = IsCheckBoxSelected,
                Name = imageName
            };
            checkbox.Checked += CheckBox_Checked;
            checkbox.Unchecked += CheckBox_Unchecked;
            image.MouseRightButtonDown += ImageClick_MouseRightButtonDown;
        }

        // Add to UI on the dispatcher
        await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            Keys.TryAdd(imageName, key);
            ImageDct.TryAdd(imageName, image);
            Border.TryAdd(key, border);

            Grid.SetRow(border, key / ThumbWidth);
            Grid.SetColumn(border, key % ThumbWidth);
            exGrid.Children.Add(border);

            if (checkbox != null)
            {
                ChkBox.TryAdd(key, checkbox);
                Grid.SetRow(checkbox, key / ThumbWidth);
                Grid.SetColumn(checkbox, key % ThumbWidth);
                exGrid.Children.Add(checkbox);
            }
        }));
    }

    /// <summary>
    /// Loads and resizes a bitmap from file (off UI thread).
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <returns></returns>
    private static BitmapImage LoadBitmap(string path, int width, int height)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null!;

        var bitmap = new BitmapImage();
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.DecodePixelWidth = width;
        bitmap.DecodePixelHeight = height;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
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
        if (MainScrollViewer == null)
        {
            return;
        }

        // Check if the item with the specified ID exists
        if (!Border.TryGetValue(id, out var targetElement))
        {
            return;
        }

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
        if (_select.Count == 0)
        {
            return;
        }

        foreach (var id in new List<int>(Selection))
        {
            if (ChkBox.TryGetValue(id, out var check))
            {
                check.IsChecked = false;
            }
        }
    }

    /// <summary>
    ///     Handles the Checked event of the CheckBox control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
    private void CheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox cb && Keys.TryGetValue(cb.Name, out var id))
            _select.TryAdd(id, true);
    }

    /// <summary>
    ///     Handles the Unchecked event of the CheckBox control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
    private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox cb && Keys.TryGetValue(cb.Name, out var id))
            _ = _select.TryRemove(id, out _);
    }

    /// <summary>
    ///     Just some Method to Delegate click
    ///     Notifies Subscriber
    /// </summary>
    /// <param name="args">Custom Events</param>
    private void OnImageThumbClicked(ImageEventArgs args)
    {
        ImageClickedCommand.Execute(args);

        ImageClicked(this, args);
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
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            // Ensure UI thread for WPF elements
            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                foreach (var image in ImageDct.Values)
                {
                    image.MouseDown -= ImageClick_MouseDown;
                    image.MouseRightButtonDown -= ImageClick_MouseRightButtonDown;
                    image.Source = null;
                }

                foreach (var checkbox in ChkBox?.Values ?? Enumerable.Empty<CheckBox>())
                {
                    checkbox.Checked -= CheckBox_Checked;
                    checkbox.Unchecked -= CheckBox_Unchecked;
                }

                Thb.Children.Clear();
            });

            Keys?.Clear();
            ImageDct?.Clear();
            Border?.Clear();
            ChkBox?.Clear();
            _select?.Clear();
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
