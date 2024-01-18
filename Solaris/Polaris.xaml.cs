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

namespace Solaris
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///     Generate a playing field for the editor
    /// </summary>
    public sealed partial class Polaris
    {
        /// <summary>
        /// The editor height
        /// </summary>
        public static readonly DependencyProperty EditorHeightProperty = DependencyProperty.Register(nameof(PolarisHeight),
            typeof(int),
            typeof(Polaris), null);

        /// <summary>
        /// The editor width
        /// </summary>
        public static readonly DependencyProperty EditorWidthProperty = DependencyProperty.Register(nameof(PolarisWidth),
            typeof(int),
            typeof(Polaris), null);

        /// <summary>
        /// The editor texture size
        /// </summary>
        public static readonly DependencyProperty EditorTextureSizeProperty = DependencyProperty.Register(
            nameof(PolarisTextureSize),
            typeof(int),
            typeof(Polaris), null);

        /// <summary>
        /// The editor map
        /// </summary>
        public static readonly DependencyProperty EditorMapProperty = DependencyProperty.Register(nameof(PolarisMap),
            typeof(Dictionary<int, List<int>>),
            typeof(Polaris), null);

        /// <summary>
        /// The editor textures
        /// </summary>
        public static readonly DependencyProperty EditorTexturesProperty = DependencyProperty.Register(nameof(PolarisTextures),
            typeof(Dictionary<int, Texture>),
            typeof(Polaris), null);

        /// <summary>
        /// The editor grid
        /// </summary>
        public static readonly DependencyProperty EditorGridProperty = DependencyProperty.Register(nameof(PolarisGrid),
            typeof(bool),
            typeof(Polaris), null);

        /// <summary>
        /// The editor number
        /// </summary>
        public static readonly DependencyProperty EditorNumberProperty = DependencyProperty.Register(nameof(PolarisNumber),
            typeof(bool),
            typeof(Polaris), null);

        /// <summary>
        /// The editor add
        /// </summary>
        public static readonly DependencyProperty EditorAddProperty = DependencyProperty.Register(nameof(PolarisAdd),
            typeof(KeyValuePair<int, int>),
            typeof(Polaris), null);

        /// <summary>
        /// The editor remove
        /// </summary>
        public static readonly DependencyProperty EditorRemoveProperty = DependencyProperty.Register(nameof(PolarisRemove),
            typeof(KeyValuePair<int, int>),
            typeof(Polaris), null);

        /// <summary>
        /// The editor add display
        /// </summary>
        public static readonly DependencyProperty EditorAddDisplayProperty = DependencyProperty.Register(
            nameof(PolarisAddDisplay),
            typeof(KeyValuePair<int, int>),
            typeof(Polaris), null);

        /// <summary>
        /// The editor remove display
        /// </summary>
        public static readonly DependencyProperty EditorRemoveDisplayProperty = DependencyProperty.Register(
            nameof(PolarisRemoveDisplay),
            typeof(int),
            typeof(Polaris), null);

        /// <summary>
        /// The cursor
        /// </summary>
        private Cursor _cursor;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="Polaris" /> class.
        /// </summary>
        public Polaris()
        {
            InitializeComponent();
            Initiate();
        }

        /// <summary>
        /// Gets the bitmap layer three.
        /// </summary>
        /// <value>
        /// The bitmap layer three.
        /// </value>
        internal Bitmap BitmapLayerThree { get; private set; }

        /// <summary>
        /// Gets the bitmap layer one.
        /// </summary>
        /// <value>
        /// The bitmap layer one.
        /// </value>
        internal Bitmap BitmapLayerOne { get; private set; }

        /// <summary>
        /// Gets or sets the height of the polaris.
        /// </summary>
        /// <value>
        /// The height of the polaris.
        /// </value>
        public int PolarisHeight
        {
            get => (int)GetValue(EditorHeightProperty);
            set => SetValue(EditorHeightProperty, value);
        }

        /// <summary>
        /// Gets or sets the width of the polaris.
        /// </summary>
        /// <value>
        /// The width of the polaris.
        /// </value>
        public int PolarisWidth
        {
            get => (int)GetValue(EditorWidthProperty);
            set => SetValue(EditorWidthProperty, value);
        }

        /// <summary>
        /// Gets or sets the size of the polaris texture.
        /// </summary>
        /// <value>
        /// The size of the polaris texture.
        /// </value>
        public int PolarisTextureSize
        {
            get => (int)GetValue(EditorTextureSizeProperty);
            set => SetValue(EditorTextureSizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the polaris map.
        /// </summary>
        /// <value>
        /// The polaris map.
        /// </value>
        public Dictionary<int, List<int>> PolarisMap
        {
            get => (Dictionary<int, List<int>>)GetValue(EditorMapProperty);
            set
            {
                if (value == null)
                {
                    return;
                }

                SetValue(EditorMapProperty, value);

                BitmapLayerOne = Helper.GenerateImage(PolarisWidth, PolarisHeight, PolarisTextureSize,
                    PolarisTextures, PolarisMap);

                BitmapLayerOne = Helper.GenerateImage(PolarisWidth, PolarisHeight, PolarisTextureSize,
                    PolarisTextures, PolarisMap);

                LayerOne.Source = BitmapLayerOne.ToBitmapImage();
            }
        }

        /// <summary>
        /// Gets or sets the dependency textures.
        /// </summary>
        /// <value>
        /// The dependency textures.
        /// </value>
        public Dictionary<int, Texture> PolarisTextures
        {
            get => (Dictionary<int, Texture>)GetValue(EditorTexturesProperty);
            set => SetValue(EditorTexturesProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [dependency grid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [dependency grid]; otherwise, <c>false</c>.
        /// </value>
        public bool PolarisGrid
        {
            get => (bool)GetValue(EditorGridProperty);
            set
            {
                SetValue(EditorGridProperty, value);
                LayerTwo.Source = !PolarisGrid
                    ? null
                    : Helper.GenerateGrid(PolarisWidth, PolarisHeight, PolarisTextureSize);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [dependency number].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [dependency number]; otherwise, <c>false</c>.
        /// </value>
        public bool PolarisNumber
        {
            get => (bool)GetValue(EditorNumberProperty);
            set
            {
                SetValue(EditorNumberProperty, value);
                LayerThree.Source = !PolarisNumber
                    ? null
                    : Helper.GenerateNumbers(PolarisWidth, PolarisHeight, PolarisTextureSize);
            }
        }

        /// <summary>
        /// Gets or sets the dependency add.
        /// </summary>
        /// <value>
        /// The dependency add.
        /// </value>
        public KeyValuePair<int, int> PolarisAdd
        {
            get => (KeyValuePair<int, int>)GetValue(EditorAddProperty);
            set
            {
                SetValue(EditorAddProperty, value);

                var (check, dictionary) = Helper.AddTile(PolarisMap, value);

                if (!check)
                {
                    return;
                }

                PolarisMap = dictionary;

                BitmapLayerOne = Helper.GenerateImage(PolarisWidth, PolarisHeight, PolarisTextureSize,
                    PolarisTextures, PolarisMap);

                LayerOne.Source = BitmapLayerOne.ToBitmapImage();
            }
        }

        /// <summary>
        /// Gets or sets the dependency remove.
        /// </summary>
        /// <value>
        /// The dependency remove.
        /// </value>
        public KeyValuePair<int, int> PolarisRemove
        {
            get => (KeyValuePair<int, int>)GetValue(EditorRemoveProperty);
            set
            {
                SetValue(EditorRemoveProperty, value);
                var (check, dictionary) = Helper.RemoveTile(PolarisMap, PolarisTextures, value);

                if (!check)
                {
                    return;
                }

                PolarisMap = dictionary;

                BitmapLayerOne = Helper.GenerateImage(PolarisWidth, PolarisHeight, PolarisTextureSize,
                    PolarisTextures, PolarisMap);

                LayerOne.Source = BitmapLayerOne.ToBitmapImage();
            }
        }

        /// <summary>
        /// Gets or sets the dependency add display.
        /// </summary>
        /// <value>
        /// The dependency add display.
        /// </value>
        public KeyValuePair<int, int> PolarisAddDisplay
        {
            get => (KeyValuePair<int, int>)GetValue(EditorAddDisplayProperty);
            set
            {
                SetValue(EditorAddDisplayProperty, value);
                var bmp = Helper.AddDisplay(PolarisWidth, PolarisTextureSize,
                    PolarisTextures, BitmapLayerThree, value);

                LayerThree.Source = bmp.ToBitmapImage();
            }
        }

        /// <summary>
        /// Gets or sets the dependency remove display.
        /// </summary>
        /// <value>
        /// The dependency remove display.
        /// </value>
        public int PolarisRemoveDisplay
        {
            get => (int)GetValue(EditorRemoveDisplayProperty);
            set
            {
                SetValue(EditorRemoveDisplayProperty, value);
                var bmp = Helper.RemoveDisplay(PolarisWidth, PolarisTextureSize, BitmapLayerThree, value);

                LayerThree.Source = bmp.ToBitmapImage();
            }
        }

        /// <summary>
        /// Initiates this instance.
        /// </summary>
        public void Initiate()
        {
            if (PolarisWidth == 0 || PolarisHeight == 0 || PolarisTextureSize == 0)
            {
                return;
            }

            Touch.Height = PolarisHeight * PolarisTextureSize;
            Touch.Width = PolarisWidth * PolarisTextureSize;

            if (PolarisGrid)
            {
                LayerTwo.Source = Helper.GenerateGrid(PolarisWidth, PolarisHeight, PolarisTextureSize);
            }

            if (PolarisNumber)
            {
                LayerThree.Source = Helper.GenerateNumbers(PolarisWidth, PolarisHeight, PolarisTextureSize);
            }

            BitmapLayerThree = new Bitmap(PolarisWidth * PolarisTextureSize,
                PolarisHeight * PolarisTextureSize);
        }

        /// <summary>
        /// Handles the MouseDown event of the Touch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Touch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _cursor = new Cursor();
            var position = e.GetPosition(Touch);

            if (position.X < PolarisTextureSize)
            {
                _cursor.X = 0;
            }
            else
            {
                _cursor.X = (int)position.X / PolarisTextureSize;
            }

            if (position.Y < PolarisTextureSize)
            {
                _cursor.Y = 0;
            }
            else
            {
                _cursor.Y = (int)position.X / PolarisTextureSize;
            }
        }
    }
}
