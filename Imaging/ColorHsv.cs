/*
* COPYRIGHT:   See COPYING in the top level directory
* PROJECT:     Imaging
* FILE:        ColorHsv.cs
* PURPOSE:     General Conversions of all Types of Color Displays, Todo Sort out Degree and radian a bit more
* PROGRAMER:   Peter Geinitz (Wayfarer)
* SOURCE:      https://manufacture.tistory.com/33
*              https://www.rapidtables.com/convert/color/rgb-to-hsv.html
*              https://en.wikipedia.org/wiki/HSL_and_HSV
*              https://docs.microsoft.com/de-de/dotnet/fundamentals/code-analysis/quality-rules/ca1036
*/

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedMember.Global
// ReSharper disable NonReadonlyMemberInGetHashCode

using System;
using System.Windows.Media;

namespace Imaging
{
    /// <inheritdoc cref="IComparable" />
    /// <summary>
    ///     HSV to RGP
    ///     And other Conversions
    /// </summary>
    public sealed class ColorHsv : IEquatable<ColorHsv>, IComparable<ColorHsv>
    {
        /// <summary>
        /// Hue [0..360]
        /// </summary>
        /// <value>
        /// The h.
        /// </value>
        public double H { get; set; }

        /// <summary>
        /// Saturation [0..1]
        /// </summary>
        /// <value>
        /// The s.
        /// </value>
        public double S { get; set; }

        /// <summary>
        /// Value [0..1]
        /// </summary>
        /// <value>
        /// The v.
        /// </value>
        public double V { get; set; }

        /// <summary>
        /// Alpha [0..255]
        /// </summary>
        /// <value>
        /// a.
        /// </value>
        public int A { get; init; }

        /// <summary>
        /// Red [0..255]
        /// </summary>
        /// <value>
        /// The r.
        /// </value>
        public int R { get; set; }

        /// <summary>
        /// Green [0..255]
        /// </summary>
        /// <value>
        /// The g.
        /// </value>
        public int G { get; set; }

        /// <summary>Blue [0..255]</summary>
        public int B { get; set; }

        /// <summary>
        /// Gets the hexadecimal.
        /// </summary>
        /// <value>
        /// The hexadecimal.
        /// </value>
        public string Hex => $"#{R:X2}{G:X2}{B:X2}";

        /// <summary>
        /// Gets the color of the open tk.
        /// </summary>
        /// <value>
        /// The color of the open tk.
        /// </value>
        public float[] OpenTkColor => new[] { R / 255f, G / 255f, B / 255f, A / 255f };

