/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Aurora.cs
 * PURPOSE:     Game Control
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Imaging;
using Mathematics;

namespace Solaris
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///     Generate a playing field
    /// </summary>
    public sealed partial class Aurora
    {
        #region Dependency Properties

        /// <summary>
        /// The aurora height property
        /// </summary>
        public static readonly DependencyProperty AuroraHeightProperty = DependencyProperty.Register(
            nameof(AuroraHeight), typeof(int), typeof(Aurora), new PropertyMetadata(100));

        /// <summary>
        /// The aurora width property
        /// </summary>
        public static readonly DependencyProperty AuroraWidthProperty = DependencyProperty.Register(
            nameof(AuroraWidth), typeof(int), typeof(Aurora), new PropertyMetadata(100));

        /// <summary>
        /// The aurora texture size property
        /// </summary>
        public static readonly DependencyProperty AuroraTextureSizeProperty = DependencyProperty.Register(
            nameof(AuroraTextureSize), typeof(int), typeof(Aurora), new PropertyMetadata(100));

        /// <summary>
        /// The aurora textures property
        /// </summary>
        public static readonly DependencyProperty AuroraTexturesProperty = DependencyProperty.Register(
            nameof(AuroraTextures), typeof(Dictionary<int, Texture>), typeof(Aurora), new PropertyMetadata(null));

        /// <summary>
        /// The aurora map property
        /// </summary>
        public static readonly DependencyProperty AuroraMapProperty = DependencyProperty.Register(
            nameof(AuroraMap), typeof(Dictionary<int, List<int>>), typeof(Aurora),
            new PropertyMetadata(null, OnMapChanged));

        /// <summary>
        /// The aurora avatar property
        /// </summary>
        public static readonly DependencyProperty AuroraAvatarProperty = DependencyProperty.Register(
            nameof(AuroraAvatar), typeof(Bitmap), typeof(Aurora),
            new PropertyMetadata(null, OnAvatarChanged));

        /// <summary>
        /// The aurora movement property
        /// </summary>
        public static readonly DependencyProperty AuroraMovementProperty = DependencyProperty.Register(
            nameof(AuroraMovement), typeof(List<int>), typeof(Aurora),
            new PropertyMetadata(null, OnMovementChanged));

        /// <summary>
        /// The aurora grid property
        /// </summary>
        public static readonly DependencyProperty AuroraGridProperty = DependencyProperty.Register(
            nameof(AuroraGrid), typeof(bool), typeof(Aurora),
            new PropertyMetadata(false, OnGridChanged));

        /// <summary>
        /// The aurora add property
        /// </summary>
        public static readonly DependencyProperty AuroraAddProperty = DependencyProperty.Register(
            nameof(AuroraAdd), typeof(KeyValuePair<int, int>), typeof(Aurora),
            new PropertyMetadata(default(KeyValuePair<int, int>), OnAddChanged));

        /// <summary>
        /// The aurora remove property
        /// </summary>
        public static readonly DependencyProperty AuroraRemoveProperty = DependencyProperty.Register(
            nameof(AuroraRemove), typeof(KeyValuePair<int, int>), typeof(Aurora),
            new PropertyMetadata(default(KeyValuePair<int, int>), OnRemoveChanged));

        /// <summary>
        /// The aurora add display property
        /// </summary>
        public static readonly DependencyProperty AuroraAddDisplayProperty = DependencyProperty.Register(
            nameof(AuroraAddDisplay), typeof(KeyValuePair<int, int>), typeof(Aurora),
            new PropertyMetadata(default(KeyValuePair<int, int>), OnAddDisplayChanged));

        /// <summary>
        /// The aurora remove display property
        /// </summary>
        public static readonly DependencyProperty AuroraRemoveDisplayProperty = DependencyProperty.Register(
            nameof(AuroraRemoveDisplay), typeof(int), typeof(Aurora),
            new PropertyMetadata(0, OnRemoveDisplayChanged));

        #endregion

        /// <summary>
        /// The cursor
        /// </summary>
        private Coordinate2D _cursor;

        /// <summary>
        /// The third layer
        /// </summary>
        private Bitmap? _thirdLayer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Aurora"/> class.
        /// </summary>
        public Aurora()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the bitmap layer one.
        /// </summary>
        /// <value>
        /// The bitmap layer one.
        /// </value>
        internal Bitmap? BitmapLayerOne { get; private set; }

        /// <summary>
        /// Occurs when [tile clicked].
        /// </summary>
        public event EventHandler<int>? TileClicked;

        #region CLR Property Wrappers (Purely Get/Set)

        /// <summary>
        /// Gets or sets the height of the aurora.
        /// </summary>
        /// <value>
        /// The height of the aurora.
        /// </value>
        public int AuroraHeight
        {
            get => (int)GetValue(AuroraHeightProperty);
            set => SetValue(AuroraHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the width of the aurora.
        /// </summary>
        /// <value>
        /// The width of the aurora.
        /// </value>
        public int AuroraWidth
        {
            get => (int)GetValue(AuroraWidthProperty);
            set => SetValue(AuroraWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets the size of the aurora texture.
        /// </summary>
        /// <value>
        /// The size of the aurora texture.
        /// </value>
        public int AuroraTextureSize
        {
            get => (int)GetValue(AuroraTextureSizeProperty);
            set => SetValue(AuroraTextureSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the aurora map.
        /// </summary>
        /// <value>
        /// The aurora map.
        /// </value>
        public Dictionary<int, List<int>>? AuroraMap
        {
            get => (Dictionary<int, List<int>>?)GetValue(AuroraMapProperty);
            set => SetValue(AuroraMapProperty, value);
        }

        /// <summary>
        /// Gets or sets the aurora avatar.
        /// </summary>
        /// <value>
        /// The aurora avatar.
        /// </value>
        public Bitmap? AuroraAvatar
        {
            get => (Bitmap?)GetValue(AuroraAvatarProperty);
            set => SetValue(AuroraAvatarProperty, value);
        }

        /// <summary>
        /// Gets or sets the aurora textures.
        /// </summary>
        /// <value>
        /// The aurora textures.
        /// </value>
        public Dictionary<int, Texture> AuroraTextures
        {
            get => (Dictionary<int, Texture>)GetValue(AuroraTexturesProperty);
            set => SetValue(AuroraTexturesProperty, value);
        }

        /// <summary>
        /// Gets or sets the aurora add.
        /// </summary>
        /// <value>
        /// The aurora add.
        /// </value>
        public KeyValuePair<int, int> AuroraAdd
        {
            get => (KeyValuePair<int, int>)GetValue(AuroraAddProperty);
            set => SetValue(AuroraAddProperty, value);
        }

        /// <summary>
        /// Gets or sets the aurora remove.
        /// </summary>
        /// <value>
        /// The aurora remove.
        /// </value>
        public KeyValuePair<int, int> AuroraRemove
        {
            get => (KeyValuePair<int, int>)GetValue(AuroraRemoveProperty);
            set => SetValue(AuroraRemoveProperty, value);
        }

        /// <summary>
        /// Gets or sets the aurora add display.
        /// </summary>
        /// <value>
        /// The aurora add display.
        /// </value>
        public KeyValuePair<int, int> AuroraAddDisplay
        {
            get => (KeyValuePair<int, int>)GetValue(AuroraAddDisplayProperty);
            set => SetValue(AuroraAddDisplayProperty, value);
        }

        /// <summary>
        /// Gets or sets the aurora remove display.
        /// </summary>
        /// <value>
        /// The aurora remove display.
        /// </value>
        public int AuroraRemoveDisplay
        {
            get => (int)GetValue(AuroraRemoveDisplayProperty);
            set => SetValue(AuroraRemoveDisplayProperty, value);
        }

        /// <summary>
        /// Gets or sets the aurora movement.
        /// </summary>
        /// <value>
        /// The aurora movement.
        /// </value>
        public List<int>? AuroraMovement
        {
            get => (List<int>?)GetValue(AuroraMovementProperty);
            set => SetValue(AuroraMovementProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [aurora grid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [aurora grid]; otherwise, <c>false</c>.
        /// </value>
        public bool AuroraGrid
        {
            get => (bool)GetValue(AuroraGridProperty);
            set => SetValue(AuroraGridProperty, value);
        }

        #endregion

        #region Dependency Property Callbacks (Safe execution)

        /// <summary>
        /// Called when [map changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Aurora)d;
            control.UpdateMapAndBitmap();
        }

        /// <summary>
        /// Called when [avatar changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnAvatarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Optional hook: If you need to immediately draw the avatar when it's assigned, do it here.
        }

        /// <summary>
        /// Called when [movement changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static async void OnMovementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Aurora)d;
            if (e.NewValue is not List<int> movement) return;

            try
            {
                // Safely await the animation without blocking the main UI thread
                await Helper.DisplayMovement(control, movement, control.AuroraAvatar, control.AuroraWidth,
                    control.AuroraHeight, control.AuroraTextureSize);
            }
            catch (Exception ex)
            {
                // Log or handle animation failure gracefully rather than crashing the app silently
                Trace.WriteLine($"Movement animation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when [grid changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Aurora)d;
            var isGridEnabled = (bool)e.NewValue;

            control.LayerTwo.Source = isGridEnabled
                ? Helper.GenerateGrid(control.AuroraWidth, control.AuroraHeight, control.AuroraTextureSize)
                : null;
        }

        /// <summary>
        /// Called when [add changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnAddChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Aurora)d;
            var value = (KeyValuePair<int, int>)e.NewValue;

            var (check, dictionary) = Helper.AddTile(control.AuroraMap, value);
            if (!check) return;

            control.AuroraMap = dictionary;
            control.UpdateMapAndBitmap();
        }

        /// <summary>
        /// Called when [remove changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnRemoveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Aurora)d;
            var value = (KeyValuePair<int, int>)e.NewValue;

            var (check, dictionary) = Helper.RemoveTile(control.AuroraMap, control.AuroraTextures, value);
            if (!check) return;

            control.AuroraMap = dictionary;
            control.UpdateMapAndBitmap();
        }

        /// <summary>
        /// Called when [add display changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnAddDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Aurora)d;
            var value = (KeyValuePair<int, int>)e.NewValue;

            var newBmp = Helper.AddDisplay(control.AuroraWidth, control.AuroraTextureSize, control.AuroraTextures,
                control._thirdLayer, value);
            control.LayerThree.Source = newBmp?.ToBitmapImage();
        }

        /// <summary>
        /// Called when [remove display changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnRemoveDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Aurora)d;
            var value = (int)e.NewValue;

            var newBmp = Helper.RemoveDisplay(control.AuroraWidth, control.AuroraTextureSize, control._thirdLayer,
                value);
            control.LayerThree.Source = newBmp?.ToBitmapImage();
        }

        #endregion

        #region Setup and Memory Management

        /// <summary>
        /// Initiates this instance.
        /// </summary>
        public void Initiate()
        {
            if (AuroraWidth == 0 || AuroraHeight == 0 || AuroraTextureSize == 0)
                return;

            Touch.Height = AuroraHeight * AuroraTextureSize;
            Touch.Width = AuroraWidth * AuroraTextureSize;

            UpdateMapAndBitmap();

            if (AuroraGrid)
                LayerTwo.Source = Helper.GenerateGrid(AuroraWidth, AuroraHeight, AuroraTextureSize);

            ReplaceThirdLayer(new Bitmap(Touch.Width > 0 ? (int)Touch.Width : 1,
                Touch.Height > 0 ? (int)Touch.Height : 1));
        }

        /// <summary>
        /// Updates the map and bitmap.
        /// </summary>
        private void UpdateMapAndBitmap()
        {
            if (AuroraTextures == null) return;

            var newBitmap =
                Helper.GenerateImage(AuroraWidth, AuroraHeight, AuroraTextureSize, AuroraTextures, AuroraMap);
            ReplaceBitmapLayerOne(newBitmap);
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
        private void ReplaceThirdLayer(Bitmap? newBitmap)
        {
            _thirdLayer?.Dispose();
            _thirdLayer = newBitmap;
            // Only assign if required, usually overlaid later
        }

        /// <summary>
        /// Handles the MouseDown event of the Touch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Touch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(Touch);

            int gridX = (int)position.X / AuroraTextureSize;
            int gridY = (int)position.Y / AuroraTextureSize;

            _cursor = new Coordinate2D(gridX, gridY);
            var id = _cursor.ToId(AuroraWidth);

            TileClicked?.Invoke(this, id);
        }

        #endregion
    }
}
