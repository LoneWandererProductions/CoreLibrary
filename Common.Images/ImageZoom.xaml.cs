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
 * CURRENT STATUS: "State/Strategy" pattern (IToolHandler) landed - see MoveToolHandler.cs,
 * DotToolHandler.cs, GestureToolHandler.cs, ToolContext.cs, Interfaces/IToolHandler.cs,
 * Interfaces/IPreviewSource.cs. SelectionAdorner.cs is now visualization-only.
 * NEXT STEP: see the open items below - transform pipeline (3) and extended toolset (6) are
 * the two sections nothing here has touched yet.
 *
 * 1. INPUT / EVENT SYSTEM
 * -----------------------
 * [x] Move input handling (mouse down/move/up) completely into ImageZoom
 * (Done: Canvas_MouseDown/Move/Up compute a ToolContext and dispatch to the resolved
 * IToolHandler; the adorner has no mouse-event code of its own left at all)
 * [x] Add a unified input dispatcher that forwards events to the current tool
 * (Done via ResolveHandler(SelectionTool), replacing the three switch(SelectionTool) statements)
 * [~] Implement ToolContext to carry image transforms, image size, modifiers (Ctrl/Shift)
 * (ToolContext exists and is what OnMouseDown/Move/Up now take, but only carries
 * ImagePosition/CanvasPosition so far - transforms/size/modifiers can be added to it later
 * without touching IToolHandler's signature or any handler body)
 *
 *
 * 2. TOOL SYSTEM REFINEMENT (The "Switch Statement" Refactor)
 * -----------------------------------------------------------
 * [~] Introduce IToolHandler interface:
 * - OnMouseDown / OnMouseMove / OnMouseUp [x]
 * - RenderOverlay(DrawingContext dc) [ ] - not done as named; instead SelectionAdorner.OnRender
 * pulls read-only preview data from the active handler via IPreviewSource, rather than the
 * handler pushing draw calls itself. Same goal (adorner doesn't own the data), different shape -
 * a better fit for one shared adorner drawing several tools' state than per-tool render methods.
 * - GetFrame() / Reset() - Reset() [x]; GetFrame() not a named interface member, but
 * GestureToolHandler's own CaptureAndClear/GetCommittedFrames serve the same purpose informally.
 *
 * [~] Convert Rectangle, Ellipse, Dot, Trace, FreeForm into dedicated tool classes
 * (e.g., RectangleTool.cs, FreeFormTool.cs) to remove the massive switch statements.
 * (The switch statements ARE gone. Deliberately not 1:1 with the list, though: Rectangle,
 * Ellipse, FreeForm, Polygon and Trace share one GestureToolHandler, driven by the existing
 * GestureCatalog data table, instead of five near-identical classes - splitting them further
 * would have quietly reintroduced the duplication GestureCatalog already solved once. Dot got
 * its own DotToolHandler; Move its own MoveToolHandler.)
 * [x] Add ToolState / ToolSession object to store points, frame, geometry
 * (No separate ToolSession class - DotToolHandler/GestureToolHandler ARE that object, each
 * owning exactly the points/frame data its own gesture needs)
 * [x] Decouple selection logic from the adorner (Adorner should only DRAW, not hold data)
 * (Done - see section 4, same underlying change)
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
 * (Untouched by the IToolHandler/adorner work - MoveToolHandler still does the same manual
 * matrix math it always did, and SelectionAdorner.ToImageSpace/_imageTransform is still one
 * Transform used in both directions rather than two independently-stored ones)
 *
 *
 * 4. ADORNER IMPROVEMENTS
 * ------------------------
 * [x] Restrict adorner to visualization-only duties (View)
 * (Done - SelectionAdorner now holds only the image transform and Source; OnRender is its
 * only real method)
 * [x] Let adorner read data from current IToolHandler instead of owning the Point Lists
 * (Done via IPreviewSource/SelectionAdorner.Source, set by ImageZoom in AttachAdorner/
 * OnSelectionToolChanged whenever the active tool changes)
 * [ ] Add double-buffering or DrawingVisual for smoother overlay drawing (optional)
 *
 *
 * 5. IMAGE OPERATIONS (COMPLETED / INTEGRATED)
 * --------------------------------------------
 * [x] Integrate DirectBitmapImage operations (Done via ImageProcessor & ImageView)
 * [x] Add selection commit logic (Done via GestureToolHandler's private CaptureAndClear,
 * called from its own OnMouseUp - moved off SelectionAdorner along with the rest of section 4)
 * [x] Support FreeForm Polygon filling (Done via auto-close logic in RecomputeCurrentFrame,
 * GestureToolHandler's version of the old UpdateCurrentSelectionFrame)
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
 * [~] Multi-layer support (background, overlay layers), partly done when the correct data-type is loaded.
 * [ ] KNOWN GAP (found during the adorner refactor, not fixed there - a behavior change, not a
 * data-ownership move): Trace never actually accumulates points. Its GestureCatalog entry uses
 * SelectionShape.None ("bespoke, drives itself"), but nothing feeds OnMouseMove points into it -
 * the old SelectionAdorner.IsTracing flag looked like it was meant to be that connection and was
 * set in three places, but nothing ever read it, so it was dropped rather than carried forward.
 * Likely fix: give Trace SelectionShape. Freeform in GestureCatalog instead of None.
 *
 *
 * 7. PERFORMANCE & ARCHITECTURE
 * ------------------------------
 * [ ] Add invalidate throttling (Redraw only when needed)
 * [x] Clear "Ghost Frames" immediately after drawing (Done via GestureToolHandler's private
 * CaptureAndClear - see section 5's note on where this moved to)
 * [ ] Add high-DPI support for Zoom + PixelGrid alignment
 * [ ] Allow async pixel operations for large fills
 *
 * END TODO LIST
 */

// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Common.Images.Interfaces;
using Imaging.Objects;
using Imaging.Objects.Documents;
using Imaging.Objects.Interfaces;

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
        public event DelegateMultiFrame? SelectedMultiFrames;

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

                    // A pencil/eraser stroke lands as a same-sized replacement image. That's an edit in
                    // place, not "a different image": keep zoom and pan exactly where the user has them
                    // (otherwise drawing while zoomed in jumps the view back to 100% on every flush).
                    if (e.OldValue is BitmapSource oldSource
                        && oldSource.PixelWidth == newSource.PixelWidth
                        && oldSource.PixelHeight == newSource.PixelHeight
                        && DateTime.UtcNow - control.LastStrokeFlush < StrokeSwapWindow)
                    {
                        control.BtmImage.Source = newSource;
                        return;
                    }

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
        ///     A third, optional content mode alongside <see cref="ImagePath" /> (files, including GIFs) and
        ///     <see cref="ImageSource" /> (a single in-memory bitmap): a layered <see cref="Document" />, kept
        ///     as data (raster layers plus shapes - see <c>Imaging.Objects.Shapes</c>) and only ever turned
        ///     into pixels here, for display. Setting this does not touch the file system or your document
        ///     model's own undo/save - ImageZoom only ever *reads* the document (via <see cref="Document.Changed" />)
        ///     and flattens it into the same <see cref="Imaging.Gifs.ImageGif" /> this control has always used;
        ///     there is no second visual tree, no per-layer compositing, and no new tool/adorner code path.
        ///     Whoever owns the document (undo, layer add/remove, drawing onto the active layer, etc.) keeps
        ///     doing so exactly as before; ImageZoom just needs to be told to look at it.
        /// </summary>
        public static readonly DependencyProperty LayeredDocumentProperty = DependencyProperty.Register(
            nameof(LayeredDocument),
            typeof(Document),
            typeof(ImageZoom),
            new PropertyMetadata(null, OnLayeredDocumentPropertyChanged));

        /// <summary>
        ///     Gets or sets the layered document to display, or null to go back to whatever
        ///     <see cref="ImageSource" />/<see cref="ImagePath" /> is currently set to (setting this to null
        ///     does not restore the previous ImageSource/ImagePath value automatically - set one of those too
        ///     if that is what you want to show instead).
        /// </summary>
        public Document? LayeredDocument
        {
            get => (Document?)GetValue(LayeredDocumentProperty);
            set => SetValue(LayeredDocumentProperty, value);
        }

        /// <summary>
        ///     Rasterizer used to flatten any shape layers of <see cref="LayeredDocument" /> for display. Not a
        ///     dependency property - it is a rendering collaborator, not content, so it does not belong in the
        ///     "which of ImagePath/ImageSource/LayeredDocument is showing" set above. Leave it null for a
        ///     document that only has raster layers; <see cref="DocumentRenderer.Flatten" /> will throw its own
        ///     clear error if a visible shape layer with shapes turns up and no rasterizer was ever set.
        /// </summary>
        public IShapeRasterizer? ShapeRasterizer { get; set; }

        /// <summary>
        /// Called when [layered document property changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnLayeredDocumentPropertyChanged(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (sender is not ImageZoom control) return;

            if (e.OldValue is Document oldDocument)
            {
                oldDocument.Changed -= control.OnLayeredDocumentChanged;
            }

            if (e.NewValue is Document newDocument)
            {
                newDocument.Changed += control.OnLayeredDocumentChanged;

                // A brand new document being attached is "a different image", exactly like assigning a new
                // ImageSource - so it resets zoom/pan the same way (respecting LockZoom the same way too).
                control.RefreshLayeredDocument(resetZoom: !control.LockZoom);
            }
            else
            {
                control.BtmImage.StopGif();
                control.BtmImage.Source = null;
            }
        }

        /// <summary>
        /// Handles <see cref="Document.Changed" /> for the attached <see cref="LayeredDocument" />: an edit
        /// to a layer (a stroke, a shape added, a layer's opacity changed, ...) re-flattens and redraws.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DocumentChangedEventArgs"/> instance containing the event data.</param>
        private void OnLayeredDocumentChanged(object? sender, DocumentChangedEventArgs e)
        {
            if (_disposed) return;

            // Document.Changed can be raised off the UI thread (e.g. a controller drawing a stroke on a
            // background task) - marshal over before touching any DependencyObject.
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(() => OnLayeredDocumentChanged(sender, e)));
                return;
            }

            // Re-flattening the SAME document is always "this image was edited", never "a different image
            // was opened" - so, unlike ImageSource's stroke-timing heuristic above, zoom/pan is simply never
            // reset here; there is no ambiguity to resolve.
            RefreshLayeredDocument(resetZoom: false);
        }

        /// <summary>
        /// Flattens <see cref="LayeredDocument" /> and pushes the result into <see cref="BtmImage" />,
        /// mirroring what <see cref="OnImageSourcePropertyChanged" /> does for a plain <see cref="ImageSource" />
        /// (stop any running GIF, push the bitmap, keep canvas size and the selection adorner's transform
        /// in sync) but driven by <paramref name="resetZoom" /> directly instead of re-deriving it from
        /// old/new bitmap sizes - a live document's canvas size does not change from one flatten to the next.
        /// </summary>
        /// <param name="resetZoom">if set to <c>true</c> [reset zoom].</param>
        private void RefreshLayeredDocument(bool resetZoom)
        {
            var document = LayeredDocument;
            if (document is null) return;

            using var flattened = DocumentRenderer.Flatten(document, ShapeRasterizer);
            var newSource = ToBitmapSource(flattened);

            BtmImage.StopGif();

            if (!resetZoom || LockZoom)
            {
                BtmImage.Source = newSource;

                var scale = BtmImage.RenderTransform is MatrixTransform mt ? mt.Matrix.M11 : 1.0;
                MainCanvas.Width = Math.Max(newSource.Width * scale, ScrollView.ActualWidth);
                MainCanvas.Height = Math.Max(newSource.Height * scale, ScrollView.ActualHeight);
            }
            else
            {
                ResetTransforms(resetZoom: true);
                BtmImage.Source = newSource;
                MainCanvas.Height = newSource.Height;
                MainCanvas.Width = newSource.Width;
            }

            SelectionAdorner?.UpdateImageTransform(BtmImage.RenderTransform);
        }

        /// <summary>
        /// Wraps an <see cref="UnmanagedImageBuffer" /> (straight-alpha BGRA, the pixel format every raster
        /// layer and <see cref="DocumentRenderer.Flatten" />'s result already use) directly into a frozen
        /// <see cref="WriteableBitmap" /> - one pixel copy via <see cref="WriteableBitmap.WritePixels(Int32Rect,IntPtr,int,int)" />,
        /// no GDI+ involved at all (unlike the Bitmap-based <c>ToBitmapSource</c> extension used for the
        /// single-image path, this never needs a <c>System.Drawing.Bitmap</c> in between).
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>A WriteableBitmap.</returns>
        private static WriteableBitmap ToBitmapSource(UnmanagedImageBuffer buffer)
        {
            var bitmap = new WriteableBitmap(buffer.Width, buffer.Height, 96, 96, PixelFormats.Bgra32, null);

            bitmap.WritePixels(
                new Int32Rect(0, 0, buffer.Width, buffer.Height),
                buffer.Buffer,
                buffer.Width * buffer.Height * UnmanagedImageBuffer.BytesPerPixel,
                buffer.Width * UnmanagedImageBuffer.BytesPerPixel);

            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>
        /// The image clicked command property
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
        internal Point OriginPoint;

        /// <summary>
        ///     The mouse down position
        /// </summary>
        internal Point StartPoint;

        /// <summary>
        ///     Pan-drag origin, captured in <see cref="MainCanvas" /> coordinates (the same space
        ///     the render-transform's OffsetX/OffsetY live in). Kept separate from
        ///     <see cref="StartPoint" />, which is deliberately captured in <see cref="BtmImage" />
        ///     - i.e. unscaled, image-local - coordinates for the drawing tools (Rectangle,
        ///     Ellipse, FreeForm, Dot). Panning used to reuse <see cref="StartPoint" /> for this,
        ///     which mixed unscaled image-space coordinates with scaled canvas-space coordinates
        ///     in the same subtraction - correct only at exactly 100% zoom, and increasingly wrong
        ///     (erratic drag speed/direction, worse right at the pannable edges where the clamp
        ///     then fights the miscalculated delta) the further zoom moved from 1.0.
        /// </summary>
        internal Point PanStartPoint;

        /// <summary>
        ///     Points accumulated for the <see cref="ImageZoomTools.Dot" /> tool (pencil,
        ///     eraser) since the last flush to <see cref="SelectedPointCommand" />. Points
        ///     are always appended here, never dropped, and every accumulated point is
        ///     eventually included in some flush - see <see cref="FlushStroke" />.
        /// </summary>
        internal readonly List<Point> StrokeBuffer = new();

        /// <summary>
        ///     When <see cref="StrokeBuffer" /> was last flushed. Used to throttle how
        ///     often a Dot-tool drag submits a batch: mouse-move events can fire well over
        ///     a hundred times a second, and each flush costs a real image edit (a clone,
        ///     the actual draw, and a commit/redraw) - submitting one for every single move
        ///     event, rather than batching the points seen between flushes, is what made a
        ///     fast stroke feel like it was lagging behind the cursor instead of following it.
        /// </summary>
        internal DateTime LastStrokeFlush = DateTime.MinValue;

        /// <summary>
        ///     True once the current drag has submitted its first batch (see <see cref="StrokeBatch.IsFirst" />).
        /// </summary>
        internal bool StrokeHasFlushed;

        /// <summary>
        ///     A stroke's result is swapped in as a new <see cref="ImageSource" /> of identical size. Within
        ///     this window after the last flush that swap is an in-place edit and must not reset zoom/pan.
        /// </summary>
        private static readonly TimeSpan StrokeSwapWindow = TimeSpan.FromSeconds(2);

        /// <summary>
        /// The move handler field
        /// </summary>
        internal IToolHandler? MoveHandlerField;

        /// <summary>
        /// Gets the move handler.
        /// </summary>
        /// <value>
        /// The move handler.
        /// </value>
        internal IToolHandler MoveHandler => MoveHandlerField ??= new MoveToolHandler(this);

        /// <summary>
        /// The dot handler field
        /// </summary>
        private IToolHandler? _dotHandlerField;

        /// <summary>
        /// Gets the dot handler.
        /// </summary>
        /// <value>
        /// The dot handler.
        /// </value>
        internal IToolHandler DotHandler => _dotHandlerField ??= new DotToolHandler(this);

        /// <summary>
        /// The gesture handler field
        /// </summary>
        private IToolHandler? _gestureHandlerField;

        /// <summary>
        /// Gets the gesture handler.
        /// </summary>
        /// <value>
        /// The gesture handler.
        /// </value>
        internal IToolHandler GestureHandler => _gestureHandlerField ??= new GestureToolHandler(this);

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
        internal SelectionAdorner? SelectionAdorner { get; set; }

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
        public event DelegateFrame? SelectedFrame;

        /// <summary>
        ///     Occurs when [selected point].
        /// </summary>
        public event DelegatePoint? SelectedPoint;

        /// <summary>
        /// Raises the selected frame.
        /// </summary>
        /// <param name="frame">The frame.</param>
        public void RaiseSelectedFrame(SelectionFrame frame)
        {
            SelectedFrame?.Invoke(frame);
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
            var handler = control.ResolveHandler(newTool);
            control.SelectionAdorner.Source = handler as IPreviewSource;
            handler?.Reset(); // Reset any existing in-progress gesture state if applicable
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

            var handler = ResolveHandler(tool);

            // If we already have an adorner instance, keep reusing it and update its mode
            if (SelectionAdorner is { })
            {
                SelectionAdorner.Tool = tool;
                SelectionAdorner.Source = handler as IPreviewSource;
                handler?.Reset();
                SelectionAdorner.UpdateImageTransform(BtmImage.RenderTransform);
                return;
            }

            // Defensive: check whether one already exists in the layer (someone might have added it externally)
            var adorners = adornerLayer.GetAdorners(BtmImage);
            if (adorners is { })
            {
                foreach (var a in adorners)
                {
                    if (a is SelectionAdorner existing)
                    {
                        SelectionAdorner = existing;
                        SelectionAdorner.Tool = tool;
                        SelectionAdorner.Source = handler as IPreviewSource;
                        handler?.Reset();
                        SelectionAdorner.UpdateImageTransform(BtmImage.RenderTransform);
                        return;
                    }
                }
            }

            // Otherwise create and add a fresh one
            SelectionAdorner = new SelectionAdorner(BtmImage, tool) { Source = handler as IPreviewSource };
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

            // Mouse position in image-local (unscaled) space - used by the drawing tools below,
            // which all work in image pixel coordinates.
            StartPoint = e.GetPosition(BtmImage);

            AttachAdorner(SelectionTool);

            // Dispatches to whichever IToolHandler owns the current tool (see ResolveHandler and
            // ImageZoom.ToolHandlers.cs) instead of a switch(SelectionTool) here - this method used to be
            // that switch itself.
            ResolveHandler(SelectionTool)?.OnMouseDown(new ToolContext(StartPoint, e.GetPosition(MainCanvas)));
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

            ResolveHandler(SelectionTool)
                ?.OnMouseUp(new ToolContext(e.GetPosition(BtmImage), e.GetPosition(MainCanvas)));
        }

        /// <summary>
        ///     Handles the MouseMove event of the Canvas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs" /> instance containing the event data.</param>
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_mouseDown) return;

            ResolveHandler(SelectionTool)
                ?.OnMouseMove(new ToolContext(e.GetPosition(BtmImage), e.GetPosition(MainCanvas)));
        }

        /// <summary>
        ///     Resolves which handler owns a given tool. <see cref="ImageZoomTools.Move" /> and
        ///     <see cref="ImageZoomTools.Dot" /> each get their own handler; every tool registered in
        ///     <see cref="GestureCatalog" /> (Rectangle, Ellipse, FreeForm, Polygon, Trace) shares
        ///     <see cref="GestureHandler" />, since <see cref="GestureCatalog" /> is exactly what already
        ///     lets one class serve all of them without a class-per-tool switch reappearing here instead.
        ///     Null for a tool that is neither of those and not registered - i.e. nothing to dispatch to,
        ///     matching the old code's defensive "unregistered tool falls through and is ignored".
        /// </summary>
        internal IToolHandler? ResolveHandler(ImageZoomTools tool) => tool switch
        {
            ImageZoomTools.Move => MoveHandler,
            ImageZoomTools.Dot => DotHandler,
            _ when GestureCatalog.TryGet(tool, out _) => GestureHandler,
            _ => null
        };

        /// <summary>
        ///     Submits every point accumulated in <see cref="StrokeBuffer" /> since the
        ///     last flush as a single batch, then clears the buffer.
        /// </summary>
        /// <remarks>
        ///     One <see cref="SelectedPointCommand" /> execution (and therefore one
        ///     underlying image edit: one clone, one set of pixel writes, one commit) per
        ///     flush, covering however many points were seen since the last one - not one
        ///     execution per point. This is what makes a fast drag apply as a continuous
        ///     stroke instead of however many independent, comparatively expensive edits
        ///     as there were mouse-move samples.
        ///     <para>
        ///     <see cref="ViewModel.AsyncDelegateCommand{T}" /> refuses to run (and does
        ///     NOT queue) a call that arrives while a previous one is still executing -
        ///     appropriate for a button, wrong for a stream of drag samples. So this only
        ///     clears the buffer once <see cref="ICommand.CanExecute" /> confirms the
        ///     command will actually run; otherwise the accumulated points are left
        ///     in place for the next attempt rather than being silently discarded.
        ///     </para>
        /// </remarks>
        /// <returns><c>true</c> if a batch was actually submitted.</returns>
        internal bool FlushStroke(bool isLast = false)
        {
            if (StrokeBuffer.Count == 0) return true;
            if (SelectedPointCommand.CanExecute(null) != true) return false;

            var points = StrokeBuffer.ToArray();
            StrokeBuffer.Clear();
            LastStrokeFlush = DateTime.UtcNow;

            var isFirst = !StrokeHasFlushed;
            StrokeHasFlushed = !isLast;

            // A StrokeBatch is still an IReadOnlyList<Point>, so existing handlers keep working.
            SafeExecuteCommand(SelectedPointCommand, new StrokeBatch(points, isFirst, isLast));
            SelectedPoint?.Invoke(points[^1]);
            return true;
        }

        /// <summary>
        ///     Flushes the stroke at the end of a gesture (MouseUp), retrying briefly if
        ///     the command is still busy with a previous flush instead of giving up
        ///     immediately.
        /// </summary>
        /// <remarks>
        ///     During a drag, a flush that can't run yet is harmless to skip - the next
        ///     mouse-move retries it a few milliseconds later. At release there's no
        ///     later mouse-move to retry it, so silently giving up here would mean the
        ///     last few points of a stroke could vanish depending on exactly when the
        ///     mouse happened to come up relative to the in-flight flush. Bounded to
        ///     ~400ms so a genuinely stuck command can't hang the UI thread's event
        ///     handling indefinitely.
        /// </remarks>
        internal async Task FlushStrokeOnReleaseAsync()
        {
            var attempts = 0;
            while (!FlushStroke(isLast: true) && attempts++ < 40)
            {
                await Task.Delay(10);
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
            if (SelectionAdorner is { })
            {
                // Get all collected frames. GetCommittedFrames lives on GestureToolHandler now (it's the
                // only handler that supports multi-select), not on the adorner - see ImageZoom.ToolHandlers.cs.
                var frames = (GestureHandler as GestureToolHandler)?.GetCommittedFrames() ??
                             new List<SelectionFrame>();

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

                    // Unsubscribe from the attached document (if any) - it's owned by whoever set
                    // LayeredDocument, not by this control, and must outlive it.
                    if (LayeredDocument is { } document)
                    {
                        document.Changed -= OnLayeredDocumentChanged;
                    }

                    SelectedFrame = null;
                    SelectedPoint = null;

                    if (SelectedPoint is { })
                    {
                        foreach (var d in SelectedPoint.GetInvocationList())
                        {
                            SelectedPoint -= (DelegatePoint)d;
                        }
                    }

                    // Dispose image resources
                    if (BtmImage is { })
                    {
                        BtmImage.StopGif(); // Stop the timer and clear internal states safely

                        try
                        {
                            var adornerLayer = AdornerLayer.GetAdornerLayer(BtmImage);
                            if (SelectionAdorner is { }) adornerLayer?.Remove(SelectionAdorner);
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
        internal static void SafeExecuteCommand(ICommand? cmd, object? parameter)
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