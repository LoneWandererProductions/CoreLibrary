/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls.Images
 * FILE:        ColorPicker.xaml.cs
 * PURPOSE:     ColorPicker control with a hue ring and saturation/value triangle, supporting RGB/Hex input and output.
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Imaging;

namespace CommonControls.Images
{
    /// <inheritdoc cref="INotifyPropertyChanged" />
    /// <summary>
    /// ColorPicker is a WPF UserControl that provides an interactive color selection interface using a hue ring and saturation/value triangle. It supports RGB and Hex input/output, and raises events when the color changes. The control is designed to be visually intuitive and responsive, allowing users to easily select colors while providing real-time feedback through property bindings and events.
    /// </summary>
    /// <seealso cref="T:System.Windows.Controls.UserControl" />
    /// <seealso cref="T:System.ComponentModel.INotifyPropertyChanged" />
    /// <seealso cref="T:System.Windows.Markup.IComponentConnector" />
    public sealed partial class ColorPicker : INotifyPropertyChanged
    {
        private WriteableBitmap _bitmap;
        private bool _isDragging;

        // Internal state
        private double _h = 0;
        private double _s = 1;
        private double _v = 1;
        private int _alpha = 255;

        // To prevent infinite loops when updating properties
        private bool _ignoreUpdates;

        /// <inheritdoc />
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colorHsv">The color HSV.</param>
        public delegate void DelegateColor(ColorHsv colorHsv);

        /// <summary>
        /// Occurs when [color changed].
        /// </summary>
        public event DelegateColor ColorChanged;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CommonControls.Images.ColorPicker" /> class.
        /// </summary>
        public ColorPicker()
        {
            InitializeComponent();
        }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CommonControls.Images.ColorPicker" /> class.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="alpha">The alpha.</param>
        public ColorPicker(int r, int g, int b, int alpha) : this() // Call default ctor for InitializeComponent
        {
            R = r;
            G = g;
            B = b;
            Alpha = alpha;

            // This is the trigger that will finally make it draw
            this.SizeChanged += (s, e) =>
            {
                if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
                {
                    RedrawAsync();
                    UpdateCursors();
                }
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing.
        /// </summary>
        /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            // If the bitmap hasn't been created yet, try to draw now
            if (_bitmap == null && ActualWidth > 0)
            {
                RedrawAsync();
            }
        }

        // --- DEPENDENCY PROPERTIES (For Bindings) ---

        /// <summary>
        /// Show/Hide inputs
        /// </summary>
        public static readonly DependencyProperty ShowTextBoxesProperty =
            DependencyProperty.Register(nameof(ShowTextBoxes), typeof(bool), typeof(ColorPicker),
                new PropertyMetadata(true));

        public bool ShowTextBoxes
        {
            get => (bool)GetValue(ShowTextBoxesProperty);
            set => SetValue(ShowTextBoxesProperty, value);
        }

        // --- PROPERTIES with Change Logic ---

        public double Hue
        {
            get => _h;
            set
            {
                if (SetField(ref _h, value))
                {
                    OnHsvChanged(true);
                }
            }
        }

        public double Sat
        {
            get => _s;
            set
            {
                if (SetField(ref _s, Math.Max(0, Math.Min(1, value))))
                {
                    OnHsvChanged(false);
                }
            }
        }

        public double Val
        {
            get => _v;
            set
            {
                if (SetField(ref _v, Math.Max(0, Math.Min(1, value))))
                {
                    OnHsvChanged(false);
                }
            }
        }

        public int Alpha
        {
            get => _alpha;
            set
            {
                if (SetField(ref _alpha, Math.Max(0, Math.Min(255, value))))
                {
                    UpdateColorOutput();
                }
            }
        }

        // --- RGB Wrappers ---
        // When these set, we calculate new HSV
        public int R
        {
            get => ColorHsv.FromHsv(_h, _s, _v).R;
            set => UpdateFromRgb(value, G, B);
        }

        public int G
        {
            get => ColorHsv.FromHsv(_h, _s, _v).G;
            set => UpdateFromRgb(R, value, B);
        }

        public int B
        {
            get => ColorHsv.FromHsv(_h, _s, _v).B;
            set => UpdateFromRgb(R, G, value);
        }

        public string Hex
        {
            get => ColorHsv.FromHsv(_h, _s, _v, _alpha).Hex;
            set
            {
                try
                {
                    // Simple Hex parser
                    var color = (Color)ColorConverter.ConvertFromString(value);
                    UpdateFromRgb(color.R, color.G, color.B);
                    Alpha = color.A;
                }
                catch
                {
                    /* Invalid hex, ignore */
                }
            }
        }

        // --- CORE UPDATE LOGIC ---

        private void OnHsvChanged(bool hueChanged)
        {
            if (_ignoreUpdates) return;

            if (hueChanged) RedrawAsync(); // Only redraw image if Hue moves

            UpdateCursors(); // Move the circles
            UpdateColorOutput(); // Notify external world
            NotifyRgbProperties(); // Update TextBoxes
        }

        private void UpdateFromRgb(int r, int g, int b)
        {
            if (_ignoreUpdates) return;

            _ignoreUpdates = true; // Prevent loop

            var hsv = ColorHsv.FromRgb(r, g, b);
            _h = hsv.H;
            _s = hsv.S;
            _v = hsv.V;

            _ignoreUpdates = false;

            // Trigger updates
            OnPropertyChanged(nameof(Hue));
            OnPropertyChanged(nameof(Sat));
            OnPropertyChanged(nameof(Val));
            NotifyRgbProperties();

            RedrawAsync();
            UpdateCursors();
            UpdateColorOutput();
        }

        private void NotifyRgbProperties()
        {
            OnPropertyChanged(nameof(R));
            OnPropertyChanged(nameof(G));
            OnPropertyChanged(nameof(B));
            OnPropertyChanged(nameof(Hex));
        }

        private void UpdateColorOutput()
        {
            ColorChanged?.Invoke(ColorHsv.FromHsv(_h, _s, _v, _alpha));
        }

        // --- VISUAL INTERACTION ---

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            Mouse.Capture(PickerImage);
            ProcessMouse(e.GetPosition(PickerImage));
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging) ProcessMouse(e.GetPosition(PickerImage));
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            Mouse.Capture(null);
        }

