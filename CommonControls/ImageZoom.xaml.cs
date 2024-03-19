/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/ImageZoom.xaml.cs
 * PURPOSE:     Image View Control, that can handle some tools
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable EventNeverSubscribedTo.Global, only used outside of the dll
// ReSharper disable MemberCanBeInternal, must be visible, if we want to use it outside of the dll
// ReSharper disable UnusedType.Global

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommonControls
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     ImageZoom Image
    /// </summary>
    public sealed partial class ImageZoom
    {
        /// <summary>
        ///     Delegate for Image Frame
        /// </summary>
        /// <param name="frame">The frame.</param>
        public delegate void DelegateFrame(SelectionFrame frame);

        /// <summary>
        ///     Delegate for Image Point
        /// </summary>
        /// <param name="point">The point.</param>
        public delegate void DelegatePoint(Point point);

        /// <summary>
        ///     The image source property
        /// </summary>
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(nameof(ItemsSource),
            typeof(BitmapImage),
            typeof(ImageZoom), new PropertyMetadata(OnImageSourcePropertyChanged));

        /// <summary>
        ///     The gif image source property
        /// </summary>
        public static readonly DependencyProperty ImageGifSourceProperty = DependencyProperty.Register(
            nameof(ImageGifPath),
            typeof(string),
            typeof(ImageZoom), new PropertyMetadata(OnImageGifSourcePropertyChanged));

        /// <summary>
        ///     The zoom tools
        /// </summary>
        public static readonly DependencyProperty ZoomTools = DependencyProperty.Register(nameof(ZoomTool),
            typeof(SelectionTools),
            typeof(ImageZoom), null);

        /// <summary>
        ///     The autoplay gif Property
        /// </summary>
        public static readonly DependencyProperty AutoplayGif = DependencyProperty.Register(nameof(AutoplayGifImage),
            typeof(bool),
            typeof(ImageZoom), null);

        /// <summary>
        ///     The image Start Point
        /// </summary>
        private Point _imageStartPoint;

        /// <summary>
        ///     The mouse down
        ///     Set to 'true' when mouse is held down.
        /// </summary>
        private bool _mouseDown;

        /// <summary>
        ///     The origin Point.
        /// </summary>
        private Point _originPoint;

        /// <summary>
        ///     The mouse down position
        /// </summary>
        private Point _startPoint;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="Window" /> class.
        /// </summary>
        public ImageZoom()
        {
            InitializeComponent();
            if (BtmImage.Source == null)
            {
                return;
            }

            MainCanvas.Height = BtmImage.Source.Height;
            MainCanvas.Width = BtmImage.Source.Width;
        }

        /// <summary>
        ///     Gets or sets the items source.
        /// </summary>
        /// <value>
        ///     The items source.
        /// </value>
        public BitmapImage ItemsSource
        {
            get => (BitmapImage)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        /// <summary>
        ///     Gets or sets the gif image  path.
        /// </summary>
        /// <value>
        ///     The gif image path.
        /// </value>
        public string ImageGifPath
        {
            get => (string)GetValue(ImageGifSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        /// <summary>
        ///     Gets or sets the zoom.
        /// </summary>
        /// <value>
        ///     The zoom.
        /// </value>
        public SelectionTools ZoomTool
        {
            get => (SelectionTools)GetValue(ZoomTools);
            set => SetValue(ZoomTools, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [autoplay GIF image].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [autoplay GIF image]; otherwise, <c>false</c>.
        /// </value>
        public bool AutoplayGifImage
        {
            get => (bool)GetValue(AutoplayGif);
            set => SetValue(AutoplayGif, value);
        }

        /// <summary>
        ///     Occurs when [selected frame] was changed
        /// </summary>
        public event DelegateFrame SelectedFrame;

        /// <summary>
        ///     Occurs when [selected point].
        /// </summary>
        public event DelegatePoint SelectedPoint;

        /// <summary>
        ///     Called when [image source property changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void OnImageSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ImageZoom;
            control?.OnImageSourceChanged();
        }

        /// <summary>
        ///     Called when [image GIF source property changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void OnImageGifSourcePropertyChanged(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ImageZoom;
            control?.OnImageSourceGifChanged();
        }

        /// <summary>
        ///     Called when [image source GIF changed].
        /// </summary>
        private void OnImageSourceGifChanged()
        {
            if (!File.Exists(ImageGifPath))
            {
                BtmImage.GifSource = null;
                BtmImage.Source = null;
                return;
            }

            //reset position
            var matrix = BtmImage.RenderTransform.Value;
            matrix.OffsetX = 0;
            matrix.OffsetY = 0;
            BtmImage.RenderTransform = new MatrixTransform(matrix);

            //reset Scrollbar
            ScrollView.ScrollToTop();
            ScrollView.UpdateLayout();

            BtmImage.GifSource = ImageGifPath;

            MainCanvas.Height = BtmImage.Source.Height;
            MainCanvas.Width = BtmImage.Source.Width;
        }

        /// <summary>
        ///     Called when [image source changed].
        /// </summary>
        private void OnImageSourceChanged()
        {
            BtmImage.StopAnimation();
            BtmImage.Source = ItemsSource;

            if (BtmImage.Source == null)
            {
                return;
            }

            //reset Scaling
            Scale.ScaleX = 1;
            Scale.ScaleY = 1;

            //reset position
            var matrix = BtmImage.RenderTransform.Value;
            matrix.OffsetX = 0;
            matrix.OffsetY = 0;
            BtmImage.RenderTransform = new MatrixTransform(matrix);

            //reset Scrollbar
            ScrollView.ScrollToTop();
            ScrollView.UpdateLayout();

            MainCanvas.Height = BtmImage.Source.Height;
            MainCanvas.Width = BtmImage.Source.Width;
        }

        /// <summary>
        ///     Handles the MouseDown event of the Grid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Capture and track the mouse.
            _mouseDown = true;
            _startPoint = e.GetPosition(MainCanvas);
            _originPoint.X = BtmImage.RenderTransform.Value.OffsetX;
            _originPoint.Y = BtmImage.RenderTransform.Value.OffsetY;
            _ = MainCanvas.CaptureMouse();

            switch (ZoomTool)
            {
                case SelectionTools.Move:
                case SelectionTools.SelectPixel:
                    // nothing
                    break;

                case SelectionTools.SelectRectangle:
                case SelectionTools.Erase:
                {
                    // Get the Position on the Image
                    _imageStartPoint = e.GetPosition(BtmImage);

                    // Initial placement of the drag selection box.
                    Canvas.SetLeft(SelectionBox, _startPoint.X);
                    Canvas.SetTop(SelectionBox, _startPoint.Y);
                    SelectionBox.Width = 0;
                    SelectionBox.Height = 0;

                    // Make the drag selection box visible.
                    SelectionBox.Visibility = Visibility.Visible;
                }
                    break;
                default:
                    // nothing
                    return;
            }
        }

        /// <summary>
        ///     Handles the MouseUp event of the Grid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Release the mouse capture and stop tracking it.
            _mouseDown = false;
            MainCanvas.ReleaseMouseCapture();

            // Hide the drag selection box.
            SelectionBox.Visibility = Visibility.Collapsed;

            //clicked Endpoint
            Point endpoint;

            switch (ZoomTool)
            {
                case SelectionTools.Move:
                    // nothing
                    break;

                case SelectionTools.SelectRectangle:
                case SelectionTools.Erase:
                {
                    SelectionBox.Visibility = Visibility.Collapsed;

                    // Get the Position on the Image
                    endpoint = e.GetPosition(BtmImage);
                    var frame = new SelectionFrame();

                    if (_imageStartPoint.X < endpoint.X)
                    {
                        frame.X = (int)_imageStartPoint.X;
                        frame.Width = (int)(endpoint.X - _imageStartPoint.X);
                    }
                    else
                    {
                        frame.Y = (int)endpoint.X;
                        frame.Width = (int)(_imageStartPoint.X - endpoint.X);
                    }

                    if (_startPoint.Y < endpoint.Y)
                    {
                        frame.Y = (int)_startPoint.Y;
                        frame.Height = (int)(endpoint.Y - _imageStartPoint.Y);
                    }
                    else
                    {
                        frame.Y = (int)endpoint.Y;
                        frame.Height = (int)(_imageStartPoint.Y - endpoint.Y);
                    }
                    //cleanups, In case we overstepped the boundaries

                    if (frame.X < 0)
                    {
                        frame.X = 0;
                    }

                    if (frame.Y < 0)
                    {
                        frame.Y = 0;
                    }

                    if (frame.Width > ItemsSource.Width)
                    {
                        frame.Width = (int)ItemsSource.Width;
                    }

                    if (frame.Height < 0)
                    {
                        frame.Height = (int)ItemsSource.Height;
                    }

                    SelectedFrame?.Invoke(frame);
                }
                    break;
                case SelectionTools.SelectPixel:
                    endpoint = e.GetPosition(BtmImage);
                    SelectedPoint?.Invoke(endpoint);
                    break;
                default:
                    // nothing
                    return;
            }
        }

        /// <summary>
        ///     Handles the MouseMove event of the Canvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_mouseDown)
            {
                return;
            }

            switch (ZoomTool)
            {
                case SelectionTools.Move:
                {
                    var position = e.GetPosition(MainCanvas);
                    var matrix = BtmImage.RenderTransform.Value;
                    matrix.OffsetX = _originPoint.X + (position.X - _startPoint.X);
                    matrix.OffsetY = _originPoint.Y + (position.Y - _startPoint.Y);
                    BtmImage.RenderTransform = new MatrixTransform(matrix);
                    break;
                }

                case SelectionTools.SelectRectangle:
                case SelectionTools.Erase:
                {
                    // When the mouse is held down, reposition the drag selection box.

                    var mousePos = e.GetPosition(MainCanvas);

                    if (_startPoint.X < mousePos.X)
                    {
                        Canvas.SetLeft(SelectionBox, _startPoint.X);
                        SelectionBox.Width = mousePos.X - _startPoint.X;
                    }
                    else
                    {
                        Canvas.SetLeft(SelectionBox, mousePos.X);
                        SelectionBox.Width = _startPoint.X - mousePos.X;
                    }

                    if (_startPoint.Y < mousePos.Y)
                    {
                        Canvas.SetTop(SelectionBox, _startPoint.Y);
                        SelectionBox.Height = mousePos.Y - _startPoint.Y;
                    }
                    else
                    {
                        Canvas.SetTop(SelectionBox, mousePos.Y);
                        SelectionBox.Height = _startPoint.Y - mousePos.Y;
                    }
                }
                    break;
                case SelectionTools.SelectPixel:
                    break;
                default:
                    // nothing
                    return;
            }
        }

        /// <summary>
        ///     Handles the MouseWheel event of the Grid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseWheelEventArgs" /> instance containing the event data.</param>
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //TODO add a Lock here for the Image change

            if (e.Delta > 0)
            {
                Scale.ScaleX *= 1.1;
                Scale.ScaleY *= 1.1;
            }
            else
            {
                Scale.ScaleX /= 1.1;
                Scale.ScaleY /= 1.1;
            }
        }
    }

    /// <summary>
    ///     The Selection Frame on the Image
    /// </summary>
    public sealed class SelectionFrame
    {
        /// <summary>
        ///     Gets or sets the x.
        /// </summary>
        /// <value>
        ///     The x.
        /// </value>
        public int X { get; internal set; }

        /// <summary>
        ///     Gets or sets the y.
        /// </summary>
        /// <value>
        ///     The y.
        /// </value>
        public int Y { get; internal set; }

        /// <summary>
        ///     Gets or sets the width.
        /// </summary>
        /// <value>
        ///     The width.
        /// </value>
        public int Width { get; internal set; }

        /// <summary>
        ///     Gets or sets the height.
        /// </summary>
        /// <value>
        ///     The height.
        /// </value>
        public int Height { get; internal set; }
    }

    /// <summary>
    ///     The possible Selection Tools
    /// </summary>
    public enum SelectionTools
    {
        /// <summary>
        ///     The move
        /// </summary>
        Move = 0,

        /// <summary>
        ///     The select rectangle
        /// </summary>
        SelectRectangle = 1,

        /// <summary>
        ///     The select Color of Point
        /// </summary>
        SelectPixel = 2,

        /// <summary>
        ///     The erase
        /// </summary>
        Erase = 3
    }
}
