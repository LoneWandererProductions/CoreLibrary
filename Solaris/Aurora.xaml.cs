/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Aurorae
 * FILE:        Aurorae/Aurora.cs
 * PURPOSE:     Game Control
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

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
        public static readonly DependencyProperty MapHeight = DependencyProperty.Register(nameof(MapHeight),
            typeof(int),
            typeof(Aurora), null);

        public static readonly DependencyProperty MapWidth = DependencyProperty.Register(nameof(MapWidth),
            typeof(int),
            typeof(Aurora), null);

        public static readonly DependencyProperty TextureSize = DependencyProperty.Register(nameof(TextureSize),
            typeof(int),
            typeof(Aurora), null);

        public static readonly DependencyProperty Map = DependencyProperty.Register(nameof(Map),
            typeof(Dictionary<int, List<int>>),
            typeof(Aurora), null);

        public static readonly DependencyProperty Avatar = DependencyProperty.Register(nameof(Avatar),
            typeof(Bitmap),
            typeof(Aurora), null);

        public static readonly DependencyProperty Movement = DependencyProperty.Register(nameof(Movement),
            typeof(List<int>),
            typeof(Aurora), null);

        public static readonly DependencyProperty Textures = DependencyProperty.Register(nameof(Textures),
            typeof(Dictionary<int, Texture>),
            typeof(Aurora), null);

        public static readonly DependencyProperty Grid = DependencyProperty.Register(nameof(Grid),
            typeof(bool),
            typeof(Aurora), null);

        public static readonly DependencyProperty Add = DependencyProperty.Register(nameof(Add),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        public static readonly DependencyProperty Remove = DependencyProperty.Register(nameof(Remove),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        public static readonly DependencyProperty AddDisplay = DependencyProperty.Register(nameof(AddDisplay),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        public static readonly DependencyProperty RemoveDisplay = DependencyProperty.Register(nameof(RemoveDisplay),
            typeof(int),
            typeof(Aurora), null);

        private Cursor _cursor;

        private Bitmap _thirdLayer;

        public Aurora()
        {
            InitializeComponent();
        }

        internal Bitmap BitmapLayerOne { get; private set; }

        public int DependencyHeight
        {
            get => (int)GetValue(MapHeight);
            set => SetValue(MapHeight, value);
        }

        public int DependencyWidth
        {
            get => (int)GetValue(MapWidth);
            set => SetValue(MapWidth, value);
        }

        public int DependencyTextureSize
        {
            get => (int)GetValue(TextureSize);
            set => SetValue(TextureSize, value);
        }

        public Dictionary<int, List<int>> DependencyMap
        {
            get => (Dictionary<int, List<int>>)GetValue(Map);
            set => SetValue(Map, value);
        }

        public Bitmap DependencyAvatar
        {
            get => (Bitmap)GetValue(Avatar);
            set => SetValue(Avatar, value);
        }

        public Dictionary<int, Texture> DependencyTextures
        {
            get => (Dictionary<int, Texture>)GetValue(Textures);
            set => SetValue(Textures, value);
        }

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
