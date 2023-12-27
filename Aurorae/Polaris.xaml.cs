// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Aurorae
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    ///     Generate a playing field for the editor
    /// </summary>
    public partial class Polaris
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
            typeof(List<int>),
            typeof(Aurora), null);

        public static readonly DependencyProperty Textures = DependencyProperty.Register(nameof(Textures),
            typeof(List<Texture>),
            typeof(Aurora), null);

        public static readonly DependencyProperty Grid = DependencyProperty.Register(nameof(Grid),
            typeof(bool),
            typeof(Aurora), null);

        public static readonly DependencyProperty Number = DependencyProperty.Register(nameof(Number),
            typeof(bool),
            typeof(Aurora), null);

        private Cursor _cursor;

        public Polaris()
        {
            InitializeComponent();
            Initiate();
        }

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

        public List<int> DependencyMap
        {
            get => (List<int>)GetValue(Map);
            set => SetValue(Map, value);
        }

        public List<Texture> DependencyTextures
        {
            get => (List<Texture>)GetValue(Textures);
            set => SetValue(Map, value);
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
