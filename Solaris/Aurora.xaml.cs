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
        public static readonly DependencyProperty MapHeight = DependencyProperty.Register(nameof(AuroraHeight),
            typeof(int),
            typeof(Aurora), null);

        /// <summary>
        /// The map width
        /// </summary>
        public static readonly DependencyProperty MapWidth = DependencyProperty.Register(nameof(AuroraWidth),
            typeof(int),
            typeof(Aurora), null);

        /// <summary>
        /// The texture size
        /// </summary>
        public static readonly DependencyProperty TextureSize = DependencyProperty.Register(nameof(AuroraTextureSize),
            typeof(int),
            typeof(Aurora), null);

        /// <summary>
        /// The map
        /// </summary>
        public static readonly DependencyProperty Map = DependencyProperty.Register(nameof(AuroraMap),
            typeof(Dictionary<int, List<int>>),
            typeof(Aurora), null);

        /// <summary>
        /// The avatar
        /// </summary>
        public static readonly DependencyProperty Avatar = DependencyProperty.Register(nameof(AuroraAvatar),
            typeof(Bitmap),
            typeof(Aurora), null);

        /// <summary>
        /// The movement
        /// </summary>
        public static readonly DependencyProperty Movement = DependencyProperty.Register(nameof(AuroraMovement),
            typeof(List<int>),
            typeof(Aurora), null);

        /// <summary>
        /// The textures
        /// </summary>
        public static readonly DependencyProperty Textures = DependencyProperty.Register(nameof(AuroraTextures),
            typeof(Dictionary<int, Texture>),
            typeof(Aurora), null);

        /// <summary>
        /// The grid
        /// </summary>
        public static readonly DependencyProperty Grid = DependencyProperty.Register(nameof(AuroraGrid),
            typeof(bool),
            typeof(Aurora), null);

        /// <summary>
        /// The add
        /// </summary>
        public static readonly DependencyProperty Add = DependencyProperty.Register(nameof(AuroraAdd),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        /// <summary>
        /// The remove
        /// </summary>
        public static readonly DependencyProperty Remove = DependencyProperty.Register(nameof(AuroraRemove),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        /// <summary>
        /// The add display
        /// </summary>
        public static readonly DependencyProperty AddDisplay = DependencyProperty.Register(nameof(AuroraAddDisplay),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        /// <summary>
        /// The remove display
        /// </summary>
        public static readonly DependencyProperty RemoveDisplay = DependencyProperty.Register(nameof(AuroraRemoveDisplay),
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
        public int AuroraHeight
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
        public int AuroraWidth
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
        public int AuroraTextureSize
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
        public Dictionary<int, List<int>> AuroraMap
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
        public Bitmap AuroraAvatar
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
        public Dictionary<int, Texture> AuroraTextures
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
        public KeyValuePair<int, int> AuroraAdd
        {
            get => (KeyValuePair<int, int>)GetValue(Add);
            set
            {
                SetValue(Add, value);
                var (check, dictionary) = Helper.AddTile(AuroraMap, value);

                if (!check)
                {
                    return;
                }

                AuroraMap = dictionary;

                BitmapLayerOne = Helper.GenerateImage(AuroraWidth, AuroraHeight, AuroraTextureSize,
                    AuroraTextures, AuroraMap);

                LayerOne.Source = BitmapLayerOne.ToBitmapImage();
            }
        }

        /// <summary>
        /// Gets or sets the dependency remove.
        /// </summary>
        /// <value>
        /// The dependency remove.
        /// </value>
        public KeyValuePair<int, int> AuroraRemove
        {
            get => (KeyValuePair<int, int>)GetValue(Remove);
            set
            {
                SetValue(Remove, value);

                var (check, dictionary) = Helper.RemoveTile(AuroraMap, AuroraTextures, value);

                if (!check)
                {
                    return;
                }

                AuroraMap = dictionary;

                BitmapLayerOne = Helper.GenerateImage(AuroraWidth, AuroraHeight, AuroraTextureSize,
                    AuroraTextures, AuroraMap);

                LayerOne.Source = BitmapLayerOne.ToBitmapImage();
            }
        }

        /// <summary>
        /// Gets or sets the dependency add display.
        /// </summary>
        /// <value>
        /// The dependency add display.
        /// </value>
        public KeyValuePair<int, int> AuroraAddDisplay
        {
            get => (KeyValuePair<int, int>)GetValue(AddDisplay);
            set
            {
                SetValue(AddDisplay, value);
                var bmp = Helper.AddDisplay(AuroraWidth, AuroraTextureSize,
                    AuroraTextures, _thirdLayer, value);

                LayerThree.Source = bmp.ToBitmapImage();
            }
        }

        /// <summary>
        /// Gets or sets the dependency remove display.
        /// </summary>
        /// <value>
        /// The dependency remove display.
        /// </value>
        public int AuroraRemoveDisplay
        {
            get => (int)GetValue(RemoveDisplay);
            set
            {
                SetValue(RemoveDisplay, value);
                var bmp = Helper.RemoveDisplay(AuroraWidth, AuroraTextureSize, _thirdLayer, value);

                LayerThree.Source = bmp.ToBitmapImage();
            }
        }

        /// <summary>
        /// Gets or sets the dependency movement.
        /// </summary>
        /// <value>
        /// The dependency movement.
        /// </value>
        public List<int> AuroraMovement
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
                Helper.DisplayMovement(this, value, AuroraAvatar, AuroraWidth, AuroraHeight,
                    AuroraTextureSize);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [dependency grid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [dependency grid]; otherwise, <c>false</c>.
        /// </value>
        public bool AuroraGrid
        {
            get => (bool)GetValue(Grid);
            set
            {
                SetValue(Grid, value);
                LayerTwo.Source = !AuroraGrid
                    ? null
                    : Helper.GenerateGrid(AuroraWidth, AuroraHeight, AuroraTextureSize);
            }
        }

        /// <summary>
        /// Initiates this instance.
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
        /// Handles the MouseDown event of the Touch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void Touch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _cursor = new Cursor();
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
                _cursor.Y = (int)position.X / AuroraTextureSize;
            }
        }
    }
}
