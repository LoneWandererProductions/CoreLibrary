/*
 * COPYRIGHT:   See COPYING in the top level directory
 * PROJECT:     CommonControls.Images
 * FILE:        SelectionFrame.cs
 * PURPOSE:     Selection Frame
 * PROGRAMER:   Peter Geinitz (Wayfarer)
 */

using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace CommonControls.Images
{
    /// <summary>
    ///      The Selection Frame on the Image
    /// </summary>
    [DebuggerDisplay("X = {X}, Y = {Y}, Width = {Width}, Height = {Height}, Tool = {Tool}")]
    public sealed class SelectionFrame
    {
        /// <summary>
        ///      Gets or sets the x.
        /// </summary>
        public int X { get; init; }

        /// <summary>
        ///      Gets or sets the y.
        /// </summary>
        public int Y { get; internal init; }

        /// <summary>
        ///      Gets or sets the width.
        /// </summary>
        public int Width { get; internal init; }

        /// <summary>
        ///      Gets or sets the height.
        /// </summary>
        public int Height { get; internal init; }

        /// <summary>
        ///      Gets the tool used to create this frame.
        /// </summary>
        public ImageZoomTools Tool { get; internal init; }

        /// <summary>
        ///      Gets the detailed points (used for FreeForm/Polygon).
        ///      For Rectangles, this may be null or empty.
        /// </summary>
        public List<Point> Points { get; internal init; } = new();
    }
}