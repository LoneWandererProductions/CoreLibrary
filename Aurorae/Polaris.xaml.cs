﻿/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Aurorae
 * FILE:        Aurorae/Polaris.cs
 * PURPOSE:     Editor Control
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

namespace Aurorae
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///     Generate a playing field for the editor
    /// </summary>
    public partial class Polaris
    {
        public static readonly DependencyProperty EditorHeight = DependencyProperty.Register(nameof(EditorHeight),
            typeof(int),
            typeof(Aurora), null);

        public static readonly DependencyProperty EditorWidth = DependencyProperty.Register(nameof(EditorWidth),
            typeof(int),
            typeof(Aurora), null);

        public static readonly DependencyProperty TextureSize = DependencyProperty.Register(nameof(TextureSize),
            typeof(int),
            typeof(Aurora), null);

        public static readonly DependencyProperty Map = DependencyProperty.Register(nameof(Map),
            typeof(Dictionary<int, List<int>>),
            typeof(Aurora), null);

        public static readonly DependencyProperty Textures = DependencyProperty.Register(nameof(Textures),
            typeof(Dictionary<int, Texture>),
            typeof(Aurora), null);

        public static readonly DependencyProperty Grid = DependencyProperty.Register(nameof(Grid),
            typeof(bool),
            typeof(Aurora), null);

        public static readonly DependencyProperty Number = DependencyProperty.Register(nameof(Number),
            typeof(bool),
            typeof(Aurora), null);

        public static readonly DependencyProperty Add = DependencyProperty.Register(nameof(Add),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        public static readonly DependencyProperty Remove = DependencyProperty.Register(nameof(Remove),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        public static readonly DependencyProperty AddDisplay = DependencyProperty.Register(nameof(Add),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        public static readonly DependencyProperty RemoveDisplay = DependencyProperty.Register(nameof(Remove),
            typeof(int),
            typeof(Aurora), null);

        private Cursor _cursor;

        private Bitmap _thirdLayer;

        public Polaris()
        {
            InitializeComponent();
            Initiate();
        }

        public int DependencyHeight
        {
            get => (int)GetValue(EditorHeight);
            set => SetValue(EditorHeight, value);
        }

        public int DependencyWidth
        {
            get => (int)GetValue(EditorWidth);
            set => SetValue(EditorWidth, value);
        }

        public int DependencyTextureSize
        {
            get => (int)GetValue(TextureSize);
            set => SetValue(TextureSize, value);
        }

        public Dictionary<int, List<int>> DependencyMap
        {
            get => (Dictionary<int, List<int>>)GetValue(Map);
            set
            {
                if (value == null)
                {
                    return;
                }

                SetValue(Map, value);
                LayerOne.Source = Helper.GenerateImage(DependencyWidth, DependencyHeight, DependencyTextureSize,
                    DependencyTextures, DependencyMap);
            }
        }

        public Dictionary<int, Texture> DependencyTextures
        {
            get => (Dictionary<int, Texture>)GetValue(Textures);
            set => SetValue(Textures, value);
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

        public bool DependencyNumber
        {
            get => (bool)GetValue(Number);
            set
            {
                SetValue(Number, value);
                LayerThree.Source = !DependencyNumber
                    ? null
                    : Helper.GenerateNumbers(DependencyWidth, DependencyHeight, DependencyTextureSize);
            }
        }

        public KeyValuePair<int, int> DependencyAdd
        {
            get => (KeyValuePair<int, int>)GetValue(Add);
            set
            {
                SetValue(Add, value);
                var check = Helper.AddTile(DependencyMap, value);
                if (!check)
                {
                    return;
                }

                LayerOne.Source = Helper.GenerateImage(DependencyWidth, DependencyHeight, DependencyTextureSize,
                    DependencyTextures, DependencyMap);
            }
        }

        public KeyValuePair<int, int> DependencyRemove
        {
            get => (KeyValuePair<int, int>)GetValue(Remove);
            set
            {
                SetValue(Remove, value);
                var check = Helper.RemoveTile(DependencyMap, DependencyTextures, value);
                if (!check)
                {
                    return;
                }

                LayerOne.Source = Helper.GenerateImage(DependencyWidth, DependencyHeight, DependencyTextureSize,
                    DependencyTextures, DependencyMap);
            }
        }

        public KeyValuePair<int, int> DependencyAddDisplay
        {
            get => (KeyValuePair<int, int>)GetValue(AddDisplay);
            set
            {
                SetValue(AddDisplay, value);
                Helper.AddDisplay(DependencyWidth, DependencyTextureSize,
                    DependencyTextures, _thirdLayer, value);

                LayerThree.Source = _thirdLayer.ToBitmapImage();
            }
        }

        public int DependencyRemoveDisplay
        {
            get => (int)GetValue(RemoveDisplay);
            set
            {
                SetValue(RemoveDisplay, value);
                Helper.RemoveDisplay(DependencyWidth, DependencyTextureSize, _thirdLayer, value);

                LayerThree.Source = _thirdLayer.ToBitmapImage();
            }
        }

        private void Initiate()
        {
            if (DependencyWidth == 0 || DependencyHeight == 0 || DependencyTextureSize == 0)
            {
                return;
            }

            Touch.Height = DependencyHeight * DependencyTextureSize;
            Touch.Width = DependencyWidth * DependencyTextureSize;

            //LayerOne.Source = Helper.EmptyImage(DependencyWidth, DependencyHeight, DependencyTextureSize);

            if (DependencyGrid)
            {
                LayerTwo.Source = Helper.GenerateGrid(DependencyWidth, DependencyHeight, DependencyTextureSize);
            }

            if (DependencyNumber)
            {
                LayerThree.Source = Helper.GenerateNumbers(DependencyWidth, DependencyHeight, DependencyTextureSize);
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
