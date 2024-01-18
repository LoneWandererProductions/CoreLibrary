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
        public static readonly DependencyProperty EditorHeight = DependencyProperty.Register(nameof(DependencyHeight),
            typeof(int),
            typeof(Aurora), null);

        /// <summary>
        /// The editor width
        /// </summary>
        public static readonly DependencyProperty EditorWidth = DependencyProperty.Register(nameof(DependencyWidth),
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
        public static readonly DependencyProperty EditorMap = DependencyProperty.Register(nameof(DependencyMap),
            typeof(Dictionary<int, List<int>>),
            typeof(Aurora), null);

        /// <summary>
        /// The editor textures
        /// </summary>
        public static readonly DependencyProperty EditorTextures = DependencyProperty.Register(nameof(DependencyTextures),
            typeof(Dictionary<int, Texture>),
            typeof(Aurora), null);

        /// <summary>
        /// The editor grid
        /// </summary>
        public static readonly DependencyProperty EditorGrid = DependencyProperty.Register(nameof(DependencyGrid),
            typeof(bool),
            typeof(Aurora), null);

        /// <summary>
        /// The editor number
        /// </summary>
        public static readonly DependencyProperty EditorNumber = DependencyProperty.Register(nameof(DependencyNumber),
            typeof(bool),
            typeof(Aurora), null);

        /// <summary>
        /// The editor add
        /// </summary>
        public static readonly DependencyProperty EditorAdd = DependencyProperty.Register(nameof(DependencyAdd),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        /// <summary>
        /// The editor remove
        /// </summary>
        public static readonly DependencyProperty EditorRemove = DependencyProperty.Register(nameof(DependencyRemove),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        /// <summary>
        /// The editor add display
        /// </summary>
        public static readonly DependencyProperty EditorAddDisplay = DependencyProperty.Register(
            nameof(DependencyAddDisplay),
            typeof(KeyValuePair<int, int>),
            typeof(Aurora), null);

        /// <summary>
        /// The editor remove display
        /// </summary>
        public static readonly DependencyProperty EditorRemoveDisplay = DependencyProperty.Register(
            nameof(DependencyRemoveDisplay),
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

        /// <summary>
        /// Gets or sets the dependency textures.
        /// </summary>
        /// <value>
        /// The dependency textures.
        /// </value>
        public Dictionary<int, Texture> DependencyTextures
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

        /// <summary>
        /// Gets or sets a value indicating whether [dependency number].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [dependency number]; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets or sets the dependency add.
        /// </summary>
        /// <value>
        /// The dependency add.
        /// </value>
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

        /// <summary>
        /// Gets or sets the dependency remove.
        /// </summary>
        /// <value>
        /// The dependency remove.
        /// </value>
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

        /// <summary>
        /// Gets or sets the dependency add display.
        /// </summary>
        /// <value>
        /// The dependency add display.
        /// </value>
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

        /// <summary>
        /// Gets or sets the dependency remove display.
        /// </summary>
        /// <value>
        /// The dependency remove display.
        /// </value>
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
