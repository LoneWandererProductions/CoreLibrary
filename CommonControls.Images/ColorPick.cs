/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls.Images
 * FILE:        ColorPick.cs
 * PURPOSE:     Color Palette Control, much like Color Picker but for a finer selection of Colors
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCE:      https://manufacture.tistory.com/33
 *              https://www.rapidtables.com/convert/color/rgb-to-hsv.html
 *              https://stackoverflow.com/questions/42531608/hsv-triangle-in-c-sharp
 *              https://en.wikipedia.org/wiki/HSL_and_HSV#From_HSV
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using Imaging;
using Point = System.Windows.Point;

namespace CommonControls.Images
{
    /// <summary>
    ///     Generate basic Color Wheel and Triangle
    /// </summary>
    internal static class ColorPick
    {
        /// <summary>
        ///     Enum for the Areas
        /// </summary>
        public enum Area
        {
            Outside = 0,
            Wheel = 1,
            Triangle = 2
        }

        /// <summary>
        ///     Gets or sets the sat.
        /// </summary>
        private static double _sat;

        /// <summary>
        ///     Gets or sets the value.
        /// </summary>
        private static double _val;

        /// <summary>
        ///     Gets or sets the hue.
        /// </summary>
        private static double _hue;

        /// <summary>
        ///     Center in our case 400/2, 200.
        ///     Since it is a circle and the radius is equal we don't need to differentiate between x or y.
        /// </summary>
        private static int Center => ColorPickerRegister.Size / 2;

        /// <summary>
        ///     Inner radius, in our case for Size 400, 166 2/3
        /// </summary>
        private static int InnerRadius => ColorPickerRegister.Size * 5 / 12;

        /// <summary>
        ///     Outer Circle, in our Case 200
        /// </summary>
        private static int OuterRadius => ColorPickerRegister.Size / 2;

        internal static void Initiate(double h, double s, double v)
        {
            _sat = s;
            _val = v;
            _hue = h;
        }

        /// <summary>
        ///     Hue might be changed. Redraw the Circle and change up Hue.
        /// </summary>
        /// <returns>New Background Image</returns>
        internal static Bitmap InitiateBackGround()
        {
            var img = DrawImage(_hue);

            //Generate two images, second one for the cursors
            using (Graphics.FromImage(img))
            {
                return new Bitmap(img);
            }
        }

        /// <summary>
        ///     Draw the Cursors again
        /// </summary>
        /// <returns>
        ///     Image with Cursors
        /// </returns>
        internal static Bitmap InitiateCursor()
        {
            var img = new Bitmap(ColorPickerRegister.Size, ColorPickerRegister.Size, PixelFormat.Format32bppArgb);

            //Generate two images, second one for the cursors

            using (var g = Graphics.FromImage(img))
            {
                var pen = Pens.Black;

                var wheelPosition = GetWheelPosition();
                g.DrawEllipse(pen, (float)wheelPosition.X - 5, (float)wheelPosition.Y - 5, 15, 15);

                //change Color of cursor depending of Background Color
                pen = _val < 0.5 ? Pens.White : Pens.Black;

                var trianglePosition = GetTrianglePosition();
                g.DrawEllipse(pen, (float)trianglePosition.X - 5, (float)trianglePosition.Y - 5, 10, 10);
            }

            return new Bitmap(img);
        }

        /// <summary>
        ///     Selections the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="alpha">Alpha Channel</param>
        internal static void Selection(double x, double y, int alpha)
        {
            var check = ColorProcessing.InTriangle(x, y);
            if (check)
            {
                _sat = ColorProcessing.CalcSat(x, y);
                _val = ColorProcessing.CalcVal(x, y);
                ColorPickerRegister.Colors = ColorHsv.FromHsv(_hue, _sat, _val, alpha);

                return;
            }

            //Check circle
            check = ColorProcessing.InCircle(x, y);
            if (check)
            {
                _hue = ColorProcessing.CalcHue(x, y);
            }

            ColorPickerRegister.Colors = ColorHsv.FromHsv(_hue, _sat, _val, alpha);
        }

        /// <summary>
        ///     Do not Change to global Hsv Values here!
        /// </summary>
        /// <param name="hue">The hue.</param>
        /// <param name="sat">The sat.</param>
        /// <param name="val">The value.</param>
        /// <returns>
        ///     New Bitmap
        /// </returns>
        private static Bitmap DrawImage(double hue = 3.3, double sat = 1.0, double val = 1.0)
        {
            var img = new Bitmap(ColorPickerRegister.Size, ColorPickerRegister.Size, PixelFormat.Format32bppArgb);
            for (var y = 0; y < ColorPickerRegister.Size; y++)
            for (var x = 0; x < ColorPickerRegister.Size; x++)
            {
                var result = Pick(x, y);

                Color color;
                switch (result.Area)
                {
                    case Area.Outside:
                        color = Color.Transparent;
                        break;
                    case Area.Wheel:
                        color = Hsv(result.Hue, sat, val, 1);
                        break;
                    default:
                        color = Hsv(hue, result.Sat, result.Val, 1);
                        break;
                }

                img.SetPixel(x, y, color);
            }

            return img;
        }

