/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls.Images
 * FILE:        ImageZoom.xaml.cs
 * PURPOSE:     Image View Control, that can handle some tools
 * PROGRAMER:   Peter Geinitz (Wayfarer)
*/

// ReSharper disable EventNeverSubscribedTo.Global, only used outside of the dll
// ReSharper disable MemberCanBeInternal, must be visible, if we want to use it outside of the dll
// ReSharper disable UnusedType.Global
// ReSharper disable MissingSpace

/*
 * TODO — ImageZoom & SelectionAdorner Architecture Roadmap
 * --------------------------------------------------------
 * CURRENT STATUS: Functional "Monolithic" Pattern.
 * NEXT STEP: Refactor to "State/Strategy" Pattern (IToolHandler).
 *
 * 1. INPUT / EVENT SYSTEM
 * -----------------------
 * [ ] Move input handling (mouse down/move/up) completely into ImageZoom
 * (currently shared/delegated to Adorner)
 * [ ] Add a unified input dispatcher that forwards events to the current tool
 * [ ] Implement ToolContext to carry image transforms, image size, modifiers (Ctrl/Shift)
 *
 *
 * 2. TOOL SYSTEM REFINEMENT (The "Switch Statement" Refactor)
 * -----------------------------------------------------------
 * [ ] Introduce IToolHandler interface:
 * - OnMouseDown / OnMouseMove / OnMouseUp
 * - RenderOverlay(DrawingContext dc)
 * - GetFrame() / Reset()
 *
 * [ ] Convert Rectangle, Ellipse, Dot, Trace, FreeForm into dedicated tool classes
 * (e.g., RectangleTool.cs, FreeFormTool.cs) to remove the massive switch statements.
 * [ ] Add ToolState / ToolSession object to store points, frame, geometry
 * [ ] Decouple selection logic from the adorner (Adorner should only DRAW, not hold data)
 *
 *
 * 3. TRANSFORM PIPELINE
 * ---------------------
 * [ ] Replace manual Matrix math with a TransformGroup:
 * - ZoomTransform
 * - ScrollOffsetTransform
 * [ ] Store both:
 * - DrawingTransform (image → screen)
 * - InputTransform   (screen → image/pixel space)
 * [-] Update SelectionFrame to always use pixel-space coordinates
 * (Currently handled via conversion in ImageProcessor.FillArea, but Frame itself stores WPF Points)
 *
 *
 * 4. ADORNER IMPROVEMENTS
 * ------------------------
 * [ ] Restrict adorner to visualization-only duties (View)
 * [ ] Let adorner read data from current IToolHandler instead of owning the Point Lists
 * [ ] Add double-buffering or DrawingVisual for smoother overlay drawing (optional)
 *
 *
 * 5. IMAGE OPERATIONS (COMPLETED / INTEGRATED)
 * --------------------------------------------
 * [x] Integrate DirectBitmapImage operations (Done via ImageProcessor & ImageView)
 * [x] Add selection commit logic (Done via SelectionAdorner.CaptureAndClear & Canvas_MouseUp)
 * [x] Support FreeForm Polygon filling (Done via auto-close logic in CaptureAndClear)
 * [ ] Add support for brush size/hardness visualization in the Adorner
 * [ ] Add pixel-snapping modes (whole pixel alignment when zoomed)
 *
 *
 * 6. EXTENDED TOOLSET (FUTURE)
 * ----------------------------
 * [ ] Polygonal lasso tool (Click-to-add-point)
 * [ ] Magic-wand / flood-fill selection (using existing flood-fill helper)
 * [ ] Text tool (typed overlay rendered to bitmap)
 * [ ] Stamp/cloning tool
 * [ ] Multi-layer support (background, overlay layers)
 *
 *
 * 7. PERFORMANCE & ARCHITECTURE
 * ------------------------------
 * [ ] Add invalidate throttling (Redraw only when needed)
 * [x] Clear "Ghost Frames" immediately after drawing (Done via CaptureAndClear)
 * [ ] Add high-DPI support for Zoom + PixelGrid alignment
 * [ ] Allow async pixel operations for large fills
 *
 * END TODO LIST
 */


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommonControls.Images
{
    /// <inheritdoc cref="Window" />
    /// <summary>
    ///     ImageZoom Image
    /// </summary>
    public sealed partial class ImageZoom : IDisposable
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

        public delegate void DelegateMultiFrame(List<SelectionFrame> frames);

        public event DelegateMultiFrame SelectedMultiFrames;

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
        ///     The tools
        /// </summary>
        public static readonly DependencyProperty SelectionToolProperty =
            DependencyProperty.Register(
                nameof(SelectionTool),
                typeof(ImageZoomTools),
                typeof(ImageZoom),
                new PropertyMetadata(OnSelectionToolChanged));

        /// <summary>
        ///     The autoplay gif Property
        /// </summary>
        public static readonly DependencyProperty AutoplayGif = DependencyProperty.Register(nameof(AutoplayGifImage),
            typeof(bool),
            typeof(ImageZoom), null);

        /// <summary>
        ///     The zoom scale property
        /// </summary>
        public static readonly DependencyProperty ZoomScaleProperty = DependencyProperty.Register(
            nameof(ZoomScale),
            typeof(double),
            typeof(ImageZoom),
            new PropertyMetadata(1.0, OnZoomScaleChanged));

        /// <summary>
        ///      The selected multi-frames command property
        /// </summary>
        public static readonly DependencyProperty SelectedMultiFramesCommandProperty =
            DependencyProperty.Register(nameof(SelectedMultiFramesCommand), typeof(ICommand), typeof(ImageZoom),
                new PropertyMetadata(null));

        /// <summary>
        ///      Gets or sets the selected multi-frames command.
        /// </summary>
        public ICommand SelectedMultiFramesCommand
        {
            get => (ICommand)GetValue(SelectedMultiFramesCommandProperty);
            set => SetValue(SelectedMultiFramesCommandProperty, value);
        }

        /// <summary>
        ///     The image clicked command property
        /// </summary>
        public static readonly DependencyProperty SelectedPointCommandProperty = DependencyProperty.Register(
            nameof(SelectedPointCommand), typeof(ICommand), typeof(ImageZoom), new PropertyMetadata(null));

        /// <summary>
        ///     The selected frame property
        /// </summary>
        public static readonly DependencyProperty SelectedFrameCommandProperty =
            DependencyProperty.Register(nameof(SelectedFrameCommand), typeof(ICommand), typeof(ImageZoom),
                new PropertyMetadata(null));

        /// <summary>
        ///     The selected free form points command property
        /// </summary>
        public static readonly DependencyProperty SelectedFreeFormPointsCommandProperty =
            DependencyProperty.Register(nameof(SelectedFreeFormPointsCommand), typeof(ICommand), typeof(ImageZoom),
                new PropertyMetadata(null));

        /// <summary>
        ///     The lock object used for monitor locking (thread-safety).
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        ///     The disposed
        /// </summary>
        private bool _disposed;

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
        ///     The selection adorner
        /// </summary>
        private SelectionAdorner SelectionAdorner { get; set; }

        /// <summary>
        ///     Gets or sets the image clicked command.
        /// </summary>
        /// <value>
        ///     The image clicked command.
        /// </value>
        public ICommand SelectedPointCommand
        {
            get => (ICommand)GetValue(SelectedPointCommandProperty);
            set => SetValue(SelectedPointCommandProperty, value);
        }

        /// <summary>
        ///     Gets or sets the selected frame.
        /// </summary>
        /// <value>
        ///     The selected frame.
        /// </value>
        public ICommand SelectedFrameCommand
        {
            get => (ICommand)GetValue(SelectedFrameCommandProperty);
            set => SetValue(SelectedFrameCommandProperty, value);
        }

        /// <summary>
        ///     Gets or sets the selected free form points command.
        /// </summary>
        /// <value>
        ///     The selected free form points command.
        /// </value>
        public ICommand SelectedFreeFormPointsCommand
        {
            get => (ICommand)GetValue(SelectedFreeFormPointsCommandProperty);
            set => SetValue(SelectedFreeFormPointsCommandProperty, value);
        }

        /// <summary>
        ///     Gets or sets the zoom scale.
        /// </summary>
        public double ZoomScale
        {
            get => (double)GetValue(ZoomScaleProperty);
            set => SetValue(ZoomScaleProperty, value);
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
        public string? ImageGifPath
        {
            get => (string)GetValue(ImageGifSourceProperty);
            set => SetValue(ImageGifSourceProperty, value);
        }

        /// <summary>
        ///     Gets or sets the zoom.
        /// </summary>
        /// <value>
        ///     The zoom.
        /// </value>
        public ImageZoomTools SelectionTool
        {
            get => (ImageZoomTools)GetValue(SelectionToolProperty);
            set => SetValue(SelectionToolProperty, value);
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

        /// <inheritdoc />
        /// <summary>
        ///     Implementation of IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // Prevent finalizer from running.
        }

        /// <summary>
        ///     Called when the ZoomScale property changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void OnZoomScaleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ImageZoom;
            control?.UpdateZoomScale((double)e.NewValue);
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
        ///     Called when [selection tool changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private static void OnSelectionToolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ImageZoom control)
            {
                return; // Ensure that we are working with an ImageZoom instance
            }

            var newTool = (ImageZoomTools)e.NewValue;

            // Detach the previous adorner if needed
            if (control.SelectionAdorner == null)
            {
                return;
            }

            control.SelectionAdorner.Tool = newTool; // Update the tool in the adorner
            control.SelectionAdorner.ClearFreeFormPoints(); // Reset any existing free-form points if applicable
        }

        /// <summary>
        ///     Called when [image source GIF changed].
        /// </summary>
        private void OnImageSourceGifChanged()
        {
            if (string.IsNullOrEmpty(ImageGifPath) || !File.Exists(ImageGifPath))
            {
                return;
            }

            // reset position + scroll
            ResetTransforms();

            BtmImage.GifSource = ImageGifPath;

            SelectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);
        }

        /// <summary>
        ///     Handles the ImageLoaded event of the BtmImage control.
        ///     Event handler for when the GIF has finished loading
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void BtmImage_ImageLoaded(object sender, EventArgs e)
        {
            // Now the source is fully loaded, you can safely access it
            MainCanvas.Height = BtmImage.Source.Height;
            MainCanvas.Width = BtmImage.Source.Width;

            // Update the adorner with the new image transform
            SelectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);

            // Reattach adorner for the new image (ensures correct behavior)
            AttachAdorner(SelectionTool);

            // Ensure the adorner updates with the new zoom scale
            SelectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);
        }

        /// <summary>
        ///     Called when [image source changed].
        /// </summary>
        private void OnImageSourceChanged()
        {
            //// Prevent any GIF from reasserting itself
            //BtmImage.GifSource = null;
            //// Force WPF to process the null assignment immediately
            //Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);

            BtmImage.StopGif();

            // this block aboe is absolutey necessary to avoid that WPF keeps the old GIF playing

            // Load ItemsSource via stream to avoid WPF URI cache if possible
            if (ItemsSource != null && !string.IsNullOrEmpty(ItemsSource.UriSource?.OriginalString))
            {
                // Best: create a new BitmapImage from a stream on demand
                try
                {
                    var uri = ItemsSource.UriSource;
                    var path = uri.IsAbsoluteUri ? uri.LocalPath : uri.OriginalString;
                    using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = fs;
                    bmp.EndInit();
                    bmp.Freeze();
                    BtmImage.Source = bmp;
                }
                catch
                {
                    // fallback to existing ItemsSource
                    BtmImage.Source = ItemsSource;
                }
            }
            else
            {
                // ItemsSource is already a BitmapImage created in-memory
                BtmImage.Source = ItemsSource;
            }

            if (BtmImage.Source == null)
                return;

            // reset position + scroll
            ResetTransforms();

            MainCanvas.Height = BtmImage.Source.Height;
            MainCanvas.Width = BtmImage.Source.Width;

            SelectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);
            AttachAdorner(SelectionTool);
        }

        /// <summary>
        ///     Attaches the adorner.
        /// </summary>
        /// <param name="tool">The tool.</param>
        private void AttachAdorner(ImageZoomTools tool)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(BtmImage);
            if (adornerLayer == null)
                return;

            // If we already have an adorner instance, keep reusing it and update its mode
            if (SelectionAdorner != null)
            {
                SelectionAdorner.Tool = tool;
                SelectionAdorner.ClearFreeFormPoints();
                SelectionAdorner.UpdateImageTransform(BtmImage.RenderTransform);
                return;
            }

            // Defensive: check whether one already exists in the layer (someone might have added it externally)
            var adorners = adornerLayer.GetAdorners(BtmImage);
            if (adorners != null)
            {
                foreach (var a in adorners)
                {
                    if (a is SelectionAdorner existing)
                    {
                        SelectionAdorner = existing;
                        SelectionAdorner.Tool = tool;
                        SelectionAdorner.ClearFreeFormPoints();
                        SelectionAdorner.UpdateImageTransform(BtmImage.RenderTransform);
                        return;
                    }
                }
            }

            // Otherwise create and add a fresh one
            SelectionAdorner = new SelectionAdorner(BtmImage, tool);
            adornerLayer.Add(SelectionAdorner);
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

            // Get the mouse position relative to the image (consistent with panning logic)
            var rawPoint = e.GetPosition(BtmImage);

            //TODO problem with our DPI and multiple Monitor Setup
            _startPoint = rawPoint;

            // Capture the mouse
            _ = MainCanvas.CaptureMouse();

            AttachAdorner(SelectionTool);

            // If this is a pan start, capture origin offsets (in image transform space)
            if (SelectionTool == ImageZoomTools.Move)
            {
                // Capture the current image transform offset as the origin for panning
                var matrix = BtmImage.RenderTransform.Value;
                _originPoint = new Point(matrix.OffsetX, matrix.OffsetY);
            }

            switch (SelectionTool)
            {
                case ImageZoomTools.Move:
                    break;
                case ImageZoomTools.Trace:
                    SelectionAdorner.IsTracing = true;
                    break;
                case ImageZoomTools.Rectangle:
                case ImageZoomTools.Ellipse:
                case ImageZoomTools.FreeForm:
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Handles the MouseUp event of the Grid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mouseDown = false;
            MainCanvas.ReleaseMouseCapture();

            if (SelectionAdorner == null) return;

            // 1. Identify "Immediate Action" tools (Drawing tools)
            bool isDrawingTool = SelectionTool == ImageZoomTools.Rectangle ||
                                 SelectionTool == ImageZoomTools.Ellipse ||
                                 SelectionTool == ImageZoomTools.FreeForm ||
                                 SelectionTool == ImageZoomTools.Trace ||
                                 SelectionTool == ImageZoomTools.Dot;

            if (isDrawingTool)
            {
                // 2. Capture the data AND Clear the visuals immediately
                // This prevents the "Ghost Frame" from sticking around
                SelectionFrame frame = SelectionAdorner.CaptureAndClear();

                // 3. Validation: Ensure we actually drew something substantial
                bool isValid = (frame.Width > 0 && frame.Height > 0) ||
                               frame.Points is { Count: > 0 };

                if (isValid)
                {
                    // 4. Fire the Command to the ViewModel (Update the Bitmap)
                    SafeExecuteCommand(SelectedFrameCommand, frame);

                    // 5. Fire the Event (if anything else is listening)
                    SelectedFrame?.Invoke(frame);
                }
            }
            else if (SelectionTool == ImageZoomTools.Move)
            {
                // Just release capture, do nothing else
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

            // Get the mouse position relative to the image instead of the canvas
            var mousePos = e.GetPosition(BtmImage);

            switch (SelectionTool)
            {
                case ImageZoomTools.Move:
                {
                    // Use the image coordinate space so panning respects the current image transform/zoom
                    var position = e.GetPosition(BtmImage);
                    var matrix = BtmImage.RenderTransform.Value;
                    matrix.OffsetX = _originPoint.X + (position.X - _startPoint.X);
                    matrix.OffsetY = _originPoint.Y + (position.Y - _startPoint.Y);
                    BtmImage.RenderTransform = new MatrixTransform(matrix);

                    SelectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);
                    break;
                }

                case ImageZoomTools.Rectangle:
                case ImageZoomTools.Ellipse:
                {
                    // Update the adorner for rectangle or ellipse selection
                    SelectionAdorner?.UpdateSelection(_startPoint, mousePos);

                    break;
                }

                case ImageZoomTools.FreeForm:
                {
                    // Update the adorner for free form selection by adding points
                    SelectionAdorner?.AddFreeFormPoint(mousePos);

                    break;
                }

                case ImageZoomTools.Trace:
                    // Handle pixel selection if needed
                    break;
                case ImageZoomTools.Dot:
                    break;
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
            lock (_lock)
            {
                var zoomFactor = e.Delta > 0 ? 1.1 : 1 / 1.1;
                var newZoomScale = Scale.ScaleX * zoomFactor; // Assume uniform scaling, so use ScaleX

                UpdateZoomScale(newZoomScale); // Centralize logic for updating the zoom scale

                // Ensure the adorner updates with the new zoom scale
                SelectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);
            }
        }

        /// <summary>
        ///     Handles the MouseRightButtonUp event of the Canvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 1. If we are currently dragging (Mouse Left is Down), cancel the current shape
            if (_mouseDown)
            {
                _mouseDown = false;
                MainCanvas.ReleaseMouseCapture();
                // Optional: reset current points in Adorner without committing
                // SelectionAdorner.ResetCurrent();
                return;
            }

            // 2. If we are idle, Right Click means "I am finished selecting"
            if (SelectionAdorner != null)
            {
                // Get all collected frames
                var frames = SelectionAdorner.GetCommittedFrames();

                if (frames.Count > 0)
                {
                    // Fire event with list of frames
                    SelectedMultiFrames?.Invoke(frames);
                    // Execute command if you have a List version of the command
                    // SafeExecuteCommand(SelectedMultiFramesCommand, frames);
                }

                // Cleanup
                var adornerLayer = AdornerLayer.GetAdornerLayer(BtmImage);
                adornerLayer?.Remove(SelectionAdorner);
                SelectionAdorner = null;

                // Optional: Reset tool to Move automatically?
                // SelectionTool = ImageZoomTools.Move;
            }
        }

        /// <summary>
        ///     Completes the free form selection.
        /// </summary>
        private void CompleteFreeFormSelection()
        {
            var frame = SelectionAdorner.CurrentSelectionFrame;
            SelectedFrame?.Invoke(frame); // Notify listeners that selection is done

            SafeExecuteCommand(SelectedFrameCommand, frame);

            SelectionAdorner.FreeFormPoints.Clear(); // Reset collected points for the next freeform drawing
        }

        /// <summary>
        ///     Updates the zoom scale for both ScaleX and ScaleY.
        /// </summary>
        /// <param name="zoomScale">The new zoom scale.</param>
        private void UpdateZoomScale(double zoomScale)
        {
            Scale.ScaleX = zoomScale;
            Scale.ScaleY = zoomScale;

            // Ensure the adorner updates with the new zoom scale
            SelectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);
        }

        /// <summary>
        ///     Sets the clicked point.
        /// </summary>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        private void SetClickedPoint(MouseEventArgs e)
        {
            var endpoint = e.GetPosition(BtmImage);
            SelectedPoint?.Invoke(endpoint);
            SafeExecuteCommand(SelectedPointCommand, endpoint);
        }

        /// <summary>
        /// Resets the Transformations.
        /// reset position + scroll
        /// </summary>
        private void ResetTransforms()
        {
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
        }

        /// <summary>
        ///     Clean up managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">Whether the method was called by Dispose or the finalizer.</param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return; // Early exit if already disposed
            }

            lock (_lock) // Ensure thread-safety
            {
                if (_disposed)
                {
                    return; // Double-check in case Dispose was called by another thread
                }

                if (disposing)
                {
                    // Managed resource cleanup

                    // Unsubscribe event handlers
                    if (SelectedFrame != null)
                    {
                        foreach (var d in SelectedFrame.GetInvocationList())
                        {
                            SelectedFrame -= (DelegateFrame)d;
                        }
                    }

                    if (SelectedPoint != null)
                    {
                        foreach (var d in SelectedPoint.GetInvocationList())
                        {
                            SelectedPoint -= (DelegatePoint)d;
                        }
                    }

                    // Dispose image resources
                    if (BtmImage != null)
                    {
                        BtmImage.Source = null;
                        BtmImage.GifSource = null;

                        // Remove adorner
                        try
                        {
                            var adornerLayer = AdornerLayer.GetAdornerLayer(BtmImage);
                            adornerLayer?.Remove(SelectionAdorner);
                        }
                        catch
                        {
                            // swallow, best-effort
                        }
                    }

                    SelectionAdorner = null;

                    // Release UI interaction resources
                    try
                    {
                        MainCanvas.ReleaseMouseCapture();
                    }
                    catch
                    {
                        // swallow
                    }

                    // Best-effort unsubscribe of common UI events (prevents memory leaks)
                    try
                    {
                        MainCanvas.MouseDown -= Canvas_MouseDown;
                        MainCanvas.MouseUp -= Canvas_MouseUp;
                        MainCanvas.MouseMove -= Canvas_MouseMove;
                        MainCanvas.MouseWheel -= Canvas_MouseWheel;
                        MainCanvas.MouseRightButtonUp -= Canvas_MouseRightButtonUp;

                        // If BtmImage exposes ImageLoaded event, unsubscribe
                        //BtmImage.ImageLoaded -= BtmImage_ImageLoaded;
                    }
                    catch
                    {
                        // swallow any unsubscribe exceptions
                    }
                }

                // If there are unmanaged resources, clean them here

                _disposed = true; // Mark as disposed
            }
        }

        /// <summary>
        ///     Finalizes an instance of the <see cref="ImageZoom" /> class.
        /// </summary>
        ~ImageZoom()
        {
            Dispose(false); // Finalizer calls Dispose(false)
        }

        #region Helpers

        /// <summary>
        /// Safely executes a command if it exists and can execute.
        /// </summary>
        /// <param name="cmd">The command to execute.</param>
        /// <param name="parameter">The parameter to pass.</param>
        private static void SafeExecuteCommand(ICommand? cmd, object? parameter)
        {
            if (cmd == null) return;

            try
            {
                if (cmd.CanExecute(parameter))
                {
                    cmd.Execute(parameter);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Command execution failed: {ex}");
            }
        }

        #endregion
    }
}
