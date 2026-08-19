/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Polaris.cs
 * PURPOSE:     Editor Control
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using RenderEngine;

namespace Solaris
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///     Generate a playing field for the editor
    /// </summary>
    public sealed partial class Polaris
    {
        #region Dependency Properties

        /// <summary>
        /// The polaris height property
        /// </summary>
        public static readonly DependencyProperty PolarisHeightProperty = DependencyProperty.Register(
            nameof(PolarisHeight), typeof(int), typeof(Polaris), new PropertyMetadata(100));

        /// <summary>
        /// The polaris width property
        /// </summary>
        public static readonly DependencyProperty PolarisWidthProperty = DependencyProperty.Register(
            nameof(PolarisWidth), typeof(int), typeof(Polaris), new PropertyMetadata(100));

        /// <summary>
        /// The polaris texture size property
        /// </summary>
        public static readonly DependencyProperty PolarisTextureSizeProperty = DependencyProperty.Register(
            nameof(PolarisTextureSize), typeof(int), typeof(Polaris), new PropertyMetadata(100));

        /// <summary>
        /// The polaris textures property
        /// </summary>
        public static readonly DependencyProperty PolarisTexturesProperty = DependencyProperty.Register(
            nameof(PolarisTextures), typeof(Dictionary<int, Texture>), typeof(Polaris), new PropertyMetadata(null));

        // Note the added Callbacks for properties that trigger visual updates

        /// <summary>
        /// The polaris map property
        /// </summary>
        public static readonly DependencyProperty PolarisMapProperty = DependencyProperty.Register(
            nameof(PolarisMap), typeof(Dictionary<int, List<int>>), typeof(Polaris),
            new PropertyMetadata(null, OnMapChanged));

        /// <summary>
        /// The polaris grid property
        /// </summary>
        public static readonly DependencyProperty PolarisGridProperty = DependencyProperty.Register(
            nameof(PolarisGrid), typeof(bool), typeof(Polaris),
            new PropertyMetadata(false, OnGridChanged));

        /// <summary>
        /// The polaris number property
        /// </summary>
        public static readonly DependencyProperty PolarisNumberProperty = DependencyProperty.Register(
            nameof(PolarisNumber), typeof(bool), typeof(Polaris),
            new PropertyMetadata(false, OnNumberChanged));

        #endregion

        /// <summary>
        /// The lock
        /// </summary>
        private readonly Lock _lock = new();

        /// <summary>
        /// The active dirty flags indicating invalid layers.
        /// </summary>
        private DirtyFlags _dirtyFlags = DirtyFlags.None;

        /// <summary>
        /// Queue of specific tile indices that need sub-region re-blitting.
        /// </summary>
        private readonly HashSet<int> _dirtyTiles = new();

        /// <summary>
        /// Tracks mouse position during pan operations.
        /// </summary>
        private Point _lastPanPoint;

        /// <summary>
        /// Indicates if pan operation is active.
        /// </summary>
        private bool _isPanning;

        /// <summary>
        /// Gets the active camera viewport managing zoom, panning, and spatial culling bounds.
        /// </summary>
        public Viewport ActiveViewport { get; } = new();

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Solaris.Polaris" /> class.
        /// </summary>
        public Polaris()
        {
            InitializeComponent();
            Initiate();

            MouseWheel += OnMouseWheelZoom;
            MouseMove += OnMouseMovePan;
            MouseDown += OnMouseDownPan;
            MouseUp += OnMouseUpPan;
        }

        // We use these properties to safely manage GDI+ memory

        /// <summary>
        /// Gets the bitmap layer one.
        /// </summary>
        /// <value>
        /// The bitmap layer one.
        /// </value>
        internal UnmanagedImageBuffer? BitmapLayerOne { get; private set; }

        /// <summary>
        /// Gets the bitmap layer three.
        /// </summary>
        /// <value>
        /// The bitmap layer three.
        /// </value>
        internal UnmanagedImageBuffer? BitmapLayerThree { get; private set; }

        /// <summary>
        /// Occurs when [clicked].
        /// </summary>
        public event EventHandler<int>? Clicked;

        #region CLR Property Wrappers (MUST stay purely Get/Set)

        /// <summary>
        /// Gets or sets the height of the polaris.
        /// </summary>
        /// <value>
        /// The height of the polaris.
        /// </value>
        public int PolarisHeight
        {
            get => (int)GetValue(PolarisHeightProperty);
            set => SetValue(PolarisHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the width of the polaris.
        /// </summary>
        /// <value>
        /// The width of the polaris.
        /// </value>
        public int PolarisWidth
        {
            get => (int)GetValue(PolarisWidthProperty);
            set => SetValue(PolarisWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets the size of the polaris texture.
        /// </summary>
        /// <value>
        /// The size of the polaris texture.
        /// </value>
        public int PolarisTextureSize
        {
            get => (int)GetValue(PolarisTextureSizeProperty);
            set => SetValue(PolarisTextureSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the polaris textures.
        /// </summary>
        /// <value>
        /// The polaris textures.
        /// </value>
        public Dictionary<int, Texture> PolarisTextures
        {
            get => (Dictionary<int, Texture>)GetValue(PolarisTexturesProperty);
            set => SetValue(PolarisTexturesProperty, value);
        }

        /// <summary>
        /// Gets or sets the polaris map.
        /// </summary>
        /// <value>
        /// The polaris map.
        /// </value>
        public Dictionary<int, List<int>>? PolarisMap
        {
            get => (Dictionary<int, List<int>>?)GetValue(PolarisMapProperty);
            set => SetValue(PolarisMapProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [polaris grid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [polaris grid]; otherwise, <c>false</c>.
        /// </value>
        public bool PolarisGrid
        {
            get => (bool)GetValue(PolarisGridProperty);
            set => SetValue(PolarisGridProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [polaris number].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [polaris number]; otherwise, <c>false</c>.
        /// </value>
        public bool PolarisNumber
        {
            get => (bool)GetValue(PolarisNumberProperty);
            set => SetValue(PolarisNumberProperty, value);
        }

        #endregion

        #region Dependency Property Callbacks (Where the magic happens)

        /// <summary>
        /// Called when [map changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Polaris)d;
            if (e.NewValue == null || control.PolarisTextures == null) return;

            control.MarkLayerDirty(DirtyFlags.TileMap);
            control.RenderDirty();
        }

        /// <summary>
        /// Called when [grid changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Polaris)d;
            var isGridEnabled = (bool)e.NewValue;

            control.LayerTwo.Source = isGridEnabled
                ? Helper.GenerateGrid(control.PolarisWidth, control.PolarisHeight, control.PolarisTextureSize)
                : null;
        }

        /// <summary>
        /// Called when [number changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Polaris)d;
            var isNumberEnabled = (bool)e.NewValue;

            control.LayerThree.Source = isNumberEnabled
                ? Helper.GenerateNumbers(control.PolarisWidth, control.PolarisHeight, control.PolarisTextureSize)
                : null;
        }

        #endregion

        #region Dirty Region Management

        /// <summary>
        /// Marks a specific tile index dirty for targeted redraw.
        /// </summary>
        /// <param name="tileIndex">The spatial tile index.</param>
        /// <param name="flag">The layer flag.</param>
        public void MarkTileDirty(int tileIndex, DirtyFlags flag = DirtyFlags.TileMap)
        {
            _dirtyTiles.Add(tileIndex);
            _dirtyFlags |= flag;
        }

        /// <summary>
        /// Marks an entire layer dirty for a full pass.
        /// </summary>
        /// <param name="flag">The layer flag.</param>
        public void MarkLayerDirty(DirtyFlags flag)
        {
            _dirtyFlags |= flag;
        }

        /// <summary>
        /// Processes dirty regions and repaints only affected tile sub-regions or viewport sweeps.
        /// </summary>
        public void RenderDirty()
        {
            if (_dirtyFlags == DirtyFlags.None) return;

            lock (_lock)
            {
                if (_dirtyFlags.HasFlag(DirtyFlags.Viewport) || _dirtyTiles.Count == 0)
                {
                    var newBitmap = Helper.GenerateImage(
                        PolarisWidth, PolarisHeight, PolarisTextureSize, PolarisTextures, PolarisMap, ActiveViewport);
                    ReplaceBitmapLayerOne(newBitmap);
                }
                else if (_dirtyFlags.HasFlag(DirtyFlags.TileMap) && BitmapLayerOne != null && PolarisMap != null)
                {
                    foreach (var tileId in _dirtyTiles)
                    {
                        Helper.RedrawTileRegion(
                            BitmapLayerOne, tileId, PolarisWidth, PolarisTextureSize, PolarisTextures, PolarisMap,
                            ActiveViewport);
                    }

                    LayerOne.Source = BitmapLayerOne.UpdateWriteableBitmap(LayerOne.Source as WriteableBitmap);
                }

                _dirtyTiles.Clear();
                _dirtyFlags = DirtyFlags.None;
            }
        }

        #endregion

        #region Viewport Input Interaction Handlers

        /// <summary>
        /// Called when [mouse wheel zoom].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseWheelEventArgs"/> instance containing the event data.</param>
        private void OnMouseWheelZoom(object sender, MouseWheelEventArgs e)
        {
            var zoomChange = e.Delta > 0 ? 1.1f : 0.9f;
            ActiveViewport.Zoom = Math.Clamp(ActiveViewport.Zoom * zoomChange, 0.2f, 5.0f);

            MarkLayerDirty(DirtyFlags.Viewport);
            RenderDirty();
        }

        /// <summary>
        /// Called when [mouse down pan].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnMouseDownPan(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                _isPanning = true;
                _lastPanPoint = e.GetPosition(this);
                CaptureMouse();
            }
        }

        /// <summary>
        /// Called when [mouse move pan].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void OnMouseMovePan(object sender, MouseEventArgs e)
        {
            if (!_isPanning) return;

            var currentPoint = e.GetPosition(this);
            var delta = currentPoint - _lastPanPoint;

            ActiveViewport.PanX += (float)delta.X;
            ActiveViewport.PanY += (float)delta.Y;
            _lastPanPoint = currentPoint;

            MarkLayerDirty(DirtyFlags.Viewport);
            RenderDirty();
        }

        /// <summary>
        /// Called when [mouse up pan].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnMouseUpPan(object sender, MouseButtonEventArgs e)
        {
            if (_isPanning && e.MiddleButton == MouseButtonState.Released)
            {
                _isPanning = false;
                ReleaseMouseCapture();
            }
        }

        #endregion

        #region Setup and Memory Management

        /// <summary>
        /// Initiates this instance.
        /// </summary>
        public void Initiate()
        {
            if (PolarisWidth == 0 || PolarisHeight == 0 || PolarisTextureSize == 0)
                return;

            Touch.Height = PolarisHeight * PolarisTextureSize;
            Touch.Width = PolarisWidth * PolarisTextureSize;

            ActiveViewport.ScreenWidth = (int)Touch.Width;
            ActiveViewport.ScreenHeight = (int)Touch.Height;

            if (PolarisGrid)
                LayerTwo.Source = Helper.GenerateGrid(PolarisWidth, PolarisHeight, PolarisTextureSize);

            if (PolarisNumber)
                LayerThree.Source = Helper.GenerateNumbers(PolarisWidth, PolarisHeight, PolarisTextureSize);

            var initialLayer = new UnmanagedImageBuffer(Touch.Width > 0 ? (int)Touch.Width : 1,
                Touch.Height > 0 ? (int)Touch.Height : 1);
            initialLayer.Clear(0, 0, 0, 0);

            ReplaceBitmapLayerThree(initialLayer);
        }

        /// <summary>
        /// Safely swaps the unmanaged LayerOne Bitmap and immediately frees the old memory.
        /// </summary>
        /// <param name="newBitmap">The new unmanaged bitmap layer.</param>
        private void ReplaceBitmapLayerOne(UnmanagedImageBuffer? newBitmap)
        {
            BitmapLayerOne?.Dispose();
            BitmapLayerOne = newBitmap;
            LayerOne.Source = BitmapLayerOne.UpdateWriteableBitmap(LayerOne.Source as WriteableBitmap);
        }

        /// <summary>
        /// Safely swaps the unmanaged LayerThree Bitmap and immediately frees the old memory.
        /// </summary>
        /// <param name="newBitmap">The new unmanaged bitmap layer.</param>
        private void ReplaceBitmapLayerThree(UnmanagedImageBuffer? newBitmap)
        {
            BitmapLayerThree?.Dispose();
            BitmapLayerThree = newBitmap;
            // Only update the Image.Source if you need it instantly, otherwise let overlays handle it.
        }

        /// <summary>
        /// Adds the tile.
        /// </summary>
        /// <param name="tileData">The tile data.</param>
        public void AddTile(KeyValuePair<int, int> tileData)
        {
            var (check, dictionary) = Helper.AddTile(PolarisMap, tileData);
            if (!check) return;

            PolarisMap = dictionary;
            MarkTileDirty(tileData.Key, DirtyFlags.TileMap);
            RenderDirty();
        }

        /// <summary>
        /// Removes the tile.
        /// </summary>
        /// <param name="tileData">The tile data.</param>
        public void RemoveTile(KeyValuePair<int, int> tileData)
        {
            var (check, dictionary) = Helper.RemoveTile(PolarisMap, PolarisTextures, tileData);
            if (!check) return;

            PolarisMap = dictionary;
            MarkTileDirty(tileData.Key, DirtyFlags.TileMap);
            RenderDirty();
        }

        /// <summary>
        /// Adds the display.
        /// </summary>
        /// <param name="tileData">The tile data.</param>
        public void AddDisplay(KeyValuePair<int, int> tileData)
        {
            var newBmp = Helper.AddDisplay(PolarisWidth, PolarisTextureSize, PolarisTextures, BitmapLayerThree,
                tileData);
            BitmapLayerThree = newBmp;
            LayerThree.Source = newBmp.UpdateWriteableBitmap(LayerThree.Source as WriteableBitmap);

            MarkTileDirty(tileData.Key, DirtyFlags.Overlays);
        }

        /// <summary>
        /// Removes the display.
        /// </summary>
        /// <param name="position">The position.</param>
        public void RemoveDisplay(int position)
        {
            var newBmp = Helper.RemoveDisplay(PolarisWidth, PolarisTextureSize, BitmapLayerThree, position);
            BitmapLayerThree = newBmp;
            LayerThree.Source = newBmp.UpdateWriteableBitmap(LayerThree.Source as WriteableBitmap);

            MarkTileDirty(position, DirtyFlags.Overlays);
        }

        /// <summary>
        /// Handles the MouseDown event of the Touch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Touch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var rawPosition = e.GetPosition(Touch);
            var screenPoint = new System.Drawing.PointF((float)rawPosition.X, (float)rawPosition.Y);

            var id = ActiveViewport.ScreenToWorld(screenPoint, PolarisWidth, PolarisHeight, PolarisTextureSize);

            if (id >= 0)
            {
                Clicked?.Invoke(this, id);
            }
        }

        #endregion
    }
}