        private void ProcessMouse(Point p)
        {
            var layout = GetLayoutInfo();
            if (layout.Size <= 0) return;

            double dx = p.X - layout.CenterX;
            double dy = p.Y - layout.CenterY;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            _ignoreUpdates = true;

            // 1. Hue Ring (Outer Circle)
            // We add +5px tolerance so you don't have to be pixel-perfect
            if (dist > layout.InnerRadius && dist <= layout.Radius + 5)
            {
                double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
                if (angle < 0) angle += 360;
                _h = angle;
                RedrawAsync();
                OnPropertyChanged(nameof(Hue));
            }
            // 2. Triangle (Inner Area)
            // We treat EVERYTHING inside the ring as a potential triangle click
            else if (dist <= layout.InnerRadius + 2)
            {
                // Try exact math first
                if (GetSvFromPoint(dx, dy, layout.InnerRadius, _h, out double s, out double v))
                {
                    _s = s;
                    _v = v;
                }
                else
                {
                    // MAGNET FIX: We are inside the circle, but outside the triangle.
                    // Snap to the closest edge (Clamp).
                    GetSvFromPointClamped(dx, dy, layout.InnerRadius, _h, out s, out v);
                    _s = s;
                    _v = v;
                }

                OnPropertyChanged(nameof(Sat));
                OnPropertyChanged(nameof(Val));
            }

            _ignoreUpdates = false;
            UpdateCursors();
            UpdateColorOutput();
            NotifyRgbProperties();
        }

        private void UpdateCursors()
        {
            var layout = GetLayoutInfo();
            if (layout.Size <= 0) return;

            // --- 1. HUE RING ---
            double hueRad = _h * Math.PI / 180.0;

            // VISUAL FIX: Align to the exact center of the drawn ring
            // The ring is drawn from (InnerRadius) to (Radius). Center is the average.
            double ringCenterRadius = (layout.Radius + layout.InnerRadius) / 2.0;

            double hx = layout.CenterX + (Math.Cos(hueRad) * ringCenterRadius);
            double hy = layout.CenterY + (Math.Sin(hueRad) * ringCenterRadius);

            // WPF positions from Top-Left, so subtract HALF the cursor size
            Canvas.SetLeft(HueCursor, hx - (HueCursor.ActualWidth / 2));
            Canvas.SetTop(HueCursor, hy - (HueCursor.ActualHeight / 2));
            HueCursor.Visibility = Visibility.Visible;

            // --- 2. TRIANGLE ---
            Point svPoint = GetPointFromSv(_h, _s, _v, layout.InnerRadius);

            // svPoint is relative to (0,0) center. Add layout center.
            double svx = svPoint.X + layout.CenterX;
            double svy = svPoint.Y + layout.CenterY;

            // WPF positions from Top-Left, so subtract HALF the cursor size
            Canvas.SetLeft(SvCursor, svx - (SvCursor.ActualWidth / 2));
            Canvas.SetTop(SvCursor, svy - (SvCursor.ActualHeight / 2));
            SvCursor.Visibility = Visibility.Visible;
        }

