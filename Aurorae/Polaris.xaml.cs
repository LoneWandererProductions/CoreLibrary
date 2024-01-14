/*
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
    public sealed partial class Polaris
    {
        public static readonly DependencyProperty EditorHeight = DependencyProperty.Register(nameof(EditorHeight),
            typeof(int),
            typeof(Aurora), null);

        public static readonly DependencyProperty EditorWidth = DependencyProperty.Register(nameof(EditorWidth),
            typeof(int),
            typeof(Aurora), null);

        public static readonly DependencyProperty EditorTextureSize = DependencyProperty.Register(
            nameof(EditorTextureSize),
            typeof(int),
            typeof(Aurora), null);

        public static readonly DependencyProperty EditorMap = DependencyProperty.Register(nameof(EditorMap),
            typeof(Dictionary<int, List<int>>),
            typeof(Aurora), null);

        public static readonly DependencyProperty EditorTextures = DependencyProperty.Register(nameof(EditorTextures),
            typeof(Dictionary<int, Texture>),
            typeof(Aurora), null);

        public static readonly DependencyProperty EditorGrid = DependencyProperty.Register(nameof(EditorGrid),
            typeof(bool),
            typeof(Aurora), null);

        public static readonly DependencyProperty EditorNumber = DependencyProperty.Register(nameof(EditorNumber),
            typeof(bool),
            typeof(Aurora), null);

        public static readonly DependencyProperty EditorAdd = DependencyProperty.Register(nameof(EditorAdd),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        public static readonly DependencyProperty EditorRemove = DependencyProperty.Register(nameof(EditorRemove),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        public static readonly DependencyProperty EditorAddDisplay = DependencyProperty.Register(
            nameof(EditorAddDisplay),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        public static readonly DependencyProperty EditorRemoveDisplay = DependencyProperty.Register(
            nameof(EditorRemoveDisplay),
            typeof(int),
            typeof(Aurora), null);

        private Cursor _cursor;

        public Polaris()
        {
            InitializeComponent();
            Initiate();
        }

        internal Bitmap BitmapLayerThree { get; private set; }

        internal Bitmap BitmapLayerOne { get; private set; }

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
            get => (int)GetValue(EditorTextureSize);
            set => SetValue(EditorTextureSize, value);
        }

        public Dictionary<int, List<int>> DependencyMap
        {
            get => (Dictionary<int, List<int>>)GetValue(EditorMap);
            set
            {
                if (value == null)
                {
                    return;
                }

                SetValue(EditorMap, value);

                BitmapLayerOne = Helper.GenerateImage(DependencyWidth, DependencyHeight, DependencyTextureSize,
                    DependencyTextures, DependencyMap);

                BitmapLayerOne = Helper.GenerateImage(DependencyWidth, DependencyHeight, DependencyTextureSize,
                    DependencyTextures, DependencyMap);

                LayerOne.Source = BitmapLayerOne.ToBitmapImage();
            }
        }

        public Dictionary<int, Texture> DependencyTextures
        {
            get => (Dictionary<int, Texture>)GetValue(EditorTextures);
            set => SetValue(EditorTextures, value);
        }

        public bool DependencyGrid
        {
            get => (bool)GetValue(EditorGrid);
            set
            {
                SetValue(EditorGrid, value);
                LayerTwo.Source = !DependencyGrid
                    ? null
                    : Helper.GenerateGrid(DependencyWidth, DependencyHeight, DependencyTextureSize);
            }
        }

        public bool DependencyNumber
        {
            get => (bool)GetValue(EditorNumber);
            set
            {
                SetValue(EditorNumber, value);
                LayerThree.Source = !DependencyNumber
                    ? null
                    : Helper.GenerateNumbers(DependencyWidth, DependencyHeight, DependencyTextureSize);
            }
        }

        public KeyValuePair<int, int> DependencyAdd
        {
            get => (KeyValuePair<int, int>)GetValue(EditorAdd);
            set
            {
                SetValue(EditorAdd, value);

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
            get => (KeyValuePair<int, int>)GetValue(EditorRemove);
            set
            {
                SetValue(EditorRemove, value);
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
            get => (KeyValuePair<int, int>)GetValue(EditorAddDisplay);
            set
            {
                SetValue(EditorAddDisplay, value);
                var bmp = Helper.AddDisplay(DependencyWidth, DependencyTextureSize,
                    DependencyTextures, BitmapLayerThree, value);

                LayerThree.Source = bmp.ToBitmapImage();
            }
        }

        public int DependencyRemoveDisplay
        {
            get => (int)GetValue(EditorRemoveDisplay);
            set
            {
                SetValue(EditorRemoveDisplay, value);
                var bmp = Helper.RemoveDisplay(DependencyWidth, DependencyTextureSize, BitmapLayerThree, value);

                LayerThree.Source = bmp.ToBitmapImage();
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

            //LayerOne.Source = Helper.EmptyImage(DependencyWidth, DependencyHeight, DependencyTextureSize);

            if (DependencyGrid)
            {
                LayerTwo.Source = Helper.GenerateGrid(DependencyWidth, DependencyHeight, DependencyTextureSize);
            }

            if (DependencyNumber)
            {
                LayerThree.Source = Helper.GenerateNumbers(DependencyWidth, DependencyHeight, DependencyTextureSize);
            }

            BitmapLayerThree = new Bitmap(DependencyWidth * DependencyTextureSize,
                DependencyHeight * DependencyTextureSize);
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
