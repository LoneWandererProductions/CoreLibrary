using System;
using System.Collections.Generic;
using System.Windows;

namespace LightVector
{
    /// <summary>
    ///     The polygon object class.
    /// </summary>
    [Serializable]
    public sealed class Polygons
    {
        /// <summary>
        ///     Gets or sets the points.
        /// </summary>
        public List<Point> Points { get; set; } = new();
    }
}