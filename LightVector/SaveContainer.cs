/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/SaveContainer.cs
 * PURPOSE:     Save our Image
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal

using System.Collections.Generic;
using System.Windows.Media;

namespace LightVector
{
    /// <summary>
    ///     Describes the typ of Vector Object
    /// </summary>
    public enum VectorObjects
    {
        /// <summary>
        ///     The line
        /// </summary>
        Point = 0,

        /// <summary>
        ///     The line
        /// </summary>
        Line = 1,

        /// <summary>
        ///     The curve
        /// </summary>
        Curve = 2
    }

    /// <summary>
    ///     The line vector class.
    /// </summary>
    public sealed class LineVector
    {
        public int MasterId { get; init; }

        /// <summary>
        ///     Needed
        ///     Row Y
        /// </summary>
        public int RowY { get; set; }

        /// <summary>
        ///     Needed
        ///     Column X
        /// </summary>
        public int ColumnX { get; set; }

        /// <summary>
        ///     Optional
        /// </summary>
        public int Thickness { get; init; } = 1;

        /// <summary>
        ///     Optional
        /// </summary>
        public SolidColorBrush Stroke { get; init; } = Brushes.Black;

        /// <summary>
        ///     Optional
        /// </summary>
        public SolidColorBrush Fill { get; init; } = Brushes.Black;

        /// <summary>
        ///     Optional
        /// </summary>
        public PenLineJoin StrokeLineJoin { get; init; } = PenLineJoin.Bevel;
    }

    /// <summary>
    ///     The curve vector class.
    /// </summary>
    internal sealed class CurveVector
    {
        /// <summary>
        ///     Curve Points
        /// </summary>
        public List<int> MasterId { get; init; }

        /// <summary>
        ///     Optional
        /// </summary>
        public int Thickness { get; init; } = 1;

        /// <summary>
        ///     Optional
        /// </summary>
        public SolidColorBrush Stroke { get; init; } = Brushes.Black;

        /// <summary>
        ///     Optional
        /// </summary>
        public SolidColorBrush Fill { get; init; } = Brushes.Black;

        /// <summary>
        ///     Optional
        /// </summary>
        public PenLineJoin StrokeLineJoin { get; init; } = PenLineJoin.Bevel;
    }
}
