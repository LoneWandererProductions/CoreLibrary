/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Aurora.cs
 * PURPOSE:     Game Control
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Imaging;
using Mathematics;

namespace Solaris;

/// <inheritdoc cref="UserControl" />
/// <summary>
///     Generate a playing field
/// </summary>
public sealed partial class Aurora
{
    /// <summary>
    ///     The map height
    /// </summary>
    public static readonly DependencyProperty AuroraHeightProperty = DependencyProperty.Register(
        nameof(AuroraHeight),
        typeof(int),
        typeof(Aurora), new PropertyMetadata(100));

    /// <summary>
    ///     The map width
    /// </summary>
    public static readonly DependencyProperty AuroraWidthProperty = DependencyProperty.Register(nameof(AuroraWidth),
        typeof(int),
        typeof(Aurora), new PropertyMetadata(100));

    /// <summary>
    ///     The texture size
    /// </summary>
    public static readonly DependencyProperty AuroraTextureSizeProperty = DependencyProperty.Register(
        nameof(AuroraTextureSize),
        typeof(int),
        typeof(Aurora), new PropertyMetadata(100));

    /// <summary>
    ///     The map
    /// </summary>
    public static readonly DependencyProperty AuroraMapProperty = DependencyProperty.Register(nameof(AuroraMap),
        typeof(Dictionary<int, List<int>>),
        typeof(Aurora),
        new PropertyMetadata(null));

    /// <summary>
    ///     The avatar
    /// </summary>
    public static readonly DependencyProperty AuroraAvatarProperty = DependencyProperty.Register(
        nameof(AuroraAvatar),
        typeof(Bitmap),
        typeof(Aurora),
        new PropertyMetadata(null));

    /// <summary>
    ///     The movement
    /// </summary>
    public static readonly DependencyProperty AuroraMovementProperty = DependencyProperty.Register(
        nameof(AuroraMovement),
        typeof(List<int>),
        typeof(Aurora),
        new PropertyMetadata(null));

    /// <summary>
    ///     The textures
    /// </summary>
    public static readonly DependencyProperty AuroraTexturesProperty = DependencyProperty.Register(
        nameof(AuroraTextures),
        typeof(Dictionary<int, Texture>),
        typeof(Aurora),
        new PropertyMetadata(null));

    /// <summary>
    ///     The grid
    /// </summary>
    public static readonly DependencyProperty AuroraGridProperty = DependencyProperty.Register(nameof(AuroraGrid),
        typeof(bool),
        typeof(Aurora),
        new PropertyMetadata(false));

    /// <summary>
    ///     The add
    /// </summary>
    public static readonly DependencyProperty AuroraAddProperty = DependencyProperty.Register(nameof(AuroraAdd),
        typeof(KeyValuePair<int, int>),
        typeof(Aurora),
        new PropertyMetadata(null));

    /// <summary>
    ///     The remove
    /// </summary>
    public static readonly DependencyProperty AuroraRemoveProperty = DependencyProperty.Register(
        nameof(AuroraRemove),
        typeof(KeyValuePair<int, int>),
        typeof(Aurora),
        new PropertyMetadata(null));

    /// <summary>
    ///     The add display
    /// </summary>
    public static readonly DependencyProperty AuroraAddDisplayProperty = DependencyProperty.Register(
        nameof(AuroraAddDisplay),
        typeof(KeyValuePair<int, int>),
        typeof(Aurora),
        new PropertyMetadata(null));

    /// <summary>
    ///     The remove display
    /// </summary>
    public static readonly DependencyProperty AuroraRemoveDisplayProperty = DependencyProperty.Register(
        nameof(AuroraRemoveDisplay),
        typeof(int),
        typeof(Aurora),
        new PropertyMetadata(null));

    /// <summary>
    ///     The cursor
    /// </summary>
    private Coordinate2D _cursor;

    /// <summary>
    ///     The third layer
    /// </summary>
    private Bitmap? _thirdLayer;

