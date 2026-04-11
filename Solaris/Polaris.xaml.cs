/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Polaris.cs
 * PURPOSE:     Editor Control
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Imaging;
using Mathematics;

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
        /// The cursor
        /// </summary>
        private Coordinate2D _cursor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Polaris"/> class.
        /// </summary>
        public Polaris()
        {
            InitializeComponent();
            Initiate();
        }

        // We use these properties to safely manage GDI+ memory

        /// <summary>
        /// Gets the bitmap layer one.
        /// </summary>
        /// <value>
        /// The bitmap layer one.
        /// </value>
        internal Bitmap? BitmapLayerOne { get; private set; }

        /// <summary>
        /// Gets the bitmap layer three.
        /// </summary>
        /// <value>
        /// The bitmap layer three.
        /// </value>
        internal Bitmap? BitmapLayerThree { get; private set; }

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

            lock (control._lock)
            {
                var newBitmap = Helper.GenerateImage(control.PolarisWidth, control.PolarisHeight,
                    control.PolarisTextureSize, control.PolarisTextures, (Dictionary<int, List<int>>)e.NewValue);
                control.ReplaceBitmapLayerOne(newBitmap);
            }
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

        /// <summary>
        /// Called when [add changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnAddChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Polaris)d;
            var value = (KeyValuePair<int, int>)e.NewValue;

            var (check, dictionary) = Helper.AddTile(control.PolarisMap, value);
            if (!check) return;

            control.PolarisMap = dictionary;

            lock (control._lock)
            {
                var newBitmap = Helper.GenerateImage(control.PolarisWidth, control.PolarisHeight,
                    control.PolarisTextureSize, control.PolarisTextures, control.PolarisMap);
                control.ReplaceBitmapLayerOne(newBitmap);
            }
        }

        /// <summary>
        /// Called when [remove changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnRemoveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Polaris)d;
            var value = (KeyValuePair<int, int>)e.NewValue;

            var (check, dictionary) = Helper.RemoveTile(control.PolarisMap, control.PolarisTextures, value);
            if (!check) return;

            control.PolarisMap = dictionary;

            lock (control._lock)
            {
                var newBitmap = Helper.GenerateImage(control.PolarisWidth, control.PolarisHeight,
                    control.PolarisTextureSize, control.PolarisTextures, control.PolarisMap);
                control.ReplaceBitmapLayerOne(newBitmap);
            }
        }

        /// <summary>
        /// Called when [add display changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnAddDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Polaris)d;
            var value = (KeyValuePair<int, int>)e.NewValue;

            var newBmp = Helper.AddDisplay(control.PolarisWidth, control.PolarisTextureSize, control.PolarisTextures,
                control.BitmapLayerThree, value);
            control.LayerThree.Source = newBmp?.ToBitmapImage();
        }

        private static void OnRemoveDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Polaris)d;
            var value = (int)e.NewValue;

            var newBmp = Helper.RemoveDisplay(control.PolarisWidth, control.PolarisTextureSize,
                control.BitmapLayerThree, value);
            control.LayerThree.Source = newBmp?.ToBitmapImage();
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

            if (PolarisGrid)
                LayerTwo.Source = Helper.GenerateGrid(PolarisWidth, PolarisHeight, PolarisTextureSize);

            if (PolarisNumber)
                LayerThree.Source = Helper.GenerateNumbers(PolarisWidth, PolarisHeight, PolarisTextureSize);

            ReplaceBitmapLayerThree(new Bitmap(Touch.Width > 0 ? (int)Touch.Width : 1,
                Touch.Height > 0 ? (int)Touch.Height : 1));
        }

        /// <summary>
        /// Safely swaps the unmanaged LayerOne Bitmap and immediately frees the old memory.
        /// </summary>
        private void ReplaceBitmapLayerOne(Bitmap? newBitmap)
        {
            BitmapLayerOne?.Dispose();
            BitmapLayerOne = newBitmap;
            LayerOne.Source = BitmapLayerOne?.ToBitmapImage();
        }

        /// <summary>
        /// Safely swaps the unmanaged LayerThree Bitmap and immediately frees the old memory.
        /// </summary>
        private void ReplaceBitmapLayerThree(Bitmap? newBitmap)
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
            lock (_lock)
            {
                var newBitmap = Helper.GenerateImage(PolarisWidth, PolarisHeight, PolarisTextureSize, PolarisTextures,
                    PolarisMap);
                ReplaceBitmapLayerOne(newBitmap);
            }
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
            lock (_lock)
            {
                var newBitmap = Helper.GenerateImage(PolarisWidth, PolarisHeight, PolarisTextureSize, PolarisTextures,
                    PolarisMap);
                ReplaceBitmapLayerOne(newBitmap);
            }
        }

        /// <summary>
        /// Adds the display.
        /// </summary>
        /// <param name="tileData">The tile data.</param>
        public void AddDisplay(KeyValuePair<int, int> tileData)
        {
            var newBmp = Helper.AddDisplay(PolarisWidth, PolarisTextureSize, PolarisTextures, BitmapLayerThree,
                tileData);
            LayerThree.Source = newBmp?.ToBitmapImage();
        }

        /// <summary>
        /// Removes the display.
        /// </summary>
        /// <param name="position">The position.</param>
        public void RemoveDisplay(int position)
        {
            var newBmp = Helper.RemoveDisplay(PolarisWidth, PolarisTextureSize, BitmapLayerThree, position);
            LayerThree.Source = newBmp?.ToBitmapImage();
        }

        /// <summary>
        /// Handles the MouseDown event of the Touch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Touch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(Touch);

            // 1. Calculate the values first.
            // Integer division naturally handles the 0 case for us!
            var gridX = (int)position.X / PolarisTextureSize;
            var gridY = (int)position.Y / PolarisTextureSize;

            _cursor = new Coordinate2D(gridX, gridY);
            var id = _cursor.ToId(PolarisWidth);

            Clicked?.Invoke(this, id);
        }

        #endregion
    }
}
