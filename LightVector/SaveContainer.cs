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
    /// Describes the typ of Vector Object
    /// </summary>
    public enum VectorObjects
    {
        /// <summary>
        /// The line
        /// </summary>
        Point = 0,

        /// <summary>
        /// The line
        /// </summary>
        Line = 0,

        /// <summary>
        /// The curve
        /// </summary>
        Curve = 1
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

    /// <summary>
    /// Save Container
    /// </summary>
    public sealed class SaveContainer
    {
        /// <summary>
        /// Gets or sets the objects.
        /// </summary>
        /// <value>
        /// The objects.
        /// </value>
        public List<SaveObject> Objects { get; init; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width { get; init; }
    }

    /// <summary>
    ///     The save object class.
    ///     Save in a Dictionary, Id will be the Key and forwarder for the ParentId
    /// </summary>
    public sealed class SaveObject
    {
        /// <summary>
        ///     Gets or sets the point id.
        /// </summary>
        public object Graphic { get; set; }

        /// <summary>
        ///     Gets or sets the Type.
        ///     0 is Point
        ///     1 is Line
        ///     2 is Curve
        /// </summary>
        public VectorObjects Type { get; set; }
    }
}
