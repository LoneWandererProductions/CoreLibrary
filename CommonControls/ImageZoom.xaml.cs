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

using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
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
        ///     The selection adorner
        /// </summary>
        private SelectionAdorner _selectionAdorner;

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
            if (BtmImage.Source == null) return;

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

            // Set GifSource and subscribe to the ImageLoaded event
            BtmImage.ImageLoaded += BtmImage_ImageLoaded;
            BtmImage.GifSource = ImageGifPath;
        }

        // Event handler for when the GIF has finished loading
        private void BtmImage_ImageLoaded(object sender, EventArgs e)
        {
            // Unsubscribe to prevent memory leaks
            BtmImage.ImageLoaded -= BtmImage_ImageLoaded;

            // Now the source is fully loaded, you can safely access it
            MainCanvas.Height = BtmImage.Source.Height;
            MainCanvas.Width = BtmImage.Source.Width;

            // Update the adorner with the new image transform
            _selectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);

            // Reattach adorner for the new image (ensures correct behavior)
            AttachAdorner(ZoomTool);
        }

        /// <summary>
        ///     Called when [image source changed].
        /// </summary>
        private void OnImageSourceChanged()
        {
            BtmImage.StopAnimation();
            BtmImage.Source = ItemsSource;

            if (BtmImage.Source == null) return;

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

            // Update the adorner with the new image transform
            _selectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);

            // Reattach adorner for new image (this ensures correct behavior for the new image)
            AttachAdorner(ZoomTool);
        }

        /// <summary>
        ///     Attaches the adorner.
        /// </summary>
        /// <param name="tool">The tool.</param>
        private void AttachAdorner(SelectionTools tool)
        {
            if (_selectionAdorner == null)
            {
                // Get the adorner layer for the BtmImage instead of the MainCanvas
                var adornerLayer = AdornerLayer.GetAdornerLayer(BtmImage);
                _selectionAdorner = new SelectionAdorner(BtmImage, tool);
                adornerLayer?.Add(_selectionAdorner);
            }
            else
            {
                // Clear points and reset for the new selection tool
                _selectionAdorner?.ClearFreeFormPoints();
                _selectionAdorner.Tool = tool; // Update the tool if necessary
            }
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

            AttachAdorner(ZoomTool); // Attach Adorner based on current tool

            switch (ZoomTool)
            {
                case SelectionTools.Move:
                case SelectionTools.SelectPixel:
                    // nothing
                    break;

                case SelectionTools.SelectRectangle:
                case SelectionTools.Erase:
                    {
                    }
                    break;
                case SelectionTools.SelectEllipse:
                    break;
                case SelectionTools.FreeForm:
                    _imageStartPoint = e.GetPosition(BtmImage);
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

            //clicked Endpoint

            switch (ZoomTool)
            {
                case SelectionTools.Move:
                    // nothing
                    break;

                case SelectionTools.SelectRectangle:
                case SelectionTools.Erase:
                    {
                        var frame = _selectionAdorner.CurrentSelectionFrame;
                        SelectedFrame?.Invoke(frame);
                    }
                    break;
                case SelectionTools.SelectPixel:
                    var endpoint = e.GetPosition(BtmImage);
                    SelectedPoint?.Invoke(endpoint);
                    break;
                default:
                    // nothing
                    return;
            }

            if (_selectionAdorner != null)
            {
                // Get the AdornerLayer for the image
                var adornerLayer = AdornerLayer.GetAdornerLayer(BtmImage);

                if (adornerLayer != null)
                {
                    // Remove the SelectionAdorner
                    adornerLayer.Remove(_selectionAdorner);
                    _selectionAdorner = null; // Clear the reference
                }
            }
        }

        /// <summary>
        ///     Handles the MouseMove event of the Canvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_mouseDown) return;

            // Get the mouse position relative to the image instead of the canvas
            var mousePos = e.GetPosition(BtmImage);

            switch (ZoomTool)
            {
                case SelectionTools.Move:
                    {
                        var position = e.GetPosition(MainCanvas);
                        var matrix = BtmImage.RenderTransform.Value;
                        matrix.OffsetX = _originPoint.X + (position.X - _startPoint.X);
                        matrix.OffsetY = _originPoint.Y + (position.Y - _startPoint.Y);
                        BtmImage.RenderTransform = new MatrixTransform(matrix);

                        _selectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);
                        break;
                    }

                case SelectionTools.SelectRectangle:
                case SelectionTools.SelectEllipse:
                    {
                        // Update the adorner for rectangle or ellipse selection
                        _selectionAdorner?.UpdateSelection(_startPoint, mousePos);

                        break;
                    }

                case SelectionTools.FreeForm:
                    {
                        // Update the adorner for free form selection by adding points
                        _selectionAdorner?.AddFreeFormPoint(mousePos);

                        break;
                    }

                case SelectionTools.SelectPixel:
                    // Handle pixel selection if needed
                    break;

                case SelectionTools.Erase:
                    {
                        // Similar to rectangle selection, but intended for erasing
                        _selectionAdorner?.UpdateSelection(_startPoint, mousePos);

                        break;
                    }

                default:
                    // Nothing
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

            _selectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);
        }
    }
}