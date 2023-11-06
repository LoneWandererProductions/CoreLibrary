/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/SaveContainer.cs
 * PURPOSE:     Save our Image
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Windows.Media;

namespace LightVector
{
    /// <summary>
    ///     The save container class.
    /// </summary>
    internal sealed class SaveContainer
    {
        internal int Width { get; init; }

        internal List<LineVector> SavedLines { get; init; }

        internal List<CurveVector> SavedCurves { get; init; }
    }

    /// <summary>
    ///     The line vector class.
    /// </summary>
    internal sealed class LineVector
    {
        internal int MasterId { get; init; }

        /// <summary>
        ///     Needed
        ///     Reihe y
        /// </summary>
        internal int RowY { get; set; }

        /// <summary>
        ///     Needed
        ///     Spalte x
        /// </summary>
        internal int ColumnX { get; set; }

        /// <summary>
        ///     Optional
        /// </summary>
        internal int Thickness { get; init; } = 1;

        /// <summary>
        ///     Optional
        /// </summary>
        internal SolidColorBrush Stroke { get; init; } = Brushes.Black;

        /// <summary>
        ///     Optional
        /// </summary>
        internal SolidColorBrush Fill { get; init; } = Brushes.Black;

        /// <summary>
        ///     Optional
        /// </summary>
        internal PenLineJoin StrokeLineJoin { get; init; } = PenLineJoin.Bevel;
    }

    /// <summary>
    ///     The curve vector class.
    /// </summary>
    internal sealed class CurveVector
    {
        /// <summary>
        ///     Curve Points
        /// </summary>
        internal List<int> MasterId { get; init; }

        /// <summary>
        ///     Optional
        /// </summary>
        internal int Thickness { get; init; } = 1;

        /// <summary>
        ///     Optional
        /// </summary>
        internal SolidColorBrush Stroke { get; init; } = Brushes.Black;

        /// <summary>
        ///     Optional
        /// </summary>
        internal SolidColorBrush Fill { get; init; } = Brushes.Black;

        /// <summary>
        ///     Optional
        /// </summary>
        internal PenLineJoin StrokeLineJoin { get; init; } = PenLineJoin.Bevel;
    }
}
