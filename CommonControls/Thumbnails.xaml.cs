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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ExtendedSystemObjects;
using Imaging;
using Mathematics;

namespace CommonControls
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     Basic Image Thumbnails
    /// </summary>
    public sealed partial class Thumbnails
    {
        /// <summary>
        ///     The selection change delegate.
        /// </summary>
        /// <param name="itemId">The itemId.</param>
        public delegate void DelegateImage(ImageEventArgs itemId);

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
        ///     The Thumb Cell Size
        /// </summary>
        public static readonly DependencyProperty DependencyThumbGrid = DependencyProperty.Register(
            nameof(DependencyThumbGrid),
            typeof(bool),
            typeof(Thumbnails), null);

        /// <summary>
        ///     The Thumb Cell Size
        /// </summary>
        public static readonly DependencyProperty SelectionBox = DependencyProperty.Register(nameof(SelectionBox),
            typeof(bool),
            typeof(Thumbnails), null);

        /// <summary>
        ///     The Thumb Cell Size
        /// </summary>
        public static readonly DependencyProperty IsSelected = DependencyProperty.Register(nameof(IsSelected),
            typeof(bool),
            typeof(Thumbnails), null);

        /// <summary>
        ///     The Thumb Cell Size
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource),
            typeof(Dictionary<int, string>),
            typeof(Thumbnails), new PropertyMetadata(OnItemsSourcePropertyChanged));

        /// <summary>
        ///     The refresh
        /// </summary>
        private static bool _refresh = true;

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
        ///     The Name of the Image Control
        /// </summary>
        /// <value>
        ///     The Id of the Key
        /// </value>
        private Dictionary<string, int> Keys { get; set; }

        /// <summary>
        ///     Gets or sets the image Dictionary.
        /// </summary>
        /// <value>
        ///     The image Dictionary.
        /// </value>
        private Dictionary<string, Image> ImageDct { get; set; }

        /// <summary>
        ///     Gets or sets the CHK box.
        /// </summary>
        /// <value>
        ///     The CHK box.
        /// </value>
        private Dictionary<int, CheckBox> ChkBox { get; set; }

        /// <summary>
        ///     Gets or sets the selection.
        /// </summary>
        /// <value>
        ///     The selection.
        /// </value>
        public List<int> Selection { get; private set; }

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
            // add an Can Execute
            if (!_refresh) return;

            control?.OnItemsSourceChanged();
        }

        /// <summary>
        ///     Handles the blanks.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void RemoveSingleItem(int id)
        {
            _refresh = false;

            if (!ItemsSource.ContainsKey(id)) return;

            var image = ImageDct[string.Concat(ComCtlResources.ImageAdd, id)];

            if (image != null) image.Source = null;

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
        private void LoadImages()
        {
            if (ItemsSource.IsNullOrEmpty())
            {
                Thb.Children.Clear();
                return;
            }

            var timer = new Stopwatch();
            timer.Start();

            //Initiate all Values
            ExtendedGrid.CellSize = ThumbCellSize;
            var pics = new Dictionary<int, string>(ItemsSource);

            Keys = new Dictionary<string, int>(pics.Count);
            ImageDct = new Dictionary<string, Image>(pics.Count);
            Selection = new List<int>();

            if (SelectBox) ChkBox = new Dictionary<int, CheckBox>(pics.Count);

            //Handle some special cases
            if (ThumbCellSize == 0) ThumbCellSize = 100;
            if (ThumbHeight == 0 && ThumbWidth == 0) ThumbHeight = 1;

            //here we are especial clever, if we add the Height in the Designer we can generate a custom Length
            //catch on reload
            if (ThumbHeight * ThumbWidth < pics.Count)
            {
                if (ThumbWidth == 1) ThumbHeight = pics.Count;

                if (ThumbHeight == 1) ThumbWidth = pics.Count;

                if (ThumbHeight != 1 && ThumbWidth != 1 && pics.Count > 1)
                {
                    var fraction = new ExtendedMath.Fraction(pics.Count, ThumbHeight);
                    ThumbWidth = (int)Math.Ceiling(fraction.Decimal);
                }
            }

            var exGrid = ExtendedGrid.ExtendGrid(ThumbWidth, ThumbHeight, ThumbGrid);
            Thb.Children.Clear();
            _ = Thb.Children.Add(exGrid);

            for (var y = 0; y < ThumbHeight; y++)
            for (var x = 0; x < ThumbWidth; x++)
            {
                //everything empty? well bail out, if not well we have work
                if (pics.Count == 0) continue;

                var (key, name) = pics.First();

                //Create new Image with Click Handler
                var images = new Image();
                images.Height = images.Width = ThumbCellSize;
                images.Name = string.Concat(ComCtlResources.ImageAdd, key);
                Keys.Add(images.Name, key);
                ImageDct.Add(images.Name, images);
                images.MouseDown += ImageClick_MouseDown;

                //Add Image to Canvas
                Grid.SetRow(images, y);
                Grid.SetColumn(images, x);
                _ = exGrid.Children.Add(images);

                //add an overlay here to get a selection frame
                if (SelectBox)
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

                    //add to our List of selected Items
                    if (IsCheckBoxSelected) Selection.Add(key);

                    checkbox.Checked += CheckBox_Checked;
                    checkbox.Unchecked += CheckBox_Unchecked;
                    ChkBox.Add(key, checkbox);

                    checkbox.Name = string.Concat(ComCtlResources.ImageAdd, key);
                    Grid.SetRow(checkbox, y);
                    Grid.SetColumn(checkbox, x);
                    _ = exGrid.Children.Add(checkbox);
                }

                _ = pics.Reduce();
                BitmapImage myBitmapCell = null;

                try
                {
                    myBitmapCell = ImageStream.GetBitmapImageFileStream(name, ThumbCellSize, ThumbCellSize);
                }
                catch (ArgumentException ex)
                {
                    Trace.WriteLine(ex);
                }
                catch (IOException ex)
                {
                    Trace.WriteLine(ex);
                }
                catch (NotSupportedException ex)
                {
                    Trace.WriteLine(ex);
                }
                catch (InvalidOperationException ex)
                {
                    Trace.WriteLine(ex);
                }

                //handle error gracefully
                if (myBitmapCell == null) continue;

                images.ToolTip = name;

                //Source:
                //http://blog.andreweichacker.com/2008/10/just-a-bit-loading-images-asynchronously-in-wpf/

                _ = images.Dispatcher?.BeginInvoke(DispatcherPriority.Loaded,
                    (ThreadStart)(() => images.Source = myBitmapCell));
            }

            timer.Stop();

            Trace.WriteLine("End: " + timer.Elapsed);
        }

        /// <summary>
        ///     Just some Method to Delegate click
        /// </summary>
        /// <param name="sender">Image</param>
        /// <param name="e">Name of Image</param>
        private void ImageClick_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //get the button that was clicked
            if (sender is not Image clickedButton) return;

            if (!Keys.ContainsKey(clickedButton.Name)) return;

            var id = Keys[clickedButton.Name];

            //create new click Object
            var args = new ImageEventArgs { Id = id };
            OnImageThumbClicked(args);
        }

        /// <summary>
        ///     Handles the MouseRightButtonDown event of the ImageClick control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void ImageClick_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //get the button that was clicked
            if (sender is not Image clickedButton) return;

            if (!Keys.ContainsKey(clickedButton.Name)) return;

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
            if (Selection.Count == 0) return;

            foreach (var check in new List<int>(Selection).Select(id => ChkBox[id])) check.IsChecked = false;
        }

        /// <summary>
        ///     Handles the Checked event of the CheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //get the button that was clicked
            if (sender is not CheckBox clickedCheckBox) return;

            if (!Keys.ContainsKey(clickedCheckBox.Name)) return;

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
            if (sender is not CheckBox clickedCheckBox) return;

            if (!Keys.ContainsKey(clickedCheckBox.Name)) return;

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
            ImageClicked?.Invoke(args);
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