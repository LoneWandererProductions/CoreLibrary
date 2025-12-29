/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     LightVector
 * FILE:        LightVector/SaveContainer.cs
 * PURPOSE:     Save our Image
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

// ReSharper disable MemberCanBeInternal
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

using System.Windows.Media;

namespace LightVector
{
    /// <summary>
    ///     The line vector class.
    /// </summary>
    public sealed class GraphicLine
    {
        /// <summary>
        ///     Gets the master identifier.
        /// </summary>
        /// <value>
        ///     The master identifier.
        /// </value>
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
}
