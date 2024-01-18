/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Solaris
 * FILE:        Solaris/Aurora.cs
 * PURPOSE:     Game Control
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
    ///     Generate a playing field
    /// </summary>
    public sealed partial class Aurora
    {
        /// <summary>
        /// The map height
        /// </summary>
        public static readonly DependencyProperty MapHeight = DependencyProperty.Register(nameof(DependencyHeight),
            typeof(int),
            typeof(Aurora), null);

        /// <summary>
        /// The map width
        /// </summary>
        public static readonly DependencyProperty MapWidth = DependencyProperty.Register(nameof(DependencyWidth),
            typeof(int),
            typeof(Aurora), null);

        /// <summary>
        /// The texture size
        /// </summary>
        public static readonly DependencyProperty TextureSize = DependencyProperty.Register(nameof(DependencyTextureSize),
            typeof(int),
            typeof(Aurora), null);

        /// <summary>
        /// The map
        /// </summary>
        public static readonly DependencyProperty Map = DependencyProperty.Register(nameof(DependencyMap),
            typeof(Dictionary<int, List<int>>),
            typeof(Aurora), null);

        /// <summary>
        /// The avatar
        /// </summary>
        public static readonly DependencyProperty Avatar = DependencyProperty.Register(nameof(DependencyAvatar),
            typeof(Bitmap),
            typeof(Aurora), null);

        /// <summary>
        /// The movement
        /// </summary>
        public static readonly DependencyProperty Movement = DependencyProperty.Register(nameof(DependencyMovement),
            typeof(List<int>),
            typeof(Aurora), null);

        /// <summary>
        /// The textures
        /// </summary>
        public static readonly DependencyProperty Textures = DependencyProperty.Register(nameof(DependencyTextures),
            typeof(Dictionary<int, Texture>),
            typeof(Aurora), null);

        /// <summary>
        /// The grid
        /// </summary>
        public static readonly DependencyProperty Grid = DependencyProperty.Register(nameof(DependencyGrid),
            typeof(bool),
            typeof(Aurora), null);

        /// <summary>
        /// The add
        /// </summary>
        public static readonly DependencyProperty Add = DependencyProperty.Register(nameof(DependencyAdd),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        /// <summary>
        /// The remove
        /// </summary>
        public static readonly DependencyProperty Remove = DependencyProperty.Register(nameof(DependencyRemove),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        /// <summary>
        /// The add display
        /// </summary>
        public static readonly DependencyProperty AddDisplay = DependencyProperty.Register(nameof(DependencyAddDisplay),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        /// <summary>
        /// The remove display
        /// </summary>
        public static readonly DependencyProperty RemoveDisplay = DependencyProperty.Register(nameof(DependencyRemoveDisplay),
            typeof(int),
            typeof(Aurora), null);

        /// <summary>
        /// The cursor
        /// </summary>
        private Cursor _cursor;

        /// <summary>
        /// The third layer
        /// </summary>
        private Bitmap _thirdLayer;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="Aurora" /> class.
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
        internal Bitmap BitmapLayerOne { get; private set; }

        /// <summary>
        /// Gets or sets the height of the dependency.
        /// </summary>
        /// <value>
        /// The height of the dependency.
        /// </value>
        public int DependencyHeight
        {
            get => (int)GetValue(MapHeight);
            set => SetValue(MapHeight, value);
        }

        /// <summary>
        /// Gets or sets the width of the dependency.
        /// </summary>
        /// <value>
        /// The width of the dependency.
        /// </value>
        public int DependencyWidth
        {
            get => (int)GetValue(MapWidth);
            set => SetValue(MapWidth, value);
        }

        /// <summary>
        /// Gets or sets the size of the dependency texture.
        /// </summary>
        /// <value>
        /// The size of the dependency texture.
        /// </value>
        public int DependencyTextureSize
        {
            get => (int)GetValue(TextureSize);
            set => SetValue(TextureSize, value);
        }

        /// <summary>
        /// Gets or sets the dependency map.
        /// </summary>
        /// <value>
        /// The dependency map.
        /// </value>
        public Dictionary<int, List<int>> DependencyMap
        {
            get => (Dictionary<int, List<int>>)GetValue(Map);
            set => SetValue(Map, value);
        }

        /// <summary>
        /// Gets or sets the dependency avatar.
        /// </summary>
        /// <value>
        /// The dependency avatar.
        /// </value>
        public Bitmap DependencyAvatar
        {
            get => (Bitmap)GetValue(Avatar);
            set => SetValue(Avatar, value);
        }

        /// <summary>
        /// Gets or sets the dependency textures.
        /// </summary>
        /// <value>
        /// The dependency textures.
        /// </value>
        public Dictionary<int, Texture> DependencyTextures
        {
            get => (Dictionary<int, Texture>)GetValue(Textures);
            set => SetValue(Textures, value);
        }

        /// <summary>
        /// Gets or sets the dependency add.
        /// </summary>
        /// <value>
        /// The dependency add.
        /// </value>
        public KeyValuePair<int, int> DependencyAdd
        {
            get => (KeyValuePair<int, int>)GetValue(Add);
            set
            {
                SetValue(Add, value);
                var (check, dictionary) = Helper.AddTile(DependencyMap, value);

                if (!check)
                {
                    return;
                }

                DependencyMap = dictionary;

                BitmapLayerOne = Helper.GenerateImage(DependencyWidth, DependencyHeight, DependencyTextureSize,
                    DependencyTextures, DependencyMap);

                LayerOne.Source = BitmapLayerOne.ToBitmapImage();
            }
        }

        /// <summary>
        /// Gets or sets the dependency remove.
        /// </summary>
        /// <value>
        /// The dependency remove.
        /// </value>
        public KeyValuePair<int, int> DependencyRemove
        {
            get => (KeyValuePair<int, int>)GetValue(Remove);
            set
            {
                SetValue(Remove, value);

                var (check, dictionary) = Helper.RemoveTile(DependencyMap, DependencyTextures, value);

                if (!check)
                {
                    return;
                }

                DependencyMap = dictionary;

                BitmapLayerOne = Helper.GenerateImage(DependencyWidth, DependencyHeight, DependencyTextureSize,
                    DependencyTextures, DependencyMap);

                LayerOne.Source = BitmapLayerOne.ToBitmapImage();
            }
        }

        /// <summary>
        /// Gets or sets the dependency add display.
        /// </summary>
        /// <value>
        /// The dependency add display.
        /// </value>
        public KeyValuePair<int, int> DependencyAddDisplay
        {
            get => (KeyValuePair<int, int>)GetValue(AddDisplay);
            set
            {
                SetValue(AddDisplay, value);
                var bmp = Helper.AddDisplay(DependencyWidth, DependencyTextureSize,
                    DependencyTextures, _thirdLayer, value);

                LayerThree.Source = bmp.ToBitmapImage();
            }
        }

        /// <summary>
        /// Gets or sets the dependency remove display.
        /// </summary>
        /// <value>
        /// The dependency remove display.
        /// </value>
        public int DependencyRemoveDisplay
        {
            get => (int)GetValue(RemoveDisplay);
            set
            {
                SetValue(RemoveDisplay, value);
                var bmp = Helper.RemoveDisplay(DependencyWidth, DependencyTextureSize, _thirdLayer, value);

                LayerThree.Source = bmp.ToBitmapImage();
            }
        }

        /// <summary>
        /// Gets or sets the dependency movement.
        /// </summary>
        /// <value>
        /// The dependency movement.
        /// </value>
        public List<int> DependencyMovement
        {
            get => (List<int>)GetValue(Movement);
            set
            {
                if (value == null)
                {
                    return;
                }

                SetValue(Movement, value);

                //display an movement Animation, block the whole control while displaying
                Helper.DisplayMovement(this, value, DependencyAvatar, DependencyWidth, DependencyHeight,
                    DependencyTextureSize);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [dependency grid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [dependency grid]; otherwise, <c>false</c>.
        /// </value>
        public bool DependencyGrid
        {
            get => (bool)GetValue(Grid);
            set
            {
                SetValue(Grid, value);
                LayerTwo.Source = !DependencyGrid
                    ? null
                    : Helper.GenerateGrid(DependencyWidth, DependencyHeight, DependencyTextureSize);
            }
        }

        /// <summary>
        /// Initiates this instance.
        /// </summary>
        public void Initiate()
        {
            if (DependencyWidth == 0 || DependencyHeight == 0 || DependencyTextureSize == 0)
            {
                return;
            }

            Touch.Height = DependencyHeight * DependencyTextureSize;
            Touch.Width = DependencyWidth * DependencyTextureSize;

            BitmapLayerOne = Helper.GenerateImage(DependencyWidth, DependencyHeight, DependencyTextureSize,
                DependencyTextures, DependencyMap);

            LayerOne.Source = BitmapLayerOne.ToBitmapImage();

            if (DependencyGrid)
            {
                LayerTwo.Source = Helper.GenerateGrid(DependencyWidth, DependencyHeight, DependencyTextureSize);
            }

            _thirdLayer = new Bitmap(DependencyWidth * DependencyTextureSize, DependencyHeight * DependencyTextureSize);
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

            if (position.X < DependencyTextureSize)
            {
                _cursor.X = 0;
            }
            else
            {
                _cursor.X = (int)position.X / DependencyTextureSize;
            }

            if (position.Y < DependencyTextureSize)
            {
                _cursor.Y = 0;
            }
            else
            {
                _cursor.Y = (int)position.X / DependencyTextureSize;
            }
        }
    }
}
