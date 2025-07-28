/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls
 * FILE:        CommonControls/ColorProcessing.cs
 * PURPOSE:     Some internal Processing for the clicked Point
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 * SOURCE:      https://stackoverflow.com/questions/42531608/hsv-triangle-in-c-sharp
 */

using System;
using ExtendedSystemObjects;

namespace CommonControls;

/// <summary>
///     Some internal Processing for the clicked Point
/// </summary>
internal static class ColorProcessing
{
    /// <summary>
    ///     The square root of 3
    /// </summary>
    private static readonly double Sqrt3 = Math.Sqrt(3);

    /// <summary>
    ///     Gets the center.
    /// </summary>
    /// <value>
    ///     The center.
    /// </value>
    private static int Center => ColorPickerRegister.InternSize / 2;

    /// <summary>
    ///     Gets the inner radius.
    /// </summary>
    /// <value>
    ///     The inner radius.
    /// </value>
    private static float InnerRadius => ColorPickerRegister.InternSize * 5 / 12;

    /// <summary>
    ///     Ins the circle.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>Check if it is within the circle</returns>
    internal static bool InCircle(double x, double y)
    {
        var xDelta = x - Center;
        var yDelta = y - Center;

        var dist = (int)Math.Sqrt((xDelta * xDelta) + (yDelta * yDelta));

        return (ColorPickerRegister.InternSize / 2).Interval(dist, 20);
    }

    /// <summary>
    ///     Calculates the hue.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>New Hue Value</returns>
    internal static double CalcHue(double x, double y)
    {
        var angle = Math.Atan2(y - Center, x - Center) + (Math.PI / 2);

        if (angle < 0)
        {
            angle += 2 * Math.PI;
        }

        return angle;
    }

    /// <summary>
    ///     Ins the triangle.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>Is it in the Triangle</returns>
    internal static bool InTriangle(double x, double y)
    {
        //above line, down line x
        //two functions x = x to check if it is within.
        //          x   ,   y
        //Top       94.5,   16  First
        //Bottom    25,     135 Second
        //Left      160,    135 Third

        //measures: max y = 190, max x = 189

        //upper and lower limit
        if (y is < 16 or >= 135)
        {
            return false;
        }

        // Inside
        var sqrt3 = Math.Sqrt(3);
        var x1 = (x - Center) * 1.0 / InnerRadius;
        var y1 = (y - Center) * 1.0 / InnerRadius;
        if ((0 * x1) + (2 * y1) > 1)
        {
            return false;
        }

        if ((sqrt3 * x1) + (-1 * y1) > 1)
        {
            return false;
        }

        if ((-sqrt3 * x1) + (-1 * y1) > 1)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Calculates the value.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>Calc val </returns>
    internal static double CalcVal(double x, double y)
    {
        var x1 = (x - Center) * 1.0 / InnerRadius;
        var y1 = (y - Center) * 1.0 / InnerRadius;

        return ((Sqrt3 * x1) - y1 + 2) / 3;
    }

    /// <summary>
    ///     Calculates the sat.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>Calc Sat</returns>
    internal static double CalcSat(double x, double y)
    {
        var x1 = (x - Center) * 1.0 / InnerRadius;
        var y1 = (y - Center) * 1.0 / InnerRadius;

        return (1 - (2 * y1)) / ((Sqrt3 * x1) - y1 + 2);
    }
}