        /// <summary>
        /// Prevents a default instance of the <see cref="ColorHsv"/> class from being created.
        /// </summary>
        private ColorHsv()
        {
        } // internal factory use

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorHsv"/> class.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="a">a.</param>
        public ColorHsv(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        /// <summary>
        /// Convert from RGB to HSV.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="a">a.</param>
        /// <returns>Color HSV object.</returns>
        public static ColorHsv FromRgb(int r, int g, int b, int a = 255)
        {
            // Robustness: Clamp instead of throwing
            r = Math.Max(0, Math.Min(255, r));
            g = Math.Max(0, Math.Min(255, g));
            b = Math.Max(0, Math.Min(255, b));
            a = Math.Max(0, Math.Min(255, a));

            var hsv = new ColorHsv { R = r, G = g, B = b, A = a };
            hsv.ToHsv();
            return hsv;
        }

        /// <summary>
        /// Creates a ColorHsv from HSV values.
        /// Automatically wraps Hue and clamps S/V/A.
        /// </summary>
        /// <param name="h">The h.</param>
        /// <param name="s">The s.</param>
        /// <param name="v">The v.</param>
        /// <param name="a">a.</param>
        /// <returns>Color HSV object.</returns>
        public static ColorHsv FromHsv(double h, double s, double v, int a = 255)
        {
            // Robustness: Handle "fuzzy" floating point math

            // 1. Wrap Hue (e.g. 361 becomes 1, -10 becomes 350)
            // This handles cases where the mouse rotation calc goes slightly over 360
            h = h % 360.0;
            if (h < 0) h += 360.0;

            // 2. Clamp Saturation and Value to 0.0 - 1.0
            // Barycentric triangle math often returns -0.00001 or 1.00001 at the edges
            s = Math.Max(0.0, Math.Min(1.0, s));
            v = Math.Max(0.0, Math.Min(1.0, v));

            // 3. Clamp Alpha
            a = Math.Max(0, Math.Min(255, a));

            var hsv = new ColorHsv { H = h, S = s, V = v, A = a };
            hsv.ToRgb();
            return hsv;
        }

        /// <summary>
        /// Froms the hexadecimal.
        /// </summary>
        /// <param name="hex">The hexadecimal.</param>
        /// <param name="a">a.</param>
        /// <returns>Color HSV object.</returns>
        /// <exception cref="System.ArgumentException">Hex string cannot be null or empty.</exception>
        public static ColorHsv FromHex(string hex, int a = 255)
        {
            if (string.IsNullOrWhiteSpace(hex))
                throw new ArgumentException("Hex string cannot be null or empty.");

            var color = (Color)ColorConverter.ConvertFromString(hex);

            return FromRgb(color.R, color.G, color.B, a);
        }

        /// <summary>
        /// Froms the ARGB int.
        /// </summary>
        /// <param name="argb">The ARGB.</param>
        /// <returns>Color HSV object.</returns>
        public static ColorHsv FromArgbInt(int argb)
        {
            var a = (argb >> 24) & 0xFF;
            var r = (argb >> 16) & 0xFF;
            var g = (argb >> 8) & 0xFF;
            var b = argb & 0xFF;

            return FromRgb(r, g, b, a);
        }

        /// <summary>
        /// Gets the color of the drawing.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="a">a.</param>
        /// <returns>Color Object from drawing.</returns>
        public static System.Drawing.Color GetDrawingColor(int r, int g, int b, int a = 255)
        {
            ValidateRgb(r, g, b, a);
            return System.Drawing.Color.FromArgb(a, r, g, b);
        }

        /// <summary>
        /// Gets the color of the drawing.
        /// </summary>
        /// <returns>Color Object from drawing.</returns>
        public System.Drawing.Color GetDrawingColor()
        {
            ValidateRgb(R, G, B, A);
            return System.Drawing.Color.FromArgb(A, R, G, B);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
        /// </returns>
        public bool Equals(ColorHsv other)
            => other is not null &&
               R == other.R && G == other.G && B == other.B && A == other.A;

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
            => obj is ColorHsv c && Equals(c);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
            => HashCode.Combine(R, G, B, A);

        // Sort by hue, then saturation, then value
        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings:
        /// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="other" /> in the sort order.</description></item><item><term> Zero</term><description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="other" /> in the sort order.</description></item></list>
        /// </returns>
        public int CompareTo(ColorHsv? other)
            => other == null ? 1 :
                H != other.H ? H.CompareTo(other.H) :
                S != other.S ? S.CompareTo(other.S) :
                V.CompareTo(other.V);

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(ColorHsv? a, ColorHsv b) => Equals(a, b);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(ColorHsv a, ColorHsv b) => !Equals(a, b);

        /// <summary>
        /// Implements the operator &lt;.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator <(ColorHsv a, ColorHsv b) => a.CompareTo(b) < 0;

        /// <summary>
        /// Implements the operator &gt;.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator >(ColorHsv a, ColorHsv b) => a.CompareTo(b) > 0;

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"{Hex} (H:{H:F1}° S:{S:F2} V:{V:F2})";

        /// <summary>
        /// Converts to hsv.
        /// </summary>
        private void ToHsv()
        {
            var r = R / 255d;
            var g = G / 255d;
            var b = B / 255d;

            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));
            var delta = max - min;

            // Hue
            if (delta == 0)
                H = 0;
            else if (max == r)
                H = 60 * (((g - b) / delta) % 6);
            else if (max == g)
                H = 60 * (((b - r) / delta) + 2);
            else
                H = 60 * (((r - g) / delta) + 4);

            if (H < 0) H += 360;

            // Saturation & Value
            S = max == 0 ? 0 : (delta / max);
            V = max;
        }

        /// <summary>
        /// Converts to rgb.
        /// </summary>
        private void ToRgb()
        {
            var c = V * S;
            var x = c * (1 - Math.Abs((H / 60 % 2) - 1));
            var m = V - c;

            double r = 0, g = 0, b = 0;

            if (H < 60)
            {
                r = c;
                g = x;
            }
            else if (H < 120)
            {
                r = x;
                g = c;
            }
            else if (H < 180)
            {
                g = c;
                b = x;
            }
            else if (H < 240)
            {
                g = x;
                b = c;
            }
            else if (H < 300)
            {
                r = x;
                b = c;
            }
            else
            {
                r = c;
                b = x;
            }

            R = (int)((r + m) * 255);
            G = (int)((g + m) * 255);
            B = (int)((b + m) * 255);
        }

        /// <summary>
        /// Validates the RGB.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        /// <param name="a">a.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">RGB values must be 0–255</exception>
        private static void ValidateRgb(int r, int g, int b, int a)
        {
            if (r is < 0 or > 255 || g is < 0 or > 255 || b is < 0 or > 255 || a is < 0 or > 255)
                throw new ArgumentOutOfRangeException("RGB values must be 0–255");
        }
    }
}
