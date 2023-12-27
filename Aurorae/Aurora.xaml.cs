using Imaging;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExtendedSystemObjects;

namespace Aurorae
{
    /// <inheritdoc cref="UserControl" />
    /// <summary>
    /// Generate a playing field
    /// </summary>
    public partial class Aurora
    {
        private readonly ImageRender _render = new();

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

        private Bitmap _LayerOne;
        private Bitmap _LayerTwo;
        private Bitmap _LayerThree;
        private Cursor _cursor;

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
            set => SetValue(Grid, value);
        }

        public bool DependencyNumber
        {
            get => (bool)GetValue(Number);
            set => SetValue(Number, value);
        }

        public Aurora()
        {
            InitializeComponent();
            Initiate();
        }

        private void Initiate()
        {
            if (DependencyWidth == 0 || DependencyHeight == 0 || DependencyTextureSize == 0) return;

            _LayerOne = new Bitmap(DependencyWidth * DependencyTextureSize, DependencyHeight * DependencyTextureSize);
            Generate();

            if (DependencyGrid) _LayerTwo = new Bitmap(DependencyWidth * DependencyTextureSize, DependencyHeight * DependencyTextureSize);

            _LayerThree = new Bitmap(DependencyWidth * DependencyTextureSize, DependencyHeight * DependencyTextureSize);

            Touch.Height = _LayerOne.Height;
            Touch.Width = _LayerOne.Width;
        }

        private void Generate()
        {
            DrawMap();
            LayerOne.Source = _LayerOne.ToBitmapImage();

            if (DependencyGrid)
            {
                DrawGrid();
                LayerTwo.Source = _LayerTwo.ToBitmapImage();
            }

            if (DependencyNumber)
            {
                DrawNumbers();
                LayerThree.Source = _LayerThree.ToBitmapImage();
            }
        }

        private void DrawMap()
        {
            var boxes = new List<Box>();

            var layers = DependencyMap.ChunkBy(DependencyHeight);


            for (var y = 0; y < layers.Count; y++)
            {
                var slice = layers[y];

                for (var x = 0; x < slice.Count; x++)
                {
                    var id = slice[x];
                    if (id <= 0) continue;

                    var texture = DependencyTextures[id];
                    var image = _render.GetBitmapFile(texture.Path);

                    var box = new Box
                    {
                        X = x * DependencyTextureSize,
                        Y = y * DependencyTextureSize,
                        Image = image,
                        Layer = texture.Layer
                    };

                    boxes.Add(box);
                }
            }

            foreach (var slice in boxes.OrderBy(layer => layer.Layer).ToList())
            {
                _LayerOne = _render.CombineBitmap(_LayerOne, slice.Image, slice.X, slice.Y);
            }
        }

        private void DrawGrid()
        {
            for (int y = 0; y < DependencyHeight; y++)
            {
                for (int x = 0; x < DependencyWidth; x++)
                {
                    using Graphics graphics = Graphics.FromImage(_LayerTwo);
                    graphics.DrawRectangle(Pens.Black, x * DependencyTextureSize, y * DependencyTextureSize, DependencyTextureSize, DependencyTextureSize);
                }
            }
        }

        private void DrawNumbers()
        {
            throw new System.NotImplementedException();
        }

        private void Touch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _cursor = new Cursor();
            var position = e.GetPosition(Touch);


            if (position.X < DependencyTextureSize) _cursor.X = 0;
            else
            {
                _cursor.X = (int)position.X / DependencyTextureSize;
            }

            if (position.Y < DependencyTextureSize) _cursor.Y = 0;
            else
            {
                _cursor.Y = (int)position.X / DependencyTextureSize;
            }
        }
    }
}
