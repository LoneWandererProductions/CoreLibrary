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
        public static readonly DependencyProperty EditorHeight = DependencyProperty.Register(nameof(PolarisHeight),
            typeof(int),
            typeof(Aurora), null);

        /// <summary>
        /// The editor width
        /// </summary>
        public static readonly DependencyProperty EditorWidth = DependencyProperty.Register(nameof(PolarisWidth),
            typeof(int),
            typeof(Aurora), null);

        /// <summary>
        /// The editor texture size
        /// </summary>
        public static readonly DependencyProperty EditorTextureSize = DependencyProperty.Register(
            nameof(EditorTextureSize),
            typeof(int),
            typeof(Aurora), null);

        /// <summary>
        /// The editor map
        /// </summary>
        public static readonly DependencyProperty EditorMap = DependencyProperty.Register(nameof(PolarisMap),
            typeof(Dictionary<int, List<int>>),
            typeof(Aurora), null);

        /// <summary>
        /// The editor textures
        /// </summary>
        public static readonly DependencyProperty EditorTextures = DependencyProperty.Register(nameof(PolarisTextures),
            typeof(Dictionary<int, Texture>),
            typeof(Aurora), null);

        /// <summary>
        /// The editor grid
        /// </summary>
        public static readonly DependencyProperty EditorGrid = DependencyProperty.Register(nameof(PolarisGrid),
            typeof(bool),
            typeof(Aurora), null);

        /// <summary>
        /// The editor number
        /// </summary>
        public static readonly DependencyProperty EditorNumber = DependencyProperty.Register(nameof(PolarisNumber),
            typeof(bool),
            typeof(Aurora), null);

        /// <summary>
        /// The editor add
        /// </summary>
        public static readonly DependencyProperty EditorAdd = DependencyProperty.Register(nameof(PolarisAdd),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        /// <summary>
        /// The editor remove
        /// </summary>
        public static readonly DependencyProperty EditorRemove = DependencyProperty.Register(nameof(PolarisRemove),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        /// <summary>
        /// The editor add display
        /// </summary>
        public static readonly DependencyProperty EditorAddDisplay = DependencyProperty.Register(
            nameof(PolarisAddDisplay),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        /// <summary>
        /// The editor remove display
        /// </summary>
        public static readonly DependencyProperty EditorRemoveDisplay = DependencyProperty.Register(
            nameof(PolarisRemoveDisplay),
            typeof(int),
            typeof(Aurora), null);

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
            get => (int)GetValue(EditorHeight);
            set => SetValue(EditorHeight, value);
        }

        /// <summary>
        /// Gets or sets the width of the polaris.
        /// </summary>
        /// <value>
        /// The width of the polaris.
        /// </value>
        public int PolarisWidth
        {
            get => (int)GetValue(EditorWidth);
            set => SetValue(EditorWidth, value);
        }

        /// <summary>
        /// Gets or sets the size of the polaris texture.
        /// </summary>
        /// <value>
        /// The size of the polaris texture.
        /// </value>
        public int PolarisTextureSize
        {
            get => (int)GetValue(EditorTextureSize);
            set => SetValue(EditorTextureSize, value);
        }

        /// <summary>
        /// Gets or sets the polaris map.
        /// </summary>
        /// <value>
        /// The polaris map.
        /// </value>
        public Dictionary<int, List<int>> PolarisMap
        {
            get => (Dictionary<int, List<int>>)GetValue(EditorMap);
            set
            {
                if (value == null)
                {
                    return;
                }

                SetValue(EditorMap, value);

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
            get => (Dictionary<int, Texture>)GetValue(EditorTextures);
            set => SetValue(EditorTextures, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether [dependency grid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [dependency grid]; otherwise, <c>false</c>.
        /// </value>
        public bool PolarisGrid
        {
            get => (bool)GetValue(EditorGrid);
            set
            {
                SetValue(EditorGrid, value);
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
            get => (bool)GetValue(EditorNumber);
            set
            {
                SetValue(EditorNumber, value);
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
            get => (KeyValuePair<int, int>)GetValue(EditorAdd);
            set
            {
                SetValue(EditorAdd, value);

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
            get => (KeyValuePair<int, int>)GetValue(EditorRemove);
            set
            {
                SetValue(EditorRemove, value);
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
            get => (KeyValuePair<int, int>)GetValue(EditorAddDisplay);
            set
            {
                SetValue(EditorAddDisplay, value);
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
            get => (int)GetValue(EditorRemoveDisplay);
            set
            {
                SetValue(EditorRemoveDisplay, value);
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
