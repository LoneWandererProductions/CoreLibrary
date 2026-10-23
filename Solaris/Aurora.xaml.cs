/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Aurora.cs
 * PURPOSE:     Game Control
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Imaging.Objects;

namespace Solaris
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///     Generate a playing field
    /// </summary>
    public sealed partial class Aurora
    {
        #region Elevation Dependency Properties

        /// <summary>
        /// The aurora elevation property mapping tile indices to vertical levels.
        /// </summary>
        public static readonly DependencyProperty AuroraElevationProperty = DependencyProperty.Register(
            nameof(AuroraElevation), typeof(Dictionary<int, int>), typeof(Aurora),
            new PropertyMetadata(null, OnMapChanged)); //

        /// <summary>
        /// The vertical extrusion height in pixels per elevation step.
        /// </summary>
        public static readonly DependencyProperty StepHeightProperty = DependencyProperty.Register(
            nameof(StepHeight), typeof(int), typeof(Aurora),
            new PropertyMetadata(16, OnMapChanged)); //[cite: 12]

        /// <summary>
        /// Gets or sets the aurora elevation.
        /// </summary>
        /// <value>
        /// The aurora elevation.
        /// </value>
        public Dictionary<int, int>? AuroraElevation
        {
            get => (Dictionary<int, int>?)GetValue(AuroraElevationProperty);
            set => SetValue(AuroraElevationProperty, value);
        }

        /// <summary>
        /// Gets or sets the height of the step.
        /// </summary>
        /// <value>
        /// The height of the step.
        /// </value>
        public int StepHeight
        {
            get => (int)GetValue(StepHeightProperty);
            set => SetValue(StepHeightProperty, value);
        }

        #endregion

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

        /// <summary>
        /// The aurora glyphs property
        /// </summary>
        public static readonly DependencyProperty AuroraGlyphsProperty = DependencyProperty.Register(
            nameof(AuroraGlyphs), typeof(Dictionary<int, OverlayGlyph>), typeof(Aurora),
            new PropertyMetadata(null, OnGlyphsChanged));

        #endregion

        #region Avatar & Dynamic Sorting Dependency Properties

        /// <summary>
        /// The current tile index of the avatar actor.
        /// </summary>
        public static readonly DependencyProperty AvatarPositionProperty = DependencyProperty.Register(
            nameof(AvatarPosition), typeof(int), typeof(Aurora),
            new PropertyMetadata(-1, OnAvatarPositionChanged));

        /// <summary>
        /// Enables sorting dynamic actors directly on LayerOne alongside standing props.
        /// </summary>
        public static readonly DependencyProperty UseUnifiedLayerOneProperty = DependencyProperty.Register(
            nameof(UseUnifiedLayerOne), typeof(bool), typeof(Aurora),
            new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets the avatar position.
        /// </summary>
        /// <value>
        /// The avatar position.
        /// </value>
        public int AvatarPosition
        {
            get => (int)GetValue(AvatarPositionProperty);
            set => SetValue(AvatarPositionProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use unified layer one].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use unified layer one]; otherwise, <c>false</c>.
        /// </value>
        public bool UseUnifiedLayerOne
        {
            get => (bool)GetValue(UseUnifiedLayerOneProperty);
            set => SetValue(UseUnifiedLayerOneProperty, value);
        }

        /// <summary>
        /// Called when [avatar position changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnAvatarPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Aurora)d;
            control.MarkLayerDirty(DirtyFlags.FullRedraw);
            control.RenderDirty();
        }

        #endregion

        #region Actor & Avatar Dependency Properties

        /// <summary>
        /// Liste oder Dictionary aller dynamischen Einheiten (Gegner, NPCs, Verbündete).
        /// </summary>
        public static readonly DependencyProperty AuroraActorsProperty = DependencyProperty.Register(
            nameof(AuroraActors), typeof(Dictionary<int, GameActor>), typeof(Aurora),
            new PropertyMetadata(null, OnActorsChanged));

        /// <summary>
        /// Gets or sets the aurora actors.
        /// </summary>
        /// <value>
        /// The aurora actors.
        /// </value>
        public Dictionary<int, GameActor>? AuroraActors
        {
            get => (Dictionary<int, GameActor>?)GetValue(AuroraActorsProperty);
            set => SetValue(AuroraActorsProperty, value);
        }

        /// <summary>
        /// Called when [actors changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnActorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Aurora)d;
            if (e.NewValue is Dictionary<int, GameActor> actors)
            {
                foreach (var actor in actors.Values)
                {
                    actor.UpdateBuffer();
                }
            }

            control.MarkLayerDirty(DirtyFlags.FullRedraw);
            control.RenderDirty();
        }

        /// <summary>
        /// Called when the player's avatar is assigned or replaced.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnAvatarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Aurora)d;
            control.UpdateAvatarBuffer();
            control.MarkLayerDirty(DirtyFlags.FullRedraw);
            control.RenderDirty();
        }

        /// <summary>
        /// Converts the allocated avatar bitmap into a fast unmanaged buffer.
        /// </summary>
        private void UpdateAvatarBuffer()
        {
            _avatarBuffer?.Dispose();
            _avatarBuffer = AuroraAvatar != null ? UnmanagedImageBuffer.FromBitmap(AuroraAvatar) : null;
        }

        #endregion

        #region Fog of War Dependency Properties

        /// <summary>
        /// Status of the Warring Mist by tile index (0 = Unexplored, 1 = Explored, 2 = Visible).
        /// </summary>
        public static readonly DependencyProperty AuroraFogProperty = DependencyProperty.Register(
            nameof(AuroraFog), typeof(byte[]), typeof(Aurora),
            new PropertyMetadata(null, OnFogChanged));

        /// <summary>
        /// Gets or sets the aurora fog.
        /// </summary>
        /// <value>
        /// The aurora fog.
        /// </value>
        public byte[]? AuroraFog
        {
            get => (byte[]?)GetValue(AuroraFogProperty);
            set => SetValue(AuroraFogProperty, value);
        }

        /// <summary>
        /// Called when [fog changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        private static void OnFogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Aurora)d;
            control.MarkLayerDirty(DirtyFlags.FullRedraw);
            control.RenderDirty();
        }

        #endregion

        /// <summary>
        /// Assigns a new list of texture layers to a tile and re-renders it.
        /// </summary>
        /// <param name="tileIndex">The 1D tile index.</param>
        /// <param name="tiles">The new list of texture IDs on this tile.</param>
        public void ChangeTiles(int tileIndex, List<int> tiles)
        {
            if (tileIndex < 0 || tileIndex >= AuroraWidth * AuroraHeight) return;

            AuroraMap ??= new Dictionary<int, List<int>>();

            if (tiles == null || tiles.Count == 0)
            {
                AuroraMap.Remove(tileIndex);
            }
            else
            {
                AuroraMap[tileIndex] = new List<int>(tiles);
            }

            MarkTileDirty(tileIndex, DirtyFlags.TileMap);
            RenderDirty();
        }

        /// <summary>
        /// Berechnet anhand einer 4-Bit-Nachbarschaftsmaske (N=1, E=2, S=4, W=8)
        /// die passenden Übergangs-/Autotiling-Texturen für eine Kachel und aktualisiert sie.
        /// Verhindert Zeilenüberläufe an den Rändern (Flat Map Bounds).
        /// </summary>
        /// <param name="tileIndex">Der Kachelindex.</param>
        /// <param name="targetBaseTextureId">Die Textur-ID des Basis-Geländes (z. B. Wasser oder Straße).</param>
        /// <param name="getTransitionTextureId">Optionale Lookup-Funktion: (bitmask) => texturId.</param>
        public void UpdateTileTransitions(
            int tileIndex,
            int targetBaseTextureId,
            Func<byte, int?>? getTransitionTextureId = null)
        {
            if (tileIndex < 0 || tileIndex >= AuroraWidth * AuroraHeight || AuroraMap == null)
                return;

            var x = tileIndex % AuroraWidth;
            var y = tileIndex / AuroraWidth;

            // 4-Bit Richtungs-Maske: Nord (1), Ost (2), Süd (4), West (8)
            byte mask = 0;

            // Nord (y > 0)
            if (y > 0 && HasMatchingTexture(tileIndex - AuroraWidth, targetBaseTextureId))
                mask |= 1;

            // Ost (x < Width - 1: kein Zeilenumbruch!)
            if (x < AuroraWidth - 1 && HasMatchingTexture(tileIndex + 1, targetBaseTextureId))
                mask |= 2;

            // Süd (y < Height - 1)
            if (y < AuroraHeight - 1 && HasMatchingTexture(tileIndex + AuroraWidth, targetBaseTextureId))
                mask |= 4;

            // West (x > 0: kein Zeilenübertrag!)
            if (x > 0 && HasMatchingTexture(tileIndex - 1, targetBaseTextureId))
                mask |= 8;

            if (getTransitionTextureId != null)
            {
                var resolvedTransition = getTransitionTextureId(mask);
                if (resolvedTransition.HasValue)
                {
                    if (!AuroraMap.TryGetValue(tileIndex, out var layers))
                    {
                        layers = new List<int>();
                        AuroraMap[tileIndex] = layers;
                    }

                    // Fügt die Übergangstextur auf oberster Ebene hinzu oder tauscht sie aus
                    if (!layers.Contains(resolvedTransition.Value))
                    {
                        layers.Add(resolvedTransition.Value);
                        MarkTileDirty(tileIndex, DirtyFlags.TileMap);
                        RenderDirty();
                    }
                }
            }
        }

        /// <summary>
        /// Prüft sicher, ob eine Kachel eine bestimmte Textur-ID enthält.
        /// </summary>
        private bool HasMatchingTexture(int checkTileIndex, int matchTextureId)
        {
            if (AuroraMap == null || !AuroraMap.TryGetValue(checkTileIndex, out var list))
                return false;

            return list.Contains(matchTextureId);
        }

        /// <summary>
        /// The third layer
        /// </summary>
        private UnmanagedImageBuffer? _thirdLayer;

        /// <summary>
        /// The active dirty flags indicating invalid layers.
        /// </summary>
        private DirtyFlags _dirtyFlags = DirtyFlags.None;

        /// <summary>
        /// Queue of specific tile indices that need sub-region re-blitting.
        /// </summary>
        private readonly HashSet<int> _dirtyTiles = new();

        /// <summary>
        /// Gets the camera viewport managing dynamic zoom and entity tracking projection.
        /// </summary>
        public Viewport ActiveViewport { get; } = new();

        /// <summary>
        /// The avatar buffer
        /// </summary>
        private UnmanagedImageBuffer? _avatarBuffer;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Solaris.Aurora" /> class.
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
        internal UnmanagedImageBuffer? BitmapLayerOne { get; private set; }

        /// <summary>
        /// Gets the bitmap layer three buffer.
        /// </summary>
        /// <value>
        /// The bitmap layer three buffer.
        /// </value>
        internal UnmanagedImageBuffer? BitmapLayerThree => _thirdLayer;

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

        /// <summary>
        /// Gets or sets the aurora glyphs.
        /// </summary>
        /// <value>
        /// The aurora glyphs.
        /// </value>
        public Dictionary<int, OverlayGlyph>? AuroraGlyphs
        {
            get => (Dictionary<int, OverlayGlyph>?)GetValue(AuroraGlyphsProperty);
            set => SetValue(AuroraGlyphsProperty, value);
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
            control.MarkLayerDirty(DirtyFlags.FullRedraw);
            control.RenderDirty();
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
                await Helper.DisplayMovement(
                    control, movement, control.AuroraAvatar, control.AuroraWidth,
                    control.AuroraHeight, control.AuroraTextureSize, control.UseUnifiedLayerOne);
            }
            catch (Exception ex)
            {
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
                ? Helper.GenerateGrid(control.AuroraWidth, control.AuroraHeight, control.AuroraTextureSize,
                    control.ActiveViewport, control.AuroraElevation, control.StepHeight)
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
            control.MarkTileDirty(value.Key, DirtyFlags.TileMap);
            control.RenderDirty();
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
            control.MarkTileDirty(value.Key, DirtyFlags.TileMap);
            control.RenderDirty();
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

            var newBmp = Helper.AddDisplay(
                control.AuroraWidth, control.AuroraHeight, control.AuroraTextureSize,
                control.AuroraTextures, control._thirdLayer, value,
                control.ActiveViewport, control.AuroraElevation, control.StepHeight);

            control._thirdLayer = newBmp;
            control.LayerThree.Source = newBmp.UpdateWriteableBitmap(control.LayerThree.Source as WriteableBitmap);
            control.MarkTileDirty(value.Key, DirtyFlags.Overlays);
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

            var newBmp = Helper.RemoveDisplay(
                control.AuroraWidth, control.AuroraTextureSize, control._thirdLayer, value,
                control.ActiveViewport, control.AuroraElevation, control.StepHeight);

            control._thirdLayer = newBmp;
            control.LayerThree.Source = newBmp?.UpdateWriteableBitmap(control.LayerThree.Source as WriteableBitmap);
            control.MarkTileDirty(value, DirtyFlags.Overlays);
        }

        /// <summary>
        /// Called when [glyphs changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnGlyphsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Aurora)d;
            control.MarkLayerDirty(DirtyFlags.Overlays);
            control.RenderDirty();
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
        /// Processes dirty regions and repaints only affected tile sub-regions.
        /// </summary>
        public void RenderDirty()
        {
            if (_dirtyFlags == DirtyFlags.None) return;

            if (_dirtyFlags.HasFlag(DirtyFlags.Viewport) || _dirtyFlags.HasFlag(DirtyFlags.FullRedraw))
            {
                UpdateMapAndBitmap();

                if (AuroraGrid)
                {
                    LayerTwo.Source = Helper.GenerateGrid(
                        AuroraWidth, AuroraHeight, AuroraTextureSize,
                        ActiveViewport, AuroraElevation, StepHeight);
                }
            }
            else if (_dirtyFlags.HasFlag(DirtyFlags.TileMap) && BitmapLayerOne != null && AuroraMap != null)
            {
                if (_dirtyTiles.Count > 0)
                {
                    foreach (var tileId in _dirtyTiles)
                    {
                        Helper.RedrawTileRegion(
                            BitmapLayerOne, tileId, AuroraWidth, AuroraTextureSize, AuroraTextures, AuroraMap,
                            ActiveViewport);
                    }

                    LayerOne.Source = BitmapLayerOne.UpdateWriteableBitmap(LayerOne.Source as WriteableBitmap);
                }
            }

            if (_dirtyFlags.HasFlag(DirtyFlags.Overlays))
            {
                UpdateGlyphLayer();
            }

            _dirtyTiles.Clear();
            _dirtyFlags = DirtyFlags.None;
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

            ActiveViewport.ScreenWidth = (int)Touch.Width;
            ActiveViewport.ScreenHeight = (int)Touch.Height;

            UpdateMapAndBitmap();

            if (AuroraGrid)
                LayerTwo.Source = Helper.GenerateGrid(AuroraWidth, AuroraHeight, AuroraTextureSize,
                    ActiveViewport, AuroraElevation, StepHeight);

            var initialLayer = new UnmanagedImageBuffer(Touch.Width > 0 ? (int)Touch.Width : 1,
                Touch.Height > 0 ? (int)Touch.Height : 1);
            initialLayer.Clear(0, 0, 0, 0);

            ReplaceThirdLayer(initialLayer);
        }

        /// <summary>
        /// Updates the map and bitmap.
        /// </summary>
        private void UpdateMapAndBitmap()
        {
            if (AuroraTextures == null) return;

            var newBitmap = Helper.GenerateImage(
                AuroraWidth, AuroraHeight, AuroraTextureSize, AuroraTextures, AuroraMap,
                ActiveViewport, AuroraElevation, StepHeight,
                AvatarPosition, _avatarBuffer, AuroraActors, AuroraFog);

            ReplaceBitmapLayerOne(newBitmap);
        }

        /// <summary>
        /// Safely swaps the unmanaged LayerOne Bitmap and immediately frees the old memory.
        /// </summary>
        private void ReplaceBitmapLayerOne(UnmanagedImageBuffer? newBitmap)
        {
            BitmapLayerOne?.Dispose();
            BitmapLayerOne = newBitmap;
            LayerOne.Source = BitmapLayerOne.UpdateWriteableBitmap(LayerOne.Source as WriteableBitmap);
        }

        /// <summary>
        /// Safely swaps the unmanaged LayerThree Bitmap and immediately frees the old memory.
        /// </summary>
        private void ReplaceThirdLayer(UnmanagedImageBuffer? newBitmap)
        {
            _thirdLayer?.Dispose();
            _thirdLayer = newBitmap;
            // Only assign if required, usually overlaid later
        }

        /// <summary>
        /// Redraws only the vector annotation layer cleanly without touching heavy ground assets.
        /// </summary>
        public void UpdateGlyphLayer()
        {
            using var overlayBmp = Helper.GenerateGlyphOverlay(
                AuroraWidth, AuroraHeight, AuroraTextureSize, AuroraGlyphs,
                ActiveViewport, AuroraElevation, StepHeight); // Viewport and heights passed

            using var overlayBuffer = UnmanagedImageBuffer.FromBitmap(overlayBmp);

            LayerFour.Source = overlayBuffer.UpdateWriteableBitmap(LayerFour.Source as WriteableBitmap);
        }

        /// <summary>
        /// Handles the MouseDown event of the Touch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Touch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var rawPosition = e.GetPosition(Touch);
            var screenPoint = new PointF((float)rawPosition.X, (float)rawPosition.Y);

            var id = ActiveViewport.ScreenToWorld(
                screenPoint, AuroraWidth, AuroraHeight, AuroraTextureSize, AuroraElevation, StepHeight);

            if (id >= 0)
            {
                TileClicked?.Invoke(this, id);
            }
        }

        #endregion
    }
}
