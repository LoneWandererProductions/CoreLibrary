/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Common.Images
 * FILE:        ImageZoom.xaml.cs
 * PURPOSE:     Image View Control, that can handle some tools
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
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
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Common.Images
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

        /// <summary>
        /// Delegate for the Selection Frames.
        /// </summary>
        /// <param name="frames">The frames.</param>
        public delegate void DelegateMultiFrame(List<SelectionFrame> frames);

        /// <summary>
        /// Occurs when [selected multi frames].
        /// </summary>
        public event DelegateMultiFrame SelectedMultiFrames;

        /// <summary>
        /// The universal image path property (handles both static and animated images).
        /// </summary>
        public static readonly DependencyProperty ImagePathProperty = DependencyProperty.Register(
            nameof(ImagePath),
            typeof(string),
            typeof(ImageZoom),
            new PropertyMetadata(null, OnImagePathPropertyChanged));

        /// <summary>
        ///     The in-memory image source property (for edits, filters, and non-GIFs).
        /// </summary>
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            nameof(ImageSource),
            typeof(BitmapSource),
            typeof(ImageZoom),
            new PropertyMetadata(null, OnImageSourcePropertyChanged));

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
        ///     The auto play gif Property
        /// </summary>
        public static readonly DependencyProperty AutoplayGif = DependencyProperty.Register(nameof(AutoplayGifImage),
            typeof(bool),
            typeof(ImageZoom), null);

        /// <summary>
        ///     The lock zoom property. When set, switching to a new image keeps the current zoom
        ///     level *and* pan position exactly as they are instead of resetting to fit/top-left -
        ///     handy for paging through a folder of equal-sized scans (e.g. a scanned book) where
        ///     you've zoomed in to read text and want every page to land in the same spot.
        /// </summary>
        public static readonly DependencyProperty LockZoomProperty = DependencyProperty.Register(
            nameof(LockZoom), typeof(bool), typeof(ImageZoom), new PropertyMetadata(false));

        /// <summary>
        ///     The zoom scale property
        /// </summary>
        public static readonly DependencyProperty ZoomScaleProperty =
            DependencyProperty.Register(
                nameof(ZoomScale),
                typeof(double),
                typeof(ImageZoom),
                new FrameworkPropertyMetadata(
                    1.0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnZoomScaleChanged));

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
        /// Gets or sets the image path.
        /// </summary>
        /// <value>
        /// The image path.
        /// </value>
        public string? ImagePath
        {
            get => (string)GetValue(ImagePathProperty);
            set => SetValue(ImagePathProperty, value);
        }

        /// <summary>
        /// Gets or sets the in-memory ImageSource.
        /// </summary>
        /// <value>
        /// The image source.
        /// </value>
        public BitmapSource? ImageSource
        {
            get => (BitmapSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        /// <summary>
        /// Called when [image source property changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnImageSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ImageZoom control)
            {
                if (e.NewValue is BitmapSource newSource)
                {
                    control.BtmImage.StopGif(); // Stop any running GIF

                    // Reset zoom/pan before swapping in the new image - unless the user has asked
                    // to keep it locked (e.g. paging through a folder of equal-sized scans, where
                    // landing on the same zoomed-in spot on every page is exactly the point).
                    // OnImagePathChanged (the GIF path) has the same reset+lock handling below, but
                    // this - the far more common, static-image path - never reset at all: the old
                    // zoom level and scroll/pan position just carried straight over onto whatever
                    // image loaded next, which is why zooming/panning one image and then switching
                    // left the view looking completely off on the next one.
                    if (!control.LockZoom)
                    {
                        control.ResetTransforms(resetZoom: true);

                        control.BtmImage.Source = newSource; // Push the edited bitmap to the core image control

                        // Instantly update canvas boundaries to match the newly generated bitmap
                        control.MainCanvas.Height = newSource.Height;
                        control.MainCanvas.Width = newSource.Width;
                    }
                    else
                    {
                        control.BtmImage.Source = newSource;

                        // Keep the canvas sized for the CURRENT zoom level, not just the raw image
                        // size - otherwise the scrollable area would shrink back down to 1:1 even
                        // though the render transform is still scaled up, clipping/misaligning the
                        // locked-in view. Mirrors the sizing logic in Canvas_MouseWheel.
                        var scale = control.BtmImage.RenderTransform is MatrixTransform mt ? mt.Matrix.M11 : 1.0;
                        control.MainCanvas.Width =
                            Math.Max(newSource.Width * scale, control.ScrollView.ActualWidth);
                        control.MainCanvas.Height =
                            Math.Max(newSource.Height * scale, control.ScrollView.ActualHeight);
                    }

                    control.SelectionAdorner?.UpdateImageTransform(control.BtmImage.RenderTransform);
                }
                else
                {
                    control.BtmImage.Source = null;
                }
            }
        }

        /// <summary>
        /// Called when [image path property changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void OnImagePathPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ImageZoom control)
            {
                control.OnImagePathChanged();
            }
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

        /// <summary>
        ///     Gets or sets a value indicating whether the current zoom level and pan position
        ///     should be kept when switching to a different image, instead of resetting.
        /// </summary>
        /// <value>
        ///   <c>true</c> to keep zoom/pan locked across images; otherwise, <c>false</c>.
        /// </value>
        public bool LockZoom
        {
            get => (bool)GetValue(LockZoomProperty);
            set => SetValue(LockZoomProperty, value);
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
        /// Called when [image source GIF changed].
        /// </summary>
        private void OnImagePathChanged()
        {
            if (string.IsNullOrEmpty(ImagePath))
            {
                BtmImage.StopGif();
                BtmImage.Source = null;
                return;
            }

            // Reset position + scroll - unless zoom is locked (see LockZoom / OnImageSourcePropertyChanged
            // for the full explanation). BtmImage_ImageLoaded below handles the matching canvas-size
            // side of the same lock once the GIF has actually finished loading.
            if (!LockZoom)
            {
                ResetTransforms(resetZoom: true);
            }

            // Pass the path to ImageGif. It will automatically figure out if it's a GIF or a static image.
            BtmImage.GifSource = ImagePath;

            // Note: SelectionAdorner and Canvas sizing will be updated automatically 
            // when BtmImage_ImageLoaded fires!
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
            if (!LockZoom)
            {
                MainCanvas.Height = BtmImage.Source.Height;
                MainCanvas.Width = BtmImage.Source.Width;
            }
            else
            {
                // Keep the canvas sized for the CURRENT zoom level rather than the raw frame size -
                // see OnImageSourcePropertyChanged for why.
                var scale = BtmImage.RenderTransform is MatrixTransform mt ? mt.Matrix.M11 : 1.0;
                MainCanvas.Width = Math.Max(BtmImage.Source.Width * scale, ScrollView.ActualWidth);
                MainCanvas.Height = Math.Max(BtmImage.Source.Height * scale, ScrollView.ActualHeight);
            }

            // Update the adorner with the new image transform
            SelectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);

            // Reattach adorner for the new image (ensures correct behavior)
            AttachAdorner(SelectionTool);

            // Ensure the adorner updates with the new zoom scale
            SelectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);
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
            _ = MainCanvas.CaptureMouse();

            // Get the mouse position relative to the image (consistent with panning logic)
            //var rawPoint = e.GetPosition(BtmImage);

            //_startPoint = e.GetPosition(MainCanvas);
            _startPoint = e.GetPosition(BtmImage);

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
                case ImageZoomTools.Dot:
                    SelectionAdorner?.UpdateSelection(_startPoint, _startPoint);
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

            // 🔴 1. NEW: Handle Point-based tools (Pencil, Color Picker) explicitly
            if (SelectionTool == ImageZoomTools.Dot)
            {
                SelectionAdorner.CaptureAndClear(); // Clear the red dot visual

                // Fire the Point command to ImageView
                SafeExecuteCommand(SelectedPointCommand, _startPoint);
                SelectedPoint?.Invoke(_startPoint);

                return; // Exit early!
            }

            // 2. Identify "Immediate Action" tools (Shapes, Frames)
            // 🔴 REMOVED 'ImageZoomTools.Dot' from this list
            var isDrawingTool = SelectionTool == ImageZoomTools.Rectangle ||
                                SelectionTool == ImageZoomTools.Ellipse ||
                                SelectionTool == ImageZoomTools.FreeForm ||
                                SelectionTool == ImageZoomTools.Trace;

            if (isDrawingTool)
            {
                // 3. Capture the data AND Clear the visuals immediately
                var frame = SelectionAdorner.CaptureAndClear();

                // 4. Validation: Ensure we actually drew something substantial
                var isValid = (frame.Width > 0 && frame.Height > 0) ||
                              frame.Points is { Count: > 0 };

                if (isValid)
                {
                    // 5. Fire the Command to the ViewModel (Update the Bitmap)
                    SafeExecuteCommand(SelectedFrameCommand, frame);

                    // 6. Fire the Event (if anything else is listening)
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
            if (!_mouseDown) return;

            var mousePos = e.GetPosition(BtmImage);

            switch (SelectionTool)
            {
                case ImageZoomTools.Move:
                {
                    var currentCanvasPos = e.GetPosition(MainCanvas);
                    var transform = (MatrixTransform)BtmImage.RenderTransform;
                    var matrix = transform.Matrix;

                    // 1. Calculate intended new offsets
                    var newX = _originPoint.X + (currentCanvasPos.X - _startPoint.X);
                    var newY = _originPoint.Y + (currentCanvasPos.Y - _startPoint.Y);

                    // 2. Boundary Checks
                    var viewWidth = ScrollView.ActualWidth;
                    var viewHeight = ScrollView.ActualHeight;
                    var tWidth = BtmImage.ActualWidth * matrix.M11;
                    var tHeight = BtmImage.ActualHeight * matrix.M22;

                    // 3. Clamp panning only if image is larger than view
                    if (tWidth > viewWidth)
                        matrix.OffsetX = Math.Max(Math.Min(newX, 0), viewWidth - tWidth);
                    if (tHeight > viewHeight)
                        matrix.OffsetY = Math.Max(Math.Min(newY, 0), viewHeight - tHeight);

                    BtmImage.RenderTransform = new MatrixTransform(matrix);
                    SelectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);
                    break;
                }

                case ImageZoomTools.Rectangle:
                case ImageZoomTools.Ellipse:
                    SelectionAdorner?.UpdateSelection(_startPoint, mousePos);
                    break;

                case ImageZoomTools.FreeForm:
                    SelectionAdorner?.AddFreeFormPoint(mousePos);
                    break;
            }
        }

        /// <summary>
        ///     Handles the MouseWheel event of the Grid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseWheelEventArgs" /> instance containing the event data.</param>
        /// <summary>
        /// Handles zooming with mouse wheel (zoom to cursor, clamped to viewport)
        /// </summary>
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (BtmImage.RenderTransform is not MatrixTransform transform)
                return;

            var mousePos = e.GetPosition(MainCanvas);

            var zoomFactor = e.Delta > 0 ? 1.1 : 1 / 1.1;

            var matrix = transform.Matrix;

            matrix.ScaleAt(zoomFactor, zoomFactor, mousePos.X, mousePos.Y);


            var viewWidth = ScrollView.ActualWidth;
            var viewHeight = ScrollView.ActualHeight;

            var transformedWidth = BtmImage.ActualWidth * matrix.M11;
            var transformedHeight = BtmImage.ActualHeight * matrix.M22;

            var minX = Math.Min(0, viewWidth - transformedWidth);
            var minY = Math.Min(0, viewHeight - transformedHeight);

            matrix.OffsetX = Math.Clamp(matrix.OffsetX, minX, 0);
            matrix.OffsetY = Math.Clamp(matrix.OffsetY, minY, 0);

            BtmImage.RenderTransform = new MatrixTransform(matrix);

            MainCanvas.Width = Math.Max(transformedWidth, viewWidth);
            MainCanvas.Height = Math.Max(transformedHeight, viewHeight);


            SelectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);

            e.Handled = true;
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
        ///     Updates the zoom scale for both ScaleX and ScaleY.
        /// </summary>
        /// <param name="zoomScale">The new zoom scale.</param>
        private void UpdateZoomScale(double zoomScale)
        {
            var matrix = ((MatrixTransform)BtmImage.RenderTransform).Matrix;

            matrix.M11 = zoomScale;
            matrix.M22 = zoomScale;

            BtmImage.RenderTransform = new MatrixTransform(matrix);

            MainCanvas.Width = BtmImage.ActualWidth * zoomScale;
            MainCanvas.Height = BtmImage.ActualHeight * zoomScale;

            SelectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);
        }

        /// <summary>
        /// Resets the Transformations.
        /// reset position + scroll
        /// </summary>
        private void ResetTransforms(bool resetZoom = true)
        {
            // 1. Reset Matrix
            var matrix = Matrix.Identity;

            if (!resetZoom)
            {
                matrix.M11 = ZoomScale;
                matrix.M22 = ZoomScale;
            }

            // Actually apply the matrix - this used to be computed and then silently dropped,
            // meaning ResetTransforms never really reset anything: the old zoom/pan stuck around
            // on the render transform no matter what was calculated above.
            BtmImage.RenderTransform = new MatrixTransform(matrix);

            // 2. Sync Sizes (Use ActualWidth if loaded, otherwise Source width)
            var baseWidth = BtmImage.ActualWidth > 0 ? BtmImage.ActualWidth : (BtmImage.Source?.Width ?? 0);
            var baseHeight = BtmImage.ActualHeight > 0 ? BtmImage.ActualHeight : (BtmImage.Source?.Height ?? 0);

            MainCanvas.Width = Math.Max(baseWidth, ScrollView.ActualWidth);
            MainCanvas.Height = Math.Max(baseHeight, ScrollView.ActualHeight);

            // 3. Reset Viewport
            ScrollView.ScrollToHome();
            ScrollView.UpdateLayout();

            SelectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);
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
                    SelectedFrame = null;
                    SelectedPoint = null;

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
                        BtmImage.StopGif(); // Stop the timer and clear internal states safely

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