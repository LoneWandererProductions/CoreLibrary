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

namespace CommonControls
{
    /// <summary>
    ///     Basic Image Thumbnails
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
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
        /// The image clicked command property
        /// </summary>
        public static readonly DependencyProperty ImageClickedCommandProperty = DependencyProperty.Register(
            nameof(ImageClickedCommand), typeof(ICommand), typeof(Thumbnails), new PropertyMetadata(null));

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
        ///     The previous selected border
        /// </summary>
        private Border _previousSelectedBorder;

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
        /// Gets or sets the image clicked command.
        /// </summary>
        /// <value>
        /// The image clicked command.
        /// </value>
        public ICommand ImageClickedCommand
        {
            get => (ICommand)GetValue(ImageClickedCommandProperty);
            set => SetValue(ImageClickedCommandProperty, value);
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
        public event DelegateImage ImageClicked;

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

            LoadImages();

            //All Images Loaded
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

            LoadImages();
        }

        /// <summary>
        ///     Loads the images.
        /// </summary>
        private async void LoadImages()
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
                    var fraction = new ExtendedMath.Fraction(pics.Count, ThumbHeight);
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
                if (tasks.Count < 4) continue;

                await Task.WhenAll(tasks);
                tasks.Clear();
            }

            // Wait for all remaining tasks
            await Task.WhenAll(tasks);

            timer.Stop();
            Trace.WriteLine("End: " + timer.Elapsed);

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
                Margin = new Thickness(1) // Optionally add some margin for spacing
            };

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

            if (!Keys.ContainsKey(clickedImage.Name))
            {
                return;
            }

            var id = Keys[clickedImage.Name];

            // Create new click object
            var args = new ImageEventArgs { Id = id };
            OnImageThumbClicked(args); // Trigger the event with the selected image ID

            // Get the parent border (since we wrapped the image in a Border)
            var clickedBorder = clickedImage.Parent as Border;
            if (clickedBorder == null)
            {
                return;
            }

            // Clear previous selection highlight if any
            if (_previousSelectedBorder != null)
            {
                _previousSelectedBorder.BorderBrush = Brushes.Transparent; // Reset previous border highlight
                _previousSelectedBorder.BorderThickness = new Thickness(0); // Reset thickness
            }

            // Highlight the currently clicked thumbnail
            clickedBorder.BorderBrush = Brushes.Blue; // Highlight with blue border
            clickedBorder.BorderThickness = new Thickness(2); // Set border thickness

            // Update the currently selected border
            _currentSelectedBorder = clickedBorder;

            // Update the previous selected border to the current one
            _previousSelectedBorder = _currentSelectedBorder;
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

            if (!Keys.ContainsKey(clickedButton.Name))
            {
                return;
            }

            _selection = Keys[clickedButton.Name];

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

            if (!Keys.ContainsKey(clickedCheckBox.Name))
            {
                return;
            }

            var id = Keys[clickedCheckBox.Name];

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

            if (!Keys.ContainsKey(clickedCheckBox.Name))
            {
                return;
            }

            var id = Keys[clickedCheckBox.Name];

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
        /// <returns></returns>
        ~Thumbnails()
        {
            Dispose(false);
        }
    }

    /// <inheritdoc />
    /// <summary>
    ///     We need the Id of the clicked Image
    /// </summary>
    public sealed class ImageEventArgs : EventArgs
    {
        /// <summary>
        ///     The tile id.
        /// </summary>
        public int Id { get; internal init; }
    }
}
