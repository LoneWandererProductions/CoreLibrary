/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/IVectors.cs
 * PURPOSE:     LightVector Interface
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

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
        /// <summary>
        ///     Rotates the specified degree.
        /// </summary>
        /// <param name="degree">The degree.</param>
        /// <returns></returns>
        ImageContainer Rotate(int degree);

        /// <summary>
        ///     Scales the specified factor.
        /// </summary>
        /// <param name="factor">The factor.</param>
        /// <returns></returns>
        ImageContainer Scale(int factor);

        /// <summary>
        ///     Lines the rotate.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="degree">The degree.</param>
        /// <returns></returns>
        LineObject LineRotate(LineObject line, int degree);

        /// <summary>
        ///     Lines the scale.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="factor">The factor.</param>
        /// <returns></returns>
        LineObject LineScale(LineObject line, int factor);

        /// <summary>
        ///     Curves the rotate.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="degree">The degree.</param>
        /// <returns></returns>
        CurveObject CurveRotate(CurveObject curve, int degree);

        /// <summary>
        ///     Curves the scale.
        /// </summary>
        /// <param name="curve">The curve.</param>
        /// <param name="factor">The factor.</param>
        /// <returns></returns>
        CurveObject CurveScale(CurveObject curve, int factor);

        /// <summary>
        ///     Gets the vector image.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        ImageContainer GetVectorImage(string path);

        /// <summary>
        ///     Gets the point.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
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

        /// <summary>
        ///     Lines the remove.
        /// </summary>
        /// <param name="line">The line.</param>
        void LineRemove(LineObject line);

        /// <summary>
        ///     Curves the remove.
        /// </summary>
        /// <param name="curve">The curve.</param>
        void CurveRemove(CurveObject curve);

        /// <summary>
        ///     Saves the container.
        /// </summary>
        /// <param name="path">The path.</param>
        void SaveContainer(string path);
    }
}