    /// <inheritdoc />
    /// <summary>
    ///     Initializes a new instance of the <see cref="Aurora" /> class.
    /// </summary>
    public Aurora()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Gets the bitmap layer one.
    /// </summary>
    /// <value>
    ///     The bitmap layer one.
    /// </value>
    internal Bitmap? BitmapLayerOne { get; private set; }

    /// <summary>
    ///     Gets or sets the height of the dependency.
    /// </summary>
    /// <value>
    ///     The height of the dependency.
    /// </value>
    public int AuroraHeight
    {
        get => (int)GetValue(AuroraHeightProperty);
        set => SetValue(AuroraHeightProperty, value);
    }

    /// <summary>
    ///     Gets or sets the width of the dependency.
    /// </summary>
    /// <value>
    ///     The width of the dependency.
    /// </value>
    public int AuroraWidth
    {
        get => (int)GetValue(AuroraWidthProperty);
        set => SetValue(AuroraWidthProperty, value);
    }

    /// <summary>
    ///     Gets or sets the size of the dependency texture.
    /// </summary>
    /// <value>
    ///     The size of the dependency texture.
    /// </value>
    public int AuroraTextureSize
    {
        get => (int)GetValue(AuroraTextureSizeProperty);
        set => SetValue(AuroraTextureSizeProperty, value);
    }

    /// <summary>
    ///     Gets or sets the dependency map.
    /// </summary>
    /// <value>
    ///     The dependency map.
    /// </value>
    public Dictionary<int, List<int>>? AuroraMap
    {
        get => (Dictionary<int, List<int>>)GetValue(AuroraMapProperty);
        set => SetValue(AuroraMapProperty, value);
    }

    /// <summary>
    ///     Gets or sets the dependency avatar.
    /// </summary>
    /// <value>
    ///     The dependency avatar.
    /// </value>
    public Bitmap? AuroraAvatar
    {
        get => (Bitmap)GetValue(AuroraAvatarProperty);
        set => SetValue(AuroraAvatarProperty, value);
    }

    /// <summary>
    ///     Gets or sets the dependency textures.
    /// </summary>
    /// <value>
    ///     The dependency textures.
    /// </value>
    public Dictionary<int, Texture> AuroraTextures
    {
        get => (Dictionary<int, Texture>)GetValue(AuroraTexturesProperty);
        set => SetValue(AuroraTexturesProperty, value);
    }

    /// <summary>
    ///     Gets or sets the dependency add.
    /// </summary>
    /// <value>
    ///     The dependency add.
    /// </value>
    public KeyValuePair<int, int> AuroraAdd
    {
        get => (KeyValuePair<int, int>)GetValue(AuroraAddProperty);
        set
        {
            SetValue(AuroraAddProperty, value);
            var (check, dictionary) = Helper.AddTile(AuroraMap, value);

            if (!check)
            {
                return;
            }

            AuroraMap = dictionary;

            UpdateMapAndBitmap();
        }
    }

    /// <summary>
    ///     Gets or sets the dependency remove.
    /// </summary>
    /// <value>
    ///     The dependency remove.
    /// </value>
    public KeyValuePair<int, int> AuroraRemove
    {
        get => (KeyValuePair<int, int>)GetValue(AuroraRemoveProperty);
        set
        {
            SetValue(AuroraRemoveProperty, value);

            var (check, dictionary) = Helper.RemoveTile(AuroraMap, AuroraTextures, value);

            if (!check)
            {
                return;
            }

            AuroraMap = dictionary;

            UpdateMapAndBitmap();
        }
    }

    /// <summary>
    ///     Gets or sets the dependency add display.
    /// </summary>
    /// <value>
    ///     The dependency add display.
    /// </value>
    public KeyValuePair<int, int> AuroraAddDisplay
    {
        get => (KeyValuePair<int, int>)GetValue(AuroraAddDisplayProperty);
        set
        {
            SetValue(AuroraAddDisplayProperty, value);
            var bmp = Helper.AddDisplay(AuroraWidth, AuroraTextureSize,
                AuroraTextures, _thirdLayer, value);

            LayerThree.Source = bmp.ToBitmapImage();
        }
    }

