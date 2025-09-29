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
    public List<int> Selection { get; private set; }

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

        var image = ImageDct[string.Concat(ComCtlResources.ImageAdd, id)];

        if (image != null)
        {
            image.Source = null;
        }

        _ = ItemsSource.Remove(id);

        _refresh = true;
    }

    /// <summary>
    ///     Called when [items source changed].
    /// </summary>
    private void OnItemsSourceChanged()
    {
        ThumbWidth = _originalWidth;
        ThumbHeight = _originalHeight;

        // Clear existing images from the grid
        Thb.Children.Clear();

        _ = LoadImages();

        //All Images Loaded
        ImageLoadedCommand?.Execute(this);
        ImageLoaded?.Invoke();
    }

    /// <summary>
    ///     Handles the Loaded event of the UserControl control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        _originalWidth = ThumbWidth;
        _originalHeight = ThumbHeight;

        _ = LoadImages();
    }

    /// <summary>
    ///     Loads the images.
    /// </summary>
    private async Task LoadImages()
    {
        if (ItemsSource?.Any() != true)
        {
            return;
        }

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
        Selection = new List<int>();

        if (SelectBox)
        {
            ChkBox = new ConcurrentDictionary<int, CheckBox>();
        }

        // Handle special cases
        if (ThumbCellSize == 0)
        {
            ThumbCellSize = 100;
        }

        if (ThumbHeight == 0 && ThumbWidth == 0)
        {
            ThumbHeight = 1;
        }

        if (ThumbHeight * ThumbWidth < pics.Count)
        {
            if (ThumbWidth == 1)
            {
                ThumbHeight = pics.Count;
            }

            if (ThumbHeight == 1)
            {
                ThumbWidth = pics.Count;
            }

            if (ThumbHeight != 1 && ThumbWidth != 1 && pics.Count > 1)
            {
                var fraction = new Fraction(pics.Count, ThumbHeight);
                ThumbWidth = (int)Math.Ceiling(fraction.Decimal);
            }
        }

        // Setup the grid layout
        var exGrid = ExtendedGrid.ExtendGrid(ThumbWidth, ThumbHeight, ThumbGrid);

        _ = Thb.Children.Add(exGrid);

        var tasks = new List<Task>();
        foreach (var (key, name) in pics)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            tasks.Add(LoadImageAsync(key, name, exGrid));

            // Limit the number of concurrent tasks to avoid overloading
            if (tasks.Count < 4)
            {
                continue;
            }

            await Task.WhenAll(tasks);
            tasks.Clear();
        }

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
    /// <param name="name">The name.</param>
    /// <param name="exGrid">The ex grid.</param>
    /// <returns>Load all images async</returns>
    private async Task LoadImageAsync(int key, string name, Panel exGrid)
    {
        var token = _cancellationTokenSource.Token;
        if (token.IsCancellationRequested)
        {
            return;
        }

        BitmapImage myBitmapCell = null;

        // Create the image placeholder
        var images = new Image
        {
            Height = ThumbCellSize, Width = ThumbCellSize, Name = string.Concat(ComCtlResources.ImageAdd, key)
        };

        // Create a border around the image
        var border = new Border
        {
            Child = images, // Set the image as the child of the border
            BorderThickness = new Thickness(0), // Initially no border
            BorderBrush = Brushes.Transparent, // Initially transparent
            Margin = new Thickness(1), // Optionally add some margin for spacing
            Name = string.Concat(ComCtlResources.ImageAdd, key)
        };

        //add a reference to Border for later use
        Border.TryAdd(key, border);

        // Add image click handler (this should run on the UI thread)
        images.MouseDown += ImageClick_MouseDown;

        // Add to dictionaries and grid on the UI thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            Keys.TryAdd(images.Name, key);
            ImageDct.TryAdd(images.Name, images);
            Grid.SetRow(border, key / ThumbWidth); // Use the border instead of the image
            Grid.SetColumn(border, key % ThumbWidth);
            _ = exGrid.Children.Add(border); // Add the border to the grid
        });

        // Try loading the bitmap image
        try
        {
            myBitmapCell = await GetBitmapImageFileStreamAsync(name, ThumbCellSize, ThumbCellSize);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException
                                       or InvalidOperationException)
        {
            Trace.WriteLine(ex);
        }

        if (myBitmapCell == null)
        {
            return;
        }

        // Set the image source on the UI thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            images.ToolTip = name;
            images.Source = myBitmapCell;
        });

        if (SelectBox)
            // Handle checkboxes for selection on the UI thread
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                images.MouseRightButtonDown += ImageClick_MouseRightButtonDown;

                var checkbox = new CheckBox
                {
                    Height = 23,
                    Width = 23,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    IsChecked = IsCheckBoxSelected
                };

                if (IsCheckBoxSelected)
                {
                    Selection.Add(key);
                }

                checkbox.Checked += CheckBox_Checked;
                checkbox.Unchecked += CheckBox_Unchecked;
                ChkBox.TryAdd(key, checkbox);

                checkbox.Name = string.Concat(ComCtlResources.ImageAdd, key);
                Grid.SetRow(checkbox, key / ThumbWidth);
                Grid.SetColumn(checkbox, key % ThumbWidth);
                _ = exGrid.Children.Add(checkbox);
            });
        }
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

        foreach (var check in new List<int>(Selection).Select(id => ChkBox[id]))
        {
            check.IsChecked = false;
        }
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

        Selection.Add(id);
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

        _ = Selection.Remove(id);
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
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Dispose managed resources
            // e.g., unsubscribe from events, dispose of Image objects, etc.
            foreach (var image in Thb.Children.OfType<Image>())
            {
                image.Source = null; // Release image source
            }

            Thb.Children.Clear(); // Clear children from the UI element
        }

        // Dispose unmanaged resources if any...

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