        // --- RENDERING & CURSORS ---

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RedrawAsync();
            UpdateCursors();
        }

        // This is the "Magic" pixel drawer (Same as before)
        private void RedrawAsync()
        {
            if (!CheckAccess())
            {
                Dispatcher.BeginInvoke(new Action(RedrawAsync));
                return;
            }

            // 1. Get the AVAILABLE space from the parent container (The Grid in Column 0)
            // We cast Parent to FrameworkElement to get its ActualWidth/Height
            var parent = PickerContainer.Parent as FrameworkElement;
            if (parent == null || parent.ActualWidth <= 0 || parent.ActualHeight <= 0) return;

            // 2. Calculate the Square Size
            // We assume the available space is the parent's size
            int size = (int)Math.Min(parent.ActualWidth, parent.ActualHeight);

            // 3. FORCE the Container to be this square size.
            // Because we set Horizontal/VerticalAlignment="Center" in XAML,
            // it will float perfectly in the middle.
            PickerContainer.Width = size;
            PickerContainer.Height = size;

            // 4. Create/Draw Bitmap (Standard)
            if (_bitmap == null || _bitmap.PixelWidth != size || _bitmap.PixelHeight != size)
            {
                _bitmap = new WriteableBitmap(size, size, 96, 96, PixelFormats.Bgra32, null);
                PickerImage.Source = _bitmap;
            }

            _bitmap.Lock();
            unsafe
            {
                int* pBackBuffer = (int*)_bitmap.BackBuffer;
                int stride = _bitmap.BackBufferStride;
                double radius = size / 2.0;
                double innerRadius = radius * 0.85;
                double c = size / 2.0;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        double dx = x - c;
                        double dy = y - c;
                        double dist = Math.Sqrt(dx * dx + dy * dy);
                        int colorData = 0;

                        if (dist <= radius && dist >= innerRadius - 1)
                        {
                            double angle = Math.Atan2(dy, dx) * 180.0 / Math.PI;
                            if (angle < 0) angle += 360;
                            int alpha = 255;
                            if (dist > radius - 1) alpha = (int)((radius - dist) * 255);
                            else if (dist < innerRadius) alpha = (int)((dist - (innerRadius - 1)) * 255);
                            colorData = HsvToInt(angle, 1, 1, alpha);
                        }
                        else if (dist < innerRadius)
                        {
                            if (GetSvFromPoint(dx, dy, innerRadius, _h, out double s, out double v))
                            {
                                colorData = HsvToInt(_h, s, v, 255);
                            }
                        }

                        if (colorData != 0) *(pBackBuffer + y * (stride / 4) + x) = colorData;
                        else if (dist < radius) *(pBackBuffer + y * (stride / 4) + x) = 0;
                    }
                }
            }

            _bitmap.AddDirtyRect(new Int32Rect(0, 0, size, size));
            _bitmap.Unlock();
        }

        // --- MATH HELPERS ---

        private bool GetSvFromPoint(double x, double y, double r, double hue, out double s, out double v)
        {
            s = 0;
            v = 0;
            double hueRad = hue * Math.PI / 180.0;
            Point pColor = new Point(Math.Cos(hueRad) * r, Math.Sin(hueRad) * r);
            Point pWhite = new Point(Math.Cos(hueRad + 2 * Math.PI / 3) * r, Math.Sin(hueRad + 2 * Math.PI / 3) * r);
            Point pBlack = new Point(Math.Cos(hueRad + 4 * Math.PI / 3) * r, Math.Sin(hueRad + 4 * Math.PI / 3) * r);

            double det = (pWhite.Y - pBlack.Y) * (pColor.X - pBlack.X) + (pBlack.X - pWhite.X) * (pColor.Y - pBlack.Y);
            double w1 = ((pWhite.Y - pBlack.Y) * (x - pBlack.X) + (pBlack.X - pWhite.X) * (y - pBlack.Y)) / det;
            double w2 = ((pBlack.Y - pColor.Y) * (x - pBlack.X) + (pColor.X - pBlack.X) * (y - pBlack.Y)) / det;
            double w3 = 1.0 - w1 - w2;

            if (w1 < -0.01 || w2 < -0.01 || w3 < -0.01) return false;

            v = w1 + w2;
            s = v <= 0.001 ? 0 : w1 / v;
            return true;
        }

        private Point GetPointFromSv(double h, double s, double v, double r)
        {
            double hueRad = h * Math.PI / 180.0;
            Point pColor = new Point(Math.Cos(hueRad) * r, Math.Sin(hueRad) * r);
            Point pWhite = new Point(Math.Cos(hueRad + 2 * Math.PI / 3) * r, Math.Sin(hueRad + 2 * Math.PI / 3) * r);
            Point pBlack = new Point(Math.Cos(hueRad + 4 * Math.PI / 3) * r, Math.Sin(hueRad + 4 * Math.PI / 3) * r);

            double w1 = s * v;
            double w2 = v * (1 - s);
            double w3 = 1 - v;

            return new Point(w1 * pColor.X + w2 * pWhite.X + w3 * pBlack.X,
                w1 * pColor.Y + w2 * pWhite.Y + w3 * pBlack.Y);
        }

        private static int HsvToInt(double h, double s, double v, int alpha)
        {
            double c = v * s;
            double x = c * (1 - Math.Abs(h / 60.0 % 2 - 1));
            double m = v - c;
            double r = 0, g = 0, b = 0;

            if (h < 60)
            {
                r = c;
                g = x;
                b = 0;
            }
            else if (h < 120)
            {
                r = x;
                g = c;
                b = 0;
            }
            else if (h < 180)
            {
                r = 0;
                g = c;
                b = x;
            }
            else if (h < 240)
            {
                r = 0;
                g = x;
                b = c;
            }
            else if (h < 300)
            {
                r = x;
                g = 0;
                b = c;
            }
            else
            {
                r = c;
                g = 0;
                b = x;
            }

            return alpha << 24 | (byte)((r + m) * 255) << 16 | (byte)((g + m) * 255) << 8 | (byte)((b + m) * 255);
        }

        /// <summary>
        /// Gets the layout information.
        /// Helper for Alignment and Sizing of Cursors and Hit Testing
        /// </summary>
        /// <returns>
        /// Layout Information.
        /// </returns>
        private (double CenterX, double CenterY, double Size, double Radius, double InnerRadius) GetLayoutInfo()
        {
            double size = PickerContainer.Width;

            if (double.IsNaN(size) || size <= 0) return (0, 0, 0, 0, 0);

            double c = size / 2.0;

            return (
                CenterX: c, // Defined X
                CenterY: c, // Defined Y (Same as X for a square)
                Size: size,
                Radius: c,
                InnerRadius: c * 0.85
            );
        }

        private void GetSvFromPointClamped(double x, double y, double r, double hue, out double s, out double v)
        {
            double hueRad = hue * Math.PI / 180.0;

            // Calculate Vertices (Same as before)
            Point pColor = new Point(Math.Cos(hueRad) * r, Math.Sin(hueRad) * r);
            Point pWhite = new Point(Math.Cos(hueRad + 2 * Math.PI / 3) * r, Math.Sin(hueRad + 2 * Math.PI / 3) * r);
            Point pBlack = new Point(Math.Cos(hueRad + 4 * Math.PI / 3) * r, Math.Sin(hueRad + 4 * Math.PI / 3) * r);

            // Barycentric Weights
            double det = (pWhite.Y - pBlack.Y) * (pColor.X - pBlack.X) + (pBlack.X - pWhite.X) * (pColor.Y - pBlack.Y);
            double w1 = ((pWhite.Y - pBlack.Y) * (x - pBlack.X) + (pBlack.X - pWhite.X) * (y - pBlack.Y)) / det;
            double w2 = ((pBlack.Y - pColor.Y) * (x - pBlack.X) + (pColor.X - pBlack.X) * (y - pBlack.Y)) / det;
            double w3 = 1.0 - w1 - w2;

            // THE FIX: Clamp weights to 0. This "pulls" the point to the edge.
            // Negative weight means "outside the edge opposite to this vertex".
            if (w1 < 0) w1 = 0;
            if (w2 < 0) w2 = 0;
            if (w3 < 0) w3 = 0;

            // Re-normalize so they sum to 1.0
            double total = w1 + w2 + w3;
            if (total == 0) total = 1; // Safety
            w1 /= total;
            w2 /= total;
            // w3 is derived

            // Convert back to SV
            v = w1 + w2;
            s = v <= 0.0001 ? 0 : w1 / v;

            // Final safety clamp
            s = Math.Max(0, Math.Min(1, s));
            v = Math.Max(0, Math.Min(1, v));
        }


        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