    /// <summary>
    ///     Gets or sets the dependency remove display.
    /// </summary>
    /// <value>
    ///     The dependency remove display.
    /// </value>
    public int AuroraRemoveDisplay
    {
        get => (int)GetValue(AuroraRemoveDisplayProperty);
        set
        {
            SetValue(AuroraRemoveDisplayProperty, value);
            var bmp = Helper.RemoveDisplay(AuroraWidth, AuroraTextureSize, _thirdLayer, value);

            LayerThree.Source = bmp.ToBitmapImage();
        }
    }

    /// <summary>
    ///     Gets or sets the dependency movement.
    /// </summary>
    /// <value>
    ///     The dependency movement.
    /// </value>
    public List<int> AuroraMovement
    {
        get => (List<int>)GetValue(AuroraMovementProperty);
        set
        {
            if (value == null)
            {
                return;
            }

            SetValue(AuroraMovementProperty, value);

            //display an movement Animation, block the whole control while displaying
            _ = Helper.DisplayMovement(this, value, AuroraAvatar, AuroraWidth, AuroraHeight,
                AuroraTextureSize);
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether [dependency grid].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [dependency grid]; otherwise, <c>false</c>.
    /// </value>
    public bool AuroraGrid
    {
        get => (bool)GetValue(AuroraGridProperty);
        set
        {
            SetValue(AuroraGridProperty, value);
            LayerTwo.Source = !AuroraGrid
                ? null
                : Helper.GenerateGrid(AuroraWidth, AuroraHeight, AuroraTextureSize);
        }
    }

    /// <summary>
    ///     Initiates this instance.
    /// </summary>
    public void Initiate()
    {
        if (AuroraWidth == 0 || AuroraHeight == 0 || AuroraTextureSize == 0)
        {
            return;
        }

        Touch.Height = AuroraHeight * AuroraTextureSize;
        Touch.Width = AuroraWidth * AuroraTextureSize;

        BitmapLayerOne = Helper.GenerateImage(AuroraWidth, AuroraHeight, AuroraTextureSize,
            AuroraTextures, AuroraMap);

        LayerOne.Source = BitmapLayerOne.ToBitmapImage();

        if (AuroraGrid)
        {
            LayerTwo.Source = Helper.GenerateGrid(AuroraWidth, AuroraHeight, AuroraTextureSize);
        }

        _thirdLayer = new Bitmap(AuroraWidth * AuroraTextureSize, AuroraHeight * AuroraTextureSize);
    }

    /// <summary>
    ///     Updates the map and bitmap.
    /// </summary>
    private void UpdateMapAndBitmap()
    {
        BitmapLayerOne = Helper.GenerateImage(AuroraWidth, AuroraHeight, AuroraTextureSize,
            AuroraTextures, AuroraMap);

        LayerOne.Source = BitmapLayerOne.ToBitmapImage();
    }


    /// <summary>
    ///     Handles the MouseDown event of the Touch control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
    private void Touch_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _cursor = new Coordinate2D();

        var position = e.GetPosition(Touch);

        if (position.X < AuroraTextureSize)
        {
            _cursor.X = 0;
        }
        else
        {
            _cursor.X = (int)position.X / AuroraTextureSize;
        }

        if (position.Y < AuroraTextureSize)
        {
            _cursor.Y = 0;
        }
        else
        {
            _cursor.Y = (int)position.Y / AuroraTextureSize;
        }

        var id = _cursor.CalculateId(AuroraWidth);

        TileClicked?.Invoke(this, id);
    }

    /// <summary>
    ///     Occurs when [delete logic].
    /// </summary>
    public event EventHandler<int> TileClicked;
}
