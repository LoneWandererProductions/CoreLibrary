/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     Mathematics
 * FILE:        Mathematics/FastMath.cs
 * PURPOSE:     Simple optimiced Math Functions
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System;

namespace Mathematics
{
    public static class FastMath
    {
        private const float Pi = (float)Math.PI;
        private const float TwoPi = 2 * Pi;

        /// <summary>
        /// Fast approximation of sin(x) using a cubic polynomial.
        /// </summary>
        public static float FastSin(float x)
        {
            x = x % TwoPi; // Keep x in valid range
            return x * (1.27323954f - 0.405284735f * Math.Abs(x));
        }

        /// <summary>
        /// Fast approximation of cos(x) using the squared sin approximation.
        /// </summary>
        public static float FastCos(float x)
        {
            return FastSin(x + Pi / 2);
        }

        /// <summary>
        /// Medium-accuracy sine approximation using a 5th-degree polynomial.
        /// </summary>
        public static float MediumSin(float x)
        {
            x = x % TwoPi;
            float x2 = x * x;
            return x * (1 - x2 / 6 + x2 * x2 / 120);
        }

        /// <summary>
        /// Fast lookup table based sin(x)
        /// </summary>
        private static readonly float[] SinLUT = new float[256];
        static FastMath()
        {
            for (int i = 0; i < SinLUT.Length; i++)
                SinLUT[i] = (float)Math.Sin(i * TwoPi / SinLUT.Length);
        }

        public static float LUTSin(float x)
        {
            int index = (int)(x * (SinLUT.Length / TwoPi)) & (SinLUT.Length - 1);
            return SinLUT[index];
        }
    }
}
