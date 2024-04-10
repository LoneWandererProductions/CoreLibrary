/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Solaris/Polaris.cs
 * PURPOSE:     Editor Control
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.Collections.Generic;
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
    ///     Generate a playing field for the editor
    /// </summary>
    public sealed partial class Polaris
    {
        /// <summary>
        ///     The editor height
        /// </summary>
        public static readonly DependencyProperty PolarisHeightProperty = DependencyProperty.Register(
            nameof(PolarisHeight),
            typeof(int),
            typeof(Polaris), new PropertyMetadata(100));

        /// <summary>
        ///     The editor width
        /// </summary>
        public static readonly DependencyProperty PolarisWidthProperty = DependencyProperty.Register(
            nameof(PolarisWidth),
            typeof(int),
            typeof(Polaris), new PropertyMetadata(100));

        /// <summary>
        ///     The editor texture size
        /// </summary>
        public static readonly DependencyProperty PolarisTextureSizeProperty = DependencyProperty.Register(
            nameof(PolarisTextureSize),
            typeof(int),
            typeof(Polaris), new PropertyMetadata(100));

        /// <summary>
        ///     The editor map
        /// </summary>
        public static readonly DependencyProperty PolarisMapProperty = DependencyProperty.Register(nameof(PolarisMap),
            typeof(Dictionary<int, List<int>>),
            typeof(Polaris), null);

        /// <summary>
        ///     The editor textures
        /// </summary>
        public static readonly DependencyProperty PolarisTexturesProperty = DependencyProperty.Register(
            nameof(PolarisTextures),
            typeof(Dictionary<int, Texture>),
            typeof(Polaris), null);

        /// <summary>
        ///     The editor grid
        /// </summary>
        public static readonly DependencyProperty PolarisGridProperty = DependencyProperty.Register(nameof(PolarisGrid),
            typeof(bool),
            typeof(Polaris), new PropertyMetadata(false));

        /// <summary>
        ///     The editor number
        /// </summary>
        public static readonly DependencyProperty PolarisNumberProperty = DependencyProperty.Register(
            nameof(PolarisNumber),
            typeof(bool),
            typeof(Polaris), new PropertyMetadata(false));

        /// <summary>
        ///     The editor add
        /// </summary>
        public static readonly DependencyProperty PolarisAddProperty = DependencyProperty.Register(nameof(PolarisAdd),
            typeof(KeyValuePair<int, int>),
            typeof(Polaris), null);

        /// <summary>
        ///     The editor remove
        /// </summary>
        public static readonly DependencyProperty PolarisRemoveProperty = DependencyProperty.Register(
            nameof(PolarisRemove),
            typeof(KeyValuePair<int, int>),
            typeof(Polaris), null);

        /// <summary>
        ///     The editor add display
        /// </summary>
        public static readonly DependencyProperty PolarisAddDisplayProperty = DependencyProperty.Register(
            nameof(PolarisAddDisplay),
            typeof(KeyValuePair<int, int>),
            typeof(Polaris), null);

        /// <summary>
        ///     The editor remove display
        /// </summary>
        public static readonly DependencyProperty PolarisRemoveDisplayProperty = DependencyProperty.Register(
            nameof(PolarisRemoveDisplay),
            typeof(int),
            typeof(Polaris), null);

        /// <summary>
        ///     The cursor
        /// </summary>
        private Coordinate2D _cursor;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="Polaris" /> class.
        /// </summary>
        public Polaris()
        {
            InitializeComponent();
            Initiate();
        }

        /// <summary>
        ///     Gets the bitmap layer three.
        /// </summary>
        /// <value>
        ///     The bitmap layer three.
        /// </value>
        internal Bitmap BitmapLayerThree { get; private set; }

        /// <summary>
        ///     Gets the bitmap layer one.
        /// </summary>
        /// <value>
        ///     The bitmap layer one.
        /// </value>
        internal Bitmap BitmapLayerOne { get; private set; }

        /// <summary>
        ///     Gets or sets the height of the polaris.
        /// </summary>
        /// <value>
        ///     The height of the polaris.
        /// </value>
        public int PolarisHeight
        {
            get => (int)GetValue(PolarisHeightProperty);
            set => SetValue(PolarisHeightProperty, value);
        }

        /// <summary>
        ///     Gets or sets the width of the polaris.
        /// </summary>
        /// <value>
        ///     The width of the polaris.
        /// </value>
        public int PolarisWidth
        {
            get => (int)GetValue(PolarisWidthProperty);
            set => SetValue(PolarisWidthProperty, value);
        }

        /// <summary>
        ///     Gets or sets the size of the polaris texture.
        /// </summary>
        /// <value>
        ///     The size of the polaris texture.
        /// </value>
        public int PolarisTextureSize
        {
            get => (int)GetValue(PolarisTextureSizeProperty);
            set => SetValue(PolarisTextureSizeProperty, value);
        }

        /// <summary>
        ///     Gets or sets the polaris map.
        /// </summary>
        /// <value>
        ///     The polaris map.
        /// </value>
        public Dictionary<int, List<int>> PolarisMap
        {
            get => (Dictionary<int, List<int>>)GetValue(PolarisMapProperty);
            set
            {
                if (value == null) return;

                SetValue(PolarisMapProperty, value);

                BitmapLayerOne = Helper.GenerateImage(PolarisWidth, PolarisHeight, PolarisTextureSize,
                    PolarisTextures, PolarisMap);

                BitmapLayerOne = Helper.GenerateImage(PolarisWidth, PolarisHeight, PolarisTextureSize,
                    PolarisTextures, PolarisMap);

                LayerOne.Source = BitmapLayerOne.ToBitmapImage();
            }
        }

        /// <summary>
        ///     Gets or sets the dependency textures.
        /// </summary>
        /// <value>
        ///     The dependency textures.
        /// </value>
        public Dictionary<int, Texture> PolarisTextures
        {
            get => (Dictionary<int, Texture>)GetValue(PolarisTexturesProperty);
            set => SetValue(PolarisTexturesProperty, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [dependency grid].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [dependency grid]; otherwise, <c>false</c>.
        /// </value>
        public bool PolarisGrid
        {
            get => (bool)GetValue(PolarisGridProperty);
            set
            {
                SetValue(PolarisGridProperty, value);
                LayerTwo.Source = !PolarisGrid
                    ? null
                    : Helper.GenerateGrid(PolarisWidth, PolarisHeight, PolarisTextureSize);
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether [dependency number].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [dependency number]; otherwise, <c>false</c>.
        /// </value>
        public bool PolarisNumber
        {
            get => (bool)GetValue(PolarisNumberProperty);
            set
            {
                SetValue(PolarisNumberProperty, value);
                LayerThree.Source = !PolarisNumber
                    ? null
                    : Helper.GenerateNumbers(PolarisWidth, PolarisHeight, PolarisTextureSize);
            }
        }

        /// <summary>
        ///     Gets or sets the dependency add.
        /// </summary>
        /// <value>
        ///     The dependency add.
        /// </value>
        public KeyValuePair<int, int> PolarisAdd
        {
            get => (KeyValuePair<int, int>)GetValue(PolarisAddProperty);
            set
            {
                SetValue(PolarisAddProperty, value);

                var (check, dictionary) = Helper.AddTile(PolarisMap, value);

                if (!check) return;

                PolarisMap = dictionary;

                BitmapLayerOne = Helper.GenerateImage(PolarisWidth, PolarisHeight, PolarisTextureSize,
                    PolarisTextures, PolarisMap);

                LayerOne.Source = BitmapLayerOne.ToBitmapImage();
            }
        }

        /// <summary>
        ///     Gets or sets the dependency remove.
        /// </summary>
        /// <value>
        ///     The dependency remove.
        /// </value>
        public KeyValuePair<int, int> PolarisRemove
        {
            get => (KeyValuePair<int, int>)GetValue(PolarisRemoveProperty);
            set
            {
                SetValue(PolarisRemoveProperty, value);
                var (check, dictionary) = Helper.RemoveTile(PolarisMap, PolarisTextures, value);

                if (!check) return;

                PolarisMap = dictionary;

                BitmapLayerOne = Helper.GenerateImage(PolarisWidth, PolarisHeight, PolarisTextureSize,
                    PolarisTextures, PolarisMap);

                LayerOne.Source = BitmapLayerOne.ToBitmapImage();
            }
        }

        /// <summary>
        ///     Gets or sets the dependency add display.
        /// </summary>
        /// <value>
        ///     The dependency add display.
        /// </value>
        public KeyValuePair<int, int> PolarisAddDisplay
        {
            get => (KeyValuePair<int, int>)GetValue(PolarisAddDisplayProperty);
            set
            {
                SetValue(PolarisAddDisplayProperty, value);
                var bmp = Helper.AddDisplay(PolarisWidth, PolarisTextureSize,
                    PolarisTextures, BitmapLayerThree, value);

                LayerThree.Source = bmp.ToBitmapImage();
            }
        }

        /// <summary>
        ///     Gets or sets the dependency remove display.
        /// </summary>
        /// <value>
        ///     The dependency remove display.
        /// </value>
        public int PolarisRemoveDisplay
        {
            get => (int)GetValue(PolarisRemoveDisplayProperty);
            set
            {
                SetValue(PolarisRemoveDisplayProperty, value);
                var bmp = Helper.RemoveDisplay(PolarisWidth, PolarisTextureSize, BitmapLayerThree, value);

                LayerThree.Source = bmp.ToBitmapImage();
            }
        }

        /// <summary>
        ///     Initiates this instance.
        /// </summary>
        public void Initiate()
        {
            if (PolarisWidth == 0 || PolarisHeight == 0 || PolarisTextureSize == 0) return;

            Touch.Height = PolarisHeight * PolarisTextureSize;
            Touch.Width = PolarisWidth * PolarisTextureSize;

            if (PolarisGrid) LayerTwo.Source = Helper.GenerateGrid(PolarisWidth, PolarisHeight, PolarisTextureSize);

            if (PolarisNumber)
                LayerThree.Source = Helper.GenerateNumbers(PolarisWidth, PolarisHeight, PolarisTextureSize);

            BitmapLayerThree = new Bitmap(PolarisWidth * PolarisTextureSize,
                PolarisHeight * PolarisTextureSize);
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

            if (position.X < PolarisTextureSize)
                _cursor.X = 0;
            else
                _cursor.X = (int)position.X / PolarisTextureSize;

            if (position.Y < PolarisTextureSize)
                _cursor.Y = 0;
            else
                _cursor.Y = (int)position.X / PolarisTextureSize;
        }
    }
}