        /// <summary>
        ///     Picks the specified Hsv.
        ///     Do not Change to global Hsv Values here!
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>Clicked results</returns>
        private static PickResults Pick(double x, double y)
        {
            var distanceFromCenter = Math.Sqrt((x - Center) * (x - Center) + (y - Center) * (y - Center));
            var sqrt3 = Math.Sqrt(3);

            if (distanceFromCenter > OuterRadius)
                // Outside
            {
                return new PickResults { Area = Area.Outside };
            }

            if (distanceFromCenter > InnerRadius)
            {
                // Wheel
                var angle = Math.Atan2(y - Center, x - Center) + Math.PI / 2;
                if (angle < 0)
                {
                    angle += 2 * Math.PI;
                }

                var hue = angle;

                return new PickResults { Area = Area.Wheel, Hue = hue };
            }

            // Inside
            var x1 = (x - Center) * 1.0 / InnerRadius;
            var y1 = (y - Center) * 1.0 / InnerRadius;
            if (0 * x1 + 2 * y1 > 1)
            {
                return new PickResults { Area = Area.Outside };
            }

            if (sqrt3 * x1 + -1 * y1 > 1)
            {
                return new PickResults { Area = Area.Outside };
            }

            if (-sqrt3 * x1 + -1 * y1 > 1)
            {
                return new PickResults { Area = Area.Outside };
            }

            // Triangle
            var sat = (1 - 2 * y1) / (sqrt3 * x1 - y1 + 2);
            var val = (sqrt3 * x1 - y1 + 2) / 3;

            return new PickResults { Area = Area.Triangle, Sat = sat, Val = val };
        }

        /// <summary>
        ///     Do not Change to global Hsv Values here!
        /// </summary>
        /// <param name="hue">The hue.</param>
        /// <param name="sat">The sat.</param>
        /// <param name="val">The value.</param>
        /// <param name="alpha">The alpha.</param>
        /// <returns>
        ///     New Hsv Values
        /// </returns>
        private static Color Hsv(double hue, double sat, double val, double alpha)
        {
            var chroma = val * sat;
            const double step = Math.PI / 3;
            var intern = chroma * (1 - Math.Abs(hue / step % 2.0 - 1));
            var shift = val - chroma;

            if (hue < 1 * step)
            {
                return Rgb(shift + chroma, shift + intern, shift + 0, alpha);
            }

            if (hue < 2 * step)
            {
                return Rgb(shift + intern, shift + chroma, shift + 0, alpha);
            }

            if (hue < 3 * step)
            {
                return Rgb(shift + 0, shift + chroma, shift + intern, alpha);
            }

            if (hue < 4 * step)
            {
                return Rgb(shift + 0, shift + intern, shift + chroma, alpha);
            }

            if (hue < 5 * step)
            {
                return Rgb(shift + intern, shift + 0, shift + chroma, alpha);
            }

            return Rgb(shift + chroma, shift + 0, shift + intern, alpha);
        }

        /// <summary>
        ///     Gets the wheel position.
        /// </summary>
        /// <returns>Position in wheel</returns>
        private static Point GetWheelPosition()
        {
            var middleRadius = (double)(InnerRadius + OuterRadius) / 2;

            return new Point { X = Center + middleRadius * Math.Sin(_hue), Y = Center - middleRadius * Math.Cos(_hue) };
        }

        /// <summary>
        ///     Gets the triangle position.
        /// </summary>
        /// <returns>Position in Triangle</returns>
        private static Point GetTrianglePosition()
        {
            var sqrt3 = Math.Sqrt(3);

            return new Point
            {
                X = Center + InnerRadius * (2 * _val - _sat * _val - 1) * sqrt3 / 2,
                Y = Center + InnerRadius * (1 - 3 * _sat * _val) / 2
            };
        }

        /// <summary>
        ///     RGBs the specified red.
        /// </summary>
        /// <param name="red">The red.</param>
        /// <param name="green">The green.</param>
        /// <param name="blue">The blue.</param>
        /// <param name="alpha">The alpha.</param>
        /// <returns>New Color Range</returns>
        private static Color Rgb(double red, double green, double blue, double alpha)
        {
            return Color.FromArgb(
                Math.Min(255, (int)(alpha * 256)),
                Math.Min(255, (int)(red * 256)),
                Math.Min(255, (int)(green * 256)),
                Math.Min(255, (int)(blue * 256)));
        }
    }
}
