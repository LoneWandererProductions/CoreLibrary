/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Imaging.Texture
 * FILE:        ColorConverter.cs
 * PURPOSE:     Provides methods for converting colors between different formats.
 * PROGRAMMER:  Peter Geinitz (Wayfarer)
 */

namespace Imaging.Texture
{
    /// <summary>
    /// Static class for converting colors between different formats.
    /// </summary>
    public static class ColorConverter
    {
        /// <summary>
        /// HSLs to RGB tuple.
        /// </summary>
        /// <param name="h">The h.</param>
        /// <param name="s">The s.</param>
        /// <param name="l">The l.</param>
        /// <returns>RGB tuple.</returns>
        public static (byte r, byte g, byte b) HslToRgbTuple(double h, double s, double l)
        {
            h /= 360.0;
            s /= 255.0;
            l /= 255.0;
            double r, g, b;

            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                var q = l < 0.5 ? l * (1.0 + s) : l + s - l * s;
                var p = 2.0 * l - q;

                r = HueToRgbComponent(p, q, h + 1.0 / 3.0);
                g = HueToRgbComponent(p, q, h);
                b = HueToRgbComponent(p, q, h - 1.0 / 3.0);
            }

            return ((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        /// <summary>
        /// HSVs to RGB tuple.
        /// </summary>
        /// <param name="h">The h.</param>
        /// <param name="s">The s.</param>
        /// <param name="v">The v.</param>
        /// <returns>RGB tuple.</returns>
        public static (byte r, byte g, byte b) HsvToRgbTuple(double h, double s, double v)
        {
            var hi = Convert.ToInt32(Math.Floor(h / 60.0)) % 6;
            var f = (h / 60.0) - Math.Floor(h / 60.0);

            v *= 255.0;
            var vByte = (byte)v;
            var p = (byte)(v * (1.0 - s));
            var q = (byte)(v * (1.0 - f * s));
            var t = (byte)(v * (1.0 - (1.0 - f) * s));

            return hi switch
            {
                0 => (vByte, t, p),
                1 => (q, vByte, p),
                2 => (p, vByte, t),
                3 => (p, q, vByte),
                4 => (t, p, vByte),
                _ => (vByte, p, q)
            };
        }

        /// <summary>
        /// Hues to RGB component.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="q">The q.</param>
        /// <param name="t">The t.</param>
        /// <returns>RGB component value.</returns>
        private static double HueToRgbComponent(double p, double q, double t)
        {
            if (t < 0.0) t += 1.0;
            if (t > 1.0) t -= 1.0;
            if (t < 1.0 / 6.0) return p + (q - p) * 6.0 * t;
            if (t < 1.0 / 2.0) return q;
            if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6.0;
            return p;
        }
    }
}