/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/IVectors.cs
 * PURPOSE:     LightVector Interface
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;

namespace LightVector
{
    /// <summary>
    ///     The IVectors interface.
    /// </summary>
    internal interface IVectors
    {
        ImageContainer Rotate(int degree);

        ImageContainer Scale(int factor);

        LineObject LineRotate(LineObject line, int degree);

        LineObject LineScale(LineObject line, int factor);

        CurveObject CurveRotate(CurveObject curve, int degree);

        CurveObject CurveScale(CurveObject curve, int factor);

        ImageContainer GetVectorImage(string path);

        Rectangle GetPoint(Point point);

        /// <summary>
        ///     The line add.
        /// </summary>
        /// <param name="line">The line.</param>
        void LineAdd(LineObject line);

        /// <summary>
        ///     The curve add.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <returns>The <see cref="System.Windows.Shapes.Path" />.</returns>
        Path CurveAdd(List<Point> curve);

        void LineRemove(LineObject line);

        void CurveRemove(CurveObject curve);

        void SaveContainer(string path);
    }
}